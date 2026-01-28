using ProtolScada;
using ProtolScadaRemake.Controls;
using ProtolScadaRemake.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class MainWindow : Window
    {
        public TGlobal _global;
        private readonly DispatcherTimer timer;
        private FrameEmPage? _emPage;
        private FrameTcPage? _TcPage;
        private FrameGroPage? _GroPage;
        private FrameGgdPage? _GgdPage;
        private FrameLog? _LogPage;
        private DispatcherTimer _logTestTimer;
        private FrameTrends _trendsPage;
        private FrameReceptPage? _ReceptPage;

        private DBUtils _dbUtils;
        private DispatcherTimer _logSyncTimer;
        private ModbusManager _modbusManager;
        private DispatcherTimer _updateTimer;
        private ModbusController _modbusController;
        private bool _modbusInitialized = false;
        private Button _activeNavigationButton;

        TWordsArea HR_0000 = new TWordsArea("127.0.0.1", 502, 1, 0x0000, 0x60);
        // Объявление потоков
        private Thread ReadDeviceDataThread;
        private Thread WriteDeviceDataThread;
        private Thread FaultUpdatesThread;
        private Thread TrendUpdatesThread;
        private Thread DBUpdatesThread;
        private Thread ReadVariablesThread;

        private FrameProductStatistics _productStatistics;
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();
            Debug.WriteLine("TGlobal создан");

            // Используйте DBUtils из _global, а не создавайте новый с другим паролем!
            _dbUtils = _global.GetDbUtils();
            Debug.WriteLine("DBUtils получен");

            // Инициализация Modbus (без блокировки UI)
            InitializeModbusAsync();

            StartUpdateTimer();

            _logSyncTimer = new DispatcherTimer();
            _logSyncTimer.Interval = TimeSpan.FromSeconds(30);
            _logSyncTimer.Tick += async (s, e) => await SyncLogsWithDatabaseAsync();
            _logSyncTimer.Start();

            // Добавление тестовых записей в журнал
            _global.Log.Add("Система", "Приложение запущено", 0);
            _global.Log.Add("Пользователь", "Пользователь вошел в систему", 1);
            _global.Log.Add("Предупреждение", "Низкий уровень бака", 2);
            _global.Log.Add("Событие", "Запуск процесса перемешивания", 3);

            // Таймер для времени
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Таймер для добавления тестовых записей
            _logTestTimer = new DispatcherTimer();
            _logTestTimer.Interval = TimeSpan.FromSeconds(5);
            //_logTestTimer.Tick += LogTestTimer_Tick;
            _logTestTimer.Start();

            // Инициализация кнопок
            InitializeButtons();

            // Показываем главную страницу по умолчанию
            ShowMainPage();
        }

        private async void InitializeModbusAsync()
        {
            try
            {
                Debug.WriteLine("Инициализация Modbus...");

                // Создаем ModbusController для прямого доступа
                _modbusController = new ModbusController("127.0.0.1", 502, 1);
                Debug.WriteLine("ModbusController создан");

                // Настройка обработчиков событий
                _modbusController.OnStatusChanged += (message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine($"Modbus статус: {message}");
                    });
                };

                _modbusController.OnConnectionStateChanged += (isConnected) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine($"Modbus соединение: {(isConnected ? "Установлено" : "Разорвано")}");
                        UpdateConnectionStatus(isConnected);
                    });
                };

                _modbusController.OnRegisterValueChanged += (address, value) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine($"Регистр {address} = {value}");
                        UpdateVariableFromRegister(address, value);
                    });
                };

                // Пробуем подключиться в фоне с задержкой
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    try
                    {
                        bool connected = await _modbusController.ConnectAsync();
                        if (connected)
                        {
                            Debug.WriteLine("Modbus подключен успешно");

                            // Запускаем опрос регистров конвейера (0-3)
                            ushort[] registersToPoll = { 0, 1, 2, 3 };
                            _modbusController.StartPolling(registersToPoll);
                            Debug.WriteLine("Опрос регистров запущен");
                        }
                        else
                        {
                            Debug.WriteLine("Не удалось подключиться к Modbus серверу");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка подключения Modbus: {ex.Message}");
                    }
                });

                // Также создаем ModbusManager для расширенной функциональности
                _modbusManager = new ModbusManager(_global);
                Debug.WriteLine("ModbusManager создан");

                _modbusInitialized = true;
                Debug.WriteLine("Modbus инициализация завершена");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
                _modbusInitialized = false;
            }
        }

        private void UpdateVariableFromRegister(ushort address, ushort value)
        {
            switch (address)
            {
                case 0:
                    UpdateVariable("CONVEYOR_STATUS", value);
                    break;
                case 1:
                    UpdateVariable("CONVEYOR_SPEED", value);
                    break;
                case 2:
                    UpdateVariable("ITEM_COUNT", value);
                    break;
                case 3:
                    UpdateVariable("EMERGENCY_STOP", value);
                    break;
            }
        }

        private void UpdateVariable(string tagName, ushort value)
        {
            try
            {
                var variable = _global.Variables.GetByName(tagName);
                if (variable != null)
                {
                    variable.ValueReal = value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления переменной {tagName}: {ex.Message}");
            }
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            bool isConnected = _modbusController?.IsConnected ?? false;
            UpdateConnectionStatus(isConnected);
            UpdateFaultCounter();
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (ControllerConnectionQualityLabel != null)
                    {
                        if (isConnected)
                        {
                            ControllerConnectionQualityLabel.Text = "✓ Связь установлена";
                        }
                        else
                        {
                            ControllerConnectionQualityLabel.Text = "✗ Связь отсутствует";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления статуса: {ex.Message}");
            }
        }

        private void UpdateFaultCounter()
        {
            try
            {
                if (_modbusController?.IsConnected == true)
                {
                    var emergencyStopValue = _modbusController.GetRegisterValue(3);

                    Dispatcher.Invoke(() =>
                    {
                        if (FaultCounterLabel != null)
                        {
                            FaultCounterLabel.Text = emergencyStopValue.ToString();

                            if (emergencyStopValue > 0)
                            {
                                FaultCounterLabel.Foreground = Brushes.White;
                                var border = FaultCounterLabel.Parent as Border;
                                if (border != null)
                                {
                                    border.Background = Brushes.Red;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления счетчика аварий: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                _updateTimer?.Stop();
                _logTestTimer?.Stop();
                _logSyncTimer?.Stop();
                _modbusManager?.Disconnect();
                _modbusController?.Disconnect();

                // Остановить таймер панели статистики
                _productStatistics?.StopTimer();

                Debug.WriteLine("Приложение закрывается...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при закрытии: {ex.Message}");
            }
        }

        public async Task AddLogAsync(string group, string text, short imageIndex, bool saveToDatabase = true)
        {
            try
            {
                var record = new TLogRecord
                {
                    Time = DateTime.Now,
                    GroupName = group,
                    Text = text,
                    ImageIndex = imageIndex
                };

                _global.Log.Add(group, text, imageIndex);

                if (saveToDatabase)
                {
                    await _dbUtils.SaveLogRecordAsync(record);
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    if (_LogPage != null && ContentGrid.Children.Contains(_LogPage))
                    {
                        _LogPage.RefreshLog();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка добавления лога: {ex.Message}");
            }
        }

        private async Task SyncLogsWithDatabaseAsync()
        {
            try
            {
                var dbRecords = await _dbUtils.LoadLogRecordsAsync(50, DateTime.Now.AddHours(-1));

                await Dispatcher.InvokeAsync(() =>
                {
                    if (_LogPage != null && ContentGrid.Children.Contains(_LogPage))
                    {
                        _LogPage.RefreshLog();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка синхронизации логов: {ex.Message}");
            }
        }

        //private void LogTestTimer_Tick(object sender, EventArgs e)
        //{
        //    string[] groups = { "Система", "Пользователь", "Оборудование", "Рецептура" };
        //    string[] messages = { "Автоматическое обновление данных", "Проверка связи с оборудованием" };

        //    Random rnd = new Random();
        //    string group = groups[rnd.Next(groups.Length)];
        //    string message = $"{messages[rnd.Next(messages.Length)]} - {DateTime.Now:HH:mm:ss}";
        //    short imageIndex = (short)rnd.Next(0, 4);

        //    _ = AddLogAsync(group, message, imageIndex);
        //}

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            DateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
        }

        private void InitializeButtons()
        {
            MainPageButton.Click += (s, e) => ShowMainPage();
            GgdPageButton.Click += (s, e) => ShowGgdPage();
            GroPageButton.Click += (s, e) => ShowGroPage();
            TcPageButton.Click += (s, e) => ShowTcPage();
            EmPageButton.Click += (s, e) => ShowEmPage();
            ReceptPageButton.Click += (s, e) => ShowReceptPage();
            LogPageButton.Click += (s, e) => ShowLogPage();
            TrendsButton.Click += (s, e) => TrendsButton_Click(s, e);
        }

        private void ShowReceptPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_ReceptPage == null)
                {
                    _ReceptPage = new FrameReceptPage();
                    _ReceptPage.Global = _global; // Передаем глобальный объект
                }

                ContentGrid.Children.Add(_ReceptPage);
                TitleLabel.Text = "СТРАНИЦА РЕЦЕПТУРЫ";
                SetActiveButton(ReceptPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить страницу рецептуры: {ex.Message}");
                Debug.WriteLine($"ShowReceptPage error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_trendsPage == null)
                {
                    _trendsPage = new FrameTrends(_global);
                }

                ContentGrid.Children.Add(_trendsPage);
                TitleLabel.Text = "ТРЕНДЫ";
                SetActiveButton(TrendsButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить тренды: {ex.Message}");
                Debug.WriteLine($"TrendsButton_Click error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ShowLogPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_LogPage == null)
                {
                    _LogPage = new FrameLog(_global);
                }

                ContentGrid.Children.Add(_LogPage);
                _LogPage.RefreshLog();
                TitleLabel.Text = "ЖУРНАЛ СОБЫТИЙ";
                SetActiveButton(LogPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить журнал: {ex.Message}");
            }
        }

        private void ShowEmPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_emPage == null)
                {
                    _emPage = new FrameEmPage(_global);
                }

                ContentGrid.Children.Add(_emPage);
                TitleLabel.Text = "ПРОИЗВОДСТВО ЭМ";
                SetActiveButton(EmPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowTcPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_TcPage == null)
                {
                    _TcPage = new FrameTcPage(_global);
                }

                ContentGrid.Children.Add(_TcPage);
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ТС";
                SetActiveButton(TcPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowGroPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_GroPage == null)
                {
                    _GroPage = new FrameGroPage(_global);
                }

                ContentGrid.Children.Add(_GroPage);
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ГРО";
                SetActiveButton(GroPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowGgdPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_GgdPage == null)
                {
                    _GgdPage = new FrameGgdPage();
                    _GgdPage.Initialize(_global);
                }

                ContentGrid.Children.Add(_GgdPage);
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ГГД";
                SetActiveButton(GgdPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowMainPage()
        {
            ContentGrid.Children.Clear();
            TitleLabel.Text = "ОСНОВНОЙ ЭКРАН";
            SetActiveButton(MainPageButton);

            // Создаем панель статистики если еще не создана
            if (_productStatistics == null)
            {
                _productStatistics = new FrameProductStatistics(_global);
            }
            else
            {
                // Если панель уже существует, создаем новый экземпляр
                // Или удаляем из предыдущего родителя
                if (_productStatistics.Parent != null)
                {
                    var parent = _productStatistics.Parent as Panel;
                    parent?.Children.Remove(_productStatistics);
                }
            }

            // Добавляем панель статистики напрямую в ContentGrid
            ContentGrid.Children.Add(_productStatistics);

            // Центрируем панель (как в WinForms коде)
            _productStatistics.HorizontalAlignment = HorizontalAlignment.Center;
            _productStatistics.VerticalAlignment = VerticalAlignment.Center;
        }

        //private void ShowPage(string pageName)
        //{
        //    ContentGrid.Children.Clear();
        //    TitleLabel.Text = pageName.ToUpper();

        //    Button? activeButton = pageName switch
        //    {
        //        "ГГД" => GgdPageButton,
        //        "ГРО" => GroPageButton,
        //        "ТС" => TcPageButton,
        //        "Рецептура" => ReceptPageButton,
        //        "Журнал" => LogPageButton,
        //        _ => null
        //    };

        //    if (activeButton != null)
        //        SetActiveButton(activeButton);

        //    var textBlock = new TextBlock
        //    {
        //        Text = $"{pageName.ToUpper()}\n\nСтраница в разработке",
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        VerticalAlignment = VerticalAlignment.Center,
        //        FontSize = 24,
        //        FontWeight = FontWeights.Bold,
        //        TextAlignment = TextAlignment.Center,
        //        Foreground = Brushes.Gray
        //    };
        //    ContentGrid.Children.Add(textBlock);
        //}

        private void ShowError(string message)
        {
            ContentGrid.Children.Clear();

            var textBlock = new TextBlock
            {
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Red
            };
            ContentGrid.Children.Add(textBlock);
        }

        private void SetActiveButton(Button activeButton)
        {
            try
            {
                var buttons = new[] {
                MainPageButton, GgdPageButton, GroPageButton,
                TcPageButton, EmPageButton, ReceptPageButton,
                LogPageButton, TrendsButton
                };

                _activeNavigationButton = activeButton;

                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        // Для кнопок с Template установим Tag
                        button.Tag = button == activeButton ? "Active" : "Normal";
                    }
                }

                Debug.WriteLine($"Установлена активная кнопка: {activeButton?.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в SetActiveButton: {ex.Message}");
            }
        }
    }
}
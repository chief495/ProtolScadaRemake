using MySql.Data.MySqlClient;
using ProtolScada;
using ProtolScadaRemake.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

        private DBUtils _dbUtils;
        private DispatcherTimer _logSyncTimer;
        private ModbusManager _modbusManager;
        private DispatcherTimer _updateTimer;
        private ModbusController _modbusController;
        private bool _modbusInitialized = false;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();
            Debug.WriteLine("TGlobal создан");

            // Используйте DBUtils из _global, а не создавайте новый с другим паролем!
            _dbUtils = _global.GetDbUtils();  // Или _dbUtils = new DBUtils { ... } но с теми же данными что в TGlobal
            Debug.WriteLine("DBUtils получен");

            // Инициализация Modbus (без блокировки UI)
            InitializeModbusAsync();

            StartUpdateTimer();

            _logSyncTimer = new DispatcherTimer();
            _logSyncTimer.Interval = TimeSpan.FromSeconds(30);
            _logSyncTimer.Tick += async (s, e) => await SyncLogsWithDatabaseAsync();
            _logSyncTimer.Start();

            // Добавление тестовых записей в журнал - исправьте чтобы сохранялось в БД
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
            _logTestTimer.Tick += LogTestTimer_Tick;
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
                        UpdateStatusLabel($"Modbus: {message}");
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
                        // Можно обновлять переменные SCADA здесь
                        UpdateVariableFromRegister(address, value);
                    });
                };

                // Пробуем подключиться в фоне с задержкой
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000); // Даем время на запуск сервера
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

        private void UpdateStatusLabel(string message)
        {
            // Можно добавить статусную строку в UI если нужно
            // Пока просто выводим в Debug
        }

        private void UpdateVariableFromRegister(ushort address, ushort value)
        {
            // Маппинг регистров Modbus на теги SCADA
            switch (address)
            {
                case 0: // Конвейер статус
                    UpdateVariable("CONVEYOR_STATUS", value);
                    break;
                case 1: // Скорость конвейера
                    UpdateVariable("CONVEYOR_SPEED", value);
                    break;
                case 2: // Счетчик деталей
                    UpdateVariable("ITEM_COUNT", value);
                    break;
                case 3: // Аварийная остановка
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
                else
                {
                    // Создаем переменную если ее нет
                    // Это для демонстрации, в реальном проекте нужно создать переменные заранее
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
            // Обновляем статус соединения
            bool isConnected = _modbusController?.IsConnected ?? false;
            UpdateConnectionStatus(isConnected);

            // Обновляем счетчик аварий если нужно
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
                            ControllerConnectionQualityLabel.Foreground = Brushes.Green;
                        }
                        else
                        {
                            ControllerConnectionQualityLabel.Text = "✗ Связь отсутствует";
                            ControllerConnectionQualityLabel.Foreground = Brushes.Red;
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
            // Здесь можно обновлять счетчик аварий на основе данных из Modbus
            // Например, если регистр 3 (аварийная остановка) != 0
            try
            {
                if (_modbusController?.IsConnected == true)
                {
                    // Получаем значение регистра аварийной остановки
                    var emergencyStopValue = _modbusController.GetRegisterValue(3);

                    Dispatcher.Invoke(() =>
                    {
                        if (FaultCounterLabel != null)
                        {
                            FaultCounterLabel.Text = emergencyStopValue.ToString();

                            // Подсвечиваем если есть аварии
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
                // Создаем запись
                var record = new TLogRecord
                {
                    Time = DateTime.Now,
                    GroupName = group,
                    Text = text,
                    ImageIndex = imageIndex
                };

                // Добавляем в локальную коллекцию
                _global.Log.Add(group, text, imageIndex);

                // Сохраняем в базу данных (если требуется)
                if (saveToDatabase)
                {
                    await _dbUtils.SaveLogRecordAsync(record);
                }

                // Обновляем UI, если журнал открыт
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

        // СИНХРОНИЗАЦИЯ ЛОГОВ С БАЗОЙ ДАННЫХ
        private async Task SyncLogsWithDatabaseAsync()
        {
            try
            {
                // Загружаем последние записи из БД
                var dbRecords = await _dbUtils.LoadLogRecordsAsync(50, DateTime.Now.AddHours(-1));

                // Обновляем локальную коллекцию
                // Здесь можно добавить логику сравнения и синхронизации

                // Обновляем UI
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

        private void LogTestTimer_Tick(object sender, EventArgs e)
        {
            // Используем новый метод
            string[] groups = { "Система", "Пользователь", "Оборудование", "Рецептура" };
            string[] messages = { "Автоматическое обновление данных", "Проверка связи с оборудованием" };

            Random rnd = new Random();
            string group = groups[rnd.Next(groups.Length)];
            string message = $"{messages[rnd.Next(messages.Length)]} - {DateTime.Now:HH:mm:ss}";
            short imageIndex = (short)rnd.Next(0, 4);

            // Асинхронно добавляем лог
            _ = AddLogAsync(group, message, imageIndex);
        }

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
            ReceptPageButton.Click += (s, e) => ShowPage("Рецептура");
            AlarmPageButton.Click += (s, e) => ShowPage("Аварии");
            LogPageButton.Click += (s, e) => ShowLogPage();
            TrendsButton.Click += (s, e) => TrendsButton_Click(s, e);
        }

        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_trendsPage == null)
                {
                    _trendsPage = new FrameTrends(_global);  // Передайте _global
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

                // ОБЯЗАТЕЛЬНО ОБНОВЛЯЕМ ЖУРНАЛ ПРИ ОТКРЫТИИ
                _LogPage.RefreshLog();

                TitleLabel.Text = "ЖУРНАЛ СОБЫТИЙ";
                SetActiveButton(LogPageButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить журнал: {ex.Message}");
            }
        }

        // ОТОБРАЖЕНИЕ SVG СТРАНИЦЫ
        private void ShowEmPage()
        {
            try
            {
                // Очищаем ContentGrid
                ContentGrid.Children.Clear();

                // Создаем или используем существующую страницу
                if (_emPage == null)
                {
                    _emPage = new FrameEmPage(_global);
                }
                // Добавляем на форму
                ContentGrid.Children.Add(_emPage);

                // Обновляем заголовок
                TitleLabel.Text = "ПРОИЗВОДСТВО ЭМ";

                // Делаем кнопку активной
                SetActiveButton(EmPageButton);
            }
            catch (Exception ex)
            {
                // Показываем ошибку
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowTcPage()
        {
            try
            {
                // Очищаем ContentGrid
                ContentGrid.Children.Clear();

                // Создаем или используем существующую страницу
                if (_TcPage == null)
                {
                    _TcPage = new FrameTcPage();
                }

                // Добавляем на форму
                ContentGrid.Children.Add(_TcPage);

                // Обновляем заголовок
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ТС";

                // Делаем кнопку активной
                SetActiveButton(TcPageButton);
            }
            catch (Exception ex)
            {
                // Показываем ошибку
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowGroPage()
        {
            try
            {
                // Очищаем ContentGrid
                ContentGrid.Children.Clear();

                // Создаем или используем существующую страницу
                if (_GroPage == null)
                {
                    _GroPage = new FrameGroPage();
                }

                // Добавляем на форму
                ContentGrid.Children.Add(_GroPage);

                // Обновляем заголовок
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ГРО";

                // Делаем кнопку активной
                SetActiveButton(GroPageButton);
            }
            catch (Exception ex)
            {
                // Показываем ошибку
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowGgdPage()
        {
            try
            {
                // Очищаем ContentGrid
                ContentGrid.Children.Clear();

                // Создаем или используем существующую страницу
                if (_GgdPage == null)
                {
                    _GgdPage = new FrameGgdPage();
                }

                // Добавляем на форму
                ContentGrid.Children.Add(_GgdPage);

                // Обновляем заголовок
                TitleLabel.Text = "ПОДГОТОВКА КОМПОНЕНТОВ ГГД";

                // Делаем кнопку активной
                SetActiveButton(GgdPageButton);
            }
            catch (Exception ex)
            {
                // Показываем ошибку
                ShowError($"Не удалось загрузить SVG: {ex.Message}");
            }
        }

        private void ShowMainPage()
        {
            ContentGrid.Children.Clear();
            TitleLabel.Text = "ОСНОВНОЙ ЭКРАН";
            SetActiveButton(MainPageButton);

            // Показываем заглушку
            var textBlock = new TextBlock
            {
                Text = "ОСНОВНОЙ ЭКРАН\n\nГлавная страница системы",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray
            };
            ContentGrid.Children.Add(textBlock);
        }

        private void ShowPage(string pageName)
        {
            ContentGrid.Children.Clear();
            TitleLabel.Text = pageName.ToUpper();

            // Определяем какая кнопка активна
            Button? activeButton = pageName switch
            {
                "ГГД" => GgdPageButton,
                "ГРО" => GroPageButton,
                "ТС" => TcPageButton,
                "Рецептура" => ReceptPageButton,
                "Аварии" => AlarmPageButton,
                "Журнал" => LogPageButton,
                _ => null
            };

            if (activeButton != null)
                SetActiveButton(activeButton);

            // Заглушка для других страниц
            var textBlock = new TextBlock
            {
                Text = $"{pageName.ToUpper()}\n\nСтраница в разработке",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray
            };
            ContentGrid.Children.Add(textBlock);
        }

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
            // Сбрасываем все кнопки
            var buttons = new[] {
                MainPageButton, GgdPageButton, GroPageButton,
                TcPageButton, EmPageButton, ReceptPageButton,
                AlarmPageButton, LogPageButton, TrendsButton
            };

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.Background = Brushes.Transparent;
                    button.Opacity = 0.7;
                    button.BorderThickness = new Thickness(0);
                }
            }

            // Активируем выбранную кнопку
            if (activeButton != null)
            {
                activeButton.Background = new SolidColorBrush(Color.FromArgb(255, 187, 222, 251));
                activeButton.Opacity = 1.0;
                activeButton.BorderThickness = new Thickness(0, 0, 4, 0);
                activeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243));
            }
        }

        private void AlarmPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Пустая реализация, обработка уже в InitializeButtons()
        }

        private void LogPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Пустая реализация, обработка уже в InitializeButtons()
        }
    }
}
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
        private DispatcherTimer _updateTimer;
        private bool _modbusInitialized = false;
        private Button _activeNavigationButton;

        private FrameProductStatistics _productStatistics;

        // Добавляем таймер для обновления UI
        private DispatcherTimer _uiUpdateTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();
            Debug.WriteLine("TGlobal создан");

            // Используйте DBUtils из _global
            _dbUtils = _global.GetDbUtils();
            Debug.WriteLine("DBUtils получен");

            StartUpdateTimer();

            _logSyncTimer = new DispatcherTimer();
            _logSyncTimer.Interval = TimeSpan.FromSeconds(30);
            _logSyncTimer.Tick += async (s, e) => await SyncLogsWithDatabaseAsync();
            _logSyncTimer.Start();

            // Таймер для времени
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Таймер для добавления тестовых записей
            _logTestTimer = new DispatcherTimer();
            _logTestTimer.Interval = TimeSpan.FromSeconds(5);
            _logTestTimer.Start();

            // Запускаем таймер обновления UI (как RepaintTimer в старом проекте)
            StartUiUpdateTimer();

            // Инициализация кнопок
            InitializeButtons();

            // Подписываемся на событие загрузки окна
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация Modbus только один раз при загрузке главного окна
            if (!_modbusInitialized)
            {
                InitializeModbus();
                _modbusInitialized = true;
            }

            // Обновляем состояние кнопки доступа после загрузки
            UpdateAccessButtonState();
        }

        private void InitializeModbus()
        {
            try
            {
                Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ MODBUS ===");

                // 1. Инициализация всех переменных Modbus
                ModbusInitializer.InitializeAllVariables(_global);

                // 2. Запуск потоков Modbus (чтение, запись, обновление переменных)
                ModbusInitializer.StartModbusThreads(_global);

                // 3. Показываем главную страницу по умолчанию
                ShowMainPage();

                // 4. Запись в журнал о запуске
                _global.Log.Add("Система", "Программа запущена", 0);

                Debug.WriteLine("=== MODBUS УСПЕШНО ИНИЦИАЛИЗИРОВАН ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации Modbus: {ex.Message}");
                ShowError($"Ошибка инициализации Modbus: {ex.Message}");
            }
        }

        private void StartUiUpdateTimer()
        {
            _uiUpdateTimer = new DispatcherTimer();
            _uiUpdateTimer.Interval = TimeSpan.FromMilliseconds(500); // Как в старом проекте
            _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            _uiUpdateTimer.Start();
        }

        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Обновляем счетчик аварий
            UpdateFaultCounter();

            // Обновляем качество связи с контроллером
            UpdateControllerConnectionQuality();

            // Проверяем время доступа (как в старом проекте)
            CheckAccessTimeout();
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Start();
        }

        // Метод для обновления качества связи с контроллером 
        private void UpdateControllerConnectionQuality()
        {
            try
            {
                var areas = ModbusInitializer.GetModbusAreas();

                if (areas == null || areas.Count == 0)
                    return;

                // Суммируем FaultsCount всех областей для расчета качества связи
                float totalCommunicationFaults = 0;
                int areaCount = 0;

                foreach (var area in areas)
                {
                    if (area != null)
                    {
                        totalCommunicationFaults += area.FaultsCount;
                        areaCount++;
                    }
                }

                // Формула для качества связи (используем ТОЛЬКО для связи)
                if (areaCount > 0)
                {
                    float quality = totalCommunicationFaults / areaCount;
                    quality = (2 - quality) * 50;
                    quality = Math.Max(0, Math.Min(100, quality));

                    Dispatcher.Invoke(() =>
                    {
                        if (ControllerConnectionQualityLabel != null)
                        {
                            ControllerConnectionQualityLabel.Text = $"{quality:F0}%";

                            // Меняем цвет в зависимости от качества
                            if (quality > 80)
                                ControllerConnectionQualityLabel.Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 100));
                            else if (quality > 50)
                                ControllerConnectionQualityLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 100));
                            else
                                ControllerConnectionQualityLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления качества связи: {ex.Message}");
            }
        }

        // Метод для обновления счетчика аварий (НЕЗАВИСИМЫЙ от качества связи)
        private void UpdateFaultCounter()
        {
            try
            {
                // Получаем актуальные аварии из системы, а не из Modbus областей
                if (_global?.Faults == null)
                    return;

                // Считаем количество активных аварий
                int activeFaults = 0;

                for (int i = 0; i < _global.Faults.GetCount(); i++)
                {
                    var fault = _global.Faults.Items[i];
                    // Проверяем, активна ли авария (например, по свойству IsActive)
                    if (fault.IsActive) // Или другое свойство, указывающее на активность
                    {
                        activeFaults++;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    if (FaultCounterLabel != null)
                    {
                        FaultCounterLabel.Text = activeFaults.ToString();

                        if (FaultCounterLabel.Parent is Border border)
                        {
                            if (activeFaults > 0)
                                border.Background = new SolidColorBrush(Color.FromRgb(220, 50, 50));
                            else
                                border.Background = new SolidColorBrush(Color.FromRgb(50, 150, 50));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления счетчика аварий: {ex.Message}");
            }
        }

        // Проверка таймаута доступа
        private void CheckAccessTimeout()
        {
            if (_global != null && _global.Access)
            {
                if (DateTime.Now - _global.PassTime > TimeSpan.FromMinutes(10))
                {
                    _global.Access = false;
                    UpdateAccessButtonState();
                    Debug.WriteLine("Доступ автоматически отключен по таймауту (10 минут)");
                    _global.Log.Add("Система", "Доступ автоматически отключен по истечении 10 минут", 0);
                }
            }
        }

        // Обновление состояния кнопки доступа
        private void UpdateAccessButtonState()
        {
            Dispatcher.Invoke(() =>
            {
                if (AccessButton != null && _global != null)
                {
                    var template = AccessButton.Template;

                    if (template != null)
                    {
                        var statusIndicator = template.FindName("StatusIndicator", AccessButton) as Border;
                        var accessText = template.FindName("AccessText", AccessButton) as TextBlock;
                        var accessBorder = template.FindName("AccessBorder", AccessButton) as Border;

                        if (_global.Access)
                        {
                            // Доступ активен
                            if (statusIndicator != null)
                                statusIndicator.Background = new SolidColorBrush(Color.FromRgb(50, 200, 50));

                            if (accessText != null)
                            {
                                accessText.Text = "ON";
                                accessText.Foreground = new SolidColorBrush(Color.FromRgb(150, 255, 150));
                            }

                            if (accessBorder != null)
                                accessBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(50, 200, 50));

                            // Обновляем текстовый статус внизу
                            if (AccessStatusText != null)
                            {
                                AccessStatusText.Text = "Доступ: активен";
                                AccessStatusText.Foreground = new SolidColorBrush(Color.FromRgb(150, 255, 150));
                            }
                        }
                        else
                        {
                            // Доступ неактивен
                            if (statusIndicator != null)
                                statusIndicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));

                            if (accessText != null)
                            {
                                accessText.Text = "ДОСТУП";
                                accessText.Foreground = new SolidColorBrush(Color.FromRgb(160, 176, 192));
                            }

                            if (accessBorder != null)
                                accessBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(58, 79, 110));

                            // Обновляем текстовый статус внизу
                            if (AccessStatusText != null)
                            {
                                AccessStatusText.Text = "Доступ: отключен";
                                AccessStatusText.Foreground = new SolidColorBrush(Color.FromRgb(160, 176, 192));
                            }
                        }
                    }
                }
            });
        }

        // Обработчик кнопки доступа
        private void AccessButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            if (_global.Access)
            {
                // Если доступ уже есть - отключаем
                _global.Access = false;
                _global.Log.Add("Пользователь", "Доступ отключен", 1);
                UpdateAccessButtonState();
            }
            else
            {
                // Если доступа нет - показываем диалог ввода пароля
                var dialog = new DialogPassword
                {
                    Global = _global,
                    Owner = this
                };

                dialog.ShowDialog();

                // После закрытия диалога обновляем состояние кнопки
                UpdateAccessButtonState();

                if (_global.Access)
                {
                    _global.Log.Add("Пользователь", "Доступ получен", 1);
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== ОСТАНОВКА ПРИЛОЖЕНИЯ ===");

                ModbusInitializer.StopModbusThreads();

                _updateTimer?.Stop();
                _logTestTimer?.Stop();
                _logSyncTimer?.Stop();
                _uiUpdateTimer?.Stop();
                timer?.Stop();

                _productStatistics?.StopTimer();

                _global?.Log?.Add("Система", "Закрытие программы", 0);

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
            // AccessButton уже подписан в XAML
        }

        private void ShowReceptPage()
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_ReceptPage == null)
                {
                    _ReceptPage = new FrameReceptPage();
                    _ReceptPage.Global = _global;
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
                    _emPage = new FrameEmPage();
                    _emPage.Initialize(_global);
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
                    _TcPage = new FrameTcPage();
                    _TcPage.Initialize(_global);
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
                    _GroPage = new FrameGroPage();
                    _GroPage.Initialize(_global);
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

            if (_productStatistics == null)
            {
                _productStatistics = new FrameProductStatistics(_global);
            }
            else
            {
                if (_productStatistics.Parent != null)
                {
                    var parent = _productStatistics.Parent as Panel;
                    parent?.Children.Remove(_productStatistics);
                }
            }

            ContentGrid.Children.Add(_productStatistics);
            _productStatistics.HorizontalAlignment = HorizontalAlignment.Center;
            _productStatistics.VerticalAlignment = VerticalAlignment.Center;
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
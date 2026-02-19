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

        // Объявление потоков
        private Thread ReadDeviceDataThread;
        private Thread WriteDeviceDataThread;
        private Thread FaultUpdatesThread;
        private Thread TrendUpdatesThread;
        private Thread DBUpdatesThread;
        private Thread ReadVariablesThread;

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

            ModbusInitializer.InitializeAllVariables(_global);

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

            // Показываем главную страницу по умолчанию
            ShowMainPage();
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
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Start();
        }

        // Метод для обновления качества связи с контроллером (как в старом проекте)
        private void UpdateControllerConnectionQuality()
        {
            try
            {
                var areas = ModbusInitializer.GetModbusAreas();

                if (areas == null || areas.Count == 0)
                    return;

                // Суммируем FaultsCount всех областей (как в старом проекте)
                float totalFaults = 0;
                int areaCount = 0;

                foreach (var area in areas)
                {
                    if (area != null)
                    {
                        totalFaults += area.FaultsCount;
                        areaCount++;
                    }
                }

                // Формула из старого проекта: F = F / 23; F = (2 - F) * 50;
                // В старом проекте было 23 области
                if (areaCount > 0)
                {
                    float quality = totalFaults / areaCount; // Вместо деления на 23, делим на реальное количество областей
                    quality = (2 - quality) * 50;

                    // Ограничиваем значение от 0 до 100
                    quality = Math.Max(0, Math.Min(100, quality));

                    Dispatcher.Invoke(() =>
                    {
                        if (ControllerConnectionQualityLabel != null)
                        {
                            ControllerConnectionQualityLabel.Text = $"{quality:F0}%";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления качества связи: {ex.Message}");
            }
        }

        // Метод для обновления счетчика аварий
        private void UpdateFaultCounter()
        {
            try
            {
                // Получаем список областей из ModbusInitializer
                var areas = ModbusInitializer.GetModbusAreas();

                if (areas == null || areas.Count == 0)
                    return;

                // Суммируем FaultsCount всех областей
                float totalFaults = 0;

                foreach (var area in areas)
                {
                    if (area != null)
                    {
                        totalFaults += area.FaultsCount;
                    }
                }

                // Обновляем текст счетчика в UI
                Dispatcher.Invoke(() =>
                {
                    if (FaultCounterLabel != null)
                    {
                        FaultCounterLabel.Text = totalFaults.ToString();

                        // Опционально: меняем цвет в зависимости от количества аварий
                        if (FaultCounterLabel.Parent is Border border)
                        {
                            if (totalFaults > 0)
                            {
                                // Если есть аварии - красный фон
                                border.Background = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                // Если аварий нет - зеленый фон
                                border.Background = new SolidColorBrush(Colors.Green);
                            }
                        }
                    }
                });
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
                _uiUpdateTimer?.Stop();

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

            // Создаем панель статистики если еще не создана
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
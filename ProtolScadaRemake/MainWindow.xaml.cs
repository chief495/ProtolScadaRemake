using ProtolScada;
using ProtolScadaRemake.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using NModbus;
using NModbus.Device;

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

        TWordsArea HR_0000 = new TWordsArea("127.0.0.1", 502, 1, 0x0000, 0x60);
        // Объявление потоков
        private Thread ReadDeviceDataThread;
        private Thread WriteDeviceDataThread;
        private Thread FaultUpdatesThread;
        private Thread TrendUpdatesThread;
        private Thread DBUpdatesThread;
        private Thread ReadVariablesThread;
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();

            _dbUtils = new DBUtils
            {
                DB_HostName = "localhost",
                DB_Port = 3306,
                DB_Name = "protolscadadb",
                DB_UserLogin = "root",
                DB_Password = "advengauser"
            };

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
            _logTestTimer.Tick += LogTestTimer_Tick;
            _logTestTimer.Start();

            // Инициализация кнопок
            InitializeButtons();

            // Показываем главную страницу по умолчанию
            ShowMainPage();
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
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления лога: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Ошибка синхронизации логов: {ex.Message}");
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
        }
        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentGrid.Children.Clear();

                if (_trendsPage == null)
                {
                    _trendsPage = new FrameTrends();
                }

                ContentGrid.Children.Add(_trendsPage);
                TitleLabel.Text = "ТРЕНДЫ";
                SetActiveButton(TrendsButton);
            }
            catch (Exception ex)
            {
                ShowError($"Не удалость загрузит тренды: {ex.Message}");  
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
            activeButton.Background = new SolidColorBrush(Color.FromArgb(255, 187, 222, 251));
            activeButton.Opacity = 1.0;
            activeButton.BorderThickness = new Thickness(0, 0, 4, 0);
            activeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer?.Stop();
        }

        private void AlarmPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Пустая реализация, обработка уже в InitializeButtons()
        }

     
        private void LogPageButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
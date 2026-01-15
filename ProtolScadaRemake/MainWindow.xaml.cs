using MySql.Data.MySqlClient;
using ProtolScada;
using ProtolScadaRemake.Views;
using System;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();

            // Используйте DBUtils из _global, а не создавайте новый с другим паролем!
            _dbUtils = _global.GetDbUtils();  // Или _dbUtils = new DBUtils { ... } но с теми же данными что в TGlobal

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
            TrendsButton.Click += (s, e) => TrendsButton_Click(s, e);
            TestDbButton.Click += (s, e) => TestDbButton_Click(s, e);
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
                System.Diagnostics.Debug.WriteLine($"TrendsButton_Click error: {ex.Message}\n{ex.StackTrace}");
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
        private async void TestDbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простой тест вместо сложного окна
                var result = await SimpleDatabaseTest();
                MessageBox.Show(result, "Тест БД",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> SimpleDatabaseTest()
        {
            var log = new StringBuilder();

            try
            {
                // 1. Тест подключения
                log.AppendLine("Тестирование подключения к БД...");

                using (var connection = DBUtils.GetDBConnection(
                    _global.DB_HostName,
                    _global.DB_Port,
                    _global.DB_Name,
                    _global.DB_UserLogin,
                    _global.DB_Password))
                {
                    await connection.OpenAsync();
                    log.AppendLine("✓ Подключение к MySQL успешно");

                    // Проверяем таблицы
                    var tables = new List<string> { "log", "trends", "trend_config" };
                    foreach (var table in tables)
                    {
                        string sql = $"SHOW TABLES LIKE '{table}'";
                        using (var cmd = new MySqlCommand(sql, connection))
                        {
                            var exists = await cmd.ExecuteScalarAsync();
                            log.AppendLine($"  {table}: {(exists != null ? "✓ найдена" : "✗ не найдена")}");
                        }
                    }
                }

                // 2. Тест записи в журнал
                log.AppendLine("\nТест записи в журнал...");
                var testRecord = new TLogRecord
                {
                    Time = DateTime.Now,
                    GroupName = "ТестБД",
                    Text = $"Тестовая запись от {DateTime.Now:HH:mm:ss}",
                    ImageIndex = 0
                };

                var id = await _dbUtils.SaveLogRecordAsync(testRecord);
                log.AppendLine($"✓ Запись добавлена с ID: {id}");

                // 3. Тест чтения из журнала
                log.AppendLine("\nТест чтения из журнала...");
                var records = await _dbUtils.LoadLogRecordsAsync(3);
                log.AppendLine($"✓ Загружено {records.Count} записей");

                // 4. Тест трендов
                log.AppendLine("\nТест трендов...");
                try
                {
                    var configs = await _dbUtils.LoadTrendConfigsAsync();
                    log.AppendLine($"✓ Конфигураций трендов: {configs.Count}");

                    if (configs.Count > 0)
                    {
                        var firstConfig = configs[0];
                        var trendData = await _dbUtils.LoadTrendDataAsync(
                            firstConfig.TagID,
                            DateTime.Now.AddDays(-1),
                            DateTime.Now,
                            10);
                        log.AppendLine($"✓ Данных тренда '{firstConfig.Name}': {trendData.Count} точек");
                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine($"✗ Ошибка трендов: {ex.Message}");
                }

                log.AppendLine("\n=== ТЕСТ ЗАВЕРШЕН УСПЕШНО ===");
            }
            catch (MySqlException ex)
            {
                log.AppendLine($"\n❌ ОШИБКА MYSQL ({ex.Number}): {ex.Message}");

                if (ex.Number == 1045) log.AppendLine("  Проверьте логин/пароль в TGlobal.cs");
                if (ex.Number == 1049) log.AppendLine("  База данных не найдена. Создайте 'protolscadadb' в phpMyAdmin");
                if (ex.Number == 2002) log.AppendLine("  MySQL сервер не запущен. Запустите MySQL через XAMPP");
            }
            catch (Exception ex)
            {
                log.AppendLine($"\n❌ ОБЩАЯ ОШИБКА: {ex.Message}");
            }

            return log.ToString();
        }
    }
}
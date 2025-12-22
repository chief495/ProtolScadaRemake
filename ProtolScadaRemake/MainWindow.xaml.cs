using ProtolScadaRemake.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class MainWindow : Window
    {
        private TGlobal _global;
        private readonly DispatcherTimer timer;
        private FrameEmPage? _emPage;
        private FrameTcPage? _TcPage;
        private FrameGroPage? _GroPage;
        private FrameGgdPage? _GgdPage;
        private FrameLog? _LogPage;
        private DispatcherTimer _logTestTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация глобального объекта
            _global = new TGlobal();

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

        private void LogTestTimer_Tick(object sender, EventArgs e)
        {
            // Добавляем тестовую запись каждые 5 секунд
            string[] groups = { "Система", "Пользователь", "Оборудование", "Рецептура" };
            string[] messages = {
                "Автоматическое обновление данных",
                "Проверка связи с оборудованием",
                "Сканирование датчиков",
                "Обновление трендов",
                "Резервное копирование данных"
            };

            Random rnd = new Random();
            string group = groups[rnd.Next(groups.Length)];
            string message = messages[rnd.Next(messages.Length)];
            short imageIndex = (short)rnd.Next(0, 4);

            _global.Log.Add(group, $"{message} - {DateTime.Now:HH:mm:ss}", imageIndex);

            // Обновляем журнал, если он открыт
            if (_LogPage != null && ContentGrid.Children.Contains(_LogPage))
            {
                _LogPage.RefreshLog();
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
            ReceptPageButton.Click += (s, e) => ShowPage("Рецептура");
            AlarmPageButton.Click += (s, e) => ShowPage("Аварии");
            LogPageButton.Click += (s, e) => ShowLogPage();
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
                    _emPage = new FrameEmPage();
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
                AlarmPageButton, LogPageButton
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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private FrameEmPage? _emPage; // Храним ссылку на страницу
        private FrameTcPage? _TcPage;
        private FrameGroPage? _GroPage;
        private FrameGgdPage? _GgdPage;

        public MainWindow()
        {
            InitializeComponent();

            // Таймер для времени
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Инициализация кнопок
            InitializeButtons();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Время и дата
            TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            DateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
        }

        private void InitializeButtons()
        {
            // Подписываем все кнопки
            MainPageButton.Click += (s, e) => ShowMainPage();
            GgdPageButton.Click += (s, e) => ShowGgdPage();
            GroPageButton.Click += (s, e) => ShowGroPage();
            TcPageButton.Click += (s, e) => ShowTcPage();
            EmPageButton.Click += (s, e) => ShowEmPage();
            ReceptPageButton.Click += (s, e) => ShowPage("Рецептура");
            AlarmPageButton.Click += (s, e) => ShowPage("Аварии");
            LogPageButton.Click += (s, e) => ShowPage("Журнал");
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

        }
    }
}
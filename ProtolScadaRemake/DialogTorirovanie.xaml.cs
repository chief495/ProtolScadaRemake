using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class DialogTorirovanie : Window
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        public DialogTorirovanie(TGlobal global)
        {
            InitializeComponent();
            _global = global;
            Initialize();
        }

        public void Initialize()
        {
            // Инициализируем таймер обновления как в старом проекте (10 Гц)
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;
            _repaintTimer.Start();
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Деактивация таймера как в старом проекте
                _repaintTimer.Stop();

                bool activateOkButton = true;

                // Измеренное время тарирования
                var tag = _global.Variables.GetByName("EM_UnloadTorirovanieTime");
                if (tag != null)
                    EM_UnloadTorirovanieTimeEdit.Text = tag.ValueString;

                // Состояние тарирования
                tag = _global.Variables.GetByName("EM_Unloading_Rejim");
                if (tag != null)
                {
                    switch (tag.ValueString)
                    {
                        case "1":
                            CurrStateLabel.Text = "Остановлен";
                            StateBorder.Background = Brushes.Silver;
                            StartButton.IsEnabled = true;
                            PauseButton.IsEnabled = false;
                            StopButton.IsEnabled = false;
                            break;
                        case "7":
                            CurrStateLabel.Text = "Запущен";
                            StateBorder.Background = Brushes.YellowGreen;
                            StartButton.IsEnabled = false;
                            PauseButton.IsEnabled = true;
                            StopButton.IsEnabled = true;
                            break;
                        case "8":
                            CurrStateLabel.Text = "Пауза";
                            StateBorder.Background = Brushes.Yellow;
                            StartButton.IsEnabled = true;
                            PauseButton.IsEnabled = false;
                            StopButton.IsEnabled = true;
                            break;
                        default:
                            CurrStateLabel.Text = "Блокировка";
                            StateBorder.Background = Brushes.Red;
                            StartButton.IsEnabled = false;
                            PauseButton.IsEnabled = false;
                            StopButton.IsEnabled = false;
                            break;
                    }
                }

                OkButton.IsEnabled = activateOkButton;

                // Расчет результатов
                UpdateResults();

                // Активация таймера как в старом проекте
                _repaintTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в таймере тарирования: {ex.Message}");
                _repaintTimer.Start();
            }
        }

        private void UpdateResults()
        {
            try
            {
                // Рассчитываем результаты тарирования
                if (double.TryParse(MassTextBox.Text, out double mass) &&
                    double.TryParse(EM_UnloadTorirovanieTimeEdit.Text, out double time) &&
                    time > 0)
                {
                    double speed = mass / time;

                    ResultMassTextBox.Text = mass.ToString("F2");
                    ResultTimeTextBox.Text = time.ToString("F2");
                    ResultSpeedTextBox.Text = speed.ToString("F4");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка расчета результатов: {ex.Message}");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var command = _global.Commands.GetByName("EM_Unload_Torirovanie_Start");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    command.SendToController();
                }
                _global.Log.Add("Пользователь", "Старт тарирования", 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка старта тарирования: {ex.Message}");
                MessageBox.Show($"Ошибка старта тарирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var command = _global.Commands.GetByName("EM_Unload_Torirovanie_Pause");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    command.SendToController();
                }
                _global.Log.Add("Пользователь", "Пауза тарирования", 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка паузы тарирования: {ex.Message}");
                MessageBox.Show($"Ошибка паузы тарирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var command = _global.Commands.GetByName("EM_Unload_Torirovanie_Stop");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    command.SendToController();
                }
                _global.Log.Add("Пользователь", "Стоп тарирования", 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки тарирования: {ex.Message}");
                MessageBox.Show($"Ошибка остановки тарирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Установка массы для тарирования
                var command = _global.Commands.GetByName("EM_Unload_Torirovanie_Mass");
                if (command != null)
                {
                    command.WriteValue = MassTextBox.Text;
                    command.NeedToWrite = true;
                    command.SendToController();
                }

                // Запуск расчета
                command = _global.Commands.GetByName("EM_Unload_Torirovanie_Calculate");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    command.SendToController();
                }

                _global.Log.Add("Пользователь", "Применены параметры тарирования", 1);

                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка применения параметров тарирования: {ex.Message}");
                MessageBox.Show($"Ошибка применения параметров тарирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Позиционирование окна по центру курсора как в старом проекте
            try
            {
                var cursorPos = Win32.GetCursorPosition();
                this.Left = cursorPos.X - this.Width / 2;
                this.Top = cursorPos.Y - this.Height / 2;

                // Проверка границ экрана
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                if (this.Left < 0) this.Left = 0;
                if (this.Top < 0) this.Top = 0;
                if (this.Left + this.Width > screenWidth) this.Left = screenWidth - this.Width;
                if (this.Top + this.Height > screenHeight) this.Top = screenHeight - this.Height;

                // Закрытие других экземпляров этой формы
                CloseOtherInstances();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки окна тарирования: {ex.Message}");
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        private void CloseOtherInstances()
        {
            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != this && window is DialogTorirovanie)
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка закрытия других экземпляров: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Остановка таймера при закрытии
            if (_repaintTimer != null)
            {
                _repaintTimer.Stop();
                _repaintTimer.Tick -= RepaintTimer_Tick;
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }

    // Вспомогательный класс для работы с курсором (аналог Cursor.Position из WinForms)
    public static class Win32
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public static System.Windows.Point GetCursorPosition()
        {
            GetCursorPos(out POINT lpPoint);
            return new System.Windows.Point(lpPoint.X, lpPoint.Y);
        }
    }
}
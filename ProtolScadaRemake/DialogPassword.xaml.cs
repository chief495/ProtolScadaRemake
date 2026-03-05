using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class DialogPassword : Window
    {
        public TGlobal Global { get; set; }

        public DialogPassword()
        {
            InitializeComponent();

            // Устанавливаем владельца для правильного поведения модального окна
            this.Owner = Application.Current.MainWindow;
        }

        private void DialogPassword_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем фокус на поле ввода пароля
            pass.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            CheckPassword();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DialogPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckPassword();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckPassword();
                e.Handled = true;
            }
        }

        private void CheckPassword()
        {
            if (Global == null) return;

            // Проверяем пароль
            bool access = pass.Password == Global.Password;
            Global.Access = access;

            if (!access)
            {
                // Неверный пароль - подсвечиваем красным
                OKButton.Background = new SolidColorBrush(Colors.Red);
                ErrorTextBlock.Visibility = Visibility.Visible;

                // Очищаем поле ввода и устанавливаем фокус
                pass.Password = "";
                pass.Focus();
            }
            else
            {
                // Верный пароль - подсвечиваем зеленым и закрываем
                OKButton.Background = new SolidColorBrush(Colors.Green);
                Global.PassTime = DateTime.Now;

                // Небольшая задержка для визуального эффекта
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(200);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.DialogResult = true;
                    this.Close();
                };
                timer.Start();
            }
        }
    }
}
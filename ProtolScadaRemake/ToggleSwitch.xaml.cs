using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProtolScadaRemake
{
    public partial class ToggleSwitch : UserControl
    {
        // Событие изменения состояния
        public event EventHandler<bool> StateChanged;

        private bool _isChecked = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    UpdateVisualState();
                    StateChanged?.Invoke(this, _isChecked);
                }
            }
        }

        // Свойства можно оставить, но они не используются в UI
        public string OnText { get; set; } = "ON";
        public string OffText { get; set; } = "OFF";

        public ToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (IsChecked)
            {
                // Включено - зеленый фон
                BackgroundBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                BackgroundBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 142, 60));

                // Анимация переключения
                var animation = (Storyboard)Resources["TurnOnAnimation"];
                if (animation != null)
                    animation.Begin();
            }
            else
            {
                // Выключено - серый фон
                BackgroundBorder.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                BackgroundBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(97, 97, 97));

                // Анимация переключения
                var animation = (Storyboard)Resources["TurnOffAnimation"];
                if (animation != null)
                    animation.Begin();
            }
        }

        private void ToggleSwitch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        }
    }
}
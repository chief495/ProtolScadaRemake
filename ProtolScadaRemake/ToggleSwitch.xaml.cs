using System;
using System.Windows;
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

        private string _onText = "ON";
        public string OnText
        {
            get { return _onText; }
            set { _onText = value; UpdateVisualState(); }
        }

        private string _offText = "OFF";
        public string OffText
        {
            get { return _offText; }
            set { _offText = value; UpdateVisualState(); }
        }

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
                StateText.Text = OnText;

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
                StateText.Text = OffText;

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
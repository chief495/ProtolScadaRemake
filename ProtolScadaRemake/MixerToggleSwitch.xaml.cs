using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProtolScadaRemake
{
    public partial class MixerToggleSwitch : UserControl
    {
        // События
        public event EventHandler<bool> StateChanged;

        // Свойства для связи с Global
        public TGlobal? Global;
        public string VarName { get; set; } = string.Empty;
        public string Description { get; set; } = "Миксер";

        // Цвета состояний (как в Element_Mixer2)
        private readonly Brush ColorDefault = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Серый
        private readonly Brush ColorWorking = new SolidColorBrush(Color.FromRgb(0, 255, 0));     // Lime
        private readonly Brush ColorNoFeedback = Brushes.Yellow;                                 // Жёлтый
        private readonly Brush ColorFault = Brushes.Red;                                         // Красный

        private bool _isChecked = false;

        public bool IsChecked
        {
            get => _isChecked;
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

        public MixerToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState();
        }

        /// <summary>
        /// Обновление визуального состояния на основе переменных ПЛК
        /// </summary>
        public void UpdateElement()
        {
            if (Global == null || string.IsNullOrWhiteSpace(VarName))
                return;

            try
            {
                // Синхронизация состояния переключателя с ПЛК
                var isWorkTag = Global.Variables?.GetByName(VarName + "_IsWork");
                if (isWorkTag != null)
                {
                    bool isWorking = isWorkTag.ValueReal > 0;
                    if (_isChecked != isWorking)
                    {
                        _isChecked = isWorking;
                        UpdateAnimation();
                    }
                }

                // Определение цвета на основе состояния (как в Element_Mixer2)
                Brush targetColor = ColorDefault;

                // Миксер включен
                if (isWorkTag != null && isWorkTag.ValueReal > 0)
                {
                    targetColor = ColorWorking;
                }

                // Нет подтверждения состояния (перезаписывает "включен")
                var feedbackTag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (feedbackTag != null && feedbackTag.ValueReal < 1)
                {
                    targetColor = ColorNoFeedback;
                }

                // Авария (перезаписывает всё)
                var faultTag = Global.Variables?.GetByName(VarName + "_Fault");
                if (faultTag != null && faultTag.ValueReal > 0)
                {
                    targetColor = ColorFault;
                }

                // Применяем цвет
                BackgroundBorder.Background = targetColor;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления миксера {VarName}: {ex.Message}");
            }
        }

        private void UpdateVisualState()
        {
            UpdateAnimation();
            UpdateColors();
        }

        private void UpdateAnimation()
        {
            var animation = _isChecked
                ? (Storyboard)Resources["TurnOnAnimation"]
                : (Storyboard)Resources["TurnOffAnimation"];

            animation?.Begin();
        }

        private void UpdateColors()
        {
            // Если Global == null, используем стандартную логику ToggleSwitch
            if (Global == null)
            {
                BackgroundBorder.Background = _isChecked
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                    : ColorDefault;
            }
            // Иначе цвет устанавливается через UpdateElement()
        }

        private void ToggleSwitch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        }
    }
}
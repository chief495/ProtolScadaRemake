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
        public event EventHandler<bool>? StateChanged;

        // Свойства для связи с Global
        public TGlobal? Global;
        public string VarName { get; set; } = string.Empty;
        public string Description { get; set; } = "Миксер";

        // Имя команды (как в старом проекте: T150_StartMixer)
        public string CommandSuffix { get; set; } = "_StartMixer";

        // Цвета состояний
        private readonly Brush ColorDefault = new SolidColorBrush(Color.FromRgb(158, 158, 158));
        private readonly Brush ColorWorking = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        private readonly Brush ColorNoFeedback = Brushes.Yellow;
        private readonly Brush ColorFault = Brushes.Red;

        private bool _isChecked = false;
        private bool _isManualMode = false;
        private bool _isWorking = false;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    UpdateAnimation();
                }
            }
        }

        public bool IsManualMode => _isManualMode;

        public MixerToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState();
        }

        public void UpdateElement()
        {
            if (Global == null || string.IsNullOrWhiteSpace(VarName))
                return;

            try
            {
                // Проверяем режим работы
                var manualTag = Global.Variables?.GetByName(VarName + "_Manual");
                _isManualMode = manualTag != null && manualTag.ValueReal > 0;

                // Проверяем состояние работы
                var isWorkTag = Global.Variables?.GetByName(VarName + "_IsWork");
                _isWorking = isWorkTag != null && isWorkTag.ValueReal > 0;

                // Синхронизация анимации переключателя
                if (_isChecked != _isWorking)
                {
                    _isChecked = _isWorking;
                    UpdateAnimation();
                }

                // Определение цвета
                Brush targetColor = ColorDefault;

                if (_isWorking)
                {
                    targetColor = ColorWorking;
                }

                var feedbackTag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (feedbackTag != null && feedbackTag.ValueReal < 1)
                {
                    targetColor = ColorNoFeedback;
                }

                var faultTag = Global.Variables?.GetByName(VarName + "_Fault");
                if (faultTag != null && faultTag.ValueReal > 0)
                {
                    targetColor = ColorFault;
                }

                BackgroundBorder.Background = targetColor;
                MainBorder.Cursor = _isManualMode ? Cursors.No : Cursors.Hand;
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
            if (Global == null)
            {
                BackgroundBorder.Background = _isChecked
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                    : ColorDefault;
            }
        }

        private void ToggleSwitch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (Global == null || string.IsNullOrWhiteSpace(VarName))
                return;

            // В ручном режиме переключатель не работает
            if (_isManualMode)
            {
                return;
            }

            // Переключаем: если работает → выключаем, если не работает → включаем
            bool turnOn = !_isWorking;

            string commandName = VarName + CommandSuffix;
            var command = Global.Commands?.GetByName(commandName);

            if (command != null)
            {
                string action = turnOn ? "Включение" : "Отключение";
                Global.Log?.Add("Пользователь", $"{action} миксера {VarName}", 1);

                command.WriteValue = turnOn ? "true" : "false";
                command.NeedToWrite = true;
            }

            StateChanged?.Invoke(this, turnOn);
        }
    }
}
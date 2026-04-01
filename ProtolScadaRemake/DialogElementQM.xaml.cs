using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProtolScadaRemake
{
    public partial class DialogElementQM : Window
    {
        public TGlobal Global;
        public string VarName = "";
        private bool _isInitializing = true;
        private const double PulseScale = 1000.0;

        private string _eu = "";
        public string EU
        {
            get => _eu;
            set
            {
                _eu = value ?? "";
                CurrentValueEU.Content = _eu;
            }
        }

        public DialogElementQM()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            _isInitializing = true;
            try
            {
                LoadCurrentValues();
                ApplyAccessRestrictions();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void LoadCurrentValues()
        {
            if (Global == null || string.IsNullOrWhiteSpace(VarName))
                return;

            TVariableTag totalTag = Global.Variables?.GetByName(VarName + "_Total");
            if (totalTag != null)
                CurrentValueTextBlock.Text = totalTag.ValueString;

            TVariableTag startValueTag = Global.Variables?.GetByName(VarName + "_StartValue");
            if (startValueTag != null)
                StartValueTextBox.Text = startValueTag.ValueReal.ToString(CultureInfo.InvariantCulture);

            TVariableTag pulseSizeTag = Global.Variables?.GetByName(VarName + "_PulseSize");
            if (pulseSizeTag != null)
            {
                double displayValue = pulseSizeTag.ValueReal / PulseScale;
                PulseSizeTextBox.Text = displayValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void ApplyAccessRestrictions()
        {
            bool hasAccess = Global?.Access == true;
            StartValueTextBox.IsEnabled = hasAccess;
            PulseSizeTextBox.IsEnabled = hasAccess;
        }

        private bool EnsureAccessForSettings()
        {
            if (Global == null) return false;
            if (Global.Access) return true;

            DialogPassword dialog = new DialogPassword
            {
                Global = Global,
                Owner = this
            };

            dialog.ShowDialog();
            ApplyAccessRestrictions();
            return Global.Access;
        }

        private bool TryParseDouble(string input, out double value)
        {
            string normalized = (input ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private void SendNumericCommand(string suffix, double value, string logMessage)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TCommandTag command = Global.Commands?.GetByName(VarName + suffix);
            if (command == null) return;

            string stringValue = value.ToString(CultureInfo.InvariantCulture);
            command.WriteValue = stringValue;
            command.NeedToWrite = true;
            command.SendToController();

            Global.Log.Add("Пользователь", $"{Title}. {logMessage}: {stringValue}.", 1);
        }

        private void StartValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TryParseDouble(StartValueTextBox.Text, out double value)) return;
            SendNumericCommand("_StartValue", value, "Изменено начальное значение");
        }

        private void PulseSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TryParseDouble(PulseSizeTextBox.Text, out double inputValue)) return;

            // Вводится значение импульсов/м3, в контроллер отправляется в формате старого проекта.
            double controllerValue = inputValue * PulseScale;
            SendNumericCommand("_PulseSize", controllerValue, "Изменено количество импульсов на м³");
        }

        private void SettingsTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Global?.Access == true) return;
            if (!EnsureAccessForSettings()) e.Handled = true;
        }

        private void SettingsTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Global?.Access == true) return;
            if (!EnsureAccessForSettings()) e.Handled = true;
        }

        private void SettingsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (sender == StartValueTextBox)
                StartValueTextBox_LostFocus(sender, new RoutedEventArgs());
            else if (sender == PulseSizeTextBox)
                PulseSizeTextBox_LostFocus(sender, new RoutedEventArgs());

            e.Handled = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Global == null) return;

            TCommandTag command = Global.Commands?.GetByName(VarName + "_Reset");
            if (command == null) return;

            command.WriteValue = "true";
            command.NeedToWrite = true;
            command.SendToController();

            Global.Log.Add("Пользователь", $"{Title}. Выполнен сброс управления.", 1);
        }
    }
}

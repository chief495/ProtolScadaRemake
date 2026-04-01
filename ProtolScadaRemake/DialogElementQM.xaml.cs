using MahApps.Metro.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace ProtolScadaRemake
{
    public partial class DialogElementQM : Window
    {
        public TGlobal Global;
        public string VarName = "";
        private bool _isInitializing = true;

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
                StartValueNumeric.Value = startValueTag.ValueReal;

            TVariableTag pulseSizeTag = Global.Variables?.GetByName(VarName + "_PulseSize");
            if (pulseSizeTag != null)
                PulseSizeNumeric.Value = pulseSizeTag.ValueReal;
        }

        private void ApplyAccessRestrictions()
        {
            bool hasAccess = Global?.Access == true;
            StartValueNumeric.IsEnabled = hasAccess;
            PulseSizeNumeric.IsEnabled = hasAccess;
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

        private void SendNumericCommand(NumericUpDown numeric, string suffix, string logMessage)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!numeric.Value.HasValue) return;

            TCommandTag command = Global.Commands?.GetByName(VarName + suffix);
            if (command == null) return;

            string value = numeric.Value.Value.ToString(CultureInfo.InvariantCulture);
            command.WriteValue = value;
            command.NeedToWrite = true;
            command.SendToController();

            Global.Log.Add("Пользователь", $"{Title}. {logMessage}: {value}.", 1);
        }

        private void StartValueNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(StartValueNumeric, "_StartValue", "Изменено начальное значение");
        }

        private void PulseSizeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(PulseSizeNumeric, "_PulseSize", "Изменено количество импульсов на м³");
        }

        private void SettingsNumeric_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Global?.Access == true) return;
            if (!EnsureAccessForSettings()) e.Handled = true;
        }

        private void SettingsNumeric_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Global?.Access == true) return;
            if (!EnsureAccessForSettings()) e.Handled = true;
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

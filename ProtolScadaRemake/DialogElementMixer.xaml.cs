using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace ProtolScadaRemake
{
    public partial class DialogElementMixer : Window
    {
        public TGlobal? Global;
        public string VarName = string.Empty;

        private bool _isInitializing = true;

        public DialogElementMixer()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            _isInitializing = true;
            try
            {
                var manual = Global?.Variables.GetByName(VarName + "_Manual");
                var startTime = Global?.Variables.GetByName(VarName + "_StartTime");
                var stopTime = Global?.Variables.GetByName(VarName + "_StopTime");

                RBAuto.IsChecked = manual == null || manual.ValueReal < 1;
                RBManual.IsChecked = manual != null && manual.ValueReal > 0;

                StartTimeNumeric.Value = startTime?.ValueReal ?? 0;
                StopTimeNumeric.Value = stopTime?.ValueReal ?? 0;

                UpdateFaultIdentifier();

                bool canEdit = Global?.Access == true;
                RBAuto.IsEnabled = canEdit;
                RBManual.IsEnabled = canEdit;
                StartTimeNumeric.IsEnabled = canEdit;
                StopTimeNumeric.IsEnabled = canEdit;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void RBAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || RBAuto.IsChecked != true) return;
            SendCommand("_Manual", "false", "Установлен автоматический режим миксера.");
        }

        private void RBManual_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || RBManual.IsChecked != true) return;
            SendCommand("_Manual", "true", "Установлен ручной режим миксера.");
        }

        private void StartTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing || !StartTimeNumeric.Value.HasValue) return;
            string value = StartTimeNumeric.Value.Value.ToString(CultureInfo.InvariantCulture);
            SendCommand("_StartTime", value, $"Установлено время запуска: {value} сек.");
        }

        private void StopTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing || !StopTimeNumeric.Value.HasValue) return;
            string value = StopTimeNumeric.Value.Value.ToString(CultureInfo.InvariantCulture);
            SendCommand("_StopTime", value, $"Установлено время остановки: {value} сек.");
        }

        private void SendCommand(string suffix, string value, string logMessage)
        {
            if (Global == null || !Global.Access) return;

            string commandName = VarName + suffix;
            var command = Global.Commands.GetByName(commandName);
            if (command == null)
            {
                Debug.WriteLine($"Команда не найдена: {commandName}");
                return;
            }

            command.WriteValue = value;
            command.NeedToWrite = true;
            command.SendToController();
            Global.Log.Add("Пользователь", $"{Title}. {logMessage}", 1);
            UpdateFaultIdentifier();
        }

        private void UpdateFaultIdentifier()
        {
            var faultTag = Global?.Variables.GetByName(VarName + "_Fault");
            var statusTag = Global?.Variables.GetByName(VarName + "_Status");

            bool isFault = faultTag != null && faultTag.ValueReal > 0;
            int statusCode = statusTag != null ? (int)statusTag.ValueReal : 0;

            FaultIdText.Text = isFault
                ? $"{VarName}_FAULT_ACTIVE (код {statusCode})"
                : $"{VarName}_FAULT_OK (код {statusCode})";
        }
    }
}

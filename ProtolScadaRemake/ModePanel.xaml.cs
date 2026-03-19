using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class ModePanel : UserControl
    {
        public event EventHandler<OperationMode> ModeChanged;          // оставляем – не будет вызываться из кода
        public event EventHandler<ModbusCommandEventArgs> ModbusCommandRequested;

        private OperationMode _currentMode = OperationMode.Off;
        private bool _isInternalUpdate;

        public ushort ModbusRegisterAddress { get; set; } = 0;
        public byte UnitId { get; set; } = 1;
        public OperationMode CurrentMode => _currentMode;

        public ModePanel()
        {
            InitializeComponent();
            OffToggle.IsChecked = true;
        }

        private void ToggleSwitch_StateChanged(object sender, bool isChecked)
        {
            if (_isInternalUpdate) return;

            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;

            if (!isChecked)
            {
                Dispatcher.BeginInvoke(new Action(() => ApplyModeToToggles(_currentMode)),
                                      DispatcherPriority.Background);
                return;
            }

            OperationMode requestedMode = toggle == OffToggle
                ? OperationMode.Off
                : toggle == SemiAutoToggle ? OperationMode.SemiAuto : OperationMode.Auto;

            if (requestedMode == _currentMode)
            {
                ApplyModeToToggles(_currentMode);
                return;
            }

            ModeChanged?.Invoke(this, requestedMode);
            SendModbusCommand(requestedMode);
            ApplyModeToToggles(_currentMode);
        }

        private void SendModbusCommand(OperationMode mode)
        {
            ushort modeValue = (ushort)mode;
            var args = new ModbusCommandEventArgs
            {
                UnitId = UnitId,
                RegisterAddress = ModbusRegisterAddress,
                Value = modeValue,
                Description = $"Запрошен режим: {mode}"
            };
            ModbusCommandRequested?.Invoke(this, args);
        }

        private void ApplyModeToToggles(OperationMode mode)
        {
            _isInternalUpdate = true;
            try
            {
                OffToggle.IsChecked = mode == OperationMode.Off;
                SemiAutoToggle.IsChecked = mode == OperationMode.SemiAuto;
                AutoToggle.IsChecked = mode == OperationMode.Auto;
                _currentMode = mode;
            }
            finally { _isInternalUpdate = false; }
        }

        public void SetMode(OperationMode mode)
        {
            if (_currentMode == mode) return;
            if (Dispatcher.CheckAccess())
                ApplyModeToToggles(mode);
            else
                Dispatcher.Invoke(() => ApplyModeToToggles(mode));
        }
    }

    public enum OperationMode : ushort
    {
        Off = 0,
        SemiAuto = 1,
        Auto = 2
    }

    public class ModbusCommandEventArgs : EventArgs
    {
        public byte UnitId { get; set; }
        public ushort RegisterAddress { get; set; }
        public ushort Value { get; set; }
        public string Description { get; set; }
    }
}

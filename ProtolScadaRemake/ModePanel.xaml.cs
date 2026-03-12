using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class ModePanel : UserControl
    {
        // Событие изменения режима
        public event EventHandler<OperationMode> ModeChanged;

        // Событие для отправки команды по Modbus
        public event EventHandler<ModbusCommandEventArgs> ModbusCommandRequested;

        private OperationMode _currentMode = OperationMode.Off;
        private bool _isInternalUpdate;

        // Регистр Modbus для записи режима
        public ushort ModbusRegisterAddress { get; set; } = 0;

        // Unit ID для Modbus
        public byte UnitId { get; set; } = 1;

        public OperationMode CurrentMode => _currentMode;

        public ModePanel()
        {
            InitializeComponent();

            // Устанавливаем начальный режим
            OffToggle.IsChecked = true;
        }

        private void ToggleSwitch_StateChanged(object sender, bool isChecked)
        {
            if (_isInternalUpdate)
                return;

            var toggle = sender as ToggleSwitch;
            if (toggle == null) return;

            if (!isChecked)
            {
                // Не даем пользователю "снять" текущий активный режим
                Dispatcher.BeginInvoke(new Action(() => ApplyModeToToggles(_currentMode)), DispatcherPriority.Background);
                return;
            }

            OperationMode requestedMode = toggle == OffToggle
                ? OperationMode.Off
                : toggle == SemiAutoToggle
                    ? OperationMode.SemiAuto
                    : OperationMode.Auto;

            // Если уже в этом режиме, просто нормализуем UI
            if (requestedMode == _currentMode)
            {
                ApplyModeToToggles(_currentMode);
                return;
            }

            // 1) Отправляем только запрос на смену режима
            ModeChanged?.Invoke(this, requestedMode);
            SendModbusCommand(requestedMode);

            // 2) Визуально оставляем подтвержденный текущий режим
            // Переключение UI произойдет только после внешнего SetMode(...) по факту от ПЛК
            ApplyModeToToggles(_currentMode);
        }

        private void SendModbusCommand(OperationMode mode)
        {
            ushort modeValue = (ushort)mode;

            // Создаем аргументы события
            var args = new ModbusCommandEventArgs
            {
                UnitId = UnitId,
                RegisterAddress = ModbusRegisterAddress,
                Value = modeValue,
                Description = $"Запрошен режим: {mode}"
            };

            // Вызываем событие
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
            finally
            {
                _isInternalUpdate = false;
            }
        }

        // Метод для внешнего обновления режима
        public void SetMode(OperationMode mode)
        {
            if (_currentMode == mode)
                return;

            if (Dispatcher.CheckAccess())
            {
                ApplyModeToToggles(mode);
                return;
            }

            Dispatcher.Invoke(() => ApplyModeToToggles(mode));
        }
    }

    // Enum для режимов работы
    public enum OperationMode : ushort
    {
        Off = 0,
        SemiAuto = 1,
        Auto = 2
    }

    // Класс аргументов для Modbus команды
    public class ModbusCommandEventArgs : EventArgs
    {
        public byte UnitId { get; set; }
        public ushort RegisterAddress { get; set; }
        public ushort Value { get; set; }
        public string Description { get; set; }
    }
}

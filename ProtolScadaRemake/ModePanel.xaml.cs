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

        public OperationMode CurrentMode
        {
            get { return _currentMode; }
            private set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    ModeChanged?.Invoke(this, _currentMode);
                    SendModbusCommand();
                }
            }
        }

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

            // Если переключатель включился
            if (isChecked)
            {
                // Определяем какой режим был выбран
                if (toggle == OffToggle)
                {
                    CurrentMode = OperationMode.Off;
                    // Отключаем другие переключатели
                    SemiAutoToggle.IsChecked = false;
                    AutoToggle.IsChecked = false;
                }
                else if (toggle == SemiAutoToggle)
                {
                    CurrentMode = OperationMode.SemiAuto;
                    // Отключаем другие переключатели
                    OffToggle.IsChecked = false;
                    AutoToggle.IsChecked = false;
                }
                else if (toggle == AutoToggle)
                {
                    CurrentMode = OperationMode.Auto;
                    // Отключаем другие переключатели
                    OffToggle.IsChecked = false;
                    SemiAutoToggle.IsChecked = false;
                }
            }
            else
            {
                // Если выключили текущий активный режим
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if ((toggle == OffToggle && CurrentMode == OperationMode.Off) ||
                        (toggle == SemiAutoToggle && CurrentMode == OperationMode.SemiAuto) ||
                        (toggle == AutoToggle && CurrentMode == OperationMode.Auto))
                    {
                        // Включаем обратно
                        toggle.IsChecked = true;
                    }
                }), DispatcherPriority.Background);
            }
        }

        private void SendModbusCommand()
        {
            ushort modeValue = (ushort)_currentMode;

            // Создаем аргументы события
            var args = new ModbusCommandEventArgs
            {
                UnitId = UnitId,
                RegisterAddress = ModbusRegisterAddress,
                Value = modeValue,
                Description = $"Установлен режим: {_currentMode}"
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

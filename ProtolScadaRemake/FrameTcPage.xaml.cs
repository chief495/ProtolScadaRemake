using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameTcPage : UserControl
    {
        private ModbusManager _modbusManager;
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        public FrameTcPage(TGlobal global)
        {
            InitializeComponent();
            Global = global;

            // Инициализация ModbusManager
            _modbusManager = new ModbusManager(global);
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            System.Windows.Threading.DispatcherTimer timer = new();

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 100);
            timer.Start();

            // Инициализация панели режима
            InitializeModePanel();
        }

        private void timerTick(object sender, EventArgs e)
        {
            // Таймерная логика (например, обновление значений с Modbus)
        }

        private void InitializeModePanel()
        {
            if (TcModePanel != null)
            {
                // Устанавливаем начальный режим для TC
                TcModePanel.SetMode(OperationMode.Off);
            }
        }

        private void TcModePanel_ModeChanged(object sender, OperationMode mode)
        {
            // Обработка изменения режима для TC
            string modeText = mode switch
            {
                OperationMode.Off => "Выключен",
                OperationMode.SemiAuto => "Полуавтомат",
                OperationMode.Auto => "Автомат",
                _ => "Неизвестно"
            };

            // Обновляем переменную в SCADA (если нужно)
            if (Global != null && Global.Variables != null)
            {
                var variable = Global.Variables.GetByName("TC_MODE");
                if (variable != null)
                {
                    variable.ValueReal = (ushort)mode;
                }
            }

            // Логируем
            System.Diagnostics.Debug.WriteLine($"Режим TC изменен на: {modeText}");
        }

        private void TcModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            // Отправка команды через ModbusManager для TC
            if (_modbusManager != null)
            {
                bool success = _modbusManager.ProcessModeCommand(
                    e.UnitId,
                    e.RegisterAddress,
                    e.Value,
                    e.Description);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Команда Modbus для TC отправлена: {e.Description}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды Modbus для TC: {e.Description}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Тест команды для TC: {e.Description}");
            }
        }

        // Методы для внешнего управления режимом TC
        public void SetTcMode(OperationMode mode)
        {
            if (TcModePanel != null)
            {
                TcModePanel.SetMode(mode);
            }
        }

        public OperationMode GetCurrentTcMode()
        {
            return TcModePanel?.CurrentMode ?? OperationMode.Off;
        }

        // Пример: обработка команд из других частей приложения
        public void ProcessExternalCommand(string command)
        {
            if (command == "TC_OFF")
            {
                SetTcMode(OperationMode.Off);
            }
            else if (command == "TC_SEMI_AUTO")
            {
                SetTcMode(OperationMode.SemiAuto);
            }
            else if (command == "TC_AUTO")
            {
                SetTcMode(OperationMode.Auto);
            }
        }

        // Обновление режима из Modbus (например, при чтении значения из ПЛК)
        public void UpdateModeFromModbus(ushort modeValue)
        {
            OperationMode mode = modeValue switch
            {
                0 => OperationMode.Off,
                1 => OperationMode.SemiAuto,
                2 => OperationMode.Auto,
                _ => OperationMode.Off
            };

            SetTcMode(mode);
        }

        // Очистка ресурсов
        public void Cleanup()
        {
            if (_modbusManager != null)
            {
                _modbusManager.Disconnect();
            }
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameGroPage : UserControl
    {
        private ModbusManager _modbusManager;
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        public FrameGroPage(TGlobal global)
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

            // Инициализация панели
            InitializeModePanel();
        }

        private void timerTick(object sender, EventArgs e)
        {
        }

        private void InitializeModePanel()
        {
            if (GroModePanel != null)
            {
                // Устанавливаем начальный режим
                GroModePanel.SetMode(OperationMode.Off);
            }
        }

        private void GroModePanel_ModeChanged(object sender, OperationMode mode)
        {
            // Обработка изменения режима
            string modeText = mode switch
            {
                OperationMode.Off => "Выключен",
                OperationMode.SemiAuto => "Полуавтомат",
                OperationMode.Auto => "Автомат",
                _ => "Неизвестно"
            };

            // Можно показать сообщение или обновить статус
            // StatusText.Text = $"Режим работы: {modeText}";

            // Логируем
            System.Diagnostics.Debug.WriteLine($"Режим изменен на: {modeText}");
        }

        private void GroModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            // Отправка команды через ModbusManager
            if (_modbusManager != null)
            {
                // Исправленная строка: передаем параметры по отдельности
                bool success = _modbusManager.ProcessModeCommand(
                    e.UnitId,           // byte unitId
                    e.RegisterAddress,  // ushort registerAddress
                    e.Value,            // ushort value
                    e.Description       // string description
                );

                if (success)
                {
                    // Команда успешно отправлена
                    System.Diagnostics.Debug.WriteLine($"Команда Modbus отправлена: {e.Description}");
                }
                else
                {
                    // Ошибка отправки
                    System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды Modbus: {e.Description}");
                }
            }
            else
            {
                // Для тестирования без Modbus
                System.Diagnostics.Debug.WriteLine($"Тест команды: {e.Description}");
            }
        }

        // Методы для внешнего управления режимом
        public void SetGroMode(OperationMode mode)
        {
            if (GroModePanel != null)
            {
                GroModePanel.SetMode(mode);
            }
        }

        public OperationMode GetCurrentGroMode()
        {
            return GroModePanel?.CurrentMode ?? OperationMode.Off;
        }

        // Пример: обработка команд из других частей приложения
        public void ProcessExternalCommand(string command)
        {
            if (command == "GRO_OFF")
            {
                SetGroMode(OperationMode.Off);
            }
            else if (command == "GRO_SEMI_AUTO")
            {
                SetGroMode(OperationMode.SemiAuto);
            }
            else if (command == "GRO_AUTO")
            {
                SetGroMode(OperationMode.Auto);
            }
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
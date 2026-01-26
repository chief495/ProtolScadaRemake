using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameEmPage : UserControl
    {
        private ModbusManager _modbusManager;
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        public FrameEmPage(TGlobal global)
        {
            InitializeComponent();
            Global = global;

            // Инициализация ModbusManager
            _modbusManager = new ModbusManager(global);

            // Инициализация элементов
            Initialize();

            // Подписка на события панели режима
            if (EmModePanel != null)
            {
                EmModePanel.ModeChanged += EmModePanel_ModeChanged;
                EmModePanel.ModbusCommandRequested += EmModePanel_ModbusCommandRequested;
            }
        }

        private void Initialize()
        {
            // Инициализация LAHH151
            LAHH151.Global = Global;
            LAHH151.VarName = "LAHH151";
            LAHH151.Description = "Датчик уровня LAHH151";
            LAHH151.TagName.Text = "LAHH-151";
            LAHH151.EU = "%";

            // Инициализация P651
            P651.Global = Global;
            P651.VarName = "P651";
            P651.Description = "Насос P651";
            P651.TAGNAME.Text = "P651";

            // Установка начального режима
            if (EmModePanel != null)
            {
                EmModePanel.SetMode(OperationMode.Off);
            }
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            System.Windows.Threading.DispatcherTimer timer = new();

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 100);
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            // Обновление элементов
            LAHH151.UpdateElement();
            P651.UpdateElement();

            // Дополнительная логика в зависимости от режима
            UpdateModeDependentLogic();
        }

        private void UpdateModeDependentLogic()
        {
            if (EmModePanel == null) return;

            var currentMode = EmModePanel.CurrentMode;

            switch (currentMode)
            {
                case OperationMode.Off:
                    // Логика для выключенного режима
                    // Например, остановка оборудования
                    break;

                case OperationMode.SemiAuto:
                    // Логика для полуавтоматического режима
                    // Например, ручное управление с некоторой автоматизацией
                    break;

                case OperationMode.Auto:
                    // Логика для автоматического режима
                    // Например, полная автоматизация процессов
                    break;
            }
        }

        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            // Обработка изменения режима для EM страницы
            string modeText = mode switch
            {
                OperationMode.Off => "Выключен",
                OperationMode.SemiAuto => "Полуавтомат",
                OperationMode.Auto => "Автомат",
                _ => "Неизвестно"
            };

            // Обновление переменных в SCADA
            UpdateSCADAVariables(mode);

            // Логирование
            System.Diagnostics.Debug.WriteLine($"Режим EM изменен на: {modeText}");

            // Применение логики в зависимости от режима
            ApplyModeLogic(mode);
        }

        private void UpdateSCADAVariables(OperationMode mode)
        {
            if (Global != null && Global.Variables != null)
            {
                var variable = Global.Variables.GetByName("EM_MODE");
                if (variable != null)
                {
                    variable.ValueReal = (ushort)mode;
                }

                // Можно также обновить связанные переменные
                // Например, управление насосом в зависимости от режима
                if (mode == OperationMode.Auto)
                {
                    // Автоматическое управление
                    // Global.Variables.GetByName("P651_AUTO").ValueReal = 1;
                }
            }
        }

        private void ApplyModeLogic(OperationMode mode)
        {
            // Применение логики в зависимости от выбранного режима
            switch (mode)
            {
                case OperationMode.Off:
                    // Отключение всех автоматических функций
                    // P651.SetManualMode();
                    break;

                case OperationMode.SemiAuto:
                    // Включение полуавтоматического режима
                    // P651.SetSemiAutoMode();
                    break;

                case OperationMode.Auto:
                    // Включение полной автоматизации
                    // P651.SetAutoMode();
                    break;
            }
        }

        private void EmModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            // Отправка команды через ModbusManager для EM
            if (_modbusManager != null)
            {
                bool success = _modbusManager.ProcessModeCommand(
                    e.UnitId,
                    e.RegisterAddress,
                    e.Value,
                    e.Description);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Команда Modbus для EM отправлена: {e.Description}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды Modbus для EM: {e.Description}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Тест команды для EM: {e.Description}");
            }
        }

        // Методы для внешнего управления режимом EM
        public void SetEmMode(OperationMode mode)
        {
            if (EmModePanel != null)
            {
                EmModePanel.SetMode(mode);
            }
        }

        public OperationMode GetCurrentEmMode()
        {
            return EmModePanel?.CurrentMode ?? OperationMode.Off;
        }

        // Пример: обработка команд из других частей приложения
        public void ProcessExternalCommand(string command)
        {
            if (command == "EM_OFF")
            {
                SetEmMode(OperationMode.Off);
            }
            else if (command == "EM_SEMI_AUTO")
            {
                SetEmMode(OperationMode.SemiAuto);
            }
            else if (command == "EM_AUTO")
            {
                SetEmMode(OperationMode.Auto);
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

            SetEmMode(mode);
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
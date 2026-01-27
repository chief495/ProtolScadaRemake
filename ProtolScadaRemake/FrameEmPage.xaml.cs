using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameEmPage : UserControl
    {
        private TGlobal _global;

        public FrameEmPage(TGlobal global)
        {
            InitializeComponent();
            _global = global;
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Инициализация LAHH151
                LAHH151.Global = _global;
                LAHH151.VarName = "LAHH151";
                LAHH151.Description = "Датчик уровня LAHH151";
                LAHH151.TagName.Text = "LAHH-151";
                LAHH151.EU = "%";

                // Инициализация P651
                P651.Global = _global;
                P651.VarName = "P651";
                P651.Description = "Насос P651";
                P651.TAGNAME.Text = "P651";

                // Инициализация панелей
                if (StartupPanelControl != null)
                {
                    StartupPanelControl.Global = _global;
                    SubscribeToStartupPanelEvents();
                }

                if (PerformancePanelControl != null)
                {
                    PerformancePanelControl.Global = _global;
                    SubscribeToPerformancePanelEvents();
                }

                if (UnloadPanelControl != null)
                {
                    UnloadPanelControl.Global = _global;
                }

                if (EmModePanel != null)
                {
                    EmModePanel.SetMode(OperationMode.Off);
                    SubscribeToModePanelEvents();
                }

                // Подписка на события глобального объекта
                _global.OnVariablesUpdated += OnGlobalVariablesUpdated;

                // Инициализация Modbus
                await _global.InitializeModbusAsync();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации FrameEmPage: {ex.Message}");
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // Этот метод вызывается при инициализации UserControl
            System.Diagnostics.Debug.WriteLine("FrameEmPage инициализирован");
        }

        private void SubscribeToModePanelEvents()
        {
            if (EmModePanel != null)
            {
                EmModePanel.ModeChanged += EmModePanel_ModeChanged;
                EmModePanel.ModbusCommandRequested += EmModePanel_ModbusCommandRequested;
            }
        }

        private void SubscribeToStartupPanelEvents()
        {
            if (StartupPanelControl != null)
            {
                StartupPanelControl.StartStartupButtonClick += StartupPanel_StartStartupButtonClick;
                StartupPanelControl.StopStartupButtonClick += StartupPanel_StopStartupButtonClick;
                StartupPanelControl.AutoModeButtonClick += StartupPanel_AutoModeButtonClick;
                StartupPanelControl.OffModeButtonClick += StartupPanel_OffModeButtonClick;
            }
        }

        private void SubscribeToPerformancePanelEvents()
        {
            if (PerformancePanelControl != null)
            {
                PerformancePanelControl.SetMassFlowButtonClick += PerformancePanel_SetMassFlowButtonClick;
                PerformancePanelControl.StartProcessButtonClick += PerformancePanel_StartProcessButtonClick;
                PerformancePanelControl.StopProcessButtonClick += PerformancePanel_StopProcessButtonClick;
                PerformancePanelControl.DojatProcessButtonClick += PerformancePanel_DojatProcessButtonClick;
                PerformancePanelControl.EmergencyStopButtonClick += PerformancePanel_EmergencyStopButtonClick;
            }
        }

        private void OnGlobalVariablesUpdated(object sender, EventArgs e)
        {
            // Обновляем панели при изменении глобальных переменных
            Dispatcher.Invoke(() =>
            {
                if (StartupPanelControl != null)
                    StartupPanelControl.UpdateFromGlobal();

                if (PerformancePanelControl != null)
                    PerformancePanelControl.UpdateFromGlobal();

                // Обновляем другие элементы UI
                UpdateUIFromVariables();
            });
        }

        private void UpdateUIFromVariables()
        {
            // Обновление LAHH151
            var lahh151Var = _global.Variables.GetByName("LAHH151_Value");
            if (lahh151Var != null && LAHH151 != null)
            {
                // Здесь должен быть код обновления элемента LAHH151
            }

            // Обновление P651
            var p651Var = _global.Variables.GetByName("P651_IsWork");
            if (p651Var != null && P651 != null)
            {
                // Здесь должен быть код обновления элемента P651
            }
        }

        // Обработчики событий панели затравки
        private void StartupPanel_StartStartupButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Запуск затравки");
            _global.SendCommand("EM_ZatravkaStart", "true");
        }

        private void StartupPanel_StopStartupButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Остановка затравки");
            _global.SendCommand("EM_ZatravkaStop", "true");
        }

        private void StartupPanel_AutoModeButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Режим АВТОМАТ для затравки");
            _global.SendCommand("EM_RejimToAuto", "true");
        }

        private void StartupPanel_OffModeButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Режим ВЫКЛ для затравки");
            _global.SendCommand("EM_RejimToOff", "true");
        }

        // Обработчики событий панели производительности
        private void PerformancePanel_SetMassFlowButtonClick(object sender, RoutedEventArgs e)
        {
            if (PerformancePanelControl != null)
            {
                int massFlow = PerformancePanelControl.GetMassFlowSetpoint();
                System.Diagnostics.Debug.WriteLine($"Установка производительности: {massFlow} кг/ч");
                _global.SendCommand("EM_AutoMassFlowSp", massFlow.ToString());
            }
        }

        private void PerformancePanel_StartProcessButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Запуск процесса производства");
            _global.SendCommand("EM_AutoStart", "true");
        }

        private void PerformancePanel_StopProcessButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Остановка процесса производства");
            _global.SendCommand("EM_AutoStop", "true");
        }

        private void PerformancePanel_DojatProcessButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Запуск дожима");
            _global.SendCommand("EM_AutoDojat", "true");
        }

        private void PerformancePanel_EmergencyStopButtonClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Аварийный останов");
            _global.SendCommand("EM_AutoFastStop", "true");
        }

        // Обработчики событий панели режима
        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            string modeText = mode switch
            {
                OperationMode.Off => "Выключен",
                OperationMode.SemiAuto => "Полуавтомат",
                OperationMode.Auto => "Автомат",
                _ => "Неизвестно"
            };

            System.Diagnostics.Debug.WriteLine($"Режим EM изменен на: {modeText}");
        }

        private void EmModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            bool success = _global.ProcessModeCommand(e.UnitId, e.RegisterAddress, e.Value, e.Description);
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"Команда Modbus для EM отправлена: {e.Description}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды Modbus для EM: {e.Description}");
            }
        }

        public void Cleanup()
        {
            _global.DisconnectAll();
        }
    }
}
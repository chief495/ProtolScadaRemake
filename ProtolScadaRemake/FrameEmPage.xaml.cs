using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameEmPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _updateTimer;
        private bool _isInitialized = false;

        public FrameEmPage(TGlobal global)
        {
            InitializeComponent();
            _global = global;

            // Подписываемся на событие загрузки
            this.Loaded += FrameEmPage_Loaded;
        }

        private async void FrameEmPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;

            try
            {
                // Инициализация панелей управления
                InitializePanels();

                // Подписка на события
                SubscribeToEvents();

                // Запуск таймера обновления
                StartUpdateTimer();

                // Инициализация Modbus
                await _global.InitializeModbusAsync();

                _isInitialized = true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации FrameEmPage: {ex.Message}");
            }
        }

        private void InitializePanels()
        {
            // Инициализация панели запуска
            if (StartupPanelControl != null)
            {
                StartupPanelControl.Global = _global;
                SubscribeToStartupPanelEvents();
            }

            // Инициализация панели производительности
            if (PerformancePanelControl != null)
            {
                PerformancePanelControl.Global = _global;
                SubscribeToPerformancePanelEvents();
            }

            // Инициализация панели отгрузки
            if (UnloadPanelControl != null)
            {
                UnloadPanelControl.Global = _global;
                SubscribeToUnloadPanelEvents();
            }

            // Инициализация панели режима
            if (EmModePanel != null)
            {
                EmModePanel.SetMode(OperationMode.Off);
                SubscribeToModePanelEvents();
            }
        }

        private void SubscribeToEvents()
        {
            // Подписка на события обновления глобальных переменных
            _global.OnVariablesUpdated += OnGlobalVariablesUpdated;
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 Гц как в старом проекте
            _updateTimer.Tick += (sender, e) => UpdateUIFromVariables();
            _updateTimer.Start();
        }

        private void OnGlobalVariablesUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUIFromVariables();
            });
        }

        private void UpdateUIFromVariables()
        {
            try
            {
                // 1. Обновляем режим работы EM
                UpdateRejimStatus();

                // 2. Обновляем режим отгрузки и управление панелью отгрузки
                UpdateUnloadStatus();

                // 3. Обновляем данные в панелях управления
                UpdateControlPanels();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления UI: {ex.Message}");
            }
        }

        private void UpdateRejimStatus()
        {
            var rejimTag = _global.Variables.GetByName("EM_Rejim");
            if (rejimTag != null)
            {
                int rejimValue = (int)rejimTag.ValueReal;

                // Управление видимостью панелей как в старом проекте
                // В старом проекте: StartupPanel и PerformancePanel управляются через EM_Rejim

                // Если режим не OFF (0), то показываем панели управления
                bool showControlPanels = rejimValue != 0;

                if (StartupPanelControl != null)
                    StartupPanelControl.Visibility = showControlPanels ? Visibility.Visible : Visibility.Collapsed;

                if (PerformancePanelControl != null)
                    PerformancePanelControl.Visibility = showControlPanels ? Visibility.Visible : Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"EM_Rejim = {rejimValue}, showControlPanels = {showControlPanels}");
            }
        }

        private void UpdateUnloadStatus()
        {
            var unloadRejimTag = _global.Variables.GetByName("EM_Unloading_Rejim");
            if (unloadRejimTag != null && UnloadPanelControl != null)
            {
                string rejimValue = unloadRejimTag.ValueString;

                // Обновляем режим в панели отгрузки (у UnloadPanel есть метод UpdateMode)
                UnloadPanelControl.UpdateMode(rejimValue);

                // Управляем видимостью панели отгрузки
                // В старом проекте: UnloadPanel видна всегда, но внутри нее меняются панели
                // У вас одна панель, которая сама управляет своим контентом
                UnloadPanelControl.Visibility = Visibility.Visible;

                System.Diagnostics.Debug.WriteLine($"EM_Unloading_Rejim = {rejimValue}");
            }
            else if (UnloadPanelControl != null)
            {
                // Если нет данных, скрываем панель
                UnloadPanelControl.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateControlPanels()
        {
            // Обновление данных в панелях управления
            StartupPanelControl?.UpdateFromGlobal();
            PerformancePanelControl?.UpdateFromGlobal();
            UnloadPanelControl?.UpdateFromGlobal();
        }

        // Методы для подписки на события панелей
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

        private void SubscribeToUnloadPanelEvents()
        {
            if (UnloadPanelControl != null)
            {
                UnloadPanelControl.SetParamsButtonClick += UnloadPanel_SetParamsButtonClick;
                UnloadPanelControl.ResetButtonClick += UnloadPanel_ResetButtonClick;
                UnloadPanelControl.PultModeClick += UnloadPanel_PultModeClick;
                UnloadPanelControl.TimeModeClick += UnloadPanel_TimeModeClick;
                UnloadPanelControl.MassModeClick += UnloadPanel_MassModeClick;
                UnloadPanelControl.TorirovanieButtonClick += UnloadPanel_TorirovanieButtonClick;
            }
        }

        private void SubscribeToModePanelEvents()
        {
            if (EmModePanel != null)
            {
                EmModePanel.ModeChanged += EmModePanel_ModeChanged;
                EmModePanel.ModbusCommandRequested += EmModePanel_ModbusCommandRequested;
            }
        }

        // Обработчики событий панелей - передача команд как в старом проекте

        private void StartupPanel_StartStartupButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_ZatravkaStart", "true");
        }

        private void StartupPanel_StopStartupButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_ZatravkaStop", "true");
        }

        private void StartupPanel_AutoModeButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_RejimToAuto", "true");
        }

        private void StartupPanel_OffModeButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_RejimToOff", "true");
        }

        private void PerformancePanel_SetMassFlowButtonClick(object sender, RoutedEventArgs e)
        {
            if (PerformancePanelControl != null)
            {
                int massFlow = PerformancePanelControl.GetMassFlowSetpoint();
                SendCommandToController("EM_AutoMassFlowSp", massFlow.ToString());
            }
        }

        private void PerformancePanel_StartProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoStart", "true");
        }

        private void PerformancePanel_StopProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoStop", "true");
        }

        private void PerformancePanel_DojatProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoDojat", "true");
        }

        private void PerformancePanel_EmergencyStopButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoFastStop", "true");
        }

        private void UnloadPanel_SetParamsButtonClick(object sender, RoutedEventArgs e)
        {
            // Логика в самой панели UnloadPanel уже реализована
        }

        private void UnloadPanel_ResetButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_Unload_Reset", "true");
        }

        private void UnloadPanel_PultModeClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_Unloading_PultButton", "true");
        }

        private void UnloadPanel_TimeModeClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_Unloading_TimeButton", "true");
        }

        private void UnloadPanel_MassModeClick(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_Unloading_MassButton", "true");
        }

        private void UnloadPanel_TorirovanieButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new DialogTorirovanie(_global);
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    dialog.Owner = parentWindow;
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога тарирования: {ex.Message}");
            }
        }

        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            System.Diagnostics.Debug.WriteLine($"Режим EM изменен на: {mode}");
        }

        private void EmModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            bool success = _global.ProcessModeCommand(e.UnitId, e.RegisterAddress, e.Value, e.Description);
            System.Diagnostics.Debug.WriteLine(success
                ? $"Команда Modbus для EM отправлена: {e.Description}"
                : $"Ошибка отправки команды Modbus для EM: {e.Description}");
        }

        private void SendCommandToController(string commandName, string value)
        {
            TCommandTag command = _global.Commands.GetByName(commandName);
            if (command != null)
            {
                try
                {
                    command.WriteValue = value;
                    command.NeedToWrite = true;
                    command.SendToController();
                }
                catch { }
            }
        }

        // Обработчики для кнопок на мнемосхеме (если они есть в XAML)
        public void UserControl_Initialized(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FrameEmPage инициализирован (UserControl_Initialized)");
        }

        public void AutoStartProcessButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoStart", "true");
        }

        public void AutoStopProcessButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoStop", "true");
        }

        public void AutoDojatButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_AutoDojat", "true");
        }

        public void RejomOffButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_RejimToOff", "true");
        }

        public void RejimAutoButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_RejimToAuto", "true");
        }

        public void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_ZatravkaStart", "true");
        }

        public void AutoStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToController("EM_ZatravkaStop", "true");
        }

        public void Cleanup()
        {
            _updateTimer?.Stop();
            _global.DisconnectAll();
        }
    }
}
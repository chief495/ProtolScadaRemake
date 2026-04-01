using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameEmPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;
        private OperationMode? _pendingMode;
        private DateTime _pendingModeSince;
        private const int PendingModeTimeoutSec = 5;
        private bool _isTorirovanieDialogOpen = false;
        private DateTime _lastTorirovanieClick = DateTime.MinValue;
        private bool _panelsInitialized = false;
        private bool _eventsSubscribed = false;

        public FrameEmPage()
        {
            InitializeComponent();
            Loaded += FrameEmPage_Loaded;
            Unloaded += FrameEmPage_Unloaded;
        }

        public void Initialize(TGlobal global)
        {
            _global = global;
            InitializeElements();

            _repaintTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _repaintTimer.Tick += RepaintTimer_Tick;

            UpdateOperationMode();
            UpdatePanelsVisibility();
            SubscribeToEvents();
        }

        private void FrameEmPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateOperationMode();
            UpdatePanelsVisibility();
            _repaintTimer?.Start();
        }

        private void FrameEmPage_Unloaded(object sender, RoutedEventArgs e) => Cleanup();

        private void InitializeElements()
        {
            try
            {
                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");
                InitializeSensor(TT252, "TT252", "Датчик температуры TT-252", "TT-252", "°C");
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");
                InitializeSensor(LT150, "LT150", "Датчик уровня LT150", "LT-150", "%");
                InitializeFM(FM601, "FM601", "Расходомер FM601", "FM601", "кг/мин");
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "атм");
                InitializeSensor(PT606, "PT606", "Датчик давления PT606", "PT-606", "атм");
                InitializeSensor(LT253, "LT253", "Датчик уровня LT253", "LT-253", "%");
                InitializeFM(FM602, "FM602", "Расходомер FM602", "FM602", "кг/мин");
                InitializeSensor(LT651, "LT651", "Датчик уровня LT651", "LT-651", "%");
                InitializeSensor(PT652, "PT652", "Давление PT652", "PT-652", "атм");
                InitializeSensor(PT604, "PT604", "Давление PT604", "PT-604", "атм");

                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");
                InitializeDiscreteSensor(LAHH653, "LAHH653", "Датчик уровня LAHH653", "LAHH-653");

                // Подписка на события миксеров (с отпиской)
                if (M150 != null)
                {
                    M150.StateChanged -= M150Mixer_StateChanged;
                    M150.StateChanged += M150Mixer_StateChanged;
                }
                if (M250 != null)
                {
                    M250.StateChanged -= M250Mixer_StateChanged;
                    M250.StateChanged += M250Mixer_StateChanged;
                }
                if (EmModePanel != null)
                {
                    EmModePanel.ModeChanged -= EmModePanel_ModeChanged;
                    EmModePanel.ModeChanged += EmModePanel_ModeChanged;
                }

                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601", "P-601");
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602", "P-602");
                InitializePumpUz(M600, "M600", "Миксер M-600", "M-600");
                InitializePumpUzUnderPanel(P651, "P651", "Насос P-651", "P-651");

                Initialize3Valve(SV601, "V601", "Клапан SV-601", "SV-601");
                Initialize3Valve(SV602, "V602", "Клапан SV-602", "SV-602");
                InitializeValve(V505, "V505", "Клапан V-505", "V-505");

                InitializePanels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов EM: {ex.Message}");
            }
        }

        private void SubscribeToEvents()
        {
            if (_global != null && !_eventsSubscribed)
            {
                _global.OnVariablesUpdated -= Global_OnVariablesUpdated;
                _global.OnVariablesUpdated += Global_OnVariablesUpdated;
                _eventsSubscribed = true;
            }
        }

        private void Global_OnVariablesUpdated(object sender, EventArgs e)
        {
            UpdateAllElements();
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            if (_global == null) return;
            _repaintTimer.Stop();
            try
            {
                ResetCommands();
                UpdateAllElements();
                UpdatePanelInfo();
                UpdateModePanelIfNeeded();
                UpdateOperationMode();
                UpdatePanelsVisibility();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления EM: {ex.Message}");
            }
            finally
            {
                _repaintTimer.Start();
            }
        }

        private void ResetCommands()
        {
            try
            {
                if (_global?.Commands == null) return;
                var cmd = _global.Commands.GetByName("T150_StartMixer");
                if (cmd != null && !cmd.NeedToWrite) cmd.WriteValue = "false";
                cmd = _global.Commands.GetByName("M250_StartMixer");
                if (cmd != null && !cmd.NeedToWrite) cmd.WriteValue = "false";
            }
            catch { }
        }

        private void UpdateAllElements()
        {
            try
            {
                TT152?.UpdateElement();
                TT252?.UpdateElement();
                TT602?.UpdateElement();
                LT150?.UpdateElement();
                FM601?.UpdateElement();
                PT601?.UpdateElement();
                PT606?.UpdateElement();
                LT253?.UpdateElement();
                FM602?.UpdateElement();
                LT651?.UpdateElement();
                PT652?.UpdateElement();
                PT604?.UpdateElement();

                LAHH151?.UpdateElement();
                LALL153?.UpdateElement();
                LAHH251?.UpdateElement();
                LAHH653?.UpdateElement();

                P601?.UpdateElement();
                P602?.UpdateElement();
                M600?.UpdateElement();
                P651?.UpdateElement();

                SV601?.UpdateElement();
                SV602?.UpdateElement();
                V505?.UpdateElement();

                UpdateMixerTogglesFromVariables();
                UpdateLiquidGauges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления элементов EM: {ex.Message}");
            }
        }

        private void UpdateMixerTogglesFromVariables()
        {
            try
            {
                var m150Tag = _global?.Variables?.GetByName("M150_IsWork");
                if (m150Tag != null && M150 != null)
                {
                    bool isWorking = m150Tag.ValueReal > 0;
                    if (M150.IsChecked != isWorking)
                    {
                        M150.IsChecked = isWorking;
                    }
                }

                var m250Tag = _global?.Variables?.GetByName("M250_IsWork");
                if (m250Tag != null && M250 != null)
                {
                    bool isWorking = m250Tag.ValueReal > 0;
                    if (M250.IsChecked != isWorking)
                    {
                        M250.IsChecked = isWorking;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления переключателей: {ex.Message}");
            }
        }

        private void UpdateLiquidGauges()
        {
            if (_global?.Variables == null) return;

            GaugeT150.FillLevel = ReadLevelPercent("LT150_Value");
            GaugeT250.FillLevel = ReadLevelPercent("LT253_Value");
            GaugeT650.FillLevel = ReadLevelPercent("LT651_Value");
        }

        private double ReadLevelPercent(string variableName)
        {
            var tag = _global?.Variables?.GetByName(variableName);
            if (tag == null) return 0;
            return Math.Max(0, Math.Min(100, tag.ValueReal));
        }

        private void UpdatePanelInfo()
        {
            try
            {
                StartupPanelControl?.UpdateFromGlobal();
                PerformancePanelControl?.UpdateFromGlobal();
                UnloadPanelControl?.UpdateFromGlobal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления панелей EM: {ex.Message}");
            }
        }

        private void UpdateModePanelIfNeeded()
        {
            try
            {
                var tag = _global?.Variables?.GetByName("EM_Rejim");
                if (tag == null) return;
                double val = tag.ValueReal;
                UpdateModePanelStatus(val);
                UpdateModePanelButtons(val);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления ModePanel: {ex.Message}");
            }
        }

        private void UpdateOperationModeFromPlc(double rejimValue)
        {
            if (EmModePanel == null) return;

            OperationMode mode = rejimValue switch
            {
                0 => OperationMode.Off,
                1 => OperationMode.SemiAuto,
                _ => OperationMode.Auto
            };

            if (EmModePanel.CurrentMode != mode)
            {
                EmModePanel.SetMode(mode);
            }
        }

        private void UpdateModePanelStatus(double rejimValue)
        {
            try
            {
                var currRejimLabel = EmModePanel?.FindName("CurrRejimLabel") as TextBlock;
                var currStageLabel = EmModePanel?.FindName("CurrStageLabel") as TextBlock;
                if (currRejimLabel != null && currStageLabel != null)
                {
                    switch (rejimValue)
                    {
                        case 0:
                            currRejimLabel.Text = "OFF";
                            currRejimLabel.Foreground = Brushes.White;
                            currStageLabel.Text = "OFF";
                            currStageLabel.Foreground = Brushes.White;
                            break;
                        case 1:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Ожидание";
                            currStageLabel.Foreground = Brushes.White;
                            break;
                        case 2:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 3:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Наполнение топливной смесью.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 4:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Наполнение ГРО.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 5:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Смешивание.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 6:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Производство. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 7:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Производство.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 8:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Промывка.";
                            currStageLabel.Foreground = Brushes.LightBlue;
                            break;
                        case 9:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Дожим. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 10:
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Дожим.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        default:
                            currRejimLabel.Text = $"Неизвестно ({rejimValue})";
                            currStageLabel.Text = "";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка UpdateModePanelStatus: {ex.Message}");
            }
        }

        private void UpdateModePanelButtons(double rejimValue)
        {
            try
            {
                var btnOff = EmModePanel?.FindName("RejimOffButton") as Button;
                var btnAuto = EmModePanel?.FindName("RejimAutoButton") as Button;
                if (btnOff == null || btnAuto == null) return;
                if (rejimValue == 0)
                {
                    btnOff.Background = Brushes.Green;
                    btnOff.Foreground = Brushes.White;
                    btnAuto.Background = Brushes.Gray;
                    btnAuto.Foreground = Brushes.White;
                }
                else
                {
                    btnOff.Background = Brushes.Gray;
                    btnOff.Foreground = Brushes.White;
                    btnAuto.Background = Brushes.Green;
                    btnAuto.Foreground = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка UpdateModePanelButtons: {ex.Message}");
            }
        }

        private void UpdateOperationMode()
        {
            var tag = _global?.Variables?.GetByName("EM_Rejim");
            if (tag == null || EmModePanel == null) return;
            int rejimValue = (int)tag.ValueReal;
            OperationMode curMode = rejimValue == 0 ? OperationMode.Off : OperationMode.Auto;

            if (_pendingMode.HasValue && _pendingMode.Value == curMode)
            {
                _pendingMode = null;
                EmModePanel.IsEnabled = true;
            }

            if (_pendingMode.HasValue && (DateTime.Now - _pendingModeSince).TotalSeconds > PendingModeTimeoutSec)
            {
                _global?.Log?.Add("Система", $"Тайм-аут перехода в режим {_pendingMode.Value}", 2);
                _pendingMode = null;
                EmModePanel.IsEnabled = true;
            }

            if (EmModePanel.CurrentMode != curMode)
            {
                EmModePanel.SetMode(curMode);
            }
        }

        private void UpdatePanelsVisibility()
        {
            try
            {
                var rejimTag = _global?.Variables?.GetByName("EM_Rejim");
                if (EmModePanel != null) EmModePanel.Visibility = Visibility.Visible;

                if (rejimTag == null)
                {
                    if (StartupPanelControl != null) StartupPanelControl.Visibility = Visibility.Collapsed;
                    if (PerformancePanelControl != null) PerformancePanelControl.Visibility = Visibility.Collapsed;
                    if (UnloadPanelControl != null) UnloadPanelControl.Visibility = Visibility.Collapsed;
                    return;
                }

                double rejimValue = rejimTag.ValueReal;
                bool isOff = rejimValue == 0;

                if (StartupPanelControl != null)
                    StartupPanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;
                if (PerformancePanelControl != null)
                    PerformancePanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;
                if (UnloadPanelControl != null)
                    UnloadPanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления видимости панелей EM: {ex.Message}");
            }
        }

        private void InitializePanels()
        {
            if (_panelsInitialized) return;
            _panelsInitialized = true;

            try
            {
                if (StartupPanelControl != null)
                {
                    var p = StartupPanelControl.GetType().GetProperty("Global");
                    if (p != null && _global != null) p.SetValue(StartupPanelControl, _global);

                    StartupPanelControl.StartStartupButtonClick -= StartupPanel_StartStartupButtonClick;
                    StartupPanelControl.StopStartupButtonClick -= StartupPanel_StopStartupButtonClick;
                    StartupPanelControl.AutoModeButtonClick -= StartupPanel_AutoModeButtonClick;
                    StartupPanelControl.OffModeButtonClick -= StartupPanel_OffModeButtonClick;

                    StartupPanelControl.StartStartupButtonClick += StartupPanel_StartStartupButtonClick;
                    StartupPanelControl.StopStartupButtonClick += StartupPanel_StopStartupButtonClick;
                    StartupPanelControl.AutoModeButtonClick += StartupPanel_AutoModeButtonClick;
                    StartupPanelControl.OffModeButtonClick += StartupPanel_OffModeButtonClick;
                }

                if (PerformancePanelControl != null)
                {
                    var p = PerformancePanelControl.GetType().GetProperty("Global");
                    if (p != null && _global != null) p.SetValue(PerformancePanelControl, _global);

                    PerformancePanelControl.SetMassFlowButtonClick -= PerformancePanel_SetMassFlowButtonClick;
                    PerformancePanelControl.StartProcessButtonClick -= PerformancePanel_StartProcessButtonClick;
                    PerformancePanelControl.StopProcessButtonClick -= PerformancePanel_StopProcessButtonClick;
                    PerformancePanelControl.DojatProcessButtonClick -= PerformancePanel_DojatProcessButtonClick;
                    PerformancePanelControl.EmergencyStopButtonClick -= PerformancePanel_EmergencyStopButtonClick;

                    PerformancePanelControl.SetMassFlowButtonClick += PerformancePanel_SetMassFlowButtonClick;
                    PerformancePanelControl.StartProcessButtonClick += PerformancePanel_StartProcessButtonClick;
                    PerformancePanelControl.StopProcessButtonClick += PerformancePanel_StopProcessButtonClick;
                    PerformancePanelControl.DojatProcessButtonClick += PerformancePanel_DojatProcessButtonClick;
                    PerformancePanelControl.EmergencyStopButtonClick += PerformancePanel_EmergencyStopButtonClick;
                }

                if (UnloadPanelControl != null)
                {
                    var p = UnloadPanelControl.GetType().GetProperty("Global");
                    if (p != null && _global != null) p.SetValue(UnloadPanelControl, _global);

                    UnloadPanelControl.SetParamsButtonClick -= UnloadPanel_SetParamsButtonClick;
                    UnloadPanelControl.ResetButtonClick -= UnloadPanel_ResetButtonClick;
                    UnloadPanelControl.PultModeClick -= UnloadPanel_PultModeClick;
                    UnloadPanelControl.TimeModeClick -= UnloadPanel_TimeModeClick;
                    UnloadPanelControl.MassModeClick -= UnloadPanel_MassModeClick;
                    UnloadPanelControl.TorirovanieButtonClick -= UnloadPanel_TorirovanieButtonClick;

                    UnloadPanelControl.SetParamsButtonClick += UnloadPanel_SetParamsButtonClick;
                    UnloadPanelControl.ResetButtonClick += UnloadPanel_ResetButtonClick;
                    UnloadPanelControl.PultModeClick += UnloadPanel_PultModeClick;
                    UnloadPanelControl.TimeModeClick += UnloadPanel_TimeModeClick;
                    UnloadPanelControl.MassModeClick += UnloadPanel_MassModeClick;
                    UnloadPanelControl.TorirovanieButtonClick += UnloadPanel_TorirovanieButtonClick;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации панелей: {ex.Message}");
            }
        }

        // ========== МЕТОДЫ ИНИЦИАЛИЗАЦИИ ЭЛЕМЕНТОВ ==========

        private void InitializeSensor(Element_AI sensor, string varName, string description, string tagName, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;
                sensor.EU = eu;
            }
        }

        private void InitializeFM(Element_FM sensor, string varName, string description, string tagName, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;
                sensor.EU = eu;
                sensor.Designation = description;
            }
        }

        private void InitializeDiscreteSensor(Element_DI sensor, string varName, string description, string tagName)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;
            }
        }

        private void InitializePumpUz(Element_PumpUz pump, string varName, string description, string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;
            }
        }

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump, string varName, string description, string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;
            }
        }

        private void Initialize3Valve(Element_3ValveH valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void InitializeValve(Element_ValveV valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ МИКСЕРОВ ==========

        private void M150Mixer_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            string message = isChecked ? "Включение миксера T150" : "Отключение миксера T150";
            _global.Log.Add("Пользователь", message, 1);
            SendCommand("T150_StartMixer", isChecked ? "true" : "false");
        }

        private void M250Mixer_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            string message = isChecked ? "Включение миксера M250" : "Отключение миксера M250";
            _global.Log.Add("Пользователь", message, 1);
            SendCommand("M250_StartMixer", isChecked ? "true" : "false");
        }

        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            if (_global == null) return;

            _pendingMode = mode;
            _pendingModeSince = DateTime.Now;
            EmModePanel.IsEnabled = false;

            string commandName = mode == OperationMode.Off ? "EM_RejimToOff" : "EM_RejimToAuto";
            var command = _global.Commands.GetByName(commandName);

            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                command.SendToController();
                _global.Log.Add("Пользователь", $"Запрос перехода в режим {mode}", 1);
            }
            else
            {
                _global?.Log?.Add("Система", $"Не найдена команда {commandName}", 2);
                _pendingMode = null;
                EmModePanel.IsEnabled = true;
            }
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ ЗАТРАВКИ ==========

        private void StartupPanel_StartStartupButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_ZatravkaStart", "true");
        }

        private void StartupPanel_StopStartupButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_ZatravkaStop", "true");
        }

        private void StartupPanel_AutoModeButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_RejimToAuto", "true");
        }

        private void StartupPanel_OffModeButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_RejimToOff", "true");
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ ПРОИЗВОДСТВА ==========

        private void PerformancePanel_SetMassFlowButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_AutoMassFlowSp_Write", "true");
        }

        private void PerformancePanel_StartProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_AutoStart", "true");
        }

        private void PerformancePanel_StopProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_AutoStop", "true");
        }

        private void PerformancePanel_DojatProcessButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_AutoDojat", "true");
        }

        private void PerformancePanel_EmergencyStopButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_AutoFastStop", "true");
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ПАНЕЛИ ВЫГРУЗКИ ==========

        private void UnloadPanel_SetParamsButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_Unload_SetParams", "true");
        }

        private void UnloadPanel_ResetButtonClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_Unload_Reset", "true");
        }

        private void UnloadPanel_PultModeClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_Unloading_PultButton", "true");
        }

        private void UnloadPanel_TimeModeClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_Unloading_TimeButton", "true");
        }

        private void UnloadPanel_MassModeClick(object sender, RoutedEventArgs e)
        {
            SendCommand("EM_Unloading_MassButton", "true");
        }

        private void UnloadPanel_TorirovanieButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // Защита от двойного клика (игнорируем клики чаще чем раз в 500мс)
            if ((DateTime.Now - _lastTorirovanieClick).TotalMilliseconds < 500)
            {
                return;
            }
            _lastTorirovanieClick = DateTime.Now;

            if (_isTorirovanieDialogOpen)
            {
                return;
            }

            _isTorirovanieDialogOpen = true;

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
            finally
            {
                // Отложенный сброс флага через Dispatcher
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _isTorirovanieDialogOpen = false;
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        private void SendCommand(string commandName, string value)
        {
            try
            {
                var cmd = _global?.Commands?.GetByName(commandName);
                if (cmd != null)
                {
                    cmd.WriteValue = value;
                    cmd.NeedToWrite = true;
                    cmd.SendToController();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды {commandName}: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            _repaintTimer?.Stop();

            if (_global != null)
            {
                _global.OnVariablesUpdated -= Global_OnVariablesUpdated;
            }

            _eventsSubscribed = false;
            _panelsInitialized = false;
        }
    }
}
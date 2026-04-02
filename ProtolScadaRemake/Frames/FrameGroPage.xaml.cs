using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameGroPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        public FrameGroPage()
        {
            InitializeComponent();
            Loaded += FrameGroPage_Loaded;
            Unloaded += FrameGroPage_Unloaded;
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

            SubscribeToEvents();

            if (GroModePanel != null)
            {
                GroModePanel.ModeChanged += GroModePanel_ModeChanged;
                GroModePanel.ModbusCommandRequested += GroModePanel_ModbusCommandRequested;
            }

            UpdatePanelsVisibility();
        }

        private void FrameGroPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePanelsVisibility();
            _repaintTimer?.Start();
        }

        private void FrameGroPage_Unloaded(object sender, RoutedEventArgs e) => Cleanup();

        private void SubscribeToEvents()
        {
            if (_global != null) _global.OnVariablesUpdated += Global_OnVariablesUpdated;
        }

        private void Global_OnVariablesUpdated(object sender, EventArgs e) => UpdateAllElements();

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            _repaintTimer.Stop();
            try
            {
                UpdateAllElements();
                //ResetCommands();
                UpdateOperationMode();
                UpdatePanelsVisibility();
                UpdateCounters();
            }
            finally { _repaintTimer.Start(); }
        }

        private void InitializeElements()
        {
            try
            {
                InitializeSensor(LT150, "LT150", "Датчик уровня LT150", "LT-150", "%");
                InitializeSensor(LT301, "LT301", "Датчик уровня LT301", "LT-301", "%");
                InitializeSensor(LT303, "LT303", "Датчик уровня LT303", "LT-303", "%");
                InitializeSensor(LT403, "LT403", "Датчик уровня LT403", "LT-403", "%");

                InitializeDiscreteSensor(LAHH101, "LAHH101", "Датчик уровня LAHH101", "LAHH-101");
                InitializeDiscreteSensor(LALL103, "LALL103", "Датчик уровня LALL103", "LALL-103");
                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
                InitializeDiscreteSensor(LAHH301, "LAHH301", "Датчик уровня LAHH301", "LAHH-301");
                InitializeDiscreteSensor(LAHH302, "LAHH302", "Датчик уровня LAHH302", "LAHH-302");
                InitializeDiscreteSensor(LAHH401, "LAHH401", "Датчик уровня LAHH401", "LAHH-401");

                InitializeSensor(TT102, "TT102", "Датчик температуры TT-102", "TT-102", "°C");
                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");
                InitializeSensor(TT302, "TT302", "Датчик температуры TT-302", "TT-302", "°C");
                InitializeSensor(TT402, "TT402", "Датчик температуры TT-402", "TT-402", "°C");
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");

                InitializeSensor(PT104, "PT104", "Датчик давления PT104", "PT-104", "атм");
                InitializeSensor(PT105, "PT105", "Датчик давления PT105", "PT-105", "атм");
                InitializeSensor(PT304, "PT304", "Датчик давления PT304", "PT-304", "атм");
                InitializeSensor(PT404, "PT404", "Датчик давления PT404", "PT-404", "атм");
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "атм");

                InitializeFM(FM401, "FM401", "Расходомер FM401", "FM401", "кг/мин");
                InitializeFM(FM601, "FM601", "Расходомер FM601", "FM601", "кг/мин");

                InitializeQM(QM400, "QM400", "Счетчик QM-400", "QM-400", "л");
                InitializeWIT(WIT100, "WIT100", "Датчик веса", "WIT-100", "кг");

                InitializePumpReverse(P300, "P300", "Насос P-300", "P-300");
                InitializePumpReverse(P400, "P400", "Насос P-400", "P-400");

                InitializePumpUzUnderPanel(P100, "P100", "Насос P-100", "P-100");
                InitializePumpUzUnderPanel(A100, "A100", "Шнек А-100", "A-100");
                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601", "P-601");

                InitializeValveV(VT101, "V101", "Клапан V-101", "V-101");
                InitializeValveH(VT151, "V151", "Клапан V-151", "V-151");
                InitializeValveH(VT152, "V152", "Клапан V-152", "V-152");
                InitializeValveV(VT302, "V302", "Клапан V-302", "V-302");
                InitializeValveV(VT305, "V305", "Клапан V-305", "V-305");
                InitializeValveV(VT401, "V401", "Клапан V-401", "V-401");
                Initialize3ValveH(VT601, "V601", "Клапан SV-601", "SV-601");

                InitializeHeater(HE300, "HE300", "Нагреватель HE-300", "HE-300");
                InitializeHeater(HE750, "HE750", "Нагреватель HE-750", "HE-750");
                InitializeHeater(HE700_1, "HE700.1", "Нагреватель HE-700.1", "HE-700.1");
                InitializeHeater(HE700_2, "HE700.2", "Нагреватель HE-700.2", "HE-700.2");

                if (M100Switch != null) { M100Switch.Tag = "M100"; M100Switch.Global = _global; M100Switch.VarName = "M100"; M100Switch.Description = "Миксер M-100"; M100Switch.StateChanged -= M100Switch_StateChanged; M100Switch.StateChanged += M100Switch_StateChanged; }
                if (M150Switch != null) { M150Switch.Tag = "M150"; M150Switch.Global = _global; M150Switch.VarName = "M150"; M150Switch.Description = "Миксер M-150"; M150Switch.StateChanged -= M150Switch_StateChanged; M150Switch.StateChanged += M150Switch_StateChanged; }
                if (M400Switch != null) { M400Switch.Tag = "M400"; M400Switch.Global = _global; M400Switch.VarName = "M400"; M400Switch.Description = "Миксер M-400"; M400Switch.StateChanged -= M400Switch_StateChanged; M400Switch.StateChanged += M400Switch_StateChanged; }
                InitializeMixerHotspot(M100MixerHotspot, "M100", "Миксер M-100");
                InitializeMixerHotspot(M150MixerHotspot, "M150", "Миксер M-150");
                InitializeMixerHotspot(M400MixerHotspot, "M400", "Миксер M-400");

                if (HE300Switch != null) { HE300Switch.Tag = "HE300"; HE300Switch.StateChanged += HE300Switch_StateChanged; }
                if (HE750Switch != null) { HE750Switch.Tag = "HE750"; HE750Switch.StateChanged += HE750Switch_StateChanged; }

                if (GroModePanel != null) GroModePanel.ModeChanged += GroModePanel_ModeChanged;

                var tag = _global.Variables.GetByName("T100_MassSp");
                if (tag != null) T100MassSpEdit.Text = tag.ValueString;

                tag = _global.Variables.GetByName("GRO_ManualSelitraCounterSp");
                if (tag != null) SelitraSpEdit.Text = tag.ValueString;

                tag = _global.Variables.GetByName("GRO_ManualWaterCounterSp");
                if (tag != null) WaterSpEdit.Text = tag.ValueString;

                tag = _global.Variables.GetByName("GRO_ManualKislotaCounterSp");
                if (tag != null) KislotaSpEdit.Text = tag.ValueString;

                tag = _global.Variables.GetByName("A100_Speed");
                if (tag != null) A100SpeedSpEdit.Text = tag.ValueString;

                tag = _global.Variables.GetByName("HE700_Rejim");
                if (tag != null && tag.ValueReal < 4) HE700ComboBox.SelectedIndex = (int)tag.ValueReal;
            }
            catch { }
        }

        private void InitializeSensor(Element_AI sensor, string varName, string description,
                                      string tagName, string eu)
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

        private void InitializeQM(Element_QM sensor, string varName, string description,
                                 string tagName, string eu)
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

        private void InitializeWIT(Element_WIT sensor, string varName, string description,
                                  string tagName, string eu)
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

        private void InitializeFM(Element_FM sensor, string varName, string description,
                                 string tagName, string eu)
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

        private void InitializeDiscreteSensor(Element_DI sensor, string varName,
                                             string description, string tagName)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;
            }
        }

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump,
                                                string varName, string description,
                                                string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;
                pump.UpdateElement();
            }
        }

        private void InitializePumpReverse(Element_PumpHReverse pump,
                                          string varName, string description,
                                          string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;
                pump.UpdateElement();
            }
        }

        private void InitializeValveV(Element_ValveV valve,
                                      string varName, string description,
                                      string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void InitializeValveH(Element_ValveH valve,
                                      string varName, string description,
                                      string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void Initialize3ValveH(Element_3ValveH valve,
                                      string varName, string description,
                                      string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void InitializeHeater(Element_Heater heater,
                                      string varName, string description,
                                      string tagName)
        {
            if (heater != null && _global != null)
            {
                heater.Global = _global;
                heater.VarName = varName;
                heater.Description = description;
                heater.TagName = tagName;
            }
        }

        private void InitializeMixerHotspot(Element_MixerHotspot hotspot, string varName, string description)
        {
            if (hotspot == null || _global == null) return;
            hotspot.Global = _global;
            hotspot.VarName = varName;
            hotspot.Description = description;
        }

        private void UpdateAllElements()
        {
            LT150?.UpdateElement(); LT301?.UpdateElement(); LT303?.UpdateElement(); LT403?.UpdateElement();
            LAHH101?.UpdateElement(); LALL103?.UpdateElement(); LAHH151?.UpdateElement(); LALL153?.UpdateElement();
            LAHH301?.UpdateElement(); LAHH302?.UpdateElement(); LAHH401?.UpdateElement();

            TT102?.UpdateElement(); TT152?.UpdateElement(); TT302?.UpdateElement(); TT402?.UpdateElement(); TT602?.UpdateElement();

            PT104?.UpdateElement(); PT105?.UpdateElement(); PT304?.UpdateElement(); PT404?.UpdateElement(); PT601?.UpdateElement();

            FM401?.UpdateElement(); FM601?.UpdateElement();

            QM400?.UpdateElement(); WIT100?.UpdateElement();

            P300?.UpdateElement(); P400?.UpdateElement();
            P100?.UpdateElement(); A100?.UpdateElement(); P601?.UpdateElement();

            VT101?.UpdateElement(); VT151?.UpdateElement(); VT152?.UpdateElement(); VT302?.UpdateElement();
            VT305?.UpdateElement(); VT401?.UpdateElement(); VT601?.UpdateElement();

            HE300?.UpdateElement(); HE750?.UpdateElement(); HE700_1?.UpdateElement(); HE700_2?.UpdateElement();

            UpdateToggleSwitchesFromVariables();
            UpdateLiquidGauges();
        }

        private void UpdateLiquidGauges()
        {
            if (_global?.Variables == null) return;
            GaugeT150.FillLevel = ReadLevelPercent("LT150_Value");
            GaugeT301.FillLevel = ReadLevelPercent("LT301_Value");
            GaugeT303.FillLevel = ReadLevelPercent("LT303_Value");
            GaugeT403.FillLevel = ReadLevelPercent("LT403_Value");
            GaugeT100.FillLevel = ReadLevelPercent("WIT100_Volume");
        }

        private double ReadLevelPercent(string variableName)
        {
            var tag = _global?.Variables?.GetByName(variableName);
            if (tag == null) return 0;
            return Math.Max(0, Math.Min(100, tag.ValueReal));
        }

        private void UpdateToggleSwitchesFromVariables()
        {
            var t = _global?.Variables?.GetByName("M100_IsWork");
            if (t != null && M100Switch != null) { M100Switch.IsChecked = t.ValueReal > 0; M100Switch.UpdateElement(); }

            t = _global?.Variables?.GetByName("M150_IsWork");
            if (t != null && M150Switch != null) { M150Switch.IsChecked = t.ValueReal > 0; M150Switch.UpdateElement(); }

            t = _global?.Variables?.GetByName("M400_IsWork");
            if (t != null && M400Switch != null) { M400Switch.IsChecked = t.ValueReal > 0; M400Switch.UpdateElement(); }

            t = _global?.Variables?.GetByName("HE300_IsOn");
            if (t != null && HE300Switch != null) HE300Switch.IsChecked = t.ValueReal > 0;

            t = _global?.Variables?.GetByName("HE750_IsOn");
            if (t != null && HE750Switch != null) HE750Switch.IsChecked = t.ValueReal > 0;
        }

        //private void ResetCommands()
        //{
        //    if (_global?.Commands == null) return;

        //    void SetFalse(string name)
        //    {
        //        var c = _global.Commands.GetByName(name);
        //        if (c != null && !c.NeedToWrite) c.WriteValue = "false";
        //    }

        //    SetFalse("M100_StartMixer");
        //    SetFalse("M150_StartMixer");
        //    SetFalse("M400_StartMixer");
        //    SetFalse("HE300_IsOn");
        //    SetFalse("HE750_IsOn");
        //    SetFalse("GRO_Manual_Selitra_Start");
        //    SetFalse("GRO_Manual_Water_Start");
        //    SetFalse("GRO_Manual_Kislota_Start");
        //    SetFalse("GRO_Manual_Stop");
        //    SetFalse("GRO_Manual_Pause");
        //    SetFalse("GRO_TransportStart");
        //    SetFalse("GRO_TransportStop");
        //    SetFalse("T100_MassSp");
        //}

        private void UpdateOperationMode()
        {
            var tag = _global?.Variables?.GetByName("GRO_Rejim");
            if (tag == null || GroModePanel == null) return;

            int mode = (int)tag.ValueReal;
            OperationMode op = mode switch
            {
                0 => OperationMode.Off,
                1 or 3 or 4 or 5 or 6 or 7 or 8 => OperationMode.SemiAuto,
                2 or 9 or 10 or 11 or 12 or 13 or 14 => OperationMode.Auto,
                _ => OperationMode.Off
            };

            if (GroModePanel.CurrentMode != op) GroModePanel.SetMode(op);
        }

        private void HideAllPanels()
        {
            if (SubstancePanel != null) SubstancePanel.Visibility = Visibility.Collapsed;
            if (TransportPanel != null) TransportPanel.Visibility = Visibility.Collapsed;
            if (A100SpeedPanel != null) A100SpeedPanel.Visibility = Visibility.Collapsed;
            if (HE700Panel != null) HE700Panel.Visibility = Visibility.Collapsed;
            if (T100MassPanel != null) T100MassPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdatePanelsVisibility()
        {
            if (GroModePanel != null) GroModePanel.Visibility = Visibility.Visible;
            HideAllPanels();

            var modeTag = _global?.Variables?.GetByName("GRO_Rejim");
            if (modeTag == null) return;

            int mode = (int)modeTag.ValueReal;

            if (mode == 0) return;

            // Перекачка (Т200 → Т250) должна быть видна в режимах 1, 2, 15, 16
            if (mode == 15 || mode == 16 || mode == 1 || mode == 2)
            {
                if (TransportPanel != null) TransportPanel.Visibility = Visibility.Visible;
            }

            // Вручную задаём вещества – только в режиме 1
            if (mode == 1 && SubstancePanel != null)
                SubstancePanel.Visibility = Visibility.Visible;

            // Остальные рабочие режимы (1‑14) показывают общие панели
            if (mode >= 1 && mode <= 14)
            {
                if (A100SpeedPanel != null) A100SpeedPanel.Visibility = Visibility.Visible;
                if (HE700Panel != null) HE700Panel.Visibility = Visibility.Visible;
                if (T100MassPanel != null) T100MassPanel.Visibility = Visibility.Visible;
            }
        }

        private void UpdateCounters()
        {
            var t = _global.Variables.GetByName("GRO_ManualSelitraCounter");
            if (t != null && GRO_ManualSelitraCounterEdit != null) GRO_ManualSelitraCounterEdit.Text = t.ValueString;

            t = _global.Variables.GetByName("GRO_ManualWaterCounter");
            if (t != null && GRO_ManualWaterCounterEdit != null) GRO_ManualWaterCounterEdit.Text = t.ValueString;

            t = _global.Variables.GetByName("GRO_ManualKislotaCounter");
            if (t != null && GRO_ManualKislotaCounterEdit != null) GRO_ManualKislotaCounterEdit.Text = t.ValueString;
        }

        private void M100Switch_StateChanged(object sender, bool isChecked) => SendSimple("M100_StartMixer", isChecked);
        private void M150Switch_StateChanged(object sender, bool isChecked) => SendSimple("M150_StartMixer", isChecked);
        private void M400Switch_StateChanged(object sender, bool isChecked) => SendSimple("M400_StartMixer", isChecked);
        private void HE300Switch_StateChanged(object sender, bool isChecked) => SendSimple("HE300_IsOn", isChecked);
        private void HE750Switch_StateChanged(object sender, bool isChecked) => SendSimple("HE750_IsOn", isChecked);

        private void SendSimple(string name, bool enable)
        {
            var cmd = _global?.Commands?.GetByName(name);
            if (cmd != null)
            {
                cmd.WriteValue = enable ? "true" : "false";
                cmd.NeedToWrite = true;
            }
        }

        private void GroModePanel_ModeChanged(object sender, OperationMode mode) => SendGroModeCommand(mode);
        private void SendGroModeCommand(OperationMode mode)
        {
            string cmdName = mode switch
            {
                OperationMode.Off => "GRO_RejimToOff",
                OperationMode.SemiAuto => "GRO_RejimToManual",
                OperationMode.Auto => "GRO_RejimToAuto",
                _ => "GRO_RejimToOff"
            };
            var cmd = _global?.Commands?.GetByName(cmdName);
            if (cmd != null) { cmd.WriteValue = "true"; cmd.NeedToWrite = true; }
        }

        private void GroModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            // Открытый канал для отправки Modbus‑команд из ModePanel,
            // но в текущей реализации мы просто делаем то же самое,
            // что и в SendGroModeCommand.
            var mode = (OperationMode)e.Value;
            SendGroModeCommand(mode);
        }

        private void SetT100MassButton_Click(object sender, RoutedEventArgs e)
        {
            var cmd = _global?.Commands?.GetByName("T100_MassSp");
            if (cmd != null && T100MassSpEdit != null)
            {
                cmd.WriteValue = T100MassSpEdit.Text;
                cmd.NeedToWrite = true;
            }
        }

        private void A100SpeedSpEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_global != null && !string.IsNullOrEmpty(A100SpeedSpEdit.Text))
            {
                var cmd = _global.Commands.GetByName("A100_Speed");
                if (cmd != null)
                {
                    cmd.WriteValue = A100SpeedSpEdit.Text;
                    cmd.NeedToWrite = true;
                }
            }
        }

        private void SubstanceStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelitraRadio.IsChecked == true) SetSpAndStart("GRO_ManualSelitraCounterSp", "GRO_Manual_Selitra_Start", SelitraSpEdit.Text);
            else if (WaterRadio.IsChecked == true) SetSpAndStart("GRO_ManualWaterCounterSp", "GRO_Manual_Water_Start", WaterSpEdit.Text);
            else if (KislotaRadio.IsChecked == true) SetSpAndStart("GRO_ManualKislotaCounterSp", "GRO_Manual_Kislota_Start", KislotaSpEdit.Text);
        }

        private void SetSpAndStart(string spCmd, string startCmd, string value)
        {
            var sp = _global.Commands.GetByName(spCmd);
            if (sp != null) { sp.WriteValue = value; sp.NeedToWrite = true; }
            var start = _global.Commands.GetByName(startCmd);
            if (start != null) { start.WriteValue = "true"; start.NeedToWrite = true; }
        }

        private void SubstanceStopButton_Click(object sender, RoutedEventArgs e) => ExecuteSimple("GRO_Manual_Stop");
        private void SubstancePauseButton_Click(object sender, RoutedEventArgs e) => ExecuteSimple("GRO_Manual_Pause");
        private void TransportStartButton_Click(object sender, RoutedEventArgs e) => ExecuteSimple("GRO_TransportStart");
        private void TransportStopButton_Click(object sender, RoutedEventArgs e) => ExecuteSimple("GRO_TransportStop");

        private void SaveHE700RejimButton_Click(object sender, RoutedEventArgs e)
        {
            var cmd = _global.Commands.GetByName("HE700_Rejim");
            if (cmd != null && HE700ComboBox.SelectedIndex >= 0)
            {
                cmd.WriteValue = HE700ComboBox.SelectedIndex.ToString();
                cmd.NeedToWrite = true;
            }
        }

        private void ExecuteSimple(string commandName)
        {
            var cmd = _global?.Commands?.GetByName(commandName);
            if (cmd != null) { cmd.WriteValue = "true"; cmd.NeedToWrite = true; }
        }

        private void Cleanup()
        {
            _repaintTimer?.Stop();
            if (_global != null) _global.OnVariablesUpdated -= Global_OnVariablesUpdated;
            if (GroModePanel != null)
            {
                GroModePanel.ModeChanged -= GroModePanel_ModeChanged;
                GroModePanel.ModbusCommandRequested -= GroModePanel_ModbusCommandRequested;
            }
        }
    }
}

using System;
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

            // Инициализация всех элементов
            InitializeElements();

            // Настройка таймера обновления (10 Гц как в старом проекте)
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;

            UpdatePanelsVisibility();

            // Подписка на события
            SubscribeToEvents();

            System.Diagnostics.Debug.WriteLine("FrameGroPage инициализирован");
        }

        private void FrameGroPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePanelsVisibility();
            // Запуск таймера после загрузки
            _repaintTimer?.Start();
        }

        private void FrameGroPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void InitializeElements()
        {
            try
            {
                // Датчики уровня
                InitializeSensor(LT150, "LT150", "Датчик уровня LT150", "LT-150", "%");
                InitializeSensor(LT301, "LT301", "Датчик уровня LT301", "LT-301", "%");
                InitializeSensor(LT303, "LT303", "Датчик уровня LT303", "LT-303", "%");
                InitializeSensor(LT403, "LT403", "Датчик уровня LT403", "LT-403", "%");

                // Сигнализаторы уровня
                InitializeDiscreteSensor(LAHH101, "LAHH101", "Датчик уровня LAHH101", "LAHH-101");
                InitializeDiscreteSensor(LALL103, "LALL103", "Датчик уровня LALL103", "LALL-103");
                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
                InitializeDiscreteSensor(LAHH301, "LAHH301", "Датчик уровня LAHH301", "LAHH-301");
                InitializeDiscreteSensor(LAHH302, "LAHH302", "Датчик уровня LAHH302", "LAHH-302");
                InitializeDiscreteSensor(LAHH401, "LAHH401", "Датчик уровня LAHH401", "LAHH-401");

                // Датчики температуры
                InitializeSensor(TT102, "TT102", "Датчик температуры TT-102", "TT-102", "°C");
                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");
                InitializeSensor(TT302, "TT302", "Датчик температуры TT-302", "TT-302", "°C");
                InitializeSensor(TT402, "TT402", "Датчик температуры TT-402", "TT-402", "°C");
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");

                // Датчики давления
                InitializeSensor(PT104, "PT104", "Датчик давления PT104", "PT-104", "атм");
                InitializeSensor(PT105, "PT105", "Датчик давления PT105", "PT-105", "атм");
                InitializeSensor(PT304, "PT304", "Датчик давления PT304", "PT-304", "атм");
                InitializeSensor(PT404, "PT404", "Датчик давления PT404", "PT-404", "атм");
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "атм");

                // Расходомеры
                InitializeFM(FM401, "FM401", "Расходомер FM401", "FM401", "кг/мин");
                InitializeFM(FM601, "FM601", "Расходомер FM601", "FM601", "кг/мин");

                // Счетчик QM400
                InitializeQM(QM400, "QM400", "Счетчик QM-400", "QM-400", "л");

                // Весовой датчик WIT100
                InitializeWIT(WIT100, "WIT100", "Вес Т-100", "WIT-100", "кг");

                // Насосы обратные (P300, P400)
                InitializePumpReverse(P300, "P300", "Насос P-300", "P-300");
                InitializePumpReverse(P400, "P400", "Насос P-400", "P-400");

                // Насосы обычные
                InitializePumpUzUnderPanel(P100, "P100", "Насос P-100", "P-100");
                InitializePumpUzUnderPanel(A100, "A100", "Шнек А-100", "A-100");
                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601", "P-601");

                // Задвижки
                InitializeValveV(VT101, "V101", "Клапан V-101", "V-101");
                InitializeValveH(VT151, "V151", "Клапан V-151", "V-151");
                InitializeValveH(VT152, "V152", "Клапан V-152", "V-152");
                InitializeValveV(VT302, "V302", "Клапан V-302", "V-302");
                InitializeValveV(VT305, "V305", "Клапан V-305", "V-305");
                InitializeValveV(VT401, "V401", "Клапан V-401", "V-401");
                Initialize3ValveH(VT601, "V601", "Клапан SV-601", "SV-601");

                // Нагреватели
                InitializeHeater(HE300, "HE300", "Нагреватель HE-300", "HE-300");
                InitializeHeater(HE750, "HE750", "Нагреватель HE-750", "HE-750");
                InitializeHeater(HE700_1, "HE700.1", "Нагреватель HE-700.1", "HE-700.1");
                InitializeHeater(HE700_2, "HE700.2", "Нагреватель HE-700.2", "HE-700.2");

                // Переключатели миксеров
                if (M100Switch != null)
                {
                    M100Switch.Tag = "M100";
                    M100Switch.StateChanged += M100Switch_StateChanged;
                }

                if (M150Switch != null)
                {
                    M150Switch.Tag = "M150";
                    M150Switch.StateChanged += M150Switch_StateChanged;
                }

                if (M400Switch != null)
                {
                    M400Switch.Tag = "M400";
                    M400Switch.StateChanged += M400Switch_StateChanged;
                }

                // Переключатели нагревателей
                if (HE300Switch != null)
                {
                    HE300Switch.Tag = "HE300";
                    HE300Switch.StateChanged += HE300Switch_StateChanged;
                }

                if (HE750Switch != null)
                {
                    HE750Switch.Tag = "HE750";
                    HE750Switch.StateChanged += HE750Switch_StateChanged;
                }

                // Панель режима
                if (GroModePanel != null)
                {
                    GroModePanel.ModeChanged += GroModePanel_ModeChanged;
                }

                // Инициализация уставки массы T-100
                var tag = _global.Variables.GetByName("T100_MassSp");
                if (tag != null)
                {
                    T100MassSpEdit.Text = tag.ValueString;
                }

                // Инициализация уставок вещества
                tag = _global.Variables.GetByName("GRO_ManualSelitraCounterSp");
                if (tag != null)
                {
                    SelitraSpEdit.Text = tag.ValueString;
                }

                tag = _global.Variables.GetByName("GRO_ManualWaterCounterSp");
                if (tag != null)
                {
                    WaterSpEdit.Text = tag.ValueString;
                }

                tag = _global.Variables.GetByName("GRO_ManualKislotaCounterSp");
                if (tag != null)
                {
                    KislotaSpEdit.Text = tag.ValueString;
                }

                // Инициализация скорости A100
                tag = _global.Variables.GetByName("A100_Speed");
                if (tag != null)
                {
                    A100SpeedSpEdit.Text = tag.ValueString;
                }

                // Инициализация режима HE-700
                tag = _global.Variables.GetByName("HE700_Rejim");
                if (tag != null && tag.ValueReal < 4)
                {
                    HE700ComboBox.SelectedIndex = (int)tag.ValueReal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов FrameGroPage: {ex.Message}");
            }
        }

        private void SubscribeToEvents()
        {
            if (_global != null)
            {
                _global.OnVariablesUpdated += Global_OnVariablesUpdated;
            }
        }

        private void Global_OnVariablesUpdated(object sender, EventArgs e)
        {
            // Обновляем все элементы при изменении переменных Modbus
            UpdateAllElements();
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            // Останавливаем таймер на время обновления (как в старом проекте)
            _repaintTimer.Stop();

            try
            {
                // 1. Обновление всех элементов
                UpdateAllElements();

                // 2. Сброс команд (как в старом проекте)
                ResetCommands();

                // 3. Обновление режимов работы
                UpdateOperationMode();

                // 4. Обновление видимости панелей
                UpdatePanelsVisibility();

                // 5. Обновление счетчиков
                UpdateCounters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в RepaintTimer_Tick: {ex.Message}");
            }
            finally
            {
                // Запускаем таймер снова (как в старом проекте)
                _repaintTimer.Start();
            }
        }

        private void UpdateAllElements()
        {
            // Датчики уровня
            LT150?.UpdateElement();
            LT301?.UpdateElement();
            LT303?.UpdateElement();
            LT403?.UpdateElement();

            // Сигнализаторы
            LAHH101?.UpdateElement();
            LALL103?.UpdateElement();
            LAHH151?.UpdateElement();
            LALL153?.UpdateElement();
            LAHH301?.UpdateElement();
            LAHH302?.UpdateElement();
            LAHH401?.UpdateElement();

            // Датчики температуры
            TT102?.UpdateElement();
            TT152?.UpdateElement();
            TT302?.UpdateElement();
            TT402?.UpdateElement();
            TT602?.UpdateElement();

            // Датчики давления
            PT104?.UpdateElement();
            PT105?.UpdateElement();
            PT304?.UpdateElement();
            PT404?.UpdateElement();
            PT601?.UpdateElement();

            // Расходомеры
            FM401?.UpdateElement();
            FM601?.UpdateElement();

            // Счетчик QM400
            QM400?.UpdateElement();

            // Весовой датчик
            WIT100?.UpdateElement();

            // Насосы обратные
            P300?.UpdateElement();
            P400?.UpdateElement();

            // Насосы обычные
            P100?.UpdateElement();
            A100?.UpdateElement();
            P601?.UpdateElement();

            // Задвижки
            VT101?.UpdateElement();
            VT151?.UpdateElement();
            VT152?.UpdateElement();
            VT302?.UpdateElement();
            VT305?.UpdateElement();
            VT401?.UpdateElement();
            VT601?.UpdateElement();

            // Нагреватели
            HE300?.UpdateElement();
            HE750?.UpdateElement();
            HE700_1?.UpdateElement();
            HE700_2?.UpdateElement();

            // Обновление состояния переключателей из переменных
            UpdateToggleSwitchesFromVariables();
        }

        private void UpdateToggleSwitchesFromVariables()
        {
            try
            {
                // Миксер M100
                var m100Tag = _global?.Variables?.GetByName("M100_IsWork");
                if (m100Tag != null && M100Switch != null)
                {
                    bool isWorking = m100Tag.ValueReal > 0;
                    if (M100Switch.IsChecked != isWorking)
                    {
                        M100Switch.IsChecked = isWorking;
                    }
                }

                // Миксер M150
                var m150Tag = _global?.Variables?.GetByName("M150_IsWork");
                if (m150Tag != null && M150Switch != null)
                {
                    bool isWorking = m150Tag.ValueReal > 0;
                    if (M150Switch.IsChecked != isWorking)
                    {
                        M150Switch.IsChecked = isWorking;
                    }
                }

                // Миксер M400
                var m400Tag = _global?.Variables?.GetByName("M400_IsWork");
                if (m400Tag != null && M400Switch != null)
                {
                    bool isWorking = m400Tag.ValueReal > 0;
                    if (M400Switch.IsChecked != isWorking)
                    {
                        M400Switch.IsChecked = isWorking;
                    }
                }

                // Нагреватель HE300
                var he300Tag = _global?.Variables?.GetByName("HE300_IsOn");
                if (he300Tag != null && HE300Switch != null)
                {
                    bool isOn = he300Tag.ValueReal > 0;
                    if (HE300Switch.IsChecked != isOn)
                    {
                        HE300Switch.IsChecked = isOn;
                    }
                }

                // Нагреватель HE750
                var he750Tag = _global?.Variables?.GetByName("HE750_IsOn");
                if (he750Tag != null && HE750Switch != null)
                {
                    bool isOn = he750Tag.ValueReal > 0;
                    if (HE750Switch.IsChecked != isOn)
                    {
                        HE750Switch.IsChecked = isOn;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления переключателей: {ex.Message}");
            }
        }

        private void ResetCommands()
        {
            try
            {
                if (_global?.Commands == null) return;

                TCommandTag command;

                // Сброс команд миксеров
                command = _global.Commands.GetByName("M100_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("M150_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("M400_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд нагревателей
                command = _global.Commands.GetByName("HE300_IsOn");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("HE750_IsOn");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд подачи вещества
                command = _global.Commands.GetByName("GRO_Manual_Selitra_Start");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("GRO_Manual_Water_Start");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("GRO_Manual_Kislota_Start");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("GRO_Manual_Stop");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("GRO_Manual_Pause");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд транспортировки
                command = _global.Commands.GetByName("GRO_TransportStart");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("GRO_TransportStop");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды задания массы Т-100
                command = _global.Commands.GetByName("T100_MassSp");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса команд: {ex.Message}");
            }
        }
        private void UpdateOperationMode()
        {
            var tag = _global.Variables.GetByName("GRO_Rejim");
            if (tag == null || GroModePanel == null) return;

            int mode = (int)tag.ValueReal;

            OperationMode currentOperationMode = mode switch
            {
                OperationMode currentOperationMode = mode switch
                {
                    0 => OperationMode.Off,
                    1 or 3 or 4 or 5 or 6 or 7 or 8 => OperationMode.SemiAuto,
                    2 or 9 or 10 or 11 or 12 or 13 or 14 => OperationMode.Auto,
                    15 or 16 => OperationMode.Off,
                    _ => OperationMode.Off
                };

            if (GroModePanel.CurrentMode != currentOperationMode)
            {
                GroModePanel.SetMode(currentOperationMode);
            }
        }

        private void UpdatePanelsVisibility()
        {
            try
            {
                var rejimTag = _global?.Variables?.GetByName("GRO_Rejim");
                // ModePanel всегда виден
                if (GroModePanel != null)
                    GroModePanel.Visibility = Visibility.Visible;

                if (rejimTag == null)
                {
                    if (SubstancePanel != null)
                        SubstancePanel.Visibility = Visibility.Collapsed;

                    if (TransportPanel != null)
                        TransportPanel.Visibility = Visibility.Collapsed;

                    if (A100SpeedPanel != null)
                        A100SpeedPanel.Visibility = Visibility.Collapsed;

                    if (HE700Panel != null)
                        HE700Panel.Visibility = Visibility.Collapsed;

                    if (T100MassPanel != null)
                        T100MassPanel.Visibility = Visibility.Collapsed;

                    return;
                }

                int mode = (int)rejimTag.ValueReal;

                // Управление видимостью панелей
                bool isOff = mode == 0 || mode == 15 || mode == 16;

                // Панель вещества - скрываем в режиме OFF
                if (SubstancePanel != null)
                    SubstancePanel.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                // Панель транспортировки - скрываем в режиме OFF
                if (TransportPanel != null)
                    TransportPanel.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                // Эти панели также скрываем в режиме OFF
                if (A100SpeedPanel != null)
                    A100SpeedPanel.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                if (HE700Panel != null)
                    HE700Panel.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                if (T100MassPanel != null)
                    T100MassPanel.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                System.Diagnostics.Debug.WriteLine($"GRO режим: {mode}, панели видимы: {!isOff}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления видимости панелей GRO: {ex.Message}");
            }
        }

        private void UpdateCounters()
        {
            try
            {
                // Обновление счетчиков в панели
                var tag = _global.Variables.GetByName("GRO_ManualSelitraCounter");
                if (tag != null && GRO_ManualSelitraCounterEdit != null)
                {
                    GRO_ManualSelitraCounterEdit.Text = tag.ValueString;
                }

                tag = _global.Variables.GetByName("GRO_ManualWaterCounter");
                if (tag != null && GRO_ManualWaterCounterEdit != null)
                {
                    GRO_ManualWaterCounterEdit.Text = tag.ValueString;
                }

                tag = _global.Variables.GetByName("GRO_ManualKislotaCounter");
                if (tag != null && GRO_ManualKislotaCounterEdit != null)
                {
                    GRO_ManualKislotaCounterEdit.Text = tag.ValueString;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления счетчиков: {ex.Message}");
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

        private void InitializeQM(Element_QM sensor, string varName, string description, string tagName, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;
                sensor.EU = eu;
                sensor.Designation = description; // Если нужно заполнить Designation
            }
        }

        private void InitializeWIT(Element_WIT sensor, string varName, string description, string tagName, string eu)
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

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump, string varName, string description, string tagName)
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

        private void InitializePumpReverse(Element_PumpHReverse pump, string varName, string description, string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;
                pump.UpdateElement(); ;
            }
        }

        private void InitializeValveV(Element_ValveV valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void InitializeValveH(Element_ValveH valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void Initialize3ValveH(Element_3ValveH valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;
            }
        }

        private void InitializeHeater(Element_Heater heater, string varName, string description, string tagName)
        {
            if (heater != null && _global != null)
            {
                heater.Global = _global;
                heater.VarName = varName;
                heater.Description = description;
                heater.TagName = tagName;
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

        private void M100Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M100", 1);
                TCommandTag command = _global.Commands.GetByName("M100_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M100", 1);
                TCommandTag command = _global.Commands.GetByName("M100_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void M150Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M150", 1);
                TCommandTag command = _global.Commands.GetByName("M150_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M150", 1);
                TCommandTag command = _global.Commands.GetByName("M150_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void M400Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M400", 1);
                TCommandTag command = _global.Commands.GetByName("M400_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M400", 1);
                TCommandTag command = _global.Commands.GetByName("M400_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void HE300Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE-300", 1);
                TCommandTag command = _global.Commands.GetByName("HE300_IsOn");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE-300", 1);
                TCommandTag command = _global.Commands.GetByName("HE300_IsOn");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void HE750Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE-750", 1);
                TCommandTag command = _global.Commands.GetByName("HE750_IsOn");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE-750", 1);
                TCommandTag command = _global.Commands.GetByName("HE750_IsOn");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void GroModePanel_ModeChanged(object sender, OperationMode mode)
        {
            SendGroModeCommand(mode, logUserAction: true);

            // Мгновенно отражаем выбор пользователя, затем ПЛК подтвердит фактическим режимом
            GroModePanel?.SetMode(mode);
        }

        private void SendGroModeCommand(OperationMode mode, bool logUserAction)
        {
            if (_global == null) return;

            try
            {
                string commandName = mode switch
                {
                    OperationMode.Off => "GRO_RejimToOff",
                    OperationMode.SemiAuto => "GRO_RejimToManual",
                    OperationMode.Auto => "GRO_RejimToAuto",
                    _ => "GRO_RejimToOff"
                };

                TCommandTag command = _global.Commands.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", $"Переход в режим {mode}", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка изменения режима GRO: {ex.Message}");
            }
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ==========

        private void SetT100MassButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                TCommandTag command = _global.Commands.GetByName("T100_MassSp");
                if (command != null && T100MassSpEdit != null)
                {
                    command.WriteValue = T100MassSpEdit.Text;
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Задание массы в Т-100", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка задания массы Т-100: {ex.Message}");
            }
        }

        private void A100SpeedSpEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматическая отправка при изменении текста
            if (_global != null && !string.IsNullOrEmpty(A100SpeedSpEdit.Text))
            {
                try
                {
                    TCommandTag command = _global.Commands.GetByName("A100_Speed");
                    if (command != null)
                    {
                        command.WriteValue = A100SpeedSpEdit.Text;
                        command.NeedToWrite = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка задания скорости A100: {ex.Message}");
                }
            }
        }

        private void SubstanceStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                // Сначала устанавливаем уставки
                if (SelitraRadio.IsChecked == true)
                {
                    TCommandTag spCommand = _global.Commands.GetByName("GRO_ManualSelitraCounterSp");
                    if (spCommand != null && SelitraSpEdit != null)
                    {
                        spCommand.WriteValue = SelitraSpEdit.Text;
                        spCommand.NeedToWrite = true;
                    }

                    TCommandTag startCommand = _global.Commands.GetByName("GRO_Manual_Selitra_Start");
                    if (startCommand != null)
                    {
                        startCommand.WriteValue = "true";
                        startCommand.NeedToWrite = true;
                    }
                    _global.Log.Add("Пользователь", "Пуск селитры", 1);
                }
                else if (WaterRadio.IsChecked == true)
                {
                    TCommandTag spCommand = _global.Commands.GetByName("GRO_ManualWaterCounterSp");
                    if (spCommand != null && WaterSpEdit != null)
                    {
                        spCommand.WriteValue = WaterSpEdit.Text;
                        spCommand.NeedToWrite = true;
                    }

                    TCommandTag startCommand = _global.Commands.GetByName("GRO_Manual_Water_Start");
                    if (startCommand != null)
                    {
                        startCommand.WriteValue = "true";
                        startCommand.NeedToWrite = true;
                    }
                    _global.Log.Add("Пользователь", "Пуск воды", 1);
                }
                else if (KislotaRadio.IsChecked == true)
                {
                    TCommandTag spCommand = _global.Commands.GetByName("GRO_ManualKislotaCounterSp");
                    if (spCommand != null && KislotaSpEdit != null)
                    {
                        spCommand.WriteValue = KislotaSpEdit.Text;
                        spCommand.NeedToWrite = true;
                    }

                    TCommandTag startCommand = _global.Commands.GetByName("GRO_Manual_Kislota_Start");
                    if (startCommand != null)
                    {
                        startCommand.WriteValue = "true";
                        startCommand.NeedToWrite = true;
                    }
                    _global.Log.Add("Пользователь", "Пуск кислоты", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка пуска вещества: {ex.Message}");
            }
        }

        private void SubstanceStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                TCommandTag command = _global.Commands.GetByName("GRO_Manual_Stop");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Остановка подачи вещества", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки вещества: {ex.Message}");
            }
        }

        private void SubstancePauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                TCommandTag command = _global.Commands.GetByName("GRO_Manual_Pause");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Пауза подачи вещества", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка паузы вещества: {ex.Message}");
            }
        }

        private void TransportStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                TCommandTag command = _global.Commands.GetByName("GRO_TransportStart");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Пуск транспортировки", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка пуска транспортировки: {ex.Message}");
            }
        }

        private void TransportStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                TCommandTag command = _global.Commands.GetByName("GRO_TransportStop");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Остановка транспортировки", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки транспортировки: {ex.Message}");
            }
        }

        private void SaveHE700RejimButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                if (HE700ComboBox.SelectedIndex >= 0)
                {
                    TCommandTag command = _global.Commands.GetByName("HE700_Rejim");
                    if (command != null)
                    {
                        command.WriteValue = HE700ComboBox.SelectedIndex.ToString();
                        command.NeedToWrite = true;
                        _global.Log.Add("Пользователь", "Смена режима HE-700", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка смены режима HE-700: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            _repaintTimer?.Stop();

            if (_global != null)
            {
                _global.OnVariablesUpdated -= Global_OnVariablesUpdated;
            }
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameTcPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        public FrameTcPage()
        {
            InitializeComponent();
            Loaded += FrameTcPage_Loaded;
            Unloaded += FrameTcPage_Unloaded;
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

            // Подписка на события
            SubscribeToEvents();

            // Первоначальное обновление видимости панелей
            UpdatePanelsVisibility();

            System.Diagnostics.Debug.WriteLine("FrameTcPage инициализирован");
        }

        private void FrameTcPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Запуск таймера после загрузки
            _repaintTimer?.Start();

            // Обновление видимости панелей при загрузке
            UpdatePanelsVisibility();
        }

        private void FrameTcPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void InitializeElements()
        {
            try
            {
                // ========== АНАЛОГОВЫЕ ДАТЧИКИ ==========
                // PT205 - Датчик протока PT205
                InitializeSensor(PT205, "PT205", "Датчик протока PT205", "PT205", "л/ч");

                // PT206 - Датчик протока PT206
                InitializeSensor(PT206, "PT206", "Датчик протока PT206", "PT206", "л/ч");

                // PT201 - Датчик давления PT201
                InitializeSensor(PT201, "PT201", "Датчик давления PT201", "PT201", "атм");

                // LT253 - Датчик уровня LT253
                InitializeSensor(LT253, "LT253", "Датчик уровня LT253", "LT253", "%");

                // TT202 - Датчик температуры TT202
                InitializeSensor(TT202, "TT202", "Датчик температуры TT202", "TT202", "°C");

                // TT252 - Датчик температуры TT252
                InitializeSensor(TT252, "TT252", "Датчик температуры TT252", "TT252", "°C");

                // FM602 - Массовый расходомер FM602
                InitializeFM(FM602, "FM602", "Расходомер FM602", "FM602", "кг/мин");

                // WIT200 - Датчик веса WIT200
                InitializeWIT(WIT200, "WIT200_Volume", "Датчик веса WIT200", "WIT200", "кг");

                // ========== ДИСКРЕТНЫЕ ДАТЧИКИ ==========
                // LAHH201 - Датчик уровня LAHH201
                InitializeDiscreteSensor(LAHH201, "LAHH201", "Датчик уровня LAHH201", "LAHH-201");

                // LAHH251 - Датчик уровня LAHH251
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");

                // ========== НАСОСЫ ==========
                // P200 - Насос P-200
                InitializePumpH(P200, "P200", "Насос P-200", "P-200");

                // P201 - Насос P-201
                InitializePumpH(P201, "P201", "Насос P-201", "P-201");

                // P202 - Насос P-202
                InitializePumpH(P202, "P202", "Насос P-202", "P-202");

                // P602 - Насос P-602
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602", "P-602");

                // ========== КЛАПАНЫ ==========
                // VT602 - Клапан SV-602 (3-ходовой)
                Initialize3Valve(VT602, "V602", "Клапан SV-602", "SV-602");

                // VT801 - Клапан V-801 (горизонтальный)
                InitializeValveH(VT801, "V801", "Клапан V-801", "V-801");

                // VT803 - Клапан V-803 (горизонтальный)
                InitializeValveH(VT803, "V803", "Клапан V-803", "V-803");

                // ========== НАГРЕВАТЕЛИ ==========
                // HE800 - Нагреватель HE-800
                InitializeHeater(HE800, "HE800", "Нагреватель HE-800", "HE-800");

                // ========== ПЕРЕКЛЮЧАТЕЛИ МИКСЕРОВ ==========
                // M200 Mixer Toggle
                if (M200MixerToggle != null)
                {
                    M200MixerToggle.Tag = "M200";
                    M200MixerToggle.StateChanged += M200MixerToggle_StateChanged;
                }

                // M250 Mixer Toggle
                if (M250MixerToggle != null)
                {
                    M250MixerToggle.Tag = "M250";
                    M250MixerToggle.StateChanged += M250MixerToggle_StateChanged;
                }

                // ========== ПЕРЕКЛЮЧАТЕЛИ НАГРЕВАТЕЛЕЙ ==========
                // HE800 Toggle
                if (HE800Toggle != null)
                {
                    HE800Toggle.Tag = "HE800";
                    HE800Toggle.StateChanged += HE800Toggle_StateChanged;
                }

                // ========== ИНИЦИАЛИЗАЦИЯ УСТАВОК ==========
                // Инициализация уставки массы Т-200
                var massTag = _global?.Variables?.GetByName("TC_AutoMassSp");
                if (massTag != null && T200MassSetEdit != null)
                {
                    T200MassSetEdit.Text = massTag.ValueString;
                }

                // Инициализация уставки массы топлива
                var fuelTag = _global?.Variables?.GetByName("TC_ManualDiselSp");
                if (fuelTag != null && FuelMassEdit != null)
                {
                    FuelMassEdit.Text = fuelTag.ValueString;
                }

                // Инициализация обработчиков для панели режимов
                InitializeModePanelHandlers();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов FrameTcPage: {ex.Message}");
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

                // 3. Обновление информации на панелях
                UpdatePanelInfo();

                // 4. Обновление видимости панелей по режиму
                UpdatePanelsVisibility();

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
            // Обновляем аналоговые датчики
            PT205?.UpdateElement();
            PT206?.UpdateElement();
            PT201?.UpdateElement();
            LT253?.UpdateElement();
            TT202?.UpdateElement();
            TT252?.UpdateElement();
            FM602?.UpdateElement();
            WIT200?.UpdateElement();

            // Обновляем дискретные датчики
            LAHH201?.UpdateElement();
            LAHH251?.UpdateElement();

            // Обновляем насосы
            P200?.UpdateElement();
            P201?.UpdateElement();
            P202?.UpdateElement();
            P602?.UpdateElement();

            // Обновляем клапаны
            VT602?.UpdateElement();
            VT801?.UpdateElement();
            VT803?.UpdateElement();

            // Обновляем нагреватель
            HE800?.UpdateElement();

            // Обновление состояния переключателей из переменных
            UpdateToggleSwitchesFromVariables();
        }

        private void UpdateToggleSwitchesFromVariables()
        {
            try
            {
                // Миксер M200
                var m200Tag = _global?.Variables?.GetByName("M200_IsWork");
                if (m200Tag != null && M200MixerToggle != null)
                {
                    bool isWorking = m200Tag.ValueReal > 0;
                    if (M200MixerToggle.IsChecked != isWorking)
                    {
                        M200MixerToggle.IsChecked = isWorking;
                    }
                }

                // Миксер M250
                var m250Tag = _global?.Variables?.GetByName("M250_IsWork");
                if (m250Tag != null && M250MixerToggle != null)
                {
                    bool isWorking = m250Tag.ValueReal > 0;
                    if (M250MixerToggle.IsChecked != isWorking)
                    {
                        M250MixerToggle.IsChecked = isWorking;
                    }
                }

                // Нагреватель HE800
                var he800Tag = _global?.Variables?.GetByName("HE800_IsOn");
                if (he800Tag != null && HE800Toggle != null)
                {
                    bool isOn = he800Tag.ValueReal > 0;
                    if (HE800Toggle.IsChecked != isOn)
                    {
                        HE800Toggle.IsChecked = isOn;
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

                // Сброс команды включения миксера Т-200
                command = _global.Commands.GetByName("M200_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения миксера Т-250
                command = _global.Commands.GetByName("M250_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения нагревателя HE800
                command = _global.Commands.GetByName("HE800_StartHeater");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд набора топлива
                command = _global.Commands.GetByName("TC_ManualStartDisel");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_ManualStartEmulgator");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_ManualStop");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_ManualPause");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд авторежима
                command = _global.Commands.GetByName("TC_AutolStart");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_AutoStop");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_AutoPause");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд перекачки
                command = _global.Commands.GetByName("TC_TransportStart");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_TransportStop");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команд режимов
                command = _global.Commands.GetByName("TC_RejimToOff");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_RejimToManual");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                command = _global.Commands.GetByName("TC_RejimToAuto");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса команд: {ex.Message}");
            }
        }

        private void UpdatePanelInfo()
        {
            try
            {
                // Обновление текущей массы в Т-200
                var massTag = _global?.Variables?.GetByName("WIT200_Volume");
                if (massTag != null && T200CurrentMassLabel != null)
                {
                    T200CurrentMassLabel.Text = $"Текущая: {massTag.ValueString} кг";
                }

                // Обновление статуса набора топлива
                UpdateFuelStatus();

                // Обновление счетчиков в панели ручного режима
                var manualDiselTag = _global?.Variables?.GetByName("TC_ManualDiselCurrent");
                if (manualDiselTag != null && TcModePanel != null)
                {
                    var diselCounterEdit = TcModePanel.FindName("TC_ManualDiselCounterEdit") as TextBox;
                    if (diselCounterEdit != null)
                        diselCounterEdit.Text = manualDiselTag.ValueString;
                }

                var manualEmulgatorTag = _global?.Variables?.GetByName("TC_ManualEmulgatorCurrent");
                if (manualEmulgatorTag != null && TcModePanel != null)
                {
                    var emulgatorCounterEdit = TcModePanel.FindName("TC_ManualEmulgatorCounterEdit") as TextBox;
                    if (emulgatorCounterEdit != null)
                        emulgatorCounterEdit.Text = manualEmulgatorTag.ValueString;
                }

                // Обновление статусов авторежима
                var autoDiselSpTag = _global?.Variables?.GetByName("TC_AutoDiselSp");
                var autoDiselCurrentTag = _global?.Variables?.GetByName("TC_AutoDiselCurrent");

                if (autoDiselSpTag != null && autoDiselCurrentTag != null && TcModePanel != null)
                {
                    var diselStatusLabel = TcModePanel.FindName("DiselStatusLabel") as TextBlock;
                    if (diselStatusLabel != null)
                        diselStatusLabel.Text = "Диз.топливо: " + autoDiselCurrentTag.ValueString + " из " + autoDiselSpTag.ValueString;
                }

                var autoEmulgatorSpTag = _global?.Variables?.GetByName("TC_AutoEmulgatorSp");
                var autoEmulgatorCurrentTag = _global?.Variables?.GetByName("TC_AutoEmulgatorCurrent");

                if (autoEmulgatorSpTag != null && autoEmulgatorCurrentTag != null && TcModePanel != null)
                {
                    var emulgatorStatusLabel = TcModePanel.FindName("EmulgatorStatusLabel") as TextBlock;
                    if (emulgatorStatusLabel != null)
                        emulgatorStatusLabel.Text = "Эмульгатор: " + autoEmulgatorCurrentTag.ValueString + " из " + autoEmulgatorSpTag.ValueString;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления панелей: {ex.Message}");
            }
        }

        private void UpdateFuelStatus()
        {
            try
            {
                if (FuelStatusLabel == null) return;

                string status = "";

                // Проверяем текущий режим набора
                var rejimTag = _global?.Variables?.GetByName("TC_Rejim");
                if (rejimTag != null)
                {
                    double rejimValue = rejimTag.ValueReal;

                    if (rejimValue == 3 || rejimValue == 4) // Набор дизеля
                    {
                        var currentTag = _global.Variables.GetByName("TC_ManualDiselCurrent");
                        var spTag = _global.Variables.GetByName("TC_ManualDiselSp");
                        if (currentTag != null && spTag != null)
                        {
                            status = $"Дизель: {currentTag.ValueString}/{spTag.ValueString} кг";
                        }
                    }
                    else if (rejimValue == 5 || rejimValue == 6) // Набор эмульгатора
                    {
                        var currentTag = _global.Variables.GetByName("TC_ManualEmulgatorCurrent");
                        var spTag = _global.Variables.GetByName("TC_ManualEmulgatorSp");
                        if (currentTag != null && spTag != null)
                        {
                            status = $"Эмульгатор: {currentTag.ValueString}/{spTag.ValueString} кг";
                        }
                    }
                    else if (rejimValue == 9 || rejimValue == 10) // Авто набор дизеля
                    {
                        var currentTag = _global.Variables.GetByName("TC_AutoDiselCurrent");
                        var spTag = _global.Variables.GetByName("TC_AutoDiselSp");
                        if (currentTag != null && spTag != null)
                        {
                            status = $"Авто дизель: {currentTag.ValueString}/{spTag.ValueString} кг";
                        }
                    }
                    else if (rejimValue == 7 || rejimValue == 8) // Авто набор эмульгатора
                    {
                        var currentTag = _global.Variables.GetByName("TC_AutoEmulgatorCurrent");
                        var spTag = _global.Variables.GetByName("TC_AutoEmulgatorSp");
                        if (currentTag != null && spTag != null)
                        {
                            status = $"Авто эмульгатор: {currentTag.ValueString}/{spTag.ValueString} кг";
                        }
                    }
                }

                FuelStatusLabel.Text = status;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления статуса топлива: {ex.Message}");
            }
        }

        private void UpdatePanelsVisibility()
        {
            try
            {
                var rejimTag = _global?.Variables?.GetByName("TC_Rejim");
                if (rejimTag == null)
                {
                    System.Diagnostics.Debug.WriteLine("ОШИБКА: Переменная TC_Rejim не найдена!");
                    return;
                }

                double rejimValue = rejimTag.ValueReal;

                // Логика видимости панелей в зависимости от режима
                switch ((int)rejimValue)
                {
                    case 0: // OFF - все панели скрыты
                        SetPanelVisibility(Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed, false);
                        System.Diagnostics.Debug.WriteLine("Режим OFF: все панели скрыты");
                        break;

                    case 1: // Полуавтомат (ожидание) - все панели видны
                        SetPanelVisibility(Visibility.Visible, Visibility.Visible, Visibility.Visible, true);
                        System.Diagnostics.Debug.WriteLine("Режим ПОЛУАВТОМАТ: все панели видны");
                        break;

                    case 2: // Автомат (ожидание) - нет панели набора топлива
                        SetPanelVisibility(Visibility.Collapsed, Visibility.Visible, Visibility.Visible, true);
                        System.Diagnostics.Debug.WriteLine("Режим АВТОМАТ: скрыта панель набора топлива");
                        break;

                    case 3: // Полуавтомат - пауза набора дизеля
                    case 4: // Полуавтомат - набор дизеля
                        SetPanelVisibility(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed, false);
                        System.Diagnostics.Debug.WriteLine($"Режим {rejimValue}: набор дизеля");
                        break;

                    case 5: // Полуавтомат - пауза набора эмульгатора
                    case 6: // Полуавтомат - набор эмульгатора
                        SetPanelVisibility(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed, false);
                        System.Diagnostics.Debug.WriteLine($"Режим {rejimValue}: набор эмульгатора");
                        break;

                    case 7: // Автомат - пауза набора эмульгатора
                    case 8: // Автомат - набор эмульгатора
                    case 9: // Автомат - пауза набора дизеля
                    case 10: // Автомат - набор дизеля
                        SetPanelVisibility(Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed, false);
                        System.Diagnostics.Debug.WriteLine($"Режим {rejimValue}: авто набор");
                        break;

                    case 11: // Перекачка - пауза
                    case 12: // Перекачка - работа
                        SetPanelVisibility(Visibility.Collapsed, Visibility.Visible, Visibility.Visible, true);
                        System.Diagnostics.Debug.WriteLine($"Режим {rejimValue}: перекачка");
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"НЕИЗВЕСТНЫЙ РЕЖИМ: {rejimValue}");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА в UpdatePanelsVisibility: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Вспомогательный метод для установки видимости всех панелей
        /// </summary>
        /// <param name="fuelPanelVisibility">Видимость панели набора топлива</param>
        /// <param name="t200PanelVisibility">Видимость панели массы Т-200</param>
        /// <param name="transferPanelVisibility">Видимость панели перекачки</param>
        /// <param name="transferPanelEnabled">Активность панели перекачки</param>
        private void SetPanelVisibility(
            Visibility fuelPanelVisibility,
            Visibility t200PanelVisibility,
            Visibility transferPanelVisibility,
            bool transferPanelEnabled)
        {
            if (FuelPanel != null)
                FuelPanel.Visibility = fuelPanelVisibility;

            if (T200MassPanel != null)
                T200MassPanel.Visibility = t200PanelVisibility;

            if (T100toT150Panel != null)
            {
                T100toT150Panel.Visibility = transferPanelVisibility;
                T100toT150Panel.IsEnabled = transferPanelEnabled;
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

        private void InitializePumpH(Element_PumpH pump, string varName, string description, string tagName)
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

        private void InitializeModePanelHandlers()
        {
            if (TcModePanel != null)
            {
                var rejimOffButton = TcModePanel.FindName("RejimOffButton") as Button;
                if (rejimOffButton != null)
                    rejimOffButton.Click += RejimOffButton_Click;

                var rejimManualButton = TcModePanel.FindName("RejimManualButton") as Button;
                if (rejimManualButton != null)
                    rejimManualButton.Click += RejimManualButton_Click;

                var rejimAutoButton = TcModePanel.FindName("RejimAutoButton") as Button;
                if (rejimAutoButton != null)
                    rejimAutoButton.Click += RejimAutoButton_Click;

                var manualStartButton = TcModePanel.FindName("ManualStartButton") as Button;
                if (manualStartButton != null)
                    manualStartButton.Click += ManualStartButton_Click;

                var manualStopButton = TcModePanel.FindName("ManualStopButton") as Button;
                if (manualStopButton != null)
                    manualStopButton.Click += ManualStopButton_Click;

                var manualPauseButton = TcModePanel.FindName("ManualPauseButton") as Button;
                if (manualPauseButton != null)
                    manualPauseButton.Click += ManualPauseButton_Click;

                var autoStartButton = TcModePanel.FindName("AutoStartButton") as Button;
                if (autoStartButton != null)
                    autoStartButton.Click += AutoStartButton_Click;

                var autoStopButton = TcModePanel.FindName("AutoStopButton") as Button;
                if (autoStopButton != null)
                    autoStopButton.Click += AutoStopButton_Click;

                var autoPauseButton = TcModePanel.FindName("AutoPauseButton") as Button;
                if (autoPauseButton != null)
                    autoPauseButton.Click += AutoPauseButton_Click;
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

        private void M200MixerToggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M200", 1);
                TCommandTag command = _global.Commands.GetByName("M200_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M200", 1);
                TCommandTag command = _global.Commands.GetByName("M200_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void M250MixerToggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M250", 1);
                TCommandTag command = _global.Commands.GetByName("M250_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M250", 1);
                TCommandTag command = _global.Commands.GetByName("M250_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void HE800Toggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE800", 1);
                TCommandTag command = _global.Commands.GetByName("HE800_StartHeater");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE800", 1);
                TCommandTag command = _global.Commands.GetByName("HE800_StartHeater");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        // Обработчики панели набора топлива
        private void FuelStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                if (DiselRadio.IsChecked == true)
                {
                    // Набор дизеля
                    if (double.TryParse(FuelMassEdit.Text, out double mass))
                    {
                        TCommandTag spCommand = _global.Commands.GetByName("TC_ManualDiselSp");
                        if (spCommand != null)
                        {
                            spCommand.WriteValue = mass.ToString();
                            spCommand.NeedToWrite = true;
                        }

                        TCommandTag startCommand = _global.Commands.GetByName("TC_ManualStartDisel");
                        if (startCommand != null)
                        {
                            startCommand.WriteValue = "true";
                            startCommand.NeedToWrite = true;
                        }
                        _global.Log.Add("Пользователь", "Запуск набора дизеля", 1);
                    }
                }
                else if (EmulgatorRadio.IsChecked == true)
                {
                    // Набор эмульгатора
                    if (double.TryParse(FuelMassEdit.Text, out double mass))
                    {
                        TCommandTag spCommand = _global.Commands.GetByName("TC_ManualEmulgatorSp");
                        if (spCommand != null)
                        {
                            spCommand.WriteValue = mass.ToString();
                            spCommand.NeedToWrite = true;
                        }

                        TCommandTag startCommand = _global.Commands.GetByName("TC_ManualStartEmulgator");
                        if (startCommand != null)
                        {
                            startCommand.WriteValue = "true";
                            startCommand.NeedToWrite = true;
                        }
                        _global.Log.Add("Пользователь", "Запуск набора эмульгатора", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска набора топлива: {ex.Message}");
            }
        }

        private void FuelStopButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_ManualStop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка набора топлива", 1);
            }
        }

        private void FuelPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_ManualPause");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пауза набора топлива", 1);
            }
        }

        // Обработчики панели массы в Т-200
        private void T200SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                if (double.TryParse(T200MassSetEdit.Text, out double mass))
                {
                    TCommandTag command = _global.Commands.GetByName("TC_AutoMassSp");
                    if (command != null)
                    {
                        command.WriteValue = mass.ToString();
                        command.NeedToWrite = true;
                        _global.Log.Add("Пользователь", $"Задание массы Т200: {mass} кг", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка задания массы Т200: {ex.Message}");
            }
        }

        private void T200ResetButton_Click(object sender, RoutedEventArgs e)
        {
            T200MassSetEdit.Text = "0";
            TCommandTag command = _global?.Commands?.GetByName("TC_AutoMassSp");
            if (command != null)
            {
                command.WriteValue = "0";
                command.NeedToWrite = true;
            }
        }

        // Обработчики панели перекачки
        private void TransferStartButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_TransportStart");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пуск перекачки", 1);
            }
        }

        private void TransferStopButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_TransportStop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка перекачки", 1);
            }
        }

        // Обработчики для кнопок панели режимов
        private void RejimOffButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_RejimToOff");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Перевод в режим OFF", 1);
            }
        }

        private void RejimManualButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_RejimToManual");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Перевод в ручной режим", 1);
            }
        }

        private void RejimAutoButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_RejimToAuto");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Перевод в авторежим", 1);
            }
        }

        private void ManualStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем существующий обработчик
            FuelStartButton_Click(sender, e);
        }

        private void ManualStopButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_ManualStop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка ручного режима", 1);
            }
        }

        private void ManualPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_ManualPause");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пауза ручного режима", 1);
            }
        }

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                var autoMassSpEdit = TcModePanel?.FindName("TCAutoMassSpEdit") as TextBox;
                if (autoMassSpEdit != null && double.TryParse(autoMassSpEdit.Text, out double mass))
                {
                    TCommandTag spCommand = _global.Commands.GetByName("TC_AutoMassSp");
                    if (spCommand != null)
                    {
                        spCommand.WriteValue = mass.ToString();
                        spCommand.NeedToWrite = true;
                    }

                    TCommandTag startCommand = _global.Commands.GetByName("TC_AutolStart");
                    if (startCommand != null)
                    {
                        startCommand.WriteValue = "true";
                        startCommand.NeedToWrite = true;
                    }
                    _global.Log.Add("Пользователь", $"Запуск авторежима с массой {mass} кг", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска авторежима: {ex.Message}");
            }
        }

        private void AutoStopButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_AutoStop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка авторежима", 1);  
            }
        }

        private void AutoPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TCommandTag command = _global?.Commands?.GetByName("TC_AutoPause");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пауза авторежима", 1);
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
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
        private bool _isInitialized = false;

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

            // Настройка таймера обновления
            InitializeTimer();

            System.Diagnostics.Debug.WriteLine("FrameGroPage инициализирован");
        }

        private void FrameGroPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized && _global != null)
            {
                InitializeElements();
                InitializeTimer();
                _isInitialized = true;
            }
            _repaintTimer?.Start();
        }

        private void FrameGroPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _repaintTimer?.Stop();
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
                // TT106 удален
                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");
                InitializeSensor(TT302, "TT302", "Датчик температуры TT-302", "TT-302", "°C");
                InitializeSensor(TT402, "TT402", "Датчик температуры TT-402", "TT-402", "°C");
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");

                // Датчики давления
                InitializeSensor(PT104, "PT104", "Датчик давления PT104", "PT-104", "бар");
                InitializeSensor(PT105, "PT105", "Датчик давления PT105", "PT-105", "бар");
                InitializeSensor(PT304, "PT304", "Датчик давления PT304", "PT-304", "бар");
                InitializeSensor(PT404, "PT404", "Датчик давления PT404", "PT-404", "бар");
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "бар");

                // Расходомеры
                InitializeSensor(FM401, "FM401", "Массовый расходомер FM401", "FM401", "кг/ч");
                InitializeSensor(FM601, "FM601", "Массовый расходомер FM601", "FM601", "кг/ч");

                // Счетчик QM400
                InitializeSensor(QM400, "QM400", "Счетчик QM-400", "QM-400", "");

                // Весовой датчик WIT100
                InitializeSensor(WIT100, "WIT100", "Вес Т-100", "WIT-100", "кг");

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

        private void InitializeTimer()
        {
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;
        }

        private void InitializeSensor(Element_AI sensor, string varName, string description, string name, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.Name = name;
                sensor.EU = eu;
                System.Diagnostics.Debug.WriteLine($"Инициализирован датчик: {name} ({varName})");
            }
        }

        private void InitializeDiscreteSensor(Element_DI sensor, string varName, string description, string name)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован дискретный датчик: {name} ({varName})");
            }
        }

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump, string varName, string description, string name)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован насос: {name} ({varName})");
            }
        }

        private void InitializePumpReverse(Element_PumpHReverse pump, string varName, string description, string name)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован насос обратный: {name} ({varName})");
            }
        }

        private void InitializeValveV(Element_ValveV valve, string varName, string description, string name)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован клапан: {name} ({varName})");
            }
        }

        private void InitializeValveH(Element_ValveH valve, string varName, string description, string name)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован клапан горизонтальный: {name} ({varName})");
            }
        }

        private void Initialize3ValveH(Element_3ValveH valve, string varName, string description, string name)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован 3-ходовой клапан: {name} ({varName})");
            }
        }

        private void InitializeHeater(Element_Heater heater, string varName, string description, string name)
        {
            if (heater != null && _global != null)
            {
                heater.Global = _global;
                heater.VarName = varName;
                heater.Description = description;
                heater.Name = name;
                System.Diagnostics.Debug.WriteLine($"Инициализирован нагреватель: {name} ({varName})");
            }
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            // Останавливаем таймер на время обновления
            _repaintTimer.Stop();

            try
            {
                // 1. Обновление всех элементов
                UpdateAllElements();

                // 2. Сброс команд
                ResetCommands();

                // 3. Обновление режимов работы
                UpdateOperationMode();

                // 4. Обновление счетчиков
                UpdateCounters();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в RepaintTimer_Tick: {ex.Message}");
            }
            finally
            {
                // Запускаем таймер снова
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
            // TT106 удален
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

                // Сброс команд миксеров
                ResetCommand("M100_StartMixer");
                ResetCommand("M150_StartMixer");
                ResetCommand("M400_StartMixer");

                // Сброс команд нагревателей
                ResetCommand("HE300_IsOn");
                ResetCommand("HE750_IsOn");

                // Сброс команд подачи вещества
                ResetCommand("GRO_Manual_Selitra_Start");
                ResetCommand("GRO_Manual_Water_Start");
                ResetCommand("GRO_Manual_Kislota_Start");
                ResetCommand("GRO_Manual_Stop");
                ResetCommand("GRO_Manual_Pause");

                // Сброс команд транспортировки
                ResetCommand("GRO_TransportStart");
                ResetCommand("GRO_TransportStop");

                // Сброс команды задания массы Т-100
                ResetCommand("T100_MassSp");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса команд: {ex.Message}");
            }
        }

        private void ResetCommand(string commandName)
        {
            var command = _global.Commands.GetByName(commandName);
            if (command != null && !command.NeedToWrite)
            {
                command.WriteValue = "false";
            }
        }

        private void UpdateOperationMode()
        {
            var tag = _global.Variables.GetByName("GRO_Rejim");
            if (tag == null) return;

            int mode = (int)tag.ValueReal;

            // Обновляем состояние элементов управления
            switch (mode)
            {
                case 0: // OFF
                    UpdateUIState(true, true, true, true, true);
                    break;

                case 1: // Полуавтомат
                    UpdateUIState(true, false, false, true, true);
                    break;

                case 2: // Автомат
                    UpdateUIState(false, false, false, true, true);
                    break;

                case 3: // Полуавтомат - Наполнение селитры
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 4: // Полуавтомат - Наполнение селитры.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 5: // Полуавтомат - Наполнение воды
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 6: // Полуавтомат - Наполнение воды.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 7: // Полуавтомат - Наполнение кислоты
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 8: // Полуавтомат - Наполнение кислоты.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 9: // Автомат - Наполнение воды и кислоты
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 10: // Автомат - Наполнение воды и кислоты.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 11: // Автомат - Наполнение селитры
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 12: // Автомат - Наполнение селитры.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 13: // Автомат - Наполнение воды
                    UpdateUIState(false, true, true, false, false);
                    break;

                case 14: // Автомат - Наполнение воды.Пауза
                    UpdateUIState(true, true, false, false, false);
                    break;

                case 15: // Транспортировка
                case 16: // Транспортировка
                    UpdateUIState(false, false, false, false, false);
                    break;
            }

            // Синхронизируем панель режимов с текущим режимом
            if (GroModePanel != null)
            {
                OperationMode currentOperationMode = mode switch
                {
                    0 => OperationMode.Off,
                    1 or 3 or 4 or 5 or 6 or 7 or 8 => OperationMode.SemiAuto,
                    2 or 9 or 10 or 11 or 12 or 13 or 14 => OperationMode.Auto,
                    15 or 16 => OperationMode.Off,
                    _ => OperationMode.Off
                };

                GroModePanel.SetMode(currentOperationMode);
            }
        }

        private void UpdateUIState(bool substanceStartEnabled, bool substanceStopEnabled, bool transportEnabled,
                                  bool substanceRadioEnabled, bool substancePanelVisible)
        {
            // Обновление кнопок выбора вещества
            SubstanceStartButton.IsEnabled = substanceStartEnabled;
            SubstanceStopButton.IsEnabled = substanceStopEnabled;
            SubstancePauseButton.IsEnabled = substanceStopEnabled;

            SelitraRadio.IsEnabled = substanceRadioEnabled;
            WaterRadio.IsEnabled = substanceRadioEnabled;
            KislotaRadio.IsEnabled = substanceRadioEnabled;

            SelitraSpEdit.IsEnabled = substanceRadioEnabled;
            WaterSpEdit.IsEnabled = substanceRadioEnabled;
            KislotaSpEdit.IsEnabled = substanceRadioEnabled;

            // Обновление панели транспортировки
            TransportStartButton.IsEnabled = transportEnabled;
            TransportStopButton.IsEnabled = transportEnabled;

            // Видимость панели выбора вещества
            SubstancePanel.Visibility = substancePanelVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateCounters()
        {
            // Обновление счетчиков в панели
            UpdateTextBox(GRO_ManualSelitraCounterEdit, "GRO_ManualSelitraCounter");
            UpdateTextBox(GRO_ManualWaterCounterEdit, "GRO_ManualWaterCounter");
            UpdateTextBox(GRO_ManualKislotaCounterEdit, "GRO_ManualKislotaCounter");
        }

        private void UpdateTextBox(TextBox textBox, string varName)
        {
            var tag = _global.Variables.GetByName(varName);
            if (tag != null)
            {
                textBox.Text = tag.ValueString;
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

        private void M100Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M100", 1);
                SendCommand("M100_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M100", 1);
                SendCommand("M100_StartMixer", "false");
            }
        }

        private void M150Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M150", 1);
                SendCommand("M150_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M150", 1);
                SendCommand("M150_StartMixer", "false");
            }
        }

        private void M400Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M400", 1);
                SendCommand("M400_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M400", 1);
                SendCommand("M400_StartMixer", "false");
            }
        }

        private void HE300Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE-300", 1);
                SendCommand("HE300_IsOn", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE-300", 1);
                SendCommand("HE300_IsOn", "false");
            }
        }

        private void HE750Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE-750", 1);
                SendCommand("HE750_IsOn", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE-750", 1);
                SendCommand("HE750_IsOn", "false");
            }
        }

        // Обработчик изменения режима через панель
        private void GroModePanel_ModeChanged(object sender, OperationMode mode)
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

                SendCommand(commandName, "true", $"Переход в режим {mode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка изменения режима GRO: {ex.Message}");
            }
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ==========

        private void SetT100MassButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("T100_MassSp", T100MassSpEdit.Text, "Задание массы в Т-100");
        }

        private void A100SpeedSpEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматическая отправка при изменении текста
            if (!string.IsNullOrEmpty(A100SpeedSpEdit.Text))
            {
                SendCommand("A100_Speed", A100SpeedSpEdit.Text, "Задание скорости A100");
            }
        }

        private void SubstanceStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Сначала устанавливаем уставки
            if (SelitraRadio.IsChecked == true)
            {
                SendCommand("GRO_ManualSelitraCounterSp", SelitraSpEdit.Text, "Уставка селитры");
                SendCommand("GRO_Manual_Selitra_Start", "true", "Пуск селитры");
            }
            else if (WaterRadio.IsChecked == true)
            {
                SendCommand("GRO_ManualWaterCounterSp", WaterSpEdit.Text, "Уставка воды");
                SendCommand("GRO_Manual_Water_Start", "true", "Пуск воды");
            }
            else if (KislotaRadio.IsChecked == true)
            {
                SendCommand("GRO_ManualKislotaCounterSp", KislotaSpEdit.Text, "Уставка кислоты");
                SendCommand("GRO_Manual_Kislota_Start", "true", "Пуск кислоты");
            }
        }

        private void SubstanceStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("GRO_Manual_Stop", "true", "Остановка подачи вещества");
        }

        private void SubstancePauseButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("GRO_Manual_Pause", "true", "Пауза подачи вещества");
        }

        private void TransportStartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("GRO_TransportStart", "true", "Пуск транспортировки");
        }

        private void TransportStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("GRO_TransportStop", "true", "Остановка транспортировки");
        }

        private void SaveHE700RejimButton_Click(object sender, RoutedEventArgs e)
        {
            if (HE700ComboBox.SelectedIndex >= 0)
            {
                SendCommand("HE700_Rejim", HE700ComboBox.SelectedIndex.ToString(), "Смена режима HE-700");
            }
        }

        private void SendCommand(string commandName, string value)
        {
            SendCommand(commandName, value, $"Команда {commandName}");
        }

        private void SendCommand(string commandName, string value, string description)
        {
            try
            {
                var command = _global?.Commands?.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = value;
                    command.NeedToWrite = true;
                    command.SendToController();

                    if (!string.IsNullOrEmpty(description))
                    {
                        _global?.Log?.Add("Пользователь", description, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды {commandName}: {ex.Message}");
            }
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameGroPage : UserControl
    {
        private DispatcherTimer _repaintTimer;
        private TGlobal _global;

        public FrameGroPage()
        {
            InitializeComponent();
        }

        public TGlobal Global
        {
            get => _global;
            set
            {
                _global = value;
                if (_global != null)
                {
                    Initialize();
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;
            _repaintTimer.Start();
        }

        public void Initialize()
        {
            if (_global == null) return;

            // Датчики уровня
            InitializeAI(LT150, "LT150", "Датчик уровня LT150", "LT-150", "Уровень", "%");
            InitializeAI(LT301, "LT301", "Датчик уровня LT301", "LT-301", "Уровень", "%");
            InitializeAI(LT303, "LT303", "Датчик уровня LT303", "LT-303", "Уровень", "%");
            InitializeAI(LT403, "LT403", "Датчик уровня LT403", "LT-403", "Уровень", "%");

            // Сигнализаторы уровня
            InitializeDI(LAHH101, "LAHH101", "Датчик уровня LAHH101", "LAHH-101");
            InitializeDI(LALL103, "LALL103", "Датчик уровня LALL103", "LALL-103");
            InitializeDI(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
            InitializeDI(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
            InitializeDI(LAHH301, "LAHH301", "Датчик уровня LAHH301", "LAHH-301");
            InitializeDI(LAHH302, "LAHH302", "Датчик уровня LAHH302", "LAHH-302");
            InitializeDI(LAHH401, "LAHH401", "Датчик уровня LAHH401", "LAHH-401");

            // Датчики температуры
            InitializeAI(TT102, "TT102", "Датчик температуры TT-102", "TT-102", "Температура", "°C");
            InitializeAI(TT106, "TT106", "Датчик температуры TT-106", "TT-106", "Температура", "°C");
            InitializeAI(TT152, "TT152", "Датчик температуры TT-152", "TT152", "Температура", "°C");
            InitializeAI(TT302, "TT302", "Датчик температуры TT-302", "TT-302", "Температура", "°C");
            InitializeAI(TT402, "TT402", "Датчик температуры TT-402", "TT-402", "Температура", "°C");
            InitializeAI(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "Температура", "°C");

            // Датчики давления
            InitializeAI(PT104, "PT104", "Датчик давления PT104", "PT-104", "Давление", "бар");
            InitializeAI(PT105, "PT105", "Датчик давления PT105", "PT-105", "Давление", "бар");
            InitializeAI(PT304, "PT304", "Датчик давления PT304", "PT-304", "Давление", "бар");
            InitializeAI(PT404, "PT404", "Датчик давления PT404", "PT-404", "Давление", "бар");
            InitializeAI(PT601, "PT601", "Датчик давления PT601", "PT-601", "Давление", "бар");

            // Расходомеры
            InitializeAI(FM401, "FM401", "Массовый расходомер FM401", "FM401", "Расход", "кг/ч");
            InitializeAI(FM601, "FM601", "Массовый расходомер FM601", "FM601", "Расход", "кг/ч");

            // Счетчик QM400
            InitializeAI(QM400, "QM400", "Счетчик QM-400", "QM-400", "Счетчик", "");

            // Весовой датчик WIT100
            InitializeAI(WIT100, "WIT100", "Вес Т-100", "WIT-100", "Вес", "кг");

            // Насосы обратные (P300, P400)
            InitializePumpReverse(P300, "P300", "Насос P-300");
            InitializePumpReverse(P400, "P400", "Насос P-400");

            // Насосы обычные
            InitializePump(P100, "P100", "Насос P-100");
            InitializePump(A100, "A100", "Шнек А-100");
            InitializePump(P601, "P601", "Насос P-601");

            // Задвижки
            InitializeValveH(VT101, "V101", "Клапан V-101");
            InitializeValveH(VT151, "V151", "Клапан V-151");
            InitializeValveH(VT152, "V152", "Клапан V-152");
            InitializeValveV(VT302, "V302", "Клапан V-302");
            InitializeValveH(VT305, "V305", "Клапан V-305");
            InitializeValveH(VT401, "V401", "Клапан V-401");
            Initialize3ValveH(VT601, "V601", "Клапан SV-601");

            // Нагреватели
            InitializeHeater(HE300, "HE300", "Нагреватель HE-300");
            InitializeHeater(HE750, "HE750", "Нагреватель HE-750");
            InitializeHeater(HE700_1, "HE700.1", "Нагреватель HE-700.1");
            InitializeHeater(HE700_2, "HE700.2", "Нагреватель HE-700.2");

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

            // Инициализация состояния миксеров
            UpdateMixerSwitchState(M100Switch, "M100");
            UpdateMixerSwitchState(M150Switch, "M150");
            UpdateMixerSwitchState(M400Switch, "M400");
        }

        private void UpdateMixerSwitchState(ToggleSwitch toggleSwitch, string varName)
        {
            var tag = _global.Variables.GetByName(varName + "_IsWork");
            if (tag != null)
            {
                toggleSwitch.IsChecked = tag.ValueReal > 0;
            }
        }

        private void InitializeAI(Element_AI element, string varName, string description, string name, string designation, string eu)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
            element.Name = name;
            element.Designation = designation;
            element.EU = eu;
        }

        private void InitializeDI(Element_DI element, string varName, string description, string name)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
            element.Name = name;
        }

        private void InitializePump(Element_PumpUzUnderPanel element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void InitializePumpReverse(Element_PumpHReverse element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void InitializeValveV(Element_ValveV element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void InitializeValveH(Element_ValveH element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void Initialize3ValveH(Element_3ValveH element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void InitializeHeater(Element_Heater element, string varName, string description)
        {
            element.Global = _global;
            element.VarName = varName;
            element.Description = description;
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            if (_global == null) return;

            _repaintTimer.Stop();

            try
            {
                // Обновление состояния оборудования
                UpdateAllElements();

                // Обновление режимов работы
                UpdateOperationMode();

                // Обновление счетчиков
                UpdateCounters();

                // Обновление состояния миксеров
                UpdateMixerSwitchState(M100Switch, "M100");
                UpdateMixerSwitchState(M150Switch, "M150");
                UpdateMixerSwitchState(M400Switch, "M400");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в таймере обновления: {ex.Message}");
            }
            finally
            {
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
            TT106?.UpdateElement();
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
                    15 or 16 => OperationMode.Off, // Для транспортировки возвращаем OFF
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
            SubstancePauseButton.IsEnabled = substanceStopEnabled; // Пауза доступна когда есть Стоп

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

            // Обновление счетчиков в информационной панели
            UpdateCounterText(SelitraCounterText, "GRO_ManualSelitraCounter", "Селитра");
            UpdateCounterText(WaterCounterText, "GRO_ManualWaterCounter", "Вода");
            UpdateCounterText(KislotaCounterText, "GRO_ManualKislotaCounter", "Кислота");
        }

        private void UpdateTextBox(TextBox textBox, string varName)
        {
            var tag = _global.Variables.GetByName(varName);
            if (tag != null)
            {
                textBox.Text = tag.ValueString;
            }
        }

        private void UpdateCounterText(TextBlock textBlock, string varName, string title)
        {
            var tag = _global.Variables.GetByName(varName);
            if (tag != null)
            {
                textBlock.Text = $"{title}: {tag.ValueString} кг";
            }
        }

        // Обработчики команд
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

        // Обработчики миксеров
        private void M100Switch_StateChanged(object sender, bool isChecked)
        {
            SendCommand("M100_StartMixer", isChecked ? "true" : "false",
                       isChecked ? "Включение миксера M100" : "Отключение миксера M100");
        }

        private void M150Switch_StateChanged(object sender, bool isChecked)
        {
            SendCommand("M150_StartMixer", isChecked ? "true" : "false",
                       isChecked ? "Включение миксера M150" : "Отключение миксера M150");
        }

        private void M400Switch_StateChanged(object sender, bool isChecked)
        {
            SendCommand("M400_StartMixer", isChecked ? "true" : "false",
                       isChecked ? "Включение миксера M400" : "Отключение миксера M400");
        }

        // Обработчик изменения режима через панель
        private void GroModePanel_ModeChanged(object sender, OperationMode mode)
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

        // Общий метод отправки команд
        private void SendCommand(string commandName, string value, string description)
        {
            if (_global == null) return;

            try
            {
                var command = _global.Commands.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = value;
                    command.NeedToWrite = true;
                    command.SendToController();

                    _global.Log.Add("Пользователь", description, 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды {commandName}: {ex.Message}");
            }
        }
    }
}
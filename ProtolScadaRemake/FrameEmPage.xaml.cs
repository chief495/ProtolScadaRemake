using System;
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

        public FrameEmPage()
        {
            InitializeComponent();
            Loaded += FrameEmPage_Loaded;
            Unloaded += FrameEmPage_Unloaded;
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

            UpdateOperationMode();
            UpdatePanelsVisibility();

            // Подписка на события
            SubscribeToEvents();

            System.Diagnostics.Debug.WriteLine("FrameGroPage инициализирован");
        }

        private void FrameEmPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateOperationMode();
            UpdatePanelsVisibility();
            // Запуск таймера после загрузки
            _repaintTimer?.Start();
        }

        private void FrameEmPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void InitializeElements()
        {
            try
            {
                // ========== АНАЛОГОВЫЕ ДАТЧИКИ ==========
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

                // ========== ДИСКРЕТНЫЕ ДАТЧИКИ ==========
                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");
                InitializeDiscreteSensor(LAHH653, "LAHH653", "Датчик уровня LAHH653", "LAHH-653");

                // ========== МИКСЕРЫ ==========
                if (M150 != null)
                    M150.StateChanged += M150Mixer_StateChanged;

                if (M250 != null)
                    M250.StateChanged += M250Mixer_StateChanged;

                // Панель режима
                if (EmModePanel != null)
                {
                    EmModePanel.ModeChanged += EmModePanel_ModeChanged;
                }

                // ========== НАСОСЫ ==========
                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601", "P-601");
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602", "P-602");
                InitializePumpUz(M600, "M600", "Миксер M-600", "M-600");
                InitializePumpUzUnderPanel(P651, "P651", "Насос P-651", "P-651");

                // ========== КЛАПАНЫ ==========
                Initialize3Valve(SV601, "V601", "Клапан SV-601", "SV-601");
                Initialize3Valve(SV602, "V602", "Клапан SV-602", "SV-602");
                InitializeValve(V505, "V505", "Клапан V-505", "V-505");

                // Инициализация панелей управления
                InitializePanels();

                System.Diagnostics.Debug.WriteLine("Элементы управления EM успешно инициализированы");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов EM: {ex.Message}");
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
            if (_global == null) return;

            _repaintTimer.Stop();

            try
            {
                // 1. Сброс команд как в старом проекте
                ResetCommands();

                // 2. Обновление всех элементов
                UpdateAllElements();

                // 3. Обновление информации на панелях
                UpdatePanelInfo();

                // 4. Обновление ModePanel (если нужно)
                UpdateModePanelIfNeeded();

                // 5. Синхронизация переключателей режима по текущему EM_Rejim
                UpdateOperationMode();

                // 6. Обновление видимости панелей по режиму
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

                // Сброс команды включения миксера Т-150
                var command = _global.Commands.GetByName("T150_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения миксера M250
                command = _global.Commands.GetByName("M250_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";
            }
            catch { }
        }

        private void UpdateAllElements()
        {
            try
            {
                // Обновляем аналоговые датчики
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

                // Обновляем дискретные датчики
                LAHH151?.UpdateElement();
                LALL153?.UpdateElement();
                LAHH251?.UpdateElement();
                LAHH653?.UpdateElement();

                // Обновляем насосы
                P601?.UpdateElement();
                P602?.UpdateElement();
                M600?.UpdateElement();
                P651?.UpdateElement();

                // Обновляем клапаны
                SV601?.UpdateElement();
                SV602?.UpdateElement();
                V505?.UpdateElement();

                // Обновляем состояние переключателей миксеров из переменных
                UpdateMixerTogglesFromVariables();
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
                // Миксер M150
                var m150Tag = _global?.Variables?.GetByName("M150_IsWork");
                if (m150Tag != null && M150 != null)
                {
                    bool isWorking = m150Tag.ValueReal > 0;
                    if (M150.IsChecked != isWorking)
                    {
                        M150.IsChecked = isWorking;
                    }
                }

                // Миксер M250
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

        private void UpdatePanelInfo()
        {
            try
            {
                // Обновляем панели управления из глобальных переменных
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
                // Обновляем режим работы EM
                var rejimTag = _global?.Variables?.GetByName("EM_Rejim");
                if (rejimTag != null)
                {
                    double rejimValue = rejimTag.ValueReal;

                    // Для отладки
                    System.Diagnostics.Debug.WriteLine($"EM_Rejim: {rejimValue}");

                    // Обновляем статусы в ModePanel (если там есть элементы для отображения)
                    UpdateModePanelStatus(rejimValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления ModePanel: {ex.Message}");
            }
        }

        private void UpdateModePanelStatus(double rejimValue)
        {
            // Обновляем текстовые метки в ModePanel
            try
            {
                // Ищем элементы в ModePanel
                var currRejimLabel = EmModePanel?.FindName("CurrRejimLabel") as TextBlock;
                var currStageLabel = EmModePanel?.FindName("CurrStageLabel") as TextBlock;

                if (currRejimLabel != null && currStageLabel != null)
                {
                    // Обновляем в зависимости от режима (аналогично старому коду)
                    switch (rejimValue)
                    {
                        case 0: // OFF
                            currRejimLabel.Text = "OFF";
                  

                            currStageLabel.Text = "OFF";
                            currStageLabel.Foreground = Brushes.White;
                            break;
                        case 1: // Автомат - OFF
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "OFF";
                            currStageLabel.Foreground = Brushes.White;
                            break;
                        case 2: // Автомат - Затравка. Выход на режим.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 3: // Автомат - Затравка. Наполнение топливной смесью.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Наполнение топливной смесью.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 4: // Автомат - Затравка. Наполнение ГРО.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Наполнение ГРО.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 5: // Автомат - Затравка. Смешивание.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Затравка. Смешивание.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 6: // Автомат - Производство. Выход на режим.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Производство. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 7: // Автомат - Производство.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Производство.";
                            currStageLabel.Foreground = Brushes.Green;
                            break;
                        case 8: // Автомат - Промывка.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Промывка.";
                            currStageLabel.Foreground = Brushes.LightBlue;
                            break;
                        case 9: // Автомат - Дожим. Выход на режим.
                            currRejimLabel.Text = "Автомат";
                            currRejimLabel.Foreground = Brushes.LimeGreen;
                            currStageLabel.Text = "Дожим. Выход на режим.";
                            currStageLabel.Foreground = Brushes.YellowGreen;
                            break;
                        case 10: // Автомат - Дожим.
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

                // Также можно обновить кнопки в ModePanel
                UpdateModePanelButtons(rejimValue);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка UpdateModePanelStatus: {ex.Message}");
            }
        }

        private void UpdateModePanelButtons(double rejimValue)
        {
            // Обновляем состояние кнопок в ModePanel
            try
            {
                var rejimOffButton = EmModePanel?.FindName("RejimOffButton") as Button;
                var rejimAutoButton = EmModePanel?.FindName("RejimAutoButton") as Button;

                if (rejimOffButton != null && rejimAutoButton != null)
                {
                    // Если режим OFF (0) - подсвечиваем кнопку OFF
                    if (rejimValue == 0)
                    {
                        rejimOffButton.Background = Brushes.Green;
                        rejimOffButton.Foreground = Brushes.White;
                        rejimAutoButton.Background = Brushes.Gray;
                        rejimAutoButton.Foreground = Brushes.White;
                    }
                    // Если режим Автомат (1-10) - подсвечиваем кнопку Авто
                    else if (rejimValue >= 1 && rejimValue <= 10)
                    {
                        rejimOffButton.Background = Brushes.Gray;
                        rejimOffButton.Foreground = Brushes.White;
                        rejimAutoButton.Background = Brushes.Green;
                        rejimAutoButton.Foreground = Brushes.White;
                    }
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

            int mode = (int)tag.ValueReal;

            // У EM фактически два режима на панели: OFF и AUTO
            OperationMode currentOperationMode = mode == 0
                ? OperationMode.Off
                : OperationMode.Auto;

            if (EmModePanel.CurrentMode != currentOperationMode)
            {
                EmModePanel.SetMode(currentOperationMode);
            }
        }

        private void UpdatePanelsVisibility()
        {
            try
            {
                var rejimTag = _global?.Variables?.GetByName("EM_Rejim");

                // ModePanel всегда виден
                if (EmModePanel != null)
                    EmModePanel.Visibility = Visibility.Visible;

                if (rejimTag == null)
                {
                    if (StartupPanelControl != null)
                        StartupPanelControl.Visibility = Visibility.Collapsed;

                    if (PerformancePanelControl != null)
                        PerformancePanelControl.Visibility = Visibility.Collapsed;

                    if (UnloadPanelControl != null)
                        UnloadPanelControl.Visibility = Visibility.Collapsed;

                    return;
                }

                double rejimValue = rejimTag.ValueReal;

                // Управление видимостью панелей
                // В старой версии: RejimAutoPanel.Visible = (rejimValue != 0)
                bool isOff = rejimValue == 0;

                // Остальные панели скрываем в режиме OFF
                if (StartupPanelControl != null)
                    StartupPanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                if (PerformancePanelControl != null)
                    PerformancePanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                if (UnloadPanelControl != null)
                    UnloadPanelControl.Visibility = isOff ? Visibility.Collapsed : Visibility.Visible;

                // Для отладки
                System.Diagnostics.Debug.WriteLine($"EM режим: {rejimValue}, панели видимы: {!isOff}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления видимости панелей EM: {ex.Message}");
            }
        }

        private void InitializePanels()
        {
            try
            {
                // Установка глобального объекта для панелей (если у них есть свойство Global)
                if (StartupPanelControl != null)
                {
                    // Проверяем, есть ли свойство Global у StartupPanel
                    var globalProperty = StartupPanelControl.GetType().GetProperty("Global");
                    if (globalProperty != null && _global != null)
                    {
                        globalProperty.SetValue(StartupPanelControl, _global);
                    }

                    StartupPanelControl.StartStartupButtonClick += StartupPanel_StartStartupButtonClick;
                    StartupPanelControl.StopStartupButtonClick += StartupPanel_StopStartupButtonClick;
                    StartupPanelControl.AutoModeButtonClick += StartupPanel_AutoModeButtonClick;
                    StartupPanelControl.OffModeButtonClick += StartupPanel_OffModeButtonClick;
                }

                if (PerformancePanelControl != null)
                {
                    var globalProperty = PerformancePanelControl.GetType().GetProperty("Global");
                    if (globalProperty != null && _global != null)
                    {
                        globalProperty.SetValue(PerformancePanelControl, _global);
                    }

                    PerformancePanelControl.SetMassFlowButtonClick += PerformancePanel_SetMassFlowButtonClick;
                    PerformancePanelControl.StartProcessButtonClick += PerformancePanel_StartProcessButtonClick;
                    PerformancePanelControl.StopProcessButtonClick += PerformancePanel_StopProcessButtonClick;
                    PerformancePanelControl.DojatProcessButtonClick += PerformancePanel_DojatProcessButtonClick;
                    PerformancePanelControl.EmergencyStopButtonClick += PerformancePanel_EmergencyStopButtonClick;
                }

                if (UnloadPanelControl != null)
                {
                    var globalProperty = UnloadPanelControl.GetType().GetProperty("Global");
                    if (globalProperty != null && _global != null)
                    {
                        globalProperty.SetValue(UnloadPanelControl, _global);
                    }

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

        // ========== ИНИЦИАЛИЗАЦИЯ ЭЛЕМЕНТОВ ==========
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

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

        private void M150Mixer_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера T150", 1);
                SendCommand("T150_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера T150", 1);
                SendCommand("T150_StartMixer", "false");
            }
        }

        private void M250Mixer_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение миксера M250", 1);
                SendCommand("M250_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M250", 1);
                SendCommand("M250_StartMixer", "false");
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ MODEPANEL ==========
        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            // Этот метод вызывается, когда пользователь меняет режим в ModePanel
            if (_global == null) return;

            try
            {
                string commandName = mode switch
                {
                    OperationMode.Off => "EM_RejimToOff",
                    OperationMode.SemiAuto => "EM_RejimToAuto",
                    OperationMode.Auto => "EM_RejimToAuto",
                    _ => "EM_RejimToOff"
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

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ПАНЕЛЕЙ ==========
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

        private void SendCommand(string commandName, string value)
        {
            try
            {
                var command = _global?.Commands?.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = value;
                    command.NeedToWrite = true;
                    command.SendToController();
                }
            }
            catch { }
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
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
                // СНАЧАЛА инициализируем глобальный объект и Modbus
                if (_global != null)
                {
                    await _global.InitializeModbusAsync();
                }

                // ПОТОМ инициализируем элементы
                InitializeElements();
                InitializePanels();
                SubscribeToEvents();

                // И ТОЛЬКО ПОТОМ запускаем таймер
                StartUpdateTimer();

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("FrameEmPage успешно инициализирован");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации FrameEmPage: {ex.Message}");
            }
        }

        private void InitializeElements()
        {
            try
            {
                // ========== АНАЛОГОВЫЕ ДАТЧИКИ ==========

                // TT152
                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");

                // TT252
                InitializeSensor(TT252, "TT252", "Датчик температуры TT-252", "TT-252", "°C");

                // TT602
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");

                // LT150
                InitializeSensor(LT150, "LT150", "Датчик уровня LT150", "LT-150", "мм");

                // FM601
                InitializeSensor(FM601, "FM601", "Массовый расходомер FM601", "FM601", "кг/ч");

                // PT601
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "бар");

                // PT606
                InitializeSensor(PT606, "PT606", "Датчик давления PT606", "PT-606", "бар");

                // LT253
                InitializeSensor(LT253, "LT253", "Датчик уровня LT253", "LT-253", "мм");

                // FM602
                InitializeSensor(FM602, "FM602", "Массовый расходомер FM602", "FM602", "кг/ч");

                // LT651
                InitializeSensor(LT651, "LT651", "Датчик уровня LT651", "LT-651", "мм");

                // PT652
                InitializeSensor(PT652, "PT652", "Датчик давления PT652", "PT-652", "бар");

                // PT604
                InitializeSensor(PT604, "PT604", "Датчик давления PT604", "PT-604", "бар");

                // ========== ДИСКРЕТНЫЕ ДАТЧИКИ ==========

                // LAHH151
                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");

                // LALL153
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");

                // LAHH251
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");

                // LAHH653
                InitializeDiscreteSensor(LAHH653, "LAHH653", "Датчик уровня LAHH653", "LAHH-653");

                // ========== МИКСЕРЫ ==========

                // M150
                InitializeToggleSwitch(M150, "M150", "Миксер M150");

                // M250
                InitializeToggleSwitch(M250, "M250", "Миксер M250");

                // ========== НАСОСЫ ==========

                // P601
                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601");

                // P602
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602");

                // M600
                InitializePumpUz(M600, "M600", "Миксер M-600");

                // P651
                InitializePumpUzUnderPanel(P651, "P651", "Насос P-651");

                // P700
                //InitializePumpH(P700, "P700", "Насос P-700");

                // ========== КЛАПАНЫ ==========

                // VT601
                Initialize3Valve(SV601, "V601", "Клапан SV-601");

                // VT602
                Initialize3Valve(SV602, "V602", "Клапан SV-602");

                // VT505
                InitializeValve(V505, "V505", "Клапан V-505");

                System.Diagnostics.Debug.WriteLine("Элементы управления успешно инициализированы");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов: {ex.Message}");
            }
        }

        private void InitializeSensor(Element_AI sensor, string varName, string description, string name, string eu)
        {
            try
            {
                if (sensor != null && _global != null)
                {
                    sensor.Global = _global;
                    sensor.VarName = varName;
                    sensor.Description = description;
                    sensor.Name = name;
                    sensor.EU = eu;
                    System.Diagnostics.Debug.WriteLine($"Инициализирован датчик: {varName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Не удалось инициализировать датчик: {varName}, sensor={sensor != null}, global={_global != null}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации датчика {varName}: {ex.Message}");
            }
        }

        private void InitializeDiscreteSensor(Element_DI sensor, string varName, string description, string name)
        {
            try
            {
                if (sensor != null && _global != null)
                {
                    sensor.Global = _global;
                    sensor.VarName = varName;
                    sensor.Description = description;
                    sensor.Name = name;  // ← Добавлено
                    System.Diagnostics.Debug.WriteLine($"Инициализирован дискретный датчик: {name} ({varName})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации дискретного датчика {varName}: {ex.Message}");
            }
        }

        private void InitializeToggleSwitch(ToggleSwitch toggle, string varName, string description)
        {
            try
            {
                if (toggle != null && _global != null)
                {
                    toggle.Tag = varName; // Сохраняем имя переменной в Tag
                    toggle.StateChanged += MixerToggle_StateChanged;
                    System.Diagnostics.Debug.WriteLine($"Инициализирован переключатель: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации переключателя {varName}: {ex.Message}");
            }
        }

        private void InitializePumpUz(Element_PumpUz pump, string varName, string description)
        {
            try
            {
                if (pump != null && _global != null)
                {
                    pump.Global = _global;
                    pump.VarName = varName;
                    pump.Description = description;
                    // Добавляем обработчик клика
                    //pump.MouseDown += (s, e) => OpenPumpDialog(description, varName);
                    System.Diagnostics.Debug.WriteLine($"Инициализирован насос: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации насоса {varName}: {ex.Message}");
            }
        }

        private void InitializePumpH(Element_PumpH pump, string varName, string description)
        {
            try
            {
                if (pump != null && _global != null)
                {
                    pump.Global = _global;
                    pump.VarName = varName;
                    pump.Description = description;
                    // Добавляем обработчик клика
                    //pump.MouseDown += (s, e) => OpenPumpDialog(description, varName);
                    System.Diagnostics.Debug.WriteLine($"Инициализирован насос P700: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации насоса {varName}: {ex.Message}");
            }
        }

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump, string varName, string description)
        {
            try
            {
                if (pump != null && _global != null)
                {
                    pump.Global = _global;
                    pump.VarName = varName;
                    pump.Description = description;
                    // Добавляем обработчик клика
                    //pump.MouseDown += (s, e) => OpenPumpDialog(description, varName);
                    System.Diagnostics.Debug.WriteLine($"Инициализирован насос: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации насоса {varName}: {ex.Message}");
            }
        }

        private void Initialize3Valve(Element_3ValveH valve, string varName, string description)
        {
            try
            {
                if (valve != null && _global != null)
                {
                    valve.Global = _global;
                    valve.VarName = varName;
                    valve.Description = description;
                    // Добавляем обработчик клика
                    //valve.MouseDown += (s, e) => OpenValveDialog(description, varName);
                    System.Diagnostics.Debug.WriteLine($"Инициализирован клапан: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации клапана {varName}: {ex.Message}");
            }
        }

        private void InitializeValve(Element_ValveV valve, string varName, string description)
        {
            try
            {
                if (valve != null && _global != null)
                {
                    valve.Global = _global;
                    valve.VarName = varName;
                    valve.Description = description;
                    // Добавляем обработчик клика
                    //valve.MouseDown += (s, e) => OpenValveDialog(description, varName);
                    System.Diagnostics.Debug.WriteLine($"Инициализирован клапан V505: {varName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации клапана {varName}: {ex.Message}");
            }
        }

        private void InitializePanels()
        {
            try
            {
                // Инициализация панели запуска
                if (StartupPanelControl != null)
                {
                    StartupPanelControl.Global = _global;
                    SubscribeToStartupPanelEvents();
                    System.Diagnostics.Debug.WriteLine("Инициализирована панель запуска");
                }

                // Инициализация панели производительности
                if (PerformancePanelControl != null)
                {
                    PerformancePanelControl.Global = _global;
                    SubscribeToPerformancePanelEvents();
                    System.Diagnostics.Debug.WriteLine("Инициализирована панель производительности");
                }

                // Инициализация панели отгрузки
                if (UnloadPanelControl != null)
                {
                    UnloadPanelControl.Global = _global;
                    SubscribeToUnloadPanelEvents();
                    System.Diagnostics.Debug.WriteLine("Инициализирована панель отгрузки");
                }

                // Инициализация панели режима
                if (EmModePanel != null)
                {
                    EmModePanel.SetMode(OperationMode.Off);
                    SubscribeToModePanelEvents();
                    System.Diagnostics.Debug.WriteLine("Инициализирована панель режима");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации панелей: {ex.Message}");
            }
        }

        private void SubscribeToEvents()
        {
            try
            {
                if (_global != null)
                {
                    _global.OnVariablesUpdated += OnGlobalVariablesUpdated;
                    System.Diagnostics.Debug.WriteLine("Подписка на события глобального объекта установлена");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка подписки на события: {ex.Message}");
            }
        }

        private void OnGlobalVariablesUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    UpdateUIFromVariables();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка в OnGlobalVariablesUpdated: {ex.Message}");
                }
            });
        }

        private void StartUpdateTimer()
        {
            try
            {
                _updateTimer = new DispatcherTimer();
                _updateTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 Гц как в старом проекте
                _updateTimer.Tick += (sender, e) =>
                {
                    try
                    {
                        UpdateUIFromVariables();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка в таймере обновления: {ex.Message}");
                    }
                };
                _updateTimer.Start();
                System.Diagnostics.Debug.WriteLine("Таймер обновления запущен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска таймера: {ex.Message}");
            }
        }

        private void UpdateUIFromVariables()
        {
            try
            {
                if (!_isInitialized || _global == null || !Dispatcher.CheckAccess())
                {
                    return;
                }

                // 1. Обновление всех элементов
                UpdateAllElements();

                // 2. Обновляем режим работы EM
                UpdateRejimStatus();

                // 3. Обновляем режим отгрузки и управление панелью отгрузки
                UpdateUnloadStatus();

                // 4. Обновляем данные в панелях управления
                UpdateControlPanels();

                // 5. Обновление состояния переключателей миксеров
                UpdateMixerTogglesFromVariables();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления UI: {ex.Message}");
            }
        }

        private void UpdateAllElements()
        {
            try
            {
                // Обновляем аналоговые датчики с проверкой
                SafeUpdateElement(TT152);
                SafeUpdateElement(TT252);
                SafeUpdateElement(TT602);
                SafeUpdateElement(LT150);
                SafeUpdateElement(FM601);
                SafeUpdateElement(PT601);
                SafeUpdateElement(PT606);
                SafeUpdateElement(LT253);
                SafeUpdateElement(FM602);
                SafeUpdateElement(LT651);
                SafeUpdateElement(PT652);
                SafeUpdateElement(PT604);

                // Обновляем дискретные датчики
                SafeUpdateElement(LAHH151);
                SafeUpdateElement(LALL153);
                SafeUpdateElement(LAHH251);
                SafeUpdateElement(LAHH653);

                // Обновляем насосы
                SafeUpdateElement(P601);
                SafeUpdateElement(P602);
                SafeUpdateElement(M600);
                SafeUpdateElement(P651);
                //SafeUpdateElement(P700);

                // Обновляем клапаны
                SafeUpdateElement(SV601);
                SafeUpdateElement(SV602);
                SafeUpdateElement(V505);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateAllElements: {ex.Message}");
            }
        }

        private void SafeUpdateElement(dynamic element)
        {
            try
            {
                if (element != null)
                {
                    element.UpdateElement();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления элемента: {ex.Message}");
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

        private void UpdateRejimStatus()
        {
            try
            {
                if (_global?.Variables == null) return;

                var rejimTag = _global.Variables.GetByName("EM_Rejim");
                if (rejimTag != null)
                {
                    int rejimValue = (int)rejimTag.ValueReal;

                    // Управление видимостью панелей как в старом проекте
                    bool showControlPanels = rejimValue != 0;

                    if (StartupPanelControl != null)
                        StartupPanelControl.Visibility = showControlPanels ? Visibility.Visible : Visibility.Collapsed;

                    if (PerformancePanelControl != null)
                        PerformancePanelControl.Visibility = showControlPanels ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления режима: {ex.Message}");
            }
        }

        private void UpdateUnloadStatus()
        {
            try
            {
                if (_global?.Variables == null || UnloadPanelControl == null) return;

                var unloadRejimTag = _global.Variables.GetByName("EM_Unloading_Rejim");
                if (unloadRejimTag != null)
                {
                    string rejimValue = unloadRejimTag.ValueString;
                    UnloadPanelControl.UpdateMode(rejimValue);
                    UnloadPanelControl.Visibility = Visibility.Visible;
                }
                else
                {
                    UnloadPanelControl.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления статуса отгрузки: {ex.Message}");
            }
        }

        private void UpdateControlPanels()
        {
            try
            {
                StartupPanelControl?.UpdateFromGlobal();
                PerformancePanelControl?.UpdateFromGlobal();
                UnloadPanelControl?.UpdateFromGlobal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления панелей управления: {ex.Message}");
            }
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

        // Обработчики событий панелей

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

        // Обработчики для миксеров
        private void MixerToggle_StateChanged(object sender, bool isChecked)
        {
            try
            {
                if (_global == null || sender == null) return;

                var toggle = sender as ToggleSwitch;
                if (toggle == null) return;

                string varName = toggle.Tag as string;
                if (string.IsNullOrEmpty(varName)) return;

                if (varName == "M150")
                {
                    if (isChecked)
                    {
                        _global.Log.Add("Пользователь", "Включение миксера M150", 1);
                        SendCommandToController("T150_StartMixer", "true");
                    }
                    else
                    {
                        _global.Log.Add("Пользователь", "Отключение миксера M150", 1);
                        SendCommandToController("T150_StartMixer", "false");
                    }
                }
                else if (varName == "M250")
                {
                    if (isChecked)
                    {
                        _global.Log.Add("Пользователь", "Включение миксера M250", 1);
                        SendCommandToController("M250_StartMixer", "true");
                    }
                    else
                    {
                        _global.Log.Add("Пользователь", "Отключение миксера M250", 1);
                        SendCommandToController("M250_StartMixer", "false");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в обработчике миксеров: {ex.Message}");
            }
        }

        // Методы для открытия диалогов
        private void OpenPumpDialog(string title, string varName)
        {
            try
            {
                var dialog = new DialogElementPumpUz();
                dialog.Title = title;
                dialog.Global = _global;
                dialog.VarName = varName;
                dialog.Initialize();

                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    dialog.Owner = parentWindow;
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога насоса: {ex.Message}");
            }
        }

        private void OpenValveDialog(string title, string varName)
        {
            try
            {
                var dialog = new DialogElementValve();
                dialog.Title = title;
                dialog.Global = _global;
                dialog.VarName = varName;
                dialog.Initialize();

                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    dialog.Owner = parentWindow;
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога клапана: {ex.Message}");
            }
        }

        private void SendCommandToController(string commandName, string value)
        {
            try
            {
                if (_global?.Commands == null) return;

                TCommandTag command = _global.Commands.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = value;
                    command.NeedToWrite = true;
                    command.SendToController();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки команды {commandName}: {ex.Message}");
            }
        }

        public void UserControl_Initialized(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FrameEmPage инициализирован (UserControl_Initialized)");
        }

        public void Cleanup()
        {
            _updateTimer?.Stop();
            _global?.DisconnectAll();
        }

        private void P651_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PT652_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ImageEmPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LAHH653_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ImageEmPage_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void FM601_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
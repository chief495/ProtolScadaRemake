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
        private bool _isInitialized = false;

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
            InitializeTimer();
        }

        private void FrameEmPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized && _global != null)
            {
                InitializeElements();
                InitializeTimer();
                _isInitialized = true;
            }
            _repaintTimer?.Start();
        }

        private void FrameEmPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _repaintTimer?.Stop();
        }

        private void InitializeElements()
        {
            try
            {
                // ========== АНАЛОГОВЫЕ ДАТЧИКИ ==========

                InitializeSensor(TT152, "TT152", "Датчик температуры TT-152", "TT-152", "°C");
                InitializeSensor(TT252, "TT252", "Датчик температуры TT-252", "TT-252", "°C");
                InitializeSensor(TT602, "TT602", "Датчик температуры TT-602", "TT-602", "°C");
                InitializeSensor(LT150, "LT150", "Датчик уровня LT150", "LT-150", "мм");
                InitializeSensor(FM601, "FM601", "Массовый расходомер FM601", "FM601", "кг/ч");
                InitializeSensor(PT601, "PT601", "Датчик давления PT601", "PT-601", "бар");
                InitializeSensor(PT606, "PT606", "Датчик давления PT606", "PT-606", "бар");
                InitializeSensor(LT253, "LT253", "Датчик уровня LT253", "LT-253", "мм");
                InitializeSensor(FM602, "FM602", "Массовый расходомер FM602", "FM602", "кг/ч");
                InitializeSensor(LT651, "LT651", "Датчик уровня LT651", "LT-651", "мм");
                InitializeSensor(PT652, "PT652", "Датчик давления PT652", "PT-652", "бар");
                InitializeSensor(PT604, "PT604", "Датчик давления PT604", "PT-604", "бар");

                // ========== ДИСКРЕТНЫЕ ДАТЧИКИ ==========

                InitializeDiscreteSensor(LAHH151, "LAHH151", "Датчик уровня LAHH151", "LAHH-151");
                InitializeDiscreteSensor(LALL153, "LALL153", "Датчик уровня LALL153", "LALL-153");
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");
                InitializeDiscreteSensor(LAHH653, "LAHH653", "Датчик уровня LAHH653", "LAHH-653");

                // ========== МИКСЕРЫ ==========

                M150.StateChanged += M150Mixer_StateChanged;
                M250.StateChanged += M250Mixer_StateChanged;

                // ========== НАСОСЫ ==========

                InitializePumpUzUnderPanel(P601, "P601", "Насос P-601");
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602");
                InitializePumpUz(M600, "M600", "Миксер M-600");
                InitializePumpUzUnderPanel(P651, "P651", "Насос P-651");

                // ========== КЛАПАНЫ ==========

                Initialize3Valve(SV601, "V601", "Клапан SV-601");
                Initialize3Valve(SV602, "V602", "Клапан SV-602");
                InitializeValve(V505, "V505", "Клапан V-505");

                // Инициализация панелей управления
                InitializePanels();

                System.Diagnostics.Debug.WriteLine("Элементы управления EM успешно инициализированы");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов EM: {ex.Message}");
            }
        }

        private void InitializeTimer()
        {
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;
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

                // 4. Обновление видимости панелей по режиму
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

                // Сброс команды включения миксера Т-150 как в старом проекте
                var command = _global.Commands.GetByName("T150_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения миксера Т-250
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления элементов EM: {ex.Message}");
            }
        }

        private void UpdatePanelInfo()
        {
            try
            {
                // Простая реализация - обновление через публичные свойства
                // Проверьте, какие свойства есть у ваших панелей
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления панелей EM: {ex.Message}");
            }
        }

        private void UpdatePanelsVisibility()
        {
            try
            {
                var rejimTag = _global?.Variables?.GetByName("EM_Rejim");
                if (rejimTag != null)
                {
                    double rejimValue = rejimTag.ValueReal;

                    // Управление видимостью панелей как в старом проекте
                    if (rejimValue == 0) // OFF
                    {
                        // Скрыть панели управления
                        if (StartupPanelControl != null)
                            StartupPanelControl.Visibility = Visibility.Collapsed;
                        if (PerformancePanelControl != null)
                            PerformancePanelControl.Visibility = Visibility.Collapsed;
                    }
                    else // Любой другой режим
                    {
                        // Показать панели управления
                        if (StartupPanelControl != null)
                            StartupPanelControl.Visibility = Visibility.Visible;
                        if (PerformancePanelControl != null)
                            PerformancePanelControl.Visibility = Visibility.Visible;
                    }
                }
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
                // Установка глобального объекта для панелей
                if (StartupPanelControl != null)
                    StartupPanelControl.Global = _global;

                if (PerformancePanelControl != null)
                    PerformancePanelControl.Global = _global;

                if (UnloadPanelControl != null)
                    UnloadPanelControl.Global = _global;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации панелей: {ex.Message}");
            }
        }

        // ========== ИНИЦИАЛИЗАЦИЯ ЭЛЕМЕНТОВ ==========
        private void InitializeSensor(Element_AI sensor, string varName, string description, string name, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.Name = name;
                sensor.EU = eu;
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
            }
        }

        private void InitializePumpUz(Element_PumpUz pump, string varName, string description)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
            }
        }

        private void InitializePumpUzUnderPanel(Element_PumpUzUnderPanel pump, string varName, string description)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
            }
        }

        private void Initialize3Valve(Element_3ValveH valve, string varName, string description)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
            }
        }

        private void InitializeValve(Element_ValveV valve, string varName, string description)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
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

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ПАНЕЛЕЙ ==========

        // Обработчики для StartupPanel
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

        // Обработчики для PerformancePanel
        private void PerformancePanel_SetMassFlowButtonClick(object sender, RoutedEventArgs e)
        {
            // Логика установки производительности
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

        // Обработчики для UnloadPanel
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

        // Обработчики панели режимов
        private void EmModePanel_ModeChanged(object sender, OperationMode mode)
        {
            System.Diagnostics.Debug.WriteLine($"Режим EM изменен на: {mode}");
        }

        private void EmModePanel_ModbusCommandRequested(object sender, ModbusCommandEventArgs e)
        {
            // Логика Modbus команд
        }

        // ========== СЛУЖЕБНЫЕ МЕТОДЫ ==========
        private void P651_Loaded(object sender, RoutedEventArgs e) { }
        private void PT652_Loaded(object sender, RoutedEventArgs e) { }
        private void ImageEmPage_Loaded(object sender, RoutedEventArgs e) { }
        private void LAHH653_Loaded(object sender, RoutedEventArgs e) { }
        private void ImageEmPage_Loaded_1(object sender, RoutedEventArgs e) { }
        private void FM601_Loaded(object sender, RoutedEventArgs e) { }
        private void UserControl_Initialized(object sender, EventArgs e) { }
    }
}
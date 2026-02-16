using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

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
            InitializeElements();
            InitializeTimer();

            UpdatePanelsVisibility();
            // Настройка таймера обновления (10 Гц как в старом проекте)
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100);
            _repaintTimer.Tick += RepaintTimer_Tick;
        }

        private void FrameTcPage_Loaded(object sender, RoutedEventArgs e)
        {
            _repaintTimer?.Start();
        }

        private void FrameTcPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _repaintTimer?.Stop();
        }

        private void InitializeElements()
        {
            try
            {
                // Инициализация датчиков
                InitializeSensor(PT205, "PT205", "Датчик протока PT205", "PT205", "л/ч");
                InitializeSensor(PT206, "PT206", "Датчик протока PT206", "PT206", "л/ч");
                InitializeSensor(PT201, "PT201", "Датчик давления PT201", "PT201", "бар");
                InitializeSensor(LT253, "LT253", "Датчик уровня LT253", "LT253", "%");
                InitializeSensor(TT202, "TT202", "Датчик температуры TT202", "TT202", "°C");
                InitializeSensor(TT252, "TT252", "Датчик температуры TT252", "TT252", "°C");
                InitializeSensor(FM602, "FM602", "Массовый расходомер FM602", "FM602", "кг/ч");

                // Датчик веса WIT200
                InitializeSensor(WIT200, "WIT200_Volume", "Датчик веса WIT200", "WIT200", "кг");

                // Инициализация дискретных датчиков
                InitializeDiscreteSensor(LAHH201, "LAHH201", "Датчик уровня LAHH201", "LAHH-201");
                InitializeDiscreteSensor(LAHH251, "LAHH251", "Датчик уровня LAHH251", "LAHH-251");

                // Инициализация насосов
                InitializePumpH(P200, "P200", "Насос P-200", "P-200");
                InitializePumpH(P201, "P201", "Насос P-201", "P-201");
                InitializePumpH(P202, "P202", "Насос P-202", "P-202");
                InitializePumpUzUnderPanel(P602, "P602", "Насос P-602", "P-602");

                // Инициализация клапанов
                Initialize3Valve(VT602, "V602", "Клапан SV-602", "SV-602");
                InitializeValveH(VT801, "V801", "Клапан V-801", "V-801");
                InitializeValveH(VT803, "V803", "Клапан V-803", "V-803");

                // Инициализация нагревателя
                InitializeHeater(HE800, "HE800", "Нагреватель HE-800");

                // Инициализация миксеров
                M200MixerToggle.StateChanged += M200MixerToggle_StateChanged;
                M250MixerToggle.StateChanged += M250MixerToggle_StateChanged;

                // Инициализация переключателя нагревателя
                HE800Toggle.StateChanged += HE800Toggle_StateChanged;

                // Инициализация обработчиков для панели режимов
                InitializeModePanelHandlers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов: {ex.Message}");
            }
        }

        private void InitializeTimer()
        {
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 Гц
            _repaintTimer.Tick += RepaintTimer_Tick;
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            _repaintTimer.Stop();

            try
            {
                // 1. Обновление всех элементов
                UpdateAllElements();

                // 2. Сброс команд как в старом проекте
                ResetCommands();

                // 3. Обновление информации на панелях
                UpdatePanelInfo();

                // 4. Обновление видимости панелей по режиму
                UpdatePanelsVisibility();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления: {ex.Message}");
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

                // Сброс команды включения миксера Т-200
                var command = _global.Commands.GetByName("M200_StartMixer");
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

            }
            catch { }
        }

        private void UpdateAllElements()
        {
            // Обновляем все элементы управления
            PT205?.UpdateElement();
            PT206?.UpdateElement();
            PT201?.UpdateElement();
            LT253?.UpdateElement();
            TT202?.UpdateElement();
            TT252?.UpdateElement();
            FM602?.UpdateElement();
            WIT200?.UpdateElement();

            LAHH201?.UpdateElement();
            LAHH251?.UpdateElement();

            P200?.UpdateElement();
            P201?.UpdateElement();
            P202?.UpdateElement();
            P602?.UpdateElement();

            VT602?.UpdateElement();
            VT801?.UpdateElement();
            VT803?.UpdateElement();

            // Обновляем нагреватель
            HE800?.UpdateElement();
        }

        private void UpdatePanelInfo()
        {
            try
            {
                // Обновление текущей массы в Т-200
                var massTag = _global?.Variables?.GetByName("WIT200_Volume");
                if (massTag != null)
                {
                    T200CurrentMassLabel.Text = $"Текущая: {massTag.ValueString} кг";
                }

                // Обновление статуса набора топлива
                UpdateFuelStatus();

                // Обновление счетчиков как в старом проекте
                var manualDiselTag = _global?.Variables?.GetByName("TC_ManualDiselCurrent");
                if (manualDiselTag != null)
                {
                    // Если есть поля для отображения в панели ручного режима
                    var diselCounterEdit = TcModePanel?.FindName("TC_ManualDiselCounterEdit") as TextBox;
                    if (diselCounterEdit != null)
                        diselCounterEdit.Text = manualDiselTag.ValueString;
                }

                var manualEmulgatorTag = _global?.Variables?.GetByName("TC_ManualEmulgatorCurrent");
                if (manualEmulgatorTag != null)
                {
                    var emulgatorCounterEdit = TcModePanel?.FindName("TC_ManualEmulgatorCounterEdit") as TextBox;
                    if (emulgatorCounterEdit != null)
                        emulgatorCounterEdit.Text = manualEmulgatorTag.ValueString;
                }

                // Обновление статусов авторежима как в старом проекте
                var autoDiselSpTag = _global?.Variables?.GetByName("TC_AutoDiselSp");
                var autoDiselCurrentTag = _global?.Variables?.GetByName("TC_AutoDiselCurrent");

                if (autoDiselSpTag != null && autoDiselCurrentTag != null)
                {
                    var diselStatusLabel = TcModePanel?.FindName("DiselStatusLabel") as TextBlock;
                    if (diselStatusLabel != null)
                        diselStatusLabel.Text = "Диз.топливо: " + autoDiselCurrentTag.ValueString + " из " + autoDiselSpTag.ValueString;
                }

                var autoEmulgatorSpTag = _global?.Variables?.GetByName("TC_AutoEmulgatorSp");
                var autoEmulgatorCurrentTag = _global?.Variables?.GetByName("TC_AutoEmulgatorCurrent");

                if (autoEmulgatorSpTag != null && autoEmulgatorCurrentTag != null)
                {
                    var emulgatorStatusLabel = TcModePanel?.FindName("EmulgatorStatusLabel") as TextBlock;
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
                if (rejimTag != null)
                {
                    double rejimValue = rejimTag.ValueReal;

                    // Устанавливаем видимость по умолчанию
                    Visibility defaultVisibility = Visibility.Visible;

                    // ВАЖНО: Явно устанавливаем видимость всех панелей в каждом случае
                    if (rejimValue == 0) // OFF
                    {
                        FuelPanel.Visibility = Visibility.Collapsed;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        // Также установите IsEnabled если нужно
                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 1) // Полуавтомат
                    {
                        FuelPanel.Visibility = Visibility.Visible;
                        T200MassPanel.Visibility = Visibility.Visible;
                        T100toT150Panel.Visibility = Visibility.Visible;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = true;
                    }
                    else if (rejimValue == 2) // Автомат
                    {
                        FuelPanel.Visibility = Visibility.Collapsed;
                        T200MassPanel.Visibility = Visibility.Visible;
                        T100toT150Panel.Visibility = Visibility.Visible;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = true;
                    }
                    else if (rejimValue == 3) // Полуавтомат - набор дизеля
                    {
                        FuelPanel.Visibility = Visibility.Visible;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 4) // Полуавтомат - набор дизеля. Пауза
                    {
                        FuelPanel.Visibility = Visibility.Visible;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 5) // Полуавтомат - набор эмульгатора
                    {
                        FuelPanel.Visibility = Visibility.Visible;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 6) // Полуавтомат - набор эмульгатора. Пауза
                    {
                        FuelPanel.Visibility = Visibility.Visible;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 7 || rejimValue == 8 ||
                             rejimValue == 9 || rejimValue == 10) // Авто режимы
                    {
                        FuelPanel.Visibility = Visibility.Collapsed;
                        T200MassPanel.Visibility = Visibility.Collapsed;
                        T100toT150Panel.Visibility = Visibility.Collapsed;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = false;
                    }
                    else if (rejimValue == 11) // Полуавтомат - перекачка
                    {
                        FuelPanel.Visibility = Visibility.Collapsed;
                        T200MassPanel.Visibility = Visibility.Visible;
                        T100toT150Panel.Visibility = Visibility.Visible;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = true;
                    }
                    else if (rejimValue == 12) // Автомат - перекачка
                    {
                        FuelPanel.Visibility = Visibility.Collapsed;
                        T200MassPanel.Visibility = Visibility.Visible;
                        T100toT150Panel.Visibility = Visibility.Visible;

                        if (T100toT150Panel != null)
                            T100toT150Panel.IsEnabled = true;
                    }

                    // Для отладки добавьте логирование
                    System.Diagnostics.Debug.WriteLine($"Режим: {rejimValue}, " +
                        $"FuelPanel: {FuelPanel.Visibility}, " +
                        $"T200MassPanel: {T200MassPanel.Visibility}, " +
                        $"T100toT150Panel: {T100toT150Panel.Visibility}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления видимости панелей: {ex.Message}");
            }
        }

        // ========== ИНИЦИАЛИЗАЦИЯ ЭЛЕМЕНТОВ ==========
        private void InitializePumpH(Element_PumpH pump, string varName, string description, string name)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.Name = name;
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
            }
        }

        private void Initialize3Valve(Element_3ValveH valve, string varName, string description,string name)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.Name = name;
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
            }
        }

        private void InitializeHeater(Element_Heater heater, string varName, string description)
        {
            if (heater != null && _global != null)
            {
                heater.Global = _global;
                heater.VarName = varName;
                heater.Description = description;
            }
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
                SendCommand("M200_StartMixer", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение миксера M200", 1);
                SendCommand("M200_StartMixer", "false");
            }
        }

        private void M250MixerToggle_StateChanged(object sender, bool isChecked)
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

        private void HE800Toggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            if (isChecked)
            {
                _global.Log.Add("Пользователь", "Включение нагревателя HE800", 1);
                SendCommand("HE800_StartHeater", "true");
            }
            else
            {
                _global.Log.Add("Пользователь", "Отключение нагревателя HE800", 1);
                SendCommand("HE800_StartHeater", "false");
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
                        SendCommand("TC_ManualDiselSp", mass.ToString());
                        SendCommand("TC_ManualStartDisel", "true");
                    }
                }
                else if (EmulgatorRadio.IsChecked == true)
                {
                    // Набор эмульгатора
                    if (double.TryParse(FuelMassEdit.Text, out double mass))
                    {
                        SendCommand("TC_ManualEmulgatorSp", mass.ToString());
                        SendCommand("TC_ManualStartEmulgator", "true");
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
            SendCommand("TC_ManualStop", "true");
        }

        private void FuelPauseButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_ManualPause", "true");
        }

        // Обработчики панели массы в Т-200
        private void T200SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                if (double.TryParse(T200MassSetEdit.Text, out double mass))
                {
                    SendCommand("TC_AutoMassSp", mass.ToString());
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
            SendCommand("TC_AutoMassSp", "0");
        }

        // Обработчики панели перекачки
        private void TransferStartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_TransportStart", "true");
        }

        private void TransferStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_TransportStop", "true");
        }

        // Обработчики для кнопок панели режимов
        private void RejimOffButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_RejimToOff", "true");
        }

        private void RejimManualButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_RejimToManual", "true");
        }

        private void RejimAutoButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_RejimToAuto", "true");
        }

        private void ManualStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Этот метод уже есть для FuelStartButton, но нужен и для панели ModePanel
            var manualStartButton = sender as Button;
            if (manualStartButton != null)
            {
                FuelStartButton_Click(sender, e);
            }
        }

        private void ManualStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_ManualStop", "true");
        }

        private void ManualPauseButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_ManualPause", "true");
        }

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Аналогично T200SetButton_Click
            if (_global == null) return;

            try
            {
                var autoMassSpEdit = TcModePanel?.FindName("TCAutoMassSpEdit") as TextBox;
                if (autoMassSpEdit != null && double.TryParse(autoMassSpEdit.Text, out double mass))
                {
                    SendCommand("TC_AutoMassSp", mass.ToString());
                    SendCommand("TC_AutolStart", "true");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска авторежима: {ex.Message}");
            }
        }

        private void AutoStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_AutoStop", "true");
        }

        private void AutoPauseButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TC_AutoPause", "true");
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
    }
}
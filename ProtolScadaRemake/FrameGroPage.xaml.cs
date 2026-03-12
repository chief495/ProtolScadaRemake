using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameGroPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        // -----------------------------------------------------------------
        // Поля, отвечающие за логику режима работы
        // -----------------------------------------------------------------
        private DateTime _lastModeChangeRequest = DateTime.MinValue; // когда отправлен последний запрос
        private DateTime _lastModeRetrySent = DateTime.MinValue; // когда отправлен последний ретрай
        private OperationMode? _pendingModeRequest;               // запрос, который надо повторять, если PLC «завис»

        // ← **Новые поля** (они и вызывали ошибки CS0103)
        private OperationMode? _pendingRequestedMode;   // запрос, который пользователь уже совершил и который нужно отобразить в UI
        private DateTime _pendingRequestedModeAt = DateTime.MinValue; // момент, когда запрос был сделан
        // -----------------------------------------------------------------

        // Словари и цвета – их объявление оставляем без изменений
        // ...

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

            // Настройка таймера обновления (10 Гц как в старом проекте)
            _repaintTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _repaintTimer.Tick += RepaintTimer_Tick;

            UpdatePanelsVisibility();

            // Подписка на события
            SubscribeToEvents();

            System.Diagnostics.Debug.WriteLine("FrameGroPage инициализирован");
        }

        // -----------------------------------------------------------------
        // Загрузка/выгрузка
        // -----------------------------------------------------------------
        private void FrameGroPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePanelsVisibility();
            _repaintTimer?.Start();
        }

        private void FrameGroPage_Unloaded(object sender, RoutedEventArgs e) => Cleanup();

        // -----------------------------------------------------------------
        // Инициализация элементов (датчики, насосы, переключатели и т.д.)
        // -----------------------------------------------------------------
        private void InitializeElements()
        {
            // Тело метода – полностью без изменений (см. ваш оригинал)
            // …
        }

        // -----------------------------------------------------------------
        // Вспомогательные функции
        // -----------------------------------------------------------------
        private static string NormalizeNumericValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var normalized = Regex.Replace(value, @"[^0-9,.+-]", "").Trim();
            normalized = normalized.TrimEnd('.', ',');
            return string.IsNullOrWhiteSpace(normalized) ? value : normalized;
        }

        private void SubscribeToEvents()
        {
            if (_global != null)
                _global.OnVariablesUpdated += Global_OnVariablesUpdated;
        }

        private void Global_OnVariablesUpdated(object sender, EventArgs e) => UpdateAllElements();

        // -----------------------------------------------------------------
        // Основной таймер (обновление UI, отправка команд, обработка режима)
        // -----------------------------------------------------------------
        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            _repaintTimer.Stop();

            try
            {
                UpdateAllElements();          // 1. Обновление всех UI‑элементов
                ResetCommands();              // 2. Сброс «одноразовых» команд
                UpdateOperationMode();        // 3. Синхронизация режима
                UpdatePanelsVisibility();     // 4. Показ/скрытие панелей
                UpdateCounters();             // 5. Обновление счётчиков
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в RepaintTimer_Tick: {ex.Message}");
            }
            finally
            {
                _repaintTimer.Start();
            }
        }

        // -----------------------------------------------------------------
        // Обновление UI‑элементов
        // -----------------------------------------------------------------
        private void UpdateAllElements()
        {
            // Датчики уровня
            LT150?.UpdateElement();
            LT301?.UpdateElement();
            LT303?.UpdateElement();
            LT403?.UpdateElement();

            // Сигнализаторы уровня
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

            // Счётчик
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

            // Переключатели (миксеры, нагреватели)
            UpdateToggleSwitchesFromVariables();
        }

        private void UpdateToggleSwitchesFromVariables()
        {
            // Пример – миксер M100
            var m100Tag = _global?.Variables?.GetByName("M100_IsWork");
            if (m100Tag != null && M100Switch != null)
                M100Switch.IsChecked = m100Tag.ValueReal > 0;

            // Другие переключатели – аналогично
            var m150Tag = _global?.Variables?.GetByName("M150_IsWork");
            if (m150Tag != null && M150Switch != null)
                M150Switch.IsChecked = m150Tag.ValueReal > 0;

            var m400Tag = _global?.Variables?.GetByName("M400_IsWork");
            if (m400Tag != null && M400Switch != null)
                M400Switch.IsChecked = m400Tag.ValueReal > 0;

            var he300Tag = _global?.Variables?.GetByName("HE300_IsOn");
            if (he300Tag != null && HE300Switch != null)
                HE300Switch.IsChecked = he300Tag.ValueReal > 0;

            var he750Tag = _global?.Variables?.GetByName("HE750_IsOn");
            if (he750Tag != null && HE750Switch != null)
                HE750Switch.IsChecked = he750Tag.ValueReal > 0;
        }

        // -----------------------------------------------------------------
        // Сброс «одноразовых» команд (миксеров, нагревателей, подачи вещества, транспортировки)
        // -----------------------------------------------------------------
        private void ResetCommands()
        {
            if (_global?.Commands == null) return;

            // Пример – миксер M100
            var command = _global.Commands.GetByName("M100_StartMixer");
            if (command != null && !command.NeedToWrite)
                command.WriteValue = "false";

            // Остальные команды – копировать аналогично вашему оригинальному коду
            // …
        }

        // -----------------------------------------------------------------
        // Обновление режима работы (логика «застревания» ПЛК и UI‑синхронизация)
        // -----------------------------------------------------------------
        private void UpdateOperationMode()
        {
            var tag = _global.Variables.GetByName("GRO_Rejim");
            if (tag == null) return;

            int mode = (int)tag.ValueReal;

            // -----------------------------------------------------------------
            // 1️⃣ Если PLC «завис» в служебных состояниях (15/16) – повторяем запрос,
            //    пока не получим ответ.
            // -----------------------------------------------------------------
            if ((mode == 15 || mode == 16) &&
                _pendingModeRequest.HasValue &&
                DateTime.UtcNow - _lastModeChangeRequest > TimeSpan.FromSeconds(2) &&
                DateTime.UtcNow - _lastModeRetrySent > TimeSpan.FromSeconds(1))
            {
                SendGroModeCommand(_pendingModeRequest.Value, logUserAction: false);
                _lastModeRetrySent = DateTime.UtcNow;
            }

            // -----------------------------------------------------------------
            // 2️⃣ Синхронизируем панель режимов с текущим состоянием PLC.
            // -----------------------------------------------------------------
            if (GroModePanel != null)
            {
                // 15/16 – служебные состояния. Даем пользователю возможность увидеть
                // свой «запрошенный» режим ещё некоторое время (3 сек).
                if (mode == 15 || mode == 16)
                {
                    bool hasFreshPending = _pendingRequestedMode.HasValue &&
                                           (DateTime.UtcNow - _pendingRequestedModeAt) < TimeSpan.FromSeconds(3);

                    if (hasFreshPending)
                    {
                        // Показать запрошенный пользователем режим
                        if (GroModePanel.CurrentMode != _pendingRequestedMode.Value)
                            GroModePanel.SetMode(_pendingRequestedMode.Value);
                    }
                    else
                    {
                        // Если запрос «устарел», принудительно ставим OFF
                        _pendingRequestedMode = null;
                        if (GroModePanel.CurrentMode != OperationMode.Off)
                            GroModePanel.SetMode(OperationMode.Off);
                    }

                    // Выходим – дальше уже не обновляем UI‑пункт.
                    return;
                }

                // -----------------------------------------------------------------
                // 3️⃣ Обычный (неслужебный) режим PLC – просто отображаем его.
                // -----------------------------------------------------------------
                OperationMode currentOperationMode = mode switch
                {
                    0 => OperationMode.Off,
                    1 or 3 or 4 or 5 or 6 or 7 or 8 or 15 => OperationMode.SemiAuto,
                    2 or 9 or 10 or 11 or 12 or 13 or 14 or 16 => OperationMode.Auto,
                    _ => GroModePanel.CurrentMode
                };

                if (GroModePanel.CurrentMode != currentOperationMode)
                    GroModePanel.SetMode(currentOperationMode);
            }

            // -----------------------------------------------------------------
            // 4️⃣ Если режим PLC уже нормальный (не 15/16) – сбрасываем «ожидание».
            // -----------------------------------------------------------------
            if (mode != 15 && mode != 16)
                _pendingModeRequest = null;
        }

        // -----------------------------------------------------------------
        // Запрос изменения режима (вызывается из UI‑панели)
        // -----------------------------------------------------------------
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

                var command = _global.Commands.GetByName(commandName);
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;

                    // Запоминаем запрос, чтобы можно было повторять и отображать в UI
                    _pendingModeRequest = mode;               // для ретраев, если PLC «зависает»
                    _lastModeChangeRequest = DateTime.UtcNow;    // время отправки

                    // ← **Новые поля** – теперь UI видит ваш запрос
                    _pendingRequestedMode = mode;
                    _pendingRequestedModeAt = DateTime.UtcNow;

                    if (logUserAction)
                        _global.Log.Add("Пользователь", $"Переход в режим {mode}", 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка изменения режима GRO: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------
        // Обновление видимости панелей (зависит от режима)
        // -----------------------------------------------------------------
        private void UpdatePanelsVisibility()
        {
            // Тело метода – без изменений из вашего оригинального кода
            // …
        }

        // -----------------------------------------------------------------
        // Обновление счётчиков (текущие значения веществ)
        // -----------------------------------------------------------------
        private void UpdateCounters()
        {
            // Тело метода – без изменений из вашего оригинального кода
            // …
        }

        // -----------------------------------------------------------------
        // ИНИЦИАЛИЗАЦИЯ ЭЛЕМЕНТОВ (датчики, насосы, клапаны и пр.)
        // -----------------------------------------------------------------
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
                sensor.Designation = description;
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
                pump.UpdateElement();   // ← лишняя точка убрана, оставлен один вызов
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

        // -----------------------------------------------------------------
        // ОБРАБОТЧИКИ СОБЫТИЙ (переключатели, кнопки, панель режима)
        // -----------------------------------------------------------------
        private void M100Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            string cmdName = "M100_StartMixer";
            var command = _global.Commands.GetByName(cmdName);
            if (command == null) return;

            command.WriteValue = isChecked ? "true" : "false";
            command.NeedToWrite = true;
            _global.Log.Add("Пользователь",
                isChecked ? "Включение миксера M100" : "Отключение миксера M100", 1);
        }

        private void M150Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("M150_StartMixer");
            if (command == null) return;

            command.WriteValue = isChecked ? "true" : "false";
            command.NeedToWrite = true;
            _global.Log.Add("Пользователь",
                isChecked ? "Включение миксера M150" : "Отключение миксера M150", 1);
        }

        private void M400Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("M400_StartMixer");
            if (command == null) return;

            command.WriteValue = isChecked ? "true" : "false";
            command.NeedToWrite = true;
            _global.Log.Add("Пользователь",
                isChecked ? "Включение миксера M400" : "Отключение миксера M400", 1);
        }

        private void HE300Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("HE300_IsOn");
            if (command == null) return;

            command.WriteValue = isChecked ? "true" : "false";
            command.NeedToWrite = true;
            _global.Log.Add("Пользователь",
                isChecked ? "Включение нагревателя HE-300" : "Отключение нагревателя HE-300", 1);
        }

        private void HE750Switch_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("HE750_IsOn");
            if (command == null) return;

            command.WriteValue = isChecked ? "true" : "false";
            command.NeedToWrite = true;
            _global.Log.Add("Пользователь",
                isChecked ? "Включение нагревателя HE-750" : "Отключение нагревателя HE-750", 1);
        }

        private void GroModePanel_ModeChanged(object sender, OperationMode mode)
        {
            SendGroModeCommand(mode, logUserAction: true);
        }

        // -----------------------------------------------------------------
        // ОБРАБОТЧИКИ КНОПОК (подача/остановка вещества, транспорт, установки масс и пр.)
        // -----------------------------------------------------------------
        private void SetT100MassButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("T100_MassSp");
            if (command != null && T100MassSpEdit != null)
            {
                command.WriteValue = T100MassSpEdit.Text;
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Задание массы в Т-100", 1);
            }
        }

        private void A100SpeedSpEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_global == null || string.IsNullOrEmpty(A100SpeedSpEdit?.Text)) return;

            var command = _global.Commands.GetByName("A100_Speed");
            if (command != null)
            {
                command.WriteValue = A100SpeedSpEdit.Text;
                command.NeedToWrite = true;
            }
        }

        private void SubstanceStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            try
            {
                if (SelitraRadio?.IsChecked == true)
                {
                    var spCmd = _global.Commands.GetByName("GRO_ManualSelitraCounterSp");
                    if (spCmd != null && SelitraSpEdit != null) spCmd.WriteValue = SelitraSpEdit.Text;

                    var startCmd = _global.Commands.GetByName("GRO_Manual_Selitra_Start");
                    if (startCmd != null) startCmd.WriteValue = "true";

                    _global.Log.Add("Пользователь", "Пуск селитры", 1);
                }
                else if (WaterRadio?.IsChecked == true)
                {
                    var spCmd = _global.Commands.GetByName("GRO_ManualWaterCounterSp");
                    if (spCmd != null && WaterSpEdit != null) spCmd.WriteValue = WaterSpEdit.Text;

                    var startCmd = _global.Commands.GetByName("GRO_Manual_Water_Start");
                    if (startCmd != null) startCmd.WriteValue = "true";

                    _global.Log.Add("Пользователь", "Пуск воды", 1);
                }
                else if (KislotaRadio?.IsChecked == true)
                {
                    var spCmd = _global.Commands.GetByName("GRO_ManualKislotaCounterSp");
                    if (spCmd != null && KislotaSpEdit != null) spCmd.WriteValue = KislotaSpEdit.Text;

                    var startCmd = _global.Commands.GetByName("GRO_Manual_Kislota_Start");
                    if (startCmd != null) startCmd.WriteValue = "true";

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

            var command = _global.Commands.GetByName("GRO_Manual_Stop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка подачи вещества", 1);
            }
        }

        private void SubstancePauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("GRO_Manual_Pause");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пауза подачи вещества", 1);
            }
        }

        private void TransportStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("GRO_TransportStart");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Пуск транспортировки", 1);
            }
        }

        private void TransportStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            var command = _global.Commands.GetByName("GRO_TransportStop");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                _global.Log.Add("Пользователь", "Остановка транспортировки", 1);
            }
        }

        private void SaveHE700RejimButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            if (HE700ComboBox?.SelectedIndex >= 0)
            {
                var command = _global.Commands.GetByName("HE700_Rejim");
                if (command != null)
                {
                    command.WriteValue = HE700ComboBox.SelectedIndex.ToString();
                    command.NeedToWrite = true;
                    _global.Log.Add("Пользователь", "Смена режима HE-700", 1);
                }
            }
        }

        // -----------------------------------------------------------------
        // Очистка ресурсов (отписка от событий, остановка таймера)
        // -----------------------------------------------------------------
        public void Cleanup()
        {
            _repaintTimer?.Stop();

            if (_global != null)
                _global.OnVariablesUpdated -= Global_OnVariablesUpdated;
        }

        // -----------------------------------------------------------------
        // Пустой обработчик из XAML (можно удалить, если он не нужен)
        // -----------------------------------------------------------------
        private void GroEditButton_Click_1(object sender, RoutedEventArgs e) { }
    }
}

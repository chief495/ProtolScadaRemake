using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameGgdPage : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        public FrameGgdPage()
        {
            InitializeComponent();
            Loaded += FrameGgdPage_Loaded;
            Unloaded += FrameGgdPage_Unloaded;
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

            System.Diagnostics.Debug.WriteLine("FrameGgdPage инициализирован");
        }

        private void FrameGgdPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Запуск таймера после загрузки
            _repaintTimer?.Start();
        }

        private void FrameGgdPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void InitializeElements()
        {
            try
            {
                // Аналоговые датчики T-400
                InitializeSensor(TT402, "TT402", "Датчик температуры TT402", "TT-402", "°C");
                InitializeSensor(LT403, "LT403", "Датчик уровня LT403", "LT-403", "%");
                InitializeSensor(PT404, "PT404", "Датчик давления PT404", "PT-404", "бар");

                // Аналоговые датчики T-500
                InitializeSensor(TT502, "TT502", "Датчик температуры TT502", "TT-502", "°C");
                InitializeSensor(LT503, "LT503", "Датчик уровня LT503", "LT-503", "%");
                InitializeSensor(PT504, "PT504", "Датчик давления PT504", "PT-504", "бар");

                // Расходомер
                InitializeSensor(FM401, "FM401", "Массовый расходомер FM401", "FM401", "кг/ч");

                // Счетчики (как Element_AI)
                InitializeSensor(QM400Counter, "QM400", "Счетчик воды QM-400", "QM-400", "л");
                InitializeSensor(QM500Counter, "QM500", "Счетчик воды QM-500", "QM-500", "л");

                // Дискретные датчики
                InitializeDiscreteSensor(LAHH401, "LAHH401", "Датчик уровня LAHH401", "LAHH-401");
                InitializeDiscreteSensor(LAHH501, "LAHH501", "Датчик уровня LAHH501", "LAHH-501");

                // Клапаны
                InitializeValve(VT401, "V401", "Клапан V-401", "V-401");
                InitializeValve(VT501, "V501", "Клапан V-501", "V-501");

                // Насосы
                InitializePumpUz(P400, "P400", "Насос P-400", "P-400");
                InitializePumpReverse(P500, "P500", "Насос P-500", "P-500");

                // Панели набора воды
                if (T400WaterPanel != null)
                {
                    T400WaterPanel.Global = _global;
                    T400WaterPanel.TankName = "T-400";
                    T400WaterPanel.StartButtonClick += T400WaterPanel_StartButtonClick;
                    T400WaterPanel.StopButtonClick += T400WaterPanel_StopButtonClick;
                }

                if (T500WaterPanel != null)
                {
                    T500WaterPanel.Global = _global;
                    T500WaterPanel.TankName = "T-500";
                    T500WaterPanel.StartButtonClick += T500WaterPanel_StartButtonClick;
                    T500WaterPanel.StopButtonClick += T500WaterPanel_StopButtonClick;
                }

                // Переключатели миксеров
                if (T400MixerToggle != null)
                {
                    T400MixerToggle.Tag = "M400";
                    T400MixerToggle.StateChanged += T400MixerToggle_StateChanged;
                }

                if (T500MixerToggle != null)
                {
                    T500MixerToggle.Tag = "M500";
                    T500MixerToggle.StateChanged += T500MixerToggle_StateChanged;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации элементов: {ex.Message}");
            }
        }

        private void InitializeSensor(Element_AI sensor, string varName, string description, string tagName, string eu)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;  // Добавлено: имя для отображения на мнемосхеме
                sensor.EU = eu;
            }
        }

        private void InitializeDiscreteSensor(Element_DI sensor, string varName, string description, string tagName)
        {
            if (sensor != null && _global != null)
            {
                sensor.Global = _global;
                sensor.VarName = varName;
                sensor.Description = description;
                sensor.TagName = tagName;  // Добавлено: имя для отображения на мнемосхеме
            }
        }

        private void InitializeValve(Element_ValveV valve, string varName, string description, string tagName)
        {
            if (valve != null && _global != null)
            {
                valve.Global = _global;
                valve.VarName = varName;
                valve.Description = description;
                valve.TagName = tagName;  // Добавлено: имя для отображения на мнемосхеме
            }
        }

        private void InitializePumpReverse(Element_PumpHReverse pump, string varName, string description, string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;  // Добавлено: имя для отображения на мнемосхеме
                pump.UpdateElement(); // для отображения имени тега
            }
        }

        private void InitializePumpUz(Element_PumpUz pump, string varName, string description, string tagName)
        {
            if (pump != null && _global != null)
            {
                pump.Global = _global;
                pump.VarName = varName;
                pump.Description = description;
                pump.TagName = tagName;  // Добавлено: имя для отображения на мнемосхеме
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

                // 3. Обновление уставок воды
                UpdateWaterSetpoints();

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
            TT402?.UpdateElement();
            LT403?.UpdateElement();
            PT404?.UpdateElement();

            TT502?.UpdateElement();
            LT503?.UpdateElement();
            PT504?.UpdateElement();
            FM401?.UpdateElement();

            // Обновляем счетчики
            QM400Counter?.UpdateElement();
            QM500Counter?.UpdateElement();

            // Обновляем дискретные датчики
            LAHH401?.UpdateElement();
            LAHH501?.UpdateElement();

            // Обновляем клапаны
            VT401?.UpdateElement();
            VT501?.UpdateElement();

            // Обновляем насосы
            P400?.UpdateElement();
            P500?.UpdateElement();

            // Обновляем панели воды
            T400WaterPanel?.UpdateFromGlobal();
            T500WaterPanel?.UpdateFromGlobal();

            // Обновляем состояние переключателей миксеров из переменных
            UpdateMixerTogglesFromVariables();
        }

        private void UpdateMixerTogglesFromVariables()
        {
            try
            {
                // Миксер M400
                var m400Tag = _global?.Variables?.GetByName("M400_IsWork");
                if (m400Tag != null && T400MixerToggle != null)
                {
                    // Синхронизируем состояние переключателя с переменной
                    bool isWorking = m400Tag.ValueReal > 0;
                    if (T400MixerToggle.IsChecked != isWorking)
                    {
                        T400MixerToggle.IsChecked = isWorking;
                    }
                }

                // Миксер M500
                var m500Tag = _global?.Variables?.GetByName("M500_IsWork");
                if (m500Tag != null && T500MixerToggle != null)
                {
                    // Синхронизируем состояние переключателя с переменной
                    bool isWorking = m500Tag.ValueReal > 0;
                    if (T500MixerToggle.IsChecked != isWorking)
                    {
                        T500MixerToggle.IsChecked = isWorking;
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
            // ТОЧНАЯ КОПИЯ ЛОГИКИ ИЗ СТАРОГО ПРОЕКТА
            try
            {
                if (_global?.Commands == null) return;

                TCommandTag command;

                // Сброс команды включения миксера Т-400
                command = _global.Commands.GetByName("T400_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения миксера Т-500
                command = _global.Commands.GetByName("T500_StartMixer");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения наполнения Т-400
                command = _global.Commands.GetByName("T400_StartWater");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды отключения наполнения Т-400
                command = _global.Commands.GetByName("T400_StopWater");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды включения наполнения Т-500
                command = _global.Commands.GetByName("T500_StartWater");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

                // Сброс команды отключения наполнения Т-500
                command = _global.Commands.GetByName("T500_StopWater");
                if (command != null && !command.NeedToWrite)
                    command.WriteValue = "false";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса команд: {ex.Message}");
            }
        }

        private void UpdateWaterSetpoints()
        {
            // Обновление уставок из переменных
            try
            {
                var sp400 = _global?.Variables?.GetByName("T400_SpWater");
                if (sp400 != null && T400WaterPanel != null)
                {
                    T400WaterPanel.SetVolume(sp400.ValueString);
                }

                var sp500 = _global?.Variables?.GetByName("T500_SpWater");
                if (sp500 != null && T500WaterPanel != null)
                {
                    T500WaterPanel.SetVolume(sp500.ValueString);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления уставок: {ex.Message}");
            }
        }

        // ========== ОБРАБОТЧИКИ КОМАНД ==========

        private void T400MixerToggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            // Упрощенная логика как в старом проекте
            if (isChecked)
            {
                // ВКЛЮЧЕНИЕ миксера Т400
                _global.Log.Add("Пользователь", "Включение миксера Т400", 1);
                TCommandTag command = _global.Commands.GetByName("T400_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                // ОТКЛЮЧЕНИЕ миксера Т400
                _global.Log.Add("Пользователь", "Отключение миксера Т400", 1);
                TCommandTag command = _global.Commands.GetByName("T400_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void T500MixerToggle_StateChanged(object sender, bool isChecked)
        {
            if (_global == null) return;

            // Упрощенная логика как в старом проекте
            if (isChecked)
            {
                // ВКЛЮЧЕНИЕ миксера Т500
                _global.Log.Add("Пользователь", "Включение миксера Т500", 1);
                TCommandTag command = _global.Commands.GetByName("T500_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }
            }
            else
            {
                // ОТКЛЮЧЕНИЕ миксера Т500
                _global.Log.Add("Пользователь", "Отключение миксера Т500", 1);
                TCommandTag command = _global.Commands.GetByName("T500_StartMixer");
                if (command != null)
                {
                    command.WriteValue = "false";
                    command.NeedToWrite = true;
                }
            }
        }

        private void T400WaterPanel_StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            // ТОЧНАЯ КОПИЯ ЛОГИКИ ИЗ СТАРОГО ПРОЕКТА T400StartWaterButton_Click
            try
            {
                TVariableTag tag = _global.Variables.GetByName("QM400_Total");

                // Установка уставки объема
                TCommandTag command1 = _global.Commands.GetByName("T400_SpWater");
                if (command1 != null && T400WaterPanel != null)
                {
                    command1.WriteValue = T400WaterPanel.GetVolume().ToString("F1");
                    command1.NeedToWrite = true;
                }

                // Запуск наполнения
                TCommandTag command2 = _global.Commands.GetByName("T400_StartWater");
                if (command2 != null)
                {
                    command2.WriteValue = "true";
                    command2.NeedToWrite = true;
                }

                // Логирование
                string counterValue = tag?.ValueString ?? "0";
                _global.Log.Add("Пользователь",
                    $"Включение наполнения ёмкости Т400. " +
                    $"Показания счетчика {counterValue}. " +
                    $"Объем к наполнению {command1?.WriteValue} л.", 1);

                System.Diagnostics.Debug.WriteLine("Запущено наполнение T400");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска наполнения T400: {ex.Message}");
            }
        }

        private void T400WaterPanel_StopButtonClick(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            // ТОЧНАЯ КОПИЯ ЛОГИКИ ИЗ СТАРОГО ПРОЕКТА T400StopWaterButton_Click
            try
            {
                TVariableTag tag = _global.Variables.GetByName("QM400_Total");
                TCommandTag command = _global.Commands.GetByName("T400_StopWater");

                if (tag != null)
                    _global.Log.Add("Пользователь",
                        $"Отключение наполнения ёмкости Т400. " +
                        $"Показания счетчика {tag.ValueString}", 1);

                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }

                System.Diagnostics.Debug.WriteLine("Остановлено наполнение T400");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки наполнения T400: {ex.Message}");
            }
        }

        private void T500WaterPanel_StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            // ТОЧНАЯ КОПИЯ ЛОГИКИ ИЗ СТАРОГО ПРОЕКТА T500StartWaterButton_Click
            try
            {
                TVariableTag tag = _global.Variables.GetByName("QM500_Total");

                // Установка уставки объема
                TCommandTag command1 = _global.Commands.GetByName("T500_SpWater");
                if (command1 != null && T500WaterPanel != null)
                {
                    command1.WriteValue = T500WaterPanel.GetVolume().ToString("F1");
                    command1.NeedToWrite = true;
                }

                // Запуск наполнения
                TCommandTag command2 = _global.Commands.GetByName("T500_StartWater");
                if (command2 != null)
                {
                    command2.WriteValue = "true";
                    command2.NeedToWrite = true;
                }

                // Логирование
                string counterValue = tag?.ValueString ?? "0";
                _global.Log.Add("Пользователь",
                    $"Включение наполнения ёмкости Т500. " +
                    $"Показания счетчика {counterValue}. " +
                    $"Объем к наполнению {command1?.WriteValue} л.", 1);

                System.Diagnostics.Debug.WriteLine("Запущено наполнение T500");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска наполнения T500: {ex.Message}");
            }
        }

        private void T500WaterPanel_StopButtonClick(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;

            // ТОЧНАЯ КОПИЯ ЛОГИКИ ИЗ СТАРОГО ПРОЕКТА T500StopWaterButton_Click
            try
            {
                TVariableTag tag = _global.Variables.GetByName("QM500_Total");
                TCommandTag command = _global.Commands.GetByName("T500_StopWater");

                if (tag != null)
                    _global.Log.Add("Пользователь",
                        $"Отключение наполнения ёмкости Т500. " +
                        $"Показания счетчика {tag.ValueString}", 1);

                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;
                }

                System.Diagnostics.Debug.WriteLine("Остановлено наполнение T500");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки наполнения T500: {ex.Message}");
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
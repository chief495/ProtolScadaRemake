using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class UnloadPanel : UserControl
    {
        // Публичные события для подписки
        public event RoutedEventHandler SetParamsButtonClick;
        public event RoutedEventHandler ResetButtonClick;
        public event RoutedEventHandler PultModeClick;
        public event RoutedEventHandler TimeModeClick;
        public event RoutedEventHandler MassModeClick;
        public event RoutedEventHandler TorirovanieButtonClick; // Новое событие для тарирования

        // Публичные свойства
        public TGlobal Global { get; set; }
        public string CurrentMode => ModeText?.Text ?? "ПУЛЬТ";
        public string CurrentStage => StageText?.Text ?? "СТОП";

        public UnloadPanel()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // Подписка на события кнопок
            if (SetParamsButton != null)
                SetParamsButton.Click += (s, e) => SetParamsButtonClick?.Invoke(this, e);

            if (ResetButton != null)
                ResetButton.Click += (s, e) => ResetButtonClick?.Invoke(this, e);

            if (PultModeButton != null)
                PultModeButton.Click += (s, e) => PultModeClick?.Invoke(this, e);

            if (TimeModeButton != null)
                TimeModeButton.Click += (s, e) => TimeModeClick?.Invoke(this, e);

            if (MassModeButton != null)
                MassModeButton.Click += (s, e) => MassModeClick?.Invoke(this, e);

            if (TorirovanieButton != null)
                TorirovanieButton.Click += (s, e) => TorirovanieButtonClick?.Invoke(this, e);
        }

        // Публичные методы для установки значений
        public void SetMode(string mode)
        {
            if (ModeText != null)
                ModeText.Text = mode;
        }

        public void SetStage(string stage)
        {
            if (StageText != null)
                StageText.Text = stage;
        }

        public void SetAmountLabel(string label)
        {
            if (AmountLabel != null)
                AmountLabel.Text = label;
        }

        public void SetAmountUnit(string unit)
        {
            if (AmountUnit != null)
                AmountUnit.Text = unit;
        }

        private string GetVariableUnit(string variableName, string fallback)
        {
            var unit = Global?.Variables?.GetByName(variableName)?.TextAfter;
            return string.IsNullOrWhiteSpace(unit) ? fallback : unit.Trim();
        }

        private string GetTimeAmountUnit() => GetVariableUnit("EM_UnloadTorirovanieTime", string.Empty);

        private string GetMassAmountUnit() => GetVariableUnit("EM_UnloadCounter", string.Empty);

        public void SetUnloadedValue(string value)
        {
            if (UnloadedValue != null)
                UnloadedValue.Text = value;
        }

        public void SetUnloadedUnit(string unit)
        {
            if (UnloadedUnit != null)
                UnloadedUnit.Text = unit;
        }

        // Метод для обновления режима отгрузки (как в старом проекте)
        public void UpdateMode(string unloadRejim)
        {
            switch (unloadRejim)
            {
                case "0": // С ПУЛЬТА
                    SetMode("С ПУЛЬТА");
                    SetStage("СТОП");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "1": // По ВРЕМЕНИ (стоп)
                    SetMode("По ВРЕМЕНИ");
                    SetStage("СТОП");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "2": // ПО ВЕСУ (стоп)
                    SetMode("ПО ВЕСУ");
                    SetStage("СТОП");
                    SetAmountLabel("ВЕС");
                    SetAmountUnit(GetMassAmountUnit());
                    break;
                case "3": // С ПУЛЬТА (работа)
                    SetMode("С ПУЛЬТА");
                    SetStage("РАБОТА");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "4": // С ПУЛЬТА (реверс)
                    SetMode("С ПУЛЬТА");
                    SetStage("РЕВЕРС");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "5": // По ВРЕМЕНИ (отгрузка)
                    SetMode("По ВРЕМЕНИ");
                    SetStage("ОТГРУЗКА");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "6": // По ВРЕМЕНИ (реверс)
                    SetMode("По ВРЕМЕНИ");
                    SetStage("РЕВЕРС");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "7": // По ВРЕМЕНИ (Торитование.Пуск)
                    SetMode("По ВРЕМЕНИ");
                    SetStage("ТОРИТОВАНИЕ");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "8": // По ВРЕМЕНИ (Торитование.Пуск)
                    SetMode("По ВРЕМЕНИ");
                    SetStage("ТОРИТОВАНИЕ");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
                case "9": // ПО ВЕСУ (отгрузка)
                    SetMode("ПО ВЕСУ");
                    SetStage("ОТГРУЗКА");
                    SetAmountLabel("ВЕС");
                    SetAmountUnit(GetMassAmountUnit());
                    break;
                case "10": // ПО ВЕСУ (реверс)
                    SetMode("ПО ВЕСУ");
                    SetStage("РЕВЕРС");
                    SetAmountLabel("ВЕС");
                    SetAmountUnit(GetMassAmountUnit());
                    break;
                default:
                    SetMode("ПУЛЬТ");
                    SetStage("СТОП");
                    SetAmountLabel("ВРЕМЯ");
                    SetAmountUnit(GetTimeAmountUnit());
                    break;
            }
        }

        // Методы для получения значений
        public int GetSpeedValue()
        {
            if (SpeedTextBox != null && int.TryParse(SpeedTextBox.Text, out int value))
                return value;
            return 0;
        }

        public int GetAmountValue()
        {
            if (AmountTextBox != null && int.TryParse(AmountTextBox.Text, out int value))
                return value;
            return 0;
        }

        // Обработчики событий (должны быть публичными, если используются в XAML)
        public void SetParamsButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            SetParamsButtonClick?.Invoke(this, e);

            // Логика обработки как в старом проекте
            int speed = GetSpeedValue();
            int amount = GetAmountValue();

            // Определяем какой режим активен
            string currentMode = CurrentMode;
            if (Global != null)
            {
                if (currentMode.Contains("ПУЛЬТ"))
                {
                    // Установка скорости для режима ПУЛЬТ
                    Global.SendCommand("EM_Unload_Sp", speed.ToString());
                }
                else if (currentMode.Contains("ВРЕМЕНИ"))
                {
                    // Установка времени для временного режима
                    Global.SendCommand("EM_Unload_TimeSp", amount.ToString());
                }
                else if (currentMode.Contains("ВЕСУ"))
                {
                    // Установка веса для весового режима
                    Global.SendCommand("EM_Unload_MassSp", amount.ToString());
                }
            }

            System.Diagnostics.Debug.WriteLine($"Установка параметров: режим={currentMode}, скорость={speed}%, количество={amount}");
        }

        public void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            ResetButtonClick?.Invoke(this, e);

            // Логика обработки
            if (SpeedTextBox != null)
                SpeedTextBox.Text = "0";
            if (AmountTextBox != null)
                AmountTextBox.Text = "0";

            if (Global != null)
            {
                Global.SendCommand("EM_Unload_Reset", "true");
            }

            System.Diagnostics.Debug.WriteLine("Сброс параметров отгрузки");
        }

        // Новый обработчик для кнопки тарирования (как в старом проекте button3_Click)
        public void TorirovanieButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            TorirovanieButtonClick?.Invoke(this, e);

            // Логика обработки тарирования как в старом проекте
            if (Global != null)
            {
                System.Diagnostics.Debug.WriteLine("Запуск тарирования (открытие диалога)");

                // Вместо открытия диалога здесь, просто вызываем событие
                // Сам диалог будет открыт в FrameEmPage через событие
            }
        }

        public void PultMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            PultModeClick?.Invoke(this, e);

            // Логика обработки как в старом проекте
            if (Global != null)
            {
                Global.SendCommand("EM_Unloading_PultButton", "true");
            }

            SetMode("С ПУЛЬТА");
            SetAmountLabel("ВРЕМЯ");
            SetAmountUnit(GetTimeAmountUnit());
            System.Diagnostics.Debug.WriteLine("Режим отгрузки: С ПУЛЬТА");
        }

        public void TimeMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            TimeModeClick?.Invoke(this, e);

            // Логика обработки как в старом проекте
            if (Global != null)
            {
                Global.SendCommand("EM_Unloading_TimeButton", "true");
            }

            SetMode("ПО ВРЕМЕНИ");
            SetAmountLabel("ВРЕМЯ");
            SetAmountUnit(GetTimeAmountUnit());
            System.Diagnostics.Debug.WriteLine("Режим отгрузки: ПО ВРЕМЕНИ");
        }

        public void MassMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            MassModeClick?.Invoke(this, e);

            // Логика обработки как в старом проекте
            if (Global != null)
            {
                Global.SendCommand("EM_Unloading_MassButton", "true");
            }

            SetMode("ПО ВЕСУ");
            SetAmountLabel("ВЕС");
            SetAmountUnit(GetMassAmountUnit());
            System.Diagnostics.Debug.WriteLine("Режим отгрузки: ПО ВЕСУ");
        }

        // Метод для обновления значений из глобальных переменных
        public void UpdateFromGlobal()
        {
            if (Global == null || Global.Variables == null) return;

            // Обновляем значение отгружено
            var unloadedCounter = Global.Variables.GetByName("EM_UnloadCounter");
            if (unloadedCounter != null)
                SetUnloadedValue(unloadedCounter.ValueString);

            // Обновляем скорость отгрузки
            var unloadSpeed = Global.Variables.GetByName("EM_Unload_Speed");
            if (unloadSpeed != null && SpeedTextBox != null)
                SpeedTextBox.Text = unloadSpeed.ValueString;

            // Обновляем режим отгрузки
            var unloadRejim = Global.Variables.GetByName("EM_Unloading_Rejim");
            if (unloadRejim != null)
            {
                UpdateMode(unloadRejim.ValueString);
            }
        }
    }
}
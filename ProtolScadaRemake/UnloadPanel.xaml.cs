using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Логика обработки
            int speed = GetSpeedValue();
            int amount = GetAmountValue();
            System.Diagnostics.Debug.WriteLine($"Установка параметров: скорость={speed}%, количество={amount}");
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
            System.Diagnostics.Debug.WriteLine("Сброс параметров");
        }

        public void PultMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            PultModeClick?.Invoke(this, e);

            // Логика обработки
            SetMode("ПУЛЬТ");
            System.Diagnostics.Debug.WriteLine("Режим: ПУЛЬТ");
        }

        public void TimeMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            TimeModeClick?.Invoke(this, e);

            // Логика обработки
            SetMode("ПО ВРЕМЕНИ");
            SetAmountLabel("ВРЕМЯ");
            SetAmountUnit("сек");
            System.Diagnostics.Debug.WriteLine("Режим: ПО ВРЕМЕНИ");
        }

        public void MassMode_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие
            MassModeClick?.Invoke(this, e);

            // Логика обработки
            SetMode("ПО ВЕСУ");
            SetAmountLabel("ВЕС");
            SetAmountUnit("кг");
            System.Diagnostics.Debug.WriteLine("Режим: ПО ВЕСУ");
        }

        // Метод для обновления значений из глобальных переменных
        public void UpdateFromGlobal()
        {
            if (Global == null || Global.Variables == null) return;

            // Пример обновления значений из глобальных переменных
            var unloadedVar = Global.Variables.GetByName("UNLOADED_VALUE");
            if (unloadedVar != null)
                SetUnloadedValue(unloadedVar.ValueReal.ToString());

            var modeVar = Global.Variables.GetByName("UNLOAD_MODE");
            if (modeVar != null)
            {
                SetMode(modeVar.ValueReal switch
                {
                    0 => "ПУЛЬТ",
                    1 => "ПО ВРЕМЕНИ",
                    2 => "ПО ВЕСУ",
                    _ => "ПУЛЬТ"
                });
            }
        }
    }
}
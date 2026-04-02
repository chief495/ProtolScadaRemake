using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class DialogElementMixer : Window
    {
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        public TGlobal? Global;
        public string VarName = string.Empty;

        private bool _isInitializing = true;
        private DispatcherTimer _repaintTimer;

        public DialogElementMixer()
        {
            InitializeComponent();

            _repaintTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _repaintTimer.Tick += RepaintTimer_Tick;
        }

        public void Initialize()
        {
            _isInitializing = true;
            try
            {
                // Режим работы
                var manual = FindVariable("_Manual");
                RBAuto.IsChecked = manual == null || manual.ValueReal < 1;
                RBManual.IsChecked = manual != null && manual.ValueReal > 0;

                // Время запуска
                var startTime = FindVariable("_StartTime");
                if (startTime != null)
                    StartTimeNumeric.Value = startTime.ValueReal;

                // Время остановки
                var stopTime = FindVariable("_StopTime");
                if (stopTime != null)
                    StopTimeNumeric.Value = stopTime.ValueReal;

                // Контроль доступа
                ApplyAccessRestrictions();

                // Запуск таймера подсветки
                _repaintTimer.Start();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void ApplyAccessRestrictions()
        {
            bool hasAccess = Global?.Access == true;

            // Режим работы доступен ВСЕГДА (без пароля)
            RBAuto.IsEnabled = true;
            RBManual.IsEnabled = true;

            // Время работы требует пароль
            StartTimeNumeric.IsEnabled = hasAccess;
            StopTimeNumeric.IsEnabled = hasAccess;
        }

        private TVariableTag? FindVariable(string suffix) => Global?.Variables?.GetByName(VarName + suffix);

        private TCommandTag? FindCommand(string suffix) => Global?.Commands?.GetByName(VarName + suffix);

        private void RepaintTimer_Tick(object? sender, EventArgs e)
        {
            _repaintTimer.Stop();

            // Подсветка времени запуска
            var startTimeVar = FindVariable("_StartTime");
            if (!StartTimeNumeric.IsFocused)
            {
                StartTimeNumeric.Background = NormalColor;
                if (startTimeVar != null)
                {
                    if (Math.Abs(startTimeVar.ValueReal - (StartTimeNumeric.Value ?? 0)) >= 1)
                        StartTimeNumeric.Background = EditColor;
                }
            }

            // Подсветка времени остановки
            var stopTimeVar = FindVariable("_StopTime");
            if (!StopTimeNumeric.IsFocused)
            {
                StopTimeNumeric.Background = NormalColor;
                if (stopTimeVar != null)
                {
                    if (Math.Abs(stopTimeVar.ValueReal - (StopTimeNumeric.Value ?? 0)) >= 1)
                        StopTimeNumeric.Background = EditColor;
                }
            }

            _repaintTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _repaintTimer.Stop();
        }

        #region Обработчики режима

        private void RBAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || RBAuto.IsChecked != true) return;
            SendCommand("_Manual", "false", "Установлен автоматический режим миксера.");
        }

        private void RBManual_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || RBManual.IsChecked != true) return;
            SendCommand("_Manual", "true", "Установлен ручной режим миксера.");
        }

        #endregion

        #region Обработчики времени

        private void StartTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing || !StartTimeNumeric.Value.HasValue) return;

            var variable = FindVariable("_StartTime");
            if (variable != null)
            {
                if (Math.Abs(variable.ValueReal - StartTimeNumeric.Value.Value) < 1)
                    return;
            }

            string value = StartTimeNumeric.Value.Value.ToString(CultureInfo.InvariantCulture);
            SendCommand("_StartTime", value, $"Установлено время запуска: {value} сек.");
        }

        private void StopTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing || !StopTimeNumeric.Value.HasValue) return;

            var variable = FindVariable("_StopTime");
            if (variable != null)
            {
                if (Math.Abs(variable.ValueReal - StopTimeNumeric.Value.Value) < 1)
                    return;
            }

            string value = StopTimeNumeric.Value.Value.ToString(CultureInfo.InvariantCulture);
            SendCommand("_StopTime", value, $"Установлено время остановки: {value} сек.");
        }

        #endregion

        private void SendCommand(string suffix, string value, string logMessage)
        {
            if (Global == null) return;

            string commandName = VarName + suffix;
            var command = FindCommand(suffix);

            if (command == null)
            {
                Debug.WriteLine($"Команда не найдена: {commandName}");
                return;
            }

            command.WriteValue = value;
            command.NeedToWrite = true;
            Global.Commands.SendToController();

            Global.Log.Add("Пользователь", $"{Title}. {logMessage}", 1);
            Debug.WriteLine($"Команда отправлена: {commandName} = {value}");
        }
    }
}
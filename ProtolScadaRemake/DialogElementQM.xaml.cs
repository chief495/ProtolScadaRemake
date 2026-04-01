using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class DialogElementQM : Window
    {
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        public TGlobal Global;
        public string VarName = "";

        private bool _isInitializing = true;
        private DispatcherTimer _repaintTimer;

        private string _eu = "л";
        public string EU
        {
            get => _eu;
            set
            {
                _eu = value ?? "л";
                CurrentValueEU.Content = _eu;
            }
        }

        public DialogElementQM()
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
                // Текущее значение
                TVariableTag variableTag = FindVariable("_Total");
                if (variableTag != null)
                    TotalLabel.Text = variableTag.ValueReal.ToString("##0.##");

                // Начальное значение
                variableTag = FindVariable("_StartValue");
                if (variableTag != null)
                    StartValueNumeric.Value = Convert.ToInt32(variableTag.ValueReal);

                // Импульсов на м³ (хранится как дробное, показываем * 1000)
                variableTag = FindVariable("_PulseSize");
                if (variableTag != null)
                    PulseInCubeNumeric.Value = Convert.ToInt32(variableTag.ValueReal * 1000);

                // Запуск таймера
                _repaintTimer.Start();

                // Контроль доступа
                ApplyAccessRestrictions();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void ApplyAccessRestrictions()
        {
            bool hasAccess = Global?.Access == true;
            SettingsGroupBox.IsEnabled = hasAccess;
        }

        private TVariableTag FindVariable(string suffix) => Global?.Variables?.GetByName(VarName + suffix);

        private TCommandTag FindCommand(string suffix) => Global?.Commands?.GetByName(VarName + suffix);

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            _repaintTimer.Stop();

            // Обновляем текущее значение
            TVariableTag totalTag = FindVariable("_Total");
            if (totalTag != null)
                TotalLabel.Text = totalTag.ValueReal.ToString("##0.##");

            // Начальное значение - подсветка
            TVariableTag startValueVariable = FindVariable("_StartValue");
            if (!StartValueNumeric.IsFocused)
            {
                StartValueNumeric.Background = NormalColor;
                if (startValueVariable != null)
                {
                    if (startValueVariable.ValueReal != Convert.ToDouble(StartValueNumeric.Value ?? 0))
                        StartValueNumeric.Background = EditColor;
                }
            }

            // Импульсов на м³ - подсветка
            TVariableTag pulseSizeVariable = FindVariable("_PulseSize");
            if (!PulseInCubeNumeric.IsFocused)
            {
                PulseInCubeNumeric.Background = NormalColor;
                if (pulseSizeVariable != null)
                {
                    if (Math.Abs(pulseSizeVariable.ValueReal * 1000 - Convert.ToDouble(PulseInCubeNumeric.Value ?? 0)) >= 1)
                        PulseInCubeNumeric.Background = EditColor;
                }
            }

            _repaintTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _repaintTimer.Stop();
        }

        #region Отправка команд

        private void SendCommand(string commandSuffix, string value, string logMessage)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            string fullCommandName = VarName + commandSuffix;
            TCommandTag command = FindCommand(commandSuffix);

            if (command == null)
            {
                Debug.WriteLine($"Команда не найдена: {fullCommandName}");
                return;
            }

            command.WriteValue = value;
            command.NeedToWrite = true;
            Global.Commands.SendToController();

            Global.Log.Add("Пользователь", $"{Title}. {logMessage}", 1);
            Debug.WriteLine($"Команда отправлена: {fullCommandName} = {value}");
        }

        #endregion

        #region Обработчики числовых полей

        private void StartValueNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (!StartValueNumeric.Value.HasValue) return;

            TVariableTag variable = FindVariable("_StartValue");
            if (variable != null)
            {
                if (Math.Abs(variable.ValueReal - StartValueNumeric.Value.Value) < 0.001)
                    return;
            }

            string value = StartValueNumeric.Value.Value.ToString();
            SendCommand("_StartValue", value, $"Начальное значение изменено: {value} л.");
        }

        private void PulseInCubeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (!PulseInCubeNumeric.Value.HasValue) return;

            TVariableTag variable = FindVariable("_PulseSize");
            if (variable != null)
            {
                if (Math.Abs(variable.ValueReal * 1000 - PulseInCubeNumeric.Value.Value) < 1)
                    return;
            }

            // Конвертируем обратно: делим на 1000
            string value = Convert.ToString(Convert.ToDouble(PulseInCubeNumeric.Value) / 1000);
            SendCommand("_PulseSize", value, $"Цена импульса изменена: {value} л.");
        }

        #endregion

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Global == null) return;

            TCommandTag resetCommand = FindCommand("_Reset");
            if (resetCommand != null)
            {
                resetCommand.WriteValue = "true";
                resetCommand.NeedToWrite = true;
                Global.Commands.SendToController();

                Global.Log.Add("Пользователь", $"{Title}. Выполнен сброс счётчика.", 1);
            }
        }
    }
}
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class DialogElementPump : Window
    {
        public TGlobal Global;
        public string VarName = "";

        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        private bool _isInitializing = true;

        public DialogElementPump()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            _isInitializing = true;

            try
            {
                // Режим работы
                TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_Manual");
                if (VariableTag != null)
                {
                    if (VariableTag.ValueReal > 0)
                    {
                        RBAuto.IsChecked = false;
                        RBManual.IsChecked = true;
                        SetButtonsVisibility(Visibility.Visible);
                    }
                    else
                    {
                        RBAuto.IsChecked = true;
                        RBManual.IsChecked = false;
                        SetButtonsVisibility(Visibility.Hidden);
                    }
                }

                // Время запуска
                VariableTag = Global.Variables.GetByName(VarName + "_StartTime");
                if (VariableTag != null)
                    StartTimeNumeric.Value = VariableTag.ValueReal;

                // Время остановки
                VariableTag = Global.Variables.GetByName(VarName + "_StopTime");
                if (VariableTag != null)
                    StopTimeNumeric.Value = VariableTag.ValueReal;

                // Блокировка на основе пароля
                GroupBox2.IsEnabled = Global.Access;

                // Обновляем цвета кнопок
                UpdateButtonColors();
                UpdateVisualStates();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void SetButtonsVisibility(Visibility visibility)
        {
            StartButton.Visibility = visibility;
            StopButton.Visibility = visibility;
        }

        private void UpdateButtonColors()
        {
            if (RBManual.IsChecked == true)
            {
                TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_ManualStart");
                if (VariableTag != null)
                {
                    if (VariableTag.ValueReal > 0)
                    {
                        StartButton.Background = ButtonActiveColor;
                        StopButton.Background = ButtonDeactiveColor;
                    }
                    else
                    {
                        StartButton.Background = ButtonDeactiveColor;
                        StopButton.Background = ButtonActiveColor;
                    }
                }
            }
        }

        private void UpdateVisualStates()
        {
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            RBAuto.Background = this.Background;
            RBManual.Background = this.Background;

            if (ManualVariable != null)
            {
                bool isManualInController = ManualVariable.ValueReal > 0;
                bool isManualInUI = RBManual.IsChecked == true;

                if (isManualInController != isManualInUI)
                {
                    RBAuto.Background = EditColor;
                    RBManual.Background = EditColor;
                }
            }

            UpdateNumericBackground(StartTimeNumeric, VarName + "_StartTime");
            UpdateNumericBackground(StopTimeNumeric, VarName + "_StopTime");
        }

        private void UpdateNumericBackground(MahApps.Metro.Controls.NumericUpDown numeric, string variableName)
        {
            if (numeric.IsFocused) return;

            TVariableTag variable = Global.Variables.GetByName(variableName);
            numeric.Background = NormalColor;

            if (variable != null && numeric.Value.HasValue)
            {
                if (Math.Abs(variable.ValueReal - numeric.Value.Value) >= 0.001)
                {
                    numeric.Background = EditColor;
                }
            }
        }

        #region Отправка команд

        private void SendCommand(string commandSuffix, string value, string logMessage)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            string fullCommandName = VarName + commandSuffix;
            TCommandTag command = Global.Commands.GetByName(fullCommandName);

            if (command == null)
            {
                Debug.WriteLine($"Команда не найдена: {fullCommandName}");
                return;
            }

            Action<string, bool, string> handler = null;
            handler = (name, success, error) =>
            {
                command.OnCommandCompleted -= handler;

                Dispatcher.BeginInvoke(() =>
                {
                    if (success)
                    {
                        Global.Log.Add("Пользователь", $"{Title}. {logMessage}", 1);
                    }
                    else
                    {
                        Debug.WriteLine($"Ошибка команды {fullCommandName}: {error}");
                    }
                });
            };

            command.OnCommandCompleted += handler;
            command.WriteValue = value;
            command.NeedToWrite = true;
            command.SendToController();

            Debug.WriteLine($"Команда отправлена: {fullCommandName} = {value}");
        }

        #endregion

        #region Обработчики режима

        private void RBAuto_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (RBAuto.IsChecked != true) return;

            SetButtonsVisibility(Visibility.Hidden);

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal > 0)
            {
                SendCommand("_Manual", "false", "Переведен в автоматический режим.");
                StopButton_Click(sender, e);
            }
        }

        private void RBManual_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (RBManual.IsChecked != true) return;

            SetButtonsVisibility(Visibility.Visible);
            UpdateButtonColors();

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal < 1)
            {
                SendCommand("_Manual", "true", "Переведен в ручной режим.");
            }
        }

        #endregion

        #region Кнопки Включить/Выключить

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualStart");

            if (ManualValueVariable != null && ManualValueVariable.ValueReal < 1)
            {
                SendCommand("_ManualStart", "true", "Значение ручного режима изменено на 'Включено'.");

                StartButton.Background = ButtonActiveColor;
                StopButton.Background = ButtonDeactiveColor;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualStart");

            if (ManualValueVariable != null && ManualValueVariable.ValueReal > 0)
            {
                SendCommand("_ManualStart", "false", "Значение ручного режима изменено на 'Отключено'.");

                StartButton.Background = ButtonDeactiveColor;
                StopButton.Background = ButtonActiveColor;
            }
        }

        #endregion

        #region Обработчики времени

        private void StartTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!StartTimeNumeric.Value.HasValue) return;

            TVariableTag StartTimeVariable = Global.Variables.GetByName(VarName + "_StartTime");

            if (StartTimeVariable != null)
            {
                if (Math.Abs(StartTimeVariable.ValueReal - StartTimeNumeric.Value.Value) >= 0.001)
                {
                    string value = StartTimeNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_StartTime", value, $"Время включения изменено на {value} сек.");
                }
            }
        }

        private void StopTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!StopTimeNumeric.Value.HasValue) return;

            TVariableTag StopTimeVariable = Global.Variables.GetByName(VarName + "_StopTime");

            if (StopTimeVariable != null)
            {
                if (Math.Abs(StopTimeVariable.ValueReal - StopTimeNumeric.Value.Value) >= 0.001)
                {
                    string value = StopTimeNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_StopTime", value, $"Время остановки изменено на {value} сек.");
                }
            }
        }

        #endregion
    }
}
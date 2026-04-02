using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class DialogElementDI : Window
    {
        public TGlobal Global;
        public string VarName = "";

        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        // Флаг для предотвращения срабатывания при инициализации
        private bool _isInitializing = true;

        public DialogElementDI()
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

                // Инверсия сигнала
                VariableTag = Global.Variables.GetByName(VarName + "_Reverse");
                if (VariableTag != null)
                {
                    ReverseCheckBox.IsChecked = VariableTag.ValueReal > 0;
                }

                // Задержка включения
                VariableTag = Global.Variables.GetByName(VarName + "_OnDelay");
                if (VariableTag != null)
                    OnDelayNumeric.Value = Convert.ToInt32(VariableTag.ValueReal);

                // Задержка отключения
                VariableTag = Global.Variables.GetByName(VarName + "_OffDelay");
                if (VariableTag != null)
                    OffDelayNumeric.Value = Convert.ToInt32(VariableTag.ValueReal);

                // Блокировка на основе пароля
                GroupBox2.IsEnabled = Global.Access;

                // Обновляем цвета кнопок если в ручном режиме
                UpdateButtonColors();
                UpdateVisualStates();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// Показать/скрыть кнопки Норма/Сработка
        /// </summary>
        private void SetButtonsVisibility(Visibility visibility)
        {
            NormButton.Visibility = visibility;
            AlarmButton.Visibility = visibility;
        }

        /// <summary>
        /// Обновить цвета кнопок в зависимости от текущего значения
        /// </summary>
        private void UpdateButtonColors()
        {
            if (RBManual.IsChecked == true)
            {
                TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_ManualValue");
                if (VariableTag != null)
                {
                    if (VariableTag.ValueReal > 0)
                    {
                        NormButton.Background = ButtonDeactiveColor;
                        AlarmButton.Background = ButtonActiveColor;
                    }
                    else
                    {
                        NormButton.Background = ButtonActiveColor;
                        AlarmButton.Background = ButtonDeactiveColor;
                    }
                }
            }
        }

        private void UpdateVisualStates()
        {
            // Подсветка режима работы если есть различия
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

            // Подсветка NumericUpDown
            UpdateNumericBackground(OnDelayNumeric, VarName + "_OnDelay");
            UpdateNumericBackground(OffDelayNumeric, VarName + "_OffDelay");

            // Подсветка CheckBox
            TVariableTag ReverseVariable = Global.Variables.GetByName(VarName + "_Reverse");
            ReverseCheckBox.Background = this.Background;
            if (ReverseVariable != null)
            {
                bool isReversedInController = ReverseVariable.ValueReal > 0;
                bool isReversedInUI = ReverseCheckBox.IsChecked == true;

                if (isReversedInController != isReversedInUI)
                {
                    ReverseCheckBox.Background = EditColor;
                }
            }
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

            // Скрываем кнопки
            SetButtonsVisibility(Visibility.Hidden);

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal > 0)
            {
                SendCommand("_Manual", "false", "Переведен в автоматический режим.");

                // Также сбрасываем ручное значение
                NormButton_Click(sender, e);
            }
        }

        private void RBManual_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (RBManual.IsChecked != true) return;

            // Показываем кнопки
            SetButtonsVisibility(Visibility.Visible);
            UpdateButtonColors();

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal < 1)
            {
                SendCommand("_Manual", "true", "Переведен в ручной режим.");
            }
        }

        #endregion

        #region Кнопки Норма/Сработка

        private void NormButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");

            if (ManualValueVariable != null && ManualValueVariable.ValueReal > 0)
            {
                SendCommand("_ManualValue", "false", "Значение ручного режима изменено на 'Норма'.");

                // Обновляем цвета кнопок
                NormButton.Background = ButtonActiveColor;
                AlarmButton.Background = ButtonDeactiveColor;
            }
        }

        private void AlarmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");

            if (ManualValueVariable != null && ManualValueVariable.ValueReal < 1)
            {
                SendCommand("_ManualValue", "true", "Значение ручного режима изменено на 'Сработка'.");

                // Обновляем цвета кнопок
                NormButton.Background = ButtonDeactiveColor;
                AlarmButton.Background = ButtonActiveColor;
            }
        }

        #endregion

        #region Остальные обработчики

        private void ReverseCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;

            TVariableTag ReverseVariable = Global.Variables.GetByName(VarName + "_Reverse");

            if (ReverseVariable != null)
            {
                bool shouldBeChecked = ReverseVariable.ValueReal > 0;
                bool isChecked = ReverseCheckBox.IsChecked == true;

                if (shouldBeChecked != isChecked)
                {
                    string value = isChecked ? "true" : "false";
                    string message = isChecked ? "Включена инверсия сигнала." : "Отключена инверсия сигнала.";
                    SendCommand("_Reverse", value, message);
                }
            }
        }

        private void OnDelayNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!OnDelayNumeric.Value.HasValue) return;

            TVariableTag OnDelayVariable = Global.Variables.GetByName(VarName + "_OnDelay");

            if (OnDelayVariable != null)
            {
                if (Math.Abs(OnDelayVariable.ValueReal - OnDelayNumeric.Value.Value) >= 0.001)
                {
                    string value = OnDelayNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_OnDelay", value, $"Задержка включения изменена на {value} сек.");
                }
            }
        }

        private void OffDelayNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!OffDelayNumeric.Value.HasValue) return;

            TVariableTag OffDelayVariable = Global.Variables.GetByName(VarName + "_OffDelay");

            if (OffDelayVariable != null)
            {
                if (Math.Abs(OffDelayVariable.ValueReal - OffDelayNumeric.Value.Value) >= 0.001)
                {
                    string value = OffDelayNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_OffDelay", value, $"Задержка отключения изменена на {value} сек.");
                }
            }
        }

        #endregion
    }
}
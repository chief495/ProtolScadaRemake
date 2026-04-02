using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class DialogElementValve : Window
    {
        public TGlobal Global;
        public string VarName = "";

        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        private bool _isInitializing = true;

        public DialogElementValve()
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

                // Время открытия
                VariableTag = Global.Variables.GetByName(VarName + "_OpenTime");
                if (VariableTag != null)
                    OpenTimeNumeric.Value = VariableTag.ValueReal;

                // Время закрытия
                VariableTag = Global.Variables.GetByName(VarName + "_CloseTime");
                if (VariableTag != null)
                    CloseTimeNumeric.Value = VariableTag.ValueReal;

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

        /// <summary>
        /// Показать/скрыть кнопки Открыть/Закрыть
        /// </summary>
        private void SetButtonsVisibility(Visibility visibility)
        {
            OpenButton.Visibility = visibility;
            CloseButton.Visibility = visibility;
        }

        /// <summary>
        /// Обновить цвета кнопок
        /// </summary>
        private void UpdateButtonColors()
        {
            if (RBManual.IsChecked == true)
            {
                TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_ManualOpen");
                if (VariableTag != null)
                {
                    if (VariableTag.ValueReal > 0)
                    {
                        OpenButton.Background = ButtonActiveColor;
                        CloseButton.Background = ButtonDeactiveColor;
                    }
                    else
                    {
                        OpenButton.Background = ButtonDeactiveColor;
                        CloseButton.Background = ButtonActiveColor;
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

            UpdateNumericBackground(OpenTimeNumeric, VarName + "_OpenTime");
            UpdateNumericBackground(CloseTimeNumeric, VarName + "_CloseTime");
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
            if (Global == null) return;

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

        /// <summary>
        /// Отправка двух команд одновременно (для клапана: Open и Close)
        /// </summary>
        private void SendValveCommands(string openValue, string closeValue, string logMessage)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            TCommandTag openCommand = Global.Commands.GetByName(VarName + "_ManualOpen");
            TCommandTag closeCommand = Global.Commands.GetByName(VarName + "_ManualClose");

            if (openCommand == null || closeCommand == null)
            {
                Debug.WriteLine($"Команды клапана не найдены: {VarName}");
                return;
            }

            // Отправляем обе команды
            openCommand.WriteValue = openValue;
            openCommand.NeedToWrite = true;

            closeCommand.WriteValue = closeValue;
            closeCommand.NeedToWrite = true;

            // Подписываемся на завершение одной из команд для логирования
            Action<string, bool, string> handler = null;
            handler = (name, success, error) =>
            {
                openCommand.OnCommandCompleted -= handler;

                Dispatcher.BeginInvoke(() =>
                {
                    if (success)
                    {
                        Global.Log.Add("Пользователь", $"{Title}. {logMessage}", 1);
                    }
                    else
                    {
                        Debug.WriteLine($"Ошибка команды клапана: {error}");
                    }
                });
            };

            openCommand.OnCommandCompleted += handler;

            // Отправляем команды
            openCommand.SendToController();
            closeCommand.SendToController();

            Debug.WriteLine($"Команды клапана отправлены: Open={openValue}, Close={closeValue}");
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
                CloseButton_Click(sender, e);
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

        #region Кнопки Открыть/Закрыть

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            TVariableTag ManualOpenVariable = Global.Variables.GetByName(VarName + "_ManualOpen");

            if (ManualOpenVariable != null && ManualOpenVariable.ValueReal < 1)
            {
                SendValveCommands("true", "false", "Значение ручного режима изменено на 'Открыть'.");

                OpenButton.Background = ButtonActiveColor;
                CloseButton.Background = ButtonDeactiveColor;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            TVariableTag ManualCloseVariable = Global.Variables.GetByName(VarName + "_ManualClose");

            if (ManualCloseVariable != null && ManualCloseVariable.ValueReal < 1)
            {
                SendValveCommands("false", "true", "Значение ручного режима изменено на 'Закрыть'.");

                OpenButton.Background = ButtonDeactiveColor;
                CloseButton.Background = ButtonActiveColor;
            }
        }

        #endregion

        #region Обработчики времени

        private void OpenTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!OpenTimeNumeric.Value.HasValue) return;

            TVariableTag OpenTimeVariable = Global.Variables.GetByName(VarName + "_OpenTime");

            if (OpenTimeVariable != null)
            {
                if (Math.Abs(OpenTimeVariable.ValueReal - OpenTimeNumeric.Value.Value) >= 0.001)
                {
                    string value = OpenTimeNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_OpenTime", value, $"Время открытия изменено на {value} сек.");
                }
            }
        }

        private void CloseTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || !Global.Access) return;
            if (!CloseTimeNumeric.Value.HasValue) return;

            TVariableTag CloseTimeVariable = Global.Variables.GetByName(VarName + "_CloseTime");

            if (CloseTimeVariable != null)
            {
                if (Math.Abs(CloseTimeVariable.ValueReal - CloseTimeNumeric.Value.Value) >= 0.001)
                {
                    string value = CloseTimeNumeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SendCommand("_CloseTime", value, $"Время закрытия изменено на {value} сек.");
                }
            }
        }

        #endregion
    }
}

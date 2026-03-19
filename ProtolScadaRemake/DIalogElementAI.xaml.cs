using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class DialogElementAI : Window
    {
        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

        public TGlobal Global;
        public string VarName = "";

        private bool _isInitializing = true;

        private string _eu;
        public string EU
        {
            get => _eu;
            set
            {
                _eu = value;
                string euWithArrows = string.IsNullOrWhiteSpace(_eu) ? "▲▼" : $"{_eu} ▲▼";
                TextEU1.Content = euWithArrows;
                TextEU2.Content = euWithArrows;
                TextEU3.Content = euWithArrows;
                TextEU4.Content = euWithArrows;
                TextEU5.Content = euWithArrows;
                TextEU6.Content = euWithArrows;
                ManualValueUnits.Content = euWithArrows;
            }
        }

        public DialogElementAI()
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
                        SetManualPanelVisibility(Visibility.Visible);
                    }
                    else
                    {
                        RBAuto.IsChecked = true;
                        RBManual.IsChecked = false;
                        SetManualPanelVisibility(Visibility.Hidden);
                    }
                }

                // Ручное значение
                VariableTag = Global.Variables.GetByName(VarName + "_ManualValue");
                if (VariableTag != null)
                    ManualValueNumeric.Value = VariableTag.ValueReal;

                // Аварийные значения
                LoadNumericValue(LWNumeric, VarName + "_LW");
                LoadNumericValue(HWNumeric, VarName + "_HW");
                LoadNumericValue(LFNumeric, VarName + "_LF");
                LoadNumericValue(HFNumeric, VarName + "_HF");

                // Настройки датчика
                LoadNumericValue(LowLevelNumeric, VarName + "_LowLevel");
                LoadNumericValue(HiLevelNumeric, VarName + "_HiLevel");
                LoadNumericValue(LowCurrNumeric, VarName + "_LowCurr");
                LoadNumericValue(HiCurrNumeric, VarName + "_HiCurr");

                // Ограничение без пароля: блокируем только ручной режим и min/max у AI
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

            if (RBManual != null)
                RBManual.IsEnabled = hasAccess;

            if (ManualValueNumeric != null)
                ManualValueNumeric.IsEnabled = hasAccess;

            if (HFNumeric != null) HFNumeric.IsEnabled = hasAccess;
            if (HWNumeric != null) HWNumeric.IsEnabled = hasAccess;
            if (LWNumeric != null) LWNumeric.IsEnabled = hasAccess;
            if (LFNumeric != null) LFNumeric.IsEnabled = hasAccess;

            if (LowLevelNumeric != null) LowLevelNumeric.IsEnabled = hasAccess;
            if (HiLevelNumeric != null) HiLevelNumeric.IsEnabled = hasAccess;
            if (LowCurrNumeric != null) LowCurrNumeric.IsEnabled = hasAccess;
            if (HiCurrNumeric != null) HiCurrNumeric.IsEnabled = hasAccess;
        }

        private void LoadNumericValue(NumericUpDown numeric, string varName)
        {
            TVariableTag tag = Global.Variables.GetByName(varName);
            if (tag != null)
                numeric.Value = tag.ValueReal;
        }

        /// <summary>
        /// Управление видимостью панели ручного значения (3 отдельных элемента)
        /// </summary>
        private void SetManualPanelVisibility(Visibility visibility)
        {
            ManualValueTitle.Visibility = visibility;
            ManualValueNumeric.Visibility = visibility;
            ManualValueUnits.Visibility = visibility;
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

            // Подписываемся на завершение
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

        private void SendNumericCommand(NumericUpDown numeric, string suffix, string paramName, string units)
        {
            if (_isInitializing) return;
            if (!numeric.Value.HasValue) return;

            TVariableTag variable = Global.Variables.GetByName(VarName + suffix);

            if (variable != null)
            {
                if (Math.Abs(variable.ValueReal - numeric.Value.Value) < 0.001)
                    return;
            }

            string value = numeric.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string logMessage = $"{paramName} изменено на {value} {units}.";

            SendCommand(suffix, value, logMessage);
        }

        #endregion

        #region Обработчики режима

        private void RBManual_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;
            if (RBManual.IsChecked != true) return;

            // ПОКАЗЫВАЕМ панель ручного значения
            SetManualPanelVisibility(Visibility.Visible);

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal < 1)
            {
                SendCommand("_Manual", "true", "Переведен в ручной режим.");
            }
        }

        private void RBAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;
            if (RBAuto.IsChecked != true) return;

            // СКРЫВАЕМ панель ручного значения
            SetManualPanelVisibility(Visibility.Hidden);

            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualVariable.ValueReal > 0)
            {
                SendCommand("_Manual", "false", "Переведен в автоматический режим.");
            }
        }

        #endregion

        #region Обработчики числовых полей

        private void ManualValueNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(ManualValueNumeric, "_ManualValue", "Ручное значение", "%");
        }

        private void HFNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(HFNumeric, "_HF", "Верхнее аварийное значение", EU ?? "%");
        }

        private void HWNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(HWNumeric, "_HW", "Верхнее предаварийное значение", EU ?? "%");
        }

        private void LWNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(LWNumeric, "_LW", "Нижнее предаварийное значение", EU ?? "%");
        }

        private void LFNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(LFNumeric, "_LF", "Нижнее аварийное значение", EU ?? "%");
        }

        private void LowLevelNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(LowLevelNumeric, "_LowLevel", "Нижняя граница измеряемого уровня", EU ?? "%");
        }

        private void HiLevelNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(HiLevelNumeric, "_HiLevel", "Верхняя граница измеряемого уровня", EU ?? "%");
        }

        private void LowCurrNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(LowCurrNumeric, "_LowCurr", "Нижняя граница токовой петли", "mA");
        }

        private void HiCurrNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(HiCurrNumeric, "_HiCurr", "Верхняя граница токовой петли", "mA");
        }

        #endregion
    }
}
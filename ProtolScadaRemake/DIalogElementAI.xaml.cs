using MahApps.Metro.Controls;
using System;
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
        private string _varPrefix = "";

        private bool _isInitializing = true;
        private string _lowBoundSuffix = "_LowLevel";
        private string _hiBoundSuffix = "_HiLevel";

        private string _eu;
        public string EU
        {
            get => _eu;
            set
            {
                _eu = value;
                string euDisplay = string.IsNullOrWhiteSpace(_eu) ? "" : _eu;
                TextEU1.Content = euDisplay;
                TextEU2.Content = euDisplay;
                TextEU3.Content = euDisplay;
                TextEU4.Content = euDisplay;
                TextEU5.Content = euDisplay;
                TextEU6.Content = euDisplay;
                ManualValueUnits.Content = euDisplay;
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
                ResolveVarPrefix();
                ResolveMeasurementBoundSuffixes();

                // Режим работы
                TVariableTag VariableTag = FindVariable("_Manual");
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
                VariableTag = FindVariable("_ManualValue");
                if (VariableTag != null)
                    ManualValueNumeric.Value = VariableTag.ValueReal;

                // Аварийные значения
                LoadNumericValue(LWNumeric, "_LW");
                LoadNumericValue(HWNumeric, "_HW");
                LoadNumericValue(LFNumeric, "_LF");
                LoadNumericValue(HFNumeric, "_HF");

                // Настройки датчика
                LoadNumericValue(LowLevelNumeric, _lowBoundSuffix);
                LoadNumericValue(HiLevelNumeric, _hiBoundSuffix);
                LoadNumericValue(LowCurrNumeric, "_LowCurr");
                LoadNumericValue(HiCurrNumeric, "_HiCurr");

                // Ограничение без пароля: параметры нормирования доступны всегда
                ApplyAccessRestrictions();
            }
            finally
            {
                _isInitializing = false;
            }
        }


        private void ResolveMeasurementBoundSuffixes()
        {
            string[] lowCandidates = { "_LowLevel", "_LowTemp", "_LowPress" };
            string[] hiCandidates = { "_HiLevel", "_HiTemp", "_HiPress" };

            _lowBoundSuffix = ResolveFirstExistingSuffix(lowCandidates, "_LowLevel");
            _hiBoundSuffix = ResolveFirstExistingSuffix(hiCandidates, "_HiLevel");
        }

        private string ResolveFirstExistingSuffix(string[] candidates, string fallback)
        {
            if (Global == null) return fallback;

            foreach (string suffix in candidates)
            {
                if (FindCommand(suffix) != null || FindVariable(suffix) != null)
                    return suffix;
            }

            return fallback;
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

            // Параметры нормирования доступны обычному пользователю без пароля
            if (LowLevelNumeric != null) LowLevelNumeric.IsEnabled = true;
            if (HiLevelNumeric != null) HiLevelNumeric.IsEnabled = true;
            if (LowCurrNumeric != null) LowCurrNumeric.IsEnabled = true;
            if (HiCurrNumeric != null) HiCurrNumeric.IsEnabled = true;
        }

        private void ResolveVarPrefix()
        {
            _varPrefix = VarName;

            if (string.IsNullOrWhiteSpace(VarName) || Global == null)
                return;

            string[] removableSuffixes = { "_Value", "_Volume", "_MassFlow", "_VolumeFlow", "_Total" };
            foreach (string suffix in removableSuffixes)
            {
                if (!VarName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string candidate = VarName[..^suffix.Length];
                if (HasAnalogLikeTags(candidate))
                {
                    _varPrefix = candidate;
                    return;
                }
            }
        }

        private bool HasAnalogLikeTags(string prefix)
        {
            string[] checkSuffixes = { "_Manual", "_ManualValue", "_LW", "_HW", "_LF", "_HF", "_LowCurr", "_HiCurr" };
            foreach (string suffix in checkSuffixes)
            {
                if (Global.Commands?.GetByName(prefix + suffix) != null || Global.Variables?.GetByName(prefix + suffix) != null)
                    return true;
            }
            return false;
        }

        private TVariableTag FindVariable(string suffix) => Global?.Variables?.GetByName(_varPrefix + suffix);

        private TCommandTag FindCommand(string suffix) => Global?.Commands?.GetByName(_varPrefix + suffix);

        private void LoadNumericValue(NumericUpDown numeric, string suffix)
        {
            TVariableTag tag = FindVariable(suffix);
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

            string fullCommandName = _varPrefix + commandSuffix;
            TCommandTag command = FindCommand(commandSuffix);

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

            TVariableTag variable = FindVariable(suffix);

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

            TVariableTag ManualVariable = FindVariable("_Manual");

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

            TVariableTag ManualVariable = FindVariable("_Manual");

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
            SendNumericCommand(LowLevelNumeric, _lowBoundSuffix, "Нижняя граница измеряемой величины", EU ?? "%");
        }

        private void HiLevelNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            SendNumericCommand(HiLevelNumeric, _hiBoundSuffix, "Верхняя граница измеряемой величины", EU ?? "%");
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

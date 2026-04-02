using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class DialogElementMixer : Window
    {
        public TGlobal? Global;
        public string VarName = string.Empty;

        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;

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
                TVariableTag? variableTag = Global?.Variables?.GetByName(VarName + "_Manual");
                if (variableTag != null)
                {
                    if (variableTag.ValueReal > 0)
                    {
                        RBAuto.IsChecked = false;
                        RBManual.IsChecked = true;
                        SetButtonsVisibility(Visibility.Visible);
                        UpdateManualStartButtons();
                    }
                    else
                    {
                        RBAuto.IsChecked = true;
                        RBManual.IsChecked = false;
                        SetButtonsVisibility(Visibility.Hidden);
                    }
                }

                // Время запуска
                variableTag = Global?.Variables?.GetByName(VarName + "_StartTime");
                if (variableTag != null)
                    StartTimeNumeric.Value = variableTag.ValueReal;

                // Время остановки
                variableTag = Global?.Variables?.GetByName(VarName + "_StopTime");
                if (variableTag != null)
                    StopTimeNumeric.Value = variableTag.ValueReal;

                // Блокировка времени на основе пароля
                GroupBox2.IsEnabled = Global?.Access == true;

                // Запуск таймера
                _repaintTimer.Start();
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

        private void UpdateManualStartButtons()
        {
            TVariableTag? variableTag = Global?.Variables?.GetByName(VarName + "_ManualStart");
            if (variableTag != null)
            {
                if (variableTag.ValueReal > 0)
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

        private void RepaintTimer_Tick(object? sender, EventArgs e)
        {
            _repaintTimer.Stop();

            // Подсветка режима работы
            TVariableTag? manualVariable = Global?.Variables?.GetByName(VarName + "_Manual");
            RBAuto.Background = this.Background;
            RBManual.Background = this.Background;

            if (manualVariable != null)
            {
                bool isManualInController = manualVariable.ValueReal > 0;
                bool isAutoChecked = RBAuto.IsChecked == true;

                if ((isManualInController && isAutoChecked) || (!isManualInController && !isAutoChecked))
                {
                    RBAuto.Background = EditColor;
                    RBManual.Background = EditColor;
                }
            }

            // Подсветка времени запуска
            UpdateNumericBackground(StartTimeNumeric, VarName + "_StartTime");

            // Подсветка времени остановки
            UpdateNumericBackground(StopTimeNumeric, VarName + "_StopTime");

            // Обновление кнопок в ручном режиме
            if (RBManual.IsChecked == true)
            {
                UpdateManualStartButtons();
            }

            _repaintTimer.Start();
        }

        private void UpdateNumericBackground(MahApps.Metro.Controls.NumericUpDown numeric, string variableName)
        {
            if (numeric.IsFocused) return;

            TVariableTag? variable = Global?.Variables?.GetByName(variableName);
            numeric.Background = NormalColor;

            if (variable != null && numeric.Value.HasValue)
            {
                if (Math.Abs(variable.ValueReal - numeric.Value.Value) >= 0.001)
                {
                    numeric.Background = EditColor;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _repaintTimer.Stop();
        }

        private void RBAuto_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;
            if (RBAuto.IsChecked != true) return;

            SetButtonsVisibility(Visibility.Hidden);

            TVariableTag? manualVariable = Global.Variables?.GetByName(VarName + "_Manual");
            TCommandTag? manualCommand = Global.Commands?.GetByName(VarName + "_Manual");

            if (manualVariable != null && manualCommand != null)
            {
                if (manualVariable.ValueReal > 0)
                {
                    manualCommand.WriteValue = "false";
                    manualCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Переведен в автоматический режим.", 1);

                    // Выключаем при переходе в авто режим
                    StopButton_Click(sender, e);
                }
            }
        }

        private void RBManual_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;
            if (RBManual.IsChecked != true) return;

            SetButtonsVisibility(Visibility.Visible);
            UpdateManualStartButtons();

            TVariableTag? manualVariable = Global.Variables?.GetByName(VarName + "_Manual");
            TCommandTag? manualCommand = Global.Commands?.GetByName(VarName + "_Manual");

            if (manualVariable != null && manualCommand != null)
            {
                if (manualVariable.ValueReal < 1)
                {
                    manualCommand.WriteValue = "true";
                    manualCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Переведен в ручной режим.", 1);

                    // Выключаем при переходе в ручной режим
                    StopButton_Click(sender, e);
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            TVariableTag? manualValueVariable = Global.Variables?.GetByName(VarName + "_ManualStart");
            TCommandTag? manualValueCommand = Global.Commands?.GetByName(VarName + "_ManualStart");

            if (manualValueVariable != null && manualValueCommand != null)
            {
                if (manualValueVariable.ValueReal < 1)
                {
                    manualValueCommand.WriteValue = "true";
                    manualValueCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Значение ручного режима изменено на 'Включено'.", 1);
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (Global == null) return;

            TVariableTag? manualValueVariable = Global.Variables?.GetByName(VarName + "_ManualStart");
            TCommandTag? manualValueCommand = Global.Commands?.GetByName(VarName + "_ManualStart");

            if (manualValueVariable != null && manualValueCommand != null)
            {
                if (manualValueVariable.ValueReal > 0)
                {
                    manualValueCommand.WriteValue = "false";
                    manualValueCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Значение ручного режима изменено на 'Отключено'.", 1);
                }
            }
        }

        private void StartTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || Global.Access != true) return;
            if (!StartTimeNumeric.Value.HasValue) return;

            TVariableTag? startTimeVariable = Global.Variables?.GetByName(VarName + "_StartTime");
            TCommandTag? startTimeCommand = Global.Commands?.GetByName(VarName + "_StartTime");

            if (startTimeVariable != null && startTimeCommand != null)
            {
                if (Math.Abs(startTimeVariable.ValueReal - StartTimeNumeric.Value.Value) >= 0.001)
                {
                    startTimeCommand.WriteValue = StartTimeNumeric.Value.Value.ToString();
                    startTimeCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Время включения изменено на {startTimeCommand.WriteValue} сек.", 1);
                }
            }
        }

        private void StopTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (_isInitializing) return;
            if (Global == null || Global.Access != true) return;
            if (!StopTimeNumeric.Value.HasValue) return;

            TVariableTag? stopTimeVariable = Global.Variables?.GetByName(VarName + "_StopTime");
            TCommandTag? stopTimeCommand = Global.Commands?.GetByName(VarName + "_StopTime");

            if (stopTimeVariable != null && stopTimeCommand != null)
            {
                if (Math.Abs(stopTimeVariable.ValueReal - StopTimeNumeric.Value.Value) >= 0.001)
                {
                    stopTimeCommand.WriteValue = StopTimeNumeric.Value.Value.ToString();
                    stopTimeCommand.NeedToWrite = true;
                    Global.Commands?.SendToController();
                    Global.Log?.Add("Пользователь", $"{Title}. Время остановки изменено на {stopTimeCommand.WriteValue} сек.", 1);
                }
            }
        }
    }
}
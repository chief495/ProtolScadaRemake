using System.Windows;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для DialogElementPump.xaml
    /// </summary>
    public partial class DialogElementPumpUz : Window
    {
        public TGlobal Global;
        private System.Windows.Media.Brush ButtonDeactiveColor = Brushes.White;
        private System.Windows.Media.Brush ButtonActiveColor = Brushes.Green;
        private System.Windows.Media.Brush NormalColor = Brushes.White;
        private System.Windows.Media.Brush EditColor = Brushes.Yellow;
        public string VarName = ""; // Основание для имен
        public DialogElementPumpUz()
        {
            InitializeComponent();
        }
        public void Initialize() // Инициализация формы
        {
            // Режим работы
            TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_Manual");
            if (VariableTag != null)
            {
                if (VariableTag.ValueReal > 0)
                {
                    RBAuto.IsChecked = false;
                    RBManual.IsChecked = true;
                    ManualSpeed.Visibility = Visibility.Visible;
                }
                else
                {
                    RBAuto.IsChecked = true;
                    RBManual.IsChecked = false;
                    ManualSpeed.Visibility = Visibility.Hidden;
                }
            }

            // Ручное значение
            VariableTag = Global.Variables.GetByName(VarName + "_ManualSpeed");
            if (VariableTag != null) if (VariableTag.ValueReal >= 0) if (VariableTag.ValueReal <= 100) ManualSpeedNumeric.Value = VariableTag.ValueReal;
            // Время запуска
            VariableTag = Global.Variables.GetByName(VarName + "_StartTime");
            if (VariableTag != null) StartTimeNumeric.Value = VariableTag.ValueReal;
            // Время остановки
            VariableTag = Global.Variables.GetByName(VarName + "_StopTime");
            if (VariableTag != null) StopTimeNumeric.Value = VariableTag.ValueReal;

            // Автоматический режим
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
            RBAuto.Background = this.Background;
            if (ManualVariable != null)
            {
                if (ManualVariable.ValueReal < 1)
                    if (!RBAuto.IsChecked == true)
                        RBAuto.Background = EditColor;
                if (ManualVariable.ValueReal > 0)
                    if (RBAuto.IsChecked == true)
                        RBAuto.Background = EditColor;
            }
            // Ручной режим
            RBManual.Background = this.Background;
            if (ManualVariable != null)
            {
                if (ManualVariable.ValueReal < 1)
                    if (!RBAuto.IsChecked == true)
                        RBManual.Background = EditColor;
                if (ManualVariable.ValueReal > 0)
                    if (RBAuto.IsChecked == true)
                        RBManual.Background = EditColor;
            }
            // Режим работы и значение ручного режима
            if (RBAuto.IsChecked == true)
            {
                RBManual.IsChecked = false;
                StartButton.Visibility = Visibility.Hidden;
                StopButton.Visibility = Visibility.Hidden;
                ManualSpeed.Visibility = Visibility.Hidden;
            }
            if (RBManual.IsChecked == true)
            {
                RBAuto.IsChecked = false;
                StartButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;
                ManualSpeed.Visibility = Visibility.Visible;
                TVariableTag VariableTag2 = Global.Variables.GetByName(VarName + "_ManualStart");
                if (VariableTag2 != null)
                    if (VariableTag2.ValueReal > 0)
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

            // Скорость в ручном режиме
            TVariableTag ManualSpeedVariable = Global.Variables.GetByName(VarName + "_ManualSpeed");
            if (ManualSpeedNumeric.IsFocused == false)
            {
                ManualSpeedNumeric.Background = NormalColor;
                if (ManualSpeedVariable != null)
                    if (ManualSpeedVariable.ValueReal != Convert.ToDouble(ManualSpeedNumeric.Value))
                        ManualSpeedNumeric.Background = EditColor;
            }
            // Время запуска
            TVariableTag StartTimeVariable = Global.Variables.GetByName(VarName + "_StartTime");
            if (StartTimeNumeric.IsFocused == false)
            {
                StartTimeNumeric.Background = NormalColor;
                if (StartTimeVariable != null)
                    if (StartTimeVariable.ValueReal != Convert.ToDouble(StartTimeNumeric.Value))
                        StartTimeNumeric.Background = EditColor;
            }
            // Время останова
            TVariableTag StoptTimeVariable = Global.Variables.GetByName(VarName + "_StopTime");
            if (StopTimeNumeric.IsFocused == false)
            {
                StopTimeNumeric.Background = NormalColor;
                if (StoptTimeVariable != null)
                    if (StoptTimeVariable.ValueReal != Convert.ToDouble(StopTimeNumeric.Value))
                        StopTimeNumeric.Background = EditColor;
            }
        }
        private async void RBAuto_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
            TCommandTag ManualCommand = Global.Commands.GetByName(VarName + "_Manual");
            if (RBAuto.IsChecked == true)
                if (ManualVariable != null)
                    if (ManualCommand != null)
                        if (ManualVariable.ValueReal > 0)
                        {
                            ManualCommand.WriteValue = "false";
                            ManualCommand.NeedToWrite = true;
                            Global.Commands.SendToController();
                            await Global.Log.Add("Пользователь", Content?.ToString() + ". Переведен в автоматический режим.", 1);
                            StopButton_Click(sender, e);
                        }
        }

        private async void RBManual_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
            TCommandTag ManualCommand = Global.Commands.GetByName(VarName + "_Manual");
            if (RBManual.IsChecked == true)
                if (ManualVariable != null)
                    if (ManualCommand != null)
                        if (ManualVariable.ValueReal < 1)
                        {
                            ManualCommand.WriteValue = "true";
                            ManualCommand.NeedToWrite = true;
                            Global.Commands.SendToController();
                            await Global.Log.Add("Пользователь", Content?.ToString() + ". Переведен в ручной режим.", 1);
                            StopButton_Click(sender, e);
                        }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualStart");
            TCommandTag ManualValueCommand = Global.Commands.GetByName(VarName + "_ManualStart");
            if (ManualValueVariable != null)
                if (ManualValueCommand != null)
                    if (ManualValueVariable.ValueReal < 1)
                    {
                        ManualValueCommand.WriteValue = "true";
                        ManualValueCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Включено'.", 1);
                    }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualStart");
            TCommandTag ManualValueCommand = Global.Commands.GetByName(VarName + "_ManualStart");
            if (ManualValueVariable != null)
                if (ManualValueCommand != null)
                    if (ManualValueVariable.ValueReal > 0)
                    {
                        ManualValueCommand.WriteValue = "false";
                        ManualValueCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Отключено'.", 1);
                    }
        }

        private async void ManualSpeedNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag StartTimeVariable = Global.Variables.GetByName(VarName + "_ManualSpeed");
            TCommandTag StartTimeCommand = Global.Commands.GetByName(VarName + "_ManualSpeed");
            if (StartTimeVariable != null)
                if (StartTimeCommand != null)
                    if (StartTimeVariable.ValueReal != Convert.ToDouble(ManualSpeedNumeric.Value))
                    {
                        StartTimeCommand.WriteValue = ManualSpeedNumeric.Value.ToString();
                        StartTimeCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Скорость в ручном режиме изменена на " + StartTimeCommand.WriteValue + " %.", 1);
                    }
        }

        private async void StartTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag StartTimeVariable = Global.Variables.GetByName(VarName + "_StartTime");
            TCommandTag StartTimeCommand = Global.Commands.GetByName(VarName + "_StartTime");
            if (StartTimeVariable != null)
                if (StartTimeCommand != null)
                    if (StartTimeVariable.ValueReal != Convert.ToDouble(StartTimeNumeric.Value))
                    {
                        StartTimeCommand.WriteValue = StartTimeNumeric.Value.ToString();
                        StartTimeCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Время включения изменено на " + StartTimeCommand.WriteValue + " сек.", 1);
                    }
        }

        private async void StopTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag StopTimeVariable = Global.Variables.GetByName(VarName + "_StopTime");
            TCommandTag StopTimeCommand = Global.Commands.GetByName(VarName + "_StopTime");
            if (StopTimeVariable != null)
                if (StopTimeCommand != null)
                    if (StopTimeVariable.ValueReal != Convert.ToDouble(StopTimeNumeric.Value))
                    {
                        StopTimeCommand.WriteValue = StopTimeNumeric.Value.ToString();
                        StopTimeCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Время включения изменено на " + StopTimeCommand.WriteValue + " сек.", 1);
                    }
        }
    }
}


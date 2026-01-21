using ProtolScadaRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;


namespace ProtolScadaRemake
{
    public partial class DialogElementDI : Window 
    {
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        private System.Windows.Media.Brush ButtonDeactiveColor = Brushes.White;
        private System.Windows.Media.Brush ButtonActiveColor = Brushes.Green;
        private System.Windows.Media.Brush NormalColor = Brushes.White;
        private System.Windows.Media.Brush EditColor = Brushes.Yellow;
        public DialogElementDI()
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
                }
                else
                {
                    RBAuto.IsChecked = true;
                    RBManual.IsChecked = false;
                }
            }
            // Инверсия сигнала
            VariableTag = Global.Variables.GetByName(VarName + "_Reverse");
            if (VariableTag != null)
            {
                ReverseCheckBox.IsChecked = false;
                if (VariableTag.ValueReal > 0) ReverseCheckBox.IsChecked = true;
            }
            // Задержка вклочения
            VariableTag = Global.Variables.GetByName(VarName + "_OnDelay");
            if (VariableTag != null) OnDelayNumeric.Value = Convert.ToInt32(VariableTag.ValueReal);
            // Задержка отключения
            VariableTag = Global.Variables.GetByName(VarName + "_OffDelay");
            if (VariableTag != null) OffDelayNumeric.Value = Convert.ToInt32(VariableTag.ValueReal);



            //RepaintTimer.Enabled = false;
            bool ActivateOkButton = true;
            // Режим работы и значение ручного режима
            if (RBAuto.IsChecked == true)
            {
                RBManual.IsChecked = false;
                NormButton.Visibility = Visibility.Hidden;
                AlarmButton.Visibility = Visibility.Hidden;
            }
            if (RBManual.IsChecked == true)
            {
                NormButton.Visibility = Visibility.Visible;
                AlarmButton.Visibility = Visibility.Visible;
                RBAuto.IsChecked = false;

                VariableTag = Global.Variables.GetByName(VarName + "_ManualValue");
                if (VariableTag != null)
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
            // Задержка включения
            TVariableTag OnDelayVariable = Global.Variables.GetByName(VarName + "_OnDelay");
            if (!OnDelayNumeric.IsFocused)
            {
                OnDelayNumeric.Background = NormalColor;
                if (OnDelayVariable != null)
                    if (OnDelayVariable.ValueReal != Convert.ToDouble(OnDelayNumeric.Value))
                        OnDelayNumeric.Background = EditColor;
            }
            // Задержка отключения
            TVariableTag OffDelayVariable = Global.Variables.GetByName(VarName + "_OffDelay");
            if (!OffDelayNumeric.IsFocused)
            {
                OffDelayNumeric.Background = NormalColor;
                if (OffDelayVariable != null)
                    if (OffDelayVariable.ValueReal != Convert.ToDouble(OffDelayNumeric.Value))
                        OffDelayNumeric.Background = EditColor;
            }

            //RepaintTimer.Enabled = true;
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
            // Инверсия
            TVariableTag ReverseVariable = Global.Variables.GetByName(VarName + "_Reverse");
            ReverseCheckBox.Background = this.Background;
            if (ReverseVariable != null)
            {
                if (ReverseVariable.ValueReal < 1)
                    if (ReverseCheckBox.IsChecked == true)
                        ReverseCheckBox.Background = EditColor;
                if (ReverseVariable.ValueReal > 0)
                    if (!ReverseCheckBox.IsChecked == true)
                        ReverseCheckBox.Background = EditColor;
            }
        }
        private async void RBAuto_CheckedChanged(object sender, EventArgs e)
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
                            NormButton_Click(sender, e);
                        }
        }

        private async void RBManual_CheckedChanged(object sender, EventArgs e)
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
                            NormButton_Click(sender, e);
                        }

        }
        private async void NormButton_Click(object sender, EventArgs e)
        {
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");
            TCommandTag ManualValueCommand = Global.Commands.GetByName(VarName + "_ManualValue");
            if (ManualValueVariable != null)
                if (ManualValueCommand != null)
                    if (ManualValueVariable.ValueReal > 0)
                    {
                        ManualValueCommand.WriteValue = "false";
                        ManualValueCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Норма'.", 1);
                    }
        }

        private async void AlarmButton_Click(object sender, EventArgs e)
        {
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");
            TCommandTag ManualValueCommand = Global.Commands.GetByName(VarName + "_ManualValue");
            if (ManualValueVariable != null)
                if (ManualValueCommand != null)
                    if (ManualValueVariable.ValueReal < 1)
                    {
                        ManualValueCommand.WriteValue = "true";
                        ManualValueCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Сработка'.", 1);
                    }
        }

        private async void ReverseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TVariableTag ReverseVariable = Global.Variables.GetByName(VarName + "_Reverse");
            TCommandTag ReverseCommand = Global.Commands.GetByName(VarName + "_Reverse");
            if (ReverseVariable != null)
                if (ReverseCommand != null)
                    if (ReverseVariable.ValueReal < 1)
                    {
                        if (ReverseCheckBox.IsChecked == true)
                        {
                            ReverseCommand.WriteValue = "true";
                            ReverseCommand.NeedToWrite = true;
                            Global.Commands.SendToController();
                            await Global.Log.Add("Пользователь", Content?.ToString() + ". Включена инверсия сигнала.", 1);
                        }
                    }
                    else
                    {
                        if (!ReverseCheckBox.IsChecked == true)
                        {
                            ReverseCommand.WriteValue = "false";
                            ReverseCommand.NeedToWrite = true;
                            Global.Commands.SendToController();
                            await Global.Log.Add("Пользователь", Content?.ToString() + ". Отключена инверсия сигнала.", 1);
                        }
                    }
        }

        private async void OnDelayNumeric_ValueChanged(object sender, EventArgs e)
        {
            TVariableTag OnDelayVariable = Global.Variables.GetByName(VarName + "_OnDelay");
            TCommandTag OnDelayCommand = Global.Commands.GetByName(VarName + "_OnDelay");
            if (OnDelayVariable != null)
                if (OnDelayCommand != null)
                    if (OnDelayVariable.ValueReal != Convert.ToDouble(OnDelayNumeric.Value))
                    {
                        OnDelayCommand.WriteValue = OnDelayNumeric.Value.ToString();
                        OnDelayCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Задержка включения изменена на " + OnDelayCommand.WriteValue + " сек.", 1);
                    }
        }

        private async void OffDelayNumeric_ValueChanged(object sender, EventArgs e)
        {
            TVariableTag OffDelayVariable = Global.Variables.GetByName(VarName + "_OffDelay");
            TCommandTag OffDelayCommand = Global.Commands.GetByName(VarName + "_OffDelay");
            if (OffDelayVariable != null)
                if (OffDelayCommand != null)
                    if (OffDelayVariable.ValueReal != Convert.ToDouble(OffDelayNumeric.Value))
                    {
                        OffDelayCommand.WriteValue = OffDelayNumeric.Value.ToString();
                        OffDelayCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() +". Задержка отключения изменена на " + OffDelayCommand.WriteValue + " сек.", 1);
                    }

        }
    }
}
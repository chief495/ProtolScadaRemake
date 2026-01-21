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
    public partial class DialogElementValve : Window
    {
        private Brush ButtonDeactiveColor = Brushes.White;
        private Brush ButtonActiveColor = Brushes.Green;
        private Brush NormalColor = Brushes.White;
        private Brush EditColor = Brushes.Yellow;
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public DialogElementValve()
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
            // Время открытия
            VariableTag = Global.Variables.GetByName(VarName + "_OpenTime");
            if (VariableTag != null) OpenTimeNumeric.Value = VariableTag.ValueReal;
            // Время закрытия
            VariableTag = Global.Variables.GetByName(VarName + "_CloseTime");
            if (VariableTag != null) CloseTimeNumeric.Value = VariableTag.ValueReal;
            // Включение таймера
            GroupBox2.IsEnabled = Global.Access;      
      
            bool ActivateOkButton = true;
            // Режим работы и значение ручного режима
            if (RBAuto.IsChecked == true)
            {
                OpenButton.Visibility = Visibility.Hidden;
                CloseButton.Visibility = Visibility.Hidden;
                RBManual.IsChecked = false;
            }
            if (RBManual.IsChecked == true)
            {
                OpenButton.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Visible;
                RBAuto.IsChecked = false;
            }
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
                TVariableTag VariableTag2 = Global.Variables.GetByName(VarName + "_ManualOpen");
                if (VariableTag2 != null)
                    if (VariableTag2.ValueReal > 0)
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
            // Время открытия
            TVariableTag OpenTimeVariable = Global.Variables.GetByName(VarName + "_OpenTime");
            if (OpenTimeNumeric.IsFocused == false)
            {
                OpenTimeNumeric.Background = NormalColor;
                if (OpenTimeVariable != null)
                    if (OpenTimeVariable.ValueReal != Convert.ToDouble(OpenTimeNumeric.Value))
                        OpenTimeNumeric.Background = EditColor;
            }
            // Время закрытия
            TVariableTag CloseTimeVariable = Global.Variables.GetByName(VarName + "_CloseTime");
            if (CloseTimeNumeric.IsFocused == false)
            {
                CloseTimeNumeric.Background = NormalColor;
                if (CloseTimeVariable != null)
                    if (CloseTimeVariable.ValueReal != Convert.ToDouble(CloseTimeNumeric.Value))
                        CloseTimeNumeric.Background = EditColor;
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
                            CloseButton_Click(sender, e);
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
                            CloseButton_Click(sender, e);
                        }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualOpenVariable = Global.Variables.GetByName(VarName + "_ManualOpen");
            TCommandTag ManualOpenCommand = Global.Commands.GetByName(VarName + "_ManualOpen");
            TVariableTag ManualCloseVariable = Global.Variables.GetByName(VarName + "_ManualClose");
            TCommandTag ManualCloseCommand = Global.Commands.GetByName(VarName + "_ManualClose");
            if (ManualOpenVariable != null)
                if (ManualOpenCommand != null)
                    if (ManualCloseVariable != null)
                        if (ManualCloseCommand != null)
                            if (ManualOpenVariable.ValueReal < 1)
                            {
                                ManualOpenCommand.WriteValue = "true";
                                ManualOpenCommand.NeedToWrite = true;
                                ManualCloseCommand.WriteValue = "false";
                                ManualCloseCommand.NeedToWrite = true;
                                Global.Commands.SendToController();
                                await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Открыть'.", 1);
                            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            TVariableTag ManualOpenVariable = Global.Variables.GetByName(VarName + "_ManualOpen");
            TCommandTag ManualOpenCommand = Global.Commands.GetByName(VarName + "_ManualOpen");
            TVariableTag ManualCloseVariable = Global.Variables.GetByName(VarName + "_ManualClose");
            TCommandTag ManualCloseCommand = Global.Commands.GetByName(VarName + "_ManualClose");
            if (ManualOpenVariable != null)
                if (ManualOpenCommand != null)
                    if (ManualCloseVariable != null)
                        if (ManualCloseCommand != null)
                            if (ManualCloseVariable.ValueReal < 1)
                            {
                                ManualOpenCommand.WriteValue = "false";
                                ManualOpenCommand.NeedToWrite = true;
                                ManualCloseCommand.WriteValue = "true";
                                ManualCloseCommand.NeedToWrite = true;
                                Global.Commands.SendToController();
                                await Global.Log.Add("Пользователь", Content?.ToString() + ". Значение ручного режима изменено на 'Закрыть'.", 1);
                            }
        }

        private async void OpenTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag OpenTimeVariable = Global.Variables.GetByName(VarName + "_OpenTime");
            TCommandTag OpenTimeCommand = Global.Commands.GetByName(VarName + "_OpenTime");
            if (OpenTimeVariable != null)
                if (OpenTimeCommand != null)
                    if (OpenTimeVariable.ValueReal != Convert.ToDouble(OpenTimeNumeric.Value))
                    {
                        OpenTimeCommand.WriteValue = OpenTimeNumeric.Value.ToString();
                        OpenTimeCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Время открытия изменено на " + OpenTimeCommand.WriteValue + " сек.", 1);
                    }
        }

        private async void CloseTimeNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag CloseTimeVariable = Global.Variables.GetByName(VarName + "_CloseTime");
            TCommandTag CloseTimeCommand = Global.Commands.GetByName(VarName + "_CloseTime");
            if (CloseTimeVariable != null)
                if (CloseTimeCommand != null)
                    if (CloseTimeVariable.ValueReal != Convert.ToDouble(CloseTimeNumeric.Value))
                    {
                        CloseTimeCommand.WriteValue = CloseTimeNumeric.Value.ToString();
                        CloseTimeCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        await Global.Log.Add("Пользователь", Content?.ToString() + ". Время закрытия изменено на " + CloseTimeCommand.WriteValue + " сек.", 1);
                    }
        }
    }
}

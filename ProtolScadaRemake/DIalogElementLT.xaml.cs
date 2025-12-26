using MahApps.Metro.Controls;
using ProtolScadaRemake;
using SharpVectors.Dom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
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
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class DialogElementLT : Window
    {
        private System.Windows.Media.Brush ButtonDeactiveColor = Brushes.White;
        private System.Windows.Media.Brush ButtonActiveColor = Brushes.Green;
        private System.Windows.Media.Brush NormalColor = Brushes.White;
        private System.Windows.Media.Brush EditColor = Brushes.Yellow;
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        private string _eu;
        public string EU
        {
            get => _eu;
            set
            {
                _eu = value;
                TextEU1.Content = _eu;
                TextEU2.Content = _eu;
                TextEU3.Content = _eu;
                TextEU4.Content = _eu;
                TextEU5.Content = _eu;
                TextEU6.Content = _eu;
                ManualValueUnits.Content = _eu;
            }
        }
        public DialogElementLT()
        {
            InitializeComponent();
        }


        public void Initialize() // инициализация формы
        {
            // режим работы
            TVariableTag VariableTag = Global.Variables.GetByName(VarName + "_Manual");
            if (VariableTag != null)
            {
                if (VariableTag.ValueReal > 0)
                {
                    RBAuto.IsChecked = false;
                    RBManual.IsChecked = true;
                    ManualValueTitle.Visibility = Visibility.Visible;
                    ManualValueNumeric.Visibility = Visibility.Visible;
                }
                else
                {
                    RBAuto.IsChecked = true;
                    RBManual.IsChecked = false;
                    ManualValueTitle.Visibility = Visibility.Hidden;
                    ManualValueNumeric.Visibility = Visibility.Hidden;
                }
            }
            // Ручное значение
            VariableTag = Global.Variables.GetByName(VarName + "_ManualValue");
            if (VariableTag != null)
                ManualValueNumeric.Value = VariableTag.ValueReal;
            // Аварийные и предаварийные значения
            VariableTag = Global.Variables.GetByName(VarName + "_LW");
            if (VariableTag != null) LWNumeric.Value = VariableTag.ValueReal;
            VariableTag = Global.Variables.GetByName(VarName + "_HW");
            if (VariableTag != null) HWNumeric.Value = VariableTag.ValueReal;
            VariableTag = Global.Variables.GetByName(VarName + "_LF");
            if (VariableTag != null) LFNumeric.Value = VariableTag.ValueReal;
            VariableTag = Global.Variables.GetByName(VarName + "_HF");
            if (VariableTag != null) HFNumeric.Value = VariableTag.ValueReal;
            // Настройки датчика
            VariableTag = Global.Variables.GetByName(VarName + "_LowLevel");
            if (VariableTag != null) LowLevelNumeric.Value = VariableTag.ValueReal;
            VariableTag = Global.Variables.GetByName(VarName + "_HiLevel");
            if (VariableTag != null) HiLevelNumeric.Value = (VariableTag.ValueReal);
            VariableTag = Global.Variables.GetByName(VarName + "_LowCurr");
            if (VariableTag != null) LowCurrNumeric.Value = VariableTag.ValueReal;
            VariableTag = Global.Variables.GetByName(VarName + "_HiCurr");
            if (VariableTag != null) HiCurrNumeric.Value = VariableTag.ValueReal;

            // RepaintTimer.Enabled = true;
            GroupBox1.IsEnabled = Global.Access;
            GroupBox2.IsEnabled = Global.Access;
            GroupBox3.IsEnabled = Global.Access;
        
            //private void Window_Loaded(object sender, RoutedEventArgs e)
            //{
            //    System.Windows.Threading.DispatcherTimer timer = new();

            //    timer.Tick += new EventHandler(timerTick);
            //    timer.Interval = new TimeSpan(0, 0, 0, 100);
            //    timer.Start();
            //}

            //private void timerTick(object sender, EventArgs e)

            // Автоматический
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
            RBAuto.Background = this.Background;
                if (ManualVariable != null)
                {
                    if (ManualVariable.ValueReal< 1)
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
            // Значение ручного режима
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");
                    if (ManualValueNumeric.IsFocused == false)
                    {
                ManualValueNumeric.Background = NormalColor;
                if (ManualValueVariable != null)
                    if (Math.Abs(ManualValueVariable.ValueReal - Convert.ToDouble(ManualValueNumeric.Value)) >= 0.001)
                        ManualValueNumeric.Background = EditColor;
            }
            // Нижнее аварийное значение
            TVariableTag LFVariable = Global.Variables.GetByName(VarName + "_LF");
                    if (!LFNumeric.IsFocused)
                    {
                LFNumeric.Background = NormalColor;
                if (LFVariable != null)
                    if (Math.Abs(LFVariable.ValueReal - Convert.ToDouble(LFNumeric.Value)) >= 0.001)
                        LFNumeric.Background = EditColor;
            }
            // Нижнее предаварийное значение
            TVariableTag LWVariable = Global.Variables.GetByName(VarName + "_LW");
                    if (!LWNumeric.IsFocused)
                    {
                LWNumeric.Background = NormalColor;
                if (LWVariable != null)
                    if (Math.Abs(LWVariable.ValueReal - Convert.ToDouble(LWNumeric.Value)) >= 0.001)
                        LWNumeric.Background = EditColor;
            }
            // Верхнее предаварийное значение
            TVariableTag HWVariable = Global.Variables.GetByName(VarName + "_HW");
                    if (!HWNumeric.IsFocused)
                    {
                HWNumeric.Background = NormalColor;
                if (HWVariable != null)
                    if (Math.Abs(HWVariable.ValueReal - Convert.ToDouble(HWNumeric.Value)) >= 0.001)
                        HWNumeric.Background = EditColor;
            }
            // Верхнее аварийное значение
            TVariableTag HFVariable = Global.Variables.GetByName(VarName + "_HF");
                    if (!HFNumeric.IsFocused)
                    {
                HFNumeric.Background = NormalColor;
                if (HFVariable != null)
                    if (Math.Abs(HFVariable.ValueReal - Convert.ToDouble(HFNumeric.Value)) >= 0.001)
                HFNumeric.Background = EditColor;
            }
            // Режим работы и значение ручного режима
            if (RBAuto.IsChecked == true)
            {
                 ManualValueTitle.Visibility = Visibility.Hidden;
                 ManualValueUnits.Visibility = Visibility.Hidden;
                 ManualValueNumeric.Visibility = Visibility.Hidden;
                 RBManual.IsChecked = false;
             }
                     if (RBManual.IsChecked == true)
                     {
                 ManualValueTitle.Visibility = Visibility.Visible;
                 ManualValueUnits.Visibility = Visibility.Visible;
                 ManualValueNumeric.Visibility = Visibility.Visible;
                 RBAuto.IsChecked = false;
             }
             // Нижняя граница измеряемого давления
             TVariableTag LowLevelVariable = Global.Variables.GetByName(VarName + "_LowLevel");
                     if (!LowLevelNumeric.IsFocused)
                     {
                 LowLevelNumeric.Background = NormalColor;
                 if (LowLevelVariable != null)
                     if (Math.Abs(LowLevelVariable.ValueReal - Convert.ToDouble(LowLevelNumeric.Value)) >= 0.001)
                         LowLevelNumeric.Background = EditColor;
             }
             // Верхняя граница измеряемого давления
             TVariableTag HiLevelVariable = Global.Variables.GetByName(VarName + "_HiLevel");
            if (!HiLevelNumeric.IsFocused)
            {
                HiLevelNumeric.Background = NormalColor;
                if (HiLevelVariable != null)
                    if (Math.Abs(HiLevelVariable.ValueReal - Convert.ToDouble(HiLevelNumeric.Value)) >= 0.001)
                        HiLevelNumeric.Background = EditColor;
            }
            // Нижняя граница токовой петли
            TVariableTag LowCurrVariable = Global.Variables.GetByName(VarName + "_LowCurr");
                    if (!LowCurrNumeric.IsFocused)
                    {
                LowCurrNumeric.Background = NormalColor;
                if (LowCurrVariable != null)
                    if (Math.Abs(LowCurrVariable.ValueReal - Convert.ToDouble(LowCurrNumeric.Value)) >= 0.001)
                        LowCurrNumeric.Background = EditColor;
            }
            // Верхняя граница токовой петли
            TVariableTag HiCurrVariable = Global.Variables.GetByName(VarName + "_HiCurr");
                    if (!HiCurrNumeric.IsFocused)
                    {
                HiCurrNumeric.Background = NormalColor;
                if (HiCurrVariable != null)
                    if (Math.Abs(HiCurrVariable.ValueReal - Convert.ToDouble(HiCurrNumeric.Value)) >= 0.001)
                        HiCurrNumeric.Background = EditColor;
            }

            }

            private void RBAuto_Checked(object sender, RoutedEventArgs e)
            {
                // Проверяем, что событие вызвано именно установкой флажка (а не снятием)
                if (RBAuto.IsChecked == true)
            {
                TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
                TCommandTag ManualCommand = Global.Commands.GetByName(VarName + "_Manual");

                if (ManualVariable != null && ManualCommand != null)
                {
                    // Если система в ручном режиме
                    if (ManualVariable.ValueReal > 0)
                    {
                        ManualCommand.WriteValue = "false";
                        ManualCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        Global.Log.Add("Пользователь", Content?.ToString() + ".Переведен в автоматический режим.", 1);         //Не уверен в сontent?.ToString()
                    }
                }
            }
        }

        private void RBAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            // Обработка снятия флажка (переход в ручной режим)
            TVariableTag ManualVariable = Global.Variables.GetByName(VarName + "_Manual");
            TCommandTag ManualCommand = Global.Commands.GetByName(VarName + "_Manual");

            if (ManualVariable != null && ManualCommand != null)
            {
                // Если система в автоматическом режиме
                if (ManualVariable.ValueReal < 1)
                {
                    ManualCommand.WriteValue = "true";
                    ManualCommand.NeedToWrite = true;
                    Global.Commands.SendToController();
                    Global.Log.Add("Пользователь", Content?.ToString() + ". Переведен в ручной режим.", 1);
                }
            }
        }

        private void ManualValueNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag ManualValueVariable = Global.Variables.GetByName(VarName + "_ManualValue");
            TCommandTag ManualValueCommand = Global.Commands.GetByName(VarName + "_ManualValue");
            if (ManualValueVariable != null)
                if (ManualValueCommand != null)
                    if (ManualValueVariable.ValueReal != Convert.ToDouble(ManualValueNumeric.Value))
                    {
                        ManualValueCommand.WriteValue = ManualValueNumeric.Value.ToString();
                        ManualValueCommand.NeedToWrite = true;
                        Global.Commands.SendToController();
                        Global.Log.Add("Пользователь", Content?.ToString() + ". Ручное значение изменено на " + ManualValueCommand.WriteValue + " %.", 1);
                        }
        }

        private void HFNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag Variable = Global.Variables.GetByName(VarName + "_HF");
            TCommandTag Command = Global.Commands.GetByName(VarName + "_HF");
            NumericUpDown Numeric = HFNumeric;

            if (Variable != null)
                if (Command != null)
                    if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                    {
                        Command.WriteValue = Numeric.Value.ToString();
                        Command.NeedToWrite = true;
                        Global.Commands.SendToController();
                        Global.Log.Add("Пользователь", Content?.ToString() + ". Верзнее аварийное значение изменено на " + Command.WriteValue + " %.", 1);
                    }
        }

        private void HWNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            TVariableTag Variable = Global.Variables.GetByName(VarName + "_HW");
            TCommandTag Command = Global.Commands.GetByName(VarName + "_HW");
            NumericUpDown Numeric = HWNumeric;

            if (Variable != null)
                if (Command != null)
                    if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                    {
                        Command.WriteValue = Numeric.Value.ToString();
                        Command.NeedToWrite = true;
                        Global.Commands.SendToController();
                        Global.Log.Add("Пользователь", Content?.ToString() + ". Верхнее предаварийное значение изменено на " + Command.WriteValue + " %.", 1);
                    }
        }

        private void LWNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_LW");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_LW");
             NumericUpDown Numeric = LWNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ". Нижнее предаварийное значение изменено на " + Command.WriteValue + " %.", 1);
                     }
        }

         private void LFNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
         {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_LF");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_LF");
             NumericUpDown Numeric = LFNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ". Верхнее предаварийное значение изменено на " + Command.WriteValue + " %.", 1);
                     }
         }

         private void LowLevelNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
         {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_LowLevel");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_LowLevel");
             NumericUpDown Numeric = LowLevelNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ". Значение нижней границы измеряемого уровня изменено на " + Command.WriteValue + " %.", 1);
                     }
         }

         private void HiLevelNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
         {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_HiLevel");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_HiLevel");
             NumericUpDown Numeric = HiLevelNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ". Значение верхней границы измеряемого уровня изменено на " + Command.WriteValue + " %.", 1);
                     }
         }

         private void LowCurrNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
         {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_LowCurr");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_LowCurr");
             NumericUpDown Numeric = LowCurrNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ".Значение нижней границы токовой петли изменено на " + Command.WriteValue + " mA.", 1);
                     }
         }

         private void HiCurrNumeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
         {
             TVariableTag Variable = Global.Variables.GetByName(VarName + "_HiCurr");
             TCommandTag Command = Global.Commands.GetByName(VarName + "_HiCurr");
             NumericUpDown Numeric = HiCurrNumeric;

             if (Variable != null)
                 if (Command != null)
                     if (Variable.ValueReal != Convert.ToDouble(Numeric.Value))
                     {
                         Command.WriteValue = Numeric.Value.ToString();
                         Command.NeedToWrite = true;
                         Global.Commands.SendToController();
                         Global.Log.Add("Пользователь", Content?.ToString() + ".Значение верхней границы токовой петли изменено на " + Command.WriteValue + " mA.", 1);
                     }
         }
    }
}
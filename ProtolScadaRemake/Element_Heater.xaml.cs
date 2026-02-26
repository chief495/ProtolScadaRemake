using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_Heater : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_Heater()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (Global == null) return;

                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                HeaterIcon.Source = FindResource("HeaterOffIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_IsWork");
                if (Tag != null && Tag.ValueReal > 0)
                    HeaterIcon.Source = FindResource("HeaterOnIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                    HeaterIcon.Source = FindResource("HeaterChangedIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                    HeaterIcon.Source = FindResource("HeaterFaultIcon") as ImageSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_Heater {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            // TODO: Реализовать диалог для нагревателя
        }
    }
}
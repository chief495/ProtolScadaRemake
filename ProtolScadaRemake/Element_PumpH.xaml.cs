using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_PumpH : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_PumpH()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (TAGNAME != null)
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
                }

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

                PumpIcon.Source = FindResource("PumpHStopIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_IsWork");
                if (Tag != null && Tag.ValueReal > 0)
                    PumpIcon.Source = FindResource("PumpHStartIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                    PumpIcon.Source = FindResource("PumpHChangedIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                    PumpIcon.Source = FindResource("PumpHFaultIcon") as ImageSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_PumpH {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementPump Dialog = new DialogElementPump();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_PumpH: {ex.Message}");
            }
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_PumpUzUnderPanel : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_PumpUzUnderPanel()
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

                if (Global == null || string.IsNullOrEmpty(VarName)) return;

                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                PumpIcon.Source = FindResource("PumpStopIcon") as ImageSource;
                SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb4b4b4"));

                Tag = Global.Variables?.GetByName(VarName + "_IsWork");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpStartIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff2fcc3a"));
                }

                Tag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                {
                    PumpIcon.Source = FindResource("PumpChangedIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff2f208"));
                }

                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpFaultIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff22222"));
                }

                Tag = Global.Variables?.GetByName(VarName + "_Speed");
                if (Tag != null && SpeedBar != null && SpeedText != null)
                {
                    SpeedBar.Value = Tag.ValueReal;
                    SpeedText.Text = $"{Tag.ValueString} %";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_PumpUzUnderPanel {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementPumpUz Dialog = new DialogElementPumpUz();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_PumpUzUnderPanel: {ex.Message}");
            }
        }
    }
}
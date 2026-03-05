using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_ValveV : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_ValveV()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            if (Global == null)
            {
                if (TAGNAME != null && !string.IsNullOrEmpty(VarName))
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
                }
                return;
            }

            try
            {
                if (TAGNAME != null)
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
                }

                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                ValveIcon.Source = FindResource("ValveVPassiveIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_IsClose");
                if (Tag != null && Tag.ValueReal > 0)
                    ValveIcon.Source = FindResource("ValveVCloseIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_IsOpen");
                if (Tag != null && Tag.ValueReal > 0)
                    ValveIcon.Source = FindResource("ValveVOpenIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_IsMoving");
                if (Tag != null && Tag.ValueReal > 0)
                    ValveIcon.Source = FindResource("ValveVMovingIcon") as ImageSource;

                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                    ValveIcon.Source = FindResource("ValveVFaultIcon") as ImageSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_ValveV {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementValve Dialog = new DialogElementValve();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_ValveV: {ex.Message}");
            }
        }
    }
}
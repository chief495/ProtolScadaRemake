using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_DI : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_DI()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (TagNameTextBlock != null)
                {
                    TagNameTextBlock.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
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

                Tag = Global.Variables?.GetByName(VarName + "_Value");
                if (Tag != null)
                {
                    DIIcon.Source = Tag.ValueReal > 0 ?
                        FindResource("DIonIcon") as ImageSource :
                        FindResource("DIoffIcon") as ImageSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_DI {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementDI Dialog = new DialogElementDI();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_DI: {ex.Message}");
            }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_WIT : UserControl
    {
        public Brush WarningColor = Brushes.Yellow;
        public Brush FaultColor = Brushes.Red;
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";
        public string EU { get; set; } = "";
        public string Designation { get; set; } = "";

        public Element_WIT()
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

                if (TextBlockEU != null && !string.IsNullOrEmpty(EU))
                {
                    TextBlockEU.Text = EU;
                }

                if (TextBlockDesignation != null && !string.IsNullOrEmpty(Designation))
                {
                    TextBlockDesignation.Text = Designation;
                }

                if (Global == null) return;

                // Для тензодатчиков используем _Volume
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Volume");
                if (Tag != null && ValueLabel != null)
                    ValueLabel.Text = Tag.ValueString;

                // Ручной режим
                Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                else
                    HandImage.Visibility = Visibility.Hidden;

                // Подсветка аварий
                if (ValueRect != null)
                {
                    ValueRect.Fill = Brushes.Transparent;

                    Tag = Global.Variables?.GetByName(VarName + "_Warning_Low");
                    if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = WarningColor;

                    Tag = Global.Variables?.GetByName(VarName + "_Warning_Hi");
                    if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = WarningColor;

                    Tag = Global.Variables?.GetByName(VarName + "_Fault_Low");
                    if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = FaultColor;

                    Tag = Global.Variables?.GetByName(VarName + "_Fault_Hi");
                    if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = FaultColor;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_WIT {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementAI Dialog = new DialogElementAI();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.EU = EU;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_WIT: {ex.Message}");
            }
        }
    }
}
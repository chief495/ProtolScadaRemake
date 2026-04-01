using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_FM : UserControl
    {
        public Brush WarningColor = Brushes.Yellow;
        public Brush FaultColor = Brushes.Red;
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";
        public string EU { get; set; } = "";
        public string Designation { get; set; } = "";

        public Element_FM()
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

                if (TextBlockDensityEU != null)
                {
                    TextBlockDensityEU.Text = Global.Variables?.GetByName(VarName + "_Density") != null ? "г/см³" : string.Empty;
                }

                // ОСНОВНОЕ ЗНАЧЕНИЕ - массовый расход (кг/мин)
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_MassFlow");
                if (Tag != null && ValueLabel != null)
                {
                    ValueLabel.Text = Tag.ValueString;
                }
                else
                {
                    // Если нет массового, пробуем объемный расход
                    Tag = Global.Variables?.GetByName(VarName + "_VolumeFlow");
                    if (Tag != null && ValueLabel != null)
                    {
                        ValueLabel.Text = Tag.ValueString;
                    }
                }

                // Ручной режим (если есть)
                Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                else
                    HandImage.Visibility = Visibility.Hidden;

                // Подсветка аварий (если есть)
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
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_FM {VarName}: {ex.Message}");
            }
        }

    }
}

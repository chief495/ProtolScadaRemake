using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_LT.xaml
    /// </summary>
    public partial class Element_LT : UserControl
    {
        public System.Drawing.Color WarningColor = System.Drawing.Color.Yellow; // Цвет предупреждения
        public System.Drawing.Color FaultColor = System.Drawing.Color.Red; // Цвет аварии
        public TGlobal Global;
        public Element_LT()
        {
            InitializeComponent();
        }
        //public void UpdateElement()
        //{
        //    Color = System.Drawing.Color.Transparent;

        //    // Ручной режим
        //    TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
        //    if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visible = false;
        //    if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visible = true;
        //    // Текущее значение
        //    Tag = Global.Variables.GetByName(VarName + "_Value");
        //    if (Tag != null) ValueLabel.Text = Tag.ValueString;
        //    // Аварии и предупреждения
        //    ValueLabel.BackColor = Color.Transparent;
        //    Tag = Global.Variables.GetByName(VarName + "_Warning_Low");
        //    if (Tag != null) if (Tag.ValueReal > 0) ValueLabel.BackColor = WarningColor;
        //    Tag = Global.Variables.GetByName(VarName + "_Warning_Hi");
        //    if (Tag != null) if (Tag.ValueReal > 0) ValueLabel.BackColor = WarningColor;
        //    Tag = Global.Variables.GetByName(VarName + "_Fault_Low");
        //    if (Tag != null) if (Tag.ValueReal > 0) ValueLabel.BackColor = FaultColor;
        //    Tag = Global.Variables.GetByName(VarName + "_Fault_Hi");
        //    if (Tag != null) if (Tag.ValueReal > 0) ValueLabel.BackColor = FaultColor;
        //}
        //private void ValueLabel_Click(object sender, EventArgs e)
        //{
        //    DialogElemetLT Dialog = new DialogElemetLT();
        //    Dialog.Text = Description;
        //    Dialog.Global = Global;
        //    Dialog.VarName = VarName;
        //    Dialog.Initialize();
        //    Dialog.ShowDialog();
        //}

    }
}

using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_LT.xaml
    /// </summary>
    public partial class Element_LT : UserControl
    {
        public System.Windows.Media.Brush WarningColor = Brushes.Yellow;  // Цвет предупреждения
        public System.Windows.Media.Brush FaultColor = Brushes.Red;     // Цвет аварии
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_LT()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {

            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Visible;
            // Текущее значение
            Tag = Global.Variables.GetByName(VarName + "_Value");
            if (Tag != null) ValueLabel.Text = Tag.ValueString;
            // Аварии и предупреждения
            ValueRect.Fill = Brushes.Transparent;
            Tag = Global.Variables.GetByName(VarName + "_Warning_Low");
            if (Tag != null) if (Tag.ValueReal > 0) ValueRect.Fill = WarningColor;
            Tag = Global.Variables.GetByName(VarName + "_Warning_Hi");
            if (Tag != null) if (Tag.ValueReal > 0) ValueRect.Fill = WarningColor;
            Tag = Global.Variables.GetByName(VarName + "_Fault_Low");
            if (Tag != null) if (Tag.ValueReal > 0) ValueRect.Fill = FaultColor;
            Tag = Global.Variables.GetByName(VarName + "_Fault_Hi");
            if (Tag != null) if (Tag.ValueReal > 0) ValueRect.Fill = FaultColor;
        }
        private void ValueLabel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogElementLT Dialog = new DialogElementLT();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}
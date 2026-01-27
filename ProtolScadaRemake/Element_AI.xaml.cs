using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_LT.xaml
    /// </summary>
    public partial class Element_AI : UserControl
    {
        public System.Windows.Media.Brush WarningColor = System.Windows.Media.Brushes.Yellow;    // Цвет предупреждения
        public System.Windows.Media.Brush FaultColor = System.Windows.Media.Brushes.Red;         // Цвет аварии
        public string Description = "";                                     // Описание
        public TGlobal Global;
        public string VarName = "";                                         // Основание для имен
        private string _eu;
        private string _designation;
        public string EU                                                    // Единицы обозначения
        {
            get => _eu;
            set
            {
                _eu = value;
                TextBlockEU.Text = value;                                   // Прямое присвоение
            }
        }

        private string Designation                                          //Измеряемы параметр
        {
            get => _designation;
            set
            {
                _designation = value;
                TextBlockDesignation.Text = value;
            }
        }



        public Element_AI()
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
            ValueRect.Fill = System.Windows.Media.Brushes.Transparent;
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
            DialogElementAI Dialog = new DialogElementAI();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.EU = EU;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}
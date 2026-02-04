using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_ValveH.xaml
    /// </summary>

    public partial class Element_3ValveH : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_3ValveH()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {
            if (TAGNAME != null && !string.IsNullOrEmpty(VarName))
            {
                TAGNAME.Text = VarName;
            }
            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Hidden;
            // Положение по умолчанию
            ValveIcon.Source = FindResource("3xValveIcon") as ImageSource;
            // Клапан в закрытом положении
            Tag = Global.Variables.GetByName(VarName + "_IsClose");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("3xValveCloseIcon") as ImageSource;
            // Клапан в открытом положении
            Tag = Global.Variables.GetByName(VarName + "_IsOpen");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("3xValveOpenIcon") as ImageSource;
            // Клапан в движении
            Tag = Global.Variables.GetByName(VarName + "_IsMoving");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("3xValveMoveIcon") as ImageSource;
            // Заклинивание клапана
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("3xValveFaultIcon") as ImageSource;
        }
        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            DialogElementValve Dialog = new DialogElementValve();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}


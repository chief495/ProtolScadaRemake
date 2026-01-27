using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_ValveH.xaml
    /// </summary>

    public partial class Element_ValveH : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_ValveH()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {
            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Hidden;
            // Положение по умолчанию
            ValveIcon.Source = FindResource("ValveHPassiveIcon") as ImageSource;
            // Клапан в закрытом положении
            Tag = Global.Variables.GetByName(VarName + "_IsClose");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("ValveHCloseIcon") as ImageSource;
            // Клапан в открытом положении
            Tag = Global.Variables.GetByName(VarName + "_IsOpen");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("ValveHCOpenIcon") as ImageSource;
            // Клапан в движении
            Tag = Global.Variables.GetByName(VarName + "_IsMoving");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("ValveHMovingIcon") as ImageSource;
            // Заклинивание клапана
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) ValveIcon.Source = FindResource("ValveHCFaultIcon") as ImageSource;
        }
        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            DialogElementValve Dialog = new DialogElementValve();
            Dialog.Content = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}


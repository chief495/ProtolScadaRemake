using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_PumpH.xaml
    /// </summary>
    public partial class Element_PumpHReverse : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_PumpHReverse()
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
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Visible;
            // Состояние по умолчанию
            PumpIcon.Source = FindResource("PumpStopIcon") as ImageSource;
            // Насос включен
            Tag = Global.Variables.GetByName(VarName + "_IsWork");
            if (Tag != null) if (Tag.ValueReal > 0) PumpIcon.Source = FindResource("PumpStartIcon") as ImageSource;
            // Нет подтверждения состояния
            Tag = Global.Variables.GetByName(VarName + "_FeedbackOk");
            if (Tag != null) if (Tag.ValueReal < 1) PumpIcon.Source = FindResource("PumpChangedIcon") as ImageSource;
            // Авария
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) PumpIcon.Source = FindResource("PumpFaultIcon") as ImageSource;
        }
        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            DialogElementPump Dialog = new DialogElementPump();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}
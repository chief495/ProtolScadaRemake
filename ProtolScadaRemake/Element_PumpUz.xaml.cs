using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_PumpH.xaml
    /// </summary>
    public partial class Element_PumpUz : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_PumpUz()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {
            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Visible;
            // Состояние по умолчанию
            PumpIcon.Source = FindResource("PumpStopIcon") as ImageSource;
            SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb4b4b4"));
            // Миксер включен
            Tag = Global.Variables.GetByName(VarName + "_IsWork");
            if (Tag != null) if (Tag.ValueReal > 0) { PumpIcon.Source = FindResource("PumpStartIcon") as ImageSource; SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff2fcc3a")); }
            // Нет подтверждения состояния
            Tag = Global.Variables.GetByName(VarName + "_FeedbackOk");
            if (Tag != null) if (Tag.ValueReal < 1) { PumpIcon.Source = FindResource("PumpChangedIcon") as ImageSource; SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff2f208")); }
            // Авария
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) { PumpIcon.Source = FindResource("PumpFaultIcon") as ImageSource; SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff22222")); }
            // Скорость
            Tag = Global.Variables.GetByName(VarName + "_Speed");
            if (Tag != null) { SpeedBar.Value = Tag.ValueReal; SpeedText.Text = (Tag.ValueString + " ,%"); }
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



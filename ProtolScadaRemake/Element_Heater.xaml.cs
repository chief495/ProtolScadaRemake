using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Heater.xaml
    /// </summary>
    public partial class Element_Heater : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_Heater()
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
            HeaterIcon.Source = FindResource("HeaterOffIcon") as ImageSource;
            // Миксер включен
            Tag = Global.Variables.GetByName(VarName + "_IsWork");
            if (Tag != null) if (Tag.ValueReal > 0) HeaterIcon.Source = FindResource("HeaterOnIcon") as ImageSource;
            // Нет подтверждения состояния
            Tag = Global.Variables.GetByName(VarName + "_FeedbackOk");
            if (Tag != null) if (Tag.ValueReal < 1) HeaterIcon.Source = FindResource("HeaterChangedIcon") as ImageSource;
            // Авария
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) HeaterIcon.Source = FindResource("HeaterFaultIcon") as ImageSource;
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

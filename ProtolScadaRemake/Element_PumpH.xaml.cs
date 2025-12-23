using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Логика взаимодействия для Element_PumpH.xaml
    /// </summary>
    public partial class Element_PumpH : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        private string StopFileName     = "pack://application:,,,/Images/Pump/PumpH_Stop.svg";
        private string WorkFileName     = "pack://application:,,,/Images/Pump/PumpH_Start.svg";
        private string ChangeFileName   = "pack://application:,,,/Images/Pump/PumpH_Changed.svg";
        private string FaultFileName    = "pack://application:,,,/Images/Pump/PumpH_Fault.svg";
        public Element_PumpH()
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
            svgViewbox.Source = new Uri(StopFileName, UriKind.RelativeOrAbsolute);
            // Миксер включен
            Tag = Global.Variables.GetByName(VarName + "_IsWork");
            if (Tag != null) if (Tag.ValueReal > 0) svgViewbox.Source = new Uri(WorkFileName, UriKind.RelativeOrAbsolute);
            // Нет подтверждения состояния
            Tag = Global.Variables.GetByName(VarName + "_FeedbackOk");
            if (Tag != null) if (Tag.ValueReal < 1) svgViewbox.Source = new Uri(ChangeFileName, UriKind.RelativeOrAbsolute);
            // Авария
            Tag = Global.Variables.GetByName(VarName + "_Fault");
            if (Tag != null) if (Tag.ValueReal > 0) svgViewbox.Source = new Uri(FaultFileName, UriKind.RelativeOrAbsolute);
        }
        //private void ValueLabel_Click(object sender, EventArgs e)
        //{
        //    DialogElementPumpH Dialog = new DialogElementPumpH();
        //    Dialog.Text = Description;
        //    Dialog.Global = Global;
        //    Dialog.VarName = VarName;
        //    Dialog.Initialize();
        //    Dialog.ShowDialog();
        //}
       
    }
}



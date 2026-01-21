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
    /// Логика взаимодействия для Element_DI.xaml
    /// </summary>
    public partial class Element_DI : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public Element_DI()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {
            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Visible;
            // Состояние
            Tag = Global.Variables.GetByName(VarName + "_Value");
            if (Tag != null) if (Tag.ValueReal <= 0) DIIcon.Source = FindResource("DIoffIcon") as ImageSource;
            if (Tag != null) if (Tag.ValueReal > 0) DIIcon.Source = FindResource("DIonIcon") as ImageSource;
        }

        private void Picture_Click(object sender, EventArgs e)
        {
            DialogElementDI Dialog = new DialogElementDI();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.Show();
        }
    }
}

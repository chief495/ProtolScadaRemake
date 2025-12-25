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
    /// Логика взаимодействия для EmPage.xaml
    /// </summary>
    public partial class FrameEmPage : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        public FrameEmPage(TGlobal global)
        {
            InitializeComponent();
            Global = global;
            Initialize();
        }

    
        private void Initialize()
        {

           LAHH151.Global = Global;
           LAHH151.VarName = "LAHH151";
           LAHH151.Description = "Датчик уровня LAHH151";
           LAHH151.TagName.Text = "LAHH-151";
           //P651
           P651.Global = Global;
           P651.VarName = "P651";
           P651.Description = "Насос P651";
            
        }
        private void Window_Loaded(object sender, EventArgs e)
        {
            System.Windows.Threading.DispatcherTimer timer = new();

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 100);
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
           // LAHH151.UpdateElement();
           // P651.UpdateElement();
        }
    }
}

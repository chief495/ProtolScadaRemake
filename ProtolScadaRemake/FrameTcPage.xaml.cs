using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameTcPage : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public FrameTcPage(TGlobal global)
        {
            InitializeComponent();
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
        }
    }
}
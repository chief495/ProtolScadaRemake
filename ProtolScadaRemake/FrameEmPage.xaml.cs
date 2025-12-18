using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class FrameEmPage : UserControl
    {
        public FrameEmPage()
        {
            InitializeComponent();
        }
        private void LT2L10_Click(object sender, EventArgs e)
        {
            DialogElementLT Dialog = new DialogElementLT();
        //  Dialog.Content.ToString = "Счетчик воды QM-400";
        //   Dialog.Global = Global;
            Dialog.VarName = "LT2L10";
        //  Dialog.Initialize();
            Dialog.Show();
        }
    }
}
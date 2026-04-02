using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProtolScadaRemake
{
    public partial class Element_MixerHotspot : UserControl
    {
        public TGlobal? Global;
        public string VarName { get; set; } = string.Empty;
        public string Description { get; set; } = "Миксер";

        public Element_MixerHotspot()
        {
            InitializeComponent();
        }

        private void Hotspot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Global == null || string.IsNullOrWhiteSpace(VarName))
                return;

            try
            {
                DialogElementMixer dialog = new DialogElementMixer
                {
                    Title = Description,
                    Global = Global,
                    VarName = VarName
                };
                dialog.Initialize();
                dialog.ShowDialog();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога миксера {VarName}: {ex.Message}");
            }
        }
    }
}

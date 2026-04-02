using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_PumpUzUnderPanel : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        // Максимальная ширина заполнения (внутренняя ширина Border минус отступы)
        private const double MaxFillWidth = 86; // 90 - 2*Margin(1) - 2

        public Element_PumpUzUnderPanel()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (TAGNAME != null)
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
                }

                if (Global == null || string.IsNullOrEmpty(VarName)) return;

                // Ручной режим
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                // Состояние по умолчанию
                PumpIcon.Source = FindResource("PumpHStopIcon") as ImageSource;

                // Насос работает
                Tag = Global.Variables?.GetByName(VarName + "_IsWork");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpHStartIcon") as ImageSource;
                }

                // Нет подтверждения состояния
                Tag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                {
                    PumpIcon.Source = FindResource("PumpHChangedIcon") as ImageSource;
                }

                // Авария
                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpHFaultIcon") as ImageSource;
                }

                // Обновление шкалы скорости
                Tag = Global.Variables?.GetByName(VarName + "_Speed");
                if (Tag != null && SpeedFill != null && SpeedText != null)
                {
                    double speed = Math.Max(0, Math.Min(100, Tag.ValueReal));

                    // Обновляем ширину заполнения
                    SpeedFill.Width = (speed / 100.0) * MaxFillWidth;

                    // Обновляем текст
                    SpeedText.Text = $"{speed:F0} %";
                }
                else
                {
                    if (SpeedFill != null) SpeedFill.Width = 0;
                    if (SpeedText != null) SpeedText.Text = "0 %";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_PumpUzUnderPanel {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementPumpUz Dialog = new DialogElementPumpUz();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_PumpUzUnderPanel: {ex.Message}");
            }
        }
    }
}
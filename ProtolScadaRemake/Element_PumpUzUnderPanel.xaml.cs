using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_PumpUzUnderPanel.xaml
    /// </summary>
    public partial class Element_PumpUzUnderPanel : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        public Element_PumpUzUnderPanel()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (Global == null || string.IsNullOrEmpty(VarName))
                {
                    return;
                }

                // Установка имени тега
                if (TAGNAME != null)
                {
                    TAGNAME.Text = VarName;
                }

                // Ручной режим
                TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }

                // Состояние по умолчанию
                PumpIcon.Source = FindResource("PumpStopIcon") as ImageSource;
                SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb4b4b4"));

                // Насос включен
                Tag = Global.Variables.GetByName(VarName + "_IsWork");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpStartIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff2fcc3a"));
                }

                // Нет подтверждения состояния
                Tag = Global.Variables.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                {
                    PumpIcon.Source = FindResource("PumpChangedIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff2f208"));
                }

                // Авария
                Tag = Global.Variables.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    PumpIcon.Source = FindResource("PumpFaultIcon") as ImageSource;
                    SpeedBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff22222"));
                }

                // Скорость
                Tag = Global.Variables.GetByName(VarName + "_Speed");
                if (Tag != null && SpeedBar != null && SpeedText != null)
                {
                    SpeedBar.Value = Tag.ValueReal;
                    SpeedText.Text = $"{Tag.ValueString} %";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_PumpUzUnderPanel {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Global == null) return;

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
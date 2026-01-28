using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class WaterFillPanel : UserControl
    {
        public event RoutedEventHandler StartButtonClick;
        public event RoutedEventHandler StopButtonClick;

        public TGlobal Global { get; set; }

        private string _tankName = "T-400";
        public string TankName
        {
            get => _tankName;
            set
            {
                _tankName = value;
                if (TankNameText != null)
                    TankNameText.Text = value;
            }
        }

        public WaterFillPanel()
        {
            InitializeComponent();
        }

        public double GetVolume()
        {
            try
            {
                if (VolumeTextBox != null && double.TryParse(VolumeTextBox.Text, out double value))
                    return value;
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public void SetVolume(string value)
        {
            if (VolumeTextBox != null)
                VolumeTextBox.Text = value;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButtonClick?.Invoke(this, e);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButtonClick?.Invoke(this, e);
        }

        private void VolumeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Валидация ввода как в старом проекте
            if (VolumeTextBox != null && StartButton != null)
            {
                try
                {
                    double volume = GetVolume();

                    // Изменение цвета фона
                    if (volume > 0)
                    {
                        VolumeTextBox.Background = new SolidColorBrush(Color.FromRgb(68, 68, 68)); // #444444
                        VolumeTextBox.Foreground = Brushes.White;
                        StartButton.IsEnabled = true;
                    }
                    else
                    {
                        VolumeTextBox.Background = new SolidColorBrush(Color.FromRgb(139, 0, 0)); // Темно-красный
                        VolumeTextBox.Foreground = Brushes.White;
                        StartButton.IsEnabled = false;
                    }
                }
                catch
                {
                    // Ошибка парсинга - красный цвет
                    VolumeTextBox.Background = new SolidColorBrush(Color.FromRgb(139, 0, 0));
                    VolumeTextBox.Foreground = Brushes.White;
                    StartButton.IsEnabled = false;
                }
            }
        }

        // Метод для обновления уставки из глобальных переменных
        public void UpdateFromGlobal()
        {
            if (Global == null || Global.Variables == null) return;

            try
            {
                // Получаем номер емкости (400 или 500)
                string tankNumber = TankName.Replace("T-", "").Replace("T", "");

                // Обновляем уставку объема
                var volumeSpVar = Global.Variables.GetByName($"T{tankNumber}_SpWater");
                if (volumeSpVar != null)
                    SetVolume(volumeSpVar.ValueString);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления WaterTankPanelSimple для {TankName}: {ex.Message}");
            }
        }
    }
}
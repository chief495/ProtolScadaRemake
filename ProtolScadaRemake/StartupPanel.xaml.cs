using System;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class StartupPanel : UserControl
    {
        // Публичные события
        public event RoutedEventHandler StartStartupButtonClick;
        public event RoutedEventHandler StopStartupButtonClick;
        public event RoutedEventHandler AutoModeButtonClick;
        public event RoutedEventHandler OffModeButtonClick;

        // Публичные свойства
        public TGlobal Global { get; set; }
        public string StartupStatus => StartupStatusText?.Text ?? "ОЖИДАНИЕ";

        public StartupPanel()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // Подписка на события кнопок
            if (StartStartupButton != null)
                StartStartupButton.Click += (s, e) => StartStartupButtonClick?.Invoke(this, e);

            if (StopStartupButton != null)
                StopStartupButton.Click += (s, e) => StopStartupButtonClick?.Invoke(this, e);

            if (AutoModeButton != null)
                AutoModeButton.Click += (s, e) => AutoModeButtonClick?.Invoke(this, e);

            if (OffModeButton != null)
                OffModeButton.Click += (s, e) => OffModeButtonClick?.Invoke(this, e);
        }

        // Публичные методы
        public void SetStartupStatus(string status, string color = "#808080")
        {
            if (StartupStatusText != null)
                StartupStatusText.Text = status;

            if (StartupStatusBorder != null)
                StartupStatusBorder.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(color);
        }

        public int GetStartupMassValue()
        {
            if (StartupMassTextBox != null && int.TryParse(StartupMassTextBox.Text, out int value))
                return value;
            return 0;
        }

        public int GetStartupTimeValue()
        {
            if (StartupTimeTextBox != null && int.TryParse(StartupTimeTextBox.Text, out int value))
                return value;
            return 0;
        }

        public void SetStartupMassValue(int value)
        {
            if (StartupMassTextBox != null)
                StartupMassTextBox.Text = value.ToString();
        }

        public void SetStartupTimeValue(int value)
        {
            if (StartupTimeTextBox != null)
                StartupTimeTextBox.Text = value.ToString();
        }

        // Обработчики событий
        public void StartStartupButton_Click(object sender, RoutedEventArgs e)
        {
            StartStartupButtonClick?.Invoke(this, e);
            SetStartupStatus("ЗАТРАВКА", "#FF1E5945");
        }

        public void StopStartupButton_Click(object sender, RoutedEventArgs e)
        {
            StopStartupButtonClick?.Invoke(this, e);
            SetStartupStatus("ОСТАНОВЛЕНО", "#7B001C");
        }

        public void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            AutoModeButtonClick?.Invoke(this, e);
            SetStartupStatus("АВТОМАТ", "#2196F3");
        }

        public void OffModeButton_Click(object sender, RoutedEventArgs e)
        {
            OffModeButtonClick?.Invoke(this, e);
            SetStartupStatus("ВЫКЛ.", "#808080");
        }

        // Обновление из глобальных переменных
        public void UpdateFromGlobal()
        {
            if (Global == null || Global.Variables == null) return;

            var massVar = Global.Variables.GetByName("EM_ReceptZatravkaMass");
            if (massVar != null)
                SetStartupMassValue((int)massVar.ValueReal);

            var timeVar = Global.Variables.GetByName("EM_ReceptZatravkaTime");
            if (timeVar != null)
                SetStartupTimeValue((int)timeVar.ValueReal);

            var statusVar = Global.Variables.GetByName("EM_Rejim");
            if (statusVar != null)
            {
                switch (statusVar.ValueReal)
                {
                    case 0:
                        SetStartupStatus("ВЫКЛ.", "#808080");
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        SetStartupStatus("АВТОМАТ", "#2196F3");
                        break;
                    default:
                        SetStartupStatus("ОШИБКА", "#D32F2F");
                        break;
                }
            }
        }
    }
}
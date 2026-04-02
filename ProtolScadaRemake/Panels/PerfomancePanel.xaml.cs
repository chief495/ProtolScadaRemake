using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake
{
    public partial class PerformancePanel : UserControl
    {
        // Публичные события
        public event RoutedEventHandler SetMassFlowButtonClick;
        public event RoutedEventHandler StartProcessButtonClick;
        public event RoutedEventHandler StopProcessButtonClick;
        public event RoutedEventHandler DojatProcessButtonClick;
        public event RoutedEventHandler EmergencyStopButtonClick;

        // Публичные свойства
        public TGlobal Global { get; set; }

        public PerformancePanel()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // Подписка на события кнопок
            if (SetMassFlowButton != null)
                SetMassFlowButton.Click += (s, e) => SetMassFlowButtonClick?.Invoke(this, e);

            if (StartProcessButton != null)
                StartProcessButton.Click += (s, e) => StartProcessButtonClick?.Invoke(this, e);

            if (StopProcessButton != null)
                StopProcessButton.Click += (s, e) => StopProcessButtonClick?.Invoke(this, e);

            if (DojatProcessButton != null)
                DojatProcessButton.Click += (s, e) => DojatProcessButtonClick?.Invoke(this, e);

            if (EmergencyStopButton != null)
                EmergencyStopButton.Click += (s, e) => EmergencyStopButtonClick?.Invoke(this, e);
        }

        // Публичные методы
        public int GetMassFlowSetpoint()
        {
            if (MassFlowSetpointTextBox != null && int.TryParse(MassFlowSetpointTextBox.Text, out int value))
                return value;
            return 0;
        }

        public void SetMassFlowSetpoint(int value)
        {
            if (MassFlowSetpointTextBox != null)
                MassFlowSetpointTextBox.Text = value.ToString();
        }

        public void SetMassFlowValue(string value)
        {
            if (MassFlowValueText != null)
                MassFlowValueText.Text = value;
        }

        public void SetPressureValue(string value)
        {
            if (PressureValueText != null)
                PressureValueText.Text = value;
        }

        public void SetTemperatureValue(string value)
        {
            if (TemperatureValueText != null)
                TemperatureValueText.Text = value;
        }

        // Обработчики событий
        public void SetMassFlowButton_Click(object sender, RoutedEventArgs e)
        {
            SetMassFlowButtonClick?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine($"Установка производительности: {GetMassFlowSetpoint()} кг/ч");
        }

        public void StartProcessButton_Click(object sender, RoutedEventArgs e)
        {
            StartProcessButtonClick?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine("Запуск процесса");
        }

        public void StopProcessButton_Click(object sender, RoutedEventArgs e)
        {
            StopProcessButtonClick?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine("Остановка процесса");
        }

        public void DojatProcessButton_Click(object sender, RoutedEventArgs e)
        {
            DojatProcessButtonClick?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine("Запуск дожима");
        }

        public void EmergencyStopButton_Click(object sender, RoutedEventArgs e)
        {
            EmergencyStopButtonClick?.Invoke(this, e);
            System.Diagnostics.Debug.WriteLine("Аварийный останов");
        }

        // Обновление из глобальных переменных
        public void UpdateFromGlobal()
        {
            if (Global == null || Global.Variables == null) return;

            // Производительность
            var massFlowVar = Global.Variables.GetByName("EM_AutoMassFlowSp");
            if (massFlowVar != null)
                SetMassFlowSetpoint((int)massFlowVar.ValueReal);

            // Текущая производительность
            var currentFlowVar = Global.Variables.GetByName("FM601_Value");
            if (currentFlowVar != null)
                SetMassFlowValue(currentFlowVar.ValueString);

            // Давление
            var pressureVar = Global.Variables.GetByName("PT601_Value");
            if (pressureVar != null)
                SetPressureValue(pressureVar.ValueString);

            // Температура
            var tempVar = Global.Variables.GetByName("TT602_Value");
            if (tempVar != null)
                SetTemperatureValue(tempVar.ValueString);
        }

        // Метод для обработки изменения уставки
        public void MassFlowSetpointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Здесь можно добавить валидацию ввода
            if (MassFlowSetpointTextBox != null)
            {
                if (int.TryParse(MassFlowSetpointTextBox.Text, out int value))
                {
                    if (value < 0)
                        MassFlowSetpointTextBox.Text = "0";
                    else if (value > 1000)
                        MassFlowSetpointTextBox.Text = "1000";
                }
                else
                {
                    MassFlowSetpointTextBox.Text = "0";
                }
            }
        }
    }
}
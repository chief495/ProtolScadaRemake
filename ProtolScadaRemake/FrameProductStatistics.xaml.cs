using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameProductStatistics : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;

        public FrameProductStatistics(TGlobal global)
        {
            InitializeComponent();
            _global = global;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _repaintTimer = new DispatcherTimer();
            _repaintTimer.Interval = TimeSpan.FromMilliseconds(500); // 0.5 секунды
            _repaintTimer.Tick += RepaintTimer_Tick;
            _repaintTimer.Start();
        }

        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Обновление показаний
                TVariableTag tag;

                // Продукция за смену
                tag = _global.Variables.GetByName("SmenaProductCouner_Volume");
                if (tag != null)
                    CounterEdit.Text = tag.ValueString;
                else
                    CounterEdit.Text = "0";

                // Вся продукция
                tag = _global.Variables.GetByName("TotalProductCouner_Volume");
                if (tag != null)
                    TotalCounterEdit.Text = tag.ValueString;
                else
                    TotalCounterEdit.Text = "0";

                // "Отлипание" команд сброса счетчиков
                TCommandTag command = _global.Commands.GetByName("SmenaProductCounter_Reset");
                if (command != null)
                {
                    if (!command.NeedToWrite && command.WriteValue == "true")
                    {
                        command.WriteValue = "false";
                        command.NeedToWrite = true;
                    }
                }

                command = _global.Commands.GetByName("TotalProductCounter_Reset");
                if (command != null)
                {
                    if (!command.NeedToWrite && command.WriteValue == "true")
                    {
                        command.WriteValue = "false";
                        command.NeedToWrite = true;
                    }
                }

                // Обновление статуса
                StatusText.Text = $"Статус: Обновлено {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Статус: Ошибка {ex.Message}";
            }
        }

        private void ResetCounterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _global.Log.Add("Пользователь",
                    $"Сброс счетчика произведенной продукции за смену. Значение до сброса {CounterEdit.Text}", 1);

                TCommandTag command = _global.Commands.GetByName("SmenaProductCounter_Reset");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;

                    // Показываем подтверждение
                    MessageBox.Show($"Счетчик смены сброшен!\nПредыдущее значение: {CounterEdit.Text}",
                        "Сброс счетчика",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сброса счетчика смены: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetTotalCounterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _global.Log.Add("Пользователь",
                    $"Сброс счетчика всей произведенной продукции. Значение до сброса {TotalCounterEdit.Text}", 1);

                TCommandTag command = _global.Commands.GetByName("TotalProductCounter_Reset");
                if (command != null)
                {
                    command.WriteValue = "true";
                    command.NeedToWrite = true;

                    // Показываем подтверждение
                    MessageBox.Show($"Общий счетчик сброшен!\nПредыдущее значение: {TotalCounterEdit.Text}",
                        "Сброс счетчика",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сброса общего счетчика: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void StopTimer()
        {
            _repaintTimer?.Stop();
        }
    }
}
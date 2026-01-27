using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake
{
    public partial class FrameProductStatistics : UserControl
    {
        private TGlobal _global;
        private DispatcherTimer _repaintTimer;
        private bool _isActive = false; // По умолчанию неактивно
        private DateTime _lastUpdateTime;
        private int _errorCount = 0;
        private const int MAX_ERRORS = 5;
        private const int UPDATE_TIMEOUT_SECONDS = 10;
        private bool _initialized = false;

        public FrameProductStatistics(TGlobal global)
        {
            InitializeComponent();
            _global = global;
            _lastUpdateTime = DateTime.Now;

            // Устанавливаем начальный статус "Загрузка..."
            StatusText.Text = "Статус: Загрузка...";
            StatusBorder.Background = new SolidColorBrush(Colors.Gray);
            StatusBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);

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
                // Проверяем глобальный объект
                if (_global == null || _global.Variables == null)
                {
                    SetInactiveStatus("Нет подключения к системе");
                    return;
                }

                // Обновление показаний
                TVariableTag tag;
                bool dataUpdated = false;

                // Продукция за смену
                tag = _global.Variables.GetByName("SmenaProductCouner_Volume");
                if (tag != null)
                {
                    CounterEdit.Text = tag.ValueString;
                    dataUpdated = true;
                }
                else
                {
                    CounterEdit.Text = "0";
                }

                // Вся продукция
                tag = _global.Variables.GetByName("TotalProductCouner_Volume");
                if (tag != null)
                {
                    TotalCounterEdit.Text = tag.ValueString;
                    dataUpdated = true;
                }
                else
                {
                    TotalCounterEdit.Text = "0";
                }

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

                // Первое обновление - устанавливаем флаг инициализации
                if (!_initialized && dataUpdated)
                {
                    _initialized = true;
                }

                // Проверка активности
                CheckActivityStatus(dataUpdated);

                // Обновление статуса
                UpdateStatusDisplay();

                // Сбрасываем счетчик ошибок при успешном обновлении
                _errorCount = 0;
            }
            catch (Exception ex)
            {
                _errorCount++;

                if (_errorCount >= MAX_ERRORS)
                {
                    SetInactiveStatus($"Ошибка: {ex.Message}");
                }
                else
                {
                    StatusText.Text = $"Статус: Ошибка ({_errorCount}/{MAX_ERRORS}) {DateTime.Now:HH:mm:ss}";
                    StatusBorder.Background = new SolidColorBrush(Colors.Orange);
                    StatusBorder.BorderBrush = new SolidColorBrush(Colors.DarkOrange);
                }
            }
        }

        private void CheckActivityStatus(bool dataUpdated)
        {
            if (dataUpdated)
            {
                _lastUpdateTime = DateTime.Now;

                // После первого успешного обновления считаем систему активной
                if (_initialized)
                {
                    _isActive = true;
                }
                else
                {
                    // Первое обновление - еще не активны, но данные получены
                    _isActive = false;
                }
            }
            else
            {
                // Проверяем таймаут обновления данных
                TimeSpan timeSinceUpdate = DateTime.Now - _lastUpdateTime;
                if (timeSinceUpdate.TotalSeconds > UPDATE_TIMEOUT_SECONDS)
                {
                    _isActive = false;
                }
            }

            // Дополнительные проверки активности
            CheckSystemHealth();
        }

        private void CheckSystemHealth()
        {
            // Проверяем доступность ключевых тегов системы
            TVariableTag healthTag = _global.Variables.GetByName("System_Health");
            if (healthTag != null)
            {
                // Если есть тег здоровья системы, проверяем его значение
                if (healthTag.ValueReal <= 0)
                {
                    _isActive = false;
                }
            }

            // Проверяем связь с контроллером
            TVariableTag commTag = _global.Variables.GetByName("Controller_Communication");
            if (commTag != null)
            {
                if (commTag.ValueReal <= 0)
                {
                    _isActive = false;
                }
            }
        }

        private void SetInactiveStatus(string reason)
        {
            _isActive = false;
            StatusText.Text = $"Статус: {reason} {DateTime.Now:HH:mm:ss}";
            UpdateStatusColor();
        }

        private void UpdateStatusDisplay()
        {
            if (!_initialized)
            {
                StatusText.Text = $"Статус: ИНИЦИАЛИЗАЦИЯ {DateTime.Now:HH:mm:ss}";
                StatusBorder.Background = new SolidColorBrush(Colors.Gray);
                StatusBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                return;
            }

            if (_isActive)
            {
                TimeSpan timeSinceUpdate = DateTime.Now - _lastUpdateTime;
                if (timeSinceUpdate.TotalSeconds < 2)
                {
                    StatusText.Text = $"Статус: АКТИВНО {DateTime.Now:HH:mm:ss}";
                }
                else if (timeSinceUpdate.TotalSeconds < UPDATE_TIMEOUT_SECONDS)
                {
                    StatusText.Text = $"Статус: ОБНОВЛЕНО {timeSinceUpdate.Seconds}с назад {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    StatusText.Text = $"Статус: НЕТ ОБНОВЛЕНИЙ {DateTime.Now:HH:mm:ss}";
                    _isActive = false;
                }
            }
            else
            {
                if (_errorCount > 0 && _errorCount < MAX_ERRORS)
                {
                    StatusText.Text = $"Статус: ОШИБКА ({_errorCount}/{MAX_ERRORS}) {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    StatusText.Text = $"Статус: НЕАКТИВНО {DateTime.Now:HH:mm:ss}";
                }
            }

            UpdateStatusColor();
        }

        private void UpdateStatusColor()
        {
            if (!_initialized)
            {
                // Серый при инициализации
                StatusBorder.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)); // #FF808080
                StatusBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));   // #FF606060
                return;
            }

            if (_isActive)
            {
                // Зеленый при активности
                StatusBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, 180, 0)); // #FF00B400
                StatusBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 150, 0)); // #FF009600
            }
            else
            {
                // Красный при неактивности
                StatusBorder.Background = new SolidColorBrush(Color.FromArgb(255, 220, 0, 0)); // #FFDC0000
                StatusBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 180, 0, 0)); // #FFB40000
            }
        }

        private void ResetCounterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isActive || !_initialized)
                {
                    MessageBox.Show("Невозможно выполнить операцию: система неактивна или не инициализирована",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

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
                if (!_isActive || !_initialized)
                {
                    MessageBox.Show("Невозможно выполнить операцию: система неактивна или не инициализирована",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

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

        public void UpdateGlobal(TGlobal global)
        {
            _global = global;
            _lastUpdateTime = DateTime.Now;
            _isActive = false;
            _initialized = false;
            _errorCount = 0;

            // Сбрасываем статус
            StatusText.Text = "Статус: Загрузка...";
            StatusBorder.Background = new SolidColorBrush(Colors.Gray);
            StatusBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
        }
    }
}
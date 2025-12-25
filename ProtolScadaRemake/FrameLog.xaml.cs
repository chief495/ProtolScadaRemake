using ProtolScadaRemake;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake.Views  // Важно: Views, а не просто ProtolScadaRemake
{
    public partial class FrameLog : UserControl
    {
        private TGlobal _global;
        private ProtolScada.DBUtils _dbUtils;

        // Конструктор без параметров (обязателен для XAML)
        public FrameLog()
        {
            InitializeComponent();
        }

        // Конструктор с параметром (для использования из MainWindow)
        public FrameLog(TGlobal global) : this()
        {
            _global = global;

            // Инициализация DBUtils с вашими настройками
            _dbUtils = new ProtolScada.DBUtils
            {
                DB_HostName = "localhost",
                DB_Port = 3306,
                DB_Name = "protolscadadb",
                DB_UserLogin = "root",
                DB_Password = "advengauser"
            };

            // Настройка UI элементов
            InitializeUI();

            // Загружаем данные из БД
            LoadDataFromDatabaseAsync();
        }

        private void InitializeUI()
        {
            // Установка начальных дат
            dpFrom.SelectedDate = DateTime.Now.AddDays(-7);
            dpTo.SelectedDate = DateTime.Now;

            // Подписка на события фильтров
            cmbPeriod.SelectionChanged += (s, e) => RefreshLog();
            dpFrom.SelectedDateChanged += (s, e) => RefreshLog();
            dpTo.SelectedDateChanged += (s, e) => RefreshLog();

            // Подписка на CheckBox'ы
            chkAlarm.Checked += (s, e) => RefreshLog();
            chkAlarm.Unchecked += (s, e) => RefreshLog();
            chkWarning.Checked += (s, e) => RefreshLog();
            chkWarning.Unchecked += (s, e) => RefreshLog();
            chkError.Checked += (s, e) => RefreshLog();
            chkError.Unchecked += (s, e) => RefreshLog();
            chkFault.Checked += (s, e) => RefreshLog();
            chkFault.Unchecked += (s, e) => RefreshLog();
            chkUser.Checked += (s, e) => RefreshLog();
            chkUser.Unchecked += (s, e) => RefreshLog();
            chkSystem.Checked += (s, e) => RefreshLog();
            chkSystem.Unchecked += (s, e) => RefreshLog();
        }

        public async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                // Получаем настройки фильтров
                DateTime? fromDate = dpFrom.SelectedDate;
                DateTime? toDate = dpTo.SelectedDate?.AddDays(1); // +1 день чтобы включить выбранную дату

                // Загружаем данные
                var records = await _dbUtils.LoadLogRecordsAsync(100, fromDate);

                // Применяем фильтры по типу
                var filteredRecords = FilterRecordsByType(records);

                // Обновляем UI
                lvLog.ItemsSource = null;
                lvLog.ItemsSource = filteredRecords;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки журнала: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Collections.Generic.List<TLogRecord> FilterRecordsByType(System.Collections.Generic.List<TLogRecord> records)
        {
            var filtered = new System.Collections.Generic.List<TLogRecord>();

            foreach (var record in records)
            {
                var recordType = record.GetRecordType();

                if (recordType == "Авария" && chkAlarm.IsChecked == true) filtered.Add(record);
                else if (recordType == "Предупреждение" && chkWarning.IsChecked == true) filtered.Add(record);
                else if (recordType == "Отказ" && chkError.IsChecked == true) filtered.Add(record);
                else if (recordType == "Сбой" && chkFault.IsChecked == true) filtered.Add(record);
                else if (recordType == "Пользователь" && chkUser.IsChecked == true) filtered.Add(record);
                else if (recordType == "Система" && chkSystem.IsChecked == true) filtered.Add(record);
            }

            return filtered;
        }

        public async void RefreshLog()
        {
            await LoadDataFromDatabaseAsync();
        }
    }
}
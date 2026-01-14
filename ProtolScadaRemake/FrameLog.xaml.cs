// FrameLog.xaml.cs - исправленная версия
using ProtolScada;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake.Views
{
    public partial class FrameLog : UserControl
    {
        private TGlobal _global;
        private DBUtils _dbUtils;

        public FrameLog()
        {
            InitializeComponent();
        }

        public FrameLog(TGlobal global) : this()
        {
            _global = global;

            // Получаем DBUtils из TGlobal
            _dbUtils = _global.GetDbUtils();

            InitializeUI();
            LoadDataFromDatabaseAsync();
        }

        private void InitializeUI()
        {
            // Установка начальных дат
            dpFrom.SelectedDate = DateTime.Now.AddDays(-7);
            dpTo.SelectedDate = DateTime.Now;

            // Подписка на события
            cmbPeriod.SelectionChanged += (s, e) => RefreshLog();
            dpFrom.SelectedDateChanged += (s, e) => RefreshLog();
            dpTo.SelectedDateChanged += (s, e) => RefreshLog();

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
                // Определяем период для загрузки
                DateTime? fromDate = null;

                // В зависимости от выбранного периода в ComboBox
                if (cmbPeriod.SelectedIndex == 0) // Сутки
                    fromDate = DateTime.Now.AddDays(-1);
                else if (cmbPeriod.SelectedIndex == 1) // Неделя
                    fromDate = DateTime.Now.AddDays(-7);
                else if (cmbPeriod.SelectedIndex == 2) // Месяц
                    fromDate = DateTime.Now.AddMonths(-1);
                else if (dpFrom.SelectedDate.HasValue) // Произвольный
                    fromDate = dpFrom.SelectedDate.Value;

                // Загружаем данные
                var records = await _dbUtils.LoadLogRecordsAsync(1000, fromDate);

                // Применяем фильтры
                var filteredRecords = FilterRecordsByType(records);

                // Обновляем UI
                lvLog.ItemsSource = filteredRecords;

                // Обновляем статус
                UpdateStatus(filteredRecords.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки журнала: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<TLogRecord> FilterRecordsByType(List<TLogRecord> records)
        {
            var filtered = new List<TLogRecord>();

            foreach (var record in records)
            {
                var recordType = record.GetRecordType();

                if (recordType == "Авария" && chkAlarm.IsChecked == true)
                    filtered.Add(record);
                else if (recordType == "Предупреждение" && chkWarning.IsChecked == true)
                    filtered.Add(record);
                else if (recordType == "Отказ" && chkError.IsChecked == true)
                    filtered.Add(record);
                else if (recordType == "Сбой" && chkFault.IsChecked == true)
                    filtered.Add(record);
                else if (recordType == "Пользователь" && chkUser.IsChecked == true)
                    filtered.Add(record);
                else if (recordType == "Система" && chkSystem.IsChecked == true)
                    filtered.Add(record);
            }

            return filtered;
        }

        private void UpdateStatus(int count)
        {
            // Можно добавить статусную строку в XAML
            // Пока просто выводим в Debug
            System.Diagnostics.Debug.WriteLine($"Загружено записей: {count}");
        }

        public async void RefreshLog()
        {
            await LoadDataFromDatabaseAsync();
        }

        // Метод для добавления тестовой записи (для проверки)
        public async void AddTestRecord()
        {
            try
            {
                var testRecord = new TLogRecord
                {
                    Time = DateTime.Now,
                    GroupName = "Система",
                    Text = "Тестовая запись из WPF приложения",
                    ImageIndex = 0
                };

                await _dbUtils.SaveLogRecordAsync(testRecord);
                await LoadDataFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
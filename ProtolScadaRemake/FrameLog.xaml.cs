using ProtolScada;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

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
            // Инициализация ComboBox с периодами
            cmbPeriod.Items.Clear();
            cmbPeriod.Items.Add("За сутки");
            cmbPeriod.Items.Add("За неделю");
            cmbPeriod.Items.Add("За месяц");
            cmbPeriod.Items.Add("Произвольный период");
            cmbPeriod.SelectedIndex = 0;

            // Установка начальных дат
            dpFrom.SelectedDate = DateTime.Now;
            dpTo.SelectedDate = DateTime.Now;

            // Скрываем панель выбора дат по умолчанию
            DateIntervalPanel.Visibility = Visibility.Collapsed;
            dpFrom.IsEnabled = false;
            dpTo.IsEnabled = false;

            // Подписка на события
            cmbPeriod.SelectionChanged += cmbPeriod_SelectionChanged;
            dpFrom.SelectedDateChanged += (s, e) => {
                if (dpFrom.SelectedDate.HasValue && cmbPeriod.SelectedIndex == 3)
                    RefreshLog();
            };
            dpTo.SelectedDateChanged += (s, e) => {
                if (dpTo.SelectedDate.HasValue && cmbPeriod.SelectedIndex == 3)
                    RefreshLog();
            };

            // Подписка на события CheckBox'ов
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

            // Кнопка Обновить
            btnRefresh.Click += btnRefresh_Click;

            // Кнопка Экспорт
            btnExport.Click += btnExport_Click;
        }

        private void cmbPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPeriod.SelectedItem == null) return;

            var selectedPeriod = cmbPeriod.SelectedItem.ToString();
            var now = DateTime.Now;

            switch (selectedPeriod)
            {
                case "За сутки":
                    // Последние 24 часа
                    dpFrom.SelectedDate = now.AddDays(-1);
                    dpTo.SelectedDate = now;
                    DateIntervalPanel.Visibility = Visibility.Collapsed;
                    dpFrom.IsEnabled = false;
                    dpTo.IsEnabled = false;
                    break;

                case "За неделю":
                    // Последние 7 дней
                    dpFrom.SelectedDate = now.AddDays(-7);
                    dpTo.SelectedDate = now;
                    DateIntervalPanel.Visibility = Visibility.Collapsed;
                    dpFrom.IsEnabled = false;
                    dpTo.IsEnabled = false;
                    break;

                case "За месяц":
                    // Последние 30 дней
                    dpFrom.SelectedDate = now.AddDays(-30);
                    dpTo.SelectedDate = now;
                    DateIntervalPanel.Visibility = Visibility.Collapsed;
                    dpFrom.IsEnabled = false;
                    dpTo.IsEnabled = false;
                    break;

                case "Произвольный период":
                    // Показываем панель выбора дат
                    DateIntervalPanel.Visibility = Visibility.Visible;
                    dpFrom.IsEnabled = true;
                    dpTo.IsEnabled = true;

                    // Устанавливаем диапазон по умолчанию: последние 7 дней
                    if (!dpFrom.SelectedDate.HasValue)
                        dpFrom.SelectedDate = now.AddDays(-7);
                    if (!dpTo.SelectedDate.HasValue)
                        dpTo.SelectedDate = now;
                    break;
            }

            // Автоматически обновляем список при смене периода
            RefreshLog();
        }

        public async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                if (_dbUtils == null || cmbPeriod.SelectedItem == null)
                    return;

                DateTime? fromDate = null;
                DateTime? toDate = DateTime.Now; // По умолчанию до текущего момента

                var selectedPeriod = cmbPeriod.SelectedItem.ToString();

                switch (selectedPeriod)
                {
                    case "За сутки":
                        fromDate = DateTime.Now.AddDays(-1);
                        break;

                    case "За неделю":
                        fromDate = DateTime.Now.AddDays(-7);
                        break;

                    case "За месяц":
                        fromDate = DateTime.Now.AddDays(-30);
                        break;

                    case "Произвольный период":
                        fromDate = dpFrom.SelectedDate;
                        toDate = dpTo.SelectedDate?.AddDays(1).AddSeconds(-1); // Конец выбранного дня
                        break;
                }

                // Проверяем, есть ли новый метод LoadLogRecordsByDateAsync
                List<TLogRecord> records;

                // Пробуем использовать новый метод с двумя параметрами дат
                try
                {
                    // Проверяем наличие метода с тремя параметрами
                    records = await _dbUtils.LoadLogRecordsByDateAsync(fromDate, toDate, 1000);
                }
                catch
                {
                    // Если нет нового метода, используем старый
                    records = await _dbUtils.LoadLogRecordsAsync(1000, fromDate);
                }

                // Применяем фильтры по типу записи
                var filteredRecords = FilterRecordsByType(records);

                // Обновляем UI
                Dispatcher.Invoke(() =>
                {
                    lvLog.ItemsSource = filteredRecords;
                    UpdateStatus(filteredRecords.Count);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка загрузки журнала: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private List<TLogRecord> FilterRecordsByType(List<TLogRecord> records)
        {
            var filtered = new List<TLogRecord>();

            foreach (var record in records)
            {
                // Определяем тип записи
                var recordType = GetRecordTypeFromGroupName(record.GroupName);

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
                // Если тип не определен, показываем как системный
                else if (recordType == "Неизвестный" && chkSystem.IsChecked == true)
                    filtered.Add(record);
            }

            return filtered;
        }

        private string GetRecordTypeFromGroupName(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return "Неизвестный";

            // Приводим к нижнему регистру для сравнения
            var lowerGroupName = groupName.ToLower();

            if (lowerGroupName.Contains("авария") || lowerGroupName.Contains("alarm"))
                return "Авария";
            else if (lowerGroupName.Contains("предупреждение") || lowerGroupName.Contains("warning"))
                return "Предупреждение";
            else if (lowerGroupName.Contains("отказ") || lowerGroupName.Contains("error"))
                return "Отказ";
            else if (lowerGroupName.Contains("сбой") || lowerGroupName.Contains("fault"))
                return "Сбой";
            else if (lowerGroupName.Contains("пользователь") || lowerGroupName.Contains("user"))
                return "Пользователь";
            else if (lowerGroupName.Contains("система") || lowerGroupName.Contains("system"))
                return "Система";
            else
                return "Неизвестный";
        }

        private void UpdateStatus(int count)
        {
            // Можно добавить статусную строку в XAML
            System.Diagnostics.Debug.WriteLine($"Загружено записей: {count}");

            // Если есть TextBlock для статуса в XAML, можно обновить его:
            // txtStatus.Text = $"Найдено записей: {count}";
        }

        public async void RefreshLog()
        {
            await LoadDataFromDatabaseAsync();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataFromDatabaseAsync();
        }

        private async void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем диалог сохранения файла
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовый файл (*.txt)|*.txt|CSV файл (*.csv)|*.csv|Excel файл (*.xlsx)|*.xlsx",
                    DefaultExt = ".csv",
                    FileName = $"Журнал_событий_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Определяем формат экспорта по расширению файла
                    if (saveDialog.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        // Экспорт в Excel
                        await ExportToExcelAsync(saveDialog.FileName);
                    }
                    else
                    {
                        // Экспорт в CSV/TXT
                        await ExportToCsvAsync(saveDialog.FileName);
                    }

                    MessageBox.Show($"Журнал успешно экспортирован в файл:\n{saveDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToCsvAsync(string fileName)
        {
            var records = lvLog.ItemsSource as IEnumerable<TLogRecord>;
            if (records == null) return;

            await Task.Run(() =>
            {
                using (var writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                {
                    // Заголовок с разделителем ;
                    writer.WriteLine("Время;Группа;Текст;Тип записи");

                    foreach (var record in records)
                    {
                        var recordType = GetRecordTypeFromGroupName(record.GroupName);
                        writer.WriteLine($"{record.Time:dd.MM.yyyy HH:mm:ss};{record.GroupName};{record.Text};{recordType}");
                    }
                }
            });
        }

        private async Task ExportToExcelAsync(string fileName)
        {
            var records = lvLog.ItemsSource as IEnumerable<TLogRecord>;
            if (records == null) return;

            await Task.Run(() =>
            {
                try
                {
                    // Используем EPPlus или другой пакет для Excel
                    // Для упрощения экспортируем в CSV, но с расширением .xlsx
                    // Для полноценного Excel нужен пакет EPPlus или ClosedXML

                    using (var writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("Время\tГруппа\tТекст\tТип записи");

                        foreach (var record in records)
                        {
                            var recordType = GetRecordTypeFromGroupName(record.GroupName);
                            writer.WriteLine($"{record.Time:dd.MM.yyyy HH:mm:ss}\t{record.GroupName}\t{record.Text}\t{recordType}");
                        }
                    }

                    // Альтернативно: если установлен EPPlus
                    // ExportWithEPPlus(fileName, records);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка экспорта в Excel: {ex.Message}");
                }
            });
        }

        // Вспомогательный метод для экспорта с использованием EPPlus
        /*
        private void ExportWithEPPlus(string fileName, IEnumerable<TLogRecord> records)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Журнал событий");
                
                // Заголовки
                worksheet.Cells[1, 1].Value = "Время";
                worksheet.Cells[1, 2].Value = "Группа";
                worksheet.Cells[1, 3].Value = "Текст";
                worksheet.Cells[1, 4].Value = "Тип записи";
                
                // Данные
                int row = 2;
                foreach (var record in records)
                {
                    var recordType = GetRecordTypeFromGroupName(record.GroupName);
                    worksheet.Cells[row, 1].Value = record.Time;
                    worksheet.Cells[row, 2].Value = record.GroupName;
                    worksheet.Cells[row, 3].Value = record.Text;
                    worksheet.Cells[row, 4].Value = recordType;
                    row++;
                }
                
                // Автоширина столбцов
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                // Сохраняем файл
                package.SaveAs(new System.IO.FileInfo(fileName));
            }
        }
        */

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
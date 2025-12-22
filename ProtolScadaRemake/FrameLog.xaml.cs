using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;
using ProtolScadaRemake;

namespace ProtolScadaRemake.Views
{
    public partial class FrameLog : UserControl
    {
        private TGlobal _global;
        private DateTime _filterFromDate;
        private DateTime _filterToDate;

        public FrameLog(TGlobal global)
        {
            InitializeComponent();
            _global = global;

            // Устанавливаем даты по умолчанию (последние сутки)
            _filterFromDate = DateTime.Now.AddDays(-1);
            _filterToDate = DateTime.Now;

            dpFrom.SelectedDate = _filterFromDate;
            dpTo.SelectedDate = _filterToDate;

            // Устанавливаем обработчики событий
            dpFrom.SelectedDateChanged += DpFrom_SelectedDateChanged;
            dpTo.SelectedDateChanged += DpTo_SelectedDateChanged;

            chkAlarm.Checked += CheckBox_CheckedChanged;
            chkAlarm.Unchecked += CheckBox_CheckedChanged;
            chkWarning.Checked += CheckBox_CheckedChanged;
            chkWarning.Unchecked += CheckBox_CheckedChanged;
            chkError.Checked += CheckBox_CheckedChanged;
            chkError.Unchecked += CheckBox_CheckedChanged;
            chkFault.Checked += CheckBox_CheckedChanged;
            chkFault.Unchecked += CheckBox_CheckedChanged;
            chkUser.Checked += CheckBox_CheckedChanged;
            chkUser.Unchecked += CheckBox_CheckedChanged;
            chkSystem.Checked += CheckBox_CheckedChanged;
            chkSystem.Unchecked += CheckBox_CheckedChanged;

            // Загружаем данные
            LoadLogData();
        }

        public void RefreshLog()
        {
            LoadLogData();
        }

        private void LoadLogData()
        {
            try
            {
                lvLog.Items.Clear();

                // Получаем все записи
                var allRecords = _global.Log.GetAllRecords();
                int filteredCount = 0;

                foreach (var record in allRecords)
                {
                    // Фильтрация по дате
                    if (record.Time < _filterFromDate || record.Time > _filterToDate.AddDays(1))
                        continue;

                    // Фильтрация по типу записи
                    bool isVisible = IsRecordTypeVisible(record);

                    if (isVisible)
                    {
                        lvLog.Items.Add(record);
                        filteredCount++;
                    }
                }

                // Прокрутка к последней записи (самой новой)
                if (lvLog.Items.Count > 0)
                {
                    lvLog.ScrollIntoView(lvLog.Items[0]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка LoadLogData: {ex.Message}");
            }
        }

        private bool IsRecordTypeVisible(TLogRecord record)
        {
            string recordType = GetRecordType(record);

            switch (recordType)
            {
                case "Авария":
                    return chkAlarm.IsChecked == true;
                case "Предупреждение":
                    return chkWarning.IsChecked == true;
                case "Отказ":
                    return chkError.IsChecked == true;
                case "Сбой":
                    return chkFault.IsChecked == true;
                case "Пользователь":
                    return chkUser.IsChecked == true;
                case "Система":
                    return chkSystem.IsChecked == true;
                default:
                    return true;
            }
        }

        private string GetRecordType(TLogRecord record)
        {
            if (record.GroupName.Contains("Авария")) return "Авария";
            if (record.GroupName.Contains("Предупреждение")) return "Предупреждение";
            if (record.GroupName.Contains("Отказ")) return "Отказ";
            if (record.GroupName.Contains("Сбой") || record.GroupName.Contains("Ошибка")) return "Сбой";
            if (record.GroupName.Contains("Пользователь")) return "Пользователь";
            if (record.GroupName.Contains("Система")) return "Система";

            // По умолчанию по ImageIndex
            return record.ImageIndex switch
            {
                0 => "Система",
                1 => "Пользователь",
                2 => "Предупреждение",
                3 => "Событие",
                4 => "Авария",
                5 => "Отказ",
                _ => "Другое"
            };
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ApplyPeriodFilter();
            LoadLogData();
        }

        private void ApplyPeriodFilter()
        {
            DateTime now = DateTime.Now;

            switch (cmbPeriod.SelectedIndex)
            {
                case 0: // Сутки
                    _filterFromDate = now.AddDays(-1);
                    break;
                case 1: // Неделя
                    _filterFromDate = now.AddDays(-7);
                    break;
                case 2: // Месяц
                    _filterFromDate = now.AddMonths(-1);
                    break;
            }

            _filterToDate = now;

            // Обновляем DatePicker
            dpFrom.SelectedDate = _filterFromDate;
            dpTo.SelectedDate = _filterToDate;
        }

        private void DpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue)
            {
                _filterFromDate = dpFrom.SelectedDate.Value;
                LoadLogData();
            }
        }

        private void DpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpTo.SelectedDate.HasValue)
            {
                _filterToDate = dpTo.SelectedDate.Value;
                LoadLogData();
            }
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            LoadLogData();
        }
    }
}
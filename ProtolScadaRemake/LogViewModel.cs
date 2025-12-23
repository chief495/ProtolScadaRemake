using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ProtolScadaRemake;

namespace ProtolScadaRemake.ViewModels
{
    public class LogViewModel : INotifyPropertyChanged
    {
        private TLogList _logList; // Изменили LogClasses на TLogList
        private ObservableCollection<TLogRecord> _logItems;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TLogRecord> LogItems
        {
            get { return _logItems; }
            set
            {
                _logItems = value;
                OnPropertyChanged(nameof(LogItems));
            }
        }

        public LogViewModel(TLogList logList) // Изменили LogClasses на TLogList
        {
            _logList = logList;
            _logItems = new ObservableCollection<TLogRecord>();
            RefreshLog();
        }

        public void RefreshLog()
        {
            try
            {
                var records = _logList.GetAllRecords();

                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogItems.Clear();
                        foreach (var record in records)
                        {
                            LogItems.Add(record);
                        }

                        OnPropertyChanged(nameof(LogItems));
                        System.Diagnostics.Debug.WriteLine($"Обновлено записей: {LogItems.Count}");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка RefreshLog: {ex.Message}");
            }
        }

        public void ClearLog()
        {
            _logList.Clear();
            RefreshLog();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
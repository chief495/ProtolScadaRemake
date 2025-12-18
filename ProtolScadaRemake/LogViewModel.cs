using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ProtolScadaRemake.ViewModels
{
    public class LogViewModel : INotifyPropertyChanged
    {
        private TLogList _logList;
        private ObservableCollection<TLogRecord> _logItems;
        private DateTime _lastRefreshTime;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TLogRecord> LogItems
        {
            get => _logItems;
            private set
            {
                _logItems = value;
                OnPropertyChanged(nameof(LogItems));
            }
        }

        public LogViewModel(TLogList logList)
        {
            _logList = logList;
            _logItems = new ObservableCollection<TLogRecord>();
            _lastRefreshTime = DateTime.Now;
            RefreshLog();
        }

        public void RefreshLog()
        {
            try
            {
                // ВАЖНО: Получаем актуальный список записей
                int recordCount = _logList.GetCount();

                LogItems.Clear();

                // Добавляем все записи в обратном порядке (новые сверху)
                for (int i = recordCount - 1; i >= 0; i--)
                {
                    if (i < _logList.Items.Length)
                    {
                        LogItems.Add(_logList.Items[i]);
                    }
                }

                _lastRefreshTime = DateTime.Now;
                OnPropertyChanged(nameof(LogItems));

                // Для отладки
                Console.WriteLine($"Обновлен журнал. Записей: {recordCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении журнала: {ex.Message}");
            }
        }

        public void ClearLog()
        {
            _logList.Clear();
            LogItems.Clear();
            _lastRefreshTime = DateTime.Now;
            OnPropertyChanged(nameof(LogItems));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
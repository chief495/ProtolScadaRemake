using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using ProtolScadaRemake.ViewModels;

namespace ProtolScadaRemake.Views
{
    public partial class FrameLog : UserControl
    {
        private LogViewModel _viewModel;
        private TGlobal _global;
        private DispatcherTimer _updateTimer;

        public FrameLog(TGlobal global)
        {
            InitializeComponent();
            _global = global;

            InitializeViewModel();
            InitializeFilters();
            UpdateStatus();

            // Запускаем таймер для периодического обновления
            StartUpdateTimer();
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(500); // Обновляем 2 раза в секунду
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Обновляем журнал, если он видим
            if (IsVisible)
            {
                RefreshLog();
            }
        }

        // Публичный метод для обновления журнала
        public void RefreshLog()
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshLog();
                UpdateStatus();
                ApplyFilters();
            }
        }

        private void InitializeViewModel()
        {
            _viewModel = new LogViewModel(_global.Log);
            this.DataContext = _viewModel;
        }

        private void InitializeFilters()
        {
            cmbFilter.Items.Add("Все сообщения");
            cmbFilter.Items.Add("Информационные");
            cmbFilter.Items.Add("Действия пользователя");
            cmbFilter.Items.Add("Предупреждения");
            cmbFilter.Items.Add("События");
            cmbFilter.SelectedIndex = 0;
        }

        private void UpdateStatus()
        {
            tbStatus.Text = $"Записей: {lvLog.Items.Count}";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshLog();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите очистить журнал?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _viewModel.ClearLog();
                UpdateStatus();
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.FileName = $"Journal_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("ЖУРНАЛ СОБЫТИЙ");
                        writer.WriteLine($"Создан: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                        writer.WriteLine("=".PadRight(80, '='));

                        foreach (TLogRecord record in lvLog.Items)
                        {
                            string imageType = GetImageTypeText(record.ImageIndex);
                            writer.WriteLine($"{record.Time:dd.MM.yyyy HH:mm:ss} [{record.GroupName}] [{imageType}] {record.Text}");
                        }

                        writer.WriteLine("=".PadRight(80, '='));
                        writer.WriteLine($"Всего записей: {lvLog.Items.Count}");
                    }

                    MessageBox.Show($"Журнал успешно экспортирован в файл:\n{saveFileDialog.FileName}",
                        "Экспорт завершен",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте журнала:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private string GetImageTypeText(Int16 imageIndex)
        {
            return imageIndex switch
            {
                0 => "ИНФО",
                1 => "ПОЛЬЗ.",
                2 => "ПРЕДУПР.",
                3 => "СОБЫТИЕ",
                _ => "НЕИЗВ."
            };
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtFilterGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var collectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(lvLog.ItemsSource);

            if (collectionView != null)
            {
                collectionView.Filter = item =>
                {
                    if (item is TLogRecord record)
                    {
                        bool typeMatch = true;
                        bool groupMatch = true;

                        // Фильтр по типу
                        switch (cmbFilter.SelectedIndex)
                        {
                            case 1: typeMatch = record.ImageIndex == 0; break;
                            case 2: typeMatch = record.ImageIndex == 1; break;
                            case 3: typeMatch = record.ImageIndex == 2; break;
                            case 4: typeMatch = record.ImageIndex == 3; break;
                        }

                        // Фильтр по группе
                        if (!string.IsNullOrEmpty(txtFilterGroup.Text))
                        {
                            groupMatch = record.GroupName.IndexOf(txtFilterGroup.Text,
                                StringComparison.OrdinalIgnoreCase) >= 0;
                        }

                        return typeMatch && groupMatch;
                    }
                    return false;
                };

                UpdateStatus();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Останавливаем таймер при выгрузке
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer = null;
            }
        }
    }
}
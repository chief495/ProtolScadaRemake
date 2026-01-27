using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace ProtolScadaRemake.Views
{
    public partial class FrameTrends : UserControl
    {
        private TGlobal _global;
        private DateTime _fromDate;
        private DateTime _toDate;
        private PlotModel _plotModel;

        // Вспомогательный класс для ComboBox
        public class TrendItem
        {
            public TTrendTag Trend { get; set; }
            public string DisplayText { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        public FrameTrends()
        {
            InitializeComponent();
            InitializePlot();
            SetDefaultDates();
            SetupTimeControls();
        }

        public FrameTrends(TGlobal global) : this()
        {
            _global = global;
            LoadTrends();
        }

        private void InitializePlot()
        {
            _plotModel = new PlotModel
            {
                Title = "Тренды",
                TitleFontSize = 14
            };

            var dateTimeAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Время",
                StringFormat = "HH:mm",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 80
            };
            _plotModel.Axes.Add(dateTimeAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Значение",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            _plotModel.Axes.Add(valueAxis);

            TrendPlot.Model = _plotModel;
        }

        private void SetDefaultDates()
        {
            _fromDate = DateTime.Now.AddHours(-1);
            _toDate = DateTime.Now;
            UpdateDateControls();
        }

        private void SetupTimeControls()
        {
            // Часы
            for (int i = 0; i < 24; i++)
            {
                CmbFromHour.Items.Add(i.ToString("00"));
                CmbToHour.Items.Add(i.ToString("00"));
            }

            // Минуты
            string[] minutes = { "00", "05", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55" };
            foreach (var min in minutes)
            {
                CmbFromMinute.Items.Add(min);
                CmbToMinute.Items.Add(min);
            }

            // Текущее время
            CmbFromHour.SelectedItem = _fromDate.Hour.ToString("00");
            CmbFromMinute.SelectedItem = "00";
            CmbToHour.SelectedItem = _toDate.Hour.ToString("00");
            CmbToMinute.SelectedItem = _toDate.Minute.ToString("00");
        }

        private void UpdateDateControls()
        {
            DpFromDate.SelectedDate = _fromDate;
            DpToDate.SelectedDate = _toDate;

            CmbFromHour.SelectedItem = _fromDate.Hour.ToString("00");
            CmbFromMinute.SelectedItem = _fromDate.Minute.ToString("00");
            CmbToHour.SelectedItem = _toDate.Hour.ToString("00");
            CmbToMinute.SelectedItem = _toDate.Minute.ToString("00");
        }

        private async void LoadTrends()
        {
            try
            {
                CmbTrend.Items.Clear();
                TxtStatus.Text = "Загрузка трендов...";

                // Список для хранения трендов
                List<TTrendTag> availableTrends = new List<TTrendTag>();

                // 1. Используем тренды из глобального объекта
                if (_global?.Trends?.Items != null)
                {
                    foreach (var trend in _global.Trends.Items)
                    {
                        availableTrends.Add(trend);
                    }
                    TxtStatus.Text = $"Используются локальные тренды: {availableTrends.Count}";
                }

                // 2. Если есть TrendManager, пытаемся загрузить из БД
                if (_global != null)
                {
                    try
                    {
                        // Получаем TrendManager через новый метод GetTrendManager()
                        var trendManager = _global.GetTrendManager();
                        if (trendManager != null)
                        {
                            if (!trendManager.IsInitialized)
                            {
                                TxtStatus.Text = "Инициализация подключения к БД...";
                                await trendManager.InitializeAsync();
                            }

                            if (trendManager.IsInitialized)
                            {
                                // Используем GetAllTrends() вместо GetAllTrendNames()
                                var dbTrends = trendManager.GetAllTrends();
                                if (dbTrends != null && dbTrends.Count > 0)
                                {
                                    availableTrends.AddRange(dbTrends);
                                    TxtStatus.Text = $"Загружено {dbTrends.Count} трендов из БД";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки трендов из БД: {ex.Message}");
                    }
                }

                // 3. Заполняем ComboBox
                foreach (var trend in availableTrends)
                {
                    string displayText = !string.IsNullOrEmpty(trend.Description)
                        ? $"{trend.Description} ({trend.Unit})"
                        : $"{trend.Name} ({trend.Unit})";

                    CmbTrend.Items.Add(new TrendItem
                    {
                        Trend = trend,
                        DisplayText = displayText
                    });
                }

                // 4. Выбираем первый если есть
                if (CmbTrend.Items.Count > 0)
                {
                    CmbTrend.SelectedIndex = 0;
                    TxtStatus.Text += $" - выбран первый тренд";
                }
                else
                {
                    TxtStatus.Text = "Нет доступных трендов";
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Ошибка загрузки трендов: {ex.Message}";
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RbCustom.IsChecked != true)
                {
                    UpdateDatesFromPeriod();
                }
                else
                {
                    UpdateDatesFromControls();
                }

                TxtStatus.Text = $"Выбран период: {_fromDate:dd.MM.yyyy HH:mm} - {_toDate:dd.MM.yyyy HH:mm}";
                UpdatePlot();
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDatesFromPeriod()
        {
            if (RbHour.IsChecked == true)
                _fromDate = DateTime.Now.AddHours(-1);
            else if (RbDay.IsChecked == true)
                _fromDate = DateTime.Now.AddDays(-1);
            else if (RbWeek.IsChecked == true)
                _fromDate = DateTime.Now.AddDays(-7);

            _toDate = DateTime.Now;
            UpdateDateControls();
        }

        private void UpdateDatesFromControls()
        {
            if (DpFromDate.SelectedDate.HasValue && DpToDate.SelectedDate.HasValue)
            {
                _fromDate = DpFromDate.SelectedDate.Value;
                _toDate = DpToDate.SelectedDate.Value;

                if (CmbFromHour.SelectedItem != null && CmbFromMinute.SelectedItem != null)
                {
                    int hour = int.Parse(CmbFromHour.SelectedItem.ToString());
                    int minute = int.Parse(CmbFromMinute.SelectedItem.ToString());
                    _fromDate = _fromDate.Date.AddHours(hour).AddMinutes(minute);
                }

                if (CmbToHour.SelectedItem != null && CmbToMinute.SelectedItem != null)
                {
                    int hour = int.Parse(CmbToHour.SelectedItem.ToString());
                    int minute = int.Parse(CmbToMinute.SelectedItem.ToString());
                    _toDate = _toDate.Date.AddHours(hour).AddMinutes(minute);
                }
            }
        }

        private async void UpdatePlot()
        {
            if (CmbTrend.SelectedItem == null)
            {
                TxtStatus.Text = "Выберите тренд для отображения";
                return;
            }

            var trendItem = CmbTrend.SelectedItem as TrendItem;
            if (trendItem?.Trend == null)
                return;

            _plotModel.Series.Clear();
            TxtStatus.Text = $"Загрузка данных для {trendItem.Trend.Description}...";

            try
            {
                // Получаем записи тренда
                var records = trendItem.Trend.GetRecordsByTimeRange(_fromDate, _toDate);

                if (records == null || records.Count == 0)
                {
                    TxtStatus.Text = $"Нет данных за выбранный период";
                    _plotModel.Title = $"Тренд: {trendItem.Trend.Description} (нет данных)";
                    _plotModel.InvalidatePlot(true);
                    return;
                }

                // Создаем серию для графика
                var lineSeries = new LineSeries
                {
                    Title = trendItem.Trend.Description,
                    Color = OxyColors.Blue,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.Red,
                    MarkerFill = OxyColors.White,
                    StrokeThickness = 2
                };

                // Добавляем точки
                foreach (var record in records)
                {
                    lineSeries.Points.Add(new DataPoint(
                        DateTimeAxis.ToDouble(record.DateTime),
                        record.ValueReal
                    ));
                }

                // Настраиваем заголовок и оси
                _plotModel.Title = $"Тренд: {trendItem.Trend.Description} ({trendItem.Trend.Unit})";

                var yAxis = _plotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Left) as LinearAxis;
                if (yAxis != null)
                {
                    yAxis.Title = $"Значение, {trendItem.Trend.Unit}";
                }

                // Добавляем серию и обновляем график
                _plotModel.Series.Add(lineSeries);
                _plotModel.InvalidatePlot(true);

                TxtStatus.Text = $"Отображено {records.Count} точек за период: {_fromDate:dd.MM.yyyy HH:mm} - {_toDate:dd.MM.yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Ошибка загрузки данных: {ex.Message}";
            }
        }

        private void BtnYesterday_Click(object sender, RoutedEventArgs e)
        {
            _fromDate = DateTime.Now.AddDays(-1).Date;
            _toDate = DateTime.Now.Date;
            UpdateDateControls();
            RbCustom.IsChecked = true;
        }

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now;
            UpdateDateControls();
            RbCustom.IsChecked = true;
        }

        private void BtnWeek_Click(object sender, RoutedEventArgs e)
        {
            _fromDate = DateTime.Now.AddDays(-7);
            _toDate = DateTime.Now;
            UpdateDateControls();
            RbCustom.IsChecked = true;
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_plotModel.Series.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Инфо",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                FileName = $"Trend_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".png"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var exporter = new PngExporter
                    {
                        Width = 1200,
                        Height = 800
                    };

                    exporter.ExportToFile(_plotModel, dialog.FileName);

                    MessageBox.Show($"График успешно сохранен в {dialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CmbTrend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTrend.SelectedItem != null)
            {
                UpdatePlot();
            }
        }
    }
}
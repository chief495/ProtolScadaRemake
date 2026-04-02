using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake.Views
{
    public partial class FrameTrends : UserControl
    {
        private TGlobal _global;
        private DateTime _fromDate;
        private DateTime _toDate;
        private PlotModel _plotModel;

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
            for (int i = 0; i < 24; i++)
            {
                CmbFromHour.Items.Add(i.ToString("00"));
                CmbToHour.Items.Add(i.ToString("00"));
            }

            string[] minutes = { "00", "05", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55" };
            foreach (var min in minutes)
            {
                CmbFromMinute.Items.Add(min);
                CmbToMinute.Items.Add(min);
            }

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

                List<TTrendTag> availableTrends = new List<TTrendTag>();

                // 1. Локальные тренды из _global.Trends
                if (_global?.Trends?.Items != null && _global.Trends.Items.Length > 0)
                {
                    foreach (var trend in _global.Trends.Items)
                    {
                        availableTrends.Add(trend);
                    }
                }

                // 2. Тренды из DatabaseTrendManager
                if (_global != null)
                {
                    try
                    {
                        var trendManager = _global.GetTrendManager();
                        if (trendManager != null)
                        {
                            if (!trendManager.IsInitialized)
                            {
                                TxtStatus.Text = "Инициализация подключения к БД...";
                                await trendManager.InitializeAsync();
                            }

                            if (trendManager.IsInitialized && trendManager.TrendCount > 0)
                            {
                                var dbTrends = trendManager.GetAllTrends();
                                if (dbTrends != null && dbTrends.Count > 0)
                                {
                                    foreach (var trend in dbTrends)
                                    {
                                        if (!availableTrends.Any(t => t.Name == trend.Name))
                                        {
                                            availableTrends.Add(trend);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ошибка подключения к БД - продолжаем без неё
                    }
                }

                // 3. Дополняем трендами из переменных
                if (_global?.Variables?.Items != null && _global.Variables.Items.Length > 0)
                {
                    var sensorPatterns = new Dictionary<string, (string unit, string prefix)>
                    {
                        { "TT", ("°C", "Температура") },
                        { "PT", ("бар", "Давление") },
                        { "LT", ("%", "Уровень") },
                        { "FM", ("кг/ч", "Расход") },
                        { "QM", ("л", "Счётчик") },
                        { "WIT", ("кг", "Вес") }
                    };

                    var valueSuffixes = new[] { "_Value", "_MassFlow", "_Total", "_MassTotal" };

                    foreach (var variable in _global.Variables.Items)
                    {
                        // Пропускаем если тренд уже есть
                        if (availableTrends.Any(t => t.Name == variable.Name))
                            continue;

                        bool added = false;

                        // Проверяем датчики (TT, PT, LT, FM, QM, WIT)
                        foreach (var pattern in sensorPatterns)
                        {
                            if (variable.Name.StartsWith(pattern.Key))
                            {
                                bool hasSuffix = valueSuffixes.Any(s => variable.Name.EndsWith(s));
                                if (hasSuffix)
                                {
                                    string baseName = variable.Name;
                                    foreach (var suffix in valueSuffixes)
                                    {
                                        baseName = baseName.Replace(suffix, "");
                                    }

                                    string description = $"{pattern.Value.prefix} {baseName}";

                                    var trend = new TTrendTag(
                                        variable.Name,
                                        description,
                                        pattern.Value.unit,
                                        60,
                                        10000
                                    );

                                    availableTrends.Add(trend);
                                    added = true;
                                    break;
                                }
                            }
                        }

                        // Скорости насосов и приводов
                        if (!added && variable.Name.EndsWith("_Speed"))
                        {
                            string baseName = variable.Name.Replace("_Speed", "");
                            string description = $"Скорость {baseName}";

                            var trend = new TTrendTag(variable.Name, description, "%", 10, 10000);
                            availableTrends.Add(trend);
                        }
                    }
                }

                // 4. Заполняем ComboBox
                foreach (var trend in availableTrends.OrderBy(t => t.Name))
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

                // 5. Выбираем первый если есть
                if (CmbTrend.Items.Count > 0)
                {
                    CmbTrend.SelectedIndex = 0;
                    TxtStatus.Text = $"Загружено {CmbTrend.Items.Count} трендов";
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

        private void UpdatePlot()
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
                var records = trendItem.Trend.GetRecordsByTimeRange(_fromDate, _toDate);

                // Если записей нет, пробуем получить текущее значение
                if ((records == null || records.Count == 0) && _global?.Variables != null)
                {
                    var variable = _global.Variables.GetByName(trendItem.Trend.Name);
                    if (variable != null)
                    {
                        records = new List<TTrendTagRecord>
                        {
                            new TTrendTagRecord
                            {
                                DateTime = DateTime.Now,
                                ValueReal = variable.ValueReal,
                                ValueString = variable.ValueString
                            }
                        };
                    }
                }

                if (records == null || records.Count == 0)
                {
                    TxtStatus.Text = $"Нет данных за выбранный период";
                    _plotModel.Title = $"Тренд: {trendItem.Trend.Description} (нет данных)";
                    _plotModel.InvalidatePlot(true);
                    return;
                }

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

                foreach (var record in records)
                {
                    lineSeries.Points.Add(new DataPoint(
                        DateTimeAxis.ToDouble(record.DateTime),
                        record.ValueReal
                    ));
                }

                _plotModel.Title = $"Тренд: {trendItem.Trend.Description} ({trendItem.Trend.Unit})";

                var yAxis = _plotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Left) as LinearAxis;
                if (yAxis != null)
                {
                    yAxis.Title = $"Значение, {trendItem.Trend.Unit}";
                }

                _plotModel.Series.Add(lineSeries);
                _plotModel.InvalidatePlot(true);

                if (records.Count == 1)
                {
                    TxtStatus.Text = $"Текущее значение: {records[0].ValueReal:F2} {trendItem.Trend.Unit}";
                }
                else
                {
                    TxtStatus.Text = $"Отображено {records.Count} точек за период: {_fromDate:dd.MM.yyyy HH:mm} - {_toDate:dd.MM.yyyy HH:mm}";
                }
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

        public void RefreshTrends()
        {
            LoadTrends();
        }
    }
}
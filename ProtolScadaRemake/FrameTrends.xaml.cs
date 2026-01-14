using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProtolScadaRemake.Views
{
    // Вспомогательный класс внутри namespace
    public class TrendItem
    {
        public TTrendTag Trend { get; set; }
        public string DisplayText { get; set; }
    }

    public partial class FrameTrends : UserControl
    {
        private TGlobal _global;
        private DateTime _fromDate;
        private DateTime _toDate;
        private PlotModel _plotModel;
        private DatabaseTrendManager _trendManager;

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
            _trendManager = global.TrendManager;

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

                // 1. Пытаемся загрузить из БД
                if (_trendManager != null)
                {
                    // Инициализируем TrendManager если еще не инициализирован
                    if (!_trendManager.IsInitialized)
                    {
                        TxtStatus.Text = "Инициализация подключения к БД...";
                        await _trendManager.InitializeAsync();
                    }

                    if (_trendManager.IsInitialized)
                    {
                        var dbTrends = _trendManager.GetAllTrends();
                        if (dbTrends.Count > 0)
                        {
                            availableTrends.AddRange(dbTrends);
                            TxtStatus.Text = $"Загружено {dbTrends.Count} трендов из БД";
                        }
                    }
                }

                // 2. Если из БД не загрузилось, используем локальные
                if (availableTrends.Count == 0 && _global?.Trends?.Items != null)
                {
                    foreach (var trend in _global.Trends.Items)
                    {
                        availableTrends.Add(trend);
                    }
                    TxtStatus.Text = $"Используются локальные тренды: {availableTrends.Count}";
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
                    TxtStatus.Text = "Нет доступных трендов. Проверьте подключение к БД.";
                    // Добавляем тестовый тренд для отладки
                    AddTestTrendForDebug();
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Ошибка загрузки трендов: {ex.Message}";

                // Для отладки
                System.Diagnostics.Debug.WriteLine($"LoadTrends error: {ex.Message}\n{ex.StackTrace}");

                // Добавляем тестовый тренд чтобы хоть что-то было видно
                AddTestTrendForDebug();
            }
        }

        private void AddTestTrendForDebug()
        {
            try
            {
                // Создаем тестовый тренд для отладки
                var testTrend = new TTrendTag("TEST_TREND", "Тестовый тренд", "ед.", 60, 1000);

                CmbTrend.Items.Add(new TrendItem
                {
                    Trend = testTrend,
                    DisplayText = "Тестовый тренд (для отладки)"
                });

                CmbTrend.SelectedIndex = 0;
                TxtStatus.Text = "Используется тестовый тренд (БД недоступна)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTestTrendForDebug error: {ex.Message}");
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
                // Если тренд из БД, загружаем данные
                if (_trendManager != null && _trendManager.IsInitialized)
                {
                    // Находим TagID для этого тренда
                    string tagId = FindTagIdForTrend(trendItem.Trend);
                    if (!string.IsNullOrEmpty(tagId))
                    {
                        await trendItem.Trend.LoadFromDatabaseAsync(_fromDate, _toDate, 1000);
                    }
                }

                var records = trendItem.Trend.GetRecordsByTimeRange(_fromDate, _toDate);

                if (records.Count == 0)
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

                TxtStatus.Text = $"Отображено {records.Count} точек за период: {_fromDate:dd.MM.yyyy HH:mm} - {_toDate:dd.MM.yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Ошибка загрузки данных: {ex.Message}";
            }
        }

        private string FindTagIdForTrend(TTrendTag trend)
        {
            // Простой поиск TagID по имени тренда
            if (_trendManager != null && _trendManager.IsInitialized)
            {
                var allTrends = _trendManager.GetAllTrends();
                foreach (var dbTrend in allTrends)
                {
                    if (dbTrend.Name == trend.Name || dbTrend.Description == trend.Description)
                    {
                        return trend.Name; // Используем Name как TagID
                    }
                }
            }
            return trend.Name;
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
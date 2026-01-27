using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProtolScadaRemake.Controls
{
    public partial class FrameReceptPage : UserControl
    {
        private DispatcherTimer _updateTimer;
        private TGlobal _global;
        private string _settingsFileName = "data\\ReceptPageSettings.dat";
        private Dictionary<string, Control> _tagToControlMap = new Dictionary<string, Control>();

        // Флаг режима редактирования
        private bool _isEditMode = false;
        // Список всех полей ввода
        private List<TextBox> _allTextBoxes = new List<TextBox>();

        public TGlobal Global
        {
            get => _global;
            set
            {
                _global = value;
                InitializeData();
            }
        }

        public FrameReceptPage()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeEventHandlers();
            CollectAllTextBoxes();
        }

        private void CollectAllTextBoxes()
        {
            // Собираем все TextBox'ы для управления режимом редактирования
            _allTextBoxes.Clear();

            // РАСТВОР ГРО
            _allTextBoxes.Add(GroRecept_Selitra);
            _allTextBoxes.Add(GroRecept_Water);
            _allTextBoxes.Add(GroRecept_Acid);
            _allTextBoxes.Add(GroRecept_TempMax);
            _allTextBoxes.Add(GroRecept_TempMin);
            _allTextBoxes.Add(GroRecept_TempMaxDelta);
            _allTextBoxes.Add(GroRecept_ScrewBlockTemp);
            _allTextBoxes.Add(GroRecept_ScrewBlockMass);
            _allTextBoxes.Add(P100_NominalSpeed);
            _allTextBoxes.Add(P100_MinSpeed);
            _allTextBoxes.Add(P100_MinMass);

            // РАСТВОР ТС
            _allTextBoxes.Add(TcRecept_Diesel);
            _allTextBoxes.Add(TcRecept_Emulsifier);
            _allTextBoxes.Add(TcRecept_TempT200);
            _allTextBoxes.Add(TcRecept_TempT250);

            // PID P-601
            _allTextBoxes.Add(P601_PID_P);
            _allTextBoxes.Add(P601_PID_I);
            _allTextBoxes.Add(P601_PID_D);
            _allTextBoxes.Add(P601_PID_T);

            // ПРОИЗВОДСТВО ЭМ
            _allTextBoxes.Add(EmRecept_Gro);
            _allTextBoxes.Add(EmRecept_FuelMix);
            _allTextBoxes.Add(EmRecept_WashFuelMass);
            _allTextBoxes.Add(EmRecept_PrimerMass);
            _allTextBoxes.Add(EmRecept_PrimerTime);
            _allTextBoxes.Add(EmRecept_T650Level);

            // PID P-602
            _allTextBoxes.Add(P602_PID_P);
            _allTextBoxes.Add(P602_PID_I);
            _allTextBoxes.Add(P602_PID_D);
            _allTextBoxes.Add(P602_PID_T);

            // ОТГРУЗКА
            _allTextBoxes.Add(Unload_ReverseTime);

            // PID P-651
            _allTextBoxes.Add(P651_PID_P);
            _allTextBoxes.Add(P651_PID_I);
            _allTextBoxes.Add(P651_PID_D);
            _allTextBoxes.Add(P651_PID_T);

            // Устанавливаем начальное состояние (только чтение)
            SetEditMode(false);
        }

        private void SetEditMode(bool isEditMode)
        {
            _isEditMode = isEditMode;
            foreach (var textBox in _allTextBoxes)
            {
                textBox.IsReadOnly = !isEditMode;
                // Меняем фон в зависимости от режима
                textBox.Background = isEditMode ? Brushes.White : (Brush)Application.Current.Resources["LightGrayBrush"] ?? Brushes.LightGray;
            }
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(500);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void InitializeEventHandlers()
        {
            // Привязка обработчиков событий
            GroEditButton.Click += GroEditButton_Click;
            TcEditButton.Click += TcEditButton_Click;
            EmEditButton.Click += EmEditButton_Click;
            UnloadEditButton.Click += UnloadEditButton_Click;
            P601EditButton.Click += P601EditButton_Click;
            P602EditButton.Click += P602EditButton_Click;
            P651EditButton.Click += P651EditButton_Click;

            // Обработчики автонастройки
            P601TuneButton.Click += P601TuneButton_Click;
            P602TuneButton.Click += P602TuneButton_Click;
            P651TuneButton.Click += P651TuneButton_Click;

            CompressorStartButton.Click += CompressorStartButton_Click;
            CompressorStopButton.Click += CompressorStopButton_Click;

            GroRecept_UseAcidCheck.Checked += GroRecept_UseAcidCheck_Changed;
            GroRecept_UseAcidCheck.Unchecked += GroRecept_UseAcidCheck_Changed;
        }

        private void InitializeData()
        {
            if (_global == null) return;
            UpdateData();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            if (_global == null || _global.Variables == null) return;

            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Если режим редактирования, не обновляем значения
                    if (_isEditMode) return;

                    // РАСТВОР ГРО
                    UpdateControlValue(GroRecept_Selitra, "GRO_Recept_Selitra");
                    UpdateControlValue(GroRecept_Water, "GRO_Recept_Water");
                    UpdateControlValue(GroRecept_Acid, "GRO_Recept_Kislota");
                    UpdateControlValue(GroRecept_TempMax, "GRO_Recept_Tmax");
                    UpdateControlValue(GroRecept_TempMin, "GRO_Recept_Tmin");
                    UpdateControlValue(GroRecept_TempMaxDelta, "GRO_Recept_TmaxDelta");
                    UpdateControlValue(GroRecept_ScrewBlockTemp, "GRO_Recept_A100BlockTemp");
                    UpdateControlValue(GroRecept_ScrewBlockMass, "GRO_Recept_A100BlockWeith");
                    UpdateControlValue(P100_NominalSpeed, "P100_SpeedHi");
                    UpdateControlValue(P100_MinSpeed, "P100_SpeedLow");
                    UpdateControlValue(P100_MinMass, "P100_MinMass");

                    var acidEnableTag = _global.Variables.GetByName("GRO_Recept_KislotaEnable");
                    if (acidEnableTag != null)
                    {
                        bool useAcid = acidEnableTag.ValueReal > 0;
                        GroRecept_UseAcidCheck.IsChecked = useAcid;
                        GroRecept_Acid.Visibility = useAcid ? Visibility.Visible : Visibility.Hidden;
                    }

                    // РАСТВОР ТС
                    UpdateControlValue(TcRecept_Diesel, "TC_Recept_Disel");
                    UpdateControlValue(TcRecept_Emulsifier, "TC_Recept_Emulgator");
                    UpdateControlValue(TcRecept_TempT200, "TC_Recept_Temperature_T200");
                    UpdateControlValue(TcRecept_TempT250, "TC_Recept_Temperature_T250");

                    // PID P-601
                    UpdateControlValue(P601_PID_P, "P601_PID_P");
                    UpdateControlValue(P601_PID_I, "P601_PID_I");
                    UpdateControlValue(P601_PID_D, "P601_PID_D");
                    UpdateControlValue(P601_PID_T, "P601_PID_T");

                    // Обновление видимости кнопки автонастройки P-601
                    UpdateTuneButtonVisibility(P601TuneButton, "P601_PID_Tune");

                    // ПРОИЗВОДСТВО ЭМ
                    UpdateControlValue(EmRecept_Gro, "EM_Recept_GRO");
                    UpdateControlValue(EmRecept_FuelMix, "EM_Recept_Disel");
                    UpdateControlValue(EmRecept_WashFuelMass, "EM_ReceptDiaeslLast");
                    UpdateControlValue(EmRecept_PrimerMass, "EM_ReceptZatravkaMass");
                    UpdateControlValue(EmRecept_PrimerTime, "EM_ReceptZatravkaTime");
                    UpdateControlValue(EmRecept_T650Level, "EM_ReceptWorkLevel");

                    // PID P-602
                    UpdateControlValue(P602_PID_P, "P602_PID_P");
                    UpdateControlValue(P602_PID_I, "P602_PID_I");
                    UpdateControlValue(P602_PID_D, "P602_PID_D");
                    UpdateControlValue(P602_PID_T, "P602_PID_T");

                    // Обновление видимости кнопки автонастройки P-602
                    UpdateTuneButtonVisibility(P602TuneButton, "P602_PID_Tune");

                    // ОТГРУЗКА
                    UpdateControlValue(Unload_ReverseTime, "EM_Recept_ReverseTime");

                    // PID P-651
                    UpdateControlValue(P651_PID_P, "P651_PID_P");
                    UpdateControlValue(P651_PID_I, "P651_PID_I");
                    UpdateControlValue(P651_PID_D, "P651_PID_D");
                    UpdateControlValue(P651_PID_T, "P651_PID_T");

                    // Обновление видимости кнопки автонастройки P-651
                    UpdateTuneButtonVisibility(P651TuneButton, "P651_PID_Tune");

                    // КОМПРЕССОР
                    var compressorTag = _global.Variables.GetByName("Compressor_Start");
                    if (compressorTag != null)
                    {
                        if (compressorTag.ValueReal > 0)
                        {
                            CompressorStatus.Text = "ВКЛЮЧЕН";
                            CompressorStatus.Background = Brushes.Green;
                        }
                        else
                        {
                            CompressorStatus.Text = "ОТКЛЮЧЕН";
                            CompressorStatus.Background = Brushes.Gray;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления данных: {ex.Message}");
            }
        }

        private void UpdateControlValue(Control control, string tagName)
        {
            var tag = _global.Variables.GetByName(tagName);
            if (tag != null)
            {
                if (control is TextBox textBox && textBox.Text != tag.ValueString)
                {
                    textBox.Text = tag.ValueString;
                }
                else if (control is CheckBox checkBox)
                {
                    bool isChecked = tag.ValueReal > 0;
                    if (checkBox.IsChecked != isChecked)
                    {
                        checkBox.IsChecked = isChecked;
                    }
                }
            }
        }

        private void UpdateTuneButtonVisibility(Button tuneButton, string tagName)
        {
            var tuneTag = _global.Variables.GetByName(tagName);
            if (tuneTag != null)
            {
                // Если автонастройка активна (значение > 0), скрываем кнопку
                tuneButton.Visibility = tuneTag.ValueReal > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        // Общий метод для сохранения раздела
        private void SaveSection(Dictionary<string, TextBox> textBoxes, Dictionary<string, CheckBox> checkBoxes, string sectionName)
        {
            if (_global == null) return;

            bool changesSaved = false;

            // Сохраняем TextBox'ы
            foreach (var kvp in textBoxes)
            {
                var tag = _global.Variables.GetByName(kvp.Key);
                if (tag != null && double.TryParse(kvp.Value.Text, out double value))
                {
                    tag.ValueReal = value;
                    changesSaved = true;
                }
            }

            // Сохраняем CheckBox'ы
            foreach (var kvp in checkBoxes)
            {
                var tag = _global.Variables.GetByName(kvp.Key);
                if (tag != null)
                {
                    tag.ValueReal = kvp.Value.IsChecked == true ? 1 : 0;
                    changesSaved = true;
                }
            }

            if (changesSaved)
            {
                MessageBox.Show($"Изменения для {sectionName} сохранены", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                // Выходим из режима редактирования после сохранения
                SetEditMode(false);
            }
        }

        // Обработчики кнопок редактирования
        private void GroEditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "GRO_Recept_Selitra", GroRecept_Selitra },
                { "GRO_Recept_Water", GroRecept_Water },
                { "GRO_Recept_Kislota", GroRecept_Acid },
                { "GRO_Recept_Tmax", GroRecept_TempMax },
                { "GRO_Recept_Tmin", GroRecept_TempMin },
                { "GRO_Recept_TmaxDelta", GroRecept_TempMaxDelta },
                { "GRO_Recept_A100BlockTemp", GroRecept_ScrewBlockTemp },
                { "GRO_Recept_A100BlockWeith", GroRecept_ScrewBlockMass },
                { "P100_SpeedHi", P100_NominalSpeed },
                { "P100_SpeedLow", P100_MinSpeed },
                { "P100_MinMass", P100_MinMass }
            };

            var checkBoxes = new Dictionary<string, CheckBox>
            {
                { "GRO_Recept_KislotaEnable", GroRecept_UseAcidCheck }
            };

            SaveSection(textBoxes, checkBoxes, "РАСТВОР ГРО");
        }

        private void TcEditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "TC_Recept_Disel", TcRecept_Diesel },
                { "TC_Recept_Emulgator", TcRecept_Emulsifier },
                { "TC_Recept_Temperature_T200", TcRecept_TempT200 },
                { "TC_Recept_Temperature_T250", TcRecept_TempT250 }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "РАСТВОР ТС");
        }

        private void EmEditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "EM_Recept_GRO", EmRecept_Gro },
                { "EM_Recept_Disel", EmRecept_FuelMix },
                { "EM_ReceptDiaeslLast", EmRecept_WashFuelMass },
                { "EM_ReceptZatravkaMass", EmRecept_PrimerMass },
                { "EM_ReceptZatravkaTime", EmRecept_PrimerTime },
                { "EM_ReceptWorkLevel", EmRecept_T650Level }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "ПРОИЗВОДСТВО ЭМ");
        }

        private void UnloadEditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "EM_Recept_ReverseTime", Unload_ReverseTime }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "ОТГРУЗКА ПРОДУКЦИИ");
        }

        private void P601EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "P601_PID_P", P601_PID_P },
                { "P601_PID_I", P601_PID_I },
                { "P601_PID_D", P601_PID_D },
                { "P601_PID_T", P601_PID_T }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "PID-регулятор P-601");
        }

        private void P602EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "P602_PID_P", P602_PID_P },
                { "P602_PID_I", P602_PID_I },
                { "P602_PID_D", P602_PID_D },
                { "P602_PID_T", P602_PID_T }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "PID-регулятор P-602");
        }

        private void P651EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
            var textBoxes = new Dictionary<string, TextBox>
            {
                { "P651_PID_P", P651_PID_P },
                { "P651_PID_I", P651_PID_I },
                { "P651_PID_D", P651_PID_D },
                { "P651_PID_T", P651_PID_T }
            };

            SaveSection(textBoxes, new Dictionary<string, CheckBox>(), "PID-регулятор P-651");
        }

        // Обработчики автонастройки
        private void P601TuneButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;
            _global.Log.Add("Пользователь", "Автонастройка PID-регулятора P-601", 1);

            var command = _global.Commands.GetByName("P601_PID_Tune");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                command.SendToController();

                // Скрываем кнопку после нажатия
                P601TuneButton.Visibility = Visibility.Collapsed;
            }
        }

        private void P602TuneButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;
            _global.Log.Add("Пользователь", "Автонастройка PID-регулятора P-602", 1);

            var command = _global.Commands.GetByName("P602_PID_Tune");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                command.SendToController();

                // Скрываем кнопку после нажатия
                P602TuneButton.Visibility = Visibility.Collapsed;
            }
        }

        private void P651TuneButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;
            _global.Log.Add("Пользователь", "Автонастройка PID-регулятора P-651", 1);

            var command = _global.Commands.GetByName("P651_PID_Tune");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                command.SendToController();

                // Скрываем кнопку после нажатия
                P651TuneButton.Visibility = Visibility.Collapsed;
            }
        }

        // Обработчики компрессора
        private void CompressorStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;
            _global.Log.Add("Пользователь", "Включение компрессора", 1);

            var command = _global.Commands.GetByName("Compressor_Start");
            if (command != null)
            {
                command.WriteValue = "true";
                command.NeedToWrite = true;
                command.SendToController();
            }
        }

        private void CompressorStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_global == null) return;
            _global.Log.Add("Пользователь", "Отключение компрессора", 1);

            var command = _global.Commands.GetByName("Compressor_Start");
            if (command != null)
            {
                command.WriteValue = "false";
                command.NeedToWrite = true;
                command.SendToController();
            }
        }

        private void GroRecept_UseAcidCheck_Changed(object sender, RoutedEventArgs e)
        {
            GroRecept_Acid.Visibility = (GroRecept_UseAcidCheck.IsChecked == true)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void GroEditButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
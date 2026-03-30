using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
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

        // Флаг: разрешено ли скрывать кнопки автонастройки
        // (запрещаем на первые несколько секунд после запуска)
        private bool _allowHideTuneButtons = false;
        private DateTime _startTime;

        // Словарь: какая секция сейчас в режиме редактирования
        private readonly Dictionary<string, List<TextBox>> _sectionTextBoxes = new();

        // Какие секции сейчас редактируются
        private readonly HashSet<string> _editingSections = new();

        // Маппинг кнопок к секциям
        private readonly Dictionary<Button, string> _buttonToSection = new();

        // Цвета
        private static readonly Brush ReadOnlyBackground = new SolidColorBrush(Color.FromRgb(224, 224, 224));
        private static readonly Brush EditableBackground = Brushes.White;
        private static readonly Brush EditableBorder = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private static readonly Brush ChangedBackground = Brushes.LightYellow;

        // Алиасы тегов
        private static readonly Dictionary<string, string[]> TagAliases = new()
        {
            ["P651_PID_P"] = new[] { "M600_PID_P" },
            ["P651_PID_I"] = new[] { "M600_PID_I" },
            ["P651_PID_D"] = new[] { "M600_PID_D" },
            ["P651_PID_T"] = new[] { "M600_PID_T" },
            ["M600_PID_P"] = new[] { "P651_PID_P" },
            ["M600_PID_I"] = new[] { "P651_PID_I" },
            ["M600_PID_D"] = new[] { "P651_PID_D" },
            ["M600_PID_T"] = new[] { "P651_PID_T" },
            ["P601_PID_Tune"] = new[] { "P601_PID_TuneCmd", "P601_Tune" },
            ["P602_PID_Tune"] = new[] { "P602_PID_TuneCmd", "P602_Tune" },
            ["P651_PID_Tune"] = new[] { "P651_PID_TuneCmd", "P651_Tune", "M600_PID_Tune" },
        };

        private static readonly Dictionary<string, string> TagUnits = new()
        {
            ["GRO_Recept_Selitra"] = "кг",
            ["GRO_Recept_Water"] = "кг",
            ["GRO_Recept_Kislota"] = "кг",
            ["GRO_Recept_Tmax"] = "°C",
            ["GRO_Recept_Tmin"] = "°C",
            ["GRO_Recept_TmaxDelta"] = "°C",
            ["GRO_Recept_A100BlockTemp"] = "°C",
            ["GRO_Recept_A100BlockWeith"] = "кг",
            ["P100_SpeedHi"] = "%",
            ["P100_SpeedLow"] = "%",
            ["P100_MinMass"] = "кг",
            ["TC_Recept_Disel"] = "кг",
            ["TC_Recept_Emulgator"] = "кг",
            ["TC_Recept_Temperature_T200"] = "°C",
            ["TC_Recept_Temperature_T250"] = "°C",
            ["EM_Recept_GRO"] = "кг",
            ["EM_Recept_Disel"] = "кг",
            ["EM_ReceptDiaeslLast"] = "кг",
            ["EM_ReceptZatravkaMass"] = "кг",
            ["EM_ReceptZatravkaTime"] = "с",
            ["EM_ReceptWorkLevel"] = "%",
            ["EM_Recept_ReverseTime"] = "с"
        };

        public TGlobal Global
        {
            get => _global;
            set
            {
                _global = value;
                if (_global != null) UpdateData();
            }
        }

        public FrameReceptPage()
        {
            InitializeComponent();

            _startTime = DateTime.Now;

            // ВАЖНО: Принудительно делаем кнопки видимыми ДО всего остального
            ForceShowTuneButtons();

            InitializeSections();
            InitializeTimer();
            InitializeEventHandlers();

            // Разрешаем скрывать кнопки через 3 секунды после запуска
            var delayTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            delayTimer.Tick += (s, e) =>
            {
                _allowHideTuneButtons = true;
                delayTimer.Stop();
                Debug.WriteLine(">>> Разрешено скрытие кнопок автонастройки");
            };
            delayTimer.Start();

            Debug.WriteLine("========================================");
            Debug.WriteLine("FrameReceptPage: Конструктор завершён");
            Debug.WriteLine($"P601TuneButton.Visibility = {P601TuneButton.Visibility}");
            Debug.WriteLine($"P602TuneButton.Visibility = {P602TuneButton.Visibility}");
            Debug.WriteLine($"P651TuneButton.Visibility = {P651TuneButton.Visibility}");
            Debug.WriteLine("========================================");
        }

        /// <summary>
        /// Принудительно показывает все кнопки автонастройки
        /// </summary>
        private void ForceShowTuneButtons()
        {
            P601TuneButton.Visibility = Visibility.Visible;
            P602TuneButton.Visibility = Visibility.Visible;
            P651TuneButton.Visibility = Visibility.Visible;

            // Также убедимся, что они включены
            P601TuneButton.IsEnabled = true;
            P602TuneButton.IsEnabled = true;
            P651TuneButton.IsEnabled = true;

            Debug.WriteLine("ForceShowTuneButtons: Все кнопки принудительно показаны");
        }

        private void InitializeSections()
        {
            // ======== РАСТВОР ГРО ========
            _sectionTextBoxes["GRO"] = new List<TextBox>
            {
                GroRecept_Selitra, GroRecept_Water, GroRecept_Acid,
                GroRecept_TempMax, GroRecept_TempMin, GroRecept_TempMaxDelta,
                GroRecept_ScrewBlockTemp, GroRecept_ScrewBlockMass,
                P100_NominalSpeed, P100_MinSpeed, P100_MinMass
            };

            // ======== РАСТВОР ТС ========
            _sectionTextBoxes["TC"] = new List<TextBox>
            {
                TcRecept_Diesel, TcRecept_Emulsifier,
                TcRecept_TempT200, TcRecept_TempT250
            };

            // ======== ПРОИЗВОДСТВО ЭМ ========
            _sectionTextBoxes["EM"] = new List<TextBox>
            {
                EmRecept_Gro, EmRecept_FuelMix, EmRecept_WashFuelMass,
                EmRecept_PrimerMass, EmRecept_PrimerTime, EmRecept_T650Level
            };

            // ======== ОТГРУЗКА ========
            _sectionTextBoxes["UNLOAD"] = new List<TextBox>
            {
                Unload_ReverseTime
            };

            // ======== PID P‑601 ========
            _sectionTextBoxes["P601"] = new List<TextBox>
            {
                P601_PID_P, P601_PID_I, P601_PID_D, P601_PID_T
            };

            // ======== PID P‑602 ========
            _sectionTextBoxes["P602"] = new List<TextBox>
            {
                P602_PID_P, P602_PID_I, P602_PID_D, P602_PID_T
            };

            // ======== PID P‑651 ========
            _sectionTextBoxes["P651"] = new List<TextBox>
            {
                P651_PID_P, P651_PID_I, P651_PID_D, P651_PID_T
            };

            // Маппинг кнопок → секций
            _buttonToSection[GroEditButton] = "GRO";
            _buttonToSection[TcEditButton] = "TC";
            _buttonToSection[EmEditButton] = "EM";
            _buttonToSection[UnloadEditButton] = "UNLOAD";
            _buttonToSection[P601EditButton] = "P601";
            _buttonToSection[P602EditButton] = "P602";
            _buttonToSection[P651EditButton] = "P651";

            // По‑умолчанию всё только‑для‑чтения
            foreach (var section in _sectionTextBoxes.Values)
                foreach (var tb in section)
                    tb.IsReadOnly = true;
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void InitializeEventHandlers()
        {
            // Кнопки «Редактировать / Сохранить»
            GroEditButton.Click += EditSaveButton_Click;
            TcEditButton.Click += EditSaveButton_Click;
            EmEditButton.Click += EditSaveButton_Click;
            UnloadEditButton.Click += EditSaveButton_Click;
            P601EditButton.Click += EditSaveButton_Click;
            P602EditButton.Click += EditSaveButton_Click;
            P651EditButton.Click += EditSaveButton_Click;

            // Автонастройка PID
            P601TuneButton.Click += P601TuneButton_Click;
            P602TuneButton.Click += P602TuneButton_Click;
            P651TuneButton.Click += P651TuneButton_Click;

            // Управление компрессором
            CompressorStartButton.Click += CompressorStartButton_Click;
            CompressorStopButton.Click += CompressorStopButton_Click;

            // Чек‑бокс «Кислота»
            GroRecept_UseAcidCheck.Checked += GroRecept_UseAcidCheck_Changed;
            GroRecept_UseAcidCheck.Unchecked += GroRecept_UseAcidCheck_Changed;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            _updateTimer.Stop();
            try { UpdateData(); }
            finally { _updateTimer.Start(); }
        }

        private void UpdateData()
        {
            if (_global?.Variables == null) return;

            try
            {
                // ---------- РАСТВОР ГРО ----------
                UpdateTextBox(GroRecept_Selitra, "GRO_Recept_Selitra", "GRO");
                UpdateTextBox(GroRecept_Water, "GRO_Recept_Water", "GRO");
                UpdateTextBox(GroRecept_Acid, "GRO_Recept_Kislota", "GRO");
                UpdateTextBox(GroRecept_TempMax, "GRO_Recept_Tmax", "GRO");
                UpdateTextBox(GroRecept_TempMin, "GRO_Recept_Tmin", "GRO");
                UpdateTextBox(GroRecept_TempMaxDelta, "GRO_Recept_TmaxDelta", "GRO");
                UpdateTextBox(GroRecept_ScrewBlockTemp, "GRO_Recept_A100BlockTemp", "GRO");
                UpdateTextBox(GroRecept_ScrewBlockMass, "GRO_Recept_A100BlockWeith", "GRO");
                UpdateTextBox(P100_NominalSpeed, "P100_SpeedHi", "GRO");
                UpdateTextBox(P100_MinSpeed, "P100_SpeedLow", "GRO");
                UpdateTextBox(P100_MinMass, "P100_MinMass", "GRO");

                // Чек‑бокс «Кислота»
                if (!_editingSections.Contains("GRO"))
                {
                    var acidTag = _global.Variables.GetByName("GRO_Recept_KislotaEnable");
                    if (acidTag != null)
                    {
                        bool useAcid = acidTag.ValueReal > 0;
                        if (GroRecept_UseAcidCheck.IsChecked != useAcid)
                            GroRecept_UseAcidCheck.IsChecked = useAcid;
                        GroRecept_Acid.Visibility = useAcid ? Visibility.Visible : Visibility.Hidden;
                    }
                }

                // ---------- РАСТВОР ТС ----------
                UpdateTextBox(TcRecept_Diesel, "TC_Recept_Disel", "TC");
                UpdateTextBox(TcRecept_Emulsifier, "TC_Recept_Emulgator", "TC");
                UpdateTextBox(TcRecept_TempT200, "TC_Recept_Temperature_T200", "TC");
                UpdateTextBox(TcRecept_TempT250, "TC_Recept_Temperature_T250", "TC");

                // ---------- PID P‑601 ----------
                UpdateTextBox(P601_PID_P, "P601_PID_P", "P601");
                UpdateTextBox(P601_PID_I, "P601_PID_I", "P601");
                UpdateTextBox(P601_PID_D, "P601_PID_D", "P601");
                UpdateTextBox(P601_PID_T, "P601_PID_T", "P601");
                UpdateTuneButtonVisibility(P601TuneButton, "P601_PID_Tune");

                // ---------- ПРОИЗВОДСТВО ЭМ ----------
                UpdateTextBox(EmRecept_Gro, "EM_Recept_GRO", "EM");
                UpdateTextBox(EmRecept_FuelMix, "EM_Recept_Disel", "EM");
                UpdateTextBox(EmRecept_WashFuelMass, "EM_ReceptDiaeslLast", "EM");
                UpdateTextBox(EmRecept_PrimerMass, "EM_ReceptZatravkaMass", "EM");
                UpdateTextBox(EmRecept_PrimerTime, "EM_ReceptZatravkaTime", "EM");
                UpdateTextBox(EmRecept_T650Level, "EM_ReceptWorkLevel", "EM");

                // ---------- PID P‑602 ----------
                UpdateTextBox(P602_PID_P, "P602_PID_P", "P602");
                UpdateTextBox(P602_PID_I, "P602_PID_I", "P602");
                UpdateTextBox(P602_PID_D, "P602_PID_D", "P602");
                UpdateTextBox(P602_PID_T, "P602_PID_T", "P602");
                UpdateTuneButtonVisibility(P602TuneButton, "P602_PID_Tune");

                // ---------- ОТГРУЗКА ----------
                UpdateTextBox(Unload_ReverseTime, "EM_Recept_ReverseTime", "UNLOAD");

                // ---------- PID P‑651 ----------
                UpdateTextBox(P651_PID_P, "P651_PID_P", "P651");
                UpdateTextBox(P651_PID_I, "P651_PID_I", "P651");
                UpdateTextBox(P651_PID_D, "P651_PID_D", "P651");
                UpdateTextBox(P651_PID_T, "P651_PID_T", "P651");
                UpdateTuneButtonVisibility(P651TuneButton, "P651_PID_Tune");

                // ---------- Компрессор ----------
                var compressorTag = _global.Variables.GetByName("Compressor_Start");
                if (compressorTag != null)
                {
                    bool compressorOn = compressorTag.ValueReal > 0;

                    if (compressorOn)
                    {
                        CompressorStatus.Text = "ВКЛЮЧЕН";
                        CompressorStatus.Background = Brushes.Green;
                    }
                    else
                    {
                        CompressorStatus.Text = "ОТКЛЮЧЕН";
                        CompressorStatus.Background = Brushes.Gray;
                    }

                    if (CompressorPressurePanel != null)
                        CompressorPressurePanel.Visibility = compressorOn ? Visibility.Visible : Visibility.Collapsed;

                    if (compressorOn && CompressorPressureText != null)
                    {
                        var pressureTag = _global.Variables.GetByName("EM_ReceptAlarmPressure");
                        if (pressureTag != null)
                            CompressorPressureText.Text = pressureTag.ValueString;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления данных: {ex.Message}");
            }
        }

        private void UpdateTextBox(TextBox textBox, string tagName, string sectionName)
        {
            if (_editingSections.Contains(sectionName) || textBox.IsFocused) return;

            var tag = FindVariableByNameOrAlias(tagName);
            if (tag != null)
            {
                var normalized = NormalizeNumericValue(tag.ValueString);
                var displayValue = TagUnits.TryGetValue(tagName, out var unit) && !string.IsNullOrWhiteSpace(normalized)
                    ? $"{normalized} {unit}"
                    : normalized;

                if (textBox.Text != displayValue)
                    textBox.Text = displayValue;
            }
        }

        private static string NormalizeNumericValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var cleaned = Regex.Replace(value, @"[^0-9\.,\+\-]", "").Trim();
            cleaned = cleaned.TrimEnd('.', ',');
            return string.IsNullOrWhiteSpace(cleaned) ? value : cleaned;
        }

        private IEnumerable<string> GetAliasCandidates(string tagName)
        {
            yield return tagName;
            if (TagAliases.TryGetValue(tagName, out var aliases))
                foreach (var a in aliases) yield return a;
        }

        private TVariableTag FindVariableByNameOrAlias(string tagName)
        {
            if (_global?.Variables == null) return null;

            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var variable = _global.Variables.GetByName(candidate);
                if (variable != null) return variable;

                variable = FindVariableCaseInsensitive(candidate);
                if (variable != null) return variable;
            }

            return null;
        }

        private TCommandTag FindCommandByNameOrAlias(string tagName)
        {
            if (_global?.Commands == null) return null;

            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var command = _global.Commands.GetByName(candidate);
                if (command != null) return command;

                command = FindCommandCaseInsensitive(candidate);
                if (command != null) return command;
            }

            return null;
        }

        private TVariableTag FindVariableCaseInsensitive(string tagName)
        {
            if (_global?.Variables?.Items == null) return null;
            foreach (var item in _global.Variables.Items)
                if (string.Equals(item.Name, tagName, StringComparison.OrdinalIgnoreCase))
                    return item;
            return null;
        }

        private TCommandTag FindCommandCaseInsensitive(string tagName)
        {
            if (_global?.Commands?.Items == null) return null;
            foreach (var item in _global.Commands.Items)
                if (string.Equals(item.Name, tagName, StringComparison.OrdinalIgnoreCase))
                    return item;
            return null;
        }

        /// <summary>
        /// Обновляет видимость кнопки автонастройки.
        /// ВАЖНО: Кнопка скрывается ТОЛЬКО если:
        /// 1. Прошло более 3 секунд с момента запуска
        /// 2. Тег найден И его значение > 0
        /// </summary>
        private void UpdateTuneButtonVisibility(Button tuneButton, string tagName)
        {
            // Не скрываем кнопки в первые 3 секунды после запуска
            if (!_allowHideTuneButtons)
            {
                // Убеждаемся, что кнопка видна
                if (tuneButton.Visibility != Visibility.Visible)
                {
                    tuneButton.Visibility = Visibility.Visible;
                    Debug.WriteLine($"[ЗАЩИТА] {tagName}: кнопка принудительно показана (защитный период)");
                }
                return;
            }

            var tuneTag = FindVariableByNameOrAlias(tagName);

            if (tuneTag == null)
            {
                // Тег не найден — кнопка ВСЕГДА видима
                if (tuneButton.Visibility != Visibility.Visible)
                {
                    tuneButton.Visibility = Visibility.Visible;
                    Debug.WriteLine($"[ВНИМАНИЕ] Тег {tagName} не найден! Кнопка остаётся видимой.");
                }
                return;
            }

            // Кнопка скрыта ТОЛЬКО когда автонастройка активна (значение > 0)
            bool tuneActive = tuneTag.ValueReal > 0;
            var newVisibility = tuneActive ? Visibility.Collapsed : Visibility.Visible;

            if (tuneButton.Visibility != newVisibility)
            {
                tuneButton.Visibility = newVisibility;
                Debug.WriteLine($"{tagName}: значение = {tuneTag.ValueReal}, кнопка → {newVisibility}");
            }
        }

        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || _global == null) return;

            if (!_global.Access)
            {
                MessageBox.Show("Нет доступа! Введите пароль.", "Доступ запрещён",
                                 MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_buttonToSection.TryGetValue(button, out var sectionName))
            {
                Debug.WriteLine($"Секция для кнопки {button.Name} не найдена");
                return;
            }

            if (_editingSections.Contains(sectionName))
                SaveSection(sectionName, button);
            else
                EnterEditMode(sectionName, button);
        }

        private void EnterEditMode(string sectionName, Button button)
        {
            _editingSections.Add(sectionName);

            if (_sectionTextBoxes.TryGetValue(sectionName, out var tbs))
                foreach (var tb in tbs)
                {
                    tb.IsReadOnly = false;
                    tb.Background = EditableBackground;
                    tb.BorderBrush = EditableBorder;
                    tb.BorderThickness = new Thickness(2);
                }

            button.Content = "СОХРАНИТЬ";
            button.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 90, 158));
            Debug.WriteLine($"Секция {sectionName}: вход в режим редактирования");
        }

        private void SaveSection(string sectionName, Button button)
        {
            if (!_sectionTextBoxes.TryGetValue(sectionName, out var textBoxes))
                return;

            int savedCount = 0, errorCount = 0;

            foreach (var tb in textBoxes)
            {
                var tagName = tb.Tag?.ToString();
                if (string.IsNullOrEmpty(tagName)) continue;

                var variable = FindVariableByNameOrAlias(tagName);
                if (variable == null)
                {
                    Debug.WriteLine($"Переменная не найдена: {tagName}");
                    continue;
                }

                var txt = NormalizeNumericValue(tb.Text.Trim());
                if (!double.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out double newValue))
                {
                    if (!double.TryParse(txt.Replace(',', '.'), NumberStyles.Any,
                                         CultureInfo.InvariantCulture, out newValue))
                    {
                        Debug.WriteLine($"Не удалось распарсить значение «{txt}» для {tagName}");
                        tb.Background = Brushes.LightCoral;
                        errorCount++;
                        continue;
                    }
                }

                if (Math.Abs(variable.ValueReal - newValue) < 0.0001) continue;

                var command = FindCommandByNameOrAlias(tagName);
                if (command != null)
                {
                    command.WriteValue = newValue.ToString(CultureInfo.InvariantCulture);
                    command.NeedToWrite = true;
                    command.SendToController();
                    savedCount++;
                    Debug.WriteLine($"{tagName}: отправлено {newValue}");
                }
                else
                {
                    Debug.WriteLine($"Команда не найдена: {tagName}");
                    errorCount++;
                }
            }

            if (sectionName == "GRO")
                SaveCheckBox(GroRecept_UseAcidCheck, "GRO_Recept_KislotaEnable", ref savedCount, ref errorCount);

            ExitEditMode(sectionName, button);

            if (savedCount > 0)
                _global.Log.Add("Пользователь",
                                 $"Изменены параметры секции {sectionName} ({savedCount} значений)", 1);

            if (errorCount > 0)
                MessageBox.Show($"Сохранено: {savedCount}, Ошибок: {errorCount}",
                                "Результат", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (savedCount > 0)
                Debug.WriteLine($"Секция {sectionName}: сохранено {savedCount} значений");
        }

        private void SaveCheckBox(CheckBox checkBox, string tagName,
                                 ref int savedCount, ref int errorCount)
        {
            var variable = FindVariableByNameOrAlias(tagName);
            if (variable == null) return;

            bool newVal = checkBox.IsChecked == true;
            bool curVal = variable.ValueReal > 0;

            if (newVal != curVal)
            {
                var command = FindCommandByNameOrAlias(tagName);
                if (command != null)
                {
                    command.WriteValue = newVal ? "true" : "false";
                    command.NeedToWrite = true;
                    command.SendToController();
                    savedCount++;
                    Debug.WriteLine($"{tagName}: отправлено {newVal}");
                }
                else
                {
                    errorCount++;
                }
            }
        }

        private void ExitEditMode(string sectionName, Button button)
        {
            _editingSections.Remove(sectionName);

            if (_sectionTextBoxes.TryGetValue(sectionName, out var tbs))
                foreach (var tb in tbs)
                {
                    tb.IsReadOnly = true;
                    tb.Background = ReadOnlyBackground;
                    tb.BorderBrush = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                    tb.BorderThickness = new Thickness(1);
                }

            button.Content = "РЕДАКТИРОВАТЬ";
            button.Background = new SolidColorBrush(Color.FromRgb(30, 89, 69));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 127));

            Debug.WriteLine($"Секция {sectionName}: выход из режима редактирования");
        }

        private void SendCommand(string commandName, string value, string logMessage, bool requireAccess = true)
        {
            if (_global == null)
                return;

            if (requireAccess && !_global.Access)
            {
                MessageBox.Show("Нет доступа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var command = FindCommandByNameOrAlias(commandName);
            if (command != null)
            {
                command.WriteValue = value;
                command.NeedToWrite = true;
                command.SendToController();
                _global.Log.Add("Пользователь", logMessage, 1);
                Debug.WriteLine($"Команда {commandName} отправлена: {value}");
            }
            else
            {
                Debug.WriteLine($"[ОШИБКА] Команда не найдена: {commandName}");
            }
        }

        private void P601TuneButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(">>> Нажата кнопка автонастройки P-601");
            SendCommand("P601_PID_Tune", "true", "Автонастройка PID‑регулятора P‑601", requireAccess: false);
        }

        private void P602TuneButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(">>> Нажата кнопка автонастройки P-602");
            SendCommand("P602_PID_Tune", "true", "Автонастройка PID‑регулятора P‑602", requireAccess: false);
        }

        private void P651TuneButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(">>> Нажата кнопка автонастройки P-651");
            SendCommand("P651_PID_Tune", "true", "Автонастройка PID‑регулятора P‑651", requireAccess: false);
        }

        private void CompressorStartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("Compressor_Start", "true", "Включение компрессора");
        }

        private void CompressorStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("Compressor_Start", "false", "Отключение компрессора");
        }

        private void GroRecept_UseAcidCheck_Changed(object sender, RoutedEventArgs e)
        {
            GroRecept_Acid.Visibility = GroRecept_UseAcidCheck.IsChecked == true
                                        ? Visibility.Visible
                                        : Visibility.Hidden;
        }

        private void GroEditButton_Click_1(object sender, RoutedEventArgs e) { }
    }
}
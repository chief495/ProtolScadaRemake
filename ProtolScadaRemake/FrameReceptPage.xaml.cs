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

        // Словарь: какая секция сейчас в режиме редактирования
        // Ключ = имя секции, Значение = список TextBox'ов этой секции
        private Dictionary<string, List<TextBox>> _sectionTextBoxes = new();

        // Какие секции сейчас редактируются
        private HashSet<string> _editingSections = new();

        // Маппинг кнопок к секциям
        private Dictionary<Button, string> _buttonToSection = new();

        // Цвета
        private static readonly Brush ReadOnlyBackground = new SolidColorBrush(Color.FromRgb(224, 224, 224));
        private static readonly Brush EditableBackground = Brushes.White;
        private static readonly Brush EditableBorder = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private static readonly Brush ChangedBackground = Brushes.LightYellow;


        // Алиасы тегов для совместимости со старым WinForms-проектом
        // (в старой версии PID секция использовала M600_PID_*, в новой может быть P651_PID_*)
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
            InitializeSections();
            InitializeTimer();
            InitializeEventHandlers();
        }

        #region Инициализация

        /// <summary>
        /// Регистрируем секции и их TextBox'ы
        /// </summary>
        private void InitializeSections()
        {
            // РАСТВОР ГРО
            _sectionTextBoxes["GRO"] = new List<TextBox>
            {
                GroRecept_Selitra, GroRecept_Water, GroRecept_Acid,
                GroRecept_TempMax, GroRecept_TempMin, GroRecept_TempMaxDelta,
                GroRecept_ScrewBlockTemp, GroRecept_ScrewBlockMass,
                P100_NominalSpeed, P100_MinSpeed, P100_MinMass
            };

            // РАСТВОР ТС
            _sectionTextBoxes["TC"] = new List<TextBox>
            {
                TcRecept_Diesel, TcRecept_Emulsifier,
                TcRecept_TempT200, TcRecept_TempT250
            };

            // ПРОИЗВОДСТВО ЭМ
            _sectionTextBoxes["EM"] = new List<TextBox>
            {
                EmRecept_Gro, EmRecept_FuelMix, EmRecept_WashFuelMass,
                EmRecept_PrimerMass, EmRecept_PrimerTime, EmRecept_T650Level
            };

            // ОТГРУЗКА
            _sectionTextBoxes["UNLOAD"] = new List<TextBox>
            {
                Unload_ReverseTime
            };

            // PID P-601
            _sectionTextBoxes["P601"] = new List<TextBox>
            {
                P601_PID_P, P601_PID_I, P601_PID_D, P601_PID_T
            };

            // PID P-602
            _sectionTextBoxes["P602"] = new List<TextBox>
            {
                P602_PID_P, P602_PID_I, P602_PID_D, P602_PID_T
            };

            // PID P-651
            _sectionTextBoxes["P651"] = new List<TextBox>
            {
                P651_PID_P, P651_PID_I, P651_PID_D, P651_PID_T
            };

            // Маппинг кнопок к секциям
            _buttonToSection[GroEditButton] = "GRO";
            _buttonToSection[TcEditButton] = "TC";
            _buttonToSection[EmEditButton] = "EM";
            _buttonToSection[UnloadEditButton] = "UNLOAD";
            _buttonToSection[P601EditButton] = "P601";
            _buttonToSection[P602EditButton] = "P602";
            _buttonToSection[P651EditButton] = "P651";

            // Все поля по умолчанию - только чтение
            foreach (var section in _sectionTextBoxes.Values)
            {
                foreach (var tb in section)
                {
                    tb.IsReadOnly = true;
                }
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
            // Все кнопки редактирования используют один обработчик
            GroEditButton.Click += EditSaveButton_Click;
            TcEditButton.Click += EditSaveButton_Click;
            EmEditButton.Click += EditSaveButton_Click;
            UnloadEditButton.Click += EditSaveButton_Click;
            P601EditButton.Click += EditSaveButton_Click;
            P602EditButton.Click += EditSaveButton_Click;
            P651EditButton.Click += EditSaveButton_Click;

            // Автонастройка
            P601TuneButton.Click += P601TuneButton_Click;
            P602TuneButton.Click += P602TuneButton_Click;
            P651TuneButton.Click += P651TuneButton_Click;

            // Компрессор
            CompressorStartButton.Click += CompressorStartButton_Click;
            CompressorStopButton.Click += CompressorStopButton_Click;

            // Чекбокс кислоты
            GroRecept_UseAcidCheck.Checked += GroRecept_UseAcidCheck_Changed;
            GroRecept_UseAcidCheck.Unchecked += GroRecept_UseAcidCheck_Changed;
        }

        #endregion

        #region Обновление данных (live)

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            _updateTimer.Stop();
            try
            {
                UpdateData();
            }
            finally
            {
                _updateTimer.Start();
            }
        }

        private void UpdateData()
        {
            if (_global?.Variables == null) return;

            try
            {
                // РАСТВОР ГРО
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

                // Чекбокс кислоты (обновляем только если секция НЕ редактируется)
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

                // РАСТВОР ТС
                UpdateTextBox(TcRecept_Diesel, "TC_Recept_Disel", "TC");
                UpdateTextBox(TcRecept_Emulsifier, "TC_Recept_Emulgator", "TC");
                UpdateTextBox(TcRecept_TempT200, "TC_Recept_Temperature_T200", "TC");
                UpdateTextBox(TcRecept_TempT250, "TC_Recept_Temperature_T250", "TC");

                // PID P-601
                UpdateTextBox(P601_PID_P, "P601_PID_P", "P601");
                UpdateTextBox(P601_PID_I, "P601_PID_I", "P601");
                UpdateTextBox(P601_PID_D, "P601_PID_D", "P601");
                UpdateTextBox(P601_PID_T, "P601_PID_T", "P601");
                UpdateTuneButtonVisibility(P601TuneButton, "P601_PID_Tune");

                // ПРОИЗВОДСТВО ЭМ
                UpdateTextBox(EmRecept_Gro, "EM_Recept_GRO", "EM");
                UpdateTextBox(EmRecept_FuelMix, "EM_Recept_Disel", "EM");
                UpdateTextBox(EmRecept_WashFuelMass, "EM_ReceptDiaeslLast", "EM");
                UpdateTextBox(EmRecept_PrimerMass, "EM_ReceptZatravkaMass", "EM");
                UpdateTextBox(EmRecept_PrimerTime, "EM_ReceptZatravkaTime", "EM");
                UpdateTextBox(EmRecept_T650Level, "EM_ReceptWorkLevel", "EM");

                // PID P-602
                UpdateTextBox(P602_PID_P, "P602_PID_P", "P602");
                UpdateTextBox(P602_PID_I, "P602_PID_I", "P602");
                UpdateTextBox(P602_PID_D, "P602_PID_D", "P602");
                UpdateTextBox(P602_PID_T, "P602_PID_T", "P602");
                UpdateTuneButtonVisibility(P602TuneButton, "P602_PID_Tune");

                // ОТГРУЗКА
                UpdateTextBox(Unload_ReverseTime, "EM_Recept_ReverseTime", "UNLOAD");

                // PID P-651
                UpdateTextBox(P651_PID_P, "P651_PID_P", "P651");
                UpdateTextBox(P651_PID_I, "P651_PID_I", "P651");
                UpdateTextBox(P651_PID_D, "P651_PID_D", "P651");
                UpdateTextBox(P651_PID_T, "P651_PID_T", "P651");
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновляет TextBox из переменной, но НЕ трогает если:
        /// - секция в режиме редактирования
        /// - поле в фокусе
        /// </summary>
        private static string NormalizeNumericValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            // Убираем дублирующиеся единицы измерения, если они приходят вместе со значением
            var normalized = Regex.Replace(value, @"[^0-9,.+-]", "").Trim();
            return string.IsNullOrWhiteSpace(normalized) ? value : normalized;
        }


        private IEnumerable<string> GetAliasCandidates(string tagName)
        {
            yield return tagName;

            if (TagAliases.TryGetValue(tagName, out var aliases))
            {
                foreach (var alias in aliases)
                    yield return alias;
            }
        }

        private TVariableTag FindVariableCaseInsensitive(string tagName)
        {
            if (_global?.Variables?.Items == null)
                return null;

            foreach (var item in _global.Variables.Items)
            {
                if (string.Equals(item.Name, tagName, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        private TCommandTag FindCommandCaseInsensitive(string tagName)
        {
            if (_global?.Commands?.Items == null)
                return null;

            foreach (var item in _global.Commands.Items)
            {
                if (string.Equals(item.Name, tagName, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        private void UpdateTextBox(TextBox textBox, string tagName, string sectionName)
        {
            // Не обновляем если секция редактируется
            if (_editingSections.Contains(sectionName)) return;

            // Не обновляем если поле в фокусе
            if (textBox.IsFocused) return;

            var tag = FindVariableByNameOrAlias(tagName);
            if (tag != null)
            {
                var normalized = NormalizeNumericValue(tag.ValueString);
                if (textBox.Text != normalized)
                    textBox.Text = normalized;
            }

            return null;
        }

        private TCommandTag ResolveCommandByNameOrAlias(string tagName)
        {
            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var command = _global?.Commands?.GetByName(candidate);
                if (command != null)
                    return command;
            }

            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var command = FindCommandCaseInsensitive(candidate);
                if (command != null)
                    return command;
            }

            return null;
        }
        private TVariableTag FindVariableByNameOrAlias(string tagName)
        {
            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var tag = _global?.Variables?.GetByName(candidate);
                if (tag != null)
                    return tag;
            }

            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var tag = FindVariableCaseInsensitive(candidate);
                if (tag != null)
                    return tag;
            }

            return null;
        }

        private TCommandTag FindCommandByNameOrAlias(string tagName)
        {
            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var command = _global?.Commands?.GetByName(candidate);
                if (command != null)
                    return command;
            }

            foreach (var candidate in GetAliasCandidates(tagName))
            {
                var command = FindCommandCaseInsensitive(candidate);
                if (command != null)
                    return command;
            }

            return null;
        }

        private TVariableTag FindVariableByNameOrAlias(string tagName)
        {
            var tag = _global?.Variables?.GetByName(tagName);
            if (tag != null)
                return tag;

            if (TagAliases.TryGetValue(tagName, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    tag = _global?.Variables?.GetByName(alias);
                    if (tag != null)
                        return tag;
                }
            }

            return null;
        }

        private TCommandTag FindCommandByNameOrAlias(string tagName)
        {
            var command = _global?.Commands?.GetByName(tagName);
            if (command != null)
                return command;

            if (TagAliases.TryGetValue(tagName, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    command = _global?.Commands?.GetByName(alias);
                    if (command != null)
                        return command;
                }
            }

            return null;
        }

        private TVariableTag FindVariableByNameOrAlias(string tagName)
        {
            var tag = _global?.Variables?.GetByName(tagName);
            if (tag != null)
                return tag;

            if (TagAliases.TryGetValue(tagName, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    tag = _global?.Variables?.GetByName(alias);
                    if (tag != null)
                        return tag;
                }
            }

            return null;
        }

        private TCommandTag FindCommandByNameOrAlias(string tagName)
        {
            var command = _global?.Commands?.GetByName(tagName);
            if (command != null)
                return command;

            if (TagAliases.TryGetValue(tagName, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    command = _global?.Commands?.GetByName(alias);
                    if (command != null)
                        return command;
                }
            }

            return null;
        }

        private void UpdateTuneButtonVisibility(Button tuneButton, string tagName)
        {
            var tuneTag = FindVariableByNameOrAlias(tagName);
            if (tuneTag != null)
            {
                tuneButton.Visibility = tuneTag.ValueReal > 0
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        #region Редактирование / Сохранение секций

        /// <summary>
        /// Единый обработчик для всех кнопок "РЕДАКТИРОВАТЬ" / "СОХРАНИТЬ"
        /// </summary>
        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || _global == null) return;

            // Проверяем доступ
            if (!_global.Access)
            {
                MessageBox.Show("Нет доступа! Введите пароль.", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Определяем секцию по кнопке
            if (!_buttonToSection.TryGetValue(button, out string sectionName))
            {
                Debug.WriteLine($"Секция для кнопки {button.Name} не найдена");
                return;
            }

            if (_editingSections.Contains(sectionName))
            {
                // Сейчас в режиме редактирования → СОХРАНЯЕМ
                SaveSection(sectionName, button);
            }
            else
            {
                // Сейчас в режиме чтения → ВХОДИМ В РЕДАКТИРОВАНИЕ
                EnterEditMode(sectionName, button);
            }
        }

        /// <summary>
        /// Вход в режим редактирования для секции
        /// </summary>
        private void EnterEditMode(string sectionName, Button button)
        {
            _editingSections.Add(sectionName);

            if (_sectionTextBoxes.TryGetValue(sectionName, out var textBoxes))
            {
                foreach (var tb in textBoxes)
                {
                    tb.IsReadOnly = false;
                    tb.Background = EditableBackground;
                    tb.BorderBrush = EditableBorder;
                    tb.BorderThickness = new Thickness(2);
                }
            }

            // Меняем кнопку на "СОХРАНИТЬ"
            button.Content = "СОХРАНИТЬ";
            button.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 90, 158));

            Debug.WriteLine($"Секция {sectionName}: вход в режим редактирования");
        }

        /// <summary>
        /// Сохранение секции и отправка команд в контроллер
        /// </summary>
        private void SaveSection(string sectionName, Button button)
        {
            if (!_sectionTextBoxes.TryGetValue(sectionName, out var textBoxes))
                return;

            int savedCount = 0;
            int errorCount = 0;

            foreach (var tb in textBoxes)
            {
                // Получаем имя тега из свойства Tag
                string tagName = tb.Tag?.ToString();
                if (string.IsNullOrEmpty(tagName)) continue;

                // Проверяем, изменилось ли значение
                var variable = FindVariableByNameOrAlias(tagName);
                if (variable == null)
                {
                    Debug.WriteLine($"Переменная не найдена: {tagName}");
                    continue;
                }

                // Парсим новое значение
                string textValue = tb.Text.Trim();
                if (!double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double newValue))
                {
                    // Пробуем с запятой
                    if (!double.TryParse(textValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out newValue))
                    {
                        Debug.WriteLine($"Не удалось распарсить значение '{textValue}' для {tagName}");
                        tb.Background = Brushes.LightCoral;
                        errorCount++;
                        continue;
                    }
                }

                // Проверяем, изменилось ли значение
                if (Math.Abs(variable.ValueReal - newValue) < 0.0001)
                {
                    Debug.WriteLine($"{tagName}: значение не изменилось ({newValue})");
                    continue;
                }

                // Отправляем команду в контроллер
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
                    Debug.WriteLine($"Команда не найдена: {tagName}. Проверьте имя команды в ModbusInitializer/конфиге.");
                    errorCount++;
                }
            }

            // Сохраняем чекбоксы (если есть в секции)
            if (sectionName == "GRO")
            {
                SaveCheckBox(GroRecept_UseAcidCheck, "GRO_Recept_KislotaEnable", ref savedCount, ref errorCount);
            }

            // Выходим из режима редактирования
            ExitEditMode(sectionName, button);

            // Логируем
            if (savedCount > 0)
            {
                _global.Log.Add("Пользователь", $"Изменены параметры секции {sectionName} ({savedCount} значений)", 1);
            }

            // Показываем результат
            if (errorCount > 0)
            {
                MessageBox.Show($"Сохранено: {savedCount}, Ошибок: {errorCount}",
                    "Результат", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (savedCount > 0)
            {
                Debug.WriteLine($"Секция {sectionName}: сохранено {savedCount} значений");
            }
        }

        private void SaveCheckBox(CheckBox checkBox, string tagName, ref int savedCount, ref int errorCount)
        {
            var variable = FindVariableByNameOrAlias(tagName);
            if (variable == null) return;

            bool newValue = checkBox.IsChecked == true;
            bool currentValue = variable.ValueReal > 0;

            if (newValue != currentValue)
            {
                var command = FindCommandByNameOrAlias(tagName);
                if (command != null)
                {
                    command.WriteValue = newValue ? "true" : "false";
                    command.NeedToWrite = true;
                    command.SendToController();
                    savedCount++;
                    Debug.WriteLine($"{tagName}: отправлено {newValue}");
                }
                else
                {
                    errorCount++;
                }
            }
        }

        /// <summary>
        /// Выход из режима редактирования
        /// </summary>
        private void ExitEditMode(string sectionName, Button button)
        {
            _editingSections.Remove(sectionName);

            if (_sectionTextBoxes.TryGetValue(sectionName, out var textBoxes))
            {
                foreach (var tb in textBoxes)
                {
                    tb.IsReadOnly = true;
                    tb.Background = ReadOnlyBackground;
                    tb.BorderBrush = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                    tb.BorderThickness = new Thickness(1);
                }
            }

            // Возвращаем кнопку в исходное состояние
            button.Content = "РЕДАКТИРОВАТЬ";
            button.Background = new SolidColorBrush(Color.FromRgb(30, 89, 69));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 127));

            Debug.WriteLine($"Секция {sectionName}: выход из режима редактирования");
        }

        #endregion

        #region Автонастройка PID

        private void SendCommand(string commandName, string value, string logMessage)
        {
            if (_global == null || !_global.Access)
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
            }
            else
            {
                Debug.WriteLine($"Команда не найдена: {commandName}");
            }
        }

        private void P601TuneButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("P601_PID_Tune", "true", "Автонастройка PID-регулятора P-601");
            P601TuneButton.Visibility = Visibility.Collapsed;
        }

        private void P602TuneButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("P602_PID_Tune", "true", "Автонастройка PID-регулятора P-602");
            P602TuneButton.Visibility = Visibility.Collapsed;
        }

        private void P651TuneButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("P651_PID_Tune", "true", "Автонастройка PID-регулятора P-651");
            P651TuneButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Компрессор

        private void CompressorStartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("Compressor_Start", "true", "Включение компрессора");
        }

        private void CompressorStopButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("Compressor_Start", "false", "Отключение компрессора");
        }

        #endregion

        #region Чекбокс кислоты

        private void GroRecept_UseAcidCheck_Changed(object sender, RoutedEventArgs e)
        {
            GroRecept_Acid.Visibility = (GroRecept_UseAcidCheck.IsChecked == true)
                ? Visibility.Visible : Visibility.Hidden;
        }

        #endregion

        // Пустой обработчик из XAML (можно удалить из XAML)
        private void GroEditButton_Click_1(object sender, RoutedEventArgs e) { }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_DI.xaml
    /// </summary>
    public partial class Element_DI : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен

        private string _name;

        // Свойство Name для отображения на мнемосхеме
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                // Обновляем TextBlock при изменении свойства
                if (TagName != null)
                    TagName.Text = value;
            }
        }

        public Element_DI()
        {
            InitializeComponent();
            Loaded += Element_DI_Loaded;
        }

        private void Element_DI_Loaded(object sender, RoutedEventArgs e)
        {
            // При загрузке элемента устанавливаем значения из свойств
            if (TagName != null && !string.IsNullOrEmpty(Name))
                TagName.Text = Name;
            else if (TagName != null && !string.IsNullOrEmpty(VarName))
                TagName.Text = VarName;
        }

        public void UpdateElement()
        {
            try
            {
                // Проверяем, что Global инициализирован
                if (Global == null)
                {
                    // Все равно обновляем имя, если оно есть
                    UpdateName();
                    return;
                }

                // Обновляем имя
                UpdateName();

                // Ручной режим
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ?
                        Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                // Состояние датчика
                Tag = Global.Variables?.GetByName(VarName + "_Value");
                if (Tag != null)
                {
                    DIIcon.Source = Tag.ValueReal > 0 ?
                        FindResource("DIonIcon") as ImageSource :
                        FindResource("DIoffIcon") as ImageSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_DI {VarName}: {ex.Message}");
            }
        }

        private void UpdateName()
        {
            if (TagName != null)
            {
                // Используем Name, если он установлен, иначе VarName
                string displayName = !string.IsNullOrEmpty(Name) ? Name : VarName;
                TagName.Text = displayName;
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementDI Dialog = new DialogElementDI();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_DI: {ex.Message}");
            }
        }
    }
}
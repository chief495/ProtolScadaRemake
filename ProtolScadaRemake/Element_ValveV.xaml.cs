using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    /// <summary>
    /// Логика взаимодействия для Element_ValveH.xaml
    /// </summary>
    public partial class Element_ValveV : UserControl
    {
        public string Description = ""; // Описание элемента
        public TGlobal Global;
        public string VarName = ""; // Основание для имен
        public string Name { get; set; } // Имя для отображения на мнемосхеме

        public Element_ValveV()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            // Проверяем, что Global инициализирован
            if (Global == null)
            {
                // Все равно обновляем имя тега, если оно есть
                if (TAGNAME != null && !string.IsNullOrEmpty(VarName))
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(Name) ? Name : VarName;
                }
                return;
            }

            try
            {
                // Обновляем имя тега
                if (TAGNAME != null)
                {
                    TAGNAME.Text = !string.IsNullOrEmpty(Name) ? Name : VarName;
                }

                // Ручной режим
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                // Положение по умолчанию
                ValveIcon.Source = FindResource("ValveVCloseIcon") as ImageSource;

                // Клапан в закрытом положении
                Tag = Global.Variables?.GetByName(VarName + "_IsClose");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    ValveIcon.Source = FindResource("ValveHCloseIcon") as ImageSource;
                }

                // Клапан в открытом положении
                Tag = Global.Variables?.GetByName(VarName + "_IsOpen");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    ValveIcon.Source = FindResource("ValveHCOpenIcon") as ImageSource;
                }

                // Клапан в движении
                Tag = Global.Variables?.GetByName(VarName + "_IsMoving");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    ValveIcon.Source = FindResource("ValveHMovingIcon") as ImageSource;
                }

                // Заклинивание клапана
                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    ValveIcon.Source = FindResource("ValveHCFaultIcon") as ImageSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_ValveV {VarName}: {ex.Message}");
            }
        }

        private void ValueLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global == null) return;

            try
            {
                DialogElementValve Dialog = new DialogElementValve();
                Dialog.Title = Description;
                Dialog.Global = Global;
                Dialog.VarName = VarName;
                Dialog.Initialize();
                Dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия диалога Element_ValveV: {ex.Message}");
            }
        }
    }
}
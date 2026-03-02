using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_DI : UserControl
    {
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";
        public string TagName { get; set; } = "";

        public Element_DI()
        {
            InitializeComponent();
        }

        public void UpdateElement()
        {
            try
            {
                if (TagNameTextBlock != null)
                {
                    TagNameTextBlock.Text = !string.IsNullOrEmpty(TagName) ? TagName : VarName;
                }

                if (Global == null) return;

                // Устанавливаем иконку по умолчанию (выключено)
                DIIcon.Source = FindResource("DIoffIcon") as ImageSource;

                // Проверяем ручной режим
                TVariableTag Tag = Global.Variables?.GetByName(VarName + "_Manual");
                if (Tag != null)
                {
                    HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                    HandImage.Visibility = Visibility.Hidden;
                }

                // Проверяем значение датчика (включен/выключен)
                Tag = Global.Variables?.GetByName(VarName + "_Value");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    DIIcon.Source = FindResource("DIonIcon") as ImageSource;
                }

                // Проверяем ошибку датчика (Fault - имеет наивысший приоритет)
                Tag = Global.Variables?.GetByName(VarName + "_Fault");
                if (Tag != null && Tag.ValueReal > 0)
                {
                    DIIcon.Source = FindResource("DIfaultIcon") as ImageSource;
                }

                // Проверяем несоответствие (Changed - если команда не совпадает с состоянием)
                // Для дискретных датчиков можно проверить, например, Feedback
                Tag = Global.Variables?.GetByName(VarName + "_FeedbackOk");
                if (Tag != null && Tag.ValueReal < 1)
                {
                    DIIcon.Source = FindResource("DIchangedIcon") as ImageSource;
                }

                // Альтернативно, можно проверить статус
                Tag = Global.Variables?.GetByName(VarName + "_Status");
                if (Tag != null && Tag.ValueReal != 0) // Если статус не 0 (не норма)
                {
                    // В зависимости от статуса можно выбрать иконку
                    // Например, статус 1 = несоответствие, 2 = ошибка и т.д.
                    if (Tag.ValueReal == 1)
                        DIIcon.Source = FindResource("DIchangedIcon") as ImageSource;
                    else if (Tag.ValueReal >= 2)
                        DIIcon.Source = FindResource("DIfaultIcon") as ImageSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления Element_DI {VarName}: {ex.Message}");
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
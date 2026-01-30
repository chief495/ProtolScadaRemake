using System.Windows;
using System.Windows.Controls;
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
        public Element_DI()
        {
            InitializeComponent();
        }
        public void UpdateElement()
        {
            UpdateTextBlock();
            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null) if (Tag.ValueReal <= 0) HandImage.Visibility = Visibility.Hidden;
            if (Tag != null) if (Tag.ValueReal > 0) HandImage.Visibility = Visibility.Visible;
            // Состояние
            Tag = Global.Variables.GetByName(VarName + "_Value");
            if (Tag != null) if (Tag.ValueReal <= 0) DIIcon.Source = FindResource("DIoffIcon") as ImageSource;
            if (Tag != null) if (Tag.ValueReal > 0) DIIcon.Source = FindResource("DIonIcon") as ImageSource;
        }

        private void UpdateTextBlock()
        {
            // Ищем TextBlock в визуальном дереве
            var textBlock = FindTextBlock(this);
            if (textBlock != null && !string.IsNullOrEmpty(VarName))
            {
                textBlock.Text = VarName;
            }
        }

        private TextBlock FindTextBlock(DependencyObject parent)
        {
            // Рекурсивный поиск TextBlock в визуальном дереве
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock textBlock)
                    return textBlock;

                var result = FindTextBlock(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void ValueLabel_Click(object sender, EventArgs e)
        {
            DialogElementDI Dialog = new DialogElementDI();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}

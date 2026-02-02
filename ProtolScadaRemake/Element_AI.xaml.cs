using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class Element_AI : UserControl
    {
        public Brush WarningColor = Brushes.Yellow;
        public Brush FaultColor = Brushes.Red;
        public string Description = "";
        public TGlobal Global;
        public string VarName = "";

        private string _eu;
        private string _designation;
        private string _name;

        // Добавьте свойство Name
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (TagName != null)
                    TagName.Text = value;
            }
        }

        public string EU
        {
            get => _eu;
            set
            {
                _eu = value;
                if (TextBlockEU != null)
                    TextBlockEU.Text = value;
            }
        }

        public string Designation
        {
            get => _designation;
            set
            {
                _designation = value;
                if (TextBlockDesignation != null)
                    TextBlockDesignation.Text = value;
            }
        }

        public Element_AI()
        {
            InitializeComponent();
            Loaded += Element_AI_Loaded;
        }

        private void Element_AI_Loaded(object sender, RoutedEventArgs e)
        {
            // При загрузке элемента устанавливаем значения из свойств
            if (TagName != null && !string.IsNullOrEmpty(Name))
                TagName.Text = Name;

            if (TextBlockDesignation != null && !string.IsNullOrEmpty(Designation))
                TextBlockDesignation.Text = Designation;

            if (TextBlockEU != null && !string.IsNullOrEmpty(EU))
                TextBlockEU.Text = EU;
        }

        public void UpdateElement()
        {
            if (TagName != null && !string.IsNullOrEmpty(Name))
            {
                TagName.Text = Name;
            }

            // Ручной режим
            TVariableTag Tag = Global.Variables.GetByName(VarName + "_Manual");
            if (Tag != null)
                HandImage.Visibility = Tag.ValueReal > 0 ? Visibility.Visible : Visibility.Hidden;

            // Текущее значение
            Tag = Global.Variables.GetByName(VarName + "_Value");
            if (Tag != null && ValueLabel != null)
                ValueLabel.Text = Tag.ValueString;

            // Аварии и предупреждения
            if (ValueRect != null)
            {
                ValueRect.Fill = Brushes.Transparent;

                Tag = Global.Variables.GetByName(VarName + "_Warning_Low");
                if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = WarningColor;

                Tag = Global.Variables.GetByName(VarName + "_Warning_Hi");
                if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = WarningColor;

                Tag = Global.Variables.GetByName(VarName + "_Fault_Low");
                if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = FaultColor;

                Tag = Global.Variables.GetByName(VarName + "_Fault_Hi");
                if (Tag != null && Tag.ValueReal > 0) ValueRect.Fill = FaultColor;
            }
        }

        private void ValueLabel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogElementAI Dialog = new DialogElementAI();
            Dialog.Title = Description;
            Dialog.Global = Global;
            Dialog.VarName = VarName;
            Dialog.EU = EU;
            Dialog.Initialize();
            Dialog.ShowDialog();
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtolScadaRemake
{
    public partial class LiquidGauge : UserControl
    {
        public LiquidGauge()
        {
            InitializeComponent();
        }

        // Обновляем Clip при загрузке и изменении размера
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClip();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClip();
            UpdateFill();
        }

        private void UpdateClip()
        {
            this.Clip = new RectangleGeometry
            {
                RadiusX = 10,
                RadiusY = 10,
                Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight)
            };
        }

        // Dependency Property для уровня заполнения (0-100)
        public static readonly DependencyProperty FillLevelProperty =
            DependencyProperty.Register(
                nameof(FillLevel),
                typeof(double),
                typeof(LiquidGauge),
                new PropertyMetadata(0.0, OnFillLevelChanged));

        public double FillLevel
        {
            get => (double)GetValue(FillLevelProperty);
            set => SetValue(FillLevelProperty, value);
        }

        private static void OnFillLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LiquidGauge gauge)
            {
                gauge.UpdateFill();
            }
        }

        private void UpdateFill()
        {
            if (this.ActualHeight <= 0) return;

            double level = Math.Max(0, Math.Min(100, FillLevel));
            FillRect.Height = (level / 100.0) * this.ActualHeight;
        }

        // Свойство для цвета заполнения
        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register(
                nameof(FillColor),
                typeof(Brush),
                typeof(LiquidGauge),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 50, 205, 50)), OnFillColorChanged));

        public Brush FillColor
        {
            get => (Brush)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        private static void OnFillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LiquidGauge gauge)
            {
                gauge.FillRect.Fill = (Brush)e.NewValue;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace QAMP
{
    public class CustomSlider : Slider
    {
        private bool bool_Clicked = false;

        public bool Clicked
        {
            get { return bool_Clicked; }
            set { bool_Clicked = value; }
        }

        private double double_Position = 0;

        public double Position
        {
            get { return double_Position; }
        }

        private Track Track_SlideBar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Track_SlideBar = GetTemplateChild("PART_Track") as Track;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs E)
        {
            if (IsMoveToPointEnabled && Track_SlideBar != null && Track_SlideBar.Thumb != null && !Track_SlideBar.Thumb.IsMouseOver)
            {
                Point XY = E.MouseDevice.GetPosition(Track_SlideBar);

                double_Position = Track_SlideBar.ValueFromPoint(XY);

                if (!double.IsInfinity(double_Position))
                    UpdateValue(double_Position);

                bool_Clicked = true;

                E.Handled = true;
            }
        }

        public void UpdateValue(double Position)
        {
            Double SnappedValue = SnapToTick(Position);

            if (SnappedValue != Value)
            {
                DoubleAnimation Animation = new DoubleAnimation();

                Animation.To = Math.Max(this.Minimum, Math.Min(this.Maximum, SnappedValue));
                Animation.Duration = TimeSpan.FromSeconds(0.25);

                Storyboard.SetTargetProperty(Animation, new PropertyPath("Value"));
                Storyboard.SetTarget(Animation, this);

                Storyboard SB = new Storyboard();
                SB.Children.Add(Animation);
                SB.Begin();
            }
        }

        private double SnapToTick(double Position)
        {
            if (IsSnapToTickEnabled)
            {
                double Previous = Minimum;
                double Next = Maximum;

                if (TickFrequency > 0.0)
                {
                    Previous = Minimum + (Math.Round(((Position - Minimum) / TickFrequency)) * TickFrequency);
                    Next = Math.Min(Maximum, Previous + TickFrequency);
                }

                Position = (Position > ((Previous + Next) * 0.25)) ? Next : Previous;
            }

            return Position;
        }
    }
}

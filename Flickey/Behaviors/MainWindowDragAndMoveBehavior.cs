using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Flickey.Behaviors
{
    public sealed class MainWindowDragAndMoveBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeftButtonDown += this.OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.AssociatedObject.DragMove();
            }
        }
    }
}
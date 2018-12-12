using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Flickey.Behaviors
{
    using Flickey.Controls;

    public sealed class KeyboardDragCancelBehavior : Behavior<Keyboard>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeftButtonDown += this.OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseLeftButtonDown -= this.OnMouseLeftButtonDown;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
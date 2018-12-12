using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Flickey.Behaviors
{
    public sealed class MinimizeButtonBehavior : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Click += this.OnClicked;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Click -= this.OnClicked;
            base.OnDetaching();
        }

        private void OnClicked(object sender, RoutedEventArgs args)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}
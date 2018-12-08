using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Flickey.Behaviors
{
    public sealed class MainWindowDisposeBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Closed += (_, __) =>
            {
                (this.AssociatedObject.DataContext as IDisposable)?.Dispose();
                (this.AssociatedObject.FindName("keyboard") as IDisposable)?.Dispose();
            };
        }
    }
}
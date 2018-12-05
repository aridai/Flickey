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

            AssociatedObject.Closed += (_, __) =>
            {
                (AssociatedObject.DataContext as IDisposable)?.Dispose();
                (AssociatedObject.FindName("keyboard") as IDisposable)?.Dispose();
            };
        }
    }
}
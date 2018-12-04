using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Flickey.Behaviors
{
    public sealed class MainWindowViewModelDisposeBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Closed += (_, __) => (AssociatedObject.DataContext as IDisposable)?.Dispose();
        }
    }
}

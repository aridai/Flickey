using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace Flickey.Behaviors
{
    using Flickey.Models.PInvokeComponents;

    public sealed class MainWindowSetupBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += this.OnLoaded;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.OnLoaded;
            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var windowHandle = new WindowInteropHelper(AssociatedObject).Handle;
            var style = PInvokeFunctions.GetWindowLong(windowHandle, WindowLongOffset.EXSTYLE);

            style |= 134217728u;
            PInvokeFunctions.SetWindowLong(windowHandle, WindowLongOffset.EXSTYLE, style);

            //  変更を適用させる。
            var flags = WindowPosFlags.NOMOVE | WindowPosFlags.NOSIZE | WindowPosFlags.NOZORDER | WindowPosFlags.FRAMECHANGED;
            PInvokeFunctions.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, flags);
        }
    }
}
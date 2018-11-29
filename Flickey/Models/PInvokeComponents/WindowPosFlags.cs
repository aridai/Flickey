using System;

namespace Flickey.Models.PInvokeComponents
{
    [Flags]
    public enum WindowPosFlags
    {
        NOSIZE = 1,
        NOMOVE = 2,
        NOZORDER = 4,
        NOREDRAW = 8,
        NOACTIVATE = 16,
        FRAMECHANGED = 32,
        SHOWWINDOW = 64,
        HIDEWINDOW = 128,
        NOCOPYBITS = 256,
        NOOWNERZORDER = 512,
        NOSENDCHANGING = 1024
    }
}
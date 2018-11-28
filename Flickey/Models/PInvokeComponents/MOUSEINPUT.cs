using System;

namespace Flickey.Models.PInvokeComponents
{
    public struct MOUSEINPUT
    {
        public int dx;

        public int dy;

        public uint mouseData;

        public uint dwFlags;

        public uint time;

        public IntPtr dwExtraInfo;
    }
}
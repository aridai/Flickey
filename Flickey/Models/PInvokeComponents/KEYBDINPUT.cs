using System;

namespace Flickey.Models.PInvokeComponents
{
    public struct KEYBDINPUT
    {
        public ushort wVk;

        public ushort wScan;

        public KeyboardInputFlag dwFlags;

        public uint time;

        public IntPtr dwExtraInfo;
    }
}
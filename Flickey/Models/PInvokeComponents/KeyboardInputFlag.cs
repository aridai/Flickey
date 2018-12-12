using System;

namespace Flickey.Models.PInvokeComponents
{
    [Flags]
    public enum KeyboardInputFlag : uint
    {
        KEYEVENTF_NONE = 0x0000,
        KEYEVENTF_EXTENDEDKEY = 0x0001,
        KEYEVENTF_KEYUP = 0x0002,
        KEYEVENTF_UNICODE = 0x0004,
        KEYEVENTF_SCANCODE = 0x0008
    }
}
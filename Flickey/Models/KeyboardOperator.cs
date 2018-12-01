using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Flickey.Models
{
    using Flickey.Models.PInvokeComponents;

    /// <summary>
    /// キーボードを制御する処理を提供します。
    /// </summary>
    public static class KeyboardOperator
    {
        /// <summary>
        /// INPUT構造体のサイズ。
        /// </summary>
        public static readonly int SizeOfInputStructure;

        static KeyboardOperator()
        {
            SizeOfInputStructure = Marshal.SizeOf<INPUT>();
        }

        /// <summary>
        /// 現在の入力モードを取得します。
        /// </summary>
        /// <returns>現在の入力モード。</returns>
        public static InputMode GetInputMode()
        {
            var guiThreadInfo = new GUITHREADINFO();
            guiThreadInfo.cbSize = Marshal.SizeOf<GUITHREADINFO>();
            PInvokeFunctions.GetGUIThreadInfo(0, ref guiThreadInfo);
            var imeWnd = PInvokeFunctions.ImmGetDefaultIMEWnd(guiThreadInfo.hwndFocus);

            var value1 = PInvokeFunctions.SendMessage(imeWnd, 0x0283, 0x0001, 0);
            var value2 = PInvokeFunctions.SendMessage(imeWnd, 0x0283, 0x0005, 0);

            if (value1 == 0) return InputMode.Disabled;
            if (value2 == 1) return InputMode.JapaneseInput;
            if (value2 == 0) return InputMode.DirectInput;
            return InputMode.Unknown;
        }

        /// <summary>
        /// 入力モードを設定します。
        /// </summary>
        /// <param name="mode">設定する入力モード。</param>
        public static void SetInputMode(InputMode mode)
        {
            var current = GetInputMode();
            if (current == mode) return;

            //  現時点では、単に「半角/全角|漢字」キーを押すだけにする。
            //  入力モードが「半角英数」(_A) になる場合については、現時点では処理しないことにする。
            InputKey(VirtualKeyCode.Kanji);
        }

        /// <summary>
        /// キー入力を行います。
        /// キーストロークをシミュレートすることで入力します。
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード。</param>
        public static void InputKey(VirtualKeyCode virtualKeyCode)
        {
            var inputs = ToInputStructure(virtualKeyCode);
            PInvokeFunctions.SendInput(2, inputs, SizeOfInputStructure);
        }

        /// <summary>
        /// キー入力を行います。
        /// キーストロークをシミュレートすることで入力します。
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード。</param>
        public static void InputKey(IReadOnlyList<VirtualKeyCode> virtualKeyCodes)
        {
            var length = virtualKeyCodes.Count;
            var inputs = new INPUT[length * 2];

            for (int i = 0; i < length; i++)
            {
                var structures = ToInputStructure(virtualKeyCodes[i]);
                inputs[i] = structures[0];
                inputs[i + length] = structures[1];
            }

            PInvokeFunctions.SendInput(length * 2, inputs, SizeOfInputStructure);
        }

        /// <summary>
        /// 文字列を直接入力します。
        /// 現在のIMEの入力モードに依存せず、また、日本語も直接入力で入力します。
        /// </summary>
        /// <param name="str">入力する文字列。</param>
        public static void InputDirectly(string str)
        {
            var length = str.Length;
            var inputs = new INPUT[length * 2];

            for (int i = 0; i < length; i++)
            {
                var j = i * 2;
                var k = j + 1;

                inputs[j] = new INPUT();
                inputs[j].type = InputType.INPUT_KEYBOARD;
                inputs[j].ki.wScan = str[i];
                inputs[j].ki.dwFlags = KeyboardInputFlag.KEYEVENTF_UNICODE;

                inputs[k] = inputs[j];
                inputs[k].ki.dwFlags |= KeyboardInputFlag.KEYEVENTF_KEYUP;
            }

            PInvokeFunctions.SendInput(length * 2, inputs, SizeOfInputStructure);
        }

        //  INPUT構造体の配列を作る。
        private static INPUT[] ToInputStructure(VirtualKeyCode virtualKeyCode)
        {
            var inputs = new INPUT[2];

            inputs[0] = new INPUT();
            inputs[0].type = InputType.INPUT_KEYBOARD;
            inputs[0].ki.wVk = (ushort)virtualKeyCode;
            inputs[0].ki.wScan = (ushort)PInvokeFunctions.MapVirtualKey((uint)virtualKeyCode, 0);
            //inputs[0].ki.dwFlags = KeyboardInputFlag.KEYEVENTF_SCANCODE;

            inputs[1] = inputs[0];
            inputs[1].ki.dwFlags |= KeyboardInputFlag.KEYEVENTF_KEYUP;

            return inputs;
        }
    }
}
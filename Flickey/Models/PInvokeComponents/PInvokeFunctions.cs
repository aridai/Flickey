using System;
using System.Runtime.InteropServices;

namespace Flickey.Models.PInvokeComponents
{
    /// <summary>
    /// PInvokeによる関数を提供します。
    /// </summary>
    public static class PInvokeFunctions
    {
        /// <summary>
        /// システムのメッセージを送信します。
        /// </summary>
        /// <param name="hWnd">対象のウィンドウハンドル。</param>
        /// <param name="Msg">送信するメッセージ。</param>
        /// <param name="wParam">メッセージの1つ目のパラメータ。</param>
        /// <param name="lParam">メッセージの2つ目のパラメータ。</param>
        /// <returns>メッセージの結果。</returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        /// <summary>
        /// 入力操作を送信します。
        /// </summary>
        /// <param name="nInputs">入力イベントの数。</param>
        /// <param name="pInputs">入力イベントの配列。</param>
        /// <param name="cbSize">構造体のサイズ。</param>
        /// <returns>入力ストリームに入力できたイベントの数。</returns>
        [DllImport("user32.dll")]
        public static extern int SendInput(int nInputs, INPUT[] pInputs, int cbSize);

        /// <summary>
        /// GUIスレッドの情報を取得します。
        /// </summary>
        /// <param name="dwthreadid">取得対象のスレッド認識子を指定します。NULLを指定するとフォアグラウンドスレッドが指定されます。</param>
        /// <param name="lpguithreadinfo">スレッドの情報を格納する構造体の参照を指定します。</param>
        /// <returns>関数が成功すると0以外の値が返り、失敗すると0が返ります。</returns>
        [DllImport("user32.dll")]
        public static extern uint GetGUIThreadInfo(uint dwthreadid, ref GUITHREADINFO lpguithreadinfo);

        /// <summary>
        /// IMEクラスの既定のウィンドウハンドルを取得します。
        /// </summary>
        /// <param name="hWnd">アプリケーションのウィンドウハンドルを指定します。</param>
        /// <returns>関数が成功すると既定のウィンドウハンドルが返ります。失敗するとNULLが返ります。</returns>
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);
    }
}
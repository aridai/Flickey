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

        /// <summary>
        /// 指定されたウィンドウに関する情報を取得します。
        /// </summary>
        /// <param name="hWnd">対象のウィンドウハンドルを指定します。</param>
        /// <param name="index">取得する値のオフセットを指定します。</param>
        /// <returns>関数が成功すると要求したデータが返ります。失敗すると0が返ります。</returns>
        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, WindowLongOffset index);

        /// <summary>
        /// 指定されたウィンドウの属性を変更します。
        /// </summary>
        /// <param name="hWnd">対象のウィンドウハンドルを指定します。</param>
        /// <param name="index">設定する値のオフセットを指定します。</param>
        /// <param name="newValue">設定する値。</param>
        /// <returns>関数が成功すると設定前の値が返ります。失敗すると0が返ります。</returns>
        [DllImport("user32.dll")]
        public static extern uint SetWindowLong(IntPtr hWnd, WindowLongOffset index, uint newValue);

        /// <summary>
        /// ウィンドウの位置などを設定します。
        /// </summary>
        /// <param name="hWnd">対象のウィンドウハンドルを指定します。</param>
        /// <param name="hWndInsertAfter">Zオーダを決めるためのウィンドウハンドルを指定します。</param>
        /// <param name="x">ウィンドウの左上端の新しいX座標をクライアント座標で指定します。</param>
        /// <param name="y">ウィンドウの左上端の新しいY座標をクライアント座標で指定します。</param>
        /// <param name="width">ウィンドウの新しい幅をピクセル単位で指定します。</param>
        /// <param name="height">ウィンドウの新しい高さをピクセル単位で指定します。</param>
        /// <param name="flags">ウィンドウのサイズと位置の変更に関するフラグを指定します。</param>
        /// <returns>関数が成功すると0以外の値が返ります。失敗すると0が返ります。</returns>
        [DllImport("user32.dll")]
        public static extern uint SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, WindowPosFlags flags);

        /// <summary>
        /// 仮想キーコードとスキャンコードを相互変換します。
        /// </summary>
        /// <param name="uCode">仮想キーコードまたはスキャンコード。</param>
        /// <param name="uMapType">0を指定すると仮想キーコードからスキャンコードへ、1を指定するとその逆の変換を行います。</param>
        /// <returns>関数が成功すると仮想キーコードまたはスキャンコードを返します。失敗すると0を返します。</returns>
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);
    }
}
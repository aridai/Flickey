using System;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーのエフェクトを列挙します。
    /// </summary>
    public enum KeyEffect
    {
        /// <summary>
        /// エフェクトを適用しません。
        /// </summary>
        NoEffect,

        /// <summary>
        /// タップ時の効果です。
        /// </summary>
        Tapped,

        /// <summary>
        /// ホールド操作でフォーカスが当てられたときの効果です。
        /// </summary>
        Focused,

        /// <summary>
        /// ぼかし効果です。
        /// </summary>
        Blurred
    }
}
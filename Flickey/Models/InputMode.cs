using System;

namespace Flickey.Models
{
    /// <summary>
    /// IMEの入力モードを列挙します。
    /// </summary>
    public enum InputMode
    {
        /// <summary>
        /// IMEが無効です。
        /// </summary>
        Disabled,

        /// <summary>
        /// 直接入力モード。
        /// </summary>
        DirectInput,

        /// <summary>
        /// 日本語入力モード。
        /// </summary>
        JapaneseInput,

        /// <summary>
        /// 不明な入力モード。
        /// </summary>
        Unknown
    }
}
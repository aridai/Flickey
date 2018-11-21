using System;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーの印字の表示方法を列挙します。
    /// </summary>
    public enum LabelStyle
    {
        /// <summary>
        /// 最初の文字のみを表示します。
        /// </summary>
        OnlyFirstCaracter,

        /// <summary>
        /// すべての文字を1行に並べて表示します。
        /// </summary>
        OneLine,

        /// <summary>
        /// 2文字目以降を2行目に並べて表示します。
        /// </summary>
        TwoLines
    }
}
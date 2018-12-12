using System;
using System.Runtime.Serialization;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーの印字の表示方法を列挙します。
    /// </summary>
    [DataContract]
    public enum LabelDisplayStyle
    {
        /// <summary>
        /// 最初の文字のみを表示します。
        /// </summary>
        [DataMember(Name = nameof(OnlyFirstCharacter))]
        OnlyFirstCharacter,

        /// <summary>
        /// すべての文字を1行に並べて表示します。
        /// </summary>
        [DataMember(Name = nameof(OneLine))]
        OneLine,

        /// <summary>
        /// 2文字目以降を2行目に並べて表示します。
        /// </summary>
        [DataMember(Name = nameof(TwoLines))]
        TwoLines
    }
}
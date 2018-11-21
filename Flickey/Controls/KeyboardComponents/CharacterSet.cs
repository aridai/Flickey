using System;
using System.Collections.Generic;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーが持つ文字の集合を提供します。
    /// </summary>
    public struct CharacterSet
    {
        /// <summary>
        /// 印字の表示方法を指定します。
        /// </summary>
        public LabelStyle LabelStyle { get; }

        /// <summary>
        /// キーが持つ文字のコレクション。
        /// </summary>
        public IReadOnlyList<string> Characters { get; }

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="displayStyle">印字の表示方法。</param>
        /// <param name="characters">キーが持つ文字のコレクション。要素数は5にする必要があります。</param>
        public CharacterSet(LabelStyle displayStyle, IReadOnlyList<string> characters)
        {
            if (characters.Count != 5) throw new ArgumentException("コレクションの要素数は5である必要があります。", nameof(characters));
            this.LabelStyle = displayStyle;
            this.Characters = characters;
        }
    }
}
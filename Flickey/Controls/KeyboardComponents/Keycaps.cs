using System;
using System.Collections.Generic;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーの配字に関する情報を提供します。
    /// </summary>
    public class Keycaps : IKeycaps
    {
        /// <summary>
        /// 印字の表示方法を指定します。
        /// </summary>
        public LabelStyle LabelStyle { get; }

        /// <summary>
        /// キーが持つ文字のコレクション。
        /// </summary>
        public IEnumerable<string> Characters { get; }

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="displayStyle">印字の表示方法。</param>
        /// <param name="characters">キーが持つ文字のコレクション。要素数は1から5までにする必要があります。</param>
        public Keycaps(LabelStyle displayStyle, IEnumerable<string> characters)
        {
            this.LabelStyle = displayStyle;
            this.Characters = characters;
        }
    }
}
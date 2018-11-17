using System;
using System.Collections.Generic;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーの配字に関する情報を提供します。
    /// </summary>
    public interface IKeycaps
    {
        /// <summary>
        /// 印字の表示方法を指定します。
        /// </summary>
        LabelStyle LabelStyle { get; }

        /// <summary>
        /// キーが持つ文字のコレクション。
        /// 要素数は1から5までにする必要があります。
        /// </summary>
        IEnumerable<string> Characters { get; }
    }
}
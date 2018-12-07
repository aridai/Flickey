using System;
using System.Collections.Generic;

namespace Flickey.Models
{
    using Flickey.Models.PInvokeComponents;

    /// <summary>
    /// 入力文字に対応した入力情報を提供します。
    /// </summary>
    public sealed class InputInfo
    {
        /// <summary>
        /// 入力文字。
        /// </summary>
        public string Character { get; }

        /// <summary>
        /// IMEの入力モード。
        /// </summary>
        public InputMode InputMode { get; }

        /// <summary>
        /// <see cref="INPUT"/>構造体のリスト。
        /// </summary>
        public IReadOnlyList<INPUT> Structures { get; }

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="character">入力文字。</param>
        /// <param name="mode">IMEの入力モード。</param>
        /// <param name="structure"><see cref="INPUT"/>構造体のリスト。</param>
        public InputInfo(string character, InputMode mode, IReadOnlyList<INPUT> structures)
        {
            this.Character = character;
            this.InputMode = mode;
            this.Structures = structures;
        }
    }
}
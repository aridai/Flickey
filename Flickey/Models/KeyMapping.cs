using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flickey.Models
{
    /// <summary>
    /// 文字に対するキーのマッピングを提供します。
    /// </summary>
    [DataContract]
    public struct KeyMapping
    {
        /// <summary>
        /// 対象の文字。
        /// </summary>
        [DataMember(Name = "character")]
        public string Character { get; set; }

        /// <summary>
        /// 必要な入力モード。
        /// </summary>
        [DataMember(Name = "mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InputMode InputMode { get; set; }

        /// <summary>
        /// 入力に必要なキーコードのコレクション。
        /// </summary>
        [DataMember(Name = "keys")]
        public IReadOnlyList<VirtualKeyCode> KeyCodes { get; set; }

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="character">対象の文字。</param>
        /// <param name="mode">必要な入力モード。</param>
        /// <param name="keyCodes">入力な必要なキーコードのコレクション。</param>
        public KeyMapping(string character, InputMode mode, IReadOnlyList<VirtualKeyCode> keyCodes)
        {
            this.Character = character;
            this.InputMode = mode;
            this.KeyCodes = keyCodes;
        }
    }
}
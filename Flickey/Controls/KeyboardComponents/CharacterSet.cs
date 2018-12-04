using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flickey.Controls.KeyboardComponents
{
    /// <summary>
    /// キーが持つ文字の集合を提供します。
    /// </summary>
    [DataContract]
    public struct CharacterSet
    {
        /// <summary>
        /// 印字の表示方法を指定します。
        /// </summary>
        [DataMember(Name = "style")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LabelStyle LabelStyle { get; set; }

        /// <summary>
        /// キーが持つ文字のコレクション。
        /// </summary>
        [DataMember(Name = "characters")]
        public IReadOnlyList<string> Characters { get; set; }

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
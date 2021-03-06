﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Flickey.Models
{
    /// <summary>
    /// 文字の変換後の文字を取得する機能を提供します。
    /// </summary>
    public sealed class ConversionQuery
    {
        private readonly IFileReader fileReader;

        private readonly IReadOnlyDictionary<string, string> dictionary;

        /// <summary>
        /// JSONファイル名。
        /// </summary>
        public const string JsonFileName = "Conversion.json";

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="fileReader">ファイルリーダ。</param>
        public ConversionQuery(IFileReader fileReader)
        {
            this.fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            var jsonStr = this.fileReader.ReadAllText(JsonFileName);

            var obj = JsonConvert.DeserializeObject<List<ConversionPair>>(jsonStr);
            this.dictionary = obj.Where(pair => !string.IsNullOrEmpty(pair.Before) && !string.IsNullOrEmpty(pair.After))
                .ToDictionary(pair=> pair.Before, pair => pair.After);
        }

        /// <summary>
        /// 変換後の文字を取得します。
        /// </summary>
        /// <param name="character">変換前の文字。</param>
        /// <returns>変換が存在する場合は変換後の文字を返します。変換が存在しない場合はnullを返します。</returns>
        public string GetConvertedCharacter(string character)
        {
            if (this.dictionary.TryGetValue(character, out var converted)) return converted;
            return null;
        }

        [DataContract]
        internal struct ConversionPair
        {
            [DataMember(Name = "before")]
            public string Before { get; set; }

            [DataMember(Name = "after")]
            public string After { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flickey.Models
{
    /// <summary>
    /// 入力処理を管理します。
    /// </summary>
    public sealed class Inputter
    {
        private readonly IReadOnlyDictionary<string, KeyMapping> dictionary;

        /// <summary>
        /// キーマッピングが保存されたJSONファイル名。
        /// </summary>
        public const string FileName = "KeyMapping.json";

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        public Inputter()
        {
            //  Jsonからマッピングデータを読み込む。
            var json = File.ReadAllText(FileName);

            var settings = new JsonSerializerSettings { Converters = new[] { new StringEnumConverter() } };
            var mappingList = JsonConvert.DeserializeObject<IReadOnlyList<KeyMapping>>(json, settings);

            //  入力文字から検索できる連想配列を作る。
            this.dictionary = mappingList.ToDictionary(data => data.Character, data => data);
        }

        /// <summary>
        /// 入力処理を行います。
        /// </summary>
        /// <param name="character">入力文字。</param>
        public void Input(string character)
        {
            if (character == "a/A") ;
            if (character == "゛゜小") ;

            //  マッピングデータが存在するならば、それを使う。
            var data = this.GetMappingData(character);
            if (data != null)
            {
                var mode = data.Value.InputMode;
                var codes = data.Value.KeyCodes;

                System.Diagnostics.Debug.WriteLine($"{character}, {mode.ToString()}, {codes.Aggregate(string.Empty, (total, next) => total + next.ToString())}");

                //  日本語入力モードならば、キーストロークをシミュレートして入力する。
                if (mode == InputMode.Japanese)
                {
                    KeyboardOperator.SetInputMode(InputMode.Japanese);
                    KeyboardOperator.InputKey(codes);
                }

                //  直接入力モードまたは入力モード未指定のとき。
                else
                {
                    //  直接入力モードが指定された場合のみ切り替える。
                    //  未指定の場合は切り替えない。
                    if (mode == InputMode.Direct)
                        KeyboardOperator.SetInputMode(InputMode.Direct);

                    //  キーコードの指定があればそれを使う。
                    if (codes.Count > 0)
                    {
                        KeyboardOperator.InputKey(codes);
                    }

                    //  指定がなければ直接入力する。
                    else
                    {
                        KeyboardOperator.InputDirectly(character);
                    }
                }
            }

            //  マッピングデータが存在しないならば、そのまま直接入力する。
            else
            {
                System.Diagnostics.Debug.WriteLine($"{character}");

                KeyboardOperator.SetInputMode(InputMode.Direct);
                KeyboardOperator.InputDirectly(character);
            }
        }

        private KeyMapping? GetMappingData(string character)
        {
            if (this.dictionary.TryGetValue(character, out var data))
                return data;

            return null;
        }
    }
}
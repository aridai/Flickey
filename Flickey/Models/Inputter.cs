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
        private readonly IReadOnlyDictionary<string, KeyMapping> mappingDictionary;

        private readonly IReadOnlyDictionary<char, char> hiraganaConversionDictionary;

        //  前回の入力を保持する。
        //  ひらがなの濁点・半濁点・小文字と
        //  アルファベットの大文字・小文字の切り替えるのに使う。
        private char? prevCharacter;

        /// <summary>
        /// キーマッピングが保存されたJSONファイル名。
        /// </summary>
        public const string KeyMappingFileName = "Mapping.json";

        /// <summary>
        /// ひらがな変換のデータが保存されたJSONファイル名。
        /// </summary>
        public const string HiraganaConversionFileName = "Conversion.json";

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        public Inputter()
        {
            //  Jsonからマッピングデータを読み込み、
            //  入力文字からキーコードを検索できる連想配列を作る。
            var mappingJson = File.ReadAllText(KeyMappingFileName);
            var settings = new JsonSerializerSettings { Converters = new[] { new StringEnumConverter() } };
            this.mappingDictionary = JsonConvert.DeserializeObject<List<KeyMapping>>(mappingJson, settings)
                .ToDictionary(data => data.Character, data => data);

            //  Jsonからひらがな変換データを読み込み、
            //  変換後の文字を検索できる連想配列を作る。
            var hiraganaConversionJson = File.ReadAllText(HiraganaConversionFileName);
            this.hiraganaConversionDictionary = JsonConvert.DeserializeObject<List<List<char>>>(hiraganaConversionJson)
                .ToDictionary(data => data[0], data => data[1]);
        }

        /// <summary>
        /// 入力処理を行います。
        /// </summary>
        /// <param name="character">入力文字。</param>
        public void Input(string character)
        {
            if (character == "a/A")
            {
                this.prevCharacter = (this.prevCharacter != null)
                    ? this.ToggleLetterCase(this.prevCharacter.Value) : default;
            }

            else if (character == "゛゜小")
            {
                this.prevCharacter = (this.prevCharacter != null)
                    ? this.ConvertHiragana(this.prevCharacter.Value) : default;
            }
            
            else
            {
                //  マッピングデータが存在するならば、それを使う。
                var result = this.GetMappingData(character);
                if (result is KeyMapping data)
                {
                    var mode = data.InputMode;
                    var codes = data.KeyCodes;

                    //  日本語入力モードならば、キーストロークをシミュレートして入力する。
                    if (mode == InputMode.Japanese)
                    {
                        KeyboardOperator.SetInputMode(InputMode.Japanese);
                        KeyboardOperator.InputKey(codes);

                        //  入力文字を更新する。
                        //  ひらがなの変換で使うため。
                        this.prevCharacter = (data.Character.Length == 1 && this.IsHiragana(data.Character[0]))
                            ? data.Character[0] : default(char?);
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

                        var hoge = data.Character[0];

                        //  入力文字を更新する。
                        //  大文字・小文字の変換で使うため。
                        this.prevCharacter = (data.Character.Length == 1 && this.IsEnlishAlphabet(data.Character[0]))
                            ? char.ToLower(data.Character[0]) : default(char?);
                    }
                }

                //  マッピングデータが存在しないならば、そのまま直接入力する。
                else
                {
                    KeyboardOperator.SetInputMode(InputMode.Direct);
                    KeyboardOperator.InputDirectly(character);
                    this.prevCharacter = null;
                }
            }
        }

        //  キーマッピングデータを取得する。
        private KeyMapping? GetMappingData(string character)
        {
            if (this.mappingDictionary.TryGetValue(character, out var data))
                return data;

            return null;
        }

        //  ひらがなの変換後の文字を取得する。
        //  変換できない場合はnullを返す。
        private char? GetHiraganaConversionCharacter(char character)
        {
            if (this.hiraganaConversionDictionary.TryGetValue(character, out var next))
                return next;

            return null;
        }

        //  アルファベットの大文字と小文字を切り替える。
        private char? ToggleLetterCase(char character)
        {
            if (this.IsEnlishAlphabet(character))
            {
                //  1文字消して、ケースを反転した文字を入力する。
                KeyboardOperator.SetInputMode(InputMode.Direct);
                KeyboardOperator.InputKey(VirtualKeyCode.Back);

                character = char.IsLower(character) ? char.ToUpper(character) : char.ToLower(character);
                KeyboardOperator.InputDirectly(character.ToString());

                return character;
            }

            return null;
        }

        //  ひらがなを変換する。
        private char? ConvertHiragana(char character)
        {
            if (this.GetHiraganaConversionCharacter(character) is char c)
            {
                if (this.GetMappingData(c.ToString()) is KeyMapping data)
                {
                    //  1文字消して、変換後の文字を入力する。
                    KeyboardOperator.SetInputMode(data.InputMode);
                    KeyboardOperator.InputKey(VirtualKeyCode.Back);

                    KeyboardOperator.InputKey(data.KeyCodes);
                    return c;
                }
            }

            return null;
        }

        //  指定した文字が日本語のひらがなかどうかを判定する。
        private bool IsHiragana(char character)
        {
            var code = (int)character;
            return 'あ' <= code && code <= 'ん';
        }

        //  指定した文字が英語のアルファベットかどうかを判定する。
        private bool IsEnlishAlphabet(char character)
        {
            //  大文字と小文字の文字コードは連続していない。
            //  大文字か小文字のいずれかならアルファベットであると判定する。
            var code = (int)character;
            return 'A' <= code && code <= 'Z' || 'a' <= code && code <= 'z';
        }
    }
}
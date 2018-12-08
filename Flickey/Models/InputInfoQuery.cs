using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flickey.Models
{
    using Flickey.Models.PInvokeComponents;

    /// <summary>
    /// 入力文字に対する入力情報を取得する機能を提供します。
    /// </summary>
    public sealed class InputInfoQuery
    {
        private readonly IFileReader fileReader;

        private readonly IReadOnlyDictionary<string, InputInfo> dictionary;

        /// <summary>
        /// JSONファイルのパス。
        /// </summary>
        public const string JsonFileName = "Mapping.json";

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        /// <param name="fileReader">ファイルリーダ。</param>
        public InputInfoQuery(IFileReader fileReader)
        {
            this.fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            var jsonStr = this.fileReader.ReadAllText(JsonFileName);

            var settings = new JsonSerializerSettings { Converters = new[] { new StringEnumConverter() } };
            var obj = JsonConvert.DeserializeObject<List<Data>>(jsonStr, settings);

            this.dictionary = obj.Select(data => (character: data.Character, mode: data.Mode, structures: this.GenerateInputStructures(data.KeyCodes)))
                .Select(tuple => tuple.structures != null ? new InputInfo(tuple.character, tuple.mode, tuple.structures) : null)
                .Where(info => info != null)
                .ToDictionary(info => info.Character, info => info, StringComparer.Ordinal);
        }

        /// <summary>
        /// 指定した入力文字に対する入力情報を取得します。
        /// </summary>
        /// <param name="character">入力文字。</param>
        /// <returns>入力情報が存在する場合は入力情報を返します。存在しない場合はnullを返します。</returns>
        public InputInfo GetInputInfo(string character)
        {
            if (this.dictionary.TryGetValue(character, out var info)) return info;
            return null;
        }

        //  仮想キーコードの2次元リストからINPUT構造体のリストを作る。
        private IReadOnlyList<INPUT> GenerateInputStructures(IReadOnlyList<IReadOnlyList<VirtualKeyCode>> keys)
        {
            var list = new LinkedList<INPUT>();

            for (int step = 0; step <= keys.Count; step++)
            {
                //  最初と最後に空配列があるかのように振る舞わせる。
                var current = (step != keys.Count) ? keys[step] : Enumerable.Empty<VirtualKeyCode>();
                var prev = (step != 0) ? keys[step - 1] : Enumerable.Empty<VirtualKeyCode>();

                //  前ステップで書かれていて、現ステップで書かれていないキーは離す。
                var inputs1 = prev.Where(key => !current.Contains(key))
                    .Select(key => new INPUT { type = InputType.INPUT_KEYBOARD, ki = new KEYBDINPUT { wVk = (ushort)key, dwFlags = KeyboardInputFlag.KEYEVENTF_KEYUP } });

                //  現ステップで書かれていて、前ステップで書かれていないキーは押す。
                var inputs2 = current.Where(key => !prev.Contains(key))
                    .Select(key => new INPUT { type = InputType.INPUT_KEYBOARD, ki = new KEYBDINPUT { wVk = (ushort)key, dwFlags = KeyboardInputFlag.KEYEVENTF_NONE } });

                foreach (var input in Enumerable.Concat(inputs1, inputs2)) list.AddLast(input);
            }

            return list.ToList();
        }

        [DataContract]
        internal struct Data
        {
            [DataMember(Name = "character")]
            public string Character { get; set; }

            [DataMember(Name = "mode")]
            public InputMode Mode { get; set; }

            [DataMember(Name = "keys")]
            public List<List<VirtualKeyCode>> KeyCodes { get; set; }
        }
    }
}
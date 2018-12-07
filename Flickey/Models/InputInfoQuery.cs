using System;
using System.Collections.Generic;
using System.Linq;
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
            var obj = JsonConvert.DeserializeObject<List<(string character, InputMode mode, List<List<VirtualKeyCode>> keys)>>(jsonStr, settings);

            this.dictionary = obj.Select(tuple => (tuple.character, tuple.mode, structures: this.GenerateInputStructure(tuple.keys)))
                .Select(tuple => tuple.structures != null ? new InputInfo(tuple.character, tuple.mode, tuple.structures) : null)
                .Where(info => info != null)
                .ToDictionary(info => info.Character, info => info);
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

        /// <summary>
        /// 仮想キーコードの2次元リストから<see cref="INPUT"/>構造体のリストを生成します。
        /// </summary>
        /// <param name="keys">仮想キーコードの2次元リスト。</param>
        /// <returns>処理に成功した場合は<see cref="INPUT"/>構造体のリストを返します。失敗した場合はnullを返します。</returns>
        public IReadOnlyList<INPUT> GenerateInputStructure(IReadOnlyList<IReadOnlyList<VirtualKeyCode>> keys)
        {
            return null;
        }
    }
}
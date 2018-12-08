using System;
using System.Collections.Generic;

namespace Flickey.Models
{
    /// <summary>
    /// テスト用の<see cref="IFileReader"/>実装です。
    /// </summary>
    public sealed class TestFileReader : IFileReader
    {
        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

        /// <summary>
        /// 指定したファイルパスに対応するテキストを紐づけます。
        /// </summary>
        /// <param name="filePath"><see cref="ReadAllText(string)"/>のfilePathに指定する値。</param>
        /// <param name="text">指定した<paramref name="filePath"/>に対して返される文字列。</param>
        /// <returns>登録が成功した場合はtrueを返し、失敗した場合はfalseを返します。</returns>
        public bool RegisterStringPair(string filePath, string text)
        {
            try
            {
                this.dictionary.Add(filePath, text);
                return true;
            }

            catch
            {
                return false;
            }
        }

        public string ReadAllText(string filePath)
        {
            if (this.dictionary.TryGetValue(filePath, out var text)) return text;
            return string.Empty;
        }
    }
}
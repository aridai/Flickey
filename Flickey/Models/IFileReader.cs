using System;

namespace Flickey.Models
{
    /// <summary>
    /// ファイルを開き、テキストを取得する機能を提供します。
    /// </summary>
    public interface IFileReader
    {
        /// <summary>
        /// すべての行のテキストを取得します。
        /// 失敗したときは空の文字列が返ります。
        /// </summary>
        /// <param name="filePath">対象のファイルのパス。</param>
        /// <returns>ファイルの内容。</returns>
        string ReadAllText(string filePath);
    }
}
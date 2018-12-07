using System;
using System.IO;

namespace Flickey.Models
{
    /// <summary>
    /// ファイルを開いて内容を取得する機能を提供します。
    /// </summary>
    public sealed class FileReader : IFileReader
    {
        /// <summary>
        /// 実際にファイルを開いて、全行のテキストを取得します。
        /// </summary>
        /// <param name="filePath">対象のファイルのパス。</param>
        /// <returns>ファイルの内容。失敗した場合は空の文字列が返ります。</returns>
        public string ReadAllText(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }

            catch
            {
                return string.Empty;
            }
        }
    }
}
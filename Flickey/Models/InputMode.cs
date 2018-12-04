using System;
using System.Runtime.Serialization;

namespace Flickey.Models
{
    /// <summary>
    /// IMEの入力モードを列挙します。
    /// </summary>
    [DataContract]
    public enum InputMode
    {
        /// <summary>
        /// 入力モードを示しません。
        /// </summary>
        [DataMember(Name = nameof(None))]
        None,

        /// <summary>
        /// IMEが無効です。
        /// </summary>
        [DataMember(Name = nameof(Disabled))]
        Disabled,

        /// <summary>
        /// 直接入力モード。
        /// </summary>
        [DataMember(Name = nameof(Direct))]
        Direct,

        /// <summary>
        /// 日本語入力モード。
        /// </summary>
        [DataMember(Name = nameof(Japanese))]
        Japanese,

        /// <summary>
        /// 不明な入力モード。
        /// </summary>
        [DataMember(Name = nameof(Unknown))]
        Unknown
    }
}
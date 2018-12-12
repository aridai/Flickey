using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flickey.Controls.KeyboardComponents
{
    [DataContract]
    public struct KeyLabel
    {
        [DataMember(Name = "style")]
        [JsonConverter(typeof(StringEnumConverter))]
        public LabelDisplayStyle LabelStyle { get; set; }

        [DataMember(Name = "characters")]
        public IReadOnlyList<string> Characters { get; set; }
    }
}
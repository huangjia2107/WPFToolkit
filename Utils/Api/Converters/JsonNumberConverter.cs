using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Api.Converters
{
    /*
    /// <summary>
    /// Json 中 number 的值为 null 时，返回指定的 number 值
    /// </summary>
    public class JsonNumberConverter : JsonConverter<int>
    {
        private int _valueWhenNull = -1;

        public JsonNumberConverter() { }

        public JsonNumberConverter(int valueWhenNull)
        {
            _valueWhenNull = valueWhenNull;
        }

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return _valueWhenNull;

            if (reader.TryGetInt32(out int value))
                return value;

            return _valueWhenNull;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    */
}

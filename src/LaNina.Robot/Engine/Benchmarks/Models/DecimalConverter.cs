﻿using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class DecimalConverter: JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.ToObject<decimal>();
            }
            if (token.Type == JTokenType.String)
            {
                // customize this to suit your needs
                return decimal.Parse(token.ToString(), NumberStyles.Number | NumberStyles.AllowExponent);
            }
            if (token.Type == JTokenType.Null && objectType == typeof(decimal?))
            {
                return null;
            }
            throw new JsonSerializationException("Unexpected token type: " +  token.Type);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = default(decimal?);
            if (value != null)
            {
                d = value as decimal?;
                if (d.HasValue) // If value was a decimal?, then this is possible
                {
                    d = new decimal(decimal.ToDouble(d.Value)); // The ToDouble-conversion removes all unnessecary precision
                }
            }
            JToken.FromObject(d).WriteTo(writer);
        }
    }
}
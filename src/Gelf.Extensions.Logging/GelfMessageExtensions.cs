using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Gelf.Extensions.Logging
{
    public static class GelfMessageExtensions
    {
        private static bool IsNumeric(object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }

        public static string ToJson(this GelfMessage message)
        {
            var messageJson = JObject.FromObject(message);

            foreach (var field in message.AdditionalFields)
            {
                object value = field.Value;
                var type = value.GetType();
                if(type.GetGenericTypeDefinition() == typeof(Func<>))
                {
                    var args = type.GenericTypeArguments;
                    if(args.Length == 1)
                    {
                        value = type.GetRuntimeMethod("Invoke", new Type[0]).Invoke(value, new object[0]);
                    }
                }
                if(IsNumeric(value))
                {
                    messageJson[$"_{field.Key}"] = JToken.FromObject(value);
                }
                else
                {
                    messageJson[$"_{field.Key}"] = value?.ToString();
                }
            }

            return JsonConvert.SerializeObject(messageJson, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}

using System;
using System.Collections.Generic;

namespace Wenjian.Client.Net
{
    public sealed class FirstChainMessage
    {
        private readonly Dictionary<string, string> fields;

        private FirstChainMessage(string raw, Dictionary<string, string> fields)
        {
            Raw = raw;
            this.fields = fields;
        }

        public string Raw { get; }

        public string Type => GetString("type");

        public string Code => GetString("code");

        public static bool TryParse(string payload, out FirstChainMessage message)
        {
            message = null;
            if (string.IsNullOrWhiteSpace(payload))
            {
                return false;
            }

            var parsedFields = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] tokens = payload.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                int separator = token.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                string key = token.Substring(0, separator);
                string value = separator == token.Length - 1 ? string.Empty : token.Substring(separator + 1);
                parsedFields[key] = value;
            }

            if (!parsedFields.TryGetValue("type", out string type) || string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            message = new FirstChainMessage(payload, parsedFields);
            return true;
        }

        public bool Has(string key)
        {
            return fields.ContainsKey(key);
        }

        public string GetString(string key)
        {
            if (!fields.TryGetValue(key, out string value))
            {
                throw new KeyNotFoundException($"First-chain message field '{key}' is missing.");
            }

            return value;
        }

        public int GetInt(string key)
        {
            string value = GetString(key);
            if (!int.TryParse(value, out int result))
            {
                throw new FormatException($"First-chain message field '{key}' is not a valid int: {value}");
            }

            return result;
        }

        public long GetLong(string key)
        {
            string value = GetString(key);
            if (!long.TryParse(value, out long result))
            {
                throw new FormatException($"First-chain message field '{key}' is not a valid long: {value}");
            }

            return result;
        }
    }
}

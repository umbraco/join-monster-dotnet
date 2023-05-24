using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using JoinMonster.Exceptions;

namespace JoinMonster
{
    internal static class ConnectionUtils
    {
        private const string Prefix = "arrayconnection:";

        public static int CursorToOffset(string cursor)
        {
            return Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Substring(Prefix.Length));
        }

        public static string OffsetToCursor(int offset)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Prefix}{offset}"));
        }

        public static string ObjectToCursor(object obj)
        {
            var str = JsonSerializer.Serialize(obj);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static IDictionary<string, object> CursorToObject(string cursor)
        {
            try
            {
                var str = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                return JsonSerializer.Deserialize<IDictionary<string, object>>(str)!; // The JSON value cannot be null
            }
            catch (Exception ex) when (ex is FormatException or JsonException)
            {
                throw new InvalidCursorException(cursor, message: "Provided cursor is not base64 as expected.");
            }
        }
    }
}

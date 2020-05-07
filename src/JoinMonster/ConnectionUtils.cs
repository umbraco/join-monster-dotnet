using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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
            var str = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return JsonSerializer.Deserialize<IDictionary<string, object>>(str);
        }
    }
}

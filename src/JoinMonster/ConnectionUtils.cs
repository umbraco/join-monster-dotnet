using System;
using System.Text;

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
    }
}

using System;
using System.Collections.Generic;

namespace JoinMonster
{
    /// <summary>
    /// Generates short aliases for tables and columns, all table aliases will be unique while column aliases will not.
    /// </summary>
    public class MinifyAliasGenerator : IAliasGenerator, IDisposable
    {
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ#$".ToCharArray();
        private readonly IEnumerator<string> _enumerator;
        private readonly Dictionary<string, string> _columnAliases;

        /// <summary>
        /// Creates a new instance of <see cref="MinifyAliasGenerator" />.
        /// </summary>
        public MinifyAliasGenerator()
        {
            _enumerator = CreateEnumerator();
            _columnAliases = new Dictionary<string, string>();
        }

        /// <summary>
        /// Generates a unique short alias, ignoring the <paramref name="name"/> parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A unique alias.</returns>
        public string GenerateTableAlias(string name) => Next();

        /// <summary>
        /// Generates a short alias, the alias will not be unique because it will be prefixed.
        /// </summary>
        /// <param name="name">The name to generate a alias for</param>
        /// <returns>A short alias.</returns>
        public string GenerateColumnAlias(string name)
        {
            if (_columnAliases.TryGetValue(name, out var alias))
                return alias;

            return _columnAliases[name] = Next();
        }

        private string Next()
        {
            _enumerator.MoveNext();
            return _enumerator.Current;
        }

        private static IEnumerator<string> CreateEnumerator()
        {
            foreach (var c in _chars)
            {
                yield return c.ToString();
            }

            using var enumerator = CreateEnumerator();
            do
            {
                foreach (var c in _chars)
                    yield return enumerator.Current + c;

            } while (enumerator.MoveNext());
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}

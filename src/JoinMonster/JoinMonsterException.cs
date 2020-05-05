using System;
using System.Runtime.Serialization;

namespace JoinMonster
{
    /// <summary>
    /// The exception that is thrown when JoinMonster encounters something unexpected.
    /// </summary>
    public class JoinMonsterException : Exception
    {
        /// <inheritdoc />
        public JoinMonsterException()
        {
        }

        /// <inheritdoc />
        protected JoinMonsterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public JoinMonsterException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public JoinMonsterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

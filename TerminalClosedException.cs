using System;
using System.Runtime.Serialization;

namespace DotCOM
{
    [Serializable]
    public class TerminalClosedException : Exception
    {
        public TerminalClosedException()
        {

        }

        public TerminalClosedException(string message) : base(message)
        {

        }

        public TerminalClosedException(string message, Exception inner) : base(message, inner)
        {

        }
        
        protected TerminalClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
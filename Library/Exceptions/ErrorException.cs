using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Exceptions
{
    public class ErrorException : Exception
    {
        public enum EXCEPTION_RESULT
        {
            ERROR = 1,
            WARNING = 2,
            INFO = 3
        }
        public EXCEPTION_RESULT exception_result;
        public Dictionary<string, object> data;
        public ErrorException() : base() { }
        public ErrorException(string message, EXCEPTION_RESULT exception_result = EXCEPTION_RESULT.ERROR, Dictionary<string, object> data=null) : base(message) {
            this.exception_result = exception_result;
            this.data = data;
        }
        public ErrorException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ErrorException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
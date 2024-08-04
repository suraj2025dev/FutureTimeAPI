using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Exceptions
{
    public class DaoException : Exception
    {
        public enum EXCEPTION_RESULT
        {
            ERROR=1,
            WARNING=2,
            INFO=3
        }

        public Dictionary<string,object> data;

        public EXCEPTION_RESULT exception_result;
        public DaoException() : base() { }
        public DaoException(string message, EXCEPTION_RESULT exception_result=EXCEPTION_RESULT.ERROR, Dictionary<string, object> data =null) : base(message) {
            this.exception_result = exception_result;
            this.data = data;
        }
        public DaoException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected DaoException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
using System;
using System.Runtime.Serialization;

namespace Nop.Core
{
    /// <summary>
    /// Represents errors that occur during application execution
    /// </summary>
    [Serializable]
    public class NopException : Exception
    {
        public string ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        public NopException(string errorCode = null)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NopException(string message, string errorCode =null)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
		/// <param name="messageFormat">The exception message format.</param>
		/// <param name="args">The exception message arguments.</param>
        public NopException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {            
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
		/// <param name="messageFormat">The exception message format.</param>
		/// <param name="args">The exception message arguments.</param>
        public NopException(string messageFormat, string errorCode = null, params object[] args)
			: base(string.Format(messageFormat, args))
		{
            this.ErrorCode = errorCode;
        }
        

        /// <summary>
        /// Initializes a new instance of the Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NopException(SerializationInfo
            info, StreamingContext context, string errorCode = null)
            : base(info, context)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public NopException(string message, Exception innerException, string errorCode = null)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }
    }
}

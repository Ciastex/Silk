using System;
using System.ComponentModel;

namespace Silk.Compiler
{
    public class Error
    {
        public ErrorLevel Level { get; private set; }
        public ErrorCode Code { get; private set; }
        public string Description { get; private set; }
        public int Line { get; private set; }

        /// <summary>
        /// Constructs an Error instance.
        /// </summary>
        /// <param name="level">Error level.</param>
        /// <param name="code">Error code</param>
        /// <param name="line">Error line number.</param>
        internal Error(ErrorLevel level, ErrorCode code, int line)
        {
            Level = level;
            Code = code;
            Description = GetEnumDescription(code);
            Line = line;
        }

        /// <summary>
        /// Constructs an Error instance.
        /// </summary>
        /// <param name="level">Error level.</param>
        /// <param name="code">Error code</param>
        /// <param name="description">Additional description about error.</param>
        /// <param name="line">Error line number.</param>
        internal Error(ErrorLevel level, ErrorCode code, string description, int line)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            Level = level;
            Code = code;
            Description = GetEnumDescription(code);
            Description += $" : {description}";
            Line = line;
        }

        /// <summary>
        /// Constructs an Error instance.
        /// </summary>
        /// <param name="level">Error level.</param>
        /// <param name="code">Error code</param>
        /// <param name="token">The token associated with this error.</param>
        internal Error(ErrorLevel level, ErrorCode code, Token token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            Level = level;
            Code = code;
            Description = GetEnumDescription(code);
            Description += $" : \"{token.Value}\"";
            Line = token.Line;
        }

        /// <summary>
        /// Constructs an Error instance.
        /// </summary>
        /// <param name="level">Error level.</param>
        /// <param name="code">Error code</param>
        /// <param name="description">Additional description about error.</param>
        /// <param name="token">The token associated with this error.</param>
        internal Error(ErrorLevel level, ErrorCode code, string description, Token token)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            Level = level;
            Code = code;
            Description = GetEnumDescription(code);
            Description += $" : {description}";
            Description += $" : \"{token.Value}\"";
            Line = token.Line;
        }

        /// <summary>
        /// Returns a description of this error.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} 1{1,03:D3} : {2} (Line {3})",
                GetEnumDescription(Level),
                (int)Code,
                Description,
                Line);
        }

        /// <summary>
        /// Converts an enum value to its associated description string.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        private string GetEnumDescription<T>(T value) where T : struct
        {
            var memberInfo = typeof(T).GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                    return ((DescriptionAttribute)attributes[0]).Description;
            }
            return value.ToString();
        }
    }
}

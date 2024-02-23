using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// An exception thrown by the <see cref="RoboCommandParser"/>
    /// </summary>
    public class RoboCommandParserException : Exception
    {
        private RoboCommandParserException() :base() { }

        internal RoboCommandParserException(string message) : base(message) { }

        /// <returns>Contains parameter data about the function that resulted in the exception.</returns>
        /// <inheritdoc cref="Exception.Data"/>
        public override IDictionary Data => (IDictionary)_data;

        private readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        internal void AddData(string key, object value) => _data.Add(key, value);
    }
}

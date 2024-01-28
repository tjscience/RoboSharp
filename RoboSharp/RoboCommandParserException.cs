using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    internal class RoboCommandParserException : Exception
    {
        private RoboCommandParserException() :base() { }

        public RoboCommandParserException(string message) : base(message) { }

        public override IDictionary Data => (IDictionary)_data;

        private readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        public void AddData(string key, object value) => _data.Add(key, value);
    }
}

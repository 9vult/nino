using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localizer
{
    internal class LocalizationException : Exception
    {
        public LocalizationException() { }

        public LocalizationException(string message) : base(message) { }

        public LocalizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}

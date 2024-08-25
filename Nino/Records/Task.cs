using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Task
    {
        public required string Id;
        public required string Abbreviation;
        public required bool Done;
    }
}

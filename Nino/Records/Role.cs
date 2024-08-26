using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Role
    {
        public required string Abbreviation;
        public required string Name;
        public decimal? Weight;
    }
}

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record CongaParticipant
    {
        public required string Id;
        public required string Current;
        public required string Next;
    }
}

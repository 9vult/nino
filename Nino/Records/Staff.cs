using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Staff
    {
        public required string Id;
        public required string UserId;
        public required Role Role;
    }
}

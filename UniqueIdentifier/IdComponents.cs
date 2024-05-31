using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueIdentifier
{
    public class IdComponents
    {
        public ulong TimeStamp { get; set; }
        public ulong ServiceId { get; set; }

        public ulong InstanceId { get; set; }
    }
}

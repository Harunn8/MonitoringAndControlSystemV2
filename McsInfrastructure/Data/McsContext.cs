using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McsInfrastructure.Data
{
    public class McsContext
    {
        public string DatabaseName { get; set; }
        public Collections Collections { get; set; }
    }

    public class Collections
    {
        public string User { get; set; }
        public string SnmpDevice { get; set; }
        public string TcpDevice { get; set; }
    }
}

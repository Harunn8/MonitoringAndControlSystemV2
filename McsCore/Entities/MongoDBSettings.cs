using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McsCore.Entities
{
    public class MongoDBSettings
    {
        public string DatabaseName { get; set; }
        public Collections Collections { get; set; }
    }

    public class Collections
    {
        public string User { get; set; }
        public string Device { get; set; }
        public string TcpDevice { get; set; }
    }
}

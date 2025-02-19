using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McsCore.Entities
{
    public class OidMappingRequest
    {
        public SnmpDevice Device { get; set; }
        public List<OidParameter> Mappings { get; set; }
    }

    public class OidParameter
    {
        public string Oid { get; set; }
        public string ParameterName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Base
{
    public interface ISnmpService
    {
        public Task StartSnmpCommunication(string ipAddress, int port, List<string> oidList, Action<string> onMessageReceived, CancellationToken cancellationToken);
        public void StopSnmpCommunication();
        public Task SendSnmpCommand(string ipAddress, int port, string oid, string value);
    }
}

using System;
using System.Collections.Generic;

namespace McsCore.Entities
{
    public class WebSocketCommand
    {
        public string Action { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}

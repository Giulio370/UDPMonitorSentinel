using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitOnLive_MonitoringApp
{
    public class Monitor
    {
        public string DisplayDevice { get; set; }
        public string Name { get; set; }
        public int ID_Monitor { get; set; }
        public int Refresh_Rate { get; set; }
        public int ID_GPU { get; set; }
    }
}

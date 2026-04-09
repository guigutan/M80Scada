using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M80Scada
{
    public class MyMachine
    {      

        public string No { get; set; }
        public string ScadaNO { get; set; }
        public int MachineID { get; set; }
        public string MachineNO { get; set; }
        public string IpAddr { get; set; }
        public int PortNum { get; set; }


        //  tempOneToMany tempItem
        public int tempOneToMany { get; set; }
        public string tempItem { get; set; }

    }
}

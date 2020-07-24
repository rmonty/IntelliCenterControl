using System;
using System.Collections.Generic;

namespace IntelliCenterControl.Models
{
    public class HardwareDefinition
    {

        public string Command { get; set; }
        public Guid messageId { get; set; }
        public string QueryName { get; set; }
        public string Description { get; set; }
        public string Response { get; set; }
        public List<Panel> answer { get; set; }


        public partial class Panel
        {
            public string Objnam { get; set; }
            public PanelParams Params { get; set; }
        }

        public partial class PanelParams
        {
            public string Objtyp { get; set; }
            public string Subtyp { get; set; }
            public string Hname { get; set; }
            public string Sname { get; set; }
            public string Panid { get; set; }
            public string Listord { get; set; }
            public string Ver { get; set; }
            public List<PanelObjList> Objlist { get; set; }
        }

        public partial class PanelObjList
        {
            public string Objnam { get; set; }
            public ModuleParams Params { get; set; }
        }

        public partial class ModuleParams
        {
            public string Objtyp { get; set; }
            public string Subtyp { get; set; }
            public string Sname { get; set; }
            public string Listord { get; set; }
            public string Parent { get; set; }
            public string Port { get; set; }
            public string Ver { get; set; }
            public string Badge { get; set; }
            public List<ModuleCircuit> Circuits { get; set; }
            public string Hname { get; set; }
            public string Cover { get; set; }
            public string Body { get; set; }
            public string Freeze { get; set; }
            public string Timzon { get; set; }
            public string Time { get; set; }
            public string Rly { get; set; }
            public string Dntstp { get; set; }
            public string Featr { get; set; }
            public List<PumpParams> Objlist { get; set; }
            public string Comuart { get; set; }
            public string Primflo { get; set; }
            public string Primtim { get; set; }
            public string Systim { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
            public string Minf { get; set; }
            public string Maxf { get; set; }
            public string Prior { get; set; }
            public string Share { get; set; }
            public string Dly { get; set; }
            public string Settmp { get; set; }
            public string Settmpnc { get; set; }
            public string Name { get; set; }
            public string Source { get; set; }
            public string Probe { get; set; }
            public string Calib { get; set; }
            public string Act { get; set; }
        }

        public partial class ModuleCircuit
        {
            public string Objnam { get; set; }
            public BodyParams Params { get; set; }
        }

        public partial class BodyParams
        {
            public string Objtyp { get; set; }
            public string Subtyp { get; set; }
            public string Sname { get; set; }
            public string Listord { get; set; }
            public string Hitmp { get; set; }
            public string Lotmp { get; set; }
            public string Htsrc { get; set; }
            public string Share { get; set; }
            public string Prim { get; set; }
            public string Sec { get; set; }
            public string Act1 { get; set; }
            public string Act2 { get; set; }
            public string Act3 { get; set; }
            public string Act4 { get; set; }
            public string Vol { get; set; }
            public string Manht { get; set; }
            public List<BodyObject> Objlist { get; set; }
            public string Hname { get; set; }
            public string Parent { get; set; }
            public string Body { get; set; }
            public string Freeze { get; set; }
            public string Ver { get; set; }
            public string Timzon { get; set; }
            public string Time { get; set; }
            public string Rly { get; set; }
            public string Dntstp { get; set; }
            public string Featr { get; set; }
            public string Cover { get; set; }
            public string Comuart { get; set; }
            public string Posit { get; set; }
            public string Assign { get; set; }
            public string Dly { get; set; }
            public string Circuit { get; set; }
            public string Start { get; set; }
            public string Stop { get; set; }
            public string COOL { get; set; }
            public string BOOST { get; set; }
            public string HTMODE { get; set; }
        }

        public partial class BodyObject
        {
            public string Objnam { get; set; }
            public BodyObjectParams Params { get; set; }
        }

        public partial class BodyObjectParams
        {
            public string Objtyp { get; set; }
            public string Subtyp { get; set; }
            public string Sname { get; set; }
            public string Comuart { get; set; }
        }

        public partial class PumpParams
        {
            public string Objnam { get; set; }
            public PumpCircuit Params { get; set; }
        }

        public partial class PumpCircuit
        {
            public string Objtyp { get; set; }
            public string Circuit { get; set; }
            public string Act { get; set; }
            public string Listord { get; set; }
            public string Dly { get; set; }
            public string Speed { get; set; }
            public string Boost { get; set; }
            public string Select { get; set; }
        }
    }

}


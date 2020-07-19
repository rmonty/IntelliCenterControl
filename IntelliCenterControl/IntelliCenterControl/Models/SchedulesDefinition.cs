using System.Collections.Generic;

namespace IntelliCenterControl.Models
{
    public class SchedulesDefinition
    {
        public string command { get; set; }
        public string messageID { get; set; }
        public string response { get; set; }
        public IList<Schedule> objectList { get; set; }

        public class Schedule
        {
            public string objnam { get; set; }
            public ScheduleParams Params { get; set; }
        }

        public class ScheduleParams
        {
            public string OBJNAM { get; set; }
            public string OBJTYP { get; set; }
            public string LISTORD { get; set; }
            public string CIRCUIT { get; set; }
            public string SNAME { get; set; }
            public string DAY { get; set; }
            public string SINGLE { get; set; }
            public string START { get; set; }
            public string TIME { get; set; }
            public string STOP { get; set; }
            public string TIMOUT { get; set; }
            public string GROUP { get; set; }
            public string HEATER { get; set; }
            public string COOLING { get; set; }
            public string LOTMP { get; set; }
            public string SMTSRT { get; set; }
            public string VACFLO { get; set; }
            public string STATUS { get; set; }
            public string DNTSTP { get; set; }
            public string ACT { get; set; }
            public string MODE { get; set; }
            public string HITMP {get; set;}
            public string STATIC {get; set;}
            public string UPDATE {get; set;}
        }
    }


    
}


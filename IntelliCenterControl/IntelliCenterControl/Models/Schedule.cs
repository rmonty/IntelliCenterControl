using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IntelliCenterControl.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    [Flags]
    public enum DaysEnum
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    public class Schedule : Circuit<IntelliCenterConnection>
    {
        public const string ScheduleKeys =
            "[\"OBJNAM : OBJTYP : LISTORD : CIRCUIT : SNAME : DAY : SINGLE : START : TIME : STOP : TIMOUT : GROUP : HEATER : COOLING : LOTMP : SMTSRT : VACFLO : STATUS : DNTSTP : ACT : MODE\"]";

        public enum TimeType
        {
            [Description("Manual Time")] ABSTIM,
            [Description("Sunrise")] SRIS,
            [Description("Sunset")] SSET
        }

        public Command SaveScheduleCommand {get; set;}
        public Command DeleteScheduleCommand {get; set;}

        public bool IsNew {get;set;}

        public List<string> TimeTypeNames => GetDescriptions(typeof(TimeType)).ToList();

        private TimeSpan _startTime = new TimeSpan(8,0,0);

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if(_startTime == value) return;
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _endTime = new TimeSpan(17,0,0);

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if(_endTime == value) return;
                _endTime = value;
                OnPropertyChanged();
            }
        }

        private string _days;

        public string Days
        {
            get => _days;
            set
            {
                if(_days == value) return;
                _days = value;
                OnPropertyChanged();
            }
        }

        private bool _repeats;

        public bool Repeats
        {
            get => _repeats;
            set
            {
                if(value == _repeats) return;
                _repeats = value;
                OnPropertyChanged();
            }
        }

        private bool _single;

        public bool Single
        {
            get => _single;
            set
            {
                if(_single == value) return;
                _single = value;
                OnPropertyChanged();
            }
        }


        private TimeType _startTimeType = TimeType.ABSTIM;

        public TimeType StartTimeType
        {
            get => _startTimeType;
            set
            {
                if(_startTimeType == value) return;
                _startTimeType = value;
                OnPropertyChanged();
            }
        }

        private TimeType _endTimeType = TimeType.ABSTIM;

        public TimeType EndTimeType
        {
            get => _endTimeType;
            set
            {
                if(_endTimeType == value) return;
                _endTimeType = value;
                OnPropertyChanged();
            }
        }

        private DaysEnum _scheduledDays = new DaysEnum();

        public DaysEnum ScheduledDays
        {
            get => _scheduledDays;
            set
            {
                if(_scheduledDays == value) return;
                _scheduledDays = value;
                SetDays();
                OnPropertyChanged();
            }
        }

        private DaysEnum _scheduledDay;

        public DaysEnum ScheduledDay
        {
            get => _scheduledDay;
            set
            {
                if(_scheduledDay == value) return;
                _scheduledDay = value;
                SetDays();
                OnPropertyChanged();
            }
        }


        private Circuit<IntelliCenterConnection> _scheduledCircuit;

        public Circuit<IntelliCenterConnection> ScheduledCircuit
        {
            get => _scheduledCircuit;
            set
            {
                if(_scheduledCircuit == value) return;
                _scheduledCircuit = value;
                Name = _scheduledCircuit.Name;
                OnPropertyChanged();
            }
        }

        private Heater _selectedHeater;

        public Heater SelectedHeater
        {
            get => _selectedHeater;
            set
            {
                if(_selectedHeater == value) return;
                _selectedHeater = value;
                OnPropertyChanged();
            }
        }

        private string _lotmp = "78";

        public string Lotmp
        {
            get => _lotmp;
            set
            {
                if(_lotmp == value) return;
                _lotmp = value;
                OnPropertyChanged();
            }
        }

        private string cooling = "81";

        private bool _expanded;

        public bool Expanded
        {
            get => _expanded;
            set
            {
                if(value == _expanded) return;
                _expanded = value;
                OnPropertyChanged();
            }
        }


        public Schedule(Circuit<IntelliCenterConnection>.CircuitType circuitType, 
            IDataInterface<IntelliCenterConnection> dataInterface) : base(circuitType, dataInterface)
        {
            Active = true;
            SaveScheduleCommand = new Command(async () => await ExecuteSaveScheduleCommand());
            DeleteScheduleCommand = new Command(async () => await ExecuteDeleteScheduleCommand());
            Expanded = true;
            Single = true;
        }

        public Schedule(string name, CircuitType circuitType, SchedulesDefinition.Schedule definition,
            Circuit<IntelliCenterConnection> scheduledCircuit, string hName,
            IDataInterface<IntelliCenterConnection> dataInterface) : base(name, circuitType, hName, dataInterface)
        {
            SaveScheduleCommand = new Command(async () => await ExecuteSaveScheduleCommand());
            DeleteScheduleCommand = new Command(async () => await ExecuteDeleteScheduleCommand());
            Expanded = false;
            var startTime = definition.Params.TIME.Split(',');
            var endTime = definition.Params.TIMOUT.Split(',');
            if (startTime.Length > 2 && endTime.Length > 2)
            {
                StartTime = new TimeSpan(
                    int.Parse(startTime[0]),
                    int.Parse(startTime[1]),
                    int.Parse(startTime[2]));
                EndTime = new TimeSpan(
                    int.Parse(endTime[0]),
                    int.Parse(endTime[1]),
                    int.Parse(endTime[2]));
            }

            if (Enum.TryParse<Schedule.TimeType
                    >(definition.Params.STOP,
                        out var eTimeType)) EndTimeType = eTimeType;

            if (Enum.TryParse<Schedule.TimeType
            >(definition.Params.START,
                out var sTimeType)) StartTimeType = sTimeType;

            ScheduledCircuit = scheduledCircuit;
            Active = definition.Params.ACT == "ON";
            Repeats = definition.Params.SINGLE == "OFF";
            Single = definition.Params.SINGLE == "ON";
            if(int.TryParse(definition.Params.LISTORD, out var liOrd)) ListOrd = liOrd;
            Lotmp = definition.Params.LOTMP;
            cooling = definition.Params.COOLING;
            
            var tempDays = definition.Params.DAY;
            
            if (Repeats)
            {
                var temp = new DaysEnum();
                if (tempDays.Contains("M")) temp ^= DaysEnum.Monday;
                if (tempDays.Contains("F")) temp ^= DaysEnum.Friday;
                if (tempDays.Contains("A")) temp ^= DaysEnum.Saturday;
                if (tempDays.Contains("U")) temp ^= DaysEnum.Sunday;
                if (tempDays.Contains("R")) temp ^= DaysEnum.Thursday;
                if (tempDays.Contains("T")) temp ^= DaysEnum.Tuesday;
                if (tempDays.Contains("W")) temp ^= DaysEnum.Wednesday;

                ScheduledDays = temp;
            }
            else
            {
                if (tempDays.Contains("M")) ScheduledDay = DaysEnum.Monday;
                if (tempDays.Contains("F")) ScheduledDay = DaysEnum.Friday;
                if (tempDays.Contains("A")) ScheduledDay = DaysEnum.Saturday;
                if (tempDays.Contains("U")) ScheduledDay = DaysEnum.Sunday;
                if (tempDays.Contains("R")) ScheduledDay = DaysEnum.Thursday;
                if (tempDays.Contains("T")) ScheduledDay = DaysEnum.Tuesday;
                if (tempDays.Contains("W")) ScheduledDay = DaysEnum.Wednesday;
            }
        }

        private void SetDays()
        {
            if (Repeats)
            {
                Days = "";
                if (ScheduledDays.HasFlag(DaysEnum.Monday)) Days += "M";
                if (ScheduledDays.HasFlag(DaysEnum.Tuesday)) Days += "T";
                if (ScheduledDays.HasFlag(DaysEnum.Wednesday)) Days += "W";
                if (ScheduledDays.HasFlag(DaysEnum.Thursday)) Days += "R";
                if (ScheduledDays.HasFlag(DaysEnum.Friday)) Days += "F";
                if (ScheduledDays.HasFlag(DaysEnum.Saturday)) Days += "A";
                if (ScheduledDays.HasFlag(DaysEnum.Sunday)) Days += "U";
            }
            else
            {
                Days = "";
                if (ScheduledDay.HasFlag(DaysEnum.Monday)) Days = "M";
                if (ScheduledDay.HasFlag(DaysEnum.Tuesday)) Days = "T";
                if (ScheduledDay.HasFlag(DaysEnum.Wednesday)) Days = "W";
                if (ScheduledDay.HasFlag(DaysEnum.Thursday)) Days = "R";
                if (ScheduledDay.HasFlag(DaysEnum.Friday)) Days = "F";
                if (ScheduledDay.HasFlag(DaysEnum.Saturday)) Days = "A";
                if (ScheduledDay.HasFlag(DaysEnum.Sunday)) Days = "U";
            }
        }

        private async Task ExecuteDeleteScheduleCommand()
        {
            if (!IsNew)
            {
                var g = Guid.NewGuid();
                var req = new
                {
                    objnam = Hname,
                    Params = new
                    {
                        STATUS = "DSTROY"
                    }
                };

                var reqSer = JsonConvert.SerializeObject(req, Formatting.Indented);

                var message = "{ \"command\": \"SETPARAMLIST\", \"objectList\": [" + reqSer + "], \"messageID\" : \"" +
                              g.ToString() + "\" }";

                DataInterface.SendMessage(message);
                DataInterface.GetScheduleDataAsync();
            }
        }

        private async Task ExecuteSaveScheduleCommand()
        {
            SetDays();
           
            if (ScheduledCircuit == null)
            {
                return;
            }

            var heaterName = "00000";

            if (SelectedHeater != null)
            {
                heaterName = SelectedHeater.Hname;
            }

            var g = Guid.NewGuid();
            if (IsNew)
            {
                var req = new
                {
                    objtyp = "SCHED",
                    Params = new
                            {
                                SNAME = Name,
                                CIRCUIT = ScheduledCircuit.Hname,
                                DAY = Days,
                                SINGLE = Single ? "ON" : "OFF",
                                START = Enum.GetName(typeof(TimeType), StartTimeType),
                                TIME = StartTime.ToString(@"hh\,mm\,ss"),
                                STOP = Enum.GetName(typeof(TimeType), EndTimeType),
                                TIMEOUT = StartTime.ToString(@"hh\,mm\,ss"),
                                GROUP = "",
                                STATUS = "ON",
                                HEATER = heaterName,
                                COOLING = cooling,
                                LOTMP = Lotmp,
                                SMTSRT = "OFF",
                                VACFLO = "OFF",
                                MODE = "0",
                                DNTSTP = "OFF"
                            }
                };
                
                var reqSer = JsonConvert.SerializeObject(req, Formatting.Indented);

                var message = "{ \"command\": \"CREATEOBJECT\", \"objectList\": [" + reqSer + "], \"messageID\" : \"" + g.ToString() + "\" }";
                
                DataInterface.SendMessage(message);
            }
            else
            {
                var req = new
                {
                    objnam = Hname,
                    Params = new
                    {
                        SNAME = Name,
                        CIRCUIT = ScheduledCircuit.Hname,
                        DAY = Days,
                        SINGLE = Single ? "ON" : "OFF",
                        START = Enum.GetName(typeof(TimeType), StartTimeType),
                        TIME = StartTime.ToString(@"hh\,mm\,ss"),
                        STOP = Enum.GetName(typeof(TimeType), EndTimeType),
                        TIMEOUT = StartTime.ToString(@"hh\,mm\,ss"),
                        GROUP = "65535",
                        HEATER = heaterName,
                        COOLING = cooling,
                        LOTMP = Lotmp,
                        SMTSRT = "65535",
                        MODE = "0",
                        DNTSTP = "OFF"
                    }
                };

                var reqSer = JsonConvert.SerializeObject(req, Formatting.Indented);

                var message = "{ \"command\": \"SETPARAMLIST\", \"objectList\": [" + reqSer + "], \"messageID\" : \"" + g.ToString() + "\" }";

                DataInterface.SendMessage(message);
            }
        }

        private static IEnumerable<string> GetDescriptions(Type type)
        {
            var descs = new List<string>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                    descs.Add(fd.Description);
                }
            }
            return descs;
        }
    }
}

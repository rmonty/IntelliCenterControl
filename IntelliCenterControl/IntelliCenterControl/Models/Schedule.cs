using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IntelliCenterControl.Annotations;
using IntelliCenterControl.Services;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    public class Schedule : Circuit<IntelliCenterConnection>
    {
        public const string ScheduleKeys = "[\"OBJNAM : OBJTYP : LISTORD : CIRCUIT : SNAME : DAY : SINGLE : START : TIME : STOP : TIMOUT : GROUP : HEATER : COOLING : LOTMP : SMTSRT : VACFLO : STATUS : DNTSTP : ACT : MODE\"]";

        private DateTime _startTime = new DateTime();

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endTime = new DateTime();

        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }



        public Schedule(string name, CircuitType circuitType, string hName, IDataInterface<IntelliCenterConnection> dataInterface):base(name,circuitType,hName,dataInterface)
        {
           
        }


        
    }

    
}

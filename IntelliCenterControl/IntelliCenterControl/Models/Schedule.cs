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
    public class Schedule : Circuit
    {
        public const string ScheduleKeys = "[\"OBJNAM : OBJTYP : LISTORD : CIRCUIT : SNAME : DAY : SINGLE : START : TIME : STOP : TIMOUT : GROUP : HEATER : COOLING : LOTMP : SMTSRT : VACFLO : STATUS : DNTSTP : ACT : MODE\"]";



        public Schedule(string name, CircuitType circuitType, string hName, IDataInterface<HardwareDefinition> dataInterface):base(name,circuitType,hName,dataInterface)
        {
           
        }


        
    }

    
}

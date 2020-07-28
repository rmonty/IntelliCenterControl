using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliCenterControl.Services;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    public class LightGroup : Circuit<IntelliCenterConnection>
    {
        public Command SyncLightsCommand { get; set; }
        public Command SwimCommand { get; set; }
        public Command ColorSetCommand { get; set; }

        public bool SupportsColor => Lights.Count(l => l.SupportsColor) > 0;
       
        public List<string> ColorNames => EnumHelpers.GetDescriptions(typeof(Light.LightColors)).ToList();

        private Light.LightColors _color = Light.LightColors.WHITER;

        public Light.LightColors Color
        {
            get => _color;
            set
            {
                if (!SupportsColor) return;
                if (_color == value) return;
                _color = value;
                OnPropertyChanged();
                if (SendColorStateUpdate) Task.Run(ExecuteLightColorCommand);
            }
        }

        public List<Light> Lights { get; set;}

        public List<string> LightHNames {get; set;}

        private bool SendColorStateUpdate = true;

        public LightGroup(string name, CircuitType circuitType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, circuitType, hName, dataInterface)
        {
            Lights = new List<Light>();
            LightHNames = new List<string>();
        }

        protected async Task ExecuteLightColorCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "ACT", Color.ToString());
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        public override async Task UpdateItemAsync(JObject data)
        {
            await base.UpdateItemAsync(data);
            if (data.TryGetValue("params", out var lightValues))
            {
                var lv = (JObject)lightValues;

                if (lv.TryGetValue("USE", out var lightColor))
                {
                    if (Enum.TryParse<Light.LightColors>(lightColor.ToString(),
                        out var color))
                    {
                        UpdateColorState(color);
                    }
                }
            }

        }

        public void UpdateColorState(Light.LightColors color)
        {
            SendColorStateUpdate = false;
            Color = color;
            SendColorStateUpdate = true;
        }
        
    }
}

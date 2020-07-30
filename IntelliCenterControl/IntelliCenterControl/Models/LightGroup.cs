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
        public const string LightGroupKeys = "[\"STATUS\",\"MODE\",\"LISTORD\",\"USAGE\",\"FREEZE\",\"LIMIT\",\"USE\",\"MANUAL\",\"FEATR\",\"DNTSTP\",\"CHILD\",\"HNAME\",\"SNAME\",\"RLY\",\"OBJNAM\",\"OBJTYP\",\"SHOMNU\",\"TIME\",\"TIMOUT\",\"SOURCE\",\"SUBTYP\",\"BODY\", \"SYNC\",\"SET\",\"SWIM\"]";

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

        private bool _sync;

        public bool Sync
        {
            get => _sync;
            set
            {
                if(_sync == value) return;
                _sync = value;
                OnPropertyChanged();
            }
        }

        private bool _set;

        public bool Set
        {
            get => _set;
            set
            {
                if(_set == value) return;
                _set = value;
                OnPropertyChanged();
            }
        }

        private bool _swim;

        public bool Swim
        {
            get => _swim;
            set
            {
                if(_swim == value) return;
                _swim = value;
                OnPropertyChanged();
            }
        }

        private bool _isSyncing;

        public bool IsSyncing
        {
            get => _isSyncing;
            set
            {
                if(_isSyncing == value) return;
                _isSyncing = value;
                OnPropertyChanged();
            }
        }


        public List<Light> Lights { get; set;}

        public List<string> LightHNames {get; set;}

        private bool SendColorStateUpdate = true;

        public LightGroup(string name, CircuitType circuitType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, circuitType, hName, dataInterface)
        {
            Lights = new List<Light>();
            LightHNames = new List<string>();
            SyncLightsCommand = new Command(async () => await ExecuteSyncLightsCommand());
            SwimCommand = new Command(async () => await ExecuteSwimCommand());
            ColorSetCommand = new Command(async () => await ExecuteColorSetCommand());

            this.PropertyChanged += LightGroup_PropertyChanged;
        }

        private void LightGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Sync" || e.PropertyName == "Swim" || e.PropertyName == "Set")
            {
                IsSyncing = Sync || Swim || Set;
            }
        }

        private async Task ExecuteColorSetCommand()
        {
           if (DataInterface != null)
           {
               //await DataInterface.UnSubscribeItemUpdate(Hname);
               await DataInterface.SendItemParamsUpdateAsync(Hname, "SET", "ON");
               //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
               await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
           }
        }

        private async Task ExecuteSwimCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "SWIM", "ON");
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        private async Task ExecuteSyncLightsCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "SYNC", "ON");
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        protected async Task ExecuteLightColorCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "ACT", Color.ToString());
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
                await DataInterface.SendItemParamsUpdateAsync(Hname, "STATUS", "ON");
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

                if (lv.TryGetValue("SYNC", out var sync))
                {
                    Sync = sync?.ToString() == "ON";
                }

                if (lv.TryGetValue("SET", out var set))
                {
                    Set = set?.ToString() == "ON";
                }

                if (lv.TryGetValue("SWIM", out var swim))
                {
                    Swim = swim?.ToString() == "ON";
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

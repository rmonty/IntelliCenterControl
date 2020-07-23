using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IntelliCenterControl.Services;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    public class Chem : Circuit<IntelliCenterConnection>
    {
        public const string ChemKeys = "[\"OBJNAM\", \"OBJTYP\", \"SUBTYP\", \"LISTORD\", \"COMUART\", \"COMPSPI\", \"ASSIGN\", \"BODY\", \"SHARE\", \"DEVCONN\", \"SALT\", \"PRIM\", \"SEC\", \"TIMOUT\", \"SUPER\", \"ABSMAX\", \"SNAME\", \"CHLOR\"]";

        public enum ChemType
        {
            [Description("IntelliChem")]
            ICHEM,
            [Description("IntelliChlor")]
            ICHLOR
        }

        public Command ChangePrimaryCommand { get; set; }
        public Command ChangeSecondaryCommand { get; set; }
        public Command ToggleSuperCommand { get; set; }

        private ChemType _type;

        public ChemType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                Description = EnumHelpers.DescriptionAttr<ChemType>(value);
                OnPropertyChanged();
            }
        }

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }


        private string _salt = "-";

        public string Salt
        {
            get => _salt;
            set
            {
                if (_salt == value) return;
                _salt = value;
                OnPropertyChanged();
            }
        }

        private bool _clean;

        public bool Clean
        {
            get => _clean;
            set
            {
                if (_clean == value) return;
                _clean = value;
                OnPropertyChanged();
            }
        }

        private int _primary;

        public int Primary
        {
            get => _primary;
            set
            {
                if (_primary == value) return;
                _primary = value;
                OnPropertyChanged();

                if (SendChangePrimaryCommand) ChangePrimaryCommand.Execute(null);
            }
        }

        private int _secondary;

        public int Secondary
        {
            get => _secondary;
            set
            {
                if (_secondary == value) return;
                _secondary = value;
                OnPropertyChanged();

                if (SendChangeSecondaryCommand) ChangeSecondaryCommand.Execute(null);
            }
        }

        private bool _saltLow;

        public bool SaltLow
        {
            get => _saltLow;
            set
            {
                if (_saltLow == value) return;
                _saltLow = value;
                OnPropertyChanged();
            }
        }

        private bool _saltVeryLow;

        public bool SaltVeryLow
        {
            get => _saltVeryLow;
            set
            {
                if (_saltVeryLow == value) return;
                _saltVeryLow = value;
                OnPropertyChanged();
            }
        }


        private string _primaryName;

        public string PrimaryName
        {
            get => _primaryName;
            set
            {
                if (_primaryName == value) return;
                _primaryName = value;
                OnPropertyChanged();
            }
        }

        private string _secondaryName;

        public string SecondaryName
        {
            get => _secondaryName;
            set
            {
                if (_secondaryName == value) return;
                _secondaryName = value;
                OnPropertyChanged();
            }
        }

        private bool _secondaryAvailable;

        public bool SecondaryAvailable
        {
            get => _secondaryAvailable;
            set
            {
                if (_secondaryAvailable == value) return;
                _secondaryAvailable = value;
                OnPropertyChanged();
            }
        }


        private bool _super;

        public bool Super
        {
            get => _super;
            set
            {
                if (_super == value) return;
                _super = value;
                OnPropertyChanged();

                if (SendToggleSuperCommand) ToggleSuperCommand.Execute(null);
            }
        }

        private bool _volt;

        public bool Volt
        {
            get => _volt;
            set
            {
                if (_volt == value) return;
                _volt = value;
                OnPropertyChanged();
            }
        }

        private Settings _settings = Settings.Instance;

        private bool SendChangePrimaryCommand = true;
        private bool SendChangeSecondaryCommand = true;
        private bool SendToggleSuperCommand = true;

        public Chem(string name, ChemType chemType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, CircuitType.CHEM, hName, dataInterface)
        {
            ChangePrimaryCommand = new Command(async () => await ExecuteChangePrimaryCommand());
            ChangeSecondaryCommand = new Command(async () => await ExecuteChangeSecondaryCommand());
            ToggleSuperCommand = new Command(async () => await ExecuteToggleSuperCommand());
            Type = chemType;
            
        }

        private async Task ExecuteChangeSecondaryCommand()
        {
            if (DataInterface != null)
            {
                if (Secondary < 100 && Secondary > 0)
                    await DataInterface.SendItemParamsUpdateAsync(Hname, "SEC", Secondary.ToString());
            }
        }

        private async Task ExecuteChangePrimaryCommand()
        {
            if (DataInterface != null)
            {
                if (Primary < 100 && Primary > 0)
                    await DataInterface.SendItemParamsUpdateAsync(Hname, "PRIM", Primary.ToString());
            }
        }

        private async Task ExecuteToggleSuperCommand()
        {
            if (DataInterface != null)
            {
                var val = Super ? "ON" : "OFF";
                await DataInterface.SendItemParamsUpdateAsync(Hname, "SUPER", val);
            }
        }

        public void UpdatePrimaryValue(int value)
        {
            SendChangePrimaryCommand = false;
            Primary = value;
            SendChangePrimaryCommand = true;
        }

        public void UpdateSecondaryValue(int value)
        {
            SendChangeSecondaryCommand = false;
            Secondary = value;
            SendChangeSecondaryCommand = true;
        }

        public void UpdateSuperValue(bool value)
        {
            SendToggleSuperCommand = false;
            Super = value;
            SendToggleSuperCommand = true;
        }

        public override async Task UpdateItemAsync(JObject data)
        {
            if (data.TryGetValue("params", out var chemValues))
            {
                var cv = (JObject)chemValues;
                
                if (cv.TryGetValue("SALT", out var salt))
                {
                    Salt = salt.ToString() == "0" ? "-" : salt.ToString();
                }

                if (cv.TryGetValue("BODY", out var bodies))
                {
                    var chemBodies = bodies.ToString().Split(" ").ToList();
                    SecondaryAvailable = false || chemBodies.Count > 1;
                }

                if (cv.TryGetValue("CLEAN", out var clean))
                {
                    Clean = clean.ToString().Contains("ON");
                }

                if (cv.TryGetValue("PRIM", out var prim))
                {
                    if (int.TryParse(prim.ToString(), out var primvalue))
                    {
                        UpdatePrimaryValue(primvalue);
                    }
                }

                if (cv.TryGetValue("SEC", out var sec))
                {
                    if (int.TryParse(sec.ToString(), out var secvalue))
                    {
                        UpdateSecondaryValue(secvalue);
                    }
                }

                if (cv.TryGetValue("SALTLO", out var saltlo))
                {
                    SaltLow = saltlo.ToString().Contains("ON");
                }

                if (cv.TryGetValue("VERYLO", out var saltvlo))
                {
                    SaltVeryLow = saltvlo.ToString().Contains("ON");
                }

                if (cv.TryGetValue("SUPER", out var super))
                {
                    UpdateSuperValue(super.ToString().Contains("ON"));
                }

                if (cv.TryGetValue("VOLT", out var volt))
                {
                    Volt = volt.ToString().Contains("ON");
                }

            }

        }

    }
}

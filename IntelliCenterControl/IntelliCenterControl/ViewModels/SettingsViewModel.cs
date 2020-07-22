using System.Reflection;
using System.Windows.Input;
using IntelliCenterControl.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IntelliCenterControl.ViewModels
{
    public class SettingsViewModel : BaseViewModel<object>
    {
        public Settings Settings => Settings.Instance;

        


        public SettingsViewModel()
        {
            Title = "Settings";
            
        }
    }
}
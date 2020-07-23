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
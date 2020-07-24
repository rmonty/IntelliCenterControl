using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChemPage : ContentPage
    {
        public ChemPage()
        {
            InitializeComponent();

            BindingContext = SimpleIoc.Default.GetInstance<ControllerViewModel>();

        }

    }
}
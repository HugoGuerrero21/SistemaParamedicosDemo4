using SistemaParamedicosDemo4.MVVM.Views;

namespace SistemaParamedicosDemo4
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new LoginView();
        }
    }
}
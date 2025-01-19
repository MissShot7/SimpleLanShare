using Microsoft.Maui.LifecycleEvents;
namespace CrossPlatformShare
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 400;
            const int newHeight = 400;

            window.Width = newWidth;
            window.Height = newHeight;
            window.Title = "SimpleLanShare";
            window.Destroying += (s, ev) => { ServerClass.StopServer();};

            return window;
        }

    }
}


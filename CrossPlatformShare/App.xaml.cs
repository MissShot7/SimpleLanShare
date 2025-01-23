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
            const int newHeight = 430;

            window.Width = newWidth;
            window.Height = newHeight;
            window.MinimumHeight = newHeight;
            window.MinimumWidth = newWidth;
            window.Title = "SimpleLanShare";
            window.Destroying += (s, ev) => 
            { 
                ServerClass.StopServer();
                Environment.Exit(Environment.ExitCode);

            };

            return window;
        }

    }
}


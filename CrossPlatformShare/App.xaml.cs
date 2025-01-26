using Microsoft.Maui.LifecycleEvents;
using SimpleLANShare;
namespace CrossPlatformShare
{
    public partial class App : Application
    {
        public App()
        {
            
            InitializeComponent();

            //MainPage = new MainPage();
<<<<<<< HEAD
<<<<<<< HEAD
            MainPage = new AppShell();
=======
=======
>>>>>>> 38f80b702583d17db3d2cdced18bb80c73617e91
            MainPage = new MainPage();
>>>>>>> 38f80b702583d17db3d2cdced18bb80c73617e91

        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 400;
            const int newHeight = 435;

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


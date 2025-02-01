using Microsoft.Maui.LifecycleEvents;
using SimpleLANShare;
namespace CrossPlatformShare
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            CheckAndRequestPermissionAsync();
            App.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            //MainPage = new MainPage();
            MainPage = new AppShell();

        }
        public async Task CheckAndRequestPermissionAsync()
        {
            // Check if storage read permission is granted
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            // Check if permission is not granted
            if (status != PermissionStatus.Granted)
            {
                // Request permission if not granted
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            if (status == PermissionStatus.Granted)
            {
                // Proceed with reading the file or other tasks that require storage access
                Console.WriteLine("Permission granted.");
            }
            else
            {
                // Handle the case where permission is denied
                Console.WriteLine("Permission denied.");
            }
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 400;
            const int newHeight = 485;

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
        private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
#if ANDROID //android doesn't automatically change AppTheme
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Force a UI update
                Application.Current.MainPage = new AppShell();
            });
#endif
        }

    }
}


namespace CrossPlatformShare.Pages;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
        //get version number
        VersionLabel.Text = $"Version: {AppInfo.Current.VersionString}";

    }
    private async void OnGitHubButtonClicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://github.com/MissShot7/SimpleLANShare");
    }

    private async void OnWebsiteButtonClicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://your-website.com");
    }
}
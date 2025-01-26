namespace CrossPlatformShare.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
        //načte uložené hodnoty
        PortEntry.Text = ServerClass.GetSavedPort().ToString();
        TimeoutEntry.Text = Preferences.Get("SavedTimeout", "5");
    }
    private void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        if (ServerClass.running) { DisplayAlert("Server running", "Stop the server first", "OK"); return; }
        string port = PortEntry.Text;
        string timeout = TimeoutEntry.Text;

        // Validate and save settings
        if (int.TryParse(port, out int portValue) && int.TryParse(timeout, out int timeoutValue))
        {
            // Save settings logic
            //DisplayAlert("Settings Saved", "Your settings have been saved successfully.", "OK");
            ServerClass.SetPort(portValue); //port
            Preferences.Set("SavedTimeout", 5);
        }
        else
        {
            DisplayAlert("Invalid Input", "Please enter valid numeric values.", "OK");
        }
    }
}
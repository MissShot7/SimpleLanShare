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
            //check if timeout is valid
            if (timeoutValue > 120 || timeoutValue < 0) { DisplayAlert("Invalid timeout value", "Timeout has to be greater than 0 and smaller than 120 sec", "OK"); return; }
            //check if port is valid
            if (portValue < 1025 || portValue > 65535) { DisplayAlert("Invalid port", "Port has to be in between 1025 and 65535", "OK"); return; }

            ServerClass.SetPort(portValue); //port
            Preferences.Set("SavedTimeout", 5);
            //DisplayAlert("Settings Saved", "Your settings have been saved successfully.", "OK");
        }
        else
        {
            DisplayAlert("Invalid Input", "Please enter valid numeric values.", "OK");
        }
    }

}
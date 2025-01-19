using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using System.IO;
using System.Threading;
using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace CrossPlatformShare
{

    public partial class MainPage : ContentPage
    {

        private List<string> ipAddresses = new List<string>();

        public void NCL(string txt) //new console log
        {
            //if (ConsoleEntry == null) { return; }
            //MainPage mp = new MainPage();
            //ConsoleEntry = mp.serverConsole;
            ServerConsole.Text += txt + "\n";
        }

        public MainPage()
        {
            //Android oprávnění


            InitializeComponent();
            BindingContext = new MainViewModel();
            
            //ServerClass.StartFileServer(); //"http://localhost:8080/"

            // Get saved IP addresses from Preferences
            var savedJson = Preferences.Get("IPlist", "[]");
            List<string> savedList = JsonConvert.DeserializeObject<List<string>>(savedJson);
            ipAddresses = ipPicker.Items.ToList();
            // Add saved IPs to the ipAddresses list
            foreach (string ip in savedList)
            {
                if (!ipAddresses.Contains(ip))
                {
                    ipAddresses.Add(ip);
                }
            }
            // Set the Picker's ItemsSource to the list
            ipPicker.ItemsSource = ipAddresses;
            //napíše poslední soubor do FileUploadEntry
            FileUploadEntry.Text = Preferences.Get("LastFileUploaded", "");
            //SavePicker poslední hodnota
            SavePicker.SelectedItem = Preferences.Get("SavePickerLast", "SemiAuto");
        }

        /*
        void OnSliderValueChanged(object sender, ValueChangedEventArgs args) //slider
        {
            valueLabel.Text = args.NewValue.ToString("F3");
        }*/

        async void HappyClicked(object sender, EventArgs args) //stisk tlačítka
        {
            await DisplayAlert("Credits", "Created by MissShot7 (https://github.com/MissShot7/) in 2025", "Continue");
        }
        void StartLanSharing(object sender, EventArgs args)
        {
            //rozhodne akci
            Button button = (Button)sender;
            if (button.Text == "Host")
            {
                if (File.Exists(FileUploadEntry.Text))
                {
                    bool successfull = ServerClass.StartFileServer(FileUploadEntry.Text); //spustí server
                    if (successfull)
                    {
                        Preferences.Set("LastFileUploaded", FileUploadEntry.Text); //uloží poslední cestu
                        button.Text = "Stop";
                    }
                }
            } else if (button.Text == "Stop")
            {
                //ukončí server
                button.Text = "Host";
                ServerClass.StopServer();
            }
        }

        async void IPpickerChanged(object sender, EventArgs args) //ippicker
        {
            Picker p = (Picker)sender;

            if (p.SelectedItem != null && p.SelectedItem.ToString() == "custom")
            {
                string result = await DisplayPromptAsync("Enter IP", "Enter LAN IPV4 adress", keyboard: Keyboard.Numeric, maxLength: 15); //vlastní ip
                if (result == "") { return; }
                bool DontAdd = false;
                foreach (string item in p.GetItemsAsArray())
                {
                    if (item == result) { DontAdd = true; break; }
                }
                if (DontAdd == false) //přidá do preferences
                {
                    //p.Items.Add(result); //přidá do itemu
                    ipAddresses.Add(result);
                    ipPicker.ItemsSource = new List<string>();
                    ipPicker.ItemsSource = ipAddresses;
                    

                    List<string> list = p.Items.ToList();
                    list.Add(result);
                    string json = JsonConvert.SerializeObject(list);
                    Preferences.Set("IPlist", json);
                }
                p.SelectedItem = result;
            }
        }
        
        async void DownloadFromIP(object sender, EventArgs args)
        {
            if (ipPicker.SelectedItem == null) { await DisplayAlert("Null", "You need to select IP", "OK"); return; }
            if (SavePicker.SelectedItem == null) { await DisplayAlert("Null", "You need to select Mode", "OK"); return; }

            string dip = ipPicker.SelectedItem.ToString();
            string port = Preferences.Get("SavedPort", "8008");
            string adress = "http://" + dip + ":" + port;

            using (var client = new WebClient())
            {
                string FileName = client.DownloadString(adress + "/GetFileName");
                
                
                string FullPath_auto = Path.Combine(GetDownlaodDir(), FileName);
                string FullPath_custom = FullPath_auto;
                bool UserSelectsFile = false;
                //Manual
                if (SavePicker.SelectedItem.ToString() == "Manual") { UserSelectsFile = true; }

                else if (SavePicker.SelectedItem.ToString() == "FullAuto")
                {
                    FullPath_custom = NextAvailableFilename(FullPath_custom);
                } else if (SavePicker.SelectedItem.ToString() == "SemiAuto" && File.Exists(FullPath_auto))  
                {
                    string action = await DisplayActionSheet("File already Exists", "Cancel", null, "Overwrite", "Custom name", "Auto name");
                    NCL("Action: " + action);
                    //akce
                    if (action == "cancel") { NCL("Canceled by user"); return; }
                    else if (action== "Overwrite") {} //pokračuje
                    else if (action=="Auto name") { FullPath_custom = NextAvailableFilename(FullPath_auto); }

                    else if (action=="Custom name") 
                    {
                        UserSelectsFile = true;
                    }
                }
                NCL("FileName: " + FileName);
                if (UserSelectsFile) //vybírání
                {
                    var fileSaverResult = await FileSaver.Default.SaveAsync(FullPath_auto, new MemoryStream(client.DownloadData(adress + "/dsf"))); //uživatel sám vybere
                    NCL("Downloading to " + fileSaverResult);
                } else 
                { 
                    client.DownloadFile(adress + "/dsf", FullPath_custom); //do stažených souborů
                    NCL("Downloading to " + FullPath_custom);
                }
                
                
            }
            NCL("Downloading done");

            string NextAvailableFilename(string path)
            {
                // Short-cut if already available
                if (!File.Exists(path))
                    return path;
                // If path has extension then insert the number pattern just before the extension and return next filename
                if (Path.HasExtension(path))
                    return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), " ({0})"));
                // Otherwise just append the pattern to the path and return next filename
                return GetNextFilename(path + " ({0})");

                string GetNextFilename(string pattern)
                {
                    string tmp = string.Format(pattern, 1);
                    if (tmp == pattern)
                        throw new ArgumentException("The pattern must include an index place-holder", "pattern");
                    if (!File.Exists(tmp))
                        return tmp; // short-circuit if no matches
                    int min = 1, max = 2; // min is inclusive, max is exclusive/untested
                    while (File.Exists(string.Format(pattern, max)))
                    {
                        min = max;
                        max *= 2;
                    }
                    while (max != min + 1)
                    {
                        int pivot = (max + min) / 2;
                        if (File.Exists(string.Format(pattern, pivot)))
                            min = pivot;
                        else
                            max = pivot;
                    }
                    return string.Format(pattern, max);
                }
            }
        }
        void SavePickerChanged(object sender, EventArgs args) //savepicker změněn
        {
            Picker p = (Picker)sender;
            if (p == null) return;
            Preferences.Set("SavePickerLast", p.SelectedItem.ToString());
        }
        void DeleteIPhistory(object sender, EventArgs args)
        {
            Preferences.Set("IPlist", "[]");
            ipAddresses.Clear();
            ipAddresses.Add("custom");
            ipPicker.ItemsSource = new List<string>();
            ipPicker.ItemsSource = ipAddresses;
        }
        async void UploadFileDialog(object sender, EventArgs args)
        {
            //ServerClass.ConsoleEntry = ServerConsole;
            try
            {
                var result = await FilePicker.Default.PickAsync(PickOptions.Default);
                if (result == null) { return; }
                FileUploadEntry.Text = result.FullPath;

            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }
        }
        private static string GetDownlaodDir()
        {
            string downloadsPath="null";
#if WINDOWS
        downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif ANDROID
        downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
#endif
            return downloadsPath ;
        }
        private static string GetLocalIPAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        // IPv4 address and not a loopback address
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "127.0.0.1"; // Fallback to localhost if no LAN IP found
        }
    }
    
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _serverConsoleText = "Welcome to the server console!\n";
        private bool didOnce = false;
        public string ServerConsoleText
        {
            get => _serverConsoleText;
            set
            {
                if (didOnce == false) { ServerClass.mvm = this; didOnce = true; } 
                
                if (_serverConsoleText != value)
                {
                    _serverConsoleText = value;
                    OnPropertyChanged();
                }
            }
        }

        // Command to clear the console
        public ICommand ClearConsoleCommand { get; }

        public MainViewModel()
        {
            ClearConsoleCommand = new Command(ClearConsole);
        }
        public void LogToConsole(string message)
        {
            ServerConsoleText += $"{DateTime.Now}: {message}\n";
        }

        private void ClearConsole()
        {
            ServerConsoleText = string.Empty; // Clear the content of the Editor
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

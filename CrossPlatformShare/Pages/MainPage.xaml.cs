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
using Newtonsoft.Json.Linq;
using CommunityToolkit.Maui.Alerts;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Microsoft.Maui.Controls;
namespace CrossPlatformShare.Pages;


public partial class MainPage : ContentPage
{

    private List<string> ipAddresses = new List<string>();
    private static readonly HttpClient client = new HttpClient()
    {
        //Timeout = TimeSpan.FromSeconds(5) // Set the timeout here
        Timeout = TimeSpan.FromSeconds(int.Parse(Preferences.Get("SavedTimeout", "5"))) // Set the timeout here
        
    };

    public void NCL(string txt) //new console log
    {
        //if (ConsoleEntry == null) { return; }
        //MainPage mp = new MainPage();
        //ConsoleEntry = mp.serverConsole;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ServerConsole.Text += txt + "\n";
        });
    }
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel();

        //načte poslední IP
        var savedJson = Preferences.Get("IPlist", "[]");
        List<string> savedList = JsonConvert.DeserializeObject<List<string>>(savedJson);
        ipAddresses = ipPicker.Items.ToList();
        // Add saved IPs to the ipAddresses list
        foreach (string ip in savedList)
        {
            if (!ipAddresses.Contains(ip) && IsValidIPv4(ip))
            {
                ipAddresses.Add(ip);
            }
        }
        // Set the Picker's ItemsSource to the list
        ipPicker.ItemsSource = ipAddresses;
        //Nastaví poslední vybraný soubor
        ipPicker.SelectedItem = Preferences.Get("LastIPSelected", "");
        //SavePicker poslední hodnota
        SavePicker.SelectedItem = Preferences.Get("SavePickerLast", "SemiAuto");
        //napíše poslední soubor do FileUploadEntry
        FileUploadEntry.Text = MiscClass.ProcessSharedObject();
    }
    


    async void StartLanSharing(object sender, EventArgs args)
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
                    BrowseBtn.IsEnabled = false;
                    FileUploadEntry.IsEnabled = false;
                }
            }
            else
            {
                await DisplayAlert("Non Existent", "Path Doesn't exist", "OK");
            }
        }
        else if (button.Text == "Stop")
        {
            //ukončí server
            button.Text = "Host";
            BrowseBtn.IsEnabled = true;
            FileUploadEntry.IsEnabled = true;
            ServerClass.StopServer();
        }
    }

    async void IPpickerChanged(object sender, EventArgs args) //ippicker
    {
        Picker p = (Picker)sender;
        if (p.SelectedIndex != -1)
        {
            if (p.SelectedItem.ToString() == "custom")
            {
                string result = await DisplayPromptAsync("Enter IP", "Enter LAN IPV4 adress", maxLength: 15); //vlastní ip
                if (!IsValidIPv4(result)) { NCL($"Invalid IP: {result}"); p.SelectedItem = null; return; }
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
            //uloží vybranou možnost
            Preferences.Set("LastIPSelected", p.SelectedItem.ToString() ?? "custom");
        }
    }
    bool IsValidIPv4(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        string[] parts = input.Split('.');
        if (parts.Length != 4)
            return false;
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                return false;
        }
        return true;
    }
    async Task<bool> IsServerOnlineAsync(string url)
    {
        NCL($"Attempting to connect to {url}");
        try
        {
            // Send a GET request to the URL
            HttpResponseMessage response = await client.GetAsync(url);
            NCL($"{url} is online");

            // If the status code is 200 (OK), the server is online
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException e) // Catches timeouts
        {
            NCL($"Request to {url} timed out.");
            NCL(e.Message);
            return false;
        }
        catch (HttpRequestException e) // Catches other HTTP-related errors
        {
            NCL($"Can't connect to: {url}");
            NCL(e.Message);
            return false;
        }
    }

    async void DownloadFromIP(object sender, EventArgs args)
    {
        if (ipPicker.SelectedItem == null) { await DisplayAlert("Null", "You need to select IP", "OK"); return; }
        if (SavePicker.SelectedItem == null) { await DisplayAlert("Null", "You need to select Mode", "OK"); return; }
        DownloadProgress.Progress = 0;

        string dip = ipPicker.SelectedItem.ToString();
        int port = ServerClass.GetSavedPort();
        string address = "http://" + dip + ":" + port;

        //jestli je soubor online
        if (!await IsServerOnlineAsync(address)) { return; }

        //zjistí informace ze serveru
        string FileInfoJSON = await client.GetStringAsync(address + "/GetFileInfo");
        var fileInfo = JObject.Parse(FileInfoJSON);
        string FileName = fileInfo["Name"].ToString();
        long FileSize = (long)fileInfo["SizeInBytes"];


        string FullPath_auto = Path.Combine(GetDownlaodDir(), FileName);
        string FullPath_custom = FullPath_auto;
        bool UserSelectsFile = false;
        //Manual
        if (SavePicker.SelectedItem.ToString() == "Manual") { UserSelectsFile = true; }

        else if (SavePicker.SelectedItem.ToString() == "FullAuto")
        {
            FullPath_custom = NextAvailableFilename(FullPath_custom);
        }
        else if (SavePicker.SelectedItem.ToString() == "SemiAuto" && File.Exists(FullPath_auto))
        {
            string action = await DisplayActionSheet("File already Exists", "Cancel", null, "Overwrite", "Custom name", "Auto name");
            NCL("Action: " + action);
            //akce
            if (action == "cancel") { NCL("Canceled by user"); return; }
            else if (action == "Overwrite") { } //pokračuje
            else if (action == "Auto name") { FullPath_custom = NextAvailableFilename(FullPath_auto); }

            else if (action == "Custom name")
            {
                UserSelectsFile = true;
            }
        }
        NCL("FileName: " + FileName);

        //vypne tlačítko
        ReciveBtn.IsEnabled = false;
        ReciveBtn.BackgroundColor = Colors.DarkBlue;
        ReciveBtn.Text = "Recieving";
        //začne stahovací vlákno
        Thread downloadThread = new Thread(dwnd_task);
        downloadThread.Start();

        async void dwnd_task()
        {
            string ConsoleDisplayPath = null;

            var downloadFileUrl = address + "/dsf";
            string savePath = UserSelectsFile ? Path.GetTempFileName() : FullPath_custom; // Determine file path based on user selection

            // Assuming FileSize is known beforehand or obtained from headers
            long totalBytesDownloaded = 0;

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(downloadFileUrl, HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];
                int bytesRead;
                double progress = 0;

                Stopwatch sw = new Stopwatch();
                sw.Start();

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // Write to file immediately
                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                    // Update total bytes downloaded
                    totalBytesDownloaded += bytesRead;

                    // Update progress every 50ms
                    if (sw.ElapsedMilliseconds >= 50)
                    {
                        sw.Restart();
                        progress = (double)totalBytesDownloaded / FileSize;

                        // Optional: Update the UI on the main thread
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DownloadProgress.Progress = progress;
                        });
                    }
                }

                // Final progress update to ensure 100% is displayed
                progress = (double)totalBytesDownloaded / FileSize;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DownloadProgress.Progress = progress;
                });
            }
            bool wasUnsuccessfull = true;
            if (UserSelectsFile)
            {
                FileSaverResult fileSaverResult;
                try
                {
                    // User selects file, save to custom location
                    fileSaverResult = await FileSaver.Default.SaveAsync(FullPath_auto, new MemoryStream(File.ReadAllBytes(savePath))); // uživatel sám vybere
                    ConsoleDisplayPath = fileSaverResult.ToString();
                    if (fileSaverResult.IsSuccessful) { wasUnsuccessfull = false; }

                }
                catch (OperationCanceledException)
                {
                    // Handle the case when the user cancels the file save
                    //ConsoleDisplayPath = "File save was cancelled.";
                    NCL("User cancelled the save operation.");
                }
                catch (Exception ex)
                {
                    // Catch other exceptions (e.g., file access issues, permissions)
                    //ConsoleDisplayPath = "An error occurred while saving the file.";
                    NCL($"Error: {ex.Message}");
                }
                finally
                {
                    // Clean up temporary file, even if an error occurred
                    File.Delete(savePath);
                }
            }
            else
            {
                // Direct download without user intervention
                ConsoleDisplayPath = FullPath_custom;
                wasUnsuccessfull = false;
            }
            //konec (musí být spuštěn na hlavním vlákně)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (wasUnsuccessfull)
                {
                    NCL("Saving Unsuccessfull");
                    DownloadProgress.Progress = 0f;
                }
                else
                {
                    NCL("File saved to " + ConsoleDisplayPath);
                    DownloadProgress.Progress = 1f;
                }
                //tlačítko
                ReciveBtn.IsEnabled = true;
                ReciveBtn.BackgroundColor = Colors.DodgerBlue;

                ReciveBtn.Text = "Recieve";
            });
        }


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
    void ClearConsole(object sender, EventArgs args)
    {
        ServerConsole.Text = string.Empty;
    }
    private static string GetDownlaodDir()
    {
        string downloadsPath = "null";
#if WINDOWS
    downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif ANDROID
    downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
#endif
        return downloadsPath;
    }
}

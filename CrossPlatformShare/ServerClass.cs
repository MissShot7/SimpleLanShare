//using AndroidX.Navigation;
using CrossPlatformShare;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.FileProviders;

class ServerClass
{
    private static string directoryPath = Path.Combine(GetAppFolderPath(), "uploadedfile");
    private static string SpecificFilePath;
    public static Editor ConsoleEntry;
    public static MainViewModel mvm;
    public static bool running = false;
    private static string serverIP;
    //public 
    private static TcpListener tcpListener;
    private static int DefaultPort = 8107;
    public static void NCL(string txt) //new console log
    {
        //if (ConsoleEntry == null) { return; }

        mvm.ServerConsoleText += txt + "\n";
    }
    public static int GetSavedPort()
    {
        bool SuccessfullyParsed = int.TryParse(Preferences.Get("SavedPort", DefaultPort.ToString()), out int ParsedPort);
        if (SuccessfullyParsed) { return ParsedPort; }
        else { return DefaultPort; }
    }
    public static void SetPort(int NewPort)
    {
        Preferences.Set("SavedPort", NewPort);
    }
    public static bool StartFileServer(string specificFilePath = "none")
    {
        serverIP = GetLocalIPAddress(); //lokální ip
        if (serverIP == "Connecting_error") { NCL("Connection error, ensure your wifi is on"); return false; } //chyba s připojením

        if (running == true) { return false; }
        //jestli soubor existuje (a není intent)
        if (!ShareIntentHelper.IntentEnabled && !File.Exists(specificFilePath)) { NCL($"File '{specificFilePath}' doesn't exist. Not starting"); return false; }

        //start
        running = true;
        if (!ShareIntentHelper.IntentEnabled) { SpecificFilePath = specificFilePath; }
        else {  SpecificFilePath = "intent"; }
        
        Thread serverThread = new Thread(RunServer);
        serverThread.Start();
        return true;
    }
    public static string GetLocalIPAddress()
    {
        //jestli je připojen k internetu
        

        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            try
            {
                socket.Connect("8.8.8.8", 65530);
            }
            catch { return "Connecting_error"; }
            
            
            
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }
        return localIP;
    }

    public static void StopServer()
    {
        running = false;
        tcpListener?.Stop();
        NCL("Server Stopped");
    }
    private static void RunServer()
    {
        //string serverIP = "192.168.1.207";

        
        //string serverIP = "169.254.173.202";
        
        /*try*/
        {
            int port = GetSavedPort();
            tcpListener = new TcpListener(IPAddress.Parse(serverIP), port);
            tcpListener.Start();

            NCL($"Server started on {serverIP}:{port}");
            

            while (running)
            {

                if (tcpListener.Pending())
                {
                    // Accept a new connection
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleRequest(client));
                    clientThread.Start();
                }
                else
                {
                    Thread.Sleep(100); // Prevent CPU spinning
                    //mvm.ServerConsoleText += "f";

                }
            }
        }
        /*catch (Exception ex)
        {
            NCL($"Error starting server: {ex.Message}");
        }*/
    }

    private static void HandleRequest(TcpClient client)
    {
 
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new StreamReader(stream))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Read the request from the client
            string request = reader.ReadLine();
            if (request == null)
            {
                return;
            }
            IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            string ClientAdress = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
            NCL($"Request: {request} from {ClientAdress}");

            // Parse the request for the file path
            string[] requestParts = request.Split(' ');
            if (requestParts.Length < 2)
            {
                return;
            }

            string fileRequested = requestParts[1].TrimStart('/');
            if (fileRequested.EndsWith("/")) { fileRequested.Substring(0, fileRequested.Length - 1); } //vymaže lomítko


            //--------------------------------------
            string responseHeader;
            string responseBody;

            if (fileRequested == "dsf_uri") //download specific file
            {
#if ANDROID
                try{
                
                //NCL(ShareIntentHelper.uribytes.Length);
                 writer.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                writer.Flush();
                stream.Write(ShareIntentHelper.uribytes, 0, ShareIntentHelper.uribytes.Length);
                } catch(Exception e){NCL(e.Message);
                }
#endif
                return;

                /*
                Android.Net.Uri uri = ShareIntentHelper.uri;
                responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                writer.Write(responseHeader);
                writer.Flush();
                byte[] bytes = ReadBytesFromUri(uri);
                stream.Write(bytes, 0, bytes.Length);

                return;*/
            }
            if (fileRequested == "dsf") //download specific file
            {
                byte[] fileBytes = null;
                if (!ShareIntentHelper.IntentEnabled) { fileBytes = File.ReadAllBytes(SpecificFilePath); } //normal
                else { fileBytes = ShareIntentHelper.uribytes; } //intent

                string fname = Path.GetFileName(ShareIntentHelper.IntentEnabled ? ShareIntentHelper.finfo.FullName : SpecificFilePath);
                responseHeader = "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: text/plain\r\n" + // You can adjust the content type if needed (e.g., application/octet-stream for binary files)
                            "Content-Disposition: attachment; filename=\"" + fname + "\"\r\n" + // specify file name
                            "\r\n";
                writer.Write(responseHeader);
                writer.Flush();
                stream.Write(fileBytes, 0, fileBytes.Length);

                return;
            }
            else if (fileRequested == "")
            {
                // Generate html
                responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n";
                string LocalFilePath;
                if (!ShareIntentHelper.IntentEnabled) { LocalFilePath = SpecificFilePath; } //normal
                else { LocalFilePath = ShareIntentHelper.finfo.FullName; } //intent
                responseBody = SimpleHTML("Server working :)", $"Current file on host device is stored on: {LocalFilePath} </br></br>" + 
                    $"""<b>Available URLs:</b></br> Download link:  <a href="/dsf">{serverIP}/dsf</a> """+
                    $"""</br> FileInfo link:  <a href="/GetFileInfo">{serverIP}/GetFileInfo</a> """);
            } else if (fileRequested == "GetFileInfo") //informace o souboru
            {

                FileDetails fileDetails;
                if (!ShareIntentHelper.IntentEnabled) //normal
                {
                    FileInfo fileInfo = new FileInfo(SpecificFilePath);
                    fileDetails = FileDetails.DetailsFromInfo(fileInfo);
                }
                else //intent
                {
                    fileDetails = ShareIntentHelper.finfo;
                }


                string FIleInfoAsJSON = JsonConvert.SerializeObject(fileDetails, Formatting.Indented);

                responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                responseBody = FIleInfoAsJSON ?? "Error";
            }
            else //chyba
            {
                responseHeader = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/html\r\n\r\n";
                responseBody = SimpleHTML("Bad Request", "Doesn't exist");
            }
            writer.Write(responseHeader);
            writer.Write(responseBody);
            writer.Flush();




            /*
            // Check if the request is for /downloadfile
            if (fileRequested == "downloadfile" || fileRequested == "downloadfile/")
            {
                // Generate directory index
                string responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n";
                string responseBody = GenerateDirectoryIndex();
                writer.Write(responseHeader);
                writer.Write(responseBody);
                writer.Flush();
                return;
            }
            // Check if the request starts with /downloadfile/
            if (!fileRequested.StartsWith("downloadfile/"))
            {
                // If it doesn't, return an error
                string responseHeader = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/html\r\n\r\n";
                //string responseBody = "<html><body><h1>Bad Request</h1><p>The URL must start with /downloadfile/.</p></body></html>";
                string responseBody = SimpleHTML("Bad Request", "The URL must start with /downloadfile/. (absolete)");
                writer.Write(responseHeader);
                writer.Write(responseBody);
                writer.Flush();
                return;
            }

            // Remove "downloadfile/" prefix
            string fileName = fileRequested.Substring("downloadfile/".Length);
            string filePath = Path.Combine(directoryPath, fileName);

            // Serve the requested file
            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string responseHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                writer.Write(responseHeader);
                writer.Flush();
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
            else
            {
                string responseHeader = "HTTP/1.1 404 Not Found\r\nContent-Type: text/html\r\n\r\n";
                //string responseBody = $"<html><body><h1>File Not Found</h1><p>The file {fileName} could not be found.</p></body></html>";
                string responseBody = SimpleHTML("File Not Found", $"The file {fileName} could not be found.");
                writer.Write(responseHeader);
                writer.Write(responseBody);
                writer.Flush();
            }*/
        }

        client.Close();
    }
    private static string SimpleHTML(string h1, string p, string title = "server")
    {
        string credits = $"""
</br></br></br></br>
<p style="text-align: left; font-family: Arial, sans-serif; font-size: 1.2em; line-height: 1.6; color: #333;">
    <strong>SimpleLANShare</strong><br>
    Download your copy of SimpleLANShare 
    <a href="https://github.com/MissShot7/SimpleLANShare" style="color: #0066cc; text-decoration: none;">here</a><br>
    <span style="font-style: italic;">created by MissShot7</span>
</p>
""";
        string responseBody = $"<html><body><h1>{h1}</h1><p>{p}</p>{credits}</body><title>{title}</title></html>";
        return responseBody;
    }


    private static string GenerateDirectoryIndex()
    {
        StringBuilder htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<html><body>");
        htmlBuilder.Append("<h1>Index of /downloadfile</h1>");
        htmlBuilder.Append("<ul>");

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                htmlBuilder.Append("<li>No files available</li>");
            }
            else
            {
                // Get all files in the directory
                string[] files = Directory.GetFiles(directoryPath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    htmlBuilder.AppendFormat("<li><a href=\"/downloadfile/{0}\">{0}</a></li>", fileName);
                }
            }
        }
        catch (Exception ex)
        {
            htmlBuilder.Append($"<p>Error generating file list: {ex.Message}</p>");
        }

        htmlBuilder.Append("</ul>");
        htmlBuilder.Append("</body></html>");

        return htmlBuilder.ToString();
    }

    private static string GetAppFolderPath()
    {
#if WINDOWS
        return AppDomain.CurrentDomain.BaseDirectory;
#elif ANDROID
        return Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
#else
        throw new PlatformNotSupportedException("Unsupported platform");
#endif
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


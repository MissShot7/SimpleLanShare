using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
#if ANDROID
using Android.Content;
using Android.Database;
#endif
public static class ShareIntentHelper
{
    private static string DefaultValue = "none";

    public static string SharedIntentFileName = DefaultValue;
    public static FileDetails finfo;

    public static string SharedText = DefaultValue;
    public static bool IntentEnabled = false;
#if ANDROID
    public static Android.Net.Uri uri;
    public static Intent intent;
#endif

    public static byte[] uribytes;

    public static string ProcessSharedObject()
    {
        string path;
        if (SharedIntentFileName != DefaultValue) //file
        {
            path = SharedIntentFileName;
            //ServerClass.NCL(path);
        } else if (SharedText != DefaultValue) //text
        {
            //make file with text
            string filepath = Path.Combine(System.IO.Path.GetTempPath(), "Text.txt");
            File.WriteAllText(filepath, SharedText);
            path = filepath;
        } else //return saved value
        {
            if (Preferences.Get("LastFileUploaded", "") == "none") { path = ""; }
            else { path = Preferences.Get("LastFileUploaded", ""); }
            
        }
        /*
        //set filename and text to default values
        SharedIntentFileName = DefaultValue;
        SharedText = DefaultValue;*/
        return path;
    }
    public static void ClearIntentData()
    {
        IntentEnabled = false;
        SharedIntentFileName = DefaultValue;
        SharedText = DefaultValue;
        finfo = null;
        uribytes = null;
        #if ANDROID
             uri = null;
            intent = null;
        #endif
    }

    public static byte[] ReadBytesFromUri()
    {
#if ANDROID
        Context context = Android.App.Application.Context; // Get global context

        using (var stream = context.ContentResolver.OpenInputStream(uri))
        {
            if (stream == null)
                throw new IOException("Failed to open stream from URI.");

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
#endif
        return null;
    }

}
public class FileDetails
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Extension { get; set; }
    public long SizeInBytes { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }

    public static FileDetails DetailsFromInfo(FileInfo fileInfo)
    {
        FileDetails fd = new FileDetails
        {
            Name = fileInfo.Name,
            FullName = fileInfo.FullName,
            Extension = fileInfo.Extension,
            SizeInBytes = fileInfo.Length,
            CreationTime = fileInfo.CreationTime,
            LastAccessTime = fileInfo.LastAccessTime,
            LastWriteTime = fileInfo.LastWriteTime
        };
        return fd;
    }
}
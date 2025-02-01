using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
#if ANDROID
using Android.Content;
#endif
public static class MiscClass
{
    private static string DefaultValue = "none";
    public static string SharedIntentFileName = DefaultValue;
#if ANDROID
    public static Android.Net.Uri uri;
#endif
    public static string SharedText = "DefaultValue";

    public static string ProcessSharedObject()
    {
        string path;
        if (SharedIntentFileName != "none") //file
        {
            path = SharedIntentFileName;
            ServerClass.NCL(path);
        } else if (SharedText != "none") //text
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

        //set filename and text t odefault values
        SharedIntentFileName = DefaultValue;
        SharedText = DefaultValue;
        return path;
    }
#if ANDROID
    public static byte[] ReadBytesFromUri(Android.Net.Uri uri)
    {
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
    }
#endif
}
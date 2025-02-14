using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using CrossPlatformShare;
using Android.Database;
using CrossPlatformShare.Pages;
using CommunityToolkit.Mvvm.Messaging;

namespace com.MissShot7.SimpleLanShare // Ensure this matches your app's package name
{
    [Activity(Label = "ShareActivity", Exported = true/*, LaunchMode = LaunchMode.SingleTask*/)]
    [IntentFilter(new[] { Intent.ActionView, Intent.ActionSend, Intent.ActionSendMultiple },
                  Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
                  DataMimeType = "*/*")]
    public class ShareActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Intent?.Action == Intent.ActionView || Intent?.Action == Intent.ActionSend || Intent?.Action == Intent.ActionSendMultiple)
            {
                HandleIncomingShare(Intent);
            }
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIncomingShare(intent);
        }

        private void HandleIncomingShare(Intent intent)
        {
            Intent mainIntent = new Intent(this, typeof(MainActivity));
            mainIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);


            //Differ whether shared object exists as file or is a plaintext (or something else idk)
            if (intent.GetParcelableExtra(Android.Content.Intent.ExtraStream) != null) //is file
            {
                ShareIntentHelper.ClearIntentData();
                Android.Net.Uri sharedUri = null;
                if (intent.ClipData != null && intent.ClipData.ItemCount > 0)
                {
                    sharedUri = intent.ClipData.GetItemAt(0).Uri; // Get first file
                }
                else
                {
                    sharedUri = intent.GetParcelableExtra(Intent.ExtraStream) as Android.Net.Uri;
                }
                string filename = GetFilePathFromUri(sharedUri); //convert uri to filepath
                ShareIntentHelper.uri = sharedUri;

                ShareIntentHelper.IntentEnabled = true;

                ShareIntentHelper.uribytes = ShareIntentHelper.ReadBytesFromUri();
                ShareIntentHelper.intent = intent;
                ShareIntentHelper.SharedIntentFileName = filename; //save file location to static class

                FileInfo fileInfo = new FileInfo(filename);
                ShareIntentHelper.finfo = FileDetails.DetailsFromInfo(fileInfo);

                //Toast.MakeText(this, $"size: {ShareIntentHelper.finfo.SizeInBytes}", ToastLength.Long).Show();

                string mimeType = intent.Type;
                //Toast.MakeText(this, $"MIME Type: {mimeType}", ToastLength.Long).Show();
                //Toast.MakeText(this, $"enabled: {ShareIntentHelper.IntentEnabled}", ToastLength.Long).Show();
                

            } else //is text or something else
            {
                string content = intent.GetStringExtra(Android.Content.Intent.ExtraText);
                ShareIntentHelper.SharedText = content;
                //Toast.MakeText(this, $"content: {content}", ToastLength.Long).Show(); 
                
            }


            //string FileLocation = ((Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream)).ToString() ?? "none";
            /*
            ShareIntentHelper.SharedIntentFileName = intent.GetStringExtra(Android.Content.Intent.ExtraText); //save file location to static class
            ShareIntentHelper.FileType = intent.Type;*/
            //Toast.MakeText(this, $"Type: {(Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream)}", ToastLength.Long).Show();
            ShareIntentHelper.IntentDisplayed = false;
            StartActivity(mainIntent);
            
            Finish(); // Closes the ShareActivity to prevent a blank screen
        }
       

        

        public string GetFilePathFromUri(Android.Net.Uri uri)
        {
            string filePath = null;

            if (uri.Scheme.Equals("content"))
            {
                string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
                using (var cursor = ContentResolver.Query(uri, projection, null, null, null))
                {
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        int columnIndex = cursor.GetColumnIndex(projection[0]);
                        filePath = cursor.GetString(columnIndex);
                    }
                }
            }
            else if (uri.Scheme.Equals("file"))
            {
                filePath = uri.Path; // For file URIs, just return the path
            }

            return filePath;
        }

    }
}
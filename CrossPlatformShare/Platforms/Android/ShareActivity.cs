using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using CrossPlatformShare;

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
                string filename = GetFilePathFromUri((Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream)); //convert uri to filepath
                MiscClass.uri = (Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream);
                //ReadBytesFromUri((Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream));
                MiscClass.SharedIntentFileName = filename; //save file location to static class
                //Toast.MakeText(this, filename, ToastLength.Long).Show();
                
            } else //is text or something else
            {
                string content = intent.GetStringExtra(Android.Content.Intent.ExtraText);
                MiscClass.SharedText = content;
                //Toast.MakeText(this, $"content: {content}", ToastLength.Long).Show(); 
                
            }

            //string FileLocation = ((Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream)).ToString() ?? "none";
            /*
            MiscClass.SharedIntentFileName = intent.GetStringExtra(Android.Content.Intent.ExtraText); //save file location to static class
            MiscClass.FileType = intent.Type;*/
            //Toast.MakeText(this, $"Type: {(Android.Net.Uri)intent.GetParcelableExtra(Android.Content.Intent.ExtraStream)}", ToastLength.Long).Show();

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
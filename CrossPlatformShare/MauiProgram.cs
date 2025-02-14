using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android; //throws error when building on windows
#endif

namespace CrossPlatformShare
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            //removes underline for entry
            RemoveUnderlines();
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("wingding.ttf", "wingding");
                });



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        //void EndServer()



        static void RemoveUnderlines()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Placeholder", (h, v) => //removes underline for Entry
            {
#if ANDROID
                h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#elif IOS
h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Placeholder", (h, v) => //removes underline for Editor
            {
#if ANDROID
                h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#elif IOS
h.PlatformView.BorderStyle = UIKit.UITextViewBorderStyle.None;
#endif
            });
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Placeholder", (h, v) => //removes underline for Picker
            {
#if ANDROID
                h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#elif IOS
h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
        }
    }
}

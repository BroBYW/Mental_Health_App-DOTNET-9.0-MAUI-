using Android.App;
using Android.Runtime;

// ADD THIS LINE HERE:
// This injects the key into AndroidManifest.xml during the build

namespace PROJECT
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
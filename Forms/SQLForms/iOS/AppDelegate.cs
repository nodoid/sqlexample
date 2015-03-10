using Foundation;
using UIKit;

namespace SQLForms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public DBManager DBManager { get; set; }

        public static AppDelegate Self
        {
            get;
            private set;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            AppDelegate.Self = this;
            DBManager = new DBManager();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}


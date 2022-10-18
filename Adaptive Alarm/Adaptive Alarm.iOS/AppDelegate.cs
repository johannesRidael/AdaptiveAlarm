using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Shiny;
using UIKit;

[assembly: Shiny.ShinyApplication(
    ShinyStartupTypeName = "Adaptive_Alarm.MyShinyStartup",
    XamarinFormsAppTypeName = "Adaptive_Alarm.App"
)]


namespace Adaptive_Alarm.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
    }
}

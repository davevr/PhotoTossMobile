using Foundation;
using UIKit;
using Facebook.CoreKit;
using PhotoToss.Core;
using System.IO.IsolatedStorage;
using System;
using System.Collections.Generic;
using JVMenuPopover;
using HockeyApp;
using Google.Maps;

namespace PhotoToss.iOSApp
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window {
			get;
			set;
		}

		string appId = "439651239569547";
		string appName = "PhotoToss";
		string flurryID = "KTC993B58WKMR9WK66G3";
		string hockeyID = "41121ea9fd8f6c879122bb728a2488d9";
		const string MapsApiKey = "AIzaSyBG5fNmvfSfJSeX7x8cpmzHqgQ6pxkXeRY";

		public static GoogleAnalytics   analytics = null;
		public UINavigationController NavigationController {get; set;}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Flurry.Analytics.FlurryAgent.StartSession(flurryID);
			InitAnalytics ();
			// This is false by default,
			// If you set true, you can handle the user profile info once is logged into FB with the Profile.Notifications.ObserveDidChange notification,
			// If you set false, you need to get the user Profile info by hand with a GraphRequest
			Profile.EnableUpdatesOnAccessTokenChange (true);
			Settings.AppID = appId;
			Settings.DisplayName = appName;
			//
			// Code to start the Xamarin Test Cloud Agent
			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

			ConfigNavMenu ();
			UINavigationBar.Appearance.TintColor = UIColor.Green;
			UINavigationBar.Appearance.BarTintColor = UIColor.White;
			UINavigationBar.Appearance.BackgroundColor = UIColor.White;

			HockeyApp.Setup.EnableCustomCrashReporting (() => {

				//Get the shared instance
				var manager = BITHockeyManager.SharedHockeyManager;

				//Configure it to use our APP_ID
				manager.Configure (hockeyID);

				//Start the manager
				manager.StartManager ();

				//Authenticate (there are other authentication options)
				manager.Authenticator.AuthenticateInstallation ();

				//Rethrow any unhandled .NET exceptions as native iOS 
				// exceptions so the stack traces appear nicely in HockeyApp
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
					Setup.ThrowExceptionAsNative(e.ExceptionObject);

				System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, e) => 
					Setup.ThrowExceptionAsNative(e.Exception);
			});

			MapServices.ProvideAPIKey (MapsApiKey);

			// This method verifies if you have been logged into the app before, and keep you logged in after you reopen or kill your app.
			return ApplicationDelegate.SharedInstance.FinishedLaunching (app, options);
		}

		private void ConfigNavMenu()
		{
			//create the initial view controller
			//var rootController = (UIViewController)board.InstantiateViewController ("HomeViewController");
			var rootController = new HomeViewController();


			//build the shared menu
			JVMenuPopoverConfig.SharedInstance.MenuItems = new List<JVMenuItem>()
			{
				new JVMenuViewControllerItem()
				{
					//View exisiting view controller, will be reused everytime the item is selected
					Icon = UIImage.FromBundle(@"home-48"),
					Title = @"Home",
					ViewController = rootController,
				},
				new JVMenuViewControllerItem()
				{
					//New view controller, will be reused everytime the item is selected
					Icon = UIImage.FromBundle(@"business_contact-48"),
					Title = @"Leaderboards",
					ViewController = new LeaderboardViewController()
				},
				new JVMenuViewControllerItem()
				{
					//New view controller, will be reused everytime the item is selected
					Icon = UIImage.FromBundle(@"ask_question-48"),
					Title = @"Profile",
					ViewController = new ProfileViewController()
				},
				new JVMenuViewControllerItem()
				{
					//New view controller, will be reused everytime the item is selected
					Icon = UIImage.FromBundle(@"settings-48"),
					Title = @"Settings",
					ViewController = new SettingsViewController()
				},
				new JVMenuViewControllerItem()
				{
					Icon = UIImage.FromBundle(@"about-48"),
					Title = @"About PhotoToss",
					ViewController = new AboutViewController()
				},
			};

			//create a Nav controller an set the root controller
			NavigationController = new UINavigationController(rootController);

			//setup the window
			Window = new UIWindow(UIScreen.MainScreen.Bounds);
			Window.RootViewController = NavigationController;
			Window.ContentMode = UIViewContentMode.ScaleAspectFill;
			Window.BackgroundColor = UIColor.FromPatternImage(JVMenuHelper.ImageWithImage(UIImage.FromBundle("app_bg1.jpg"),this.Window.Frame.Width));
			Window.Add(NavigationController.View);
			Window.MakeKeyAndVisible();

		}

		private void InitAnalytics()
		{
			string uniqueId;

			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains("uniqueId"))
				uniqueId = settings["uniqueId"].ToString();
			else
			{
				uniqueId = Guid.NewGuid().ToString();
				settings.Add("uniqueId", uniqueId);
				settings.Save();

			}

			string maker = "Apple";
			string model = UIDevice.CurrentDevice.Model;
			string version = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
			string platform = "IOS";
			string userAgent = "Mozilla/5.0 (IOS; Apple; Mobile) ";

			analytics = new GoogleAnalytics(userAgent, maker, model, version, platform, uniqueId);
			analytics.StartSession();
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			// We need to handle URLs by passing them to their own OpenUrl in order to make the SSO authentication works.
			return ApplicationDelegate.SharedInstance.OpenUrl (application, url, sourceApplication, annotation);
		}



		public override void OnResignActivation (UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground (UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground (UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated (UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
			Facebook.CoreKit.AppEvents.ActivateApp();
		}

		public override void WillTerminate (UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}



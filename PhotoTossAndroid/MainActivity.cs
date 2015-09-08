using System;
using System.Net;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Android.Support.V7.AppCompat;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Graphics;
using Android.Media;

using Android.Util;
using Android.Text;
using Android.Text.Style;
using Android.Provider;

using Android.Locations;
using Java.IO;
using System.Diagnostics;

using ZXing;
using ZXing.Mobile;

using PhotoToss.Core;

using ByteSmith.WindowsAzure.Messaging;
using Gcm.Client;


using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Debug = System.Diagnostics.Debug;
using Xamarin.Facebook;
using Xamarin.Facebook.AppEvents;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;

using File = Java.IO.File;
[assembly:MetaData ("com.facebook.sdk.ApplicationId", Value ="@string/app_id")]
[assembly:MetaData ("com.facebook.sdk.ApplicationName", Value ="@string/app_name")]
namespace PhotoToss.AndroidApp
{
	[Activity(Label = "PhotoToss", MainLauncher = true, Icon = "@drawable/iconnoborder", Theme = "@style/Theme.AppCompat.Light", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
	public class MainActivity : Android.Support.V7.App.AppCompatActivity, ILocationListener
	{
		private String[] mDrawerTitles = new string[] { "Home", "Leaderboards", "Profile", "Settings",  "About PhotoToss"};
		private DrawerLayout mDrawerLayout;
		private ListView mDrawerList;
		private MyDrawerToggle mDrawerToggle;
		private bool refreshInProgress = false;

		private HomeFragment homePage;
		private BrowseFragment browsePage;
		private SettingsFragment settingsPage;
		private ProfileFragment profilePage;
		private AboutFragment aboutPage;
		public static Typeface headlineFace;
		public static Typeface bodyFace;
		public static File _dir;
		public static File _file;
		public static PhotoRecord _uploadPhotoRecord;
		public const string SENDER_ID = "865065760693";
		public const string ConnectionString = "Endpoint=sb://phototossnotify-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=FwWsviEIwwCK5vSg0kNiKcJs9GKuz70mXxYBGDYIvIU=";
		public const string NotificationHubPath = "phototossnotify";
		private const string flurryId = "YS7CWBRTNVQN3HV7Y3N5";
		private const string hockeyId = "8a9ce92b2889680033ff5c7edb65678a";
		public LinearLayout loginView = null;
		ProfileTracker profileTracker;
        private TextView promptText, actionPrompt;
        private ProgressDialog progressDlg;
        private int MAX_IMAGE_SIZE = 2048;

        public static GoogleAnalytics analytics = null;
		MobileBarcodeScanner scanner;

		public static Location	_lastLocation = new Location("passive");
		private LocationManager _locationManager;
		ICallbackManager callbackManager;

		public event Action PulledToRefresh;


		class MyDrawerToggle : Android.Support.V7.App.ActionBarDrawerToggle
		{
			private MainActivity baseActivity;

			public MyDrawerToggle(Activity activity, DrawerLayout drawerLayout, int openDrawerContentDescRes, int closeDrawerContentDescRes) :
			base(activity, drawerLayout, openDrawerContentDescRes, closeDrawerContentDescRes)
			{
				baseActivity = (MainActivity)activity;
			}
			public override void OnDrawerOpened(View drawerView)
			{
				base.OnDrawerOpened(drawerView);
				//baseActivity.Title = openString;


			}

			public override void OnDrawerClosed(View drawerView)
			{
				base.OnDrawerClosed(drawerView);
				//baseActivity.Title = closeString;
			}
		}

		class DrawerItemAdapter<T> : ArrayAdapter<T>
		{
			T[] _items;
			Activity _context;

			public DrawerItemAdapter(Context context, int textViewResourceId, T[] objects) :
			base(context, textViewResourceId, objects)
			{
				_items = objects;
				_context = (Activity)context;
			}

			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				View mView = convertView;
				if (mView == null)
				{
					mView = _context.LayoutInflater.Inflate(Resource.Layout.DrawerListItem, parent, false);

				}

				TextView text = mView.FindViewById<TextView>(Resource.Id.ItemName);

				if (_items[position] != null)
				{
					text.Text = _items[position].ToString();
					text.SetTextColor(_context.Resources.GetColor(Resource.Color.PT_dark_teal));
					text.SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
				}

				return mView;
			}
		}


		protected override void OnCreate(Bundle bundle)
		{
			Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			base.OnCreate(bundle);
			InitAnalytics();
			Flurry.Analytics.FlurryAgent.Init(this, flurryId);
			FacebookSdk.SdkInitialize (this.ApplicationContext);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			headlineFace = Typeface.CreateFromAsset(Assets, "fonts/RammettoOne-Regular.ttf");
			bodyFace = Typeface.CreateFromAsset(Assets, "fonts/SourceCodePro-Regular.ttf");

			// set up drawer
			mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			mDrawerList = FindViewById<ListView>(Resource.Id.left_drawer);
			// Set the adapter for the list view
			mDrawerList.Adapter = new DrawerItemAdapter<String>(this, Resource.Layout.DrawerListItem, mDrawerTitles);
			// Set the list's click listener
			mDrawerList.ItemClick += mDrawerList_ItemClick;

			mDrawerToggle = new MyDrawerToggle(this, mDrawerLayout, Resource.String.drawer_open, Resource.String.drawer_close);


			mDrawerLayout.SetDrawerListener(mDrawerToggle);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable( Resources.GetColor(Resource.Color.PT_light_teal)));

            loginView = FindViewById<LinearLayout>(Resource.Id.loginView);
            promptText = FindViewById<TextView>(Resource.Id.promptText);
            actionPrompt = FindViewById<TextView>(Resource.Id.actionPrompt);
            CreateDirectoryForPictures();

			selectItem(0);

			callbackManager = CallbackManagerFactory.Create ();

			var loginCallback = new FacebookCallback<LoginResult> {
				HandleSuccess = loginResult => {
					UpdateUI ();
				},
				HandleCancel = () => {
					ShowAlert (
						GetString (Resource.String.cancelled),
						GetString (Resource.String.permission_not_granted));


					UpdateUI ();                        
				},
				HandleError = loginError => {
					if (loginError is FacebookAuthorizationException) {
						ShowAlert (
							GetString (Resource.String.cancelled),
							GetString (Resource.String.permission_not_granted));
					}
					UpdateUI ();
				}
			};

			LoginManager.Instance.RegisterCallback (callbackManager, loginCallback);

			profileTracker = new CustomProfileTracker {
				HandleCurrentProfileChanged = (oldProfile, currentProfile) => {
					UpdateUI ();
				}
			};

            Android.Content.PM.PackageInfo siglist = this.PackageManager.GetPackageInfo("com.eweware.phototoss", Android.Content.PM.PackageInfoFlags.Signatures);

            foreach (Android.Content.PM.Signature curSig in siglist.Signatures)
            {
                Java.Security.MessageDigest md = Java.Security.MessageDigest.GetInstance("SHA");
                md.Update(curSig.ToByteArray());
                string something = Base64.EncodeToString(md.Digest(), Base64Flags.Default);
                System.Console.WriteLine(something);
            }

			// Register the crash manager before Initializing the trace writer
			HockeyApp.CrashManager.Register (this, hockeyId); 

			//Register to with the Update Manager
			HockeyApp.UpdateManager.Register (this, hockeyId);

			// Initialize the Trace Writer
			HockeyApp.TraceWriter.Initialize ();

			// Wire up Unhandled Expcetion handler from Android
			AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) => 
			{
				// Use the trace writer to log exceptions so HockeyApp finds them
				HockeyApp.TraceWriter.WriteTrace(args.Exception);
				args.Handled = true;
			};

			// Wire up the .NET Unhandled Exception handler
			AppDomain.CurrentDomain.UnhandledException +=
				(sender, args) => HockeyApp.TraceWriter.WriteTrace(args.ExceptionObject);

			// Wire up the unobserved task exception handler
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += 
				(sender, args) => HockeyApp.TraceWriter.WriteTrace(args.Exception);
			
            progressDlg = new ProgressDialog(this);
            progressDlg.SetProgressStyle(ProgressDialogStyle.Spinner);

            UpdateUI();

		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();

			profileTracker.StopTracking ();
		}

		private void UpdateUI ()
		{
			var enableButtons = AccessToken.CurrentAccessToken != null;


			var profile = Profile.CurrentProfile;
			ProfilePictureView pic = loginView.FindViewById<ProfilePictureView> (Resource.Id.profilePicture);

			if (enableButtons && profile != null) {
				pic.ProfileId = profile.Id;
                SupportActionBar.Show();
                actionPrompt.Visibility = ViewStates.Invisible;
                promptText.Visibility = ViewStates.Visible;
                PhotoTossRest.Instance.FacebookLogin(profile.Id, AccessToken.CurrentAccessToken.Token, (newUser) =>
                {
                    if (newUser != null)
                    {
                        RunOnUiThread(() =>
                        {
                            loginView.Visibility = ViewStates.Gone;
                            selectItem(0);
                            homePage.Refresh();
                        });

                    }
                });
                
			} else {
				pic.ProfileId = null;
                SupportActionBar.Hide();
                loginView.Visibility = ViewStates.Visible;
                actionPrompt.Visibility = ViewStates.Visible;
                promptText.Visibility = ViewStates.Invisible;
                if (PhotoTossRest.Instance.CurrentUser != null)
                    PhotoTossRest.Instance.Logout();
            }


		}


		void ShowAlert (string title, string msg, string buttonText = null)
		{
			new Android.Support.V7.App.AlertDialog.Builder (this)
				.SetTitle (title)
				.SetMessage (msg)
				.SetPositiveButton (buttonText, (s2, e2) => { })
				.Show ();
		}

		void InitLocation()
		{
			// get location

			if (_locationManager == null)
				_locationManager = GetSystemService (Context.LocationService) as LocationManager;
			Criteria locationCriteria = new Criteria();
			locationCriteria.Accuracy = Accuracy.Fine;
			locationCriteria.PowerRequirement = Power.NoRequirement;

			string locationProvider = _locationManager.GetBestProvider(locationCriteria, true);
			_locationManager.RequestLocationUpdates (locationProvider, 5 * 60 * 1000, 50.0f, this);
		}

		// ILocationListener
		public void OnLocationChanged (Location location)
		{
			_lastLocation = location;
		}

		public void OnProviderDisabled (string provider)
		{

		}



		public void OnProviderEnabled (string provider)
		{

		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{

		}

        protected override void OnStop()
        {
            progressDlg.Dismiss();
            base.OnStop();
        }

        protected override void OnResume()
		{
			base.OnResume();
			AppEventsLogger.ActivateApp (this);
			InitLocation ();
			UpdateFB ();
		}

		protected void UpdateFB()
		{
			if (AccessToken.CurrentAccessToken == null) {
				// better sign in
				//Intent	promptTask = new Intent (this, typeof(FirstRunActivity));
				//StartActivityForResult (promptTask, Utilities.SIGNIN_INTENT);

			} else {
				// already signed in
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			AppEventsLogger.DeactivateApp (this);
			_locationManager.RemoveUpdates(this);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainMenu, menu);


			return true;

		}


			

		public override bool OnMenuOpened(int featureId, IMenu menu)
		{
			if (featureId == (int)WindowFeatures.ActionBar && menu != null)
			{
				try
				{
					var menuBuilder = JNIEnv.GetObjectClass(menu.Handle);
					var setOptionalIconsVisibleMethod = JNIEnv.GetMethodID(menuBuilder, "setOptionalIconsVisible",
						"(Z)V");
					JNIEnv.CallVoidMethod(menu.Handle, setOptionalIconsVisibleMethod, new[] { new JValue(true) });

				}
				catch (Exception e)
				{
					System.Console.WriteLine (e.Message);
				}
			}
			return base.OnMenuOpened(featureId, menu);
		}




		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			mDrawerToggle.SyncState();
		}

		public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			mDrawerToggle.OnConfigurationChanged(newConfig);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (mDrawerToggle.OnOptionsItemSelected(item))
			{
				return true;
			}
			else
			{ 
				switch (item.ItemId)
				{
				case Resource.Id.PhotoButton:
					TakeAPicture();
					return true;
					break;
				case Resource.Id.CatchButton:
					CatchAPicture();
					return true;
					break;
				default:
					// show never get here.
					break;
				}
			}
			// Handle your other action bar items...

			return base.OnOptionsItemSelected(item);
		}

		void mDrawerList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			selectItem(e.Position);
		}

		private Android.Support.V4.App.Fragment oldPage = null;

		private void selectItem(int position)
		{
			Android.Support.V4.App.Fragment newPage = null;
			var fragmentManager = this.SupportFragmentManager;
			var ft = fragmentManager.BeginTransaction();
			bool firstTime = false;
			string pageName = "";

			switch (position)
			{
			case 0: // home
				if (homePage == null)
				{
					homePage = new HomeFragment();
					homePage.MainPage = this;
					firstTime = true;
				}
				newPage = homePage;
				pageName = "PhotoToss";
				break;
			case 1: // leaderboards
				if (browsePage == null)
				{
					browsePage = new BrowseFragment();
					browsePage.MainPage = this;
					firstTime = true;
				}
				newPage = browsePage;
				break;
			case 2: // profile
				if (profilePage == null)
				{
					profilePage = new ProfileFragment();
					profilePage.MainPage = this;
					firstTime = true;
				}
				newPage = profilePage;
				break;

			case 3: // Settings
				if (settingsPage == null)
				{
					settingsPage = new SettingsFragment();
					settingsPage.MainPage = this;
					firstTime = true;
				}
				newPage = settingsPage;
				break;

			case 4: // About
				if (aboutPage == null)
				{
					aboutPage = new AboutFragment();
					aboutPage.MainPage = this;
					firstTime = true;
				}
				newPage = aboutPage;
				break;
			}

			if (oldPage != newPage)
			{
				if (oldPage != null)
				{
					// to do - deactivate it
					ft.Hide(oldPage);

				}

				oldPage = newPage;

				if (newPage != null)
				{
					if (firstTime)
						ft.Add(Resource.Id.fragmentContainer, newPage);
					else
						ft.Show(newPage);
				}

				ft.Commit();

				// update selected item title, then close the drawer
				if (!String.IsNullOrEmpty(pageName))
					Title = pageName;
				else
					Title = mDrawerTitles[position];

				mDrawerList.SetItemChecked(position, true);
				mDrawerLayout.CloseDrawer(mDrawerList);
			}
		}

		protected override void OnTitleChanged(Java.Lang.ICharSequence title, Android.Graphics.Color color)
		{
			//base.OnTitleChanged (title, color);
			this.SupportActionBar.Title = title.ToString();

			SpannableString s = new SpannableString(title);
			s.SetSpan(new TypefaceSpan(this, "RammettoOne-Regular.ttf"), 0, s.Length(), SpanTypes.ExclusiveExclusive);
			s.SetSpan(new ForegroundColorSpan(Resources.GetColor(Resource.Color.PT_dark_orange)), 0, s.Length(), SpanTypes.ExclusiveExclusive);

			this.SupportActionBar.TitleFormatted = s;



		}

		public void StartRefresh(Action callback = null)
		{
			if (!refreshInProgress)
			{
				refreshInProgress = true;

				RunOnUiThread(() =>
					{
						if (homePage != null)
							homePage.Refresh();
						if (browsePage != null)
							browsePage.Refresh();
						if (settingsPage != null)
							settingsPage.Refresh();
						if (profilePage != null)
							profilePage.Refresh();
						if (callback != null)
							callback();
						refreshInProgress = false;
					});


			}
		}

		private void CreateDirectoryForPictures()
		{
			_dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "PhotoTossImages");
			if (!_dir.Exists())
			{
				_dir.Mkdirs();
			}
		}

		private async void CatchAPicture()
		{
			Button flashButton;
			View zxingOverlay;

            if (scanner == null)
                scanner = new MobileBarcodeScanner();
			//Tell our scanner we want to use a custom overlay instead of the default
			scanner.UseCustomOverlay = true;

			//Inflate our custom overlay from a resource layout
			zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

			//Find the button from our resource layout and wire up the click event
			flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
			flashButton.Click += (sender, e) => scanner.ToggleTorch();

			//Set our custom overlay

			scanner.CustomOverlay = zxingOverlay;

			//Start scanning!
			var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
			options.PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.AZTEC };
			var result = await scanner.Scan(options);


			HandleScanResult(result);

		}

		void HandleScanResult(ZXing.Result result)
		{
			string msg = "";	

			if (result != null && !string.IsNullOrEmpty (result.Text)) {
				PhotoTossRest.Instance.GetCatchURL ((urlStr) => {
					double latitude = 0.0;
					double longitude = 0.0;
					Bitmap tempImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.iconNoBorder);
					System.IO.Stream photoStream = new MemoryStream();
					tempImage.Compress(Bitmap.CompressFormat.Jpeg, 0, photoStream);
					// remove toss ID from URL
					long tossId = long.Parse(result.Text.Substring(result.Text.LastIndexOf("/") + 1));

					PhotoTossRest.Instance.CatchToss(photoStream, tossId, longitude, latitude, (newRec) => 
						{
							if (newRec != null)
								homePage.AddImage(newRec);
							else
							{
								RunOnUiThread(()=> {
									Toast.MakeText(this, "Catch failed.  Please try again.", ToastLength.Long).Show();
								});
							}

						});
				});
			} else {
				msg = "Scanning Canceled!";
				this.RunOnUiThread (() => {
					Toast.MakeText (this, msg, ToastLength.Short).Show ();
				});
			}
		}

		private Bitmap GetImageBitmapFromUrl(string url)
		{
			Bitmap imageBitmap = null;

			using (var webClient = new WebClient())
			{
				var imageBytes = webClient.DownloadData(url);
				if (imageBytes != null && imageBytes.Length > 0)
				{
					imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
				}
			}

			return imageBitmap;
		}


		private void TakeAPicture()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			_file = new File(_dir, String.Format("PhotoTossPhoto_{0}.jpg", Guid.NewGuid()));

			intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(_file));

			StartActivityForResult(intent, Utilities.PHOTO_CAPTURE_EVENT);
		}

		protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
		{
			if (resultCode == Android.App.Result.Ok)
			{
				switch (requestCode) {
				case Utilities.PHOTO_CAPTURE_EVENT:
                        DoPhotoUpload();
					break;

				

				default:
					base.OnActivityResult (requestCode, resultCode, data);
					callbackManager.OnActivityResult (requestCode, (int)resultCode, data);
					break;
				}
			}
			else
				base.OnActivityResult(requestCode, resultCode, data);
		}

        private void DoPhotoUpload()
        {
            RunOnUiThread(() =>
            {
                progressDlg.SetMessage("uploading image...");
                progressDlg.Show();
            });

            PhotoTossRest.Instance.GetUploadURL((theURL) => {
                using (System.IO.MemoryStream photoStream = new System.IO.MemoryStream())
                {
                    Bitmap scaledBitmap = BitmapHelper.LoadAndResizeBitmap(MainActivity._file.AbsolutePath, MAX_IMAGE_SIZE);
                    scaledBitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, photoStream);
                    photoStream.Flush();

                    double longitude = MainActivity._lastLocation.Longitude;
                    double latitude = MainActivity._lastLocation.Latitude;

                    PhotoTossRest.Instance.UploadImage(photoStream, longitude, latitude, (newRec) => {

                        if (newRec != null)
                        {
                            // now we upload a thumbnail immediately
                            PhotoTossRest.Instance.GetUploadURL((uploadStr) =>
                            {
                                Bitmap thumbnail = ThumbnailUtils.ExtractThumbnail(scaledBitmap, 64, 64);

                                using (System.IO.MemoryStream thumbStream = new System.IO.MemoryStream())
                                {
                                    thumbnail.Compress(Bitmap.CompressFormat.Png, 100, thumbStream);
                                    thumbStream.Flush();

                                    PhotoTossRest.Instance.UploadImageThumb(thumbStream, newRec.id, (theStr) =>
                                    {
                                        if (!String.IsNullOrEmpty(theStr))
                                            newRec.thumbnailurl = theStr;
                                        MainActivity._file.Delete();
                                        MainActivity._uploadPhotoRecord = newRec;
                                        FinishPhotoUpload();
                                    });
                                }
                            });
                        }
                        else
                        {
                            RunOnUiThread(() => {
                                progressDlg.Hide();
                                Toast.MakeText(this, "Image upload failed, please try again", ToastLength.Long).Show();
                            });
                        }
                    });
                }
            });
        }

        private void FinishPhotoUpload()
        {
            RunOnUiThread(() => {
                progressDlg.Hide();
            });
            if (_uploadPhotoRecord != null)
            {
                homePage.AddImage(_uploadPhotoRecord);
                _uploadPhotoRecord = null;
            }
            else
            {
                // to do - some error
            }
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

			string maker = Build.Manufacturer;
			string model = Build.Model;
			string version = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0).VersionName;
			string platform = "ANDROID";
			string userAgent = "Mozilla/5.0 (Linux; Android; Mobile) ";

			analytics = new GoogleAnalytics(userAgent, maker, model, version, platform, uniqueId);
			analytics.StartSession();
		}

		private void RegisterWithGCM()
		{
			// Check to ensure everything's setup right
			GcmClient.CheckDevice(this);
			GcmClient.CheckManifest(this);

			// Register for push notifications
			System.Diagnostics.Debug.WriteLine("Registering...");
			GcmClient.Register(this, SENDER_ID);
		}

		public static void DisplayAlert(Activity activity, string titleString, string descString)
		{
			activity.RunOnUiThread(() =>
				{
					Android.App.AlertDialog alert = new Android.App.AlertDialog.Builder(activity).Create();
					alert.SetTitle(titleString);
					alert.SetMessage(descString);
					alert.SetButton("ok", (sender, args) =>
						{
							alert.Dismiss();
						});
					alert.Show();
				});

		}




	}

	class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
	{
		public Action HandleCancel { get; set; }
		public Action<FacebookException> HandleError { get; set; }
		public Action<TResult> HandleSuccess { get; set; }

		public void OnCancel ()
		{
			var c = HandleCancel;
			if (c != null)
				c ();
		}

		public void OnError (FacebookException error)
		{
			var c = HandleError;
			if (c != null)
				c (error);
		}

		public void OnSuccess (Java.Lang.Object result)
		{
			var c = HandleSuccess;
			if (c != null)
				c (result.JavaCast<TResult> ());
		}
	}

	class CustomProfileTracker : ProfileTracker
	{
		public delegate void CurrentProfileChangedDelegate (Profile oldProfile, Profile currentProfile);

		public CurrentProfileChangedDelegate HandleCurrentProfileChanged { get; set; }

		protected override void OnCurrentProfileChanged (Profile oldProfile, Profile currentProfile)
		{
			var p = HandleCurrentProfileChanged;
			if (p != null)
				p (oldProfile, currentProfile);
		}
	}


}

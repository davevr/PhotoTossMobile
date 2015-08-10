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


using File = Java.IO.File;

namespace PhotoToss.AndroidApp
{
	[Activity(Label = "PhotoToss", MainLauncher = true, Icon = "@drawable/iconnoborder", Theme = "@style/Theme.AppCompat.Light", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
	public class MainActivity : Android.Support.V7.App.AppCompatActivity, ILocationListener
	{
		private String[] mDrawerTitles = new string[] { "Home", "Browse", "Stats", "Profile"};
		private DrawerLayout mDrawerLayout;
		private ListView mDrawerList;
		private MyDrawerToggle mDrawerToggle;
		private bool refreshInProgress = false;

		private HomeFragment homePage;
		private BrowseFragment browsePage;
		private StatsFragment statsPage;
		private ProfileFragment profilePage;
		public static Typeface headlineFace;
		public static Typeface bodyFace;
		public static File _dir;
		public static File _file;
		public static PhotoRecord _uploadPhotoRecord;
		public const string SENDER_ID = "865065760693";
		public const string ConnectionString = "Endpoint=sb://phototossnotify-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=FwWsviEIwwCK5vSg0kNiKcJs9GKuz70mXxYBGDYIvIU=";
		public const string NotificationHubPath = "phototossnotify";

		public static GoogleAnalytics analytics = null;
		MobileBarcodeScanner scanner;

		public static Location	_lastLocation = new Location("passive");
		private LocationManager _locationManager;

		public event Action PulledToRefresh;

		class TypefaceSpan : MetricAffectingSpan
		{
			private static LruCache sTypefaceCache = new LruCache(1024);

			private Typeface mTypeface;

			public TypefaceSpan(Context context, String typefaceName) 
			{
				mTypeface = (Typeface)sTypefaceCache.Get(typefaceName);

				if (mTypeface == null)
				{
					mTypeface = Typeface.CreateFromAsset(context.Assets, string.Format("fonts/{0}", typefaceName));
					sTypefaceCache.Put(typefaceName, mTypeface);
				}
			}

			public override void UpdateMeasureState(TextPaint tp)
			{
				tp.SetTypeface(mTypeface);
				tp.Flags = tp.Flags | PaintFlags.SubpixelText;
			}

			public override void UpdateDrawState(TextPaint tp)
			{
				tp.SetTypeface(mTypeface);
				tp.Flags = tp.Flags | PaintFlags.SubpixelText;
			}
		}


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

			CreateDirectoryForPictures();

			selectItem(0);

			FinishLoad ();


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

		protected override void OnResume()
		{
			base.OnResume();
			InitLocation ();
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainMenu, menu);

			/*
			if (_lastLocation != null) 
			{
				menu.FindItem(Resource.Id.CatchButton).SetEnabled(true);
				menu.FindItem(Resource.Id.PhotoButton).SetEnabled (true);
			} 
			else 
			{
				menu.FindItem(Resource.Id.CatchButton).SetEnabled(false);
				menu.FindItem(Resource.Id.PhotoButton).SetEnabled (false);
			}
			*/
			return true;

		}

		private void FinishLoad()
		{
			string username = (string)Utilities.SafeLoadSetting (Utilities.USERNAME, null);
			string password = (string)Utilities.SafeLoadSetting (Utilities.PASSWORD, null);

			if ((!String.IsNullOrEmpty (username)) && (!String.IsNullOrEmpty (password))) {
				PhotoTossRest.Instance.Login (username, password, (theUser) => {
					if (theUser == null)
						PromptForSignIn (username);
					else
						InitForSignIn ();
				});
			} else
				PromptForSignIn (username);
		}

		public void PromptForSignIn(string username = "")
		{
			// to do - add the user name
			Intent	promptTask = new Intent (this, typeof(FirstRunActivity));
			StartActivityForResult (promptTask, Utilities.SIGNIN_INTENT);
		}

		public void InitForSignIn()
		{
			Debug.Assert (PhotoTossRest.Instance.CurrentUser != null);
			StartRefresh ();
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
				case Resource.Id.AboutBtn:
					break;
				case Resource.Id.SettingsBtn:
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
			case 0:
				if (homePage == null)
				{
					homePage = new HomeFragment();
					homePage.MainPage = this;
					firstTime = true;
				}
				newPage = homePage;
				pageName = "PhotoToss";
				break;
			case 1:
				if (browsePage == null)
				{
					browsePage = new BrowseFragment();
					browsePage.MainPage = this;
					firstTime = true;
				}
				newPage = browsePage;
				break;
			case 2:
				if (statsPage == null)
				{
					statsPage = new StatsFragment();
					statsPage.MainPage = this;
					firstTime = true;
				}
				newPage = statsPage;
				break;
			case 3:
				if (profilePage == null)
				{
					profilePage = new ProfileFragment();
					profilePage.MainPage = this;
					firstTime = true;
				}
				newPage = profilePage;
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
						if (statsPage != null)
							statsPage.Refresh();
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
				scanner = new MobileBarcodeScanner(this);
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
					PhotoTossRest.Instance.GetUploadURL ((theURL) => {
						Intent upload = new Intent (this, typeof(UploadActivity));

						StartActivityForResult (upload, Utilities.PHOTO_UPLOAD_SUCCESS);
					});
					break;

				case Utilities.PHOTO_UPLOAD_SUCCESS:
					if (_uploadPhotoRecord != null) {
						homePage.AddImage (_uploadPhotoRecord);
						_uploadPhotoRecord = null;
					} else {
						// to do - some error
					}
					break;

				case Utilities.PROFILEIMAGE_CAPTURE_EVENT:
					ProgressDialog progressDlg = new ProgressDialog(this);
					RunOnUiThread(() =>
						{
							progressDlg.SetProgressStyle(ProgressDialogStyle.Spinner);
							progressDlg.SetMessage("uploading image...");
							progressDlg.Show();
						});

					PhotoTossRest.Instance.GetUserImageUploadURL((theURL) =>
						{
							using (System.IO.MemoryStream photoStream = new System.IO.MemoryStream())
							{
								int MAX_PROFILE_IMAGE_SIZE = 512;
								Bitmap scaledBitmap;
								scaledBitmap = BitmapHelper.LoadAndCropBitmap(MainActivity._file.AbsolutePath, MAX_PROFILE_IMAGE_SIZE);

								scaledBitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, photoStream);
								photoStream.Flush();

								PhotoTossRest.Instance.UploadUserImage(photoStream, (newRec) =>
									{
										if (newRec != null)
										{
											PhotoTossRest.Instance.CurrentUser.imageurl = newRec;
											RunOnUiThread(() =>
												{
													progressDlg.Hide();
													if ((profilePage != null) && (oldPage == profilePage))
														profilePage.UpdateUserImage();

												});
										}
										else
										{
											RunOnUiThread(() =>
												{
													progressDlg.Hide();
													Toast.MakeText(this, "Image upload failed, please try again", ToastLength.Long).Show();
												});
										}
									});
							}

						});
					break;

				case Utilities.SIGNIN_INTENT:
					// Complete the signin
					InitForSignIn ();
					break;

				default:
					base.OnActivityResult (requestCode, resultCode, data);
					break;
				}
			}
			else
				base.OnActivityResult(requestCode, resultCode, data);
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
}

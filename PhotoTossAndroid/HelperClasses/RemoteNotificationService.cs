using System;
using Android.App;
using Android.Content;
using Android.OS;
using WindowsAzure.Messaging;
using Android.Graphics;
using Android.Text;
using Android.Support.V4.App;
using Gcm;


namespace PhotoToss.AndroidApp
{
	
	[Service]
	public class RemoteNotificationService : GcmServiceBase
	{
		public static string RegistrationID { get; private set; }
		private NotificationHub Hub { get; set; }

		static NotificationHub hub;
        private static int _messageId = 0;
        public static void Initialize(Context context)
		{
			var cs = ConnectionString.CreateUsingSharedAccessKeyWithListenAccess (
				new Java.Net.URI ("sb://" + MyBroadcastReceiver.HUB_NAME + "-ns.servicebus.windows.net/"),
				MyBroadcastReceiver.HUB_LISTEN_SECRET);

			hub = new NotificationHub (MyBroadcastReceiver.HUB_NAME, cs, context);
		}

		public static void Register(Context Context)
		{
			GcmClient.Register (Context, MyBroadcastReceiver.SENDER_IDS);
		}

		public RemoteNotificationService() : base(MyBroadcastReceiver.SENDER_IDS) 
		{ 
		}

		protected override void OnRegistered (Context context, string registrationId)
		{
			//Receive registration Id for sending GCM Push Notifications to

			if (hub != null) {
                string[] keys;
                string UserId = null;

                if ((PhotoToss.Core.PhotoTossRest.Instance != null) &&
                    (PhotoToss.Core.PhotoTossRest.Instance.CurrentUser != null))
                    UserId = PhotoToss.Core.PhotoTossRest.Instance.CurrentUser.id.ToString();

                if (String.IsNullOrEmpty(UserId))
                    keys = new[] { "android" };
                else
                    keys = new[] { "user_" + UserId, "android" };

                var registration = hub.Register (registrationId, keys);

				Console.WriteLine ("Azure Registered: " + registration.PNSHandle + " -> " + registration.NotificationHubPath);
			}

		}

		protected override void OnUnRegistered (Context context, string registrationId)
		{
            if (hub != null)
                hub.Unregister();
		}

		protected override void OnMessage (Context context, Intent intent)
		{
			Console.WriteLine ("Received Notification");

            string titleParam = intent.GetStringExtra("title");
            string contentParam = intent.GetStringExtra("body");
            string imageParam = intent.GetStringExtra("image");
            string imageIdParam = intent.GetStringExtra("imageid");

            if (titleParam == null)
                titleParam = "News from PhotoToss";

            if (contentParam == null)
                contentParam = "";

            if (imageIdParam == null)
                imageIdParam = "0";


            SpannedString titleStr = new SpannedString(titleParam);
            SpannedString contentStr = new SpannedString(contentParam);
            long imageId = long.Parse(imageIdParam);

            var nMgr = (NotificationManager)GetSystemService(NotificationService);
            Bitmap iconBitmap;

            if (imageParam == null)
                iconBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_notify_color);
            else
            {
                iconBitmap = GetImageBitmapFromUrl(imageParam);
                if (iconBitmap == null)
                    iconBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_notify_color);
            }

            var newIntent = new Intent(this, typeof(MainActivity));
            Bundle b = new Bundle();
            b.PutLong("imageid", imageId);
            newIntent.PutExtras(b);

            var pendingIntent = PendingIntent.GetActivity(this, 99, newIntent, PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this);
            Notification notification = builder.SetSmallIcon(Resource.Drawable.ic_notify_white).SetLargeIcon(iconBitmap).SetTicker(titleStr).SetContentText(contentParam).SetContentTitle(titleStr).SetContentIntent(pendingIntent).Build();

            nMgr.Notify(_messageId, notification);
            _messageId++;
        }

		protected override bool OnRecoverableError (Context context, string errorId)
		{
			//Some recoverable error happened
			return true;
		}

		protected override void OnError (Context context, string errorId)
		{
			//Some more serious error happened
		}


		/*
		static PowerManager.WakeLock _wakeLock;
		static readonly object Lock = new object();

		public static INotificationHub Hub { get; set; }
		public static Registration NativeRegistration { get; set; }

		private static int _messageId;

		static RemoteNotificationService ()
		{
			Hub = new NotificationHub(
				"phototossnotify",
				"Endpoint=sb://phototossnotify-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=FwWsviEIwwCK5vSg0kNiKcJs9GKuz70mXxYBGDYIvIU=");
		}

		public static void RunIntentInService(Context context, Intent intent)
		{
			lock (Lock)
			{
				if (_wakeLock == null)
				{
					// This is called from BroadcastReceiver, there is no init.
					var pm = PowerManager.FromContext(context);
					_wakeLock = pm.NewWakeLock(
						WakeLockFlags.Partial, "My WakeLock Tag");
				}
			}

			_wakeLock.Acquire();
			intent.SetClass(context, typeof(RemoteNotificationService));
			context.StartService(intent);
		}


		protected override void OnHandleIntent(Intent intent)
		{
			try
			{
				Context context = ApplicationContext;
				string action = intent.Action;

				if (action.Equals("com.google.android.c2dm.intent.REGISTRATION"))
				{
					HandleRegistration(context, intent);
				}
				else if (action.Equals("com.google.android.c2dm.intent.RECEIVE"))
				{
					HandleMessage(context, intent);
				}
			}
			finally
			{
				lock (Lock)
				{
					//Sanity check for null as this is a public method
					if (_wakeLock != null)
						_wakeLock.Release();
				}
			}
		}

		protected async void HandleRegistration(Context context, Intent intent)
		{
			// TODO:  Use NotificationBuilder
			string registrationId = intent.GetStringExtra("registration_id");
			string error = intent.GetStringExtra("error");
			string unregistration = intent.GetStringExtra("unregistered");
			string[] keys;
			string UserId = null;

			if ((PhotoToss.Core.PhotoTossRest.Instance != null) &&
			    (PhotoToss.Core.PhotoTossRest.Instance.CurrentUser != null))
				UserId = PhotoToss.Core.PhotoTossRest.Instance.CurrentUser.id.ToString ();

			if (String.IsNullOrEmpty (UserId))
				keys = new [] { "android" };
			else
				keys = new [] { "user_" + UserId, "android" };

			var nMgr = (NotificationManager)GetSystemService (NotificationService);

			if (!String.IsNullOrEmpty (registrationId)) {
				
				NativeRegistration = await Hub.RegisterNativeAsync (registrationId, keys);

				var notificationNativeRegistration = new Notification (Resource.Drawable.ic_notify_white, "Welcome to PhotoToss!");
				var pendingIntentNativeRegistration = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(MainActivity)), 0);
				notificationNativeRegistration.LargeIcon = BitmapFactory.DecodeResource (Resources, Resource.Drawable.ic_notify_color);
				notificationNativeRegistration.SetLatestEventInfo (this, "Welcome to PhotoToss", "Have fun tossing!", pendingIntentNativeRegistration);
				nMgr.Notify (_messageId, notificationNativeRegistration);
				_messageId++;
			}
			else if (!String.IsNullOrEmpty (error)) {
				var notification = new Notification (Resource.Drawable.ic_notify_white, "Gcm Registration Failed.");
				var pendingIntent = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(MainActivity)), 0);
				notification.SetLatestEventInfo (this, "Gcm Registration Failed", error, pendingIntent);
				nMgr.Notify (_messageId, notification);
				_messageId++;
			}
			else if (!String.IsNullOrEmpty (unregistration)) {
				if (NativeRegistration != null)
					await Hub.UnregisterAsync (NativeRegistration);

				var notification = new Notification (Resource.Drawable.ic_notify_white, "Push Notifications Disabled");
				var pendingIntent = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(MainActivity)), 0);
				notification.LargeIcon = BitmapFactory.DecodeResource (Resources, Resource.Drawable.ic_notify_color);
				notification.SetLatestEventInfo (this, "Push Notifications Disabled", "Re-enable in your profile.", pendingIntent);
				nMgr.Notify (_messageId, notification);
				_messageId++;
			}
		}

		protected void HandleMessage(Context context, Intent intent)
		{
			string titleParam = intent.GetStringExtra ("title");
			string contentParam = intent.GetStringExtra ("body");
			string imageParam = intent.GetStringExtra ("image");
			string imageIdParam = intent.GetStringExtra ("imageid");

			if (titleParam == null)
				titleParam = "News from PhotoToss";

			if (contentParam == null)
				contentParam = "";

			if (imageIdParam == null)
				imageIdParam = "0";


			SpannedString titleStr = new SpannedString (titleParam);
			SpannedString contentStr = new SpannedString (contentParam);
			long imageId = long.Parse (imageIdParam);

			var nMgr = (NotificationManager)GetSystemService (NotificationService);
			var notification = new Notification (Resource.Drawable.ic_notify_white, titleStr);
			Bitmap iconBitmap;

			if (imageParam == null)
				iconBitmap = BitmapFactory.DecodeResource (Resources, Resource.Drawable.ic_notify_color);
			else {
				iconBitmap = GetImageBitmapFromUrl (imageParam);
				if (iconBitmap == null)
					iconBitmap = BitmapFactory.DecodeResource (Resources, Resource.Drawable.ic_notify_color);
			}
			
			notification.LargeIcon = iconBitmap; 
			notification.Icon = Resource.Drawable.ic_notify_white;
			notification.TickerText = titleStr;
			var newIntent = new Intent (this, typeof(MainActivity));
			Bundle b = new Bundle ();
			b.PutLong ("imageid", imageId);
			newIntent.PutExtras (b);

			var pendingIntent = PendingIntent.GetActivity (this, 99, newIntent, PendingIntentFlags.UpdateCurrent);

			notification.SetLatestEventInfo (this, titleStr, contentStr, pendingIntent);
			nMgr.Notify (_messageId, notification);
			_messageId++;
		}
		*/

		private Bitmap GetImageBitmapFromUrl(string url)
		{
			Bitmap imageBitmap = null;

			using (var webClient = new System.Net.WebClient())
			{
				var imageBytes = webClient.DownloadData(url);
				if (imageBytes != null && imageBytes.Length > 0)
				{
					imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
				}
			}

			return imageBitmap;
		}

	}
}
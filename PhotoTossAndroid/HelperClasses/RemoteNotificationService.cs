using System;
using Android.App;
using Android.Content;
using Android.OS;
using ByteSmith.WindowsAzure.Messaging;
using Android.Graphics;
using Android.Text;

namespace PhotoToss.AndroidApp
{
	[Service]
	public class RemoteNotificationService : IntentService
	{
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
				keys = new [] { "user_" + UserId };

			var nMgr = (NotificationManager)GetSystemService (NotificationService);

			if (!String.IsNullOrEmpty (registrationId)) {
				NativeRegistration = await Hub.RegisterNativeAsync (registrationId, keys);

				var notificationNativeRegistration = new Notification (Resource.Drawable.ic_notify_white, "Native Registered");
				var pendingIntentNativeRegistration = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(MainActivity)), 0);
				notificationNativeRegistration.SetLatestEventInfo (this, "Native Reg. ID", NativeRegistration.RegistrationId, pendingIntentNativeRegistration);
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

				var notification = new Notification (Resource.Drawable.ic_notify_white, "Unregistered successfully.");
				var pendingIntent = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(MainActivity)), 0);
				notification.SetLatestEventInfo (this, "MyIntentService", "Unregistered successfully.", pendingIntent);
				nMgr.Notify (_messageId, notification);
				_messageId++;
			}
		}

		protected void HandleMessage(Context context, Intent intent)
		{
			string titleParam = intent.GetStringExtra ("title");
			string contentParam = intent.GetStringExtra ("body");


			SpannedString titleStr = new SpannedString (titleParam);
			SpannedString contentStr = new SpannedString (contentParam);
			var nMgr = (NotificationManager)GetSystemService (NotificationService);
			var notification = new Notification (Resource.Drawable.ic_notify_white, titleStr);
			notification.LargeIcon = BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_notify_color);
			notification.Icon = Resource.Drawable.ic_notify_white;
			notification.TickerText = titleStr;
			var pendingIntent = PendingIntent.GetActivity (this, 99, new Intent (this, typeof(MainActivity)), 0);
			notification.SetLatestEventInfo (this, titleStr, contentStr, pendingIntent);
			nMgr.Notify (_messageId, notification);
			_messageId++;
		}
	}
}
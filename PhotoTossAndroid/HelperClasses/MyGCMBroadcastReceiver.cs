using Android.App;
using Android.Content;
using Gcm;

[assembly: UsesPermission(Android.Manifest.Permission.ReceiveBootCompleted)]
namespace PhotoToss.AndroidApp
{
    /*
	[BroadcastReceiver(Permission= "com.google.android.c2dm.permission.SEND")]
	[IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new[] {"PhotoToss" })]
	[IntentFilter(new[] { "com.google.android.c2dm.intent.REGISTRATION" }, Categories = new[] {"PhotoToss" })]
	[IntentFilter(new[] { "com.google.android.gcm.intent.RETRY" }, Categories = new[] { "PhotoToss"})]
	public class MyGCMBroadcastReceiver : BroadcastReceiver
	{
		const string TAG = "PushHandlerBroadcastReceiver";
		public override void OnReceive(Context context, Intent intent)
		{
			RemoteNotificationService.RunIntentInService(context, intent);
			SetResult(Result.Ok, null, null);
		}
	}
	*/

    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })] // Allow GCM on boot and when app is closed
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<RemoteNotificationService>
	{
		public static string[] SENDER_IDS = new string[] { MainActivity.SENDER_ID };
		public const string HUB_NAME = "phototossnotify";
		public const string HUB_LISTEN_SECRET = "FwWsviEIwwCK5vSg0kNiKcJs9GKuz70mXxYBGDYIvIU=";

		public const string TAG = "MyBroadcastReceiver-GCM";
	}
}
using Android.App;
using Android.Content;

namespace PhotoToss.AndroidApp
{
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
}
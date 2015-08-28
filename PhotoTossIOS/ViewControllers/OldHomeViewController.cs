using System;
using System.Collections.Generic;
using UIKit;
using Facebook.LoginKit;
using Facebook.CoreKit;
using CoreGraphics;
using Foundation;
using JVMenuPopover;


namespace PhotoToss.iOSApp
{
	public partial class OldHomeViewController : JVMenuViewController
	{

		List<string> readPermissions = new List<string> { "public_profile" };

		LoginButton loginButton;
		ProfilePictureView pictureView;
		UILabel nameLabel;
		public OldHomeViewController () : base ()
		{
		}

		public OldHomeViewController (IntPtr point) : base ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// If was send true to Profile.EnableUpdatesOnAccessTokenChange method
			// this notification will be called after the user is logged in and
			// after the AccessToken is gotten
			Profile.Notifications.ObserveDidChange ((sender, e) => {

				if (e.NewProfile == null)
					return;

				nameLabel.Text = e.NewProfile.Name;
			});

			// Set the Read and Publish permissions you want to get
			loginButton = new LoginButton (new CGRect (80, 20, 220, 46)) {
				LoginBehavior = LoginBehavior.Native,
				ReadPermissions = readPermissions.ToArray ()
			};

			// Handle actions once the user is logged in
			loginButton.Completed += (sender, e) => {
				if (e.Error != null) {
					// Handle if there was an error
				}

				if (e.Result.IsCancelled) {
					// Handle if the user cancelled the login request
				}

				// Handle your successful login
			};

			// Handle actions once the user is logged out
			loginButton.LoggedOut += (sender, e) => {
				// Handle your logout
				nameLabel.Text = "";
			};

			// The user image profile is set automatically once is logged in
			pictureView = new ProfilePictureView (new CGRect (80, 100, 220, 220));

			// Create the label that will hold user's facebook name
			nameLabel = new UILabel (new CGRect (20, 319, 280, 21)) {
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear
			};

			// If you have been logged into the app before, ask for the your profile name
			if (AccessToken.CurrentAccessToken != null) {
				var request = new GraphRequest ("/me?fields=name,id", null, AccessToken.CurrentAccessToken.TokenString, null, "GET");
				request.Start ((connection, result, error) => {
					// Handle if something went wrong with the request
					if (error != null) {
						new UIAlertView ("Error...", error.Description, null, "Ok", null).Show ();
						return;
					}

					// Get your profile name
					var userInfo = result as NSDictionary;
					nameLabel.Text = userInfo ["name"].ToString ();
					string uniqueId = userInfo["id"].ToString();
					DoSignin(uniqueId, AccessToken.CurrentAccessToken.TokenString);
				});
			}

			// Add views to main view
			View.AddSubview (loginButton);
			View.AddSubview (pictureView);
			View.AddSubview (nameLabel);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		public void DoSignin(string userId, string accessToken)
		{
			PhotoToss.Core.PhotoTossRest.Instance.FacebookLogin (userId, accessToken, (theUser) => {


			});
		}


	}
}


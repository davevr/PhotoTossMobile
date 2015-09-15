
using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using JVMenuPopover;
using Facebook.LoginKit;
using Facebook.CoreKit;
using PhotoToss.Core;
using CoreGraphics;

namespace PhotoToss.iOSApp
{


	public partial class ProfileViewController : JVMenuViewController
	{
		LoginButton loginButton;
		ProfilePictureView pictureView;
		UILabel nameLabel;
		List<string> readPermissions = new List<string> { "public_profile" };
		private UIView loadingOverlay = null;

		public ProfileViewController () : base ()
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			AddFacebookUI ();

			// Perform any additional setup after loading the view, typically from a nib.
			if (AccessToken.CurrentAccessToken == null)
				EnsureFacebookSignin ();
			else if (PhotoTossRest.Instance.CurrentUser == null)
				EnsurePhotoTossSignin ();
			else
				UpdateUI ();
			
		}

		private void AddFacebookUI()
		{
			var bounds = UIScreen.MainScreen.Bounds;

			Profile.Notifications.ObserveDidChange ((sender, e) => {

				if (e.NewProfile == null) {
					nameLabel.Text = "";
					pictureView.ProfileId = "";
				} else {
					pictureView.ProfileId = e.NewProfile.UserID;
					nameLabel.Text = e.NewProfile.Name;
				}


			});

			// Set the Read and Publish permissions you want to get
			loginButton = new LoginButton (new CGRect (80, 120, 220, 46)) {
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

				EnsurePhotoTossSignin();
			};

			// Handle actions once the user is logged out
			loginButton.LoggedOut += (sender, e) => {
				// Handle your logout
				nameLabel.Text = "";
			};

			// The user image profile is set automatically once is logged in
			pictureView = new ProfilePictureView (new CGRect (80, 200, 220, 220));

			// Create the label that will hold user's facebook name
			nameLabel = new UILabel (new CGRect (20, 420, 280, 21)) {
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear
			};



			// Add views to main view
			View.AddSubview (loginButton);
			View.AddSubview (pictureView);
			View.AddSubview (nameLabel);

		}


		private void EnsureFacebookSignin()
		{
			// nothing we need to do here
			UpdateUI();
		}


		private void EnsurePhotoTossSignin ()
		{
			if (AccessToken.CurrentAccessToken != null) {
				if (PhotoTossRest.Instance.CurrentUser == null) {
					ShowOverlay (View, "Connecting to the Tosstaplex...");
					var request = new GraphRequest ("/me?fields=name,id", null, AccessToken.CurrentAccessToken.TokenString, null, "GET");
					request.Start ((connection, result, error) => {
						// Handle if something went wrong with the request
						if (error != null) {
							new UIAlertView ("Error...", error.Description, null, "Ok", null).Show ();
							return;
						}

						// Get your profile name
						var userInfo = result as NSDictionary;

						PhotoToss.Core.PhotoTossRest.Instance.FacebookLogin (userInfo ["id"].ToString (), AccessToken.CurrentAccessToken.TokenString, (theUser) => {
							// to do - complete sign in!
							HideOverlay();
							UpdateUI();
						});
					});
				}
			}
		}

		private void UpdateUI()
		{
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

					InvokeOnMainThread (() => {
						nameLabel.Text = userInfo["name"].ToString();
						pictureView.ProfileId = userInfo["id"].ToString();

					});
				});
			} else {
				nameLabel.Text = "";
				pictureView.ProfileId = null;
			}

		}

		private void ShowOverlay(UIView targetView, string prompt)
		{
			var bounds = UIScreen.MainScreen.Bounds;
			if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight) {
				bounds.Size = new CGSize(bounds.Size.Height, bounds.Size.Width);
			}
			// show the loading overlay on the UI thread using the correct orientation sizing
			this.loadingOverlay = new LoadingOverlay (bounds);
			((LoadingOverlay)loadingOverlay).Prompt = prompt;

			InvokeOnMainThread (() => {
				targetView.Add (this.loadingOverlay);
			});



		}

		private void HideOverlay()
		{
			InvokeOnMainThread (() => {
				((LoadingOverlay)loadingOverlay).Hide ();
			});



		}


	}
}


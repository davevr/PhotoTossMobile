﻿
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

			FakeHeaderView.Layer.ShadowOffset = new CGSize (1, 5);
			FakeHeaderView.Layer.ShadowColor = new CGColor (0, 0, 0);
			FakeHeaderView.Layer.ShadowOpacity = 0.5f;

			ViewTitle.AttributedText = new NSAttributedString("Profile", UIFont.FromName("RammettoOne-Regular", 17),
				UIColor.FromRGB(255,121,0));
			
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
					ProfileNameLabel.Text = "";
					pictureView.ProfileId = "";
				} else {
					pictureView.ProfileId = e.NewProfile.UserID;
					ProfileNameLabel.Text = e.NewProfile.Name;
				}


			});

			// Set the Read and Publish permissions you want to get
			loginButton = new LoginButton (new CGRect ((bounds.Width / 2) - 64, 232, 128, 30)) {
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
				ProfileNameLabel.Text = "";
			};

			// The user image profile is set automatically once is logged in
			pictureView = new ProfilePictureView (new CGRect ((bounds.Width /2 ) - 64, 98, 128,128));




			// Add views to main view
			View.AddSubview (loginButton);
			View.AddSubview (pictureView);

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
					ShowOverlay (View, "Connecting to the PhotoToss...");
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
					PhotoTossRest.Instance.GetUserStats((theStats) => {
						InvokeOnMainThread (() => {
							ProfileNameLabel.Text = userInfo["name"].ToString();
							pictureView.ProfileId = userInfo["id"].ToString();
							TossesCount.Text = theStats.numtosses.ToString();
							CatchesCount.Text = theStats.numcatches.ToString();
							TakenCount.Text = theStats.numoriginals.ToString();
							CollectedCount.Text = theStats.numimages.ToString();
						});
					});
				});
			} else {
				InvokeOnMainThread (() => {
					ProfileNameLabel.Text = "";
					pictureView.ProfileId = "";
					TossesCount.Text = "--";
					CatchesCount.Text = "--";
					TakenCount.Text = "--";
					CollectedCount.Text = "--";
				});
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


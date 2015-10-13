
using System;

using Foundation;
using UIKit;
using JVMenuPopover;
using CoreGraphics;
using MessageUI;

namespace PhotoToss.iOSApp
{
	public partial class AboutViewController : JVMenuViewController
	{
		public AboutViewController () : base ()
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
			
			// Perform any additional setup after loading the view, typically from a nib.
			FakeHeaderView.Layer.ShadowOffset = new CGSize (1, 5);
			FakeHeaderView.Layer.ShadowColor = new CGColor (0, 0, 0);
			FakeHeaderView.Layer.ShadowOpacity = 0.5f;

			ViewTitle.AttributedText = new NSAttributedString("About PhotoToss", UIFont.FromName("RammettoOne-Regular", 17),
				UIColor.FromRGB(255,121,0));

			ReviewInStoreBtn.TouchUpInside += (object sender, EventArgs e) => { HandleReview(); };
			SendFeedbackBtn.TouchUpInside += (object sender, EventArgs e) => { HandleFeedback(); };
			ShareBtn.TouchUpInside += (object sender, EventArgs e) => { HandleShare();};
		}

		private void HandleReview()
		{
			UIApplication.SharedApplication.OpenUrl(new NSUrl("itms-apps://itunes.apple.com/app/id890164360"));
		}


		private void HandleFeedback()
		{
			MFMailComposeViewController mailController;

			if (MFMailComposeViewController.CanSendMail) {
				// do mail operations here
				mailController = new MFMailComposeViewController ();
				mailController.SetToRecipients (new string[]{"phototoss@eweware.com"});
				mailController.SetSubject ("Thoughts on PhotoToss");
				mailController.SetMessageBody ("Here are some thoughts on PhotoToss", false);
				mailController.Finished += ( object s, MFComposeResultEventArgs args) => {
					Console.WriteLine (args.Result.ToString ());
					args.Controller.DismissViewController (true, null);
				};
				this.PresentViewController (mailController, true, null);

			}

		}

		private void HandleShare()
		{
			NSUrl newURL = new NSUrl("http://wwww.PhotoToss.com");
			var activityViewController = new UIActivityViewController(new NSObject[] {newURL }, null)
			{

			};
			PresentViewController(activityViewController, true, null);
		}
	}
}


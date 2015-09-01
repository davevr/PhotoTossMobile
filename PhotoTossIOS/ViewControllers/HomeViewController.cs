
using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using JVMenuPopover;
using ZXing.Mobile;

using CoreGraphics;
using UIKit;
using AVFoundation;
using CoreVideo;
using CoreMedia;

using CoreFoundation;
using System.Runtime.InteropServices;
using PhotoToss.Core;

using Facebook.LoginKit;
using Facebook.CoreKit;


namespace PhotoToss.iOSApp
{
	public partial class HomeViewController : JVMenuViewController
	{
		public static AVCaptureSession session;
		public static UIImageView ImageView;
		OutputRecorder outputRecorder;
		DispatchQueue queue;
		private UIView loadingOverlay = null;
		List<string> readPermissions = new List<string> { "public_profile" };

		LoginButton loginButton;
		ProfilePictureView pictureView;
		UILabel nameLabel;

		public HomeViewController () : base ()
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
			CameraBtn.Clicked += (object sender, EventArgs e) => {
				DoTakePicture ();
			};

			CatchBtn.Clicked += (object sender, EventArgs e) => {
				DoCatchPicture ();
			};

			// make the image view (temp)
			ImageView = new UIImageView (new CGRect (20, 20, 280, 280));
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
			View.Add (ImageView);
			ImageView.Hidden = true;

			// Ensure Facebook Signin
			if (AccessToken.CurrentAccessToken == null)
				EnsureFacebookSignin ();
			else
				CompleteSignin ();
		}

		private void CompleteSignin ()
		{
			// If you have been logged into the app before, ask for the your profile name
			if (AccessToken.CurrentAccessToken != null) {
				ShowOverlay (View, "Connecting to the Tosstopolis...");
				var request = new GraphRequest ("/me?fields=name,id", null, AccessToken.CurrentAccessToken.TokenString, null, "GET");
				request.Start ((connection, result, error) => {
					// Handle if something went wrong with the request
					if (error != null) {
						new UIAlertView ("Error...", error.Description, null, "Ok", null).Show ();
						return;
					}

					// Get your profile name
					var userInfo = result as NSDictionary;

					PhotoToss.Core.PhotoTossRest.Instance.FacebookLogin (userInfo["id"].ToString(), AccessToken.CurrentAccessToken.TokenString, (theUser) => {

						InvokeOnMainThread(() => {
							HideOverlay();
						}); 
					});
				});
			}
		}

		private void EnsureFacebookSignin()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			loadingOverlay = new UIView (bounds);
			loadingOverlay.BackgroundColor = UIColor.Black;
			loadingOverlay.Alpha = 0.75f;
			//View.AddSubview (loadingOverlay);


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
				loadingOverlay.RemoveFromSuperview();
				CompleteSignin();
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



			// Add views to main view
			View.AddSubview (loginButton);
			View.AddSubview (pictureView);
			View.AddSubview (nameLabel);
		}

		private void DoTakePicture()
		{
			var filePicker = new UIImagePickerController();
			filePicker.FinishedPickingMedia += FileChooseFinished;
			filePicker.Canceled += (sender1, eventArguments) => {

				filePicker.DismissViewController(true, 
					() => {});
			};

			filePicker.SourceType = UIImagePickerControllerSourceType.Camera;
			PresentViewController (filePicker, true, 
				() => UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Slide));
			
		}

		private void FileChooseFinished(object sender, UIImagePickerMediaPickedEventArgs eventArgs)
		{
			// NOTE:  when this is called, the photo is still on the screen
			ShowOverlay(((UIImagePickerController) sender).View, "Uploading Image to the Tosstoplex...");
			LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
			LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
			LocationHelper.StopLocationManager ();

			UIImage imageForUploading =  UIImageHelper.ScaleAndRotateImage(eventArgs.OriginalImage);
			DateTime now = DateTime.Now;
			string imageName = String.Format ("{0}_{1}.jpg", now.ToLongDateString(), PhotoTossRest.Instance.CurrentUser.id);

			PhotoTossRest.Instance.GetUploadURL ((theUrl) => {
				PhotoTossRest.Instance.UploadImage (imageForUploading.AsJPEG ().AsStream (), "", curLoc.Longitude, curLoc.Latitude, (theRecord) => {
				
					InvokeOnMainThread(() => {
						((UIImagePickerController)sender).DismissViewController (true, () => {
						});
						// to do - update the collection view
					});

				});
			});


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


		private async void DoCatchPicture()
		{
			var scanner = new ZXing.Mobile.MobileBarcodeScanner();
			var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
			options.PossibleFormats = new List<ZXing.BarcodeFormat>() { 
				ZXing.BarcodeFormat.AZTEC 
			};

			var result = await scanner.Scan(options, true);

			if (result != null) {
				CaptureOneFrame ();
			}
		}

		private void CaptureOneFrame()
		{
			ImageView.Hidden = false;
			SetupCaptureSession ();
		}

		bool SetupCaptureSession ()
		{
			// configure the capture session for low resolution, change this if your code
			// can cope with more data or volume
			session = new AVCaptureSession () {
				SessionPreset = AVCaptureSession.Preset1280x720
			};

			// create a device input and attach it to the session
			var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
			if (captureDevice == null){
				Console.WriteLine ("No captureDevice - this won't work on the simulator, try a physical device");
				return false;
			}
			//Configure for 15 FPS. Note use of LockForConigfuration()/UnlockForConfiguration()
			NSError error = null;
			captureDevice.LockForConfiguration(out error);
			if(error != null)
			{
				Console.WriteLine(error);
				captureDevice.UnlockForConfiguration();
				return false;
			}
			if(UIDevice.CurrentDevice.CheckSystemVersion(7,0))
				captureDevice.ActiveVideoMinFrameDuration = new CMTime (1,15);
			captureDevice.UnlockForConfiguration();

			var input = AVCaptureDeviceInput.FromDevice (captureDevice);

			if (input == null){
				Console.WriteLine ("No input - this won't work on the simulator, try a physical device");
				return false;
			}
			session.AddInput (input);

			// create a VideoDataOutput and add it to the sesion
			var settings = new CVPixelBufferAttributes { 
				PixelFormatType = CVPixelFormatType.CV32BGRA
			};
			using (var output = new AVCaptureVideoDataOutput { WeakVideoSettings = settings.Dictionary }) {
				queue = new CoreFoundation.DispatchQueue ("myQueue");
				outputRecorder = new OutputRecorder ();
				output.SetSampleBufferDelegate (outputRecorder, queue);
				session.AddOutput (output);
			}

			session.StartRunning ();
			return true;
		}

		public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
		{
			public override void DidOutputSampleBuffer (AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
			{
				try {
					var image = ImageFromSampleBuffer (sampleBuffer);
					BeginInvokeOnMainThread (()=> {
						TryDispose(ImageView.Image);
						ImageView.Image = image;
						ImageView.Transform = CGAffineTransform.MakeRotation((float)Math.PI/2);
					});
					session.StopRunning();
				} catch (Exception e){
					Console.WriteLine (e);
				} finally {
					sampleBuffer.Dispose ();
				}
			}

			UIImage ImageFromSampleBuffer (CMSampleBuffer sampleBuffer)
			{
				// Get the CoreVideo image
				using (var pixelBuffer = sampleBuffer.GetImageBuffer () as CVPixelBuffer) {
					// Lock the base address
					pixelBuffer.Lock (0);
					// Get the number of bytes per row for the pixel buffer
					var baseAddress = pixelBuffer.BaseAddress;
					int bytesPerRow = (int) pixelBuffer.BytesPerRow;
					int width = (int) pixelBuffer.Width;
					int height = (int) pixelBuffer.Height;
					var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
					// Create a CGImage on the RGB colorspace from the configured parameter above
					using (var cs = CGColorSpace.CreateDeviceRGB ()) {
						using (var context = new CGBitmapContext (baseAddress, width, height, 8, bytesPerRow, cs, (CGImageAlphaInfo)flags)) {
							using (CGImage cgImage = context.ToImage ()) {
								pixelBuffer.Unlock (0);
								return UIImage.FromImage (cgImage);
							}
						}
					}
				}
			}

			void TryDispose (IDisposable obj)
			{
				if (obj != null)
					obj.Dispose ();
			}
		}


	}
}


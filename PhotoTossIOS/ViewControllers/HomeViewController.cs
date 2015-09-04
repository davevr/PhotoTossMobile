
using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using JVMenuPopover;
using ZXing.Mobile;

using CoreGraphics;
using AVFoundation;
using CoreVideo;
using CoreMedia;

using CoreFoundation;
using System.Runtime.InteropServices;
using PhotoToss.Core;

using Facebook.LoginKit;
using Facebook.CoreKit;
using CoreMotion;
using CoreLocation;


namespace PhotoToss.iOSApp
{
	public partial class HomeViewController : JVMenuViewController, IUICollectionViewDelegate
	{
		public static AVCaptureSession session;
		public static PhotoRecord CurrentPhotoRecord { get; set; }
		public static UIImageView ImageView;
		OutputRecorder outputRecorder;
		DispatchQueue queue;
		private UIView loadingOverlay = null;
		List<string> readPermissions = new List<string> { "public_profile" };
		CMMotionManager motionManager = null;
		private double maxAcc = 0.0001f;
		private double maxSpeed = 0.1f;
		private double maxRot = Math.PI / 4;
		LoginButton loginButton;
		ProfilePictureView pictureView;
		UILabel nameLabel;
		public static string kTossCellName = "TossedImageCell";
		private List<double> dataList = new List<double> ();

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

			// set up collection view
			TossedImageCollectionView.RegisterNibForCell(UINib.FromName("TossedImageCell", NSBundle.MainBundle), kTossCellName);
			TossedImageCollectionView.SetCollectionViewLayout (new UICollectionViewFlowLayout () {
				SectionInset = new UIEdgeInsets (50,16,20,16),
				ItemSize = new CGSize(120,120),
				ScrollDirection = UICollectionViewScrollDirection.Vertical,
				MinimumInteritemSpacing = 8, // minimum spacing between cells
				MinimumLineSpacing = 16 // minimum spacing between rows if ScrollDirection is Vertical or between columns if Horizontal
			}, true);

			TossedImageCollectionView.Delegate = this;

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

			try {
				motionManager = new CMMotionManager();
			}
			catch (Exception ) {
				// no motion manager available
				motionManager = null;
			}


		}

		[Export ("collectionView:didSelectItemAtIndexPath:")]
		public void ItemSelected (UIKit.UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			CurrentPhotoRecord = ((TossedImageDataSource)TossedImageCollectionView.DataSource).GetItem (indexPath);

			ImageViewController imageViewer = new ImageViewController ();
			if (imageViewer != null) {
				
				this.NavigationController.PushViewController (imageViewer, true);
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			if (motionManager != null)
				motionManager.StartAccelerometerUpdates (NSOperationQueue.CurrentQueue, MotionCallback);
			base.ViewDidAppear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			if (motionManager != null)
				motionManager.StopAccelerometerUpdates ();
			base.ViewDidDisappear (animated);
		}

		private void MotionCallback(CMAccelerometerData data, NSError theErr)
		{
			double newRot = -data.Acceleration.X;
			double targetRot = 0;

			dataList.Insert (0, newRot);
			if (dataList.Count > 10)
				dataList.RemoveAt (10);
			
			foreach (double curVal in dataList) {
				targetRot += curVal;
			}
			targetRot /= dataList.Count;
			targetRot = Math.Round(targetRot, 2);

			InvokeOnMainThread (() => {
				foreach (TossedImageCell curCell in TossedImageCollectionView.VisibleCells) 
				//if (TossedImageCollectionView.VisibleCells.Length > 0)
				{
					//TossedImageCell curCell = (TossedImageCell)TossedImageCollectionView.VisibleCells[0];

					if (curCell.Rotation != targetRot)
					{
						if (curCell.Rotation < targetRot) {
							curCell.RotationSpeed += maxAcc;
							if (curCell.RotationSpeed > maxSpeed)
								curCell.RotationSpeed = maxSpeed;
						}
						else {
							curCell.RotationSpeed -= maxAcc;
							if (curCell.RotationSpeed < -maxSpeed)
								curCell.RotationSpeed = -maxSpeed;
						}
						curCell.Rotation += curCell.RotationSpeed;
						if (curCell.Rotation > maxRot) {
							curCell.Rotation = maxRot;
							curCell.RotationSpeed = 0;
						}
						else if (curCell.Rotation < -maxRot) {
							curCell.Rotation = -maxRot;
							curCell.RotationSpeed = 0;
						}
						curCell.Rotation = Math.Round(curCell.Rotation, 2);
						curCell.Rotation = targetRot;
						curCell.Transform = CGAffineTransform.MakeRotation ((nfloat)curCell.Rotation);
					}

				}
			});

		}

		private void CompleteSignin ()
		{
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
							RefreshGrid(() => 
							{
								HideOverlay();
								LocationHelper.StartLocationManager(CoreLocation.CLLocation.AccuracyBest);
								InvokeInBackground(() => {
									while (CLLocationManager.Status == CLAuthorizationStatus.NotDetermined)
									{}
									if (CLLocationManager.Status != CLAuthorizationStatus.AuthorizedWhenInUse)
									{
										InvokeOnMainThread(() => {
											var alertView = new UIAlertView("Error", "PhotoToss will not run without location services.  Please enable the setting and try again.", null, "Ok", null);
											alertView.Clicked += (object sender, UIButtonEventArgs e) => {
												UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
											};
											alertView.Show();
											
										});
									}
									LocationHelper.StopLocationManager();
								});

								
							}); 
					});
				});
			}
		}

		private void RefreshGrid(null_callback callback = null)
		{
			System.Console.WriteLine ("Refreshing Grid!");
			PhotoTossRest.Instance.GetUserImages ((userImageList) => {
				System.Console.WriteLine ("Got %d user images!", userImageList.Count);
				TossedImageDataSource dataSource = new TossedImageDataSource();
				dataSource.photoList = userImageList;
				InvokeOnMainThread(() => {
					TossedImageCollectionView.DataSource = dataSource;
					TossedImageCollectionView.ReloadData();
					if (callback != null)
						callback ();
				});
			});

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

				// Handle your successful login
				RemoveFacebookOverlay();


				CompleteSignin();
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

		private void RemoveFacebookOverlay()
		{
			InvokeOnMainThread (() => {
				loadingOverlay.RemoveFromSuperview();
				loginButton.RemoveFromSuperview();
				pictureView.RemoveFromSuperview();
				nameLabel.RemoveFromSuperview();
			});
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

			PhotoTossRest.Instance.GetUploadURL ((theUrl) => {
				PhotoTossRest.Instance.UploadImage (imageForUploading.AsJPEG ().AsStream (), curLoc.Longitude, curLoc.Latitude, (theRecord) => {

					PhotoTossRest.Instance.GetUploadURL((uploadStr) =>
						{
							UIImage thumbnail = CropImage(imageForUploading, new CGRect(0,0,64,64));

							PhotoTossRest.Instance.UploadImageThumb(thumbnail.AsPNG().AsStream (), theRecord.id, (theStr) =>
									{
										if (!String.IsNullOrEmpty(theStr))
											theRecord.thumbnailurl = theStr;
									
										InvokeOnMainThread(() => {
											((UIImagePickerController)sender).DismissViewController (true, () => {
												});
										});
									});
						});
				});
			});
		}

		private UIImage CropImage(UIImage srcImage, CGRect rect) 
		{ 
			using (CGImage cr = srcImage.CGImage.WithImageInRect (rect)) {
				UIImage cropped = UIImage.FromImage (cr);
				return cropped;
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



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
using CoreImage;


namespace PhotoToss.iOSApp
{
	public delegate void image_callback(UIImage theImage);
	public delegate void stream_callback(System.IO.Stream theStream);

	public partial class HomeViewController : JVMenuViewController, IUICollectionViewDelegate
	{
		public static AVCaptureSession session;

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
		private string catchResult;
		public nfloat kCellSize;


		public HomeViewController () : base ()
		{
			AutomaticallyAdjustsScrollViewInsets = false;

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
			//TossedImageCollectionView.ContentInset = new UIEdgeInsets (-40, 0, 0, 0);

			nfloat marginSize = 24;
			nfloat screenSize = UIScreen.MainScreen.Bounds.Width;
			nfloat availableSize = screenSize - marginSize * 3;
			kCellSize = availableSize / 2;
			if (kCellSize > 155) {
				// must be iPhone 6 plus....
				availableSize = screenSize - marginSize * 4;
				kCellSize = availableSize / 3;
			}


			// set up collection view
			TossedImageCollectionView.RegisterNibForCell(UINib.FromName("TossedImageCell", NSBundle.MainBundle), kTossCellName);
			TossedImageCollectionView.SetCollectionViewLayout (new UICollectionViewFlowLayout () {
				SectionInset = new UIEdgeInsets (marginSize,marginSize,0,marginSize),
				ItemSize = new CGSize(kCellSize, kCellSize),
				ScrollDirection = UICollectionViewScrollDirection.Vertical,
				MinimumInteritemSpacing = marginSize, // minimum spacing between cells
				MinimumLineSpacing = 16 // minimum spacing between rows if ScrollDirection is Vertical or between columns if Horizontal
			}, true);

			TossedImageCollectionView.Delegate = this;

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


			FakeHeader.Layer.ShadowOffset = new CGSize (1, 5);
			FakeHeader.Layer.ShadowColor = new CGColor (0, 0, 0);
			FakeHeader.Layer.ShadowOpacity = 0.5f;

			TossTitle.AttributedText = new NSAttributedString("PhotoToss", UIFont.FromName("RammettoOne-Regular", 17),
				UIColor.FromRGB(255,121,0));

			var catchBtn = new UIBarButtonItem(UIImage.FromBundle("CatchIcon"),UIBarButtonItemStyle.Plain,  (sender, e) => {
				DoCatchPicture();
			});

			var cameraBtn = new UIBarButtonItem(UIImage.FromBundle("CameraIcon"),UIBarButtonItemStyle.Plain,  (sender, e) => {
				DoTakePicture();
			});

			this.NavigationItem.RightBarButtonItems = new UIBarButtonItem[] {catchBtn, cameraBtn};

		}



		[Export ("collectionView:didSelectItemAtIndexPath:")]
		public void ItemSelected (UIKit.UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			PhotoTossRest.Instance.CurrentImage = ((TossedImageDataSource)TossedImageCollectionView.DataSource).GetItem (indexPath);

			ImageViewController imageViewer = new ImageViewController ();
			if (imageViewer != null) {
				imageViewer.ImageDeleted += HandleImageDeleted;
				this.NavigationController.PushViewController (imageViewer, true);
			}
		}

		public void HandleImageDeleted (PhotoRecord deadImage)
		{
			RemoveDeadImage (deadImage);

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
			double newRot = 0;
			double targetRot = 0;

			switch (InterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft:
				newRot = -data.Acceleration.Y;
				break;
			case UIInterfaceOrientation.LandscapeRight:
				newRot = data.Acceleration.Y;
				break;
			case UIInterfaceOrientation.Portrait:
				newRot = -data.Acceleration.X;
				newRot += 0;
				break;
			case UIInterfaceOrientation.PortraitUpsideDown:
				newRot = data.Acceleration.X;
				newRot += Math.PI;
				break;
			}


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
				{
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
						curCell.Rotation = targetRot /2;
						curCell.Transform = CGAffineTransform.MakeRotation ((nfloat)curCell.Rotation);
					}

				}
			});

		}

		private void CompleteSignin ()
		{
			if (AccessToken.CurrentAccessToken != null) {
				ShowOverlay (View, "Connecting to PhotoToss...");
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
						((AppDelegate)UIApplication.SharedApplication.Delegate).RegisterForPushNotifications();
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
				if (userImageList == null)
					userImageList = new List<PhotoRecord>();
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
			ShowOverlay(((UIImagePickerController) sender).View, "Uploading To PhotoToss...");
			LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
			LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
			LocationHelper.StopLocationManager ();

			UIImage imageForUploading =  UIImageHelper.ScaleAndRotateImage(eventArgs.OriginalImage);
			DateTime now = DateTime.Now;

			PhotoTossRest.Instance.GetUploadURL ((theUrl) => {
				
				PhotoTossRest.Instance.UploadImage (imageForUploading.AsJPEG ().AsStream (), curLoc.Longitude, curLoc.Latitude, (theRecord) => {

					PhotoTossRest.Instance.GetUploadURL((uploadStr) =>
						{
							UIImage thumbnail = UIImageHelper.ScaleAndCrop(imageForUploading, 64);

							PhotoTossRest.Instance.UploadImageThumb(thumbnail.AsPNG().AsStream (), theRecord.id, (theStr) =>
									{
										if (!String.IsNullOrEmpty(theStr))
											theRecord.thumbnailurl = theStr;
									
										InvokeOnMainThread(() => {
											((UIImagePickerController)sender).DismissViewController (true, () => {
											if (theRecord != null) 
												AddNewImage(theRecord);
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
			options.CameraResolutionSelector = (resList) => {
				CameraResolution finalRes = null;

				foreach( CameraResolution curRes in resList) {
					if (((curRes.Height == 640) || (curRes.Width == 640)) && finalRes == null)
						finalRes = curRes;
					else if ((curRes.Height == 720) || (curRes.Width == 720))
						finalRes = curRes;
				
				}

				return finalRes;
			};

			var result = await scanner.Scan(options, false);

			if (result != null) {
				catchResult = result.Text;
				FinalizeCatch (result);

			}
		}


			
		public void FinalizeCatch(ZXing.Result theResult)
		{
			InvokeOnMainThread (() => {
				ShowOverlay(View, "Finishing the catch...");
				PhotoTossRest.Instance.GetCatchURL ((urlStr) => {
					LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
					LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
					LocationHelper.StopLocationManager ();
					long tossId = 0;

					try {
						tossId = long.Parse (catchResult.Substring (catchResult.LastIndexOf ("/") + 1));
					} catch (Exception exp) {
						Console.Error.WriteLine("Invalid barcode");
					}
					BarcodeLocation barcodeLoc = new BarcodeLocation();
					barcodeLoc.topleft = new BarcodePoint( theResult.ResultPoints[3].X, theResult.ResultPoints[3].Y);
					barcodeLoc.topright = new BarcodePoint( theResult.ResultPoints[0].X, theResult.ResultPoints[0].Y);
					barcodeLoc.bottomleft = new BarcodePoint( theResult.ResultPoints[2].X, theResult.ResultPoints[2].Y);
					barcodeLoc.bottomright = new BarcodePoint( theResult.ResultPoints[1].X, theResult.ResultPoints[1].Y);

					if (tossId != 0) {
						UIImage catchImage = theResult.CaptureImage as UIImage;
						PhotoTossRest.Instance.CatchToss (catchImage.AsJPEG().AsStream (), tossId, curLoc.Longitude, curLoc.Latitude, barcodeLoc, (newRec) => 
							{
								InvokeOnMainThread(() => 
									{
										HideOverlay();
										if (newRec != null) 
											AddNewImage(newRec);
										else 
											new UIAlertView("Error", "Catch failed.  Please try again.", null, "dang", null).Show();
									});

							});
					} else {
						InvokeOnMainThread(() => 
							{
								HideOverlay();
								new UIAlertView("Error", "Catch failed.  Invalid Barcode.", null, "dang", null).Show();
							});
					}
				});

			});
		}

		private void AddNewImage(PhotoRecord newRec)
		{
			((TossedImageDataSource)TossedImageCollectionView.DataSource).photoList.Insert (0, newRec);
			TossedImageCollectionView.ReloadData ();
			NSIndexPath path = NSIndexPath.FromRowSection (0, 0);
			TossedImageCollectionView.ScrollToItem (path, UICollectionViewScrollPosition.Top, true);
		}

		private void RemoveDeadImage(PhotoRecord newRec)
		{
			((TossedImageDataSource)TossedImageCollectionView.DataSource).photoList.Remove (newRec);
			TossedImageCollectionView.ReloadData ();
			NSIndexPath path = NSIndexPath.FromRowSection (0, 0);
			TossedImageCollectionView.ScrollToItem (path, UICollectionViewScrollPosition.Top, true);
		}


		private static void TryDispose (IDisposable obj)
		{
			if (obj != null)
				obj.Dispose ();
		}



	}
}


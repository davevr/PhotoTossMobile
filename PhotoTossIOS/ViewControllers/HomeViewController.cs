
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

namespace PhotoToss.iOSApp
{
	public partial class HomeViewController : JVMenuViewController
	{
		public static AVCaptureSession session;
		public static UIImageView ImageView;
		OutputRecorder outputRecorder;
		DispatchQueue queue;

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
			new UIAlertView ("Photo Taken!", "This is where the upload progress bar would go", null, "Ok", null).Show ();
			/*
			UIImage image = imageForUploading =  UIImageHelper.ScaleAndRotateImage(eventArgs.OriginalImage);
			DateTime now = DateTime.Now;
			string imageName = String.Format ("{0}_{1}.jpg", now.ToLongDateString(), BlahguaAPIObject.Current.CurrentUser.UserName);
			BlahguaAPIObject.Current.UploadPhoto (image.AsJPEG ().AsStream (), imageName, ImageUploaded);
			*/
			((UIImagePickerController) sender).DismissViewController(true,  () => {});
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


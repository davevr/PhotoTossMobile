
using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using MapKit;
using CoreLocation;
using CoreGraphics;
using PhotoToss.Core;
using Google.Maps;
using SDWebImage;

namespace PhotoToss.iOSApp
{
	public partial class ImageSpreadViewController : UIViewController
	{
		private MapView mapView;
		private MapDelegate mapDelegate;
		private UIView infoWindowView = null;
		private UIImageView catchImageView;
		private UIImageView userImageView;
		private UILabel titleView;
		private UILabel dateView;
		private Dictionary<string, PhotoRecord> markerMap = new Dictionary<string, PhotoRecord>();
		private CoordinateBounds markerBounds;
		private bool isUpdated = false;

		public ImageSpreadViewController () : base ("ImageSpreadViewController", null)
		{
		}

		public class PhotoWrapper : NSObject
		{
			public PhotoRecord record { get; set;}
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			isUpdated = false;
			CGRect bounds = View.Frame;
			bounds.Height -= 100;

			View.Frame = bounds;
			View.BackgroundColor = UIColor.White;
			//View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			SetupMap();
			//Setup button to clear map
			var btn = new UIButton(new CGRect(16, UIScreen.MainScreen.Bounds.Bottom - 152, 100, 30));
			btn.Layer.BorderWidth = 1;
			btn.Layer.CornerRadius = 0;
			btn.SetTitleColor (UIColor.Blue, UIControlState.Normal);
			btn.Layer.BorderColor = UIColor.Black.CGColor;
			btn.SetTitle(@"Show Me", UIControlState.Normal);
			btn.BackgroundColor = UIColor.White;
			btn.TouchUpInside += (s, e) => {
				ShowMe ();
			};
				
			var btn2= new UIButton(new CGRect(UIScreen.MainScreen.Bounds.Width - (100 + 16),  UIScreen.MainScreen.Bounds.Bottom - 152, 100, 30));
			btn2.Layer.BorderWidth = 1;
			btn2.Layer.CornerRadius = 0;
			btn2.Layer.BorderColor = UIColor.Black.CGColor;
			btn2.SetTitleColor (UIColor.Blue, UIControlState.Normal);
			btn2.SetTitle(@"Show All", UIControlState.Normal);
			btn2.BackgroundColor = UIColor.White;
			btn2.TouchUpInside += (s, e) =>
			{
				ShowAll();
			};


			mapView.AddSubview(btn);
			mapView.AddSubview(btn2);
			CreateInfoWindow();
			DoInitialUpdate ();
		}

		private void AddMarker(PhotoRecord theRec, string title)
		{
			UIColor theHue;
			if (theRec.tossid != 0) {
				theHue = UIColor.Green;
			}
			else
			{
				theHue = UIColor.Red;
			}
			if (theRec == HomeViewController.CurrentPhotoRecord)
				theHue = UIColor.Blue;

			CLLocationCoordinate2D newLoc = GetPhotoLocation (theRec);

			if (mapView != null) {
				InvokeOnMainThread(() =>
					{
						var marker = Marker.FromPosition (newLoc);

						marker.Title = title;
						marker.Icon = Marker.MarkerImage (theHue);
						marker.UserData = new PhotoWrapper () { record = theRec };

						marker.Map = mapView;
					});
				markerBounds = markerBounds.Including (newLoc);
				CacheImages(theRec);
			}

		}

		private void CacheImages(PhotoRecord theRec)
		{
			if ((theRec.tosserid != 0) && (theRec.tosserid != PhotoTossRest.Instance.CurrentUser.id))
			{
				string userImageUrl = PhotoTossRest.Instance.GetUserProfileImage(theRec.tossername);
				SDWebImageManager.SharedManager.Download (new NSUrl (userImageUrl), SDWebImageOptions.HighPriority, 
					progressHandler: (recievedSize, expectedSize) => {
					// Track progress...
				},
					completedHandler: (image, error, cacheType, finished, imageURL) => {
						if (image != null) {
							// do something with the image
						}
					});
			}

			string imageUrl;


			if (theRec.tossid == 0)
			{
				imageUrl = theRec.imageUrl;
			}
			else
			{
				imageUrl = theRec.catchUrl;
			}

			imageUrl += "=s128-c";
			SDWebImageManager.SharedManager.Download (new NSUrl (imageUrl), SDWebImageOptions.HighPriority, 
				progressHandler: (recievedSize, expectedSize) => {
					// Track progress...
				},
				completedHandler: (image, error, cacheType, finished, imageURL) => {
					if (image != null) {
						// do something with the image
					}
				});

		}

		private CLLocationCoordinate2D GetPhotoLocation(PhotoRecord theRec)
		{
			double latLoc, longLoc;

			if (theRec.tossid != 0) {
				latLoc = theRec.receivedlat;
				longLoc = theRec.receivedlong;
			}
			else
			{
				latLoc = theRec.createdlat;
				longLoc = theRec.createdlong;
			}

			if (latLoc < -90)
				latLoc = 0;
			else if (latLoc > 90)
				latLoc = 0;

			if (longLoc < -180)
				longLoc = -180;
			else if (longLoc > 0)
				longLoc = 0;

			CLLocationCoordinate2D newLoc = new CLLocationCoordinate2D (latLoc, longLoc);

			return newLoc;
		}

		private void DoInitialUpdate()
		{
			if (isUpdated)
				return;
			isUpdated = true;
			CLLocationCoordinate2D baseMap = GetPhotoLocation(HomeViewController.CurrentPhotoRecord);
			CLLocationCoordinate2D southWest = new CLLocationCoordinate2D(baseMap.Latitude - 0.01, baseMap.Longitude - 0.01);
			CLLocationCoordinate2D northEast = new CLLocationCoordinate2D(baseMap.Latitude + 0.01, baseMap.Longitude + 0.01);
			markerBounds = new CoordinateBounds(southWest, northEast);
			AddMarker (HomeViewController.CurrentPhotoRecord, "your image");

			if (HomeViewController.CurrentPhotoRecord.tossid != 0) {
				PhotoTossRest.Instance.GetImageLineage (HomeViewController.CurrentPhotoRecord.id, (parentList) => {
					if ((parentList != null) && (parentList.Count > 0))
					{
						MutablePath newPath = new MutablePath();
						newPath.AddCoordinate(GetPhotoLocation(HomeViewController.CurrentPhotoRecord));
						int parentCount = 1;
						string parentString;

						foreach(PhotoRecord curImage in parentList)
						{
							CLLocationCoordinate2D curLoc = GetPhotoLocation(curImage);
							newPath.AddCoordinate(curLoc);
							if (parentCount == parentList.Count)
								parentString = "original image";
							else
								parentString = "parent #" + parentCount++;

							AddMarker(curImage, parentString);

						}

						InvokeOnMainThread(() =>
							{
								Polyline newLine = Polyline.FromPath(newPath);
								newLine.StrokeWidth = 5;
								newLine.StrokeColor = UIColor.Green;
								newLine.Map = mapView;
								ShowAll();
							});
					}
				});
			}
			else
			{
				ShowAll();
			}

			ShowImageChildren(HomeViewController.CurrentPhotoRecord);

		}

		private void ShowImageChildren(PhotoRecord parentPhoto)
		{
			int catchCount = 1;
			PhotoTossRest.Instance.GetImageTosses(parentPhoto.id, (tossList) =>
				{
					if ((tossList != null) && (tossList.Count > 0))
					{
						foreach (TossRecord curToss in tossList)
						{
							PhotoTossRest.Instance.GetTossCatches(curToss.id, (catchList) =>
								{
									if ((catchList != null) && (catchList.Count > 0))
									{
										CLLocationCoordinate2D parentLoc = GetPhotoLocation(parentPhoto);
										foreach (PhotoRecord curCatch in catchList)
										{
											CLLocationCoordinate2D catchLoc = GetPhotoLocation(curCatch);
											MutablePath newPath = new MutablePath();
											newPath.AddCoordinate(parentLoc);
											newPath.AddCoordinate(catchLoc);

											// add a line from the parent to this catch
											InvokeOnMainThread(() =>
												{
													Polyline newLine = Polyline.FromPath(newPath);
													newLine.StrokeWidth = 5;
													newLine.StrokeColor = UIColor.Red;
													newLine.Map = mapView;
												});
											// add a marker
											string markerTitle = String.Format("catch #{0}", catchCount++);
											AddMarker(curCatch, markerTitle);
											ShowImageChildren(curCatch);
										}
									}

								});
						}

					}
				});
		}

		public UIView GetInfoContents(Marker theMarker)
		{
			if (infoWindowView != null)
			{
				titleView.Text = theMarker.Title;
				PhotoRecord markerRecord = ((PhotoWrapper)theMarker.UserData).record; // to do - look up actual record
				long tosserId = markerRecord.tosserid;
				string tosserName;

				if (tosserId == 0)
					tosserName = markerRecord.ownername;
				else
					tosserName = markerRecord.tossername;

				string userImageUrl = PhotoTossRest.Instance.GetUserProfileImage(markerRecord.tossername);

				userImageView.SetImage(new NSUrl(userImageUrl), UIImage.FromBundle ("unknownperson"));

				string imageUrl;

				if (markerRecord.tossid == 0)
					imageUrl = markerRecord.imageUrl;
				else
					imageUrl = markerRecord.catchUrl;

				catchImageView.SetImage(new NSUrl(imageUrl + "=s128-c"), UIImage.FromBundle("placeholder"));
				infoWindowView.Bounds = new CGRect (0, 0, 136, 168);
				userImageView.Layer.CornerRadius = 16;

			}
			return infoWindowView;
		}

		private void ShowAll()
		{
			if (mapView != null)
			{
				CameraUpdate.FitBounds (markerBounds);
				CameraPosition pos = mapView.CameraForBounds (markerBounds, UIEdgeInsets.Zero);
				InvokeOnMainThread (() => {
					mapView.Animate(pos);
				});
			}
		}

		private void ShowMe()
		{
			if (mapView != null)
			{
				CLLocationCoordinate2D pos = mapView.MyLocation.Coordinate;
				InvokeOnMainThread (() => {
					mapView.Animate(pos);
				});
			}
		}

		private void SetupMap()
		{
			//Init Map wiht Camera
			var camera = new CameraPosition(new CLLocationCoordinate2D(36.069082, -94.155976), 15, 30, 0);
			mapView = MapView.FromCamera(CGRect.Empty, camera); 
			mapView.MyLocationEnabled = true;
			//Add button to zoom to location
			mapView.Settings.MyLocationButton = true;
			mapView.MapType = MapViewType.Normal;
			mapView.Settings.SetAllGesturesEnabled(true);
			//Init MapDelegate
			mapDelegate = new MapDelegate() {controller = this};
			mapView.Delegate = mapDelegate;
			View = mapView;
		}

		private void CreateInfoWindow()
		{
			infoWindowView = new UIView (new CGRect (0, 0, 136, 168));
			catchImageView = new UIImageView (new CGRect (0, 32, 128, 128));
			userImageView = new UIImageView (new CGRect (104, 136, 32, 32));
			userImageView.Layer.CornerRadius = 16;
			titleView = new UILabel (new CGRect (0, 0, 128, 16));
			titleView.Font = UIFont.BoldSystemFontOfSize (10);
			dateView = new UILabel (new CGRect (0, 16, 128, 16));
			dateView.Font = UIFont.ItalicSystemFontOfSize (8);
			infoWindowView.AddSubviews (new UIView[] { catchImageView, userImageView, titleView, dateView });
			titleView.Text = "this is the title";
			titleView.TextAlignment = UITextAlignment.Center;
			dateView.Text = "the date";
			dateView.TextAlignment = UITextAlignment.Left;
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public class MapDelegate : MapViewDelegate
		{
			public ImageSpreadViewController controller { get; set;}

			public List<CLLocationCoordinate2D>	Locations = new List<CLLocationCoordinate2D>();

			public override void DidTapInfoWindowOfMarker(MapView mapView, Marker marker)
			{
				ImageLineageViewController imageViewer = new ImageLineageViewController ();
				if (imageViewer != null) {
					PhotoRecord markerRecord = ((PhotoWrapper)marker.UserData).record; 
					imageViewer.CurrentMarkerRecord = markerRecord;
					this.controller.NavigationController.PushViewController (imageViewer, true);
				}
			}

			public override UIView MarkerInfoContents(MapView mapView, Marker marker)
			{
				return controller.GetInfoContents (marker);
			}

			public void Clear()
			{
				Locations.Clear ();
			}

		}
	}
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using PhotoToss.Core;

namespace PhotoToss.AndroidApp
{
	public class ImageViewSpreadFragment : Android.Support.V4.App.Fragment, Koush.IUrlImageViewCallback, IOnMapReadyCallback, GoogleMap.IInfoWindowAdapter, GoogleMap.IOnInfoWindowClickListener
	{
		private GoogleMap map;
		private Button showMeBtn;
		private Button showAllBtn;
		private bool isUpdated { get; set;}
		private PhotoRecord curRec;
		private LatLngBounds markerBounds;
        private View infoWindowView = null;
        private ImageView catchImageView;
        private ImageView userImageView;
        private TextView titleView;
        private TextView dateView;
        private Dictionary<string, PhotoRecord> markerMap = new Dictionary<string, PhotoRecord>();

		public override void OnCreate (Bundle savedInstanceState)
		{
			isUpdated = false;
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewSpreadFragment, null);
            var mapFragment = (SupportMapFragment) ChildFragmentManager.FindFragmentById(Resource.Id.map);
			showMeBtn = fragment.FindViewById<Button> (Resource.Id.showMeBtn);
			showAllBtn = fragment.FindViewById<Button> (Resource.Id.showAllBtn);
			mapFragment.GetMapAsync (this);
			curRec = PhotoTossRest.Instance.CurrentImage;

			showAllBtn.Click += (object sender, EventArgs e) => 
			{
                ShowAll();

			};

            CreateInfoWindow();

			return fragment;
		}

        private void CreateInfoWindow()
        {
            infoWindowView = GetLayoutInflater(null).Inflate(Resource.Layout.MapInfoView, null);
            catchImageView = infoWindowView.FindViewById<ImageView>(Resource.Id.tossImage);
            userImageView = infoWindowView.FindViewById<ImageView>(Resource.Id.userImage);
            titleView = infoWindowView.FindViewById<TextView>(Resource.Id.tossTitleView);
            dateView = infoWindowView.FindViewById<TextView>(Resource.Id.tossDateView);

        }

        private void ShowAll()
        {
            if (map != null)
            {

                CameraUpdate theUpdate = CameraUpdateFactory.NewLatLngBounds(markerBounds, 0);
                Activity.RunOnUiThread(() =>
                {
                    map.AnimateCamera(theUpdate);
                });
            }
        }

        private void ShowMe()
        {
            if (map != null)
            {
                Activity.RunOnUiThread(() =>
                {
                    LatLng theLoc = new LatLng(MainActivity._lastLocation.Latitude, MainActivity._lastLocation.Longitude);
                    CameraPosition thePos = new CameraPosition(theLoc, 16, 0, 0);
                    CameraUpdate theUpdate = CameraUpdateFactory.NewCameraPosition(thePos);

                    map.AnimateCamera(theUpdate);
                });
            }
        }

		public void OnMapReady (GoogleMap googleMap)
		{
			map = googleMap;
            map.SetInfoWindowAdapter(this);
            map.SetOnInfoWindowClickListener(this);
			LatLng theLoc = new LatLng (MainActivity._lastLocation.Latitude, MainActivity._lastLocation.Longitude);
			CameraPosition thePos = new CameraPosition (theLoc, 16, 0, 0);
			CameraUpdate theUpdate = CameraUpdateFactory.NewCameraPosition (thePos);

			map.AnimateCamera (theUpdate);
		}

		public void Update()
		{
			if (!isUpdated)
				DoInitialUpdate ();
		}

		private void AddMarker(PhotoRecord theRec, string title)
		{
			float theHue;
			if (theRec.tossid != 0) {
				theHue = BitmapDescriptorFactory.HueGreen;
			}
			else
			{
				theHue = BitmapDescriptorFactory.HueRed;
			}
			if (theRec == curRec)
				theHue = BitmapDescriptorFactory.HueBlue;

			LatLng newLoc = GetPhotoLocation (theRec);

			if (map != null) {
                System.Console.WriteLine(string.Format("{0},{1}", newLoc.Latitude, newLoc.Longitude));
				MarkerOptions markerOpt1 = new MarkerOptions();
				markerOpt1.SetPosition(newLoc);
				markerOpt1.SetTitle(title);
                markerOpt1.SetIcon (BitmapDescriptorFactory.DefaultMarker (theHue));
                Activity.RunOnUiThread(() =>
                {
                    Marker newMarker = map.AddMarker(markerOpt1);
                    markerMap[newMarker.Id] = theRec;
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
                Koush.UrlImageViewHelper.LoadUrlDrawable(Activity, userImageUrl);
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
            Koush.UrlImageViewHelper.LoadUrlDrawable(Activity, imageUrl);
        }

		private LatLng GetPhotoLocation(PhotoRecord theRec)
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

			LatLng newLoc = new LatLng (latLoc, longLoc);

			return newLoc;
		}

		private void DoInitialUpdate()
		{
            if (isUpdated)
                return;
            isUpdated = true;
            LatLng baseMap = GetPhotoLocation(curRec);
            LatLng southWest = new LatLng(baseMap.Latitude - 0.01, baseMap.Longitude - 0.01);
            LatLng northEast = new LatLng(baseMap.Latitude + 0.01, baseMap.Longitude + 0.01);
            markerBounds = new LatLngBounds(southWest, northEast);
			AddMarker (curRec, "your image");

			if (curRec.tossid != 0) {
				PhotoTossRest.Instance.GetImageLineage (curRec.id, (parentList) => {
					if ((parentList != null) && (parentList.Count > 0))
					{
                        PolylineOptions polyOptions = new PolylineOptions().Visible(true).InvokeWidth(10).InvokeColor(Android.Graphics.Color.Green);
						polyOptions.Add(GetPhotoLocation(curRec));
                        int parentCount = 1;
						string parentString;

						foreach(PhotoRecord curImage in parentList)
						{
                            LatLng curLoc = GetPhotoLocation(curImage);
                            polyOptions.Add(curLoc);
							if (parentCount == parentList.Count)
								parentString = "original image";
							else
								parentString = "parent #" + parentCount++;

                            AddMarker(curImage, parentString);

						}

                        Activity.RunOnUiThread(() =>
                        {
                            map.AddPolyline(polyOptions);
                            ShowAll();
                        });
					}
				});
			}
            else
            {
                ShowAll();
            }

            ShowImageChildren(curRec);

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
                                LatLng parentLoc = GetPhotoLocation(parentPhoto);
                                foreach (PhotoRecord curCatch in catchList)
                                {
                                    LatLng catchLoc = GetPhotoLocation(curCatch);
                                    // add a line from the parent to this catch
                                    PolylineOptions polyOptions = new PolylineOptions().Visible(true).InvokeWidth(10).InvokeColor(Android.Graphics.Color.Red);
                                    polyOptions.Add(parentLoc);
                                    polyOptions.Add(catchLoc);
                                    Activity.RunOnUiThread(() =>
                                    {
                                        map.AddPolyline(polyOptions);
                                    });
                                    // add a marker
                                    string markerTitle = String.Format("catch #{0}", catchCount++);
                                    System.Console.WriteLine(markerTitle);
                                    AddMarker(curCatch, markerTitle);
                                    ShowImageChildren(curCatch);
                                }
                            }

                        });
                    }

                }
            });
        }

        public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
        {
            SetRoundImage(theImage, theBitmap);
        }

        private void SetRoundImage(ImageView theImage, Bitmap theBitmap)
        {
            Activity.RunOnUiThread(() => {
                CircleDrawable myCircle = new CircleDrawable(theBitmap);
                theImage.SetImageDrawable(myCircle);
            });
        }

        public View GetInfoContents(Marker theMarker)
        {
            if (infoWindowView != null)
            {
                titleView.Text = theMarker.Title;
                PhotoRecord markerRecord = markerMap[theMarker.Id]; // to do - look up actual record
                long tosserId = markerRecord.tosserid;
                string tosserName;

                if (tosserId == 0)
                    tosserName = markerRecord.ownername;
                else
                    tosserName = markerRecord.tossername;

                string userImageUrl = PhotoTossRest.Instance.GetUserProfileImage(markerRecord.tossername);



                var theObj = Koush.DrawableCache.Instance.Get(userImageUrl);

                if (theObj == null)
                    Koush.UrlImageViewHelper.SetUrlDrawable(userImageView, userImageUrl, Resource.Drawable.unknown_octopus, this);
                else
                {
                    Koush.UrlImageViewHelper.ZombieDrawable drawable = (Koush.UrlImageViewHelper.ZombieDrawable)theObj;
                    SetRoundImage(userImageView, drawable.Bitmap);
                }
                string imageUrl;

                if (markerRecord.tossid == 0)
                    imageUrl = markerRecord.imageUrl;
                else
                    imageUrl = markerRecord.catchUrl;

                Koush.UrlImageViewHelper.SetUrlDrawable(catchImageView, imageUrl + "=s128-c", Resource.Drawable.ic_camera);

            }
            return infoWindowView;
        }

        public View GetInfoWindow(Marker theMarker)
        {
            // use the default view
            return null;
        }

        public void OnInfoWindowClick(Marker theMarker)
        {

        }

    }

  
}


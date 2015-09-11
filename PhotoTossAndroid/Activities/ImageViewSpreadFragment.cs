
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

using PhotoToss.Core;

namespace PhotoToss.AndroidApp
{
	public class ImageViewSpreadFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
	{
		private GoogleMap map;
		private Button showMeBtn;
		private Button showAllBtn;
		private ListView spreadList;
		private bool isUpdated { get; set;}
		private PhotoRecord curRec;
		private LatLngBounds markerBounds;

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
			spreadList = fragment.FindViewById<ListView> (Resource.Id.spreadList);
			mapFragment.GetMapAsync (this);
			curRec = PhotoTossRest.Instance.CurrentImage;

			showAllBtn.Click += (object sender, EventArgs e) => 
			{
				if (map != null) {

					CameraUpdate theUpdate = CameraUpdateFactory.NewLatLngBounds(markerBounds, 0);
					map.AnimateCamera (theUpdate);
				}

			};

			return fragment;
		}

		public void OnMapReady (GoogleMap googleMap)
		{
			map = googleMap;
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
			if (curRec.tossid != 0) {
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
				MarkerOptions markerOpt1 = new MarkerOptions();
				markerOpt1.SetPosition(newLoc);
				markerOpt1.SetTitle(title);
				//markerOpt1.SetIcon (BitmapDescriptorFactory.DefaultMarker (theHue));
				map.AddMarker (markerOpt1);
				markerBounds = markerBounds.Including (newLoc);
			}

		}

		private LatLng GetPhotoLocation(PhotoRecord theRec)
		{
			double latLoc, longLoc;

			if (curRec.tossid != 0) {
				latLoc = curRec.receivedlat;
				longLoc = curRec.receivedlong;
			}
			else
			{
				latLoc = curRec.createdlat;
				longLoc = curRec.createdlong;
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
			LatLng newLoc = GetPhotoLocation (curRec);
			markerBounds = new LatLngBounds (newLoc, newLoc);

			AddMarker (curRec, "your image");

			if (curRec.tossid != 0) {
				PhotoTossRest.Instance.GetImageLineage (curRec.id, (parentList) => {
					if ((parentList != null) && (parentList.Count > 0))
					{
						PolylineOptions polyOptions = new PolylineOptions();
						polyOptions.Add(GetPhotoLocation(curRec));
						int parentCount = 1;
						string parentString;

						foreach(PhotoRecord curImage in parentList)
						{
							polyOptions.Add(GetPhotoLocation(curImage));
							if (parentCount == parentList.Count)
								parentString = "original image";
							else
								parentString = "parent #" + parentCount++;
							AddMarker(curImage, parentString);

						}
						//polyOptions.InvokeColor(Android.Graphics.Color.Red.ToArgb());
						map.AddPolyline(polyOptions);

					}

				});
			}

		}


	}
}


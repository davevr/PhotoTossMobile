using System;
using Foundation;
using CoreLocation;

namespace PhotoToss.iOSApp
{
	public class LocationHelper
	{
		private static bool _isTracking;
		public static bool IsTracking { get { return _isTracking; } }
		private static string _longitude;
		private static string _latitude;
		private static DateTime _lastUpdated;

		public static event EventHandler LocationUpdated;

		public static CLLocationManager LocationManager { private set; get; }

		public static void StartLocationManager(double accuracy)
		{
			LocationManager = new CLLocationManager();
			if (LocationManager.RespondsToSelector(new ObjCRuntime.Selector("requestWhenInUseAuthorization")))
				LocationManager.RequestWhenInUseAuthorization();

			LocationManager.DistanceFilter = CLLocationDistance.FilterNone;
			LocationManager.DesiredAccuracy = accuracy;
			UpdateLocation(LocationManager.Location);
			LocationManager.LocationsUpdated += LocationManager_LocationsUpdated;
			LocationManager.StartUpdatingLocation();



			_isTracking = true;

			System.Diagnostics.Debug.WriteLine("Location manager started ");
		}

		public static void StopLocationManager()
		{
			if (LocationManager != null)
			{
				LocationManager.LocationsUpdated -= LocationManager_LocationsUpdated;
				LocationManager = null;
				_isTracking = false;
			}

		}

		public static void Refresh()
		{
			LocationManager.StopUpdatingLocation();
			LocationManager.StartUpdatingLocation();
		}

		private static void LocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
		{
			if (LocationUpdated != null)
				LocationUpdated(null, null);

			UpdateLocation(e.Locations[e.Locations.Length - 1]);
		}

		private static void UpdateLocation(CLLocation location)
		{
			try {
			_longitude = location.Coordinate.Longitude.ToString();
			_latitude = location.Coordinate.Latitude.ToString();
			}
			catch (Exception exp) {
				_longitude = "0";
				_latitude = "0";
			}
			_lastUpdated = DateTime.Now;           
		}

		public static LocationResult GetLocationResult()
		{
			return new LocationResult(_latitude, _longitude, _lastUpdated);
		}

		public class LocationResult
		{
			public DateTime UpdatedTime { private set; get; }
			public double Latitude { private set; get; }
			public double Longitude { private set; get; }

			public LocationResult(string latitude, string longitude, DateTime updated)
			{
				UpdatedTime = updated;
				Latitude = Double.Parse(latitude);
				Longitude = Double.Parse(longitude);
			}

		}
	}
}


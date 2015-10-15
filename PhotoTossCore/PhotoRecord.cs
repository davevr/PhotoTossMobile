using System;
using System.Collections.Generic;


namespace PhotoToss.Core
{
    public class PhotoRecord
    {
		// unique to every photo
        public long id {get; set;}
		public string ownername { get; set; }
		public long ownerid { get; set; }

		// copied from source image
		public string caption { get; set; }
		public List<string> tags { get; set; }
		public DateTime created { get; set; }
		public double createdlat { get; set; }
		public double createdlong { get; set; }
		public string imageUrl { get; set; }
		public string thumbnailurl { get; set; }


		// completed on a toss of a new image
		public long originid { get; set; }
		public long parentid { get; set;}
		public string catchUrl { get; set; }
		public double receivedlong { get; set; }
		public double receivedlat { get; set; }
		public string receivedcaption { get; set; }
		public DateTime received { get; set; }
		public long tosserid { get; set; }
        public string tossername { get; set; }
		public long tossid { get; set; }
		public BarcodeLocation barcodelocation { get; set; }

		// updated after a toss
        public long totalshares { get; set; }
		public int tossCount { get; set; }
		public DateTime lastshared { get; set; }

        private object cachedImage = null;
        private object cachedCatchImage = null;


		public List<TossRecord> tossList {get; set;}

        

        public object CachedImage
        {
            get { return cachedImage; }
            set { cachedImage = value; }
        }

        public object CachedCatchImage
        {
            get { return cachedCatchImage; }
            set { cachedCatchImage = value; }
        }

		public string ShareURL {
			get {
				return string.Format ("http://phototoss-server-01.appspot.com/image/{0}", this.id.ToString ());
			}
		}

		public double DistanceFromHome {
			get {
				return GetGeoDistance (createdlat, createdlong, receivedlat, receivedlong, 'M');
			}
		}

		private double GetGeoDistance(double lat1, double lon1, double lat2, double lon2, char unit) {
			double theta = lon1 - lon2;
			double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
			dist = Math.Acos(dist);
			dist = rad2deg(dist);
			dist = dist * 60 * 1.1515;
			if (unit == 'K') {
				dist = dist * 1.609344;
			} else if (unit == 'N') {
				dist = dist * 0.8684;
			}
			return (dist);
		}

		private double deg2rad(double deg) {
			return (deg * Math.PI / 180.0);
		}


		private double rad2deg(double rad) {
			return (rad / Math.PI * 180.0);
		}


    }

	public class ImageStatsRecord 
	{
		public long imageid { get; set;}
		public int numcopies { get; set;}
		public int numtosses { get; set;}
		public int numparents { get; set;}
		public int numchildren { get; set;}
	}

	public class BarcodeLocation 
	{
		public BarcodePoint topleft {get; set;}
		public BarcodePoint topright {get; set;}
		public BarcodePoint bottomleft {get; set;}
		public BarcodePoint bottomright {get; set;}


	}

	public struct BarcodePoint
	{
		public float x {get; set;}
		public float y { get; set; }

		public BarcodePoint(float xLoc, float yLoc)
		{
			x = xLoc;
			y = yLoc;
		}
	}

   

}
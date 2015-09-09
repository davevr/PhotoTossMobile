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


		// updated after a toss
        public long totalshares { get; set; }
		public int tossCount { get; set; }
		public DateTime lastshared { get; set; }

        private object cachedImage = null;
        private object cachedCatchImage = null;

		public List<TossRecord> tossList {get; set;}

        public static PhotoRecord MakeSample()
        {
            PhotoRecord newRec = new PhotoRecord();
            newRec.id = 0L;
            newRec.caption = "some image";
            newRec.totalshares = 1000;
            newRec.ownerid = 0;
            newRec.ownername = "davevr";
            newRec.tags = new List<string>() { "sheep", "nose", "fred" };
            newRec.received = DateTime.Now;
            newRec.created = DateTime.Now;
            newRec.lastshared = DateTime.Now;
            newRec.createdlat = 34.0824;
            newRec.createdlong = -118.3941;
            newRec.receivedlat = 34.0824;
            newRec.receivedlong = -118.3941;
            newRec.receivedcaption = "cool share";
            newRec.imageUrl = "http://lh6.ggpht.com/mmXPSLXJbkXflBYX525inqAmT93u409QyD9KJgkvEyvPhCNxwbiZIhDG-KTTVvP39Z0G88AmHcLk50S81wHy6us7x3a7JFQo9A";
            newRec.catchUrl = "http://lh6.ggpht.com/mmXPSLXJbkXflBYX525inqAmT93u409QyD9KJgkvEyvPhCNxwbiZIhDG-KTTVvP39Z0G88AmHcLk50S81wHy6us7x3a7JFQo9A";
            

            return newRec;

        }

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

    }

   

}
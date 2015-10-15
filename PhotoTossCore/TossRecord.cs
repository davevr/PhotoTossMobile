using System;
using System.Collections.Generic;

namespace PhotoToss.Core
{
	public class TossRecord
	{
		public long id { get; set; }
		public long ownerId { get; set; }
		public long imageId{ get; set; }
		public int gameType { get; set; }
		public long catchCount { get; set; }
		public DateTime shareTime { get; set; }
		public double shareLong { get; set; }
		public double shareLat { get; set; }
		public List<PhotoRecord> catchList { get; set; }
		public BarcodeLocation barcodelocation { get; set; }

		public TossRecord ()
		{
			catchList = null;
		}
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
	}
}


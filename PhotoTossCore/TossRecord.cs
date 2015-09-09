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


		public TossRecord ()
		{
			catchList = null;
		}
	}
}


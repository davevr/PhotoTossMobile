using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoToss.Core
{
	public class PresenceMessage
	{
		public string action { get; set; }
		public int timestamp { get; set; }
		public string uuid { get; set; }
		public int occupancy { get; set; }
	}

	public class ChatTurn
	{
		public string text { get; set; }
		public string image { get; set; }
		public string userimage { get; set; }
		public long userid {get; set;}
		public bool sameUser { get; set;}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoToss.Core
{
    public class User
    {
        public long id { get; set; }
        public string username { get; set; }
		public string nickname {get; set;}
        public string imageurl { get; set; }
        public DateTime lastActiveDate { get; set; }
        public bool signedon { get; set; }


        public static User MakeSample()
        {
            User newRec = new User();

            newRec.id = 0;
            newRec.username = "davevr";
            newRec.imageurl = "https://s3-us-west-2.amazonaws.com/blahguaimages/image/54aae1b1e4b07c9835243427-A.jpg";

            return newRec;
        }

    }
}

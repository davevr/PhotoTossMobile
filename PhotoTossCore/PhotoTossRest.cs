using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using RestSharp;
using System.Runtime.Serialization;
using ServiceStack.Text;
using System.Threading.Tasks;
#if ANDROID
using Xamarin.Facebook;
#elif IOS
using Facebook;
#endif

namespace PhotoToss.Core
{
    public delegate void PhotoRecordList_callback(List<PhotoRecord> theResult);
	public delegate void TossList_classback(List<TossRecord> theResult);
    public delegate void PhotoRecord_callback(PhotoRecord theResult);
    public delegate void User_callback(User theResult);
    public delegate void String_callback(String theResult);
    public delegate void Toss_callback(TossRecord theResult);
	public delegate void null_callback();

    public class PhotoTossRest
    {
        private RestClient apiClient;
        private static PhotoTossRest _singleton = null;
        private string apiPath = "http://phototoss-server-01.appspot.com/api/";  //"http://localhost:8080/api/";  //"http://phototoss-server-01.appspot.com/api/";//"http://127.0.0.1:8080/api/"; //"http://phototoss-server-01.appspot.com/api/";//"http://www.photostore.com/api/";
        //private Random rndBase = new Random();
        private string _uploadURL;
        private string _catchURL;
        private string _userImageURL;
        private User _currentUser = null;
        public PhotoRecord CurrentImage { get; set; }
        
        public PhotoTossRest()
        {
            System.Console.WriteLine("Using Production Server");
            apiClient = new RestClient(apiPath);
            apiClient.CookieContainer = new CookieContainer();
        }

        public static PhotoTossRest Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new PhotoTossRest();
                return _singleton;
            }
        }

        public User CurrentUser
        {
            get { return _currentUser; }
        }

        public string GetUserProfileImage(string username)
        {
            if (!String.IsNullOrEmpty(username))
             return "https://graph.facebook.com/" + username + "/picture?type=square";
            else
                return "https://s3-us-west-2.amazonaws.com/app.goheard.com/images/unknown-user.png";
        }

        public void GetUserImages(PhotoRecordList_callback callback)
        {
            string fullURL = "images";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync<List<PhotoRecord>>(request, (response) =>
            {
                if (response == null)
                    return;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<PhotoRecord> imageList = response.Data;

						if (imageList != null)
							imageList = imageList.OrderByDescending(o => o.created).ToList();

                    callback(imageList);
                }
                else
                    callback(null);
            });
        }

		public void GetImageLineage(long imageId, PhotoRecordList_callback callback)
		{
			string fullURL = "image/lineage";

			RestRequest request = new RestRequest(fullURL, Method.GET);
			request.AddParameter("imageid", imageId);

			apiClient.ExecuteAsync<List<PhotoRecord>>(request, (response) =>
				{
					if (response == null)
						return;
					if (response.StatusCode == HttpStatusCode.OK)
					{
						List<PhotoRecord> imageList = response.Data;

						//imageList.Sort(objListOrder.OrderBy(o=>o.OrderDate).ToList();

						callback(imageList);
					}
					else
						callback(null);
				});
		}

		public void GetImageTosses(long imageId, TossList_classback callback)
		{
			string fullURL = "image/tosses";

			RestRequest request = new RestRequest(fullURL, Method.GET);
			request.AddParameter("imageid", imageId);

			apiClient.ExecuteAsync<List<TossRecord>>(request, (response) =>
				{
					if (response == null)
						return;
					if (response.StatusCode == HttpStatusCode.OK)
					{
						List<TossRecord> tossList = response.Data;

						//imageList.Sort(objListOrder.OrderBy(o=>o.OrderDate).ToList();

						callback(tossList);
					}
					else
						callback(null);
				});
		}

		public void GetTossCatches(long tossId, PhotoRecordList_callback callback)
		{
			string fullURL = "toss/catches";

			RestRequest request = new RestRequest(fullURL, Method.GET);
			request.AddParameter("tossid", tossId);

			apiClient.ExecuteAsync<List<PhotoRecord>>(request, (response) =>
				{
					if (response == null)
						return;
					if (response.StatusCode == HttpStatusCode.OK)
					{
						List<PhotoRecord> photoList = response.Data;

						//imageList.Sort(objListOrder.OrderBy(o=>o.OrderDate).ToList();

						callback(photoList);
					}
					else
						callback(null);
				});
		}


        public void Logout()
        {
            string fullURL = "user/logout";

            RestRequest request = new RestRequest(fullURL, Method.POST);

            apiClient.Execute(request);

            _currentUser = null;
        }



        public void FacebookLogin(string userId, string token, User_callback callback)
        {
            string fullURL = "user/facebooklogin";

            RestRequest request = new RestRequest(fullURL, Method.POST);
            request.AddParameter("id", userId);
            request.AddParameter("token", token);

            apiClient.ExecuteAsync<User>(request, (response) =>
                {
                    User newUser = response.Data;
                    if (newUser != null)
                    {
                        _currentUser = newUser;
                        callback(newUser);
                    }
                    else
                        callback(null);
                });
        }

  

        public void GetUploadURL(String_callback callback)
        {
            string fullURL = "image/upload";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync(request, (response) =>
            {
                _uploadURL = response.Content;
                callback(_uploadURL);
            });

        }

        public void GetUserImageUploadURL(String_callback callback)
        {
            string fullURL = "user/image";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync(request, (response) =>
            {
                _userImageURL = response.Content;
                callback(_userImageURL);
            });

        }

        public void GetCatchURL(String_callback callback)
        {
            string fullURL = "catch";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync(request, (response) =>
            {
                _catchURL = response.Content;
                callback(_catchURL);
            });

        }




        public void GetImage(String_callback callback)
        {
            string fullURL = "image";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync(request, (response) =>
            {
                _uploadURL = response.Content;
                callback(_uploadURL);
            });
        }


        public void StartToss(long imageId, int gameType, double longitude, double latitude, Toss_callback callback)
        {
            string fullURL = "toss";

            RestRequest request = new RestRequest(fullURL, Method.POST);
            request.AddParameter("image", imageId);
            request.AddParameter("game", gameType);
            request.AddParameter("long", longitude);
            request.AddParameter("lat", latitude);

            apiClient.ExecuteAsync<TossRecord>(request, (response) =>
            {
                callback(response.Data);
            });
        }

        public void CatchToss(Stream photoStream, long tossid, double longitude, double latitude, PhotoRecord_callback callback)
        {
            RestClient onetimeClient = new RestClient(_catchURL);
            onetimeClient.CookieContainer = apiClient.CookieContainer;

            var request = new RestRequest("", Method.POST);
            request.AddHeader("Accept", "*/*");
            //request.AlwaysMultipartFormData = true;
            request.AddParameter("toss", tossid);
            request.AddParameter("long", longitude);
            request.AddParameter("lat", latitude);
            request.AddFile("file", ReadToEnd(photoStream), "file", "image/jpeg");

            onetimeClient.ExecuteAsync(request, (response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    PhotoRecord newRec = response.Content.FromJson<PhotoRecord>();
                    callback(newRec);
                }
                else
                {
                    //error ocured during upload
                    callback(null);
                }
            });
        }

        public void GetTossStatus(String_callback callback)
        {
            string fullURL = "toss/status";

            RestRequest request = new RestRequest(fullURL, Method.GET);

            apiClient.ExecuteAsync(request, (response) =>
            {
                _uploadURL = response.Content;
                callback(_uploadURL);
            });

        }



        public void UploadImage(Stream photoStream, double longitude, double latitude, PhotoRecord_callback callback)
        {
            RestClient onetimeClient = new RestClient(_uploadURL);
            onetimeClient.CookieContainer = apiClient.CookieContainer;

            var request = new RestRequest("", Method.POST);
            request.AddHeader("Accept", "*/*");
            //request.AlwaysMultipartFormData = true;
            request.AddParameter("long", longitude);
            request.AddParameter("lat", latitude);
            request.AddFile("file", ReadToEnd(photoStream), "file", "image/jpeg");

            onetimeClient.ExecuteAsync(request, (response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    PhotoRecord newRec = response.Content.FromJson<PhotoRecord>();
                    callback(newRec);
                }
                else
                {
                    //error ocured during upload
                    callback(null);
                }
            });
        }

        public void UploadImageThumb(Stream photoStream, long imageId, String_callback callback)
        {
            RestClient onetimeClient = new RestClient(_uploadURL);
            onetimeClient.CookieContainer = apiClient.CookieContainer;

            var request = new RestRequest("", Method.POST);
            request.AddHeader("Accept", "*/*");
            //request.AlwaysMultipartFormData = true;
            request.AddParameter("thumbnail", true);
            request.AddParameter("imageid", imageId);
            request.AddFile("file", ReadToEnd(photoStream), "file", "image/jpeg");

            onetimeClient.ExecuteAsync(request, (response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    callback(response.Content);
                }
                else
                {
                    //error ocured during upload
                    callback(null);
                }
            });
        }




        public byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

    }
}
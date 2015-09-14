using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using ServiceStack.Text;
using Android.Hardware;

using PhotoToss.Core;

namespace PhotoToss.AndroidApp
{
	public class HomeFragment : Android.Support.V4.App.Fragment, ISensorEventListener
    {
        public MainActivity MainPage { get; set; }
        GridView imageGrid;
        public List<PhotoRecord> PhotoList { get; set; }
		private SensorManager _sensorManager;
		private List<double> dataList = new List<double> ();

        public event Action PulledToRefresh;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.HomeFragment, container, false);

			PhotoList = new List<PhotoRecord> ();

            imageGrid = view.FindViewById<GridView>(Resource.Id.imagesView);
            imageGrid.Visibility = ViewStates.Invisible;
            imageGrid.Adapter = new PhotoRecordAdapter(this.Activity, this);
            imageGrid.NumColumns = 2;
            imageGrid.StretchMode = StretchMode.StretchColumnWidth;
            imageGrid.ItemClick += imageGrid_ItemClick;

            Refresh();
			_sensorManager = (SensorManager) Activity.GetSystemService(Context.SensorService);
            return view;
        }

        void imageGrid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
			PhotoTossRest.Instance.CurrentImage = PhotoList [e.Position];
            this.Activity.StartActivity(typeof(ImageViewActivity));
        }

        
		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{
			// We don't want to do anything here.
		}

		public void OnSensorChanged(SensorEvent e)
		{
			double X ;
			double Y ;
			double Z ;
			//lock (_syncLock)
			{
				X = e.Values [0];
				Y = e.Values [1];
				Z = e.Values [2];
			}

			UpdateViewRotation (X,Y,Z);

		}

		private void UpdateViewRotation(double x, double y, double z)
		{
			double newRot = 0;
			double targetRot = 0;

			switch (Activity.WindowManager.DefaultDisplay.Rotation)
			{
			case SurfaceOrientation.Rotation90:
				newRot = -y;
				break;
			case SurfaceOrientation.Rotation270:
				newRot = y;
				break;
			case SurfaceOrientation.Rotation0:
				newRot = x;
				newRot += 0;
				break;
			case SurfaceOrientation.Rotation180:
				newRot = x;
				newRot += 0;
				break;
			}

			dataList.Insert (0, newRot);
			if (dataList.Count > 10)
				dataList.RemoveAt (10);

			foreach (double curVal in dataList) {
				targetRot += curVal;
			}
			targetRot /= dataList.Count;
			targetRot = Math.Round(targetRot, 2);

			targetRot *= 1;//(180.0 / Math.PI);

			Activity.RunOnUiThread(() => {
				for (int i = imageGrid.FirstVisiblePosition; i <= imageGrid.LastVisiblePosition; i++)
				{
					View curCell = imageGrid.GetChildAt(i);

					if ((curCell != null) && (curCell.Rotation != (float)targetRot))
					{
						/*
						if (curCell.Rotation < targetRot) {
							curCell.RotationSpeed += maxAcc;
							if (curCell.RotationSpeed > maxSpeed)
								curCell.RotationSpeed = maxSpeed;
						}
						else {
							curCell.RotationSpeed -= maxAcc;
							if (curCell.RotationSpeed < -maxSpeed)
								curCell.RotationSpeed = -maxSpeed;
						}
						curCell.Rotation += curCell.RotationSpeed;

						if (curCell.Rotation > maxRot) {
							curCell.Rotation = maxRot;
							curCell.RotationSpeed = 0;
						}
						else if (curCell.Rotation < -maxRot) {
							curCell.Rotation = -maxRot;
							curCell.RotationSpeed = 0;
						}

						curCell.Rotation = Math.Round(curCell.Rotation, 2);
						curCell.Rotation = targetRot;
						curCell.Transform = CGAffineTransform.MakeRotation ((nfloat)curCell.Rotation);
						*/
						curCell.Rotation = (float)targetRot;
					}

				}
			});
		}


		public override void OnPause()
		{
			base.OnPause();
			_sensorManager.UnregisterListener(this);
		}

		public override void OnResume()
		{
			base.OnResume();
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Normal);
		}
        



        public void Refresh()
        {
			if (PhotoTossRest.Instance.CurrentUser != null) 
			{
				PhotoTossRest.Instance.GetUserImages ((userImageList) => 
					{
						this.Activity.RunOnUiThread(() =>
							{
								this.PhotoList.Clear();
								if ((userImageList == null) || (userImageList.Count == 0))
								{
									imageGrid.Visibility = ViewStates.Invisible;
								} 
								else 
								{
									imageGrid.Visibility = ViewStates.Visible;
									PhotoList.AddRange(userImageList);
									int numItems = imageGrid.Adapter.Count;
									((PhotoRecordAdapter)imageGrid.Adapter).NotifyDataSetChanged();
									imageGrid.InvalidateViews ();

									imageGrid.SmoothScrollToPosition (0);
								}
							});

					});
			}
			else imageGrid.Visibility = ViewStates.Invisible;
        }

        public void AddImage(PhotoRecord newRec)
        {
			PhotoList.Insert (0, newRec);
            this.Activity.RunOnUiThread(() =>
                {
                    imageGrid.Visibility = ViewStates.Visible;
					((PhotoRecordAdapter)imageGrid.Adapter).NotifyDataSetChanged();
                    imageGrid.InvalidateViews();
                    imageGrid.SmoothScrollToPosition(0);
                });
        }

        public class PhotoRecordAdapter : BaseAdapter, Koush.IUrlImageViewCallback
        {
            private readonly Context context;
            private readonly HomeFragment home;
			private int itemWidth = 256, profileWidth = 64;

            public PhotoRecordAdapter(Context c, HomeFragment theFragment)
            {
                context = c;
                var metrics = context.Resources.DisplayMetrics;
                int screenWidth = metrics.WidthPixels;
                float margin = 0f;
                float marginPixels = margin * context.Resources.DisplayMetrics.Density;
                itemWidth = (int)(((float)screenWidth - (marginPixels * 3f)) / 2f);
				profileWidth = (int)((float)profileWidth * context.Resources.DisplayMetrics.Density);


                home = theFragment;
            }

            public override int Count
            {
                get 
                {
                    if ((home != null) && (home.PhotoList != null))
                        return home.PhotoList.Count; 
                    else
                        return 0;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

			public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
			{
				SetRoundImage (theImage, theBitmap);
			}

			private void SetRoundImage(ImageView theImage, Bitmap theBitmap)
			{
				((Activity)context).RunOnUiThread (() => {
					CircleDrawable myCircle = new CircleDrawable (theBitmap);
					theImage.SetImageDrawable (myCircle);
				});
			}

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
				Random	rnd = new Random (position);
				ImageView imageView, userView;
                View curView;
                PhotoRecord curRec = home.PhotoList[position];


                if (convertView == null)
                {
                    curView = home.Activity.LayoutInflater.Inflate(Resource.Layout.photoGridCell,null);
                }
                else
                {
                    curView = convertView;
                }
				curView.Rotation = ((float)(50 - rnd.Next (100)))/ 10.0f;
                imageView = curView.FindViewById<ImageView>(Resource.Id.imageView);
				userView = curView.FindViewById<ImageView> (Resource.Id.profileImage);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                userView.Visibility = ViewStates.Gone;
				long tosserId = curRec.tosserid;
				if ((tosserId != 0) && (tosserId != PhotoTossRest.Instance.CurrentUser.id))
				{
                    string imageUrl = PhotoTossRest.Instance.GetUserProfileImage(curRec.tossername);
					userView.Visibility = ViewStates.Visible;

					var theObj = Koush.DrawableCache.Instance.Get (imageUrl);

					if (theObj == null) {
						Koush.UrlImageViewHelper.SetUrlDrawable (userView, imageUrl, Resource.Drawable.unknown_octopus, this);
						Android.Graphics.Drawables.Drawable theDrawable = userView.Drawable;
						if (theDrawable != null) {
							System.Console.WriteLine ("a draable!");
						}
					}
					else {
						Koush.UrlImageViewHelper.ZombieDrawable drawable = (Koush.UrlImageViewHelper.ZombieDrawable)theObj;
						SetRoundImage (userView, drawable.Bitmap);
					}

				}
				Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString() + "-c", Resource.Drawable.ic_camera);

               
                return curView;
            }

            

            
        }
    }

}
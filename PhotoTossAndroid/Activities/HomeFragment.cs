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


using PhotoToss.Core;

namespace PhotoToss.AndroidApp
{
    public class HomeFragment : Android.Support.V4.App.Fragment
    {
        public MainActivity MainPage { get; set; }
        GridView imageGrid;
        public List<PhotoRecord> PhotoList { get; set; }

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

			MainActivity.loginView = view.FindViewById<LinearLayout> (Resource.Id.loginView);



           
           

            Refresh();
            return view;
        }

        void imageGrid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
			PhotoTossRest.Instance.CurrentImage = PhotoList [e.Position];
            this.Activity.StartActivity(typeof(TossActivity));
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

        public class PhotoRecordAdapter : BaseAdapter
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

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
				Random	rnd = new Random (position);
				ImageView imageView, userView;
                TextView captionText;
                View curView;
                PhotoRecord curRec = home.PhotoList[position];


                if (convertView == null)
                {
                    curView = home.Activity.LayoutInflater.Inflate(Resource.Layout.photoGridCell,null);


                    //curView.LayoutParameters = new ViewGroup.LayoutParams(itemWidth, itemWidth); 
                  

                    //imageView.SetPadding(8, 8, 8, 8);
                }
                else
                {
                    curView = convertView;
                }
				curView.Rotation = ((float)(50 - rnd.Next (100)))/ 10.0f;
                imageView = curView.FindViewById<ImageView>(Resource.Id.imageView);
				userView = curView.FindViewById<ImageView> (Resource.Id.profileImage);
                captionText = curView.FindViewById<TextView>(Resource.Id.captionText);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                userView.Visibility = ViewStates.Gone;
				long tosserId = curRec.tosserid;
				if ((tosserId != 0) && (tosserId != PhotoTossRest.Instance.CurrentUser.id))
				{
					PhotoTossRest.Instance.GetUserProfileImage (curRec.tosserid, (imageUrl) => 
						{
							if (String.IsNullOrEmpty (imageUrl))
								imageUrl = "https://s3-us-west-2.amazonaws.com/app.goheard.com/images/unknown-user.png";
							else
								imageUrl += "=s128-c";

		                    BitmapHelper.GetImageBitmapFromUrlAsync(imageUrl, (theBitmap) =>
		                        {
		                            ((Activity)context).RunOnUiThread(() =>
		                                {
		                                    userView.Visibility = ViewStates.Visible;
		                                    Bitmap userBitMap = BitmapHelper.GetImageBitmapFromUrl(imageUrl);
		                                    CircleDrawable myCircle = new CircleDrawable(userBitMap);
		                                    userView.SetImageDrawable(myCircle);
		                                });

		                        });
						});
				}
                captionText.Text = curRec.caption;
				Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString(), Resource.Drawable.ic_camera);

               
                return curView;
            }

            

            
        }
    }

}
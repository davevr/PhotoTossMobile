
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhotoToss.Core;
using Android.Content.PM;
using ImageViews.Photo;
using Android.Graphics;


namespace PhotoToss.AndroidApp
{
	[Activity(Theme = "@style/AppSubTheme", ScreenOrientation = ScreenOrientation.Portrait)]	
	public class PhotoLineageActivity : Android.Support.V7.App.AppCompatActivity
	{
		private ListView lineageListView;
		private PhotoLineageAdapter adapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			// Create your application here
			SetContentView (Resource.Layout.PhotoLineageActivity);


			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tool_bar);
			SetSupportActionBar(toolbar);

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(false);
			SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable( Resources.GetColor(Resource.Color.PT_light_teal)));


			this.Title =  "Toss History";

			lineageListView = FindViewById<ListView> (Resource.Id.lineageListView);
			adapter = new PhotoLineageAdapter (this, new List<PhotoRecord> ());
			lineageListView.Adapter = adapter;
			Refresh ();
		}
			
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{

			case 16908332:// the back button apparently...
				{
					Finish();
				}
				break;

			}
			return base.OnOptionsItemSelected(item);
		}

		public void Refresh()
		{
			if (ImageViewSpreadFragment.CurrentMarkerRecord != null) {
				PhotoTossRest.Instance.GetImageLineage (ImageViewSpreadFragment.CurrentMarkerRecord.id, (parents) => {

					UpdateList (parents);
				});
			}
		}

		private void UpdateList(List<PhotoRecord> parents)
		{
			if (parents != null) {
				parents.Insert (0, ImageViewSpreadFragment.CurrentMarkerRecord);
				RunOnUiThread (() => {
					adapter.allItems = parents;
					adapter.NotifyDataSetChanged ();
					lineageListView.InvalidateViews ();
				});
			}
		}
	}

	public class PhotoLineageAdapter : BaseAdapter<PhotoRecord>, Koush.IUrlImageViewCallback {
		public List<PhotoRecord>	allItems;
		Activity context;



		public PhotoLineageAdapter(Activity context, List<PhotoRecord> theItems) : base() {
			this.context = context;
			this.allItems = theItems;
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override PhotoRecord this[int position] {  
			get { return allItems[position]; }
		}
		public override int Count {
			get { return allItems.Count; }
		}

		public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
		{
			AttacherView parentView = (AttacherView)theImage.Parent.Parent;

			PhotoViewAttacher attacher = parentView.attacher;

			if (attacher == null) {
				attacher = new  PhotoViewAttacher (theImage);
				attacher.MaximumScale = 5;
				parentView.attacher = attacher;
			}

			// make the new image
			PhotoRecord curImage = parentView.curPhoto;
			BarcodeLocation barLoc = curImage.barcodelocation;

			if (barLoc != null) {
				Bitmap canvasMap = theBitmap.Copy(theBitmap.GetConfig(), true);
				Bitmap tinyImage = null;//ImageViewDetailFragment.CurrentImage;
				Canvas newCanvas = new Canvas(canvasMap);
				Paint thePaint = new Paint(PaintFlags.AntiAlias);
				BarcodePoint bottomLeftExt;
				BarcodePoint bottomRightExt; 
				bottomLeftExt.x = barLoc.topleft.x + (barLoc.bottomleft.x - barLoc.topleft.x) * 2;
				bottomLeftExt.y = barLoc.topleft.y + (barLoc.bottomleft.y - barLoc.topleft.y) * 2;
				bottomRightExt.x = barLoc.topright.x + (barLoc.bottomright.x - barLoc.topright.x) * 2;
				bottomRightExt.y = barLoc.topright.y + (barLoc.bottomright.y - barLoc.topright.y) * 2;


				Matrix matrix = new Matrix ();
				matrix.SetPolyToPoly (new float[] { 0, 0, tinyImage.Width, 0, tinyImage.Width, tinyImage.Height, 0, tinyImage.Height }, 0,
					new float[] {barLoc.topleft.x, barLoc.topleft.y, barLoc.topright.x, barLoc.topright.y,
						bottomRightExt.x, bottomRightExt.y, bottomLeftExt.x, bottomLeftExt.y
					}, 0, 4);

				newCanvas.DrawBitmap (tinyImage, matrix, thePaint);

				theImage.SetImageBitmap (canvasMap);
			}

			attacher.Update();
		}


		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			AttacherView view = convertView as AttacherView; // re-use an existing view, if one is available
			if (view == null) {
				view = new AttacherView (context);

				//View.Inflate(view.Context, Resource.Layout.PhotoLineageCell, null);
			}

			var imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			var userImageView = view.FindViewById<ImageView> (Resource.Id.userImageView);
			var dateTossView = view.FindViewById<TextView> (Resource.Id.tossDateText);
			var photoTextView = view.FindViewById<EditText> (Resource.Id.photoText);
			var photoTextStatic = view.FindViewById<TextView> (Resource.Id.photoTextStatic);
			PhotoRecord curItem = allItems [position];
			view.curPhoto = curItem;

			string imageUrl, dateString, userUrl;

			if (!String.IsNullOrEmpty (curItem.catchUrl)) {
				// caught image
				imageUrl = curItem.catchUrl;
				DateTime photoDate = curItem.received.ToLocalTime();
				dateString = string.Format("caught {0} {1}", photoDate.ToShortDateString(), photoDate.ToShortTimeString());
				userUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);

			}
			else {
				// original image
				imageUrl = curItem.imageUrl;
				DateTime photoDate = curItem.created.ToLocalTime();
				dateString = string.Format("originally taken {0} {1}", photoDate.ToShortDateString(), photoDate.ToShortTimeString());
				userUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);
			}
				
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, imageUrl+ "=s2048", Resource.Drawable.ic_camera, this);
			dateTossView.Text = dateString;
			Koush.UrlImageViewHelper.SetUrlDrawable (userImageView, userUrl, Resource.Drawable.unknown_octopus);


			if (curItem.ownerid == PhotoTossRest.Instance.CurrentUser.id) {
				photoTextView.Visibility = ViewStates.Visible;
				photoTextStatic.Visibility = ViewStates.Gone;
				photoTextView.Text = curItem.caption;
			} else {
				photoTextView.Visibility = ViewStates.Gone;
				if (!String.IsNullOrEmpty (curItem.caption)) {
					photoTextStatic.Visibility = ViewStates.Visible;
					photoTextStatic.Text = curItem.caption;
				}
				else
					photoTextStatic.Visibility = ViewStates.Gone;
				
			}
			return view;
		}
	}

	public class AttacherView : LinearLayout
	{
		public PhotoViewAttacher attacher = null;
		public PhotoRecord curPhoto { get; set; }

		public AttacherView( Context context) : base(context) {
			LayoutInflater mInflater = (LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
			mInflater.Inflate (Resource.Layout.PhotoLineageCell, this, true);
			}
	}
}


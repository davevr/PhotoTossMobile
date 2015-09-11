using System;
using Android.Text;
using Android.Text.Format;
using Android.Text.Style;
using Android.Util;
using Android.Graphics;
using Android.Content;

namespace PhotoToss.AndroidApp
{
	class TypefaceSpan : MetricAffectingSpan
	{
		private static LruCache sTypefaceCache = new LruCache(1024);

		private Typeface mTypeface;

		public TypefaceSpan(Context context, String typefaceName) 
		{
			mTypeface = (Typeface)sTypefaceCache.Get(typefaceName);

			if (mTypeface == null)
			{
				mTypeface = Typeface.CreateFromAsset(context.Assets, string.Format("fonts/{0}", typefaceName));
				sTypefaceCache.Put(typefaceName, mTypeface);
			}
		}

		public override void UpdateMeasureState(TextPaint tp)
		{
			tp.SetTypeface(mTypeface);
			tp.Flags = tp.Flags | PaintFlags.SubpixelText;
		}

		public override void UpdateDrawState(TextPaint tp)
		{
			tp.SetTypeface(mTypeface);
			tp.Flags = tp.Flags | PaintFlags.SubpixelText;
		}
	}
}


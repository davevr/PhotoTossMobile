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
using System.IO;
using Android.Database;
using Android.Graphics;
using Android.Media;

using Android.Provider;

namespace PhotoToss.AndroidApp
{
    class StreamHelper
    {
		public static System.IO.Stream GetStreamFromFileUri(Context context, Android.Net.Uri uri, int maxSize = 0)
        {
			System.IO.Stream result = null;
            if (context is Activity)
            {
                string filePath = null;

                if (uri.Scheme.Equals("content"))
                {
                    // old method (osVersion < 19)
                    filePath = GetPathToImage((Activity)context, uri);
                   
                }
                else if (uri.Scheme.Equals("file"))
                {
                    filePath = uri.EncodedPath;
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (maxSize > 0)
                    {
                        // TO DO:  Limit image size to 1024
                        Bitmap scaledImage = ResizeImageFile(filePath, maxSize);
                        MemoryStream stream = new MemoryStream();
                        scaledImage.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                        result = stream;
                    }
                    else
                        result = System.IO.File.OpenRead(filePath);

                }
            }

            return result;
        }

        private static Bitmap ResizeImageFile(string filePath, int maxSize)
        {
 			Bitmap bp = BitmapFactory.DecodeFile (filePath);
			int originalWidth = bp.Width;
			int originalHeight = bp.Height;

			ExifInterface	exif = new ExifInterface (filePath);
			int orientation = exif.GetAttributeInt (ExifInterface.TagOrientation, (int)Android.Media.Orientation.Normal);
			int angle = 0;

			switch (orientation) 
			{
			case (int)Android.Media.Orientation.Normal:
						break;

			case (int)Android.Media.Orientation.Rotate90:
				angle = 90;
				break;

			case (int)Android.Media.Orientation.Rotate180:
				angle = 180;
				break;

			case (int)Android.Media.Orientation.Rotate270:
				angle = 270;
				break;
			}

            float resizeScale = 1;

			if (originalWidth > maxSize || originalHeight > maxSize) {
				double ratio = (double)originalWidth / (double)originalHeight;

				if (ratio > 1)
					resizeScale = (float)maxSize / (float)originalWidth;
				else
					resizeScale = (float)maxSize / (float)originalHeight;
			}
				
         
			Matrix mat = new Matrix ();
			mat.PostScale (resizeScale, resizeScale);
			mat.PostRotate ((float)angle);
			Bitmap capBitmap = Bitmap.CreateBitmap (bp, 0, 0, originalWidth, originalHeight, mat, true);

			return capBitmap;

        }
        public static string GetFileName(Context context, Android.Net.Uri uri)
        {
            String result = null;
            if (uri.Scheme.Equals("content"))
            {
                string imgPath = GetPathToImage((Activity)context, uri);

                result = System.IO.Path.GetFileName(imgPath);
            }
            else if (uri.Scheme.Equals("file"))
            {
                result = System.IO.Path.GetFileName(uri.EncodedPath);
            }

            return result;
        }
        public static string GetPathToImage(Activity activity, Android.Net.Uri uri)
        {
			Boolean isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(activity, uri))
            {
                if (isExternalStorageDocument(uri))
                {
                    string docID = DocumentsContract.GetDocumentId(uri);
                    string[] split = docID.Split(':');
                    string type = split[0];

                    if (type.Equals("primary", StringComparison.OrdinalIgnoreCase))
                    {
                        return (string)Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                else if (isDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));
                    return getDataColumn(activity, contentUri, null, null);
                }
                else if (isMediaDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    Android.Net.Uri contentUri = null;

                    switch (type.ToLower())
                    {
                        case "image":
                            contentUri = MediaStore.Images.Media.ExternalContentUri;
                            break;
                        case "video":
                            contentUri = MediaStore.Video.Media.ExternalContentUri;
                            break;
                        case "audio":
                            contentUri = MediaStore.Audio.Media.ExternalContentUri;
                            break;

                    }

                    string selection = "_id?";
                    string[] selectionArgs = new string[] { split[1] };

                    return getDataColumn(activity, contentUri, selection, selectionArgs);

                }
            }
            // MediaStore (and general)
            else if (uri.Scheme.Equals("content", StringComparison.OrdinalIgnoreCase))
            {
                return getDataColumn(activity, uri, null, null);
            }
            // File
            else if(uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        public static String getDataColumn(Activity activity, Android.Net.Uri uri, string selection,  string[] selectionArgs) 
        {

            ICursor cursor = null;
            string column = "_data";
            string[] projection = {
                    column
            };

            try 
            {
                cursor = activity.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst()) 
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            } 
            finally 
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase);
        }

        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase);
        }

        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority, StringComparison.OrdinalIgnoreCase);
        }


        /*
        public static String getPath(Context context, Android.Net.Uri uri) 
        {
            bool isKitKat = Build.VERSION.SdkInt >= 19;
            

            if (isKitKat)
            {
                if (isExternalStorageDocument(uri))
                {
                     String docId = DocumentsContract.getDocumentId(uri);
                     String[] split = docId.split(":");
                     String type = split[0];

                if ("primary".equalsIgnoreCase(type)) {
                    return Environment.getExternalStorageDirectory() + "/" + split[1];
                }

                }
                else if (isDownloadsDocument(uri))
                {


                }
                else if (isMediaDocument(uri))
                {


                }

            }
            else
            {


            }
            return null;
        }
        /*
    // DocumentProvider
    if (isKitKat && DocumentsContract.isDocumentUri(context, uri)) {
        // ExternalStorageProvider
        if (isExternalStorageDocument(uri)) {
            final String docId = DocumentsContract.getDocumentId(uri);
            final String[] split = docId.split(":");
            final String type = split[0];

            if ("primary".equalsIgnoreCase(type)) {
                return Environment.getExternalStorageDirectory() + "/" + split[1];
            }

            // TODO handle non-primary volumes
        }
        // DownloadsProvider
        else if (isDownloadsDocument(uri)) {

            final String id = DocumentsContract.getDocumentId(uri);
            final Uri contentUri = ContentUris.withAppendedId(
                    Uri.parse("content://downloads/public_downloads"), Long.valueOf(id));

            return getDataColumn(context, contentUri, null, null);
        }
        // MediaProvider
        else if (isMediaDocument(uri)) {
            final String docId = DocumentsContract.getDocumentId(uri);
            final String[] split = docId.split(":");
            final String type = split[0];

            Uri contentUri = null;
            if ("image".equals(type)) {
                contentUri = MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
            } else if ("video".equals(type)) {
                contentUri = MediaStore.Video.Media.EXTERNAL_CONTENT_URI;
            } else if ("audio".equals(type)) {
                contentUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI;
            }

            final String selection = "_id=?";
            final String[] selectionArgs = new String[] {
                    split[1]
            };

            return getDataColumn(context, contentUri, selection, selectionArgs);
        }
    }
    // MediaStore (and general)
    else if ("content".equalsIgnoreCase(uri.getScheme())) {

        // Return the remote address
        if (isGooglePhotosUri(uri))
            return uri.getLastPathSegment();

        return getDataColumn(context, uri, null, null);
    }
    // File
    else if ("file".equalsIgnoreCase(uri.getScheme())) {
        return uri.getPath();
    }

    return null;
}


public static String getDataColumn(Context context, Uri uri, String selection,
        String[] selectionArgs) {

    Cursor cursor = null;
    final String column = "_data";
    final String[] projection = {
            column
    };

    try {
        cursor = context.getContentResolver().query(uri, projection, selection, selectionArgs,
                null);
        if (cursor != null && cursor.moveToFirst()) {
            final int index = cursor.getColumnIndexOrThrow(column);
            return cursor.getString(index);
        }
    } finally {
        if (cursor != null)
            cursor.close();
    }
    return null;
}


public static boolean isExternalStorageDocument(Uri uri) {
    return "com.android.externalstorage.documents".equals(uri.getAuthority());
}


public static boolean isDownloadsDocument(Uri uri) {
    return "com.android.providers.downloads.documents".equals(uri.getAuthority());
}


public static boolean isMediaDocument(Uri uri) {
    return "com.android.providers.media.documents".equals(uri.getAuthority());
}


public static boolean isGooglePhotosUri(Uri uri) {
    return "com.google.android.apps.photos.content".equals(uri.getAuthority());
}

        */
    }
}
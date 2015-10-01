using System;

namespace PhotoToss.Core
{
	public class Utilities
	{
		public const string USERNAME = "username";
		public const string PASSWORD = "password";
		public const int SIGNIN_INTENT = 0x111;
		public const int PHOTO_CATCH_EVENT = 0x555;
		public const int PHOTO_UPLOAD_SUCCESS = 0x666;
		public const int PHOTO_CAPTURE_EVENT = 0x777;
		public const int IMAGE_DELETE_EVENT = 0x888;

		public Utilities ()
		{

		}

		public static object SafeLoadSetting(string setting, object defVal)
		{
			System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains(setting))
				return settings[setting];
			else
			{
				settings.Add(setting, defVal);
				return defVal;
			}
		}
			
		public static void SafeSaveSetting(string setting, object val)
		{
			System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains(setting))
				settings[setting] = val;
			else
			{
				settings.Add(setting, val);

			}
			settings.Save();
		}

		public static void SafeClearSetting(string setting)
		{
			System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
			if (settings.Contains(setting))
			{
				settings.Remove(setting);
				settings.Save();
			}
		}


	}
}


using Windows.Storage;

namespace Konachan.Helper
{
    public struct WallpaperTaskSettings
    {
        public bool IsEnable;
        public bool IsWPEnable;
        public bool IsLSEnable;

        public int WPTimer;
        public int LSTimer;

        public string WPTag;
        public string LSTag;
    }

    public sealed class SettingHelper
    {
        //public enum DType
        //{
        //    PC,
        //    Mobile,
        //    Unknown
        //}
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Trying to get key '" + key + "' that doesn't exist.");
                return null;
            }
        }
        /// <summary>
        /// 设置指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        public static void SetValue(string key, object value)
        {
            container.Values[key] = value;
        }
        /// <summary>
        /// 指示应用容器内是否存在某键
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            if (container.Values[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取设备类型
        /// </summary>
        /// <returns></returns>
        //public static DType DeviceType
        //{
        //    get
        //    {
        //        string type = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
        //        switch (type)
        //        {
        //            case "Windows.Desktop": return DType.PC;
        //            case "Windows.Mobile": return DType.Mobile;
        //            default: return DType.Unknown;
        //        }
        //    }        
        //}

        public static WallpaperTaskSettings GetWallpaperTaskSettings()
        {
            WallpaperTaskSettings settings = new WallpaperTaskSettings();

            ///Default settings hard-coded here.
            settings.IsEnable = false;
            settings.IsLSEnable = false;
            settings.IsWPEnable = false;
            settings.LSTag = "";
            settings.LSTimer = 15;
            settings.WPTag = "";
            settings.WPTimer = 15;

            if (GetValue("wpt_enable") != null)
                settings.IsEnable = (bool)GetValue("wpt_enable");
            if (GetValue("wpt_wpenable") != null)
                settings.IsWPEnable = (bool)GetValue("wpt_wpenable");
            if (GetValue("wpt_lsenable") != null)
                settings.IsLSEnable = (bool)GetValue("wpt_lsenable");
            if (GetValue("wpt_lstag") != null)
                settings.LSTag = (string)GetValue("wpt_lstag");
            if (GetValue("wpt_wptag") != null)
                settings.WPTag = (string)GetValue("wpt_wptag");
            if (GetValue("wpt_lstimer") != null)
                settings.LSTimer = (int)GetValue("wpt_lstimer");
            if (GetValue("wpt_wptimer") != null)
                settings.WPTimer = (int)GetValue("wpt_wptimer");

            return settings;
        }

        public static void SaveWallpaperTaskSettings(WallpaperTaskSettings settings)
        {
            SetValue("wpt_enable", settings.IsEnable);
            SetValue("wpt_wpenable", settings.IsWPEnable);
            SetValue("wpt_lsenable", settings.IsLSEnable);
            SetValue("wpt_lstag", settings.LSTag);
            SetValue("wpt_wptag", settings.WPTag);
            SetValue("wpt_lstimer", settings.LSTimer);
            SetValue("wpt_wptimer", settings.WPTimer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;

namespace Konachan.Helper
{
    public sealed class Methods
    {
        /// <summary>
        /// 转换Linux时间戳为标准时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string LinuxToData(string str)
        {
            try
            {
                long sec = long.Parse(str);
                DateTimeOffset start = DateTimeOffset.FromUnixTimeSeconds(sec);
                return start.DateTime.ToLocalTime().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        public static Windows.Foundation.IAsyncOperation<StorageFolder> GetMyFolderAsync()
        {
            return Internal_GetMyFolderAsync().AsAsyncOperation();
        }

        static async Task<StorageFolder> Internal_GetMyFolderAsync()
        {
            StorageFolder folder = null;
            if (!SettingHelper.ContainsKey("_path"))
            {
                SettingHelper.SetValue("_path", string.Empty);
                folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
            }
            else
            {
                string path = SettingHelper.GetValue("_path").ToString();
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        folder = await StorageFolder.GetFolderFromPathAsync(path);
                    }
                    catch
                    {
                        folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
                    }
                }
                else
                {
                    folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
                }
            }
            return folder;
        }

        public static Windows.Foundation.IAsyncOperation<StorageFile> CreateFileInMyFolderWithID(string id, string ext)
        {
            return Internal_CreateFileInMyFolderWithID(id, ext).AsAsyncOperation();
        }

        static async Task<StorageFile> Internal_CreateFileInMyFolderWithID(string id, string ext)
        {
            string Name = "Konachan_" + id + ext;
            StorageFolder folder = await GetMyFolderAsync();
            StorageFile file = await folder.CreateFileAsync(Name, CreationCollisionOption.ReplaceExisting);
            return file;
        }

        public static Windows.Foundation.IAsyncOperation<bool> SetPicAsWallPapaer(StorageFile pic, bool setLockscrren)
        {
            return Internal_SetPicAsWallPapaer(pic, setLockscrren).AsAsyncOperation();
        }

        async static Task<bool> Internal_SetPicAsWallPapaer(StorageFile pic, bool setLockscrren)
        {
            if (pic == null) return false;
            
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile wallPaper;
            if(await localFolder.TryGetItemAsync(pic.Name) == null)
            {
                wallPaper = await pic.CopyAsync(localFolder);
            }
            else
            {
                wallPaper = await localFolder.GetFileAsync(pic.Name);//File already exists
            }
            if (setLockscrren)
            {
                return await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(wallPaper);
            }
            else
            {
                return await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(wallPaper);
            }
        }
    }
}

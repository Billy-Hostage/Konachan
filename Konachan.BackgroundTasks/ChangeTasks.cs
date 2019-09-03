using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.ApplicationModel.Background;
using Konachan.Helper;
using Newtonsoft.Json;
using Windows.Storage;

namespace Konachan.BackgroundTasks
{
    public sealed class WallpaperChangeTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        static string RequestURL;

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            await RunOnce();

            _deferral.Complete();
        }

        public static Windows.Foundation.IAsyncAction RunOnce()
        {
            return Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Wallpaper Change Task Runned");

                if((bool)SettingHelper.GetValue("wpt_enable") && (bool)SettingHelper.GetValue("wpt_wpenable"))
                {
                    RequestURL = BackgroundChangeTasksNetworkHelper.BuildURL(BackgroundChangeTasksNetworkHelper.BCTType.Wallpaper);
                    BackgroundTaskPicInfo picInfo = JsonConvert.DeserializeObject<List<BackgroundTaskPicInfo>>(await BackgroundChangeTasksNetworkHelper.SendGETAsyncAsString(RequestURL))[0];
                    if(picInfo != null)
                    {
                        HttpResponseMessage res = await BackgroundChangeTasksNetworkHelper.SendGETAsyncAsBinary(picInfo.file_url);
                        StorageFile picFile = await Helper.Methods.CreateFileInMyFolderWithID(picInfo.id.ToString(), picInfo.file_url.Substring(picInfo.file_url.Length - 4));
                        Windows.Storage.Streams.IRandomAccessStream writeStream = await picFile.OpenAsync(FileAccessMode.ReadWrite);
                        await writeStream.WriteAsync(await res.Content.ReadAsBufferAsync());
                        writeStream.Dispose();
                        await Methods.SetPicAsWallPapaer(picFile, false);
                    }
                }
            }).AsAsyncAction();
        }
    }

    public sealed class LockscreenChangeTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        static string RequestURL;
        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            await RunOnce();

            _deferral.Complete();
        }
        public static Windows.Foundation.IAsyncAction RunOnce()
        {
            return Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Lockscreen Change Task Runned");

                if ((bool)SettingHelper.GetValue("wpt_enable") && (bool)SettingHelper.GetValue("wpt_lsenable"))
                {
                    RequestURL = BackgroundChangeTasksNetworkHelper.BuildURL(BackgroundChangeTasksNetworkHelper.BCTType.Lockscreen);
                    BackgroundTaskPicInfo picInfo = JsonConvert.DeserializeObject<List<BackgroundTaskPicInfo>>(await BackgroundChangeTasksNetworkHelper.SendGETAsyncAsString(RequestURL))[0];
                    if(picInfo != null)
                    {
                        HttpResponseMessage res =  await BackgroundChangeTasksNetworkHelper.SendGETAsyncAsBinary(picInfo.file_url);
                        StorageFile picFile = await Helper.Methods.CreateFileInMyFolderWithID(picInfo.id.ToString(), picInfo.file_url.Substring(picInfo.file_url.Length - 4));
                        Windows.Storage.Streams.IRandomAccessStream writeStream = await picFile.OpenAsync(FileAccessMode.ReadWrite);
                        await writeStream.WriteAsync(await res.Content.ReadAsBufferAsync());
                        writeStream.Dispose();
                        await Methods.SetPicAsWallPapaer(picFile, true);
                    }
                }
            }).AsAsyncAction();
        }
    }

    static class BackgroundChangeTasksNetworkHelper
    {
        public enum BCTType
        {
            Wallpaper,
            Lockscreen
        }
        static string BaseURL = "http://konachan.com/post.json?limit=1&page=1&tags=";

        public static string BuildURL(BCTType type)
        {
            switch (type)
            {
                case BCTType.Lockscreen:
                    return BaseURL + (string)SettingHelper.GetValue("wpt_lstag");
                case BCTType.Wallpaper:
                    return BaseURL + (string)SettingHelper.GetValue("wpt_wptag");
            }
            return BaseURL;
        }

        public static async Task<string> SendGETAsyncAsString(string url)
        {
            string msgContent = "";

            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(url);
                try
                {
                    HttpResponseMessage msg = await client.GetAsync(uri);
                    msgContent = await msg.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            return msgContent;
        }

        public static async Task<HttpResponseMessage> SendGETAsyncAsBinary(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri reqURI = new Uri(url);
                HttpResponseMessage res = await client.GetAsync(reqURI);
                if (!res.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Background pic download failed.");
                }
                return res;
            }
        }
    }

    //We only care about these infomations while running a background task.
    class BackgroundTaskPicInfo
    {
        public int id { get; set; }
        public string file_url { get; set; }
    }
}

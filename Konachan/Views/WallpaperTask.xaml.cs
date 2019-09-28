using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Konachan.Helper;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Konachan.Views
{

    public sealed partial class WallpaperTask : Page
    {
        private WallpaperTaskSettings wallpaperTaskSettings;

        private bool isWallpaperTaskRegistered = false;
        private KeyValuePair<Guid, IBackgroundTaskRegistration> wallpaperTask;

        private bool isLockscreenTaskRegistered = false;
        private KeyValuePair<Guid, IBackgroundTaskRegistration> lockscreenTask;

        public WallpaperTask()
        {
            this.InitializeComponent();
            Init();
        }

        void Init()
        {
            wallpaperTaskSettings = SettingHelper.GetWallpaperTaskSettings();

            switch_enable.IsOn = wallpaperTaskSettings.IsEnable;
            switch_wallpaper.IsOn = wallpaperTaskSettings.IsWPEnable;
            switch_lockscreen.IsOn = wallpaperTaskSettings.IsLSEnable;
            Switch_enable_Toggled(null, null);
            Switch_lockscreen_Toggled(null, null);
            Switch_wallpaper_Toggled(null, null);

            text_wallpapertag.Text = wallpaperTaskSettings.WPTag;
            text_lockscreentag.Text = wallpaperTaskSettings.LSTag;
            text_wallpapertimer.Text = wallpaperTaskSettings.WPTimer.ToString();
            text_lockscreentimer.Text = wallpaperTaskSettings.LSTimer.ToString();

            foreach(var task in BackgroundTaskRegistration.AllTasks)
            {
                if(task.Value.Name == "wallpapertask")
                {
                    isWallpaperTaskRegistered = true;
                    wallpaperTask = task;
                }
                else if(task.Value.Name == "lockscreentask")
                {
                    isLockscreenTaskRegistered = true;
                    lockscreenTask = task;
                }
            }
        }

        private void Switch_enable_Toggled(object sender, RoutedEventArgs e)
        {
            wallpaperTaskSettings.IsEnable = switch_enable.IsOn;
            panel_config.Visibility = switch_enable.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Switch_wallpaper_Toggled(object sender, RoutedEventArgs e)
        {
            wallpaperTaskSettings.IsWPEnable = switch_wallpaper.IsOn;
            text_wallpapertimer.Visibility = switch_wallpaper.IsOn ? Visibility.Visible : Visibility.Collapsed;
            text_wallpapertag.Visibility = switch_wallpaper.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Switch_lockscreen_Toggled(object sender, RoutedEventArgs e)
        {
            wallpaperTaskSettings.IsLSEnable = switch_lockscreen.IsOn;
            text_lockscreentimer.Visibility = switch_lockscreen.IsOn ? Visibility.Visible : Visibility.Collapsed;
            text_lockscreentag.Visibility = switch_lockscreen.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Barbutton_apply_Click(object sender, RoutedEventArgs e)
        {
            //Check
            if (wallpaperTaskSettings.IsEnable)
            {
                if(wallpaperTaskSettings.IsLSEnable && wallpaperTaskSettings.LSTimer < 15)
                {
                    popup.Show("时间间隔最小为15分钟！", 3000);
                    return;
                }
                if (wallpaperTaskSettings.IsWPEnable && wallpaperTaskSettings.WPTimer < 15)
                {
                    popup.Show("时间间隔最小为15分钟！", 3000);
                    return;
                }
            }

            //Save settings
            SettingHelper.SaveWallpaperTaskSettings(wallpaperTaskSettings);

            await SetupBackGroundTaskAsync();

            popup.Show("设置已保存");
        }

        private async void Barbutton_applyrun_Click(object sender, RoutedEventArgs e)
        {
            Barbutton_apply_Click(null, null);//Apply first

            if (wallpaperTaskSettings.IsEnable)
            {
                if (wallpaperTaskSettings.IsWPEnable)
                {
                    await BackgroundTasks.WallpaperChangeTask.RunOnce();
                }
                if (wallpaperTaskSettings.IsLSEnable)
                {
                    await BackgroundTasks.LockscreenChangeTask.RunOnce();
                }
            }
        }

        private void Text_wallpapertimer_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            int time;
            if(!int.TryParse(sender.Text, out time) || time < 15)
            {
                popup.Show("请输入大于等于15的合法数字！");
                //sender.Text = "15";
                //return;
            }
            wallpaperTaskSettings.WPTimer = time;
        }

        private void Text_wallpapertag_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            wallpaperTaskSettings.WPTag = sender.Text;
        }

        private void Text_lockscreentimer_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            int time;
            if (!int.TryParse(sender.Text, out time) || time < 15)
            {
                popup.Show("请输入大于等于15的合法数字！");
                //sender.Text = "15";
                //return;
            }
            wallpaperTaskSettings.LSTimer = time;
        }

        private void Text_lockscreentag_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            wallpaperTaskSettings.LSTag = sender.Text;
        }

        private async System.Threading.Tasks.Task SetupBackGroundTaskAsync()
        {
            TerminateLockscreenTask();
            TerminateWallpaperTask();
            if (!wallpaperTaskSettings.IsEnable)
                return;

            int req = (int)await BackgroundExecutionManager.RequestAccessAsync();
            if(req != 1 || req != 4)
            {
                popup.Show("后台任务受到系统限制，任务可能无法成功运行");
            }
            if (wallpaperTaskSettings.IsWPEnable)
                SetupWallpaperTask();
            if (wallpaperTaskSettings.IsLSEnable)
                SetupLockscreenTask();
        }

        private void TerminateWallpaperTask()
        {
            if (isWallpaperTaskRegistered)
            {
                wallpaperTask.Value.Unregister(true);
            }
        }
        private void TerminateLockscreenTask()
        {
            if (isLockscreenTaskRegistered)
            {
                lockscreenTask.Value.Unregister(true);
            }
        }
        private void SetupWallpaperTask()
        {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = "wallpapertask",
                IsNetworkRequested = true,
                TaskEntryPoint = "Konachan.BackgroundTasks.WallpaperChangeTask"
            };
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.SetTrigger(new TimeTrigger((uint)wallpaperTaskSettings.WPTimer, false));
            builder.Register();
        }
        private void SetupLockscreenTask()
        {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = "lockscreentask",
                IsNetworkRequested = true,
                TaskEntryPoint = "Konachan.BackgroundTasks.LockscreenChangeTask"
            };
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.SetTrigger(new TimeTrigger((uint)wallpaperTaskSettings.LSTimer, false));
            builder.Register();
        }
    }
}

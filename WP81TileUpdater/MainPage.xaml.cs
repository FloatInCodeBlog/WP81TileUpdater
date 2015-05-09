using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WP81TileUpdater
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RegisterBackgroundTask();
        }

        //Custom name to of background task
        private const string TaskName = "MyCustomTileUpdater";
        //                                     Namespace     .ClassName
        private const string TaskEntryPoint = "BackgroundTask.AppTileUpdater";

        private async void RegisterBackgroundTask()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            //Check for permissions to use bakground tasks
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == TaskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = TaskName;
                taskBuilder.TaskEntryPoint = TaskEntryPoint;
                //Update interval
                taskBuilder.SetTrigger(new TimeTrigger(15, false));
                var registration = taskBuilder.Register();
            }
        }
    }
}

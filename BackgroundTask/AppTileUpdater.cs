namespace BackgroundTask
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Background;
    using Windows.Data.Xml.Dom;
    using Windows.Foundation;
    using Windows.Graphics.Imaging;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.UI.Xaml.Markup;
    using System.IO;

    public sealed class AppTileUpdater : XamlRenderingBackgroundTask
    {
        private const string MediumTileImage = "MediumTile.png";
        private const string MediumTileTemplate = "MediumTileTemplate.xml";

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await MediumTileImageAsync(MediumTileTemplate, MediumTileImage, new Size(150, 150));

            UpdateTile(MediumTileImage);
            deferral.Complete();
        }

        void UpdateTile(string mediumTileImage)
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
            tileUpdater.Clear();

            var mediumTileTemplate = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Image);
            var mediumTileImageAttributes = mediumTileTemplate.GetElementsByTagName("image");
            ((XmlElement)mediumTileImageAttributes.Item(0)).SetAttribute("src", Path.Combine(ApplicationData.Current.LocalFolder.Path, mediumTileImage));

            //To hide application name in tile
            var mediumBrandingAttribute = mediumTileTemplate.GetElementsByTagName("binding");
            ((XmlElement)mediumBrandingAttribute.Item(0)).SetAttribute("branding", "none");

            var mediumTileNotification = new TileNotification(mediumTileTemplate);
            tileUpdater.Update(mediumTileNotification);
        }

        private async Task MediumTileImageAsync(string inputMarkupFilename, string outputImageFilename, Size size)
        {
            StorageFile markupFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + inputMarkupFilename));
            var markupContent = await FileIO.ReadTextAsync(markupFile);

            Border border = (Border)XamlReader.Load(markupContent);

            StackPanel stackPanel = (StackPanel)border.Child;

            TextBlock timeTextBlock = (TextBlock)stackPanel.FindName("TimeTextBlock");
            timeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            await renderBitmap.RenderAsync(border, (int)size.Width, (int)size.Height);

            var buffer = await renderBitmap.GetPixelsAsync();
            DataReader dataReader = DataReader.FromBuffer(buffer);
            byte[] data = new byte[buffer.Length];
            dataReader.ReadBytes(data);

            var outputFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(outputImageFilename, CreationCollisionOption.ReplaceExisting);
            var outputFileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite);
            var encodetBits = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputFileStream);
            encodetBits.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, (uint)renderBitmap.PixelWidth, (uint)renderBitmap.PixelHeight, 96, 96, data);
            await encodetBits.FlushAsync();
        }
    }
}
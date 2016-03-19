namespace CameraControlLibrary
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Threading.Tasks;
    using global::System.Runtime.InteropServices.WindowsRuntime;
    using global::System.Linq;
    using Windows.Devices.Enumeration;
    using Windows.Foundation;
    using Windows.Graphics.Imaging;
    using Windows.Media;
    using Windows.Media.Capture;
    using Windows.Media.FaceAnalysis;
    using Windows.Media.MediaProperties;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.UI.Xaml.Controls;
    using Windows.Storage.Streams;
    using System.IO;
    using System.Diagnostics;
    public sealed partial class CameraDisplay : UserControl
    {
        public CameraDisplay()
        {
            this.InitializeComponent();
            this.capturingTask = new TaskCompletionSource<bool>();
            this.Loaded += OnLoaded;
        }
        public void SetFaceProcessor(Func<SoftwareBitmap, Task> processor)
        {
            // We don't allow you to change this once it's set - it's a
            // one shot.
            if (this.faceProcessor != null)
            {
                throw new InvalidOperationException("Already set - sorry, can't change it");
            }
            this.faceProcessor = processor;

            Task.Run(
              async () =>
              {
                  await this.capturingTask.Task;

                  this.capturingTask = null;

                  while (true)
                  {
                      if (!this.faceProcessingPaused)
                      {
                          await this.mediaCapture.GetPreviewFrameAsync(this.videoFrame);

                          await this.Dispatcher.RunAsync(
                              Windows.UI.Core.CoreDispatcherPriority.Normal,
                              async () =>
                              {
                                  await this.faceProcessor(this.videoFrame.SoftwareBitmap);
                              }
                            );
                      }
                      else
                      {
                            // cheap and cheerful.
                            await Task.Delay(1000);
                      }

                      this.faceProcessingPaused = true;
                  }
              }
            );
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var cameras = await this.GetCameraDetailsAsync();

            var firstCamera = cameras.FirstOrDefault();

            if (firstCamera != null)
            {
                MediaCaptureInitializationSettings settings =
                  new MediaCaptureInitializationSettings();

                settings.VideoDeviceId = firstCamera.Id;

                this.mediaCapture = new MediaCapture();

                await this.mediaCapture.InitializeAsync(settings);

                this.InitialiseFlowDirectionFromCameraLocation(firstCamera);

                this.captureElement.Source = this.mediaCapture;

                await this.mediaCapture.StartPreviewAsync();

                this.InialisePreviewSizesFromVideoDevice();

                this.InitialiseVideoFrameFromDetectorFormats();

                this.capturingTask.SetResult(true);
            }
        }
        void InitialiseFlowDirectionFromCameraLocation(DeviceInformation firstCamera)
        {
            var externalCamera =
              (firstCamera.EnclosureLocation?.Panel ==
              Windows.Devices.Enumeration.Panel.Unknown);

            var frontCamera =
              (!externalCamera && firstCamera.EnclosureLocation?.Panel ==
                Windows.Devices.Enumeration.Panel.Front);

            this.captureElement.FlowDirection =
              frontCamera ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        void InitialiseVideoFrameFromDetectorFormats()
        {
            var bitmapFormats = FaceDetector.GetSupportedBitmapPixelFormats();

            this.videoFrame = new VideoFrame(
              bitmapFormats.First(),
              (int)this.previewVideoSize.Width,
              (int)this.previewVideoSize.Height);
        }
        void InialisePreviewSizesFromVideoDevice()
        {
            var previewProperties =
              this.mediaCapture.VideoDeviceController.GetMediaStreamProperties(
              MediaStreamType.VideoPreview) as VideoEncodingProperties;

            this.previewVideoSize = new Size(
              previewProperties.Width, previewProperties.Height);
        }

        async Task<List<DeviceInformation>> GetCameraDetailsAsync()
        {
            var deviceInfo = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            List<DeviceInformation> returnedList = new List<DeviceInformation>();

            if (deviceInfo != null)
            {
                returnedList = deviceInfo.OrderBy(
                  di =>
                  {
                      var sortOrder = 2;

                      if (di.EnclosureLocation != null)
                      {
                          sortOrder = di.EnclosureLocation.Panel ==
                    Windows.Devices.Enumeration.Panel.Front ? 0 : 1;
                      }
                      return (sortOrder);
                  }
                ).ToList();
            }
            return (returnedList);
        }


        //Lilian changed the way this works
        public async Task<string> Snap()
        {
            this.faceProcessingPaused = true;
          
            InMemoryRandomAccessStream fPhotoStream = new InMemoryRandomAccessStream();
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            mediaCapture.CapturePhotoToStreamAsync(imageProperties, fPhotoStream).AsTask().Wait();

            fPhotoStream.FlushAsync().AsTask().Wait();
            fPhotoStream.Seek(0);

            Guid photoID = Guid.NewGuid();
            string photoName = photoID.ToString() + ".jpg";
            StorageFolder appFolder = ApplicationData.Current.LocalCacheFolder;
            StorageFile file = await appFolder.CreateFileAsync(photoName, CreationCollisionOption.ReplaceExisting);


            if (file != null)
            {

                WriteableBitmap writeableBitmap = new WriteableBitmap(300, 300);
                writeableBitmap.SetSource(fPhotoStream);

                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    // Get pixels of the WriteableBitmap object 
                    Stream pixelStream = writeableBitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    // Save the image file with jpg extension 
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)writeableBitmap.PixelWidth,
                        (uint)writeableBitmap.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                }
            }

            return photoName;
        }

        VideoFrame videoFrame;
        Func<SoftwareBitmap, Task> faceProcessor;
        MediaCapture mediaCapture;
        TaskCompletionSource<bool> capturingTask;
        Size previewVideoSize;
        public bool faceProcessingPaused;
    }
}

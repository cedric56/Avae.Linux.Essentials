using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Storage;
using System.Runtime.InteropServices;
using Mat = Emgu.CV.Mat;
using Orientation = Avalonia.Layout.Orientation;
using VideoCapture = Emgu.CV.VideoCapture;
using VideoWriter = Emgu.CV.VideoWriter;

namespace Microsoft.Maui.Media
{
    public interface ICapturePicker
    {
        Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null);
        Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null);
    }

    class CapturePickerImplementation : ICapturePicker
    {
        public async Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null)
        {
            var window = new MediaPickerImplementation.CapturePhotoWindow();
            return await window.ShowDialog<FileResult>(WindowStateManager.Default.GetActiveWindow(false));
        }
        public async Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null)
        {
            var window = new MediaPickerImplementation.CaptureVideoWindow();
            return await window.ShowDialog<FileResult>(WindowStateManager.Default.GetActiveWindow(false));
        }
    }

    class MediaPickerImplementation : IMediaPicker
    {
        ICapturePicker CapturePicker { get; set; }
        public MediaPickerImplementation(ICapturePicker? capturePicker = null)
        {
            CapturePicker = capturePicker ?? new CapturePickerImplementation();

            if(capturePicker == null)
            {
                try
                {    
                    // Optionally: preload cvextern to avoid DllNotFoundException
                    NativeLibrary.Load(Platform.LibcvexternPath);
                }
                catch (Exception e)
                {
                    IsCaptureSupported = false;
                    Console.WriteLine(e);
                }
            }
        }

        public bool IsCaptureSupported { get; set; } = true;

        public async Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });
            return result ?? null!;
        }

        public Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null)
        {
            return CapturePicker.CapturePhotoAsync(options);
        }

        public async Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Videos
            });
            return result ?? null!;
        }

        public Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null)
        {
            return CapturePicker.CaptureVideoAsync(options);
        }

        public class CapturePhotoWindow : Window
        {
            private Image _cameraImage;
            private TextBlock _noCamera;
            private Button _captureButton;
            private Button _acceptButton;
            private Button _cancelButton;

            private VideoCapture _capture;
            private System.Timers.Timer _frameTimer;
            private byte[]? _capturedImageBytes;
            private string? _tempFilePath;
            private bool _isPreviewing = false;

            public CapturePhotoWindow()
                : base()
            {
                Width = 600;
                Height = 400;
                Title = "Camera";
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                _noCamera = new TextBlock()
                {
                    Text = "No camera detected",
                    IsVisible = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                _cameraImage = new Image
                {
                    Margin = new Thickness(5),
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                _captureButton = new Button
                {
                    Content = "Capture Photo",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5)
                };
                _captureButton.Click += OnCaptureClick;

                _acceptButton = new Button
                {
                    Content = "Accept",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsVisible = false,
                    Margin = new Thickness(5)
                };
                _acceptButton.Click += OnAcceptClick;

                _cancelButton = new Button
                {
                    Content = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsVisible = false,
                    Margin = new Thickness(5)
                };
                _cancelButton.Click += OnCancelClick;

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children = { _captureButton, _acceptButton, _cancelButton },
                    Spacing = 10,
                    Margin = new Thickness(10)
                };

                var layout = new Grid
                {
                    RowDefinitions = new RowDefinitions
                    {
                        new RowDefinition(GridLength.Star),
                        new RowDefinition(GridLength.Auto)
                    }
                };

                Grid.SetRow(_cameraImage, 0);
                Grid.SetRow(_noCamera, 0);
                
                Grid.SetRow(buttonPanel, 1);
                layout.Children.Add(_cameraImage);
                layout.Children.Add(_noCamera);
                layout.Children.Add(buttonPanel);

                Content = layout;

                Opened += OnOpened;
                Closed += OnClosed;
            }

            private void OnOpened(object? sender, EventArgs e)
            {
                _capture = new VideoCapture(0); 
                if (!_capture.IsOpened)
                {
                    Console.WriteLine("Failed to open video capture device.");
                    _noCamera.IsVisible = true;
                    return;
                }
                _capture.Set(CapProp.FrameWidth, 640);
                _capture.Set(CapProp.FrameHeight, 480);

                _frameTimer = new System.Timers.Timer(33); // ~30 FPS
                _frameTimer.Elapsed += async (s, a) => { try { await UpdateFrameAsync(); } catch { } };
                _frameTimer.Start();
            }

            private async Task UpdateFrameAsync()
            {
                if (_isPreviewing) return;

                var mat = _capture.QueryFrame();
                if (mat is not null)
                {
                    var image = mat.ToImage<Bgr, byte>();
                    var bytes = image.ToJpegData();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        using var memoryStream = new MemoryStream(bytes);
                        _cameraImage.Source = new Bitmap(memoryStream);
                    });
                }
            }

            private async void OnCaptureClick(object? sender, EventArgs e)
            {
                try
                {
                    var currentFrame = new Mat();
                    _capture.Read(currentFrame);
                    if (currentFrame.IsEmpty) return;

                    var image = currentFrame.ToImage<Bgr, byte>();
                    _capturedImageBytes = image.ToJpegData();

                    _tempFilePath = Path.Combine(Path.GetTempPath(), $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                    File.WriteAllBytes(_tempFilePath, _capturedImageBytes);

                    _isPreviewing = true;
                    _frameTimer?.Stop();
                    _captureButton.IsVisible = false;
                    _acceptButton.IsVisible = true;
                    _cancelButton.IsVisible = true;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        using var memoryStream = new MemoryStream(_capturedImageBytes);
                        _cameraImage.Source = new Bitmap(memoryStream);
                    });
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            private void OnAcceptClick(object? sender, EventArgs e)
            {
                if (_tempFilePath != null)
                {
                    Close(new FileResult(_tempFilePath));
                }
            }

            private void OnCancelClick(object? sender, EventArgs e)
            {
                if (_tempFilePath != null && File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                _isPreviewing = false;
                _acceptButton.IsVisible = false;
                _cancelButton.IsVisible = false;
                _captureButton.IsVisible = true;
                _frameTimer?.Start();
            }

            private void OnClosed(object? sender, EventArgs e)
            {
                _frameTimer?.Stop();
                _capture?.Dispose();
            }
        }

        public class CaptureVideoWindow : Window
        {
            private Slider _videoSlider;
            private int _totalPreviewFrames;
            private bool _sliderDragging = false;
            private Image _cameraImage;
            private Button _recordButton;
            private Button _acceptButton;
            private Button _cancelButton;
            private TextBlock _noCamera;
            private Button _playButton;
            private Grid _sliderRow;
            private bool _isPlayingPreview = false;

            private VideoCapture _capture;
            private VideoCapture? _previewCapture;
            private VideoWriter? _videoWriter;
            private System.Timers.Timer _frameTimer;
            private string? _videoFilePath;
            private bool _isRecording = false;
            private bool _isPreviewing = false;

            public CaptureVideoWindow()
                : base()
            {
                Width = 600;
                Height = 400;
                Title = "Camera";
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                _noCamera = new TextBlock()
                {
                    Text = "No camera detected",
                    IsVisible = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                _videoSlider = new Slider
                {
                    Minimum = 0,
                    Maximum = 100,
                    IsVisible = false,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                _videoSlider.PointerPressed += (_, _) => _sliderDragging = true;
                _videoSlider.PointerReleased += (_, _) => _sliderDragging = false;
                _videoSlider.PropertyChanged += async (s, e) =>
                {
                    if (e.Property == Slider.ValueProperty && _isPreviewing && !_sliderDragging)
                    {
                        int frame = (int)_videoSlider.Value;
                        if (_previewCapture != null && _previewCapture.IsOpened)
                        {
                            _previewCapture.Set(CapProp.PosFrames, frame);
                            await ShowPreviewFrameAsync();
                        }
                    }
                };

                _playButton = new Button
                {
                    Content = "▶️",
                    Width = 40,
                    Height = 30,
                    Margin = new Thickness(5),
                    IsVisible = false
                };
                _playButton.Click += OnPlayButtonClick;

                _sliderRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto, *"),
                    //Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(5),
                    Children = { _playButton, _videoSlider },
                    IsVisible = false
                };
                Grid.SetColumn(_playButton, 0);
                Grid.SetColumn(_videoSlider, 1);

                _cameraImage = new Image
                {
                    Margin = new Thickness(5),
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                _recordButton = new Button
                {
                    Content = "Start Recording",
                    Margin = new Thickness(5)
                };
                _recordButton.Click += OnRecordClick;

                _acceptButton = new Button
                {
                    Content = "Accept",
                    IsVisible = false,
                    Margin = new Thickness(5)
                };
                _acceptButton.Click += OnAcceptClick;

                _cancelButton = new Button
                {
                    Content = "Cancel",
                    IsVisible = false,
                    Margin = new Thickness(5)
                };
                _cancelButton.Click += OnCancelClick;

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 10,
                    Margin = new Thickness(10),
                    Children = { _recordButton, _acceptButton, _cancelButton }
                };

                var layout = new Grid
                {
                    RowDefinitions = new RowDefinitions
                    {
                        new RowDefinition(GridLength.Star),
                        new RowDefinition(GridLength.Auto),
                        new RowDefinition(GridLength.Auto)
                    }
                };

                Grid.SetRow(_cameraImage, 0);
                Grid.SetRow(_noCamera, 0);
                Grid.SetRow(_sliderRow, 1);
                Grid.SetRow(buttons, 2);
                layout.Children.Add(_cameraImage);
                layout.Children.Add(_noCamera);
                layout.Children.Add(_sliderRow);
                layout.Children.Add(buttons);

                Content = layout;

                Opened += OnOpened;
                Closed += OnClosed;
            }

            private void OnOpened(object? sender, EventArgs e)
            {
                _capture = new VideoCapture(0);
                if(!_capture.IsOpened)
                {
                    _noCamera.IsVisible = true;
                    return;
                }

                _capture.Set(CapProp.FrameWidth, 640);
                _capture.Set(CapProp.FrameHeight, 480);

                _frameTimer = new System.Timers.Timer(33); // ~30 FPS
                _frameTimer.Elapsed += async (s, a) => await UpdateFrameAsync();
                _frameTimer.Start();
            }

            private async Task UpdateFrameAsync()
            {
                if (_isPreviewing) return;

                var mat = _capture.QueryFrame();
                if (mat is null || mat.IsEmpty) return;

                if (_isRecording && _videoWriter != null)
                {
                    _videoWriter.Write(mat);
                }

                using var image = mat.ToImage<Bgr, byte>();
                byte[] jpeg = image.ToJpegData();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    using var stream = new MemoryStream(jpeg);
                    _cameraImage.Source = new Bitmap(stream);
                });
            }

            private void OnRecordClick(object? sender, EventArgs e)
            {
                if (_noCamera.IsVisible)
                    return;

                if (!_isRecording)
                {
                    // Start recording
                    _videoFilePath = Path.Combine(Path.GetTempPath(), $"video_{DateTime.Now:yyyyMMdd_HHmmss}.avi");

                    _videoWriter = new VideoWriter(
                        _videoFilePath,       
                        VideoWriter.Fourcc('M','J','P','G'),
                        //FourCC.MJPG, // or MJPG
                        30,
                        new System.Drawing.Size(640, 480),
                        true
                    );

                    _isRecording = true;
                    _recordButton.Content = "Stop Recording";
                }
                else
                {
                    // Stop recording
                    _isRecording = false;
                    _videoWriter?.Dispose();
                    _videoWriter = null;

                    _isPreviewing = true;
                    _recordButton.IsVisible = false;
                    _acceptButton.IsVisible = true;
                    _cancelButton.IsVisible = true;

                    // Show video preview
                    _ = Task.Run(async () =>
                    {
                        _previewCapture = new VideoCapture(_videoFilePath);
                        _totalPreviewFrames = (int)_previewCapture.Get(CapProp.FrameCount);

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            _videoSlider.Maximum = _totalPreviewFrames - 1;
                            _videoSlider.Value = 0;
                            _videoSlider.IsVisible = true;
                            _playButton.Content = "▶️";
                            _sliderRow.IsVisible = true;
                            _playButton.IsVisible = true;
                        });

                        Mat frame = new();

                        while (_isPreviewing && _previewCapture.Read(frame) && !frame.IsEmpty)
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await ShowPreviewFrameAsync();
                                if (!_sliderDragging)
                                    _videoSlider.Value = _previewCapture.Get(CapProp.PosFrames);
                            });

                            await Task.Delay(33);
                        }
                    });
                }
            }

            private async Task ShowPreviewFrameAsync()
            {
                if (_previewCapture == null) return;

                using var frame = _previewCapture.QueryFrame();
                if (frame is null || frame.IsEmpty) return;

                using var image = frame.ToImage<Bgr, byte>();
                byte[] jpeg = image.ToJpegData();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    using var stream = new MemoryStream(jpeg);
                    _cameraImage.Source = new Bitmap(stream);
                });
            }

            private void OnAcceptClick(object? sender, EventArgs e)
            {
                if (_videoFilePath != null)
                {
                    Close(new FileResult(_videoFilePath));
                }
            }

            private async void OnPlayButtonClick(object? sender, EventArgs e)
            {
                if (_previewCapture == null) return;

                _isPlayingPreview = !_isPlayingPreview;
                _playButton.Content = _isPlayingPreview ? "⏹" : "▶️";

                if (_isPlayingPreview)
                {
                    if (_videoSlider.Value == _videoSlider.Maximum)
                        _videoSlider.Value = 0;

                    Mat frame = new();
                    while (_isPlayingPreview &&
                           _previewCapture.Read(frame) &&
                           !frame.IsEmpty &&
                           _videoSlider.Value < _videoSlider.Maximum)
                    {
                        await ShowPreviewFrameAsync();
                        if (!_sliderDragging)
                            _videoSlider.Value = _previewCapture.Get(CapProp.PosFrames);

                        await Task.Delay(33);
                    }

                    _isPlayingPreview = false;
                    _playButton.Content = "▶️";
                }
            }

            private void OnCancelClick(object? sender, EventArgs e)
            {
                if (_videoFilePath != null && File.Exists(_videoFilePath))
                    File.Delete(_videoFilePath);

                _isPreviewing = false;
                _videoSlider.IsVisible = false;
                _acceptButton.IsVisible = false;
                _cancelButton.IsVisible = false;
                _sliderRow.IsVisible = false;
                _playButton.IsVisible = false;
                _recordButton.IsVisible = true;
                _recordButton.Content = "Start Recording";
            }

            private void OnClosed(object? sender, EventArgs e)
            {
                _frameTimer?.Stop();
                _capture?.Dispose();
                _videoWriter?.Dispose();
            }
        }
    }
}

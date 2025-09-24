using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Digital_Imager
{
    public partial class Form1 : Form
    {
        private VideoCapture videoCapture;
        private Mat frame;
        private Thread cameraThread;
        private bool isWebcamRunning = false;
        private bool stopCameraThread = false;
        private Bitmap capturedImageOriginal = null;
        private Bitmap capturedImageProcessed = null;
        private List<CameraInfo> availableCameras = new List<CameraInfo>();

        public class CameraInfo
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public bool IsWorking { get; set; }

            public override string ToString()
            {
                return $"Camera {Index}: {Name}" + (IsWorking ? "" : " (Not responding)");
            }
        }

        public Form1()
        {
            InitializeComponent();
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            goBackToolStripMenuItem.Visible = false;

            HideWebcamControls();
            frame = new Mat();
        }

        private void HideWebcamControls()
        {
            if (pictureBox6 != null) pictureBox6.Visible = false;
            if (pictureBox7 != null) pictureBox7.Visible = false;
        }

        private void ShowWebcamControls()
        {
            try
            {
                if (pictureBox6 != null)
                {
                    if (pictureBox6.Image != null)
                    {
                        pictureBox6.Image.Dispose();
                        pictureBox6.Image = null;
                    }
                    pictureBox6.Visible = true;
                }
                if (pictureBox7 != null)
                {
                    if (pictureBox7.Image != null)
                    {
                        pictureBox7.Image.Dispose();
                        pictureBox7.Image = null;
                    }
                    pictureBox7.Visible = true;
                }
            }
            catch
            {
            }
        }

        private void UpdateCapturedImageDisplay()
        {
            if (pictureBox7 != null)
            {
                if (capturedImageProcessed != null)
                {
                    pictureBox7.Image = capturedImageProcessed;
                }
                else if (capturedImageOriginal != null)
                {
                    pictureBox7.Image = capturedImageOriginal;
                }
                else
                {
                    pictureBox7.Image = null;
                }
                pictureBox7.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void InitializeWebcam()
        {
            try
            {
                availableCameras.Clear();

                for (int i = 0; i < 10; i++)
                {
                    VideoCapture testCapture = null;
                    Mat testFrame = null;
                    try
                    {
                        testCapture = new VideoCapture(i);
                        Thread.Sleep(100);

                        if (testCapture.IsOpened())
                        {
                            testFrame = new Mat();
                            bool canRead = testCapture.Read(testFrame);

                            availableCameras.Add(new CameraInfo
                            {
                                Index = i,
                                Name = $"Camera Device {i}",
                                IsWorking = canRead && !testFrame.Empty()
                            });
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        try
                        {
                            testFrame?.Dispose();
                        }
                        catch { }

                        try
                        {
                            if (testCapture != null)
                            {
                                if (testCapture.IsOpened())
                                {
                                    testCapture.Release();
                                }
                                testCapture.Dispose();
                            }
                        }
                        catch { }

                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing webcam: " + ex.Message);
            }
        }

        private void CameraCapture()
        {
            try
            {
                while (!stopCameraThread && videoCapture != null && videoCapture.IsOpened())
                {
                    try
                    {
                        if (stopCameraThread || videoCapture == null)
                            break;

                        if (videoCapture.Read(frame) && !frame.Empty())
                        {
                            Bitmap bitmap = BitmapConverter.ToBitmap(frame);

                            if (pictureBox6 != null && !stopCameraThread)
                            {
                                if (pictureBox6.InvokeRequired)
                                {
                                    try
                                    {
                                        pictureBox6.Invoke(new Action(() =>
                                        {
                                            if (!stopCameraThread && pictureBox6 != null)
                                            {
                                                if (pictureBox6.Image != null)
                                                {
                                                    pictureBox6.Image.Dispose();
                                                }
                                                pictureBox6.Image = bitmap;
                                                pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
                                            }
                                            else
                                            {
                                                bitmap?.Dispose();
                                            }
                                        }));
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                        bitmap?.Dispose();
                                        break;
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        bitmap?.Dispose();
                                        break;
                                    }
                                }
                                else if (!stopCameraThread)
                                {
                                    if (pictureBox6.Image != null)
                                    {
                                        pictureBox6.Image.Dispose();
                                    }
                                    pictureBox6.Image = bitmap;
                                    pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
                                }
                                else
                                {
                                    bitmap?.Dispose();
                                }
                            }
                            else
                            {
                                bitmap?.Dispose();
                            }
                        }

                        if (stopCameraThread)
                            break;

                        Thread.Sleep(33);
                    }
                    catch
                    {
                        if (stopCameraThread || videoCapture == null || !videoCapture.IsOpened())
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!stopCameraThread)
                {
                    try
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show("Camera error: " + ex.Message);
                                StopWebcam();
                            }));
                        }
                        else
                        {
                            MessageBox.Show("Camera error: " + ex.Message);
                            StopWebcam();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void StartWebcam()
        {
            try
            {
                if (isWebcamRunning)
                {
                    StopWebcam();
                    Thread.Sleep(500);
                }

                InitializeWebcam();

                if (availableCameras == null || availableCameras.Count == 0)
                {
                    MessageBox.Show("No webcam devices found.");
                    return;
                }

                CameraInfo selectedCamera = ShowCameraSelectionDialog();
                if (selectedCamera == null)
                {
                    return;
                }

                if (frame != null && !frame.IsDisposed)
                {
                    frame.Dispose();
                }
                frame = new Mat();

                int retryCount = 0;
                bool cameraOpened = false;

                while (retryCount < 3 && !cameraOpened)
                {
                    try
                    {
                        if (videoCapture != null)
                        {
                            try
                            {
                                videoCapture.Dispose();
                            }
                            catch { }
                            videoCapture = null;
                        }

                        videoCapture = new VideoCapture(selectedCamera.Index);
                        Thread.Sleep(200);

                        if (videoCapture.IsOpened())
                        {
                            cameraOpened = true;
                        }
                        else
                        {
                            videoCapture?.Dispose();
                            videoCapture = null;
                            retryCount++;
                            if (retryCount < 3) Thread.Sleep(300);
                        }
                    }
                    catch
                    {
                        try
                        {
                            videoCapture?.Dispose();
                        }
                        catch { }
                        videoCapture = null;
                        retryCount++;
                        if (retryCount < 3) Thread.Sleep(300);
                    }
                }

                if (!cameraOpened || videoCapture == null)
                {
                    MessageBox.Show($"Failed to open camera {selectedCamera.Index} after {retryCount} attempts. Try selecting a different camera or restart the application.");
                    return;
                }

                try
                {
                    videoCapture.Set(VideoCaptureProperties.FrameWidth, 640);
                    videoCapture.Set(VideoCaptureProperties.FrameHeight, 480);
                    videoCapture.Set(VideoCaptureProperties.Fps, 30);
                }
                catch
                {
                }

                try
                {
                    Mat testFrame = new Mat();
                    bool canRead = videoCapture.Read(testFrame);
                    if (!canRead || testFrame.Empty())
                    {
                        testFrame.Dispose();
                        throw new Exception("Camera cannot capture frames");
                    }
                    testFrame.Dispose();
                }
                catch (Exception testEx)
                {
                    MessageBox.Show($"Camera {selectedCamera.Index} cannot capture frames: {testEx.Message}");
                    videoCapture?.Dispose();
                    videoCapture = null;
                    return;
                }

                isWebcamRunning = true;
                stopCameraThread = false;

                try
                {
                    cameraThread = new Thread(CameraCapture)
                    {
                        IsBackground = true,
                        Name = "CameraThread"
                    };
                    cameraThread.Start();
                }
                catch (Exception threadEx)
                {
                    MessageBox.Show($"Error starting camera thread: {threadEx.Message}");

                    isWebcamRunning = false;
                    stopCameraThread = true;
                    videoCapture?.Dispose();
                    videoCapture = null;
                    return;
                }

                ShowWebcamControls();
                button4.Visible = true;
                loadImageToolStripMenuItem.Visible = false;

                MessageBox.Show($"Camera {selectedCamera.Index} started successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting webcam: {ex.Message}");
                isWebcamRunning = false;
                stopCameraThread = true;

                try
                {
                    videoCapture?.Dispose();
                }
                catch { }
                videoCapture = null;
            }
        }

        private CameraInfo ShowCameraSelectionDialog()
        {
            Form cameraForm = new Form();
            cameraForm.Text = "Select Camera Device";
            cameraForm.Size = new System.Drawing.Size(450, 300);
            cameraForm.StartPosition = FormStartPosition.CenterParent;
            cameraForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            cameraForm.MaximizeBox = false;
            cameraForm.MinimizeBox = false;

            Label label = new Label();
            label.Text = "Available camera devices (double-click to select):";
            label.Location = new System.Drawing.Point(20, 20);
            label.Size = new System.Drawing.Size(400, 20);
            cameraForm.Controls.Add(label);

            ListBox listBox = new ListBox();
            listBox.Location = new System.Drawing.Point(20, 50);
            listBox.Size = new System.Drawing.Size(390, 150);

            foreach (var camera in availableCameras)
            {
                listBox.Items.Add(camera);
            }

            if (listBox.Items.Count > 0)
            {
                var workingCamera = availableCameras.FirstOrDefault(c => c.IsWorking);
                if (workingCamera != null)
                {
                    listBox.SelectedItem = workingCamera;
                }
                else
                {
                    listBox.SelectedIndex = 0;
                }
            }

            listBox.DoubleClick += (sender, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    cameraForm.DialogResult = DialogResult.OK;
                    cameraForm.Close();
                }
            };

            cameraForm.Controls.Add(listBox);

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new System.Drawing.Point(200, 220);
            okButton.Size = new System.Drawing.Size(75, 25);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += (sender, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    cameraForm.DialogResult = DialogResult.OK;
                    cameraForm.Close();
                }
            };
            cameraForm.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new System.Drawing.Point(285, 220);
            cancelButton.Size = new System.Drawing.Size(75, 25);
            cancelButton.DialogResult = DialogResult.Cancel;
            cameraForm.Controls.Add(cancelButton);

            cameraForm.AcceptButton = okButton;
            cameraForm.CancelButton = cancelButton;

            if (cameraForm.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
            {
                return (CameraInfo)listBox.SelectedItem;
            }

            return null;
        }

        private void StopWebcam()
        {
            try
            {
                stopCameraThread = true;
                isWebcamRunning = false;

                if (cameraThread != null && cameraThread.IsAlive)
                {
                    if (!cameraThread.Join(3000))
                    {
                    }
                    cameraThread = null;
                }

                if (videoCapture != null)
                {
                    try
                    {
                        if (videoCapture.IsOpened())
                        {
                            videoCapture.Release();
                        }
                    }
                    catch { }

                    try
                    {
                        videoCapture.Dispose();
                    }
                    catch { }

                    videoCapture = null;
                }

                if (frame != null && !frame.IsDisposed)
                {
                    try
                    {
                        frame.Dispose();
                        frame = new Mat();
                    }
                    catch
                    {
                        frame = new Mat();
                    }
                }

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => CleanupUI()));
                }
                else
                {
                    CleanupUI();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Thread.Sleep(100);
            }
            catch
            {
                isWebcamRunning = false;
                stopCameraThread = true;
                videoCapture = null;
                cameraThread = null;

                try
                {
                    if (frame != null && !frame.IsDisposed)
                    {
                        frame.Dispose();
                    }
                }
                catch { }
                frame = new Mat();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => CleanupUI()));
                }
                else
                {
                    CleanupUI();
                }
            }
        }

        private void CleanupUI()
        {
            try
            {
                if (pictureBox6 != null)
                {
                    if (pictureBox6.Image != null)
                    {
                        pictureBox6.Image.Dispose();
                        pictureBox6.Image = null;
                    }
                    pictureBox6.Visible = false;
                    pictureBox6.Refresh();
                }

                if (pictureBox7 != null)
                {
                    if (pictureBox7.Image != null)
                    {
                        pictureBox7.Image.Dispose();
                        pictureBox7.Image = null;
                    }
                    pictureBox7.Visible = false;
                }

                button4.Visible = false;
                loadImageToolStripMenuItem.Visible = true;

                if (capturedImageOriginal != null)
                {
                    capturedImageOriginal.Dispose();
                    capturedImageOriginal = null;
                }
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                    capturedImageProcessed = null;
                }
            }
            catch
            {
            }
        }

        private void CaptureWebcamImage()
        {
            try
            {
                if (videoCapture != null && videoCapture.IsOpened() && isWebcamRunning)
                {
                    Mat captureFrame = new Mat();
                    if (videoCapture.Read(captureFrame) && !captureFrame.Empty())
                    {
                        capturedImageOriginal = BitmapConverter.ToBitmap(captureFrame);

                        if (capturedImageProcessed != null)
                        {
                            capturedImageProcessed.Dispose();
                            capturedImageProcessed = null;
                        }

                        UpdateCapturedImageDisplay();
                        MessageBox.Show("Image captured successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to capture image from webcam.");
                    }
                    captureFrame.Dispose();
                }
                else
                {
                    MessageBox.Show("Webcam is not running. Please start webcam first.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error capturing image: " + ex.Message);
            }
        }

        private void startWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartWebcam();
        }

        private void stopWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWebcam();
        }

        private void captureImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureWebcamImage();
        }

        private void resetImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Visible)
            {
                if (pictureBox3.Image != null) { pictureBox3.Image.Dispose(); pictureBox3.Image = null; }
                if (pictureBox4.Image != null) { pictureBox4.Image.Dispose(); pictureBox4.Image = null; }
                if (pictureBox5.Image != null) { pictureBox5.Image.Dispose(); pictureBox5.Image = null; }
            }
            else
            {
                if (pictureBox1.Image != null) { pictureBox1.Image.Dispose(); pictureBox1.Image = null; }
                if (pictureBox2.Image != null) { pictureBox2.Image.Dispose(); pictureBox2.Image = null; }
            }

            if (pictureBox6 != null && pictureBox6.Image != null)
            {
                pictureBox6.Image.Dispose();
                pictureBox6.Image = null;
            }
            if (pictureBox7 != null && pictureBox7.Image != null)
            {
                pictureBox7.Image.Dispose();
                pictureBox7.Image = null;
            }

            if (capturedImageOriginal != null)
            {
                capturedImageOriginal.Dispose();
                capturedImageOriginal = null;
            }
            if (capturedImageProcessed != null)
            {
                capturedImageProcessed.Dispose();
                capturedImageProcessed = null;
            }
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isWebcamRunning)
            {
                StopWebcam();
            }

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }
                pictureBox1.Image = new Bitmap(openFile.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image sourceImage = null;
            PictureBox targetPictureBox = null;

            if (capturedImageOriginal != null)
            {
                sourceImage = capturedImageOriginal;
            }
            else if (pictureBox1.Image != null)
            {
                sourceImage = pictureBox1.Image;
                targetPictureBox = pictureBox2;
            }

            if (sourceImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(sourceImage);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                    bmp.SetPixel(x, y, grayColor);
                }
            }

            if (capturedImageOriginal != null)
            {
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                }
                capturedImageProcessed = bmp;
                UpdateCapturedImageDisplay();
            }
            else if (targetPictureBox != null)
            {
                if (targetPictureBox.Image != null)
                {
                    targetPictureBox.Image.Dispose();
                }
                targetPictureBox.Image = bmp;
                targetPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image sourceImage = null;
            PictureBox targetPictureBox = null;

            if (capturedImageOriginal != null)
            {
                sourceImage = capturedImageOriginal;
            }
            else if (pictureBox1.Image != null)
            {
                sourceImage = pictureBox1.Image;
                targetPictureBox = pictureBox2;
            }

            if (sourceImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(sourceImage);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, pixelColor);
                }
            }

            if (capturedImageOriginal != null)
            {
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                }
                capturedImageProcessed = bmp;
                UpdateCapturedImageDisplay();
            }
            else if (targetPictureBox != null)
            {
                if (targetPictureBox.Image != null)
                {
                    targetPictureBox.Image.Dispose();
                }
                targetPictureBox.Image = bmp;
                targetPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image sourceImage = null;
            PictureBox targetPictureBox = null;

            if (capturedImageOriginal != null)
            {
                sourceImage = capturedImageOriginal;
            }
            else if (pictureBox1.Image != null)
            {
                sourceImage = pictureBox1.Image;
                targetPictureBox = pictureBox2;
            }

            if (sourceImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(sourceImage);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int r = Math.Min(255, Math.Max(0, 255 - pixelColor.R));
                    int g = Math.Min(255, Math.Max(0, 255 - pixelColor.G));
                    int b = Math.Min(255, Math.Max(0, 255 - pixelColor.B));
                    Color invertedColor = Color.FromArgb(r, g, b);
                    bmp.SetPixel(x, y, invertedColor);
                }
            }

            if (capturedImageOriginal != null)
            {
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                }
                capturedImageProcessed = bmp;
                UpdateCapturedImageDisplay();
            }
            else if (targetPictureBox != null)
            {
                if (targetPictureBox.Image != null)
                {
                    targetPictureBox.Image.Dispose();
                }
                targetPictureBox.Image = bmp;
                targetPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image sourceImage = null;
            PictureBox targetPictureBox = null;

            if (capturedImageOriginal != null)
            {
                sourceImage = capturedImageOriginal;
            }
            else if (pictureBox1.Image != null)
            {
                sourceImage = pictureBox1.Image;
                targetPictureBox = pictureBox2;
            }

            if (sourceImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(sourceImage);
            int[] hist = new int[256];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    hist[grayValue]++;
                }
            }

            int histWidth = 256;
            int histHeight = 200;
            int max = hist.Max();

            if (max == 0) max = 1;

            Bitmap histImage = new Bitmap(histWidth, histHeight);
            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);

                for (int i = 0; i < 256; i++)
                {
                    int val = (int)((hist[i] / (float)max) * histHeight);
                    g.DrawLine(Pens.Black, i, histHeight, i, histHeight - val);
                }
            }

            if (capturedImageOriginal != null)
            {
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                }
                capturedImageProcessed = histImage;
                UpdateCapturedImageDisplay();
            }
            else if (targetPictureBox != null)
            {
                if (targetPictureBox.Image != null)
                {
                    targetPictureBox.Image.Dispose();
                }
                targetPictureBox.Image = histImage;
                targetPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image sourceImage = null;
            PictureBox targetPictureBox = null;

            if (capturedImageOriginal != null)
            {
                sourceImage = capturedImageOriginal;
            }
            else if (pictureBox1.Image != null)
            {
                sourceImage = pictureBox1.Image;
                targetPictureBox = pictureBox2;
            }

            if (sourceImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(sourceImage);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int newR = (int)Math.Min(255, ((0.393 * pixelColor.R) + (0.769 * pixelColor.G) + (0.189 * pixelColor.B)));
                    int newG = (int)Math.Min(255, ((0.349 * pixelColor.R) + (0.686 * pixelColor.G) + (0.168 * pixelColor.B)));
                    int newB = (int)Math.Min(255, ((0.272 * pixelColor.R) + (0.534 * pixelColor.G) + (0.131 * pixelColor.B)));

                    Color sepiaColor = Color.FromArgb(newR, newG, newB);
                    bmp.SetPixel(x, y, sepiaColor);
                }
            }

            if (capturedImageOriginal != null)
            {
                if (capturedImageProcessed != null)
                {
                    capturedImageProcessed.Dispose();
                }
                capturedImageProcessed = bmp;
                UpdateCapturedImageDisplay();
            }
            else if (targetPictureBox != null)
            {
                if (targetPictureBox.Image != null)
                {
                    targetPictureBox.Image.Dispose();
                }
                targetPictureBox.Image = bmp;
                targetPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image imageToSave = null;

            if (capturedImageProcessed != null)
            {
                imageToSave = capturedImageProcessed;
            }
            else if (capturedImageOriginal != null)
            {
                imageToSave = capturedImageOriginal;
            }
            else if (pictureBox5.Visible && pictureBox5.Image != null)
            {
                imageToSave = pictureBox5.Image;
            }
            else if (pictureBox2.Visible && pictureBox2.Image != null)
            {
                imageToSave = pictureBox2.Image;
            }
            else if (pictureBox1.Image != null)
            {
                imageToSave = pictureBox1.Image;
            }

            if (imageToSave == null)
            {
                MessageBox.Show("No image to save. Please process or load an image first.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                sfd.Title = "Save Image As";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat format = ImageFormat.Png;

                    switch (Path.GetExtension(sfd.FileName).ToLower())
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                    }

                    try
                    {
                        imageToSave.Save(sfd.FileName, format);
                        MessageBox.Show("Image saved successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving image: {ex.Message}");
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isWebcamRunning)
            {
                StopWebcam();
            }
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeWebcam();
        }

        private void swtichToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isWebcamRunning)
            {
                StopWebcam();
            }

            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;

            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;

            processToolStripMenuItem.Visible = false;
            swtichToToolStripMenuItem.Visible = false;
            loadImageToolStripMenuItem.Visible = false;
            cameraToolStripMenuItem.Visible = false;
            goBackToolStripMenuItem.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (pictureBox3.Image != null)
                {
                    pictureBox3.Image.Dispose();
                }
                pictureBox3.Image = new Bitmap(openFile.FileName);
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (pictureBox4.Image != null)
                {
                    pictureBox4.Image.Dispose();
                }
                pictureBox4.Image = new Bitmap(openFile.FileName);
                pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null || pictureBox4.Image == null)
            {
                MessageBox.Show("Please load both foreground and background images first.");
                return;
            }

            Bitmap imageB = new Bitmap(pictureBox3.Image);
            Bitmap imageA = new Bitmap(pictureBox4.Image);

            if (imageB.Width != imageA.Width || imageB.Height != imageA.Height)
            {
                MessageBox.Show("Both images must have the same dimensions for green screen subtraction.");
                return;
            }

            Bitmap resultImage = new Bitmap(imageB.Width, imageB.Height);

            Color myGreen = Color.FromArgb(0, 255, 0);
            int threshold = 5;

            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixel = imageB.GetPixel(x, y);
                    Color backPixel = imageA.GetPixel(x, y);

                    if (pixel.G > threshold && pixel.G > pixel.R + 30 && pixel.G > pixel.B + 30)
                    {
                        resultImage.SetPixel(x, y, backPixel);
                    }
                    else
                    {
                        resultImage.SetPixel(x, y, pixel);
                    }
                }
            }

            if (pictureBox5.Image != null)
            {
                pictureBox5.Image.Dispose();
            }
            pictureBox5.Image = resultImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;

            imageB.Dispose();
            imageA.Dispose();
        }

        private void goBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;

            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;

            processToolStripMenuItem.Visible = true;
            swtichToToolStripMenuItem.Visible = true;
            loadImageToolStripMenuItem.Visible = true;
            cameraToolStripMenuItem.Visible = true;
            goBackToolStripMenuItem.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CaptureWebcamImage();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isWebcamRunning)
            {
                StopWebcam();
            }

            if (frame != null && !frame.IsDisposed)
            {
                frame.Dispose();
            }

            base.OnFormClosing(e);
        }

        private void stopWebcamToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            StopWebcam();
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Digital_Imager
{
    public partial class Form1 : Form
    {
        Bitmap loaded, processed, originalImage;
        Bitmap imageB, imageA, resultImage;
        Bitmap capturedImage; // Store captured webcam image
        OpenFileDialog openFileDialog;
        SaveFileDialog saveFileDialog;

        // OpenCV webcam variables
        private VideoCapture capture;
        private Mat frame;
        private Timer webcamTimer;
        private bool isWebcamRunning = false;

        // Add tracking variable for subtraction mode
        private bool isInSubtractionMode = false;

        public Form1()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG|*.jpg|PNG|*.png|Bitmap|*.bmp";

            // Initialize webcam timer
            webcamTimer = new Timer();
            webcamTimer.Interval = 33; // ~30 FPS
            webcamTimer.Tick += WebcamTimer_Tick;
        }

        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            if (capture != null && capture.IsOpened())
            {
                capture.Read(frame);
                if (!frame.Empty())
                {
                    pictureBox6.Image = BitmapConverter.ToBitmap(frame);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set PictureBox properties
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;

            // Hide subtraction controls initially
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;

            // Initialize menu visibility
            swtichToToolStripMenuItem.Visible = true;   // "Switch to Subtraction" menu
            goBackToolStripMenuItem.Visible = false;    // "Go Back" menu
            isInSubtractionMode = false;
        }

        // Helper method to ensure bitmap is in correct format
        private Bitmap ConvertToCompatibleBitmap(Bitmap source)
        {
            Bitmap newBitmap = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(source, 0, 0);
            }
            return newBitmap;
        }

        // File Menu Handlers
        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap tempBitmap = new Bitmap(openFileDialog.FileName);
                loaded = ConvertToCompatibleBitmap(tempBitmap);
                tempBitmap.Dispose();
                originalImage = (Bitmap)loaded.Clone();
                pictureBox1.Image = loaded;
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (processed != null)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    processed.Save(saveFileDialog.FileName);
                    MessageBox.Show("Image saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No processed image to save!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            pictureBox7.Image = null;
            loaded = null;
            processed = null;
            originalImage = null;
            imageA = null;
            imageB = null;
            resultImage = null;
            capturedImage = null;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Basic Process Handlers
        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                for (int y = 0; y < processed.Height; y++)
                {
                    for (int x = 0; x < processed.Width; x++)
                    {
                        Color pixel = processed.GetPixel(x, y);
                        int gray = (int)((pixel.R * 0.3) + (pixel.G * 0.59) + (pixel.B * 0.11));
                        processed.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                for (int y = 0; y < processed.Height; y++)
                {
                    for (int x = 0; x < processed.Width; x++)
                    {
                        Color pixel = processed.GetPixel(x, y);
                        processed.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                    }
                }

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                // First convert to grayscale
                Bitmap grayImage = (Bitmap)loaded.Clone();
                for (int y = 0; y < grayImage.Height; y++)
                {
                    for (int x = 0; x < grayImage.Width; x++)
                    {
                        Color pixel = grayImage.GetPixel(x, y);
                        int gray = (int)((pixel.R * 0.3) + (pixel.G * 0.59) + (pixel.B * 0.11));
                        grayImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }

                // Calculate histogram
                int[] histogram = new int[256];
                for (int y = 0; y < grayImage.Height; y++)
                {
                    for (int x = 0; x < grayImage.Width; x++)
                    {
                        Color pixel = grayImage.GetPixel(x, y);
                        histogram[pixel.R]++;
                    }
                }

                // Find max value for scaling
                int maxValue = 0;
                for (int i = 0; i < 256; i++)
                {
                    if (histogram[i] > maxValue)
                        maxValue = histogram[i];
                }

                // Create histogram image
                int histWidth = 256;
                int histHeight = 100;
                Bitmap histogramImage = new Bitmap(histWidth, histHeight);

                using (Graphics g = Graphics.FromImage(histogramImage))
                {
                    g.Clear(Color.White);

                    // Draw histogram bars
                    for (int i = 0; i < 256; i++)
                    {
                        int barHeight = (int)((histogram[i] / (float)maxValue) * histHeight);
                        g.DrawLine(Pens.Black, i, histHeight, i, histHeight - barHeight);
                    }
                }

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = histogramImage;
                else
                    pictureBox2.Image = histogramImage;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                for (int y = 0; y < processed.Height; y++)
                {
                    for (int x = 0; x < processed.Width; x++)
                    {
                        Color pixel = processed.GetPixel(x, y);
                        int tr = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                        int tg = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                        int tb = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                        int r = tr > 255 ? 255 : tr;
                        int g = tg > 255 ? 255 : tg;
                        int b = tb > 255 ? 255 : tb;

                        processed.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Convolution Filter Handlers - FIXED to properly set processed bitmap
        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.Smooth(processed, 1);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.GaussianBlur(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.Sharpen(processed, 11);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void meanRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.MeanRemoval(processed, 9);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void embossLaplascianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.EmbossLaplascian(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void embossHorzVertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.EmbossHorzVert(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void embossAllDirectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.EmbossAllDirections(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void embossHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.EmbossHorizontal(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void embossVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed = (Bitmap)loaded.Clone();
                BitmapFilter.EmbossVertical(processed);

                // Display in appropriate pictureBox based on current view
                if (isWebcamRunning)
                    pictureBox7.Image = processed;
                else
                    pictureBox2.Image = processed;
            }
            else
            {
                MessageBox.Show("Please load an image first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Image Subtraction View Handlers - FIXED
        private void swtichToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Hide normal view and webcam view
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            button4.Visible = false;

            // Show subtraction view
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;

            // Update menu visibility
            swtichToToolStripMenuItem.Visible = false;  // Hide "Switch to Subtraction"
            cameraToolStripMenuItem.Visible = false;
            convolutionFiltersToolStripMenuItem.Visible = false;
            cameraToolStripMenuItem.Visible = false;
            processToolStripMenuItem.Visible = false;
            goBackToolStripMenuItem.Visible = true;     // Show "Go Back"
            isInSubtractionMode = true;
        }

        private void goBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Hide subtraction view
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;

            // Show normal view only if webcam is not running
            if (!isWebcamRunning)
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox6.Visible = false;
                pictureBox7.Visible = false;
                button4.Visible = false;
            }
            // If webcam is running, keep webcam view visible
            else
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = false;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                button4.Visible = true;
            }

            // Update menu visibility
            swtichToToolStripMenuItem.Visible = true;   // Show "Switch to Subtraction"
            cameraToolStripMenuItem.Visible = true;
            convolutionFiltersToolStripMenuItem.Visible = true;
            cameraToolStripMenuItem.Visible = true;
            processToolStripMenuItem.Visible = true;
            goBackToolStripMenuItem.Visible = false;    // Hide "Go Back"
            isInSubtractionMode = false;
        }

        // Image Subtraction Button Handlers
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap tempBitmap = new Bitmap(openFileDialog.FileName);
                imageB = ConvertToCompatibleBitmap(tempBitmap);
                tempBitmap.Dispose();
                pictureBox3.Image = imageB;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap tempBitmap = new Bitmap(openFileDialog.FileName);
                imageA = ConvertToCompatibleBitmap(tempBitmap);
                tempBitmap.Dispose();
                pictureBox4.Image = imageA;
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


        // Camera Handlers with OpenCV - UPDATED
        private void startWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isWebcamRunning)
                {
                    frame = new Mat();
                    capture = new VideoCapture(0); // 0 for default camera

                    if (capture.IsOpened())
                    {
                        // Only show camera view if not in subtraction mode
                        if (!isInSubtractionMode)
                        {
                            pictureBox1.Visible = false;
                            pictureBox2.Visible = false;
                        }

                        pictureBox6.Visible = true;
                        pictureBox7.Visible = true;
                        button4.Visible = true;

                        webcamTimer.Start();
                        isWebcamRunning = true;
                        MessageBox.Show("Webcam started successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to open webcam!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting webcam: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void stopWebcamToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (isWebcamRunning)
            {
                webcamTimer.Stop();

                if (capture != null)
                {
                    capture.Dispose();
                }

                if (frame != null)
                {
                    frame.Dispose();
                }

                isWebcamRunning = false;

                pictureBox6.Image = null;
                pictureBox6.Visible = false;
                pictureBox7.Visible = false;
                button4.Visible = false;

                // Only show normal view if not in subtraction mode
                if (!isInSubtractionMode)
                {
                    pictureBox1.Visible = true;
                    pictureBox2.Visible = true;
                }

                MessageBox.Show("Webcam stopped!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Capture image from webcam
            if (pictureBox6.Image != null)
            {
                // Convert to compatible format for processing
                Bitmap tempBitmap = new Bitmap(pictureBox6.Image);
                capturedImage = ConvertToCompatibleBitmap(tempBitmap);
                tempBitmap.Dispose();

                // Display captured image in pictureBox7
                pictureBox7.Image = capturedImage;

                // Also set as loaded for filter processing
                loaded = (Bitmap)capturedImage.Clone();
                originalImage = (Bitmap)loaded.Clone();

                MessageBox.Show("Image captured! Apply filters from Process or Convolution Filters menu. The result will appear in pictureBox7.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No webcam image to capture!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Add any click handling if needed
        }

        // Cleanup on form closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isWebcamRunning)
            {
                webcamTimer.Stop();
                if (capture != null)
                {
                    capture.Dispose();
                }
                if (frame != null)
                {
                    frame.Dispose();
                }
            }
            base.OnFormClosing(e);
        }
    }
}
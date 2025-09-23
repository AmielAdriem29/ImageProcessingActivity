using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digital_Imager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;
            pictureBox5.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            goBackToolStripMenuItem.Visible = false;
        }

        private void resetImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Visible)
            {
                pictureBox3.Image = null;
                pictureBox4.Image = null;
                pictureBox5.Image = null;
            }
            else 
            {
                pictureBox1.Image = null;
                pictureBox2.Image = null;
            }
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFile.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) { 
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
            for(int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R + pixelColor.G + pixelColor.B)/3;
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                    bmp.SetPixel(x, y, grayColor);
                }
            }

            pictureBox2.Image = bmp;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }


        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, pixelColor);
                }
            }

            pictureBox2.Image = bmp;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
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

            pictureBox2.Image = bmp;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
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

            pictureBox2.Image = histImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int newR = (int)Math.Min(225, ((0.393 * pixelColor.R) + (0.769 * pixelColor.G) + (0.189 * pixelColor.B)));
                    int newG = (int)Math.Min(255, ((0.349 * pixelColor.R) + (0.686 * pixelColor.G) + (0.168 * pixelColor.B)));
                    int newB = (int)Math.Min(255, ((0.272 * pixelColor.R) + (0.534 * pixelColor.G) + (0.131 * pixelColor.B)));
                        
                    Color invertedColor = Color.FromArgb(newR, newG, newB);
                    bmp.SetPixel(x, y, invertedColor);
                }
            }

            pictureBox2.Image = bmp;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image imageToSave = null;

            if (pictureBox5.Visible && pictureBox5.Image != null)
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
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                    switch (Path.GetExtension(sfd.FileName).ToLower())
                    {
                        case ".jpg":
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = System.Drawing.Imaging.ImageFormat.Bmp;
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
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void swtichToToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            goBackToolStripMenuItem.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.Image = new Bitmap(openFile.FileName);
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox4.Image = new Bitmap(openFile.FileName);
                pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap imageB, imageA, colorgreen;
            imageB = new Bitmap(pictureBox3.Image);
            imageA = new Bitmap(pictureBox4.Image);

            if (imageB == null || imageA == null)
            {
                MessageBox.Show("Please load both foreground and background images first.");
                return;
            }

            Bitmap resultImage = new Bitmap(imageB.Width, imageB.Height);

            Color myGreen = Color.FromArgb(0, 255, 0);
            int greygreen = (myGreen.R + myGreen.G + myGreen.B) / 3;
            int threshold = 5;

            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixel = imageB.GetPixel(x, y);
                    Color backPixel = imageA.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractValue = Math.Abs(grey - greygreen);
                    if (pixel.G > threshold && pixel.G > pixel.R + 30 && pixel.G > pixel.B + 30)
                    {
                        // Replace with background
                        resultImage.SetPixel(x, y, backPixel);
                    }
                    else
                    {
                        // Keep foreground
                        resultImage.SetPixel(x, y, pixel);
                    }
                }
            }

            // Show in pictureBox5
            pictureBox5.Image = resultImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
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
            goBackToolStripMenuItem.Visible = false;
        }
    }
}

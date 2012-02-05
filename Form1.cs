using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Collections;

using System.Threading;
using System.Windows;

namespace RPQ
{

    public partial class Form1 : Form
    {
        public Semaphore semaphore = null;
        public Dictionary <int, InterpolationMode> InterpolateQuality;
        public string pathToSave ;
        public string pathToRamka;
        Thread []threads;
        ImageCodecInfo[] iciCodecs, iciDecodres;
        ArrayList supportedExtensions;
        List<string> AllowedExt = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        public bool checkFileForOpen(FileInfo file)
        {
            string ext = file.Extension; 
            if (ext.Length == 0) return false;
            ext = ext.ToUpper().Substring(1);
            int i, n = supportedExtensions.Count;
            for (i = 0; i < n; i++)
            {
                if (ext == supportedExtensions[i].ToString()) return true;                                
            }
            return false;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (fbdOpen.ShowDialog() == DialogResult.OK)
            {
                LoadFiles(fbdOpen.SelectedPath);
            }

        }
        private void LoadFiles(string path)
        {
            lstPhotos.Items.Clear();
           // lstPhotos.Items.Add("d:\\ftp\\Izobilnoe-Gurzuf\\IMG_3683.jpg");
            foreach (string filename in Directory.GetFiles(path))
            {
                var fiPicture = new FileInfo(filename);
                if (checkFileForOpen(fiPicture))
                    lstPhotos.Items.Add(filename);
            }
            prgReduce.Maximum = lstPhotos.Items.Count;
            lblCountInputImg.Text = lstPhotos.Items.Count.ToString();
        }
        public static int GetSize(double mPix, double h, double w)
        {
            return (int)Math.Sqrt(w * mPix * 1000000 / h);
        }
        public static void ConvertImage(object data)
        {
            myData pData = (myData)data;
            string dbg = string.Empty;
            try
            {

               
                ImageCodecInfo iciJpegCodec = null;
                var epQuality = new EncoderParameter(Encoder.Quality, pData.jpegQuality);

                var iciCodecs = ImageCodecInfo.GetImageEncoders();

                var epParameters = new EncoderParameters(1);

                epParameters.Param[0] = epQuality;

                iciJpegCodec = pData.Codek;
                for (int i = 0; i < pData.path.Length; i++)
                {

                    
                    if (pData.path == null || pData.path.Length == 0 || !File.Exists(pData.path[i]))
                    {
                        continue;
                    }
                    Image newImage;
                    Bitmap bmp;
                    using (FileStream fs = File.OpenRead(pData.path[i]))
                    {
                       newImage = Bitmap.FromFile(pData.path[i]);
                    }


 
                     
                    dbg = pData.path[i];
                    if (pData.mPix != -1) pData.width = Form1.GetSize(pData.mPix, newImage.Height, newImage.Width);


                    if (pData.width > 0 || pData.Ramka != null)
                    {
                        if (pData.width <= 0) pData.width = newImage.Width;
                        bmp = Form1.FixedSize((Image)newImage, ref pData.Ramka, pData.width, pData.width * newImage.Height / newImage.Width, pData.interpolationMode);
                    }
                    else
                    {
                        bmp = ExtractBitmapFromOpenedFile(newImage);
                        newImage.Dispose();
                    }

                    var fiPicture = new FileInfo(pData.path[i]);
                    string s = fiPicture.Name, fullName;
                    s = s.Remove(s.Length - fiPicture.Extension.Length + 1) + iciJpegCodec.FilenameExtension.Split(';')[0].Substring(2);
                    fullName = pData.pathToSave + "\\" + s;
                    PathCheck.CheckExistFile(ref fullName);
                    
                    if (iciJpegCodec != null)
                    {
                        using (FileStream fs = File.OpenWrite(fullName))
                        {
                            bmp.Save(fs, iciJpegCodec, epParameters);
                            bmp.Dispose();
                        }
                    }
                    pData.Increment();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + dbg);

            }
            finally
            {
                
                if (pData != null && pData.Ramka != null) pData.Ramka.Dispose();
                pData.endProcess.Set();
            }
        }
        private static void WaitProcess(object state)
        {
            WaitParametrsType wt = state as WaitParametrsType;
            WaitHandle.WaitAll(wt.waitHandles);
            wt.progressBar.Invoke((MethodInvoker)delegate()
            {
                wt.progressBar.Value = 0;
            });
            wt.releaseButton.Invoke((MethodInvoker)delegate()
            {
                wt.releaseButton.Enabled = true;
            });

            MessageBox.Show("Конвертация завершена");

 
        }
        private static Bitmap ExtractBitmapFromOpenedFile(Image curentImage)
        {
            Bitmap bitmapToSave = new Bitmap(curentImage.Width, curentImage.Height, PixelFormat.Format24bppRgb);
            bitmapToSave.SetResolution(curentImage.Width, curentImage.Height);

            using (Graphics graphics = Graphics.FromImage(bitmapToSave))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(curentImage,
                                   new Rectangle(0, 0, curentImage.Width, curentImage.Height),
                                   0, 0, curentImage.Width, curentImage.Height,
                                   GraphicsUnit.Pixel);
                
            }
            return bitmapToSave;
        }

        //--------------

        //private static void SaveBitmapByExtension(string fullFileName, Image curentImage, ImageFormat imageFormat)
        //{
        //    if (!string.IsNullOrEmpty(fullFileName))
        //    {
        //        Bitmap bitmapToSave = ExtractBitmapFromOpenedFile(curentImage);

        //        EncoderParameters encoderParameters = new EncoderParameters(1);
        //        encoderParameters.Param[0] =
        //            new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, imageQuality);
        //        bitmapToSave.Save(fullFileName, GetEncoder(imageFormat), encoderParameters);

        //        bitmapToSave.Dispose();
        //    }
        //}
        public static Bitmap FixedSize(Image imgPhoto, ref Image Ramka, int Width, int Height, InterpolationMode interpolationMode)
        {
            try
            {
                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                Bitmap bmPhoto = new Bitmap(Width, Height);

                bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                 imgPhoto.VerticalResolution);
                using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
                {
                    grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                    grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    grPhoto.Clear(Color.White);
                    grPhoto.InterpolationMode = interpolationMode;

                    grPhoto.DrawImage(imgPhoto,
                        new Rectangle(0, 0, Width, Height),
                        new Rectangle(0, 0, sourceWidth, sourceHeight),
                        GraphicsUnit.Pixel);

                    if (Ramka != null)
                    {

                        grPhoto.DrawImage(Ramka,
                                         new Rectangle(0, 0, Width, Height),
                                         new Rectangle(0, 0, Ramka.Width, Ramka.Height),
                                         GraphicsUnit.Pixel);
                    }

                }
                return bmPhoto;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                if (imgPhoto != null) imgPhoto.Dispose();
            }
        }
        private void btnReduce_Click(object sender, EventArgs e)
        {
            try
            {
                btnReduce.Enabled = false;
                if (lstPhotos.Items.Count == 0) throw new Exception("Добавтье хотя бы 1 картинку");
                if (threads[0] != null)
                {
                    for (int i = 0; i < 32; i++)
                        if (threads[i] != null && threads[i].IsAlive)
                            throw new Exception("Погоди, ещё не все фотки перекодировались!");
                }
               
               
                int n = lstPhotos.Items.Count;
                int  width = -1;
                double mPix = -1;
                string[] list = new string[n];
                 
                
                for (int i = 0; i < n; i++)
                {
                    list[i] = lstPhotos.Items[i].ToString();
                }
                if (checkBox1.Checked)
                {
                    if (radioButton1.Checked)
                    {
                        width = Convert.ToInt32(textBox1.Text);
                        mPix = -1;
                        if (width < 0 || width > 32768) throw new Exception("Ширина должна быть в диапазоне от 1 до 32768");
                    }
                    else {

                        mPix = Convert.ToDouble(textBox2.Text);
                    }
                }
                else width = -1;
                prgReduce.Maximum = list.Length;
                prgReduce.Minimum = 0;
                int Cores = comboBox3.SelectedIndex + 1;
                int numOfTask = list.Length / Cores + 1;
                string[,] taskList = new string[Cores, numOfTask];
                ManualResetEvent[] waitHandels = new ManualResetEvent[Cores];
                
                for (int i = 0; i < Cores; i++)
                {
                    for (int j = 0; j < numOfTask; j++)
                    {
                        if (i * numOfTask + j >= list.Length) break;
                        taskList[i, j] = list[i * numOfTask + j];
                    }
                    waitHandels[i] = new ManualResetEvent(false);
                }

                Directory.CreateDirectory(pathToSave);
                for (int i = 0; i < Cores; i++)
                {
                    Image Ramka = null;
                    if (pathToRamka.Length > 0) Ramka = Image.FromFile(pathToRamka);

                    threads[i] = new Thread(Form1.ConvertImage);
                    string[] signleTask = new string[numOfTask];
                    for(int j = 0; j < numOfTask; j++)
                        signleTask[j] = taskList[i,j];
                    


                    myData data = new myData(signleTask, pathToSave, prgReduce, width,
                                           iciCodecs[comboBox1.SelectedIndex],
                                           InterpolateQuality[comboBox2.SelectedIndex],
                                           numQual.Value , Ramka, mPix, waitHandels[i]
                                           );
                    threads[i].Start(data);
                }
                pathToRamka = "";
                WaitParametrsType wt = new WaitParametrsType()
                {
                    progressBar = prgReduce,
                    releaseButton = btnReduce,
                    waitHandles = waitHandels
                };
                Thread waitThread = new Thread(new ParameterizedThreadStart(WaitProcess));
                waitThread.Start(wt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
        private void numQual_Scroll(object sender, EventArgs e)
        {
            qlabel.Text = numQual.Value + " %";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                
                iciCodecs = ImageCodecInfo.GetImageEncoders();
                if (iciCodecs.Length == 0)
                {
                    MessageBox.Show("This program can not find any encoders...");
                    this.Close();
                }
                iciDecodres = ImageCodecInfo.GetImageDecoders();
                for (int i = 0; i < iciCodecs.Length; i++)
                {
                    comboBox1.Items.Add(iciCodecs[i].MimeType);
                }
                comboBox1.SelectedIndex = 1;
                InterpolateQuality = new Dictionary<int, InterpolationMode>();
                InterpolateQuality[0] = InterpolationMode.Bicubic;
                InterpolateQuality[1] = InterpolationMode.Bilinear;
                InterpolateQuality[2] = InterpolationMode.High;
                InterpolateQuality[3] = InterpolationMode.HighQualityBicubic;
                InterpolateQuality[4] = InterpolationMode.HighQualityBilinear;
                InterpolateQuality[5] = InterpolationMode.Low;
                InterpolateQuality[6] = InterpolationMode.NearestNeighbor;
                for (int i = 0; i < InterpolateQuality.Count; i++)
                {
                    comboBox2.Items.Add(InterpolateQuality[i].ToString());
                }
                comboBox2.SelectedIndex = 3;
                supportedExtensions = new ArrayList();
                for (int i = 0; i < iciDecodres.Length; i++)
                {

                    string[] ext = iciDecodres[i].FilenameExtension.Split(';');
                    for (int j = 0; j < ext.Length; j++)
                    {
                        supportedExtensions.Add(ext[j].Substring(2));
                    }
                }
                pathToSave = "c:\\PictureQualityImages\\";
                //LoadFiles("d:\\ftp\\Izobilnoe-Gurzuf\\");
                threads = new Thread[32];
                pathToRamka = "";
                for (int i = 0; i < 32; i++)
                    threads[i] = null;

                for (int i = 1; i <= Math.Min(Environment.ProcessorCount,31); i++)
                {
                    comboBox3.Items.Add(i.ToString());
                }
                comboBox3.SelectedIndex = Environment.ProcessorCount - 1;
                lstPhotos.MultiColumn = true;
                this.AllowDrop = true;
                lstPhotos.AllowDrop = true;
                InitAllowedExtension();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 1) numQual.Enabled = false;
            else numQual.Enabled = true;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false; 
                comboBox2.Enabled = false;
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
            }
            else
            {
                if (radioButton1.Checked)
                    textBox1.Enabled = true;
                else 
                    textBox2.Enabled = true;
                
                comboBox2.Enabled = true;
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (fbdSave.ShowDialog() == DialogResult.OK)
            {
                pathToSave = fbdSave.SelectedPath;
            }

        }
       
        private void ДобавлениеРамкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ВыборРамки.ShowDialog() == DialogResult.OK)
            {
                pathToRamka = ВыборРамки.FileName;
            }
        }
        private void оПрограммеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string message = "Конвертор картинок 1.0\nНаписал Абоимов Сергей\naboimov@gmail.com\n";
            string caption = "Информация";
            MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.OK;
            MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information;
            MessageBoxOptions options = MessageBoxOptions.RightAlign;
            MessageBox.Show(message, caption, buttons, icon, System.Windows.Forms.MessageBoxDefaultButton.Button1, options);
        }
        private void lstPhotos_KeyDown(object sender, KeyEventArgs e)
        {

            if (lstPhotos.Items.Count > 0 && e.KeyValue == 46) // Delete
            {
                int a = lstPhotos.SelectedIndex;
                lstPhotos.Items.RemoveAt(a);
                if (lstPhotos.Items.Count > 0)
                {
                    if (a >= lstPhotos.Items.Count) a--;
                    lstPhotos.SelectedIndex = a;
                }
            }
            if (e.KeyValue == 45) //Insert
            {
                OpenFileDialog addFileDlg = new OpenFileDialog();
                string s = "";
                for (int i = 0; i < iciCodecs.Length; i++)
                {
                    if (i == 0) s += iciCodecs[i].CodecName + "|" + iciCodecs[i].FilenameExtension;
                    else s += "|" + iciCodecs[i].CodecName + "|" + iciCodecs[i].FilenameExtension;
                }
                addFileDlg.Filter = s;
                s = "|All Suppoted Types|";

                for (int i = 0; i < iciCodecs.Length; i++)
                    s += iciCodecs[i].FilenameExtension + ";";
                
                addFileDlg.Filter += s;
                addFileDlg.FilterIndex = iciCodecs.Length + 1;
                addFileDlg.Multiselect = true;

                if (addFileDlg.ShowDialog() == DialogResult.OK)
                    for(int i = 0; i < addFileDlg.SafeFileNames.Length; i++)
                        lstPhotos.Items.Add(addFileDlg.FileNames[i]);
            }
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox2.Enabled = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            textBox2.Enabled = true;
            textBox1.Enabled = false;
        }
        private void InitAllowedExtension()
        {
         
            foreach (var codek in iciCodecs)
            {
                string[] exts = codek.FilenameExtension.Split(';');
                foreach (string ext in exts)
                {
                    AllowedExt.Add(ext.Replace("*",string.Empty).Trim().ToUpper());
                }
            }
 
        }
        private void lstPhotos_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
 
            foreach (string file in files)
            {
                if(AllowedExt.Contains(Path.GetExtension(file).ToUpper()))  
                {
                     lstPhotos.Items.Add(file);
                }

            }
        }

        private void lstPhotos_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
  
        }

        private void lstPhotos_DragOver(object sender, DragEventArgs e)
        {
           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections;

using System.Threading;

namespace RPQ
{

    class myData
    {

        public string[] path;
        public string pathToSave;
        public Image Ramka;
        public ImageCodecInfo Codek;
        ProgressBar progressBar;
        public int height;
        public int width;
        public double mPix;
        public int jpegQuality;
        public InterpolationMode interpolationMode;
        public void Increment()
        {
            progressBar.Invoke((MethodInvoker)delegate
                {
                    progressBar.Increment(1);
                });
           
        }
        public myData(string [] filenames, string pathToSave_ , ProgressBar progressBar1, int W, 
                    ImageCodecInfo codek , InterpolationMode mode, int jpgQual , Image Ramka_ , double mpix)
        {
            if (W > 0) width = W;
            height = -1;
            Codek = codek;
            this.Ramka = Ramka_;
            pathToSave = pathToSave_;
            jpegQuality = jpgQual;
            interpolationMode = mode;
            progressBar = progressBar1;
            this.mPix = mpix;
            path = new string[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
                path[i] = filenames[i];
            

        }
        public int maxProgress
        {
            get
            {
                return progressBar.Maximum;
            }
        }
    }
}

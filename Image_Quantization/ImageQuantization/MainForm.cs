using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

        }

        RGBPixel[,] ImageMatrix;
       
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void btn_process_Click(object sender, EventArgs e)
        {
            int height = ImageOperations.GetHeight(ImageMatrix);
            int width = ImageOperations.GetWidth(ImageMatrix);
           PROCESSING_DATA.Get_Distinict_Colors(ImageMatrix, height, width);
           PROCESSING_DATA.Make_Fully_Connected_Graph();
           PROCESSING_DATA.Make_Mst_sum();
           if (int.Parse(txt_kclustert.Text) == 0)
           {
              //PROCESSING_DATA.Kclusters = PROCESSING_DATA.find_best_k();
           }
           else
           {
               PROCESSING_DATA.Kclusters = int.Parse(txt_kclustert.Text);
           }

           PROCESSING_DATA.Make_clusters();//chane the number
           PROCESSING_DATA.Make_clusters_data();
           PROCESSING_DATA.convert_photo(ImageMatrix,height, width);  
           ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
      
           txtdistinct.Text = PROCESSING_DATA.count_dis.ToString();
           txtMstSum.Text = Math.Round(PROCESSING_DATA.MST_Sum,2).ToString(); 
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }





       
       
    }
}
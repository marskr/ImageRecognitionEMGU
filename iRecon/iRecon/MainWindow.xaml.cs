using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using iRecon.DrawManager;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System;
using iRecon.LogManager;
using iRecon.DataManager;

namespace iRecon
{
    public partial class MainWindow : Window
    {
        // Custom class to store image scores
        class WeightedImages
        {
            public string s_ImagePath { get; set; } = string.Empty;
            public string s_ImageBasicPath { get; set; } = string.Empty;
            public long l_Score { get; set; } = 0;
        }

        // A List<> which contains processed images scores
        List<WeightedImages> l_imgList = new List<WeightedImages>();
        List<string> l_basicImgPath = new List<string>();

        private void ProcessFolder(string s_mainFolder, string s_basicImage)
        {
            foreach (string file in System.IO.Directory.GetFiles(s_mainFolder))
                ProcessImage(file, s_basicImage);

            foreach (string dir in System.IO.Directory.GetDirectories(s_mainFolder))
                ProcessFolder(dir, s_basicImage);
        }

        private void ProcessImage(string s_testImage, string s_basicImage)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the ProcessImage");
            if (s_testImage == s_basicImage)
            {
                ErrInfLogger.LockInstance.InfoLog("End of the ProcessImage");
                return;
            }

            try
            {
                long l_score;
                long l_matchTime;

                KeyValuePair <long, long> pair = ComputeImages(s_testImage, s_basicImage);
                l_score = pair.Key;
                l_matchTime = pair.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("PROBLEM WITH PROCESSING AN IMAGE!");
                MessageBox.Show("{0}", ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }
            ErrInfLogger.LockInstance.InfoLog("End of the ProcessImage");
        }

        KeyValuePair<long, long> ComputeImages(string s_testImage, string s_basicImage)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the ComputeImages");
            long l_score;
            long l_matchTime;
            using (Mat modelImage = CvInvoke.Imread(s_basicImage, ImreadModes.Color))
            {
                using (Mat observedImage = CvInvoke.Imread(s_testImage, ImreadModes.Color))
                {
                    Mat homography;
                    VectorOfKeyPoint modelKeyPoints;
                    VectorOfKeyPoint observedKeyPoints;

                    using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
                    {
                        Mat mask;
                        DrawMatches.FindMatch(modelImage, observedImage, out l_matchTime, out modelKeyPoints,
                                              out observedKeyPoints, matches, out mask, out homography, out l_score);
                    }

                    l_imgList.Add(new WeightedImages()
                    {
                        s_ImagePath = s_testImage.Remove(0, SettingsContainer.i_testImageOffset),
                        s_ImageBasicPath = s_basicImage.Remove(0, SettingsContainer.i_basicImageOffset),
                        l_Score = l_score
                    });
                }
            }
            ErrInfLogger.LockInstance.InfoLog("End of the ComputeImages");
            return new KeyValuePair<long, long>(l_score, l_matchTime);
        }

        public MainWindow()
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the application");

            InitializeComponent();

            ErrInfLogger.LockInstance.InfoLog("Aplication finished without major errors");
        }

        void btn_DataProcess(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataProcess");
            try
            {
                ProcessFolder(SettingsContainer.Instance.s_Path + @"\reco_settings\img",
                              SettingsContainer.Instance.s_Path + @"\test\test.jpg");
                l_imgList = l_imgList.OrderByDescending(x => x.l_Score).ToList();
                ResultsTable.ItemsSource = l_imgList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("THE PROBLEM OCCURED WITH DATA PROCESSING!");
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }
            ErrInfLogger.LockInstance.InfoLog("End of the DataProcess");
        }

        void btn_DataShow(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataShow");
            try
            {
                if ((ResultsTable.Items.Count > 0) && (ResultsTable.Columns.Count > 0))
                {
                    string s_imgPath = SettingsContainer.Instance.s_Path + @"\test\" + txtPhotoPath.Text + @".jpg";

                    long l_score;
                    long l_matchTime;

                    using (Mat modelImage = CvInvoke.Imread(SettingsContainer.Instance.s_Path + @"\reco_settings\img" + 
                                                            l_imgList[0].s_ImagePath, ImreadModes.Color))
                    using (Mat observedImage = CvInvoke.Imread(s_imgPath, ImreadModes.Color))
                    {
                        Mat result = DrawMatches.Draw(modelImage, observedImage, out l_matchTime, out l_score);
                        emImageViewer iv = new emImageViewer(result, l_score);
                        iv.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DID YOU PROVIDE PROPER PATH TO TEXT BOX?\n" +
                                "IN EXAMPLE FOR 'test.jpg' FILE YOU TYPE IN TEXT BOX JUST 'test'!");
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }
            ErrInfLogger.LockInstance.InfoLog("End of the DataShow");
        }

        void btn_Predict(object sender, RoutedEventArgs e)
        {
            string s_choice = l_imgList[0].s_ImagePath.Remove(0, 1);
            switch (new string(s_choice.Take(3).ToArray()))
            {
                case "dog":
                    MessageBox.Show("My predict is: DOG!!!");
                    break;
                case "cat":
                    MessageBox.Show("My predict is: CAT!!!");
                    break;
                case "rat":
                    MessageBox.Show("My predict is: RAT!!!");
                    break;
                case "hor":
                    MessageBox.Show("My predict is: HORSE!!!");
                    break;
                default:
                    MessageBox.Show("Some kind of an error...");
                    break;
            }
        }
    }
}

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
using System.Threading;
using System.Threading.Tasks;

namespace iRecon
{
    public partial class MainWindow : Window
    {
        private static string s_Path { get { return GetPath(); } }
        private static string GetPath() { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }

        // Custom class to store image scores
        class WeightedImages
        {
            public string ImagePath { get; set; } = "";
            public string ImageBasicPath { get; set; } = "";
            public long Score { get; set; } = 0;
        }

        // A List<> which contains processed images scores
        List<WeightedImages> imgList = new List<WeightedImages>();
        List<string> l_basicImgPath;

        private void ProcessFolder(string mainFolder, string basicImage)
        {
            foreach (var file in System.IO.Directory.GetFiles(mainFolder))
                ProcessImage(file, basicImage);

            foreach (var dir in System.IO.Directory.GetDirectories(mainFolder))
                ProcessFolder(dir, basicImage);
        }

        private void ProcessImage(string testImage, string basicImage)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the ProcessImage");
            if (testImage == basicImage)
            {
                ErrInfLogger.LockInstance.InfoLog("End of the ComputeImages");
                return;
            }

            try
            {
                long score;
                long matchTime;

                var pair = ComputeImages(testImage, basicImage);
                score = pair.Key;
                matchTime = pair.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("PROBLEM WITH PROCESSING AN IMAGE!");
                MessageBox.Show("{0}", ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }
            ErrInfLogger.LockInstance.InfoLog("End of the ProcessImage");
        }

        KeyValuePair<long, long> ComputeImages(string testImage, string basicImage)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the ComputeImages");
            long score;
            long matchTime;
            using (Mat modelImage = CvInvoke.Imread(basicImage, ImreadModes.Color))
            {
                using (Mat observedImage = CvInvoke.Imread(testImage, ImreadModes.Color))
                {
                    Mat homography;
                    VectorOfKeyPoint modelKeyPoints;
                    VectorOfKeyPoint observedKeyPoints;

                    using (var matches = new VectorOfVectorOfDMatch())
                    {
                        Mat mask;
                        DrawMatches.FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                           out mask, out homography, out score);
                    }

                    imgList.Add(new WeightedImages() { ImagePath = testImage, ImageBasicPath = basicImage, Score = score });
                }
            }
            ErrInfLogger.LockInstance.InfoLog("End of the ComputeImages");
            return new KeyValuePair<long, long>(score, matchTime);
        }

        public MainWindow()
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the application");

            InitializeComponent();

            // Source image, small portion of image to be found in bigger pictures
            l_basicImgPath = new List<string>();
            l_basicImgPath.Add(s_Path + @"\reco_settings\img\dog\basic.jpg");
            l_basicImgPath.Add(s_Path + @"\reco_settings\img\cat\basic.jpg");
            l_basicImgPath.Add(s_Path + @"\reco_settings\img\horse\basic.jpg");
            l_basicImgPath.Add(s_Path + @"\reco_settings\img\rat\basic.jpg");

            ErrInfLogger.LockInstance.InfoLog("Aplication finished without major errors");
        }
        void btn_DataProcess(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataProcess");
            try
            {
                foreach (var element in l_basicImgPath)
                {
                    ProcessImage(s_Path + @"\test\test.jpg", element);
                }

                ResultsTable.ItemsSource = imgList.OrderByDescending(x => x.Score).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("THE PROBLEM OCCURED WITH DATA PROCESSING!");
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            ResultsTable.ItemsSource = imgList.OrderByDescending(x => x.Score).ToList();
            ErrInfLogger.LockInstance.InfoLog("End of the DataProcess");
        }
        void btn_DataShow(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataShow");
            try
            {
                if ((ResultsTable.Items.Count > 0) && (ResultsTable.Columns.Count > 0))
                {
                    string imgPath = s_Path + @"\test\" + txtPhotoPath.Text + @".jpg";

                    long score;
                    long matchTime;

                    using (Mat modelImage = CvInvoke.Imread(l_basicImgPath[0], ImreadModes.Color))
                    using (Mat observedImage = CvInvoke.Imread(imgPath, ImreadModes.Color))
                    {
                        var result = DrawMatches.Draw(modelImage, observedImage, out matchTime, out score);
                        var iv = new emImageViewer(result, score);
                        iv.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DID YOU PROVIDE PROPER PATH TO TEXT BOX?\n" +
                                "IN EXAMPLE FOR 'test.jpg' FILE YOU TYPE IN TEXT BOX JUST 'test'.");
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }
            ErrInfLogger.LockInstance.InfoLog("End of the DataShow");
        }
    }
}

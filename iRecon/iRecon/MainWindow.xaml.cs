using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using iRecon.DrawManager;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace iRecon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Source image, small portion of image to be found in bigger pictures
        const string _detailedImage = @"C:\Users\Marcin\Desktop\src\basic.jpg";

        // Custom class to store image scores
        class WeightedImages
        {
            public string ImagePath { get; set; } = "";
            public long Score { get; set; } = 0;
        }

        // A List<> which contains processed images scores
        List<WeightedImages> imgList = new List<WeightedImages>();

        private void ProcessFolder(string mainFolder, string detailImage)
        {
            foreach (var file in System.IO.Directory.GetFiles(mainFolder))
                ProcessImage(file, detailImage);

            foreach (var dir in System.IO.Directory.GetDirectories(mainFolder))
                ProcessFolder(dir, detailImage);
        }

        private void ProcessImage(string completeImage, string detailImage)
        {
            if (completeImage == detailImage) return;

            try
            {
                long score;
                long matchTime;

                using (Mat modelImage = CvInvoke.Imread(detailImage, ImreadModes.Color))
                using (Mat observedImage = CvInvoke.Imread(completeImage, ImreadModes.Color))
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

                    imgList.Add(new WeightedImages() { ImagePath = completeImage, Score = score });
                }
            }
            catch { }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        void btn_DataProcess(object sender, RoutedEventArgs e)
        {
            ProcessFolder(@"C:\Users\Marcin\Desktop\src", _detailedImage);
            ResultsTable.ItemsSource = imgList.OrderByDescending(x => x.Score).ToList();
            ResultsTable.CanUserResizeColumns = false;
            ResultsTable.CanUserResizeRows = false;
        }
        void btn_DataShow(object sender, RoutedEventArgs e)
        {

        }


    }
}

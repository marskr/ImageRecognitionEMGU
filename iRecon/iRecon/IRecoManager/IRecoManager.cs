using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using iRecon.DataManager;
using iRecon.DrawManager;
using iRecon.LogManager;
using System;
using System.Collections.Generic;
using System.Windows;

namespace iRecon.IRecoManager
{
    /// <summary>
    /// The class implemented for image recognition issues. Contains the biggest part of features computation in application.
    /// </summary>
    public class IRecoManager
    {
        /// <summary>
        /// The class generated to store image scores and time of calculation.
        /// </summary>
        public class ImageParameters
        {
            // the path to the test image (test dir)
            public string s_ImagePath { get; set; } = string.Empty;
            // the path to the library basic images (reco_settings dir)
            public string s_ImageBasicPath { get; set; } = string.Empty;
            public long l_Score { get; set; } = 0;
            public double l_MatchTime { get; set; } = 0;
        }

        /// <summary>
        /// List generated to store the data bounded with processed images (among others score obtained by image and time of calculation).
        /// </summary>
        public List<ImageParameters> l_imgList = new List<ImageParameters>();
        /// <summary>
        /// List generated to store data 
        /// </summary>
        public List<string> l_basicImgPath = new List<string>();

        /// <summary>
        /// The method which allows to compute all images in set directory. Uses recurence.
        /// </summary>
        /// <param name="s_mainFolder"> The directory of folder which contains test image. </param>
        /// <param name="s_basicImage"> The directory of folder which contains library basic image. </param>
        public void ProcessFolder(string s_mainFolder, string s_basicImage)
        {
            foreach (string file in System.IO.Directory.GetFiles(s_mainFolder))
                ProcessImage(file, s_basicImage);

            foreach (string dir in System.IO.Directory.GetDirectories(s_mainFolder))
                ProcessFolder(dir, s_basicImage);
        }

        /// <summary>
        /// The method which is responsible for score and match time calculations.
        /// </summary>
        /// <param name="s_testImage"> The test image directory. </param>
        /// <param name="s_basicImage"> The basic library image directory. </param>
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
                double l_matchTime;

                KeyValuePair<long, double> pair = ComputeImage(s_testImage, s_basicImage);
                l_score = pair.Key;
                l_matchTime = pair.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("PROBLEM WITH PROCESSING AN IMAGE!");
                //MessageBox.Show("{0}", ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }

            ErrInfLogger.LockInstance.InfoLog("End of the ProcessImage");
        }

        /// <summary>
        /// The method responsible for image calculations (among others the image similarity score calculations and time of mething calc.
        /// </summary>
        /// <param name="s_testImage"> The test image directory. </param>
        /// <param name="s_basicImage"> The basic library image directory. </param>
        /// <returns> Method returns a tuple, which contains score and matching time fields. </returns>
        KeyValuePair<long, double> ComputeImage(string s_testImage, string s_basicImage)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the ComputeImages");

            long l_score;
            double l_matchTime;
            using (Mat m_modelImage = CvInvoke.Imread(s_basicImage, ImreadModes.Color))
            {
                using (Mat m_observedImage = CvInvoke.Imread(s_testImage, ImreadModes.Color))
                {
                    Mat m_homography;
                    VectorOfKeyPoint v_modelKeyPoints;
                    VectorOfKeyPoint v_observedKeyPoints;

                    using (VectorOfVectorOfDMatch v_matches = new VectorOfVectorOfDMatch())
                    {
                        Mat m_mask;
                        DrawMatches.FindMatch(m_modelImage, m_observedImage, out l_matchTime, out v_modelKeyPoints,
                                              out v_observedKeyPoints, v_matches, out m_mask, out m_homography, out l_score);
                        string s_score = "The score obtained is " + l_score.ToString();
                        string s_matchTime = "The time to obtain score is " + l_matchTime.ToString() + " ms";
                        ErrInfLogger.LockInstance.ScoreLog(s_score);
                        ErrInfLogger.LockInstance.MTimeLog(s_matchTime);
                    }

                    l_imgList.Add(new ImageParameters()
                    {
                        s_ImagePath = s_testImage.Remove(0, SettingsContainer.Instance.i_TestImageOffset),
                        s_ImageBasicPath = s_basicImage.Remove(0, SettingsContainer.Instance.i_BasicImageOffset),
                        l_Score = l_score,
                        l_MatchTime = l_matchTime
                    });
                }
            }

            ErrInfLogger.LockInstance.InfoLog("End of the ComputeImages");
            return new KeyValuePair<long, double>(l_score, l_matchTime);
        }
    }
}

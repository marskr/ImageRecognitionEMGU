using System;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using iRecon.LogManager;
using iRecon.DataManager;

namespace iRecon.DrawManager
{
    /// <summary>
    /// The class implemented for image recognition similarity display issues.
    /// </summary>
    public static class DrawMatches
    {
        /// <summary>
        /// The method used to discover similarities amongst the images, and populating arrays. 
        /// </summary>
        /// <param name="modelImage"> The model image (library basic). </param>
        /// <param name="observedImage"> The observed image (test).  </param>
        /// <param name="matchTime"> The output total time for computing the homography matrix. </param>
        /// <param name="modelKeyPoints"></param>
        /// <param name="observedKeyPoints"></param>
        /// <param name="matches"></param>
        /// <param name="mask"></param>
        /// <param name="homography"></param>
        /// <param name="score"> Field contains the score of matching. </param>
        public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints,
                                     out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask,
                                     out Mat homography, out long score)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the FindMatch");
            
            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            KAZE featureDetector = new KAZE();

            Mat modelDescriptors = new Mat();
            featureDetector.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);

            watch = Stopwatch.StartNew();

            Mat observedDescriptors = new Mat();
            featureDetector.DetectAndCompute(observedImage, null, observedKeyPoints, observedDescriptors, false);

            // KdTree for faster results / less accuracy
            using (var ip = new Emgu.CV.Flann.KdTreeIndexParams())
            using (var sp = new SearchParams())
            using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
            {
                matcher.Add(modelDescriptors);

                matcher.KnnMatch(observedDescriptors, matches, SettingsContainer.Instance.i_K, null);
                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, SettingsContainer.Instance.d_UniquenessThreshold, mask);

                // Calculate score based on matches size
                // ---------------------------------------------->
                score = 0;
                for (int i = 0; i < matches.Size; i++)
                {
                    if (mask.GetData(i)[0] == 0) continue;
                    foreach (var e in matches[i].ToArray())
                        ++score;
                }
                // <----------------------------------------------

                int nonZeroCount = CvInvoke.CountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches,
                                                                               mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints,
                                                                                              matches, mask, 2);
                }
            }
            watch.Stop();
            matchTime = watch.ElapsedMilliseconds;

            ErrInfLogger.LockInstance.InfoLog("End of the FindMatch");
        }

        /// <summary>
        /// Method that draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage"> The model image. </param>
        /// <param name="observedImage"> The observed image. </param>
        /// <param name="matchTime"> The output total time for computing the homography matrix. </param>
        /// <param name="score"> The score of matching. </param>
        /// <returns> Method returns the model image and the observed image, the matched features and homography projection. </returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime, out long score)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the Draw");

            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography, out score);

                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

                #region draw the projected region on the image

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

#if NETFX_CORE
               Point[] points = Extensions.ConvertAll<PointF, Point>(pts, Point.Round);
#else
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
#endif
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }
                }
                #endregion

                ErrInfLogger.LockInstance.InfoLog("End of the Draw");
                return result;
            }
        }
    }
}

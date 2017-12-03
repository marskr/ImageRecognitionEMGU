using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using iRecon.LogManager;
using iRecon.DataManager;
using iRecon.TimeManager;

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
        /// <param name="m_modelImage"> The model image (library basic). </param>
        /// <param name="m_observedImage"> The observed image (test).  </param>
        /// <param name="d_matchTime"> The output total time for computing the homography matrix. </param>
        /// <param name="v_modelKeyPoints"></param>
        /// <param name="v_observedKeyPoints"></param>
        /// <param name="v_matches"></param>
        /// <param name="m_mask"></param>
        /// <param name="m_homography"></param>
        /// <param name="l_score"> Field contains the score of matching. </param>
        public static void FindMatch(Mat m_modelImage, Mat m_observedImage, out double d_matchTime, out VectorOfKeyPoint v_modelKeyPoints,
                                     out VectorOfKeyPoint v_observedKeyPoints, VectorOfVectorOfDMatch v_matches, out Mat m_mask,
                                     out Mat m_homography, out long l_score)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the FindMatch");

            TimerAbstraction _tim = new TimerRefinedAbstraction();
            _tim._iTimer = new TimerFractional();
            
            m_homography = null;

            v_modelKeyPoints = new VectorOfKeyPoint();
            v_observedKeyPoints = new VectorOfKeyPoint();

            KAZE featureDetector = new KAZE();

            Mat modelDescriptors = new Mat();
            featureDetector.DetectAndCompute(m_modelImage, null, v_modelKeyPoints, modelDescriptors, false);
            
            _tim.MeasureStart();

            Mat observedDescriptors = new Mat();
            featureDetector.DetectAndCompute(m_observedImage, null, v_observedKeyPoints, observedDescriptors, false);

            // KdTree for faster results / less accuracy
            using (KdTreeIndexParams ip = new KdTreeIndexParams())
            using (SearchParams sp = new SearchParams())
            using (DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
            {
                matcher.Add(modelDescriptors);

                matcher.KnnMatch(observedDescriptors, v_matches, SettingsContainer.Instance.i_K, null);
                m_mask = new Mat(v_matches.Size, 1, DepthType.Cv8U, 1);
                m_mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(v_matches, SettingsContainer.Instance.d_UniquenessThreshold, m_mask);

                // Calculate score based on matches size
                // ---------------------------------------------->
                l_score = 0;
                for (int i = 0; i < v_matches.Size; i++)
                {
                    if (m_mask.GetData(i)[0] == 0) continue;
                    foreach (var e in v_matches[i].ToArray())
                        ++l_score;
                }
                // <----------------------------------------------

                int nonZeroCount = CvInvoke.CountNonZero(m_mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(v_modelKeyPoints, v_observedKeyPoints, v_matches,
                                                                               m_mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        m_homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(v_modelKeyPoints, v_observedKeyPoints,
                                                                                              v_matches, m_mask, 2);
                }
            }
            _tim.MeasureStop();
            d_matchTime = Math.Round(_tim.MeasureResult().TotalMilliseconds, 2);
            _tim.MeasureRestart();

            ErrInfLogger.LockInstance.InfoLog("End of the FindMatch");
        }

        /// <summary>
        /// Method that draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="m_modelImage"> The model image. </param>
        /// <param name="m_observedImage"> The observed image. </param>
        /// <param name="d_matchTime"> The output total time for computing the homography matrix. </param>
        /// <param name="l_score"> The score of matching. </param>
        /// <returns> Method returns the model image and the observed image, the matched features and homography projection. </returns>
        public static Mat Draw(Mat m_modelImage, Mat m_observedImage, out double d_matchTime, out long l_score)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the Draw");

            Mat m_homography;
            VectorOfKeyPoint v_modelKeyPoints;
            VectorOfKeyPoint v_observedKeyPoints;
            using (VectorOfVectorOfDMatch v_matches = new VectorOfVectorOfDMatch())
            {
                Mat m_mask;
                FindMatch(m_modelImage, m_observedImage, out d_matchTime, out v_modelKeyPoints, out v_observedKeyPoints, v_matches,
                   out m_mask, out m_homography, out l_score);

                //Draw the matched keypoints
                Mat m_result = new Mat();
                Features2DToolbox.DrawMatches(m_modelImage, v_modelKeyPoints, m_observedImage, v_observedKeyPoints,
                   v_matches, m_result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), m_mask);

                #region draw the projected region on the image

                if (m_homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle r_rect = new Rectangle(Point.Empty, m_modelImage.Size);
                    PointF[] p_pts = new PointF[]
                    {
                  new PointF(r_rect.Left, r_rect.Bottom),
                  new PointF(r_rect.Right, r_rect.Bottom),
                  new PointF(r_rect.Right, r_rect.Top),
                  new PointF(r_rect.Left, r_rect.Top)
                    };
                    p_pts = CvInvoke.PerspectiveTransform(p_pts, m_homography);

#if NETFX_CORE
               Point[] points = Extensions.ConvertAll<PointF, Point>(p_pts, Point.Round);
#else
                    Point[] p_points = Array.ConvertAll<PointF, Point>(p_pts, Point.Round);
#endif
                    using (VectorOfPoint v_vp = new VectorOfPoint(p_points))
                    {
                        CvInvoke.Polylines(m_result, v_vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }
                }
                #endregion

                ErrInfLogger.LockInstance.InfoLog("End of the Draw");
                return m_result;
            }
        }
    }
}

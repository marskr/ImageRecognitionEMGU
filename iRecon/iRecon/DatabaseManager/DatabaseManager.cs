using iRecon.LogManager;
using System;
using System.Windows;

namespace iRecon.DatabaseManager
{
    class DatabaseManager
    {
        public void RunMethod()
        {
            //CRUDMask_Results.LockInstance.Read();
            //CRUDMask_Results.LockInstance.i_UpdateID = 6;
            //CRUDMask_Results.LockInstance.Update();

            //CRUDMask_Results.LockInstance.Read();
        }
    }

    public abstract class CRUDOps
    {
        public abstract void Create(string s_imageTested, string s_imageTesting, int i_score, decimal d_matchTime);
    }

    public sealed class ImageRecoResults : CRUDOps
    {
        private static ImageRecoResults Instance = null;
        private static readonly object Lock = new object();

        public override void Create(string s_imageTested, string s_imageTesting, int i_score, decimal d_matchTime)
        {
            try
            {
                using (Measure_ResultsEntities ctx = new Measure_ResultsEntities())
                {
                    ctx.Image_reco.Add(new Image_reco()
                    {
                        Image_tested = s_imageTested,
                        Image_testing = s_imageTesting,
                        Score = i_score,
                        Match_time = d_matchTime
                    });

                    ctx.SaveChanges();
                }
            }
            catch (ApplicationException ex)
            {
                ErrInfLogger.LockInstance.ErrorLog("THE PROBLEM WITH DATABASE INTERACTION [CREATE] POPED OUT!");
                MessageBox.Show("DATABASE PROCESSING EXCEPTION CAUGHT in CreateMessageWithAttachment():\n {0}", ex.ToString());
            }
        }

        public static ImageRecoResults LockInstance
        {
            get
            {
                lock (Lock)
                {
                    if (Instance == null)
                        Instance = new ImageRecoResults();
                    return Instance;
                }
            }
        }
    }
}

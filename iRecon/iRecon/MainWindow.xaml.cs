using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using iRecon.DrawManager;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;
using iRecon.LogManager;
using iRecon.DataManager;
using iRecon.XMLManager;
using iRecon.DatabaseManager;
using static iRecon.IRecoManager.IRecoManager;

namespace iRecon
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the application");

            InitializeComponent();

            txtPhotoPath.Tag = @"whale\basic";
            txtPhotoPath.Text = (string)txtPhotoPath.Tag;

            txtPhotoPath2.Tag = "test";
            txtPhotoPath2.Text = (string)txtPhotoPath2.Tag;

            //SerializationManager<ObjectToUse> SM = new SerializationManager<ObjectToUse>(SingletonStorage.Instance.s_Path);
            //ObjectToUse OTU = new ObjectToUse();
            //SM.SerializeIntoFile();

            ErrInfLogger.LockInstance.InfoLog("Aplication finished without major errors");
        }

        IRecoManager.IRecoManager IRM = new IRecoManager.IRecoManager();

        private void btn_DataProcess(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataProcess");

            try
            {
                IRM.ProcessFolder(SettingsContainer.Instance.s_Path + SettingsContainer.Instance.s_RecoImgFolder,
                                                        SettingsContainer.Instance.s_Path + SettingsContainer.Instance.s_TestImgDir 
                                                        + txtPhotoPath2.Text + ".jpg");
                IRM.l_imgList = IRM.l_imgList.OrderByDescending(x => x.l_Score).ToList();
                ResultsTable.ItemsSource = IRM.l_imgList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("THE PROBLEM OCCURED WITH DATA PROCESSING!");
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }

            ErrInfLogger.LockInstance.InfoLog("End of the DataProcess");
        }

        private void btn_DataShow(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DataShow");

            try
            {
                if ((ResultsTable.Items.Count > 0) && (ResultsTable.Columns.Count > 0))
                {
                    string s_imgPath = SettingsContainer.Instance.s_Path + SettingsContainer.Instance.s_TestImgDir +
                                                             txtPhotoPath2.Text + ".jpg";
                    long l_score;
                    double l_matchTime;

                    using (Mat modelImage = CvInvoke.Imread(SettingsContainer.Instance.s_Path + SettingsContainer.Instance.s_RecoImgFolder +
                                                             txtPhotoPath.Text + @".jpg", ImreadModes.Color))
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

        private void btn_Predict(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the Predict");

            try
            {
                string s_choice = IRM.l_imgList[0].s_ImagePath.Remove(0, 1);
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
                    case "ele":
                        MessageBox.Show("My predict is: ELEPHANT!!!");
                        break;
                    case "par":
                        MessageBox.Show("My predict is: PARROT!!!");
                        break;
                    case "wha":
                        MessageBox.Show("My predict is: WHALE!!!");
                        break;
                    default:
                        MessageBox.Show("Some kind of an error...");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }

            ErrInfLogger.LockInstance.InfoLog("End of the Predict");
        }

        private void btn_SendToDB(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the SendToDB");

            try
            {
                int i_parse1 = 0;
                decimal d_parse2 = 0;
                List<ImageParameters> l_resultsList = new List<ImageParameters>();
                l_resultsList = IRM.l_imgList.OrderByDescending(o => o.l_Score).ToList();

                int.TryParse(l_resultsList[0].l_Score.ToString(), out i_parse1);
                decimal.TryParse(l_resultsList[0].l_MatchTime.ToString(), out d_parse2);

                ImageRecoResults.LockInstance.Create(l_resultsList[0].s_ImagePath,
                                                     l_resultsList[0].s_ImageBasicPath,
                                                     i_parse1,
                                                     d_parse2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }

            ErrInfLogger.LockInstance.InfoLog("End of the SendToDB");
        }

        private void btn_Clear(object sender, RoutedEventArgs e)
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the Clear");

            try
            {
                IRM.l_imgList.Clear();
                ResultsTable.ItemsSource = IRM.l_imgList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ErrInfLogger.LockInstance.ErrorLog(ex.ToString());
            }

            ErrInfLogger.LockInstance.InfoLog("End of the Clear");
        }
    }
}

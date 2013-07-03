using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace YoutubeExport
{

    public partial class Mainfrm : Form
    {
        public Mainfrm()
        {
            InitializeComponent();
        }

        private string GetBlockContent(ref string strSearch, string strBlockStart, string strBLockEnd)
        {
            int iBlockStart = 0;
            int iBlockEnd = 0;
            string strResult="";
                        
            try
            {
                iBlockStart = strSearch.IndexOf(strBlockStart);

                if (iBlockStart != -1)
                {
                    iBlockEnd = strSearch.IndexOf(strBLockEnd, iBlockStart + strBlockStart.Length);

                    strResult = strSearch.Substring(iBlockStart + strBlockStart.Length, iBlockEnd - (strBlockStart.Length + iBlockStart));
                    strSearch = strSearch.Substring(iBlockEnd + strBLockEnd.Length, strSearch.Length - (iBlockEnd + strBLockEnd.Length));
                }
                else
                {
                    strSearch = "";
                }

            }
            catch //(Exception e)
            {
                strResult = "";
                strSearch = "";
            }

            return strResult;

        }

        private void Mainfrm_Load(object sender, EventArgs e)
        {
            txtUrl.Text = "http://www.youtube.com/playlist?list=FLGYr00JNLYnkz3agksMszxA";
        }

        private delegate void SetCtrlPropDelegate(Control control, string propertyName, object propertyValue);

        public static void SetCtrlProp(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetCtrlPropDelegate(SetCtrlProp), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }

        private delegate void SetCtrlPropDelegateCollection(Control control, object item ,string propertyName, object propertyValue);

        public static void SetCtrlPropCollection(Control control,object item, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetCtrlPropDelegateCollection(SetCtrlPropCollection), new object[] { control, item, propertyName, propertyValue });
            }
            else
            {
               item.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, item , new object[] { propertyValue });
            }
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            cmdRefresh.Enabled = false;
             
            Thread t = new Thread(() => GetData(txtUrl.Text));
            t.IsBackground = true;
            t.Start();
        }

        private void GetData(string strMainURL)
        {
            string strSearchURL = "";

            string strBlock = "";
            string strExtrasBlock = "";
            string strVideoUrl = "";
            string strTitle = "";
            string strOwner = "";
            string strViews = "";

            string strResults = "";
            string strLine = "";
            string strdummy = "";

            bool blnResultOk;
            string result;
            int iCounter = 0;
            int iLineCounter = 0;

            if (strMainURL != "")
            {
                do
                {
                    iCounter += 1;

                    strSearchURL = strMainURL + "&page=" + iCounter.ToString();

                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strSearchURL);
                    myRequest.UserAgent = "Mozilla/6.0 (Windows NT 6.2; WOW64; rv:16.0.1) Gecko/20121011 Firefox/16.0.1";
                    myRequest.Method = "GET";
                    WebResponse myResponse = myRequest.GetResponse();
                    StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                    result = sr.ReadToEnd();
                    sr.Close();
                    myResponse.Close();

                    blnResultOk = result.Contains("<h3 class=\"video-title-container\">");

                    do
                    {
                        strBlock = GetBlockContent(ref result, "<h3 class=\"video-title-container\">", "</div>");

                        strExtrasBlock = strBlock;

                        if (strExtrasBlock != "1")
                        {
                            strVideoUrl = "www.youtube.com/watch?v=" + GetBlockContent(ref strExtrasBlock, "watch?v=", "&amp");
                            strTitle = GetBlockContent(ref strExtrasBlock, "dir=\"ltr\">", "</");
                            strOwner = GetBlockContent(ref strExtrasBlock, "dir=\"ltr\">", "</");
                            strViews = GetBlockContent(ref strExtrasBlock, "\"video-view-count\">", "</");
                            strViews = strViews.Trim();
                            strdummy = strViews;
                            strViews = GetBlockContent(ref strdummy, "", " ");

                            strLine = iLineCounter.ToString() + "\t" + strVideoUrl + "\t" + strOwner + "\t" + strViews + "\t" + strTitle;
                            strResults += strLine + "\r\n";

                            iLineCounter += 1;
                            SetCtrlPropCollection(sstrip, sstrip.Items[0], "Text", "Reading Page " + iCounter.ToString() + " (" + iLineCounter.ToString() + " Results.)");
                        }

                    } while (strBlock != "");

                } while (blnResultOk);

                SetCtrlProp(textBox1, "Text", strResults);
            }

            SetCtrlProp(cmdRefresh, "Enabled", true);

        }

    }
}


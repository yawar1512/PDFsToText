using java.awt;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using java.util;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace PDFsToText
{
    public partial class Form1 : Form
    {
        List<FileProcess> listProcess = null;
        XmlNodeList nodes = null;
        XmlNode rect = null;
        int rectW = 122;
        int rectH = 57;
        int borderleft = 50;
        int borderTop = 100;
        int bordertable = 0;
        int irowCount = 12;
        int icolCount = 3;
        RadioButton m_radiogroup1Checked = null;
        private delegate string MethodDelegate(string pdffile, string textfile);
        public Form1()
        {
            InitializeComponent();
            CreateRadioButtons();
            LoadNode();
        }

        private void CreateRadioButtons()
        {
            string configfile = (Application.StartupPath + "\\config.xml").Replace("\\bin\\Debug", "");
            XmlDocument doc = new XmlDocument();
            doc.Load(configfile);
            XmlNodeList list = doc.GetElementsByTagName("rectangle");

            for (int i = 0; i < list.Count; i++)
            {
                RadioButton rdo = new RadioButton();

                rdo.Name = "radioButton" + list[i].Attributes["id"].Value;
                rdo.Tag = list[i].Attributes["id"].Value;
                rdo.Text = list[i].Attributes["Name"].Value;
                if (i == 0)
                {
                    rdo.Checked = true;
                    m_radiogroup1Checked = rdo;
                }
                flowLayoutPanel1.Controls.Add(rdo);
                rdo.Click += radioGroup1_Clicked;
            }
        }

        private void radioGroup1_Clicked(object sender, EventArgs e)
        {
            m_radiogroup1Checked = sender as RadioButton;
            LoadNode();
        }

        private void LoadNode()
        {
            string configfile = (Application.StartupPath + "\\config.xml").Replace("\\bin\\Debug", "");
            if (!File.Exists(configfile))
            {
                MessageBox.Show("Config File Does not Exists");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(configfile);
            XmlNode parentNode = doc.SelectSingleNode("/text/extraction/rectangle[@id='" + m_radiogroup1Checked.Tag + "']");
            nodes = parentNode.SelectNodes("node");
            if (parentNode != null)
            {
                rectW = parentNode.Attributes["width"] != null ? (parentNode.Attributes["width"].Value != null ? Convert.ToInt32(parentNode.Attributes["width"].Value) : rectW) : rectW;
                rectH = parentNode.Attributes["height"] != null ? parentNode.Attributes["height"].Value != null ? Convert.ToInt32(parentNode.Attributes["height"].Value) : rectH : rectH;
                borderleft = parentNode.Attributes["borderleft"] != null ? parentNode.Attributes["borderleft"].Value != null ? Convert.ToInt32(parentNode.Attributes["borderleft"].Value) : borderleft : borderleft;
                borderTop = parentNode.Attributes["borderTop"] != null ? parentNode.Attributes["borderTop"].Value != null ? Convert.ToInt32(parentNode.Attributes["borderTop"].Value) : borderTop : borderTop;
                bordertable = parentNode.Attributes["bordertable"] != null ? parentNode.Attributes["bordertable"].Value != null ? Convert.ToInt32(parentNode.Attributes["bordertable"].Value) : bordertable : bordertable;
                irowCount = parentNode.Attributes["irowCount"] != null ? parentNode.Attributes["irowCount"].Value != null ? Convert.ToInt32(parentNode.Attributes["irowCount"].Value) : irowCount : irowCount;
                icolCount = parentNode.Attributes["icolCount"] != null ? parentNode.Attributes["icolCount"].Value != null ? Convert.ToInt32(parentNode.Attributes["icolCount"].Value) : icolCount : icolCount;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.txtPDFFiles.Text = (Application.StartupPath + "\\PDF File").Replace("\\bin\\Debug", "");
            this.txtTextFiles.Text = (Application.StartupPath + "\\Text Files").Replace("\\bin\\Debug", "");
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(this.txtPDFFiles.Text);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(this.txtTextFiles.Text);
            listProcess = new List<FileProcess>();
            int num = 0;
            if (directoryInfo1.Exists)
            {
                FileInfo[] files = directoryInfo1.GetFiles("*.pdf");
                num = ((IEnumerable<FileInfo>)files).Count<FileInfo>();
                MethodDelegate methodDelegate = new MethodDelegate(this.LongRunningMethod);
                AsyncCallback callback = new AsyncCallback(this.MyAsyncCallback);
                string fullName = "";
                string str = "";
                string textfile = "";
                int iCount = 5;
                int iLoop = 1;
                int divided = num / iLoop;
                FileInfo fileInfo = null;
                DirectoryInfo dirTextWorking = new DirectoryInfo(this.txtTextFiles.Text + "\\Working");
                if (!dirTextWorking.Exists)
                {
                    dirTextWorking.Create();
                }
                for (int i = 0; i < divided; i++)
                {
                    for (int j = 1; j <= iLoop; j++)
                    {
                        fileInfo = files[i * j];
                        fullName = fileInfo.FullName;
                        str = fileInfo.Name.Replace("pdf", "txt");
                        textfile = dirTextWorking.ToString() + "\\" + str;
                        listProcess.Add(new FileProcess { Completed = false, FileName = fileInfo.Name.Replace("pdf", "txt") });
                        methodDelegate.BeginInvoke(fullName, textfile, callback, (object)methodDelegate);
                    }
                    while (listProcess.Any(s => s.Completed == false))
                    {
                        Thread.Sleep(5000);
                    }
                    listProcess.Clear();
                    if (((i + 1) % iCount == 0) || ((i + 1) == divided))
                    {


                        if (textBox1.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate ()
                            {
                                textBox1.AppendText("Completed " + (i * iLoop) + "files");
                            });
                        }

                        foreach (FileInfo mFile in dirTextWorking.GetFiles())
                        {

                            if (new FileInfo(dirTextWorking + "\\" + mFile.Name).Exists == true)
                            {
                                if (new FileInfo(directoryInfo2 + "\\" + mFile.Name).Exists == true)
                                {
                                    File.Delete(directoryInfo2 + "\\" + mFile.Name);
                                }
                                mFile.MoveTo(directoryInfo2 + "\\" + mFile.Name);
                            }
                        }
                    }
                }

            }
            this.stsStrip.Text = "";
        }

        private void downloadPdfFromHtmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form2().Show();
            this.Hide();
        }

        private void openTxtToExcelFileConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() != DialogResult.OK)
                return;
            this.txtPDFFiles.Text = this.fbd.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() != DialogResult.OK)
                return;
            this.txtTextFiles.Text = this.fbd.SelectedPath;
        }

        private string LongRunningMethod(string pdffile, string textfile)
        {
            FileInfo fileInfo = new FileInfo(textfile);
            if (chkUnicode.Checked)
            {
                using (StreamWriter sw = new StreamWriter(textfile))
                {
                    string text = "";
                    switch (m_radiogroup1Checked.Tag)
                    {
                        default:
                            text = parseUsingPDFBox(pdffile);
                            break;
                    }
                    sw.WriteLine(text);
                }
            }
            else
            {
                new PDFParser().ExtractText(pdffile, textfile);
            }

            return "Converted " + fileInfo.Name + " to Text File \r\n";
        }

        public void MyAsyncCallback(IAsyncResult result)
        {
            string s = "";
            try
            {
                s = ((MethodDelegate)result.AsyncState).EndInvoke(result);
            }
            catch
            {

            }
            string strfileName = s.Replace("Converted ", "");
            strfileName = strfileName.Replace(" to Text File \r\n", "");
            List<FileProcess> process = listProcess.Where(j => j.FileName == strfileName).ToList();
            process[0].Completed = true;
            if (textBox1.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    textBox1.AppendText(s);

                });
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }


        private string parseUsingPDFBox(string input)
        {
            PDDocument doc = null;

            try
            {
                doc = PDDocument.load(input);
                java.util.List pages = doc.getDocumentCatalog().getAllPages();
                int iPageCount = pages.size();
                StringBuilder strText = new StringBuilder();
                PDFTextStripperByArea stripper = new PDFTextStripperByArea();
                stripper.setSuppressDuplicateOverlappingText(false);
                stripper.setSortByPosition(true);
                java.awt.geom.Rectangle2D region1 = null;
                java.awt.geom.Rectangle2D region2 = null;
                java.awt.geom.Rectangle2D region3 = null;
                java.awt.geom.Rectangle2D region4 = null;
                java.awt.geom.Rectangle2D region5 = null;
                java.awt.geom.Rectangle2D region6 = null;
                for (int iPage = 0; iPage < iPageCount; iPage++)
                {
                    for (int i = 1; i <= irowCount; i++)
                    {

                        region1 = null; region2 = null; region3 = null; region4 = null; region5 = null; region6 = null;
                        if (icolCount >= 1)
                        {

                            region1 = new java.awt.geom.Rectangle2D.Double(borderleft, borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region1", region1);
                        }

                        if (icolCount >= 2)
                        {
                            region2 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 1), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region2", region2);
                        }

                        if (icolCount >= 3)
                        {
                            region3 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 2), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region3", region3);
                        }

                        if (icolCount >= 4)
                        {
                            region4 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 3), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region4", region4);
                        }
                        if (icolCount >= 5)
                        {
                            region5 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 4), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region5", region5);
                        }

                        if (icolCount >= 6)
                        {
                            region6 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 5), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                            stripper.addRegion("region6", region6);
                        }
                        PDPage page = (PDPage)pages.get(iPage);
                        stripper.extractRegions(page);
                        string strregion1 = region1 != null ? ReplaceText(stripper.getTextForRegion("region1")) : "";
                        string strregion2 = region2 != null ? ReplaceText(stripper.getTextForRegion("region2")) : "";
                        string strregion3 = region3 != null ? ReplaceText(stripper.getTextForRegion("region3")) : "";
                        string strregion4 = region4 != null ? ReplaceText(stripper.getTextForRegion("region4")) : "";
                        string strregion5 = region5 != null ? ReplaceText(stripper.getTextForRegion("region5")) : "";
                        string strregion6 = region6 != null ? ReplaceText(stripper.getTextForRegion("region6")) : "";
                        if (!string.IsNullOrEmpty(strregion1))
                            strText.AppendLine(strregion1);
                        if (!string.IsNullOrEmpty(strregion2))
                            strText.AppendLine(strregion2);
                        if (!string.IsNullOrEmpty(strregion3))
                            strText.AppendLine(strregion3);
                        if (!string.IsNullOrEmpty(strregion4))
                            strText.AppendLine(strregion4);
                        if (!string.IsNullOrEmpty(strregion5))
                            strText.AppendLine(strregion5);
                        if (!string.IsNullOrEmpty(strregion6))
                            strText.AppendLine(strregion6);

                    }
                }

                return strText.ToString();
                //java.awt.geom.Rectangle2D region1 = new java.awt.geom.Rectangle2D.Double(175, 0, 175, 175);
                //string regionName = "region";



            }
            finally
            {
                if (doc != null)
                {
                    doc.close();
                }
            }
        }

        private string ReplaceText(string currentText)
        {
            if (string.IsNullOrEmpty(currentText))
                return currentText;

            foreach (XmlNode item in nodes)
            {
                string strvalue = item.Attributes["value"].Value;
                string strreplace = item.Attributes["replace"].Value;
                bool newlinebefore = Convert.ToBoolean(item.Attributes["newlinebefore"].Value);
                bool newlineafter = Convert.ToBoolean(item.Attributes["newlineafter"].Value);
                string tobereplace = newlinebefore ? Environment.NewLine : "";
                tobereplace += strreplace;
                tobereplace += newlineafter ? Environment.NewLine : "";

                currentText = currentText.Replace(strvalue, tobereplace);
            }
            //currentText = currentText.Replace("પિતન ંુ નામ", "~Husband");
            //currentText = currentText.Replace("પિતન ંુ નામ", "~Husband");
            //currentText = currentText.Replace("િપતાન ંુ નામ", "~Father");
            //currentText = currentText.Replace("ઘર નબંર", "~HNO");
            //currentText = currentText.Replace("ઉંમર", "~Age");
            //currentText = currentText.Replace("Әમર", "~Age");
            //currentText = currentText.Replace("જાિત : Ęી", Environment.NewLine +"~Gender:F");
            //currentText = currentText.Replace("Ĥિત : Ęી", Environment.NewLine +"~Gender:F");
            //currentText = currentText.Replace("જાિત : પĮુષ", Environment.NewLine + "~Gender:M");
            //currentText = currentText.Replace("Ĥિત : પĮુષ", Environment.NewLine + "~Gender:F");
            //currentText = currentText.Replace("Ĥિત : પĮુષ", Environment.NewLine + "~Gender:F");
            //currentText = currentText.Replace("નામ", "~VName");


            Regex reg = new Regex("\r\n");
            string[] splits = reg.Split(currentText, 2);
            if (splits.Count() == 2)
            {
                splits[0] = "!" + splits[0];
                splits[0] = splits[0].Trim().Replace(" ", " ~");
                return String.Join(Environment.NewLine, splits);
            }
            else
            {
                return currentText;
            }


        }



        private string parseUsingPDFBox1(string input)
        {
            PDDocument doc = null;

            try
            {
                doc = PDDocument.load(input);
                java.util.List pages = doc.getDocumentCatalog().getAllPages();
                int iPageCount = pages.size();
                StringBuilder strText = new StringBuilder();
                PDFTextStripperByArea stripper = new PDFTextStripperByArea();
                stripper.setSuppressDuplicateOverlappingText(false);
                stripper.setSortByPosition(true);
                int rectW = 122;
                int rectH = 1000;
                int borderleft = 50;
                int borderTop = 100;
                int bordertable = 0;
                java.awt.geom.Rectangle2D region1 = null;
                //java.awt.geom.Rectangle2D region2 = null;
                //java.awt.geom.Rectangle2D region3 = null;
                //java.awt.geom.Rectangle2D region4 = null;
                //for (int iPage = 0; iPage < iPageCount; iPage++)
                //{
                //    for (int i = 1; i <= 12; i++)
                //    {
                region1 = null;// region2 = null; region3 = null; region4 = null;
                region1 = new java.awt.geom.Rectangle2D.Double(borderleft, borderTop + ((rectH + bordertable) * (1 - 1)), rectW, rectH);
                //region2 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 1), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                //region3 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 2), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                //region4 = new java.awt.geom.Rectangle2D.Double((borderleft + rectW * 3), borderTop + ((rectH + bordertable) * (i - 1)), rectW, rectH);
                stripper.addRegion("region1", region1);
                //stripper.addRegion("region2", region2);
                //stripper.addRegion("region3", region3);
                //stripper.addRegion("region4", region4);

                PDPage page = (PDPage)pages.get(2);

                stripper.extractRegions(page);
                string strregion1 = ReplaceText(stripper.getTextForRegion("region1"));
                //string strregion2 = ReplaceText(stripper.getTextForRegion("region2"));
                //string strregion3 = ReplaceText(stripper.getTextForRegion("region3"));
                //string strregion4 = ReplaceText(stripper.getTextForRegion("region4"));
                if (!string.IsNullOrEmpty(strregion1))
                    strText.AppendLine(strregion1);
                //if (!string.IsNullOrEmpty(strregion2))
                //    strText.AppendLine(strregion2);
                //if (!string.IsNullOrEmpty(strregion3))
                //    strText.AppendLine(strregion3);
                //if (!string.IsNullOrEmpty(strregion4))
                //    strText.AppendLine(strregion4);
                //}
                //}

                return strText.ToString();
                //java.awt.geom.Rectangle2D region1 = new java.awt.geom.Rectangle2D.Double(175, 0, 175, 175);
                //string regionName = "region";



            }
            finally
            {
                if (doc != null)
                {
                    doc.close();
                }
            }
        }
    }
    public class MyPDFTextStripper : PDFTextStripper
    {


        public override void setArticleEnd(string articleEndValue)
        {
            base.setArticleEnd(articleEndValue);
        }
        protected override void processTextPosition(TextPosition text)
        {
            base.processTextPosition(text);

        }
        //protected override void writeString(String text, List textPositions)
        //{

        //}

        // Another option is to hide the original method by using the keyword new
        protected override void writeString(string text, java.util.List textPositions)
        {

            base.writeString(text, textPositions);
        }
    }


    public class FileProcess
    {
        public string FileName { get; set; }

        public bool Completed { get; set; }

        public string error { get; set; }
    }
}



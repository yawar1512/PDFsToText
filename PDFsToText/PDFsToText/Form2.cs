using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFsToText
{
    public partial class Form2 : Form
    {
        private delegate string MethodDelegate(string strhref, string dest);
        private static int i;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            foreach (HtmlElement htmlElement in this.webBrowser1.Document.GetElementsByTagName("a"))
            {
                string dest = this.txtPdfFile.Text + "\\" + htmlElement.InnerHtml + ".pdf";
                string attribute = htmlElement.GetAttribute("href");
                Form2.MethodDelegate methodDelegate = new Form2.MethodDelegate(this.LongRunningMethod);
                AsyncCallback callback = new AsyncCallback(this.MyAsyncCallback);
                methodDelegate.BeginInvoke(attribute, dest, callback, (object)methodDelegate);
            }
        }

        private string LongRunningMethod(string strhref, string dest)
        {
            string fileName = Path.GetFileName(dest);
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(strhref, dest);
                }
                catch
                {
                    return "\r\n Error Occures " + dest + "\r\n";
                }
            }
            return fileName + "\t";
        }

        public void MyAsyncCallback(IAsyncResult ar)
        {
            ++i;
            string s = ((MethodDelegate)ar.AsyncState).EndInvoke(ar);
            if (!this.textBox1.InvokeRequired)
                return;
            this.textBox1.Invoke((MethodInvoker)(() => this.textBox1.AppendText(s)));
        }

        private void downloadPdfFromHtmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form1().Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtHtmlFile.Text) || string.IsNullOrEmpty(this.txtPdfFile.Text))
            {
                int num = (int)MessageBox.Show("Please Select Html File / Pdf File Folder");
            }
            else
            {
                Application.StartupPath.Replace("\\bin\\Debug", "");
                this.webBrowser1.Url = new Uri(this.txtHtmlFile.Text);
            }

        }

        private void openTxtToExcelFileConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form3().Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ofd1.ShowDialog() != DialogResult.OK)
                return;
            txtHtmlFile.Text = this.ofd1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fdb1.ShowDialog() != DialogResult.OK)
                return;
            this.txtPdfFile.Text = this.fdb1.SelectedPath;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }
    }
}

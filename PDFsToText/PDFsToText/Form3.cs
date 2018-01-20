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

namespace PDFsToText
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            string path1 = Application.StartupPath.Replace("\\bin\\Debug", "\\Text Files");
            string path2 = Application.StartupPath.Replace("\\bin\\Debug", "\\Excel Files");
            DirectoryInfo directoryInfo1 = new DirectoryInfo(path1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(path2);
            int num1 = 0;
            if (directoryInfo1.Exists)
            {
                FileInfo[] files = directoryInfo1.GetFiles("*.txt");
                num1 = ((IEnumerable<FileInfo>)files).Count<FileInfo>();
                foreach (FileInfo fileInfo in files)
                {
                    if (fileInfo.Exists)
                    {
                        string fullName = fileInfo.FullName;
                        string FileName = fileInfo.Name.Replace("txt", "xls");
                        string str = directoryInfo2.ToString() + "\\" + FileName;
                        Bytescout.Spreadsheet.Spreadsheet spreadsheet = new Bytescout.Spreadsheet.Spreadsheet();
                        spreadsheet.LoadFromFile(fileInfo.FullName);
                        spreadsheet.SaveAsXLS(FileName);
                    }
                }
            }
            int num2 = (int)MessageBox.Show("All the Pdf files Converted");
        }
    }
}

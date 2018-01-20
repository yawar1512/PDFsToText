using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace PDFsToText
{
    public class PDFParser
    {
        private static int _numberOfCharsToKeep = 15;
        XmlNode attributes = null;
        public bool ExtractText(string inFileName, string outFileName)
        {
            StreamWriter streamWriter = (StreamWriter)null;
            try
            {
                PdfReader pdfReader = new PdfReader(inFileName);
                streamWriter = new StreamWriter(outFileName, false, Encoding.UTF8);
                Console.Write("Processing: ");
                int num1 = 68;
                float num2 = (float)num1 / (float)pdfReader.NumberOfPages;
                int num3 = 0;
                float num4 = 0.0f;
                for (int pageNum = 1; pageNum <= pdfReader.NumberOfPages; ++pageNum)
                {
                    streamWriter.Write(this.ExtractTextFromPDFBytes(pdfReader.GetPageContent(pageNum)) + " ");
                    if ((double)num2 >= 1.0)
                    {
                        for (int index = 0; index < (int)num2; ++index)
                        {
                            Console.Write("#");
                            ++num3;
                        }
                    }
                    else
                    {
                        num4 += num2;
                        if ((double)num4 >= 1.0)
                        {
                            for (int index = 0; index < (int)num4; ++index)
                            {
                                Console.Write("#");
                                ++num3;
                            }
                            num4 = 0.0f;
                        }
                    }
                }
                if (num3 < num1)
                {
                    for (int index = 0; index < num1 - num3; ++index)
                        Console.Write("#");
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (streamWriter != null)
                    streamWriter.Close();
            }
        }

        private string ExtractTextFromPDFBytes(byte[] input)
        {
            if (input == null || input.Length == 0)
                return "";
            try
            {
                string str = "";
                bool flag1 = false;
                bool flag2 = false;
                int num1 = 0;
                char[] recent = new char[PDFParser._numberOfCharsToKeep];
                for (int index = 0; index < PDFParser._numberOfCharsToKeep; ++index)
                    recent[index] = ' ';
                for (int index1 = 0; index1 < input.Length; ++index1)
                {
                    char ch = (char)input[index1];
                    if (flag1)
                    {
                        if (num1 == 0)
                        {
                            if (this.CheckToken(new string[2]
                            {
                "TD",
                "Td"
                            }, recent))
                                str += "\n\r";
                            else if (this.CheckToken(new string[3]
                            {
                "'",
                "T*",
                "\""
                            }, recent))
                                str += "\n";
                            else if (this.CheckToken(new string[1] { "Tj" }, recent))
                                str += " ";
                        }
                        int num2;
                        if (num1 == 0)
                            num2 = !this.CheckToken(new string[1] { "ET" }, recent) ? 1 : 0;
                        else
                            num2 = 1;
                        if (num2 == 0)
                        {
                            flag1 = false;
                            str += " ";
                        }
                        else if ((int)ch == 40 && num1 == 0 && !flag2)
                            num1 = 1;
                        else if ((int)ch == 41 && num1 == 1 && !flag2)
                            num1 = 0;
                        else if (num1 == 1)
                        {
                            if ((int)ch == 92 && !flag2)
                            {
                                flag2 = true;
                            }
                            else
                            {
                                if ((int)ch >= 32 && (int)ch <= 126 || (int)ch >= 128 && (int)ch < (int)byte.MaxValue)
                                    str += ch.ToString();
                                flag2 = false;
                            }
                        }
                    }
                    for (int index2 = 0; index2 < PDFParser._numberOfCharsToKeep - 1; ++index2)
                        recent[index2] = recent[index2 + 1];
                    recent[PDFParser._numberOfCharsToKeep - 1] = ch;
                    int num3;
                    if (!flag1)
                        num3 = !this.CheckToken(new string[1] { "BT" }, recent) ? 1 : 0;
                    else
                        num3 = 1;
                    if (num3 == 0)
                        flag1 = true;
                }
                return ReplaceText(str);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private XmlNodeList LoadNode()
        {
            string configfile = (Application.StartupPath + "\\config.xml").Replace("\\bin\\Debug", "");
            XmlDocument doc = new XmlDocument();
            doc.Load(configfile);
            attributes = doc.GetElementsByTagName("rectangle").Count > 0 ? doc.GetElementsByTagName("rectangle")[0] : null;

            return doc.GetElementsByTagName("node");

        }
        private string ReplaceText(string currentText)
        {
            if (string.IsNullOrEmpty(currentText))
                return currentText;
            XmlNodeList nodes = LoadNode();
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
        private bool CheckToken(string[] tokens, char[] recent)
        {
            foreach (string token in tokens)
            {
                if ((int)recent[PDFParser._numberOfCharsToKeep - 3] == (int)token[0] && (int)recent[PDFParser._numberOfCharsToKeep - 2] == (int)token[1] && ((int)recent[PDFParser._numberOfCharsToKeep - 1] == 32 || (int)recent[PDFParser._numberOfCharsToKeep - 1] == 13 || (int)recent[PDFParser._numberOfCharsToKeep - 1] == 10) && ((int)recent[PDFParser._numberOfCharsToKeep - 4] == 32 || (int)recent[PDFParser._numberOfCharsToKeep - 4] == 13 || (int)recent[PDFParser._numberOfCharsToKeep - 4] == 10))
                    return true;
            }
            return false;
        }
    }
}

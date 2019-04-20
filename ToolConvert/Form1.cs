using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolConvert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void scanAll(string path)
        {
            if(!File.Exists(path))
            {
                MessageBox.Show("File is not exist!");
                return;
            }

            string preambleString = "static GUI_CONST_STORAGE unsigned long";

            string text = File.ReadAllText(path);

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(textBoxOut.Text))
            {
                int dx = text.IndexOf(preambleString);

                if (dx == -1)
                {
                    return;
                }

                file.Write(text.Substring(0, dx));

                int startIdx = 0;

                while (true)
                {
                    int tempIdx = text.Substring(startIdx).IndexOf(preambleString);

                    ManualResetEvent syncEvent = new ManualResetEvent(false);

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = (int)(100 * (double)startIdx / text.Length);
                            syncEvent.Set();
                        });
                    }
                    else
                    {
                        progressBar1.Value = (int)(100 * startIdx / text.Length);
                        syncEvent.Set();
                    }

                    syncEvent.WaitOne();

                    if (tempIdx == -1)
                    {
                        if (startIdx > 0)
                            file.Write(text.Substring(startIdx + 1, text.Length - startIdx - 1));
                        break;
                    }
                    else
                    {
                        startIdx += tempIdx;
                    }

                    int firstBr = text.Substring(startIdx).IndexOf('{') + startIdx;
                    if (firstBr == -1 + startIdx)
                    {
                        MessageBox.Show("Error at : " + text.Substring(startIdx, 20));
                        return;
                    }

                    int secondBr = text.Substring(firstBr).IndexOf('}') + firstBr;

                    if (secondBr == -1 + firstBr)
                    {
                        MessageBox.Show("Error at : " + text.Substring(firstBr, 20));
                        return;
                    }

                    string dataContent = text.Substring(firstBr + 1, secondBr - firstBr - 1);

                    //write data
                    file.Write(text.Substring(startIdx, firstBr - startIdx + 1) + "\r\n");

                    int temp = text.Substring(startIdx).IndexOf("unsigned long");

                    if(temp >= 0)
                    {
                        string name = text.Substring(startIdx + temp + 15);

                        name = name.Substring(0, name.IndexOf('['));

                        file.Write(EncodeImgData(dataContent, name) + "\r\n};\r\n\r\n");

                        startIdx = secondBr + 1;
                    }
                    else
                    {
                        //file.Write(text.Substring(startIdx, text.Length - startIdx));
                        MessageBox.Show("Error data string format!");
                        break;
                    }
                    
                }
            }
            

        }

        private string EncodeImgData(string content, string name)
        {
            string findingText = string.Empty;
            int findingPos = 0;
            int findingNum = 0;
            int idx = 0;

            List<pivotStringFinder> pSF = new List<pivotStringFinder>();

            List<string> destString = new List<string>();

            string InputText = content;

            string[] elements = InputText.Replace("\r\n", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in elements)
            {

                if (s.Trim() != findingText || idx == elements.Length - 1)
                {
                    if (idx == elements.Length - 1)
                    {
                        findingNum++;
                    }

                    if (findingText == string.Empty)
                    {
                        findingText = s.Trim();
                        findingPos = idx;
                        findingNum = 1;
                    }
                    else
                    {
                        if (findingNum > 2)
                        {
                            pivotStringFinder tempPSF = new pivotStringFinder();

                            tempPSF.finderString = findingText.Trim();
                            tempPSF.num = findingNum;
                            tempPSF.pos = findingPos;

                            pSF.Add(tempPSF);

                            destString.Add(findingText);
                        }
                        else
                        {
                            //
                            for (int i = 0; i < findingNum; i++)
                            {
                                destString.Add(findingText);
                            }
                        }
                        findingText = s.Trim();
                        findingPos = idx;
                        findingNum = 1;
                    }
                }
                else
                {
                    findingNum++;
                }

                idx++;
            }

            //Build header
            uint temp = (uint)elements.Length;
            UInt32 firstH = ((temp) << 16) | (uint)(pSF.Count * 4 + 4);
            //Build all data array
            List<string> retStr = new List<string>();

            retStr.Add("0x" + firstH.ToString("X2"));

            foreach (pivotStringFinder p in pSF)
            {
                UInt32 btemp = ((uint)(p.pos) << 16) | (uint)(p.num);
                retStr.Add("0x" + btemp.ToString("X2"));
            }

            retStr.AddRange(destString);


            //Print to file from retStr
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(name))
            {
                int rettrIdx = 1;
                foreach (string line in retStr)
                {
                    // 
                    if (rettrIdx % 10 == 0)
                    {
                        file.WriteLine(line + ", ");
                    }
                    else
                    {
                        file.Write(line + ", ");
                    }
                    rettrIdx++;
                }
            }

            return File.ReadAllText(name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += delegate
            {
                scanAll(textBoxIn.Text);
            };
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ProcessRunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void ProcessRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        progressBar1.Value = 0;

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                });
            }
            else
            {
                progressBar1.Value = 0;
            }
            
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            GenRawFile genRawFile = new GenRawFile();
            genRawFile.ShowDialog();
        }
    }

    public class pivotStringFinder
    {
        public string finderString = string.Empty;
        public int pos = 0;
        public int num = 0;
    }
}

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

namespace ToolConvert
{
    public partial class GenRawFile : Form
    {
        string preData = "static GUI_CONST_STORAGE unsigned long ";

        public GenRawFile()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pathFoder = textBoxFoder.Text;
            string extention = textBoxExtention.Text;
            string target = textBoxTarget.Text;

            //DirectoryInfo di = new DirectoryInfo(pathFoder);
            //FileInfo[] files = di.GetFiles(extention);

            //Array.Sort(files, delegate (FileInfo f1, FileInfo f2) {
            //    return f1.Name.CompareTo(f2.Name);
            //});

            //foreach (FileInfo file in files)
            //{
            //    string newDataName = string.Format("_ac{0}_{1}", target, file.Name).Replace(extention.Replace("*", ""), "");
            //}

            for (int i = 0; i < 60; i++)
            {
                string filenName = string.Format("{0}/{1}.c", pathFoder, i + "");
                if(!File.Exists(filenName))
                {
                    MessageBox.Show("Could not found file: " + filenName);
                    return;
                }
            }

            string allText = "";
            string output = textBoxOutput.Text;

            for(int i = 0; i < 60; i++)
            {
                string filenName = string.Format("{0}/{1}.c", pathFoder, i + "");
                string content = File.ReadAllText(filenName);

                int idxStart = content.IndexOf("{") + 1;
                int idxEnd = content.IndexOf("};");

                string rawContent = content.Substring(idxStart, idxEnd - idxStart);

                string defineData = preData + string.Format("_ac{0}_{1}", target, i);
                defineData += "[] = {\r\n";
                defineData += rawContent;
                defineData += "};\r\n";

                allText += defineData;
            }

            File.WriteAllText(output, allText);
            MessageBox.Show("Generate file : " + output);
        }
    }
}

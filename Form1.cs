using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileStats
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnFile_Click(object sender, EventArgs e)
        {
            FileInfo fi = null;

            listView1.View = View.Details;
            listView1.Columns.Add("Byte Value");
            listView1.Columns.Add("Count");

            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Select a file";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fi = new FileInfo(dialog.FileName);
                }
                if (fi != null)
                {
                    if (fi.Length > 0)
                    {
                        txtFileName.Text = fi.FullName;
                        txtFileSize.Text = Convert.ToString(fi.Length);
                        FileRead(fi);
                    }
                    else
                    {
                        MessageBox.Show("File size should be at least one byte of size", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FileRead(FileInfo fi)
        {
            Thread oThread = new Thread(() => FileProcess(fi));
            oThread.Start();
        }

        private void FileProcess(FileInfo fi)
        {
            int byteCapacity = 2048;

            FileStream fs = new FileStream(fi.FullName, FileMode.Open, System.IO.FileAccess.Read);

            byte[] chunk = new byte[byteCapacity];
            Queue<byte> fQueue = new Queue<byte>();

            while (true)
            {
                int index = 0;
                while (index < chunk.Length)
                {
                    int bytesRead = fs.Read(chunk, index, chunk.Length - index);
                    if (bytesRead == 0)
                        break;
                    index += bytesRead;
                }

                if (index != 0)
                {
                    byte[] cbuffer = new byte[index];
                    fs.Lock(0, byteCapacity);
                    for (int i = 0; i < index; i++)
                    {
                        fQueue.Enqueue(chunk[i]);
                    }
                    fs.Unlock(0, byteCapacity);
                }
                if (index != byteCapacity)
                {
                    fs.Flush();
                    fs.Close();
                    ListByteCount(fQueue);
                    return;
                }
            }
        }

        private void ListByteCount(Queue<byte> fQueue)
        {
            Dictionary<string, int> byteList = new Dictionary<string, int>();

            byte[] byteArray = new byte[fQueue.Count];

            for (int i = 0; i < byteArray.Length; i++)
            {
                string byt = fQueue.Dequeue().ToString("X2");

                if (byteList.ContainsKey(byt))
                {
                    byteList[byt] = GetCount(byteList, byt) + 1;
                }
                else
                    byteList[byt] = 1;
            }

            var listByteData = byteList.OrderByDescending(r => r.Value).ToList();


            foreach (var item in listByteData)
            {
                Invoke(new MethodInvoker(
                    delegate { listView1.Items.Add(new ListViewItem(new string[] { item.Key, item.Value.ToString() })); }
                    ));
            }
        }

        private int GetCount(Dictionary<string, int> byteList, string key)
        {
            int countVal;
            byteList.TryGetValue(key, out countVal);
            return countVal;
        }
    }
}

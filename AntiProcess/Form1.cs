using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AntiProcess
{
    public partial class Form1 : Form
    {

        private int WM_SYSCOMMAND = 0x112;
        private IntPtr SC_MINIMIZE = (IntPtr)0xF020;
        List<string> names = new List<string>();
        //Hashtable setting;
        Setting setting = new Setting();

        const string filename = "antiprocess.xml";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = (int)interval.Value;
            setting.Interval = (int)interval.Value;
            names.Clear();
            foreach (var item in listBox1.Items)
                names.Add(item.ToString());
            setting.Names = names;

            XmlSerialize(filename, setting);
            MessageBox.Show("設定を保存しました", "AntiProcess");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (var name in names)
                {
                    System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(name);
                    foreach (var p in ps)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            interval.Value = timer1.Interval;
            timer1.Start();
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    setting = XmlDeserialize(filename);
                    timer1.Interval = setting.Interval;
                    interval.Value = setting.Interval;
                    names = setting.Names;
                    foreach (var item in names)
                        listBox1.Items.Add(item);
                    //names.Clear();

                }catch{

                }
            }
            else
            {
                setting.Interval = 100;
                XmlSerialize(filename, setting);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
            textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        #region タスクトレイ関連

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        protected override void WndProc(ref Message m)
        {
            //最小化されたときにフォームを非表示にする
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == SC_MINIMIZE))
            {
                this.Hide();
            }
            //上記以外はデフォルトの処理をおこなう
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion

        private void XmlSerialize(string FILENAME, object obj)
        {
            System.Xml.Serialization.XmlSerializer serializer = new
                System.Xml.Serialization.XmlSerializer(typeof(Setting));

            System.IO.FileStream fs = new System.IO.FileStream(FILENAME, System.IO.FileMode.Create);
            serializer.Serialize(fs, obj);
            fs.Close();
        }
        private Setting XmlDeserialize(string FILENAME)
        {

            System.Xml.Serialization.XmlSerializer serializer = new
                System.Xml.Serialization.XmlSerializer(typeof(Setting));

            System.IO.FileStream fs = new System.IO.FileStream(FILENAME, System.IO.FileMode.Open);

            var obj = (Setting)serializer.Deserialize(fs);
            fs.Close();

            return obj;
        }

    }

    public class Setting
    {
        public int Interval { get; set; }
        public List<string> Names { get; set; }
    }

}

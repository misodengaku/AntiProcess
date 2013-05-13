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
        Hashtable setting;
        const string filename = "antiprocess.xml";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = (int)interval.Value;
            names.Clear();
            foreach (var item in listBox1.Items)
                names.Add(item.ToString());
            setting["Interval"] = timer1.Interval;
            setting["Names"] = names[0];
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
                    timer1.Interval = (int)setting["Interval"];
                    interval.Value = (int)setting["Interval"];
                    listBox1.Items.Add((string)setting["Names"]);
                    names.Clear();
                    foreach (var item in listBox1.Items)
                        names.Add(item.ToString());

                }catch{
                    setting = new Hashtable();
                }
            }
            else
            {
                setting = new Hashtable();
                setting["Interval"] = 100;
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


        #region XML関連
        /// <summary>
        /// DictionaryEntry配列をHashtableに変換する
        /// </summary>
        /// <param name="ary">変換するDictionaryEntry配列</param>
        /// <returns>変換されたHashtable</returns>
        public static Hashtable ConvertArrayToHashtable(DictionaryEntry[] ary)
        {
            Hashtable ht = new Hashtable(ary.Length);
            foreach (DictionaryEntry de in ary)
            {
                ht.Add(de.Key, de.Value);
            }
            return ht;
        }


        /// <summary>
        /// HashtableをDictionaryEntryの配列に変換する
        /// </summary>
        /// <param name="ht">変換するHashtable</param>
        /// <returns>変換されたDictionaryEntry配列</returns>
        public static DictionaryEntry[] ConvertHashtableToArray(Hashtable ht)
        {
            DictionaryEntry[] entries = new DictionaryEntry[ht.Count];
            int entryIndex = 0;
            foreach (DictionaryEntry de in ht)
            {
                entries[entryIndex] = de;
                entryIndex++;
            }
            return entries;
        }

        /// <summary>
        /// HashtableをXMLファイルに保存する
        /// </summary>
        /// <param name="fileName">保存先のファイル名</param>
        /// <param name="ht">保存するHashtable</param>
        public static void XmlSerialize(string fileName, Hashtable ht)
        {
            //シリアル化できる型に変換
            DictionaryEntry[] obj = ConvertHashtableToArray(ht);

            //XMLファイルに保存
            XmlSerializer serializer =
                new XmlSerializer(typeof(DictionaryEntry[]));
            FileStream fs = new FileStream(fileName, FileMode.Create);
            serializer.Serialize(fs, obj);
            fs.Close();
        }

        /// <summary>
        /// シリアル化されたXMLファイルからHashtableを復元する
        /// </summary>
        /// <param name="fileName">復元するXMLファイル名</param>
        /// <returns>復元されたHashtable</returns>
        public static Hashtable XmlDeserialize(string fileName)
        {
            //XMLファイルから復元
            FileStream fs = new FileStream(fileName, FileMode.Open);
            try
            {
                XmlSerializer serializer =
                    new XmlSerializer(typeof(DictionaryEntry[]));
                DictionaryEntry[] obj = (DictionaryEntry[])serializer.Deserialize(fs);
                fs.Close();

                //Hashtableに戻す
                Hashtable ht = ConvertArrayToHashtable(obj);
                return ht;
            }
            catch
            {
                throw new Exception("Deserialize error.");
            }
            finally
            {
                fs.Close();
            }
        }
        #endregion

    }
}

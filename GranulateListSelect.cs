using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LUPLoader
{
    public partial class GranulateListSelect : Form
    {
        List<Tuple<string, string>> Granulates;
        public string SelectedGranulate;
        public GranulateListSelect()
        {
            InitializeComponent();
            Granulates = GranulateNames();
            RefillList();
        }


        public List<Tuple<string,string>> GranulateNames()
        {
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            var fulllist=SAPConnect.AppData.Instance.GetMaterialNames();
            fulllist.RemoveAll(m => !gran.Contains(m.No));
            var res=fulllist.Select(m => new Tuple<string,string>(m.No, m.Name)).ToList();
            return res;
        }

        private void RefillList()
        {
            listBox1.Items.Clear();
            Granulates.ForEach(g=>listBox1.Items.Add($"{g.Item2} ({g.Item1.TrimStart('0')})"));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                SelectedGranulate = Granulates[listBox1.SelectedIndex].Item1.TrimStart('0');
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                btnOK.PerformClick();
            }
        }


    }
}

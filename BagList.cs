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
    public partial class BagList : Form
    {
        string Granulate;
        string Batch;
        public BagList(string granulate,string batch)
        {
            Granulate = granulate;
            Batch = batch;
            
            InitializeComponent();
            ShowList();
            Text = "Гранулят: " + granulate + " Партия: " + batch;
        }

        private void ShowList()
        {
            if (String.IsNullOrWhiteSpace(Granulate)) return;

            UPMAction.PrepareBagsList(Granulate, Batch);

            var LastBag = UPMAction.GetLastBag(Granulate,Batch,false);
            var lst=UPMAction.HU_At_UPM_ForLoad(Granulate, Batch);
            listView1.Items.Clear();
            int n = 0;
            lst.ForEach(hu => 
            {
                var li = new ListViewItem(new string[] { (++n).ToString(), hu.SU.TrimStart('0'), Granulate,hu.Batch, hu.Quantity.ToString(), hu.DT.ToString(), hu.TransferOrderNumber.ToString() }) ;
                listView1.Items.Add(li);
            });
        }
    }
}

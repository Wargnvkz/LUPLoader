﻿using System;
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
        public BagList(string granulate)
        {
            Granulate = granulate;
            InitializeComponent();
            ShowList();
        }

        private void ShowList()
        {
            if (String.IsNullOrWhiteSpace(Granulate)) return;
            var LastBag = UPMAction.GetLastBag(Granulate);
            var lb = new LUPLoader.LastBag() { LastBagDateTime = LastBag.LastBag, TransferOrder = LastBag.LastTransferOrder };
            var lst=UPMAction.HU_At_UPM(Granulate, lb);
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

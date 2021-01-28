namespace LUPLoader
{
    partial class BagList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colN = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHU = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMaterial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBatch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colQuantity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTO = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            listView1 = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // listView1
            // 
            listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colN,
            this.colHU,
            this.colMaterial,
            this.colBatch,
            this.colQuantity,
            this.colDateTime,
            this.colTO});
            listView1.FullRowSelect = true;
            listView1.HideSelection = false;
            listView1.Location = new System.Drawing.Point(0, 2);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(800, 448);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.Details;
            // 
            // colN
            // 
            this.colN.Text = "№";
            // 
            // colHU
            // 
            this.colHU.Text = "Номер ЕО мешка";
            this.colHU.Width = 120;
            // 
            // colMaterial
            // 
            this.colMaterial.Text = "Материал";
            this.colMaterial.Width = 80;
            // 
            // colBatch
            // 
            this.colBatch.Text = "Номер партии";
            this.colBatch.Width = 120;
            // 
            // colQuantity
            // 
            this.colQuantity.Text = "Количество материала";
            this.colQuantity.Width = 120;
            // 
            // colDateTime
            // 
            this.colDateTime.Text = "Дата и время поступления";
            this.colDateTime.Width = 150;
            // 
            // colTO
            // 
            this.colTO.Text = "Номер ТЗ";
            this.colTO.Width = 120;
            // 
            // BagList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(listView1);
            this.Name = "BagList";
            this.Text = "BagList";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colHU;
        private System.Windows.Forms.ColumnHeader colBatch;
        private System.Windows.Forms.ColumnHeader colDateTime;
        private System.Windows.Forms.ColumnHeader colTO;
        private System.Windows.Forms.ColumnHeader colN;
        private System.Windows.Forms.ColumnHeader colQuantity;
        private System.Windows.Forms.ColumnHeader colMaterial;
    }
}
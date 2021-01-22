using STRAINER_EXTRACT.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STRAINER_EXTRACT
{
    public partial class frmMain : Form
    {
        StoredProcController controller = null;

        public frmMain()
        {
            InitializeComponent();
            controller = new StoredProcController();
           
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Cleaning folders...";
                controller.ClearFile();
                lblStatus.Text = "Start generating text files...";
                controller.Extract();
                lblStatus.Text = "Succesfully generated text files...";
                controller.GetZip(lblStatus);

            }
            catch (Exception er)
            {

                MessageBox.Show(er.Message);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            CheckBox box;
            for (int i = 0; i < 10; i++)
            {
                box = new CheckBox();
                box.Tag = i.ToString();
                box.Text = "All";
                box.AutoSize = true;
                box.Location = new Point(10, 10);
                this.tableLayoutPanel1.Controls.Add(box);
                box.CheckedChanged += Box_CheckedChanged;
            }
        }

        private void Box_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }
}

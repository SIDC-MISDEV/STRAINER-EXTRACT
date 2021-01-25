using STRAINER_EXTRACT.Controller;
using STRAINER_EXTRACT.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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

            this.Text = Application.ProductName;

            treeView1.AfterCheck += TreeView1_AfterCheck;
        }

        private void TreeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Text == "All Transaction Types")
            {
                foreach (TreeNode rootNodes in treeView1.Nodes)
                {
                    if (rootNodes.Text != "All Transaction Types")
                        rootNodes.Checked = e.Node.Checked;

                    
                }
            }
            else
            {
                if (e.Node.Nodes.Count > 0)
                {
                    foreach (TreeNode childNode in e.Node.Nodes)
                    {
                        childNode.Checked = e.Node.Checked;
                    }
                }
            }
        }

        private void Initialize()
        {
            try
            {
                var folders = Properties.Settings.Default.FOLDERS.Split(',');
                var prefix = new List<Prefix>();


                controller.InitializeFolders();

                prefix = controller.GetPrefixes();

                treeView1.Nodes.Add("All Transaction Types");

                for (int i = 0; i < folders.Length - 1; i++)
                {
                    var prefixes = new List<string>();

                    treeView1.Nodes.Add(folders[i]);

                    switch (folders[i])
                    {
                        case "AR":

                            prefixes = prefix.Where(s => s.ObjectType == "13").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "GI":

                            prefixes = prefix.Where(s => s.ObjectType == "60").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "GR":

                            prefixes = prefix.Where(s => s.ObjectType == "59").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "IP":

                            prefixes = prefix.Where(s => s.ObjectType == "24").Select(f => f.ObjectPrefix).ToList();
                            prefixes.Add("SI");

                            break;

                        case "PR":

                            prefixes = prefix.Where(s => s.ObjectType == "22").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "RC":

                            prefixes = prefix.Where(s => s.ObjectType == "14").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "RG":

                            prefixes = prefix.Where(s => s.ObjectType == "21").Select(f => f.ObjectPrefix).ToList();

                            break;

                        case "RV":

                            prefixes = prefix.Where(s => s.ObjectType == "13").Select(f => f.ObjectPrefix).ToList();

                            break;

                        default:
                            break;
                    }

                    foreach (var pref in prefixes)
                    {
                        treeView1.Nodes[i + 1].Nodes.Add(pref);
                    }
                }

                //CheckUncheckNodes(treeView1.Nodes, true);
            }
            catch
            {

                throw;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var forGenerate = new List<string>();

                btnGenerate.Enabled = false;

                ThreadHelper.SetButtonEnable(this, btnGenerate, false);

                foreach (TreeNode rootNote in treeView1.Nodes)
                {
                    if (rootNote.Text != "All Transaction Types")
                    {
                        foreach (TreeNode childNode in rootNote.Nodes)
                        {
                            if (childNode.Checked)
                            {
                                forGenerate.Add($"{rootNote.Text}_{childNode.Text}");
                            }
                        }
                    }
                }

                controller.ClearFile();

                controller.Extract(forGenerate, dtDate.Value.ToString("yyyy-MM-dd"));
            }
            catch (Exception er)
            {

                MessageBox.Show(er.Message);
            }
            finally
            {
                btnGenerate.Enabled = true;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception er)
            {
                MessageBox.Show(this, er.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
           
        }


    }
}

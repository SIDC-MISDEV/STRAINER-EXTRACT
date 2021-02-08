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

        private delegate void SetButtonState(bool enabled);
        BackgroundWorker bg = new BackgroundWorker();


        public frmMain()
        {
            InitializeComponent();
            controller = new StoredProcController(this);

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

                //check all folders are exist
                controller.InitializeFolders();

                //clear all unecessary files
                controller.ClearFile(Properties.Settings.Default.TEMP_FOLDER);

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

                            prefixes = prefix.Where(s => s.ObjectType == "20").Select(f => f.ObjectPrefix).ToList();

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
            treeView1.Enabled = false;

            var thread = new Thread(StartGeneration);
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartGeneration()
        {
            try
            {
                var forGenerate = new List<string>();
                List<string> rcTransType = new List<string>();

                ThreadHelper.SetControlState(this, btnGenerate, false);
                ThreadHelper.SetControlState(this, treeView1, false);

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

                foreach (TreeNode rootNote in treeView1.Nodes)
                {
                    if (rootNote.Text == "AR")
                    {
                        foreach (TreeNode childNode in rootNote.Nodes)
                        {
                            rcTransType.Add(childNode.Text.ToString());
                        }
                    }
                }

                controller.ClearFile();


                controller.Extract(forGenerate, dtDate.Value.ToString("yyyy-MM-dd"), rcTransType);

                controller.FinalSync();

                MessageBox.Show("Generation completed!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ThreadHelper.SetControlState(this, btnGenerate, true);
                //ThreadHelper.SetValue(this, progressBar1, 0, 100);
                ThreadHelper.SetControlState(this, treeView1, true);

            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                var details = controller.GetBranchName();

                if (!string.IsNullOrEmpty(details.BranchName))
                {
                    txtBranchName.Text = details.BranchName;
                    txtWarehouseCode.Text = details.WarehouseCode;
                }

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

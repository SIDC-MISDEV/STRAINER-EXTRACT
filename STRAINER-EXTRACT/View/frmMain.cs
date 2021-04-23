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
        List<Prefix> prefix = new List<Prefix>();
        StoredProcController controller = null;
        private delegate void SetButtonState(bool enabled);
        BackgroundWorker bg = new BackgroundWorker();
        string batchReference = string.Empty;
        int autorun = Properties.Settings.Default.AUTO_RUN;

        private delegate void CloseForm();

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
                            //prefixes.Add("SI");

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
            if (!string.IsNullOrEmpty(txtBranchName.Text) && !string.IsNullOrEmpty(txtWarehouseCode.Text))
            {
                bool generated = false;

                foreach (TreeNode rootNote in treeView1.Nodes)
                {
                    foreach (TreeNode childNode in rootNote.Nodes)
                    {
                        if (childNode.Checked)
                        {
                            generated = true;
                        }
                    }
                }

                if (!generated)
                {
                    MessageBox.Show(this, "Unable to generate due to no transaction was checked. Please check any or all transaction type and try again.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    treeView1.Enabled = false;

                    var thread = new Thread(StartGeneration);
                    thread.IsBackground = true;
                    thread.Start();
                }
                
            }
            else
            {
                MessageBox.Show(this, "Please set branch code and warehouse code before generating.",Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartGeneration()
        {
            try
            {
                var forGenerate = new List<string>();
                List<string> rcTransType = new List<string>();
                List<string> trType = new List<string>();
                string trVal = string.Empty;

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
                                trType.Add(childNode.Text.ToString());
                            }
                        }
                    }
                    else
                    {
                        foreach (TreeNode childNode in rootNote.Nodes)
                        {
                            trType.Add(childNode.Text.ToString());
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
                controller.ClearFile(Properties.Settings.Default.TEMP_FOLDER);


                controller.Extract(forGenerate, dtDate.Value.ToString("yyyy-MM-dd"), rcTransType, batchReference);

                //trVal = $"'{string.Join(",'", trType)}'";

                //update transaction ectracted
                controller.UpdateExtracted(dtDate.Value.ToString("yyyy-MM-dd"), trType);

                int sentFiles = controller.FinalSync();
                

                if (autorun == 0)
                {
                    MessageBox.Show($"{sentFiles} file(s) successfully generated.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    CloseApp();
                }
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ThreadHelper.SetLabel(this, lblStatus, "");
                ThreadHelper.SetControlState(this, btnGenerate, true);
                //ThreadHelper.SetValue(this, progressBar1, 0, 100);
                ThreadHelper.SetControlState(this, treeView1, true);

            }
        }

        private void CloseApp()
        {
            if (this.InvokeRequired)
            {
                CloseForm s = new CloseForm(CloseApp);
                this.Invoke(s, new object[] { });
            }
            else
                this.Close();
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
                    batchReference = details.BranchCodeNumber;
                }

                Initialize();

                if (autorun == 1)
                {
                    foreach (TreeNode rootNode in treeView1.Nodes)
                    {
                        if (rootNode.Text == "All Transaction Types")
                        {
                            foreach (TreeNode rootNodes in treeView1.Nodes)
                            {
                                if (rootNodes.Text != "All Transaction Types")
                                    rootNodes.Checked = true;

                            }
                        }
                    }

                    btnGenerate.PerformClick();
                }

            }
            catch (Exception er)
            {
                MessageBox.Show(this, er.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
           
        }


    }
}

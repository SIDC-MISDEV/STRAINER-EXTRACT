using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STRAINER_EXTRACT.Service;
using System.Data;
using Ionic.Zip;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Threading;
using STRAINER_EXTRACT.Model;

namespace STRAINER_EXTRACT.Controller
{
    public class StoredProcController
    {
        MySQLHelper db = new MySQLHelper();

        private string transactionFolders = Properties.Settings.Default.FOLDERS;
        private string tempPath = Properties.Settings.Default.TEMP_FOLDER;
        private string syncFolders = Properties.Settings.Default.FOR_SYNC_FOLDER;
        private string dropSitePath = Properties.Settings.Default.DROPSITE_FOLDER;

        private string zipPassword = "m1s@dm1n";
        private string compressedFileName = string.Empty;

        private Dictionary<string, string[]> storedProcedures = new Dictionary<string, string[]>()
        {
            { "AR", Properties.Settings.Default.AR_STORED_PROC.Split(',') },
            { "GI", Properties.Settings.Default.GI_STORED_PROC.Split(',') },
            { "GR", Properties.Settings.Default.GR_STORED_PROC.Split(',') },
            { "PR", Properties.Settings.Default.PR_STORED_PROC.Split(',') },
            { "RC", Properties.Settings.Default.RC_STORED_PROC.Split(',') },
            { "RG", Properties.Settings.Default.RG_STORED_PROC.Split(',') },
            { "RV", Properties.Settings.Default.RV_STORED_PROC.Split(',') }
        };

        public static List<string> FolderPath = new List<string>();


        public StoredProcController()
        {

        }

        public void InitializeFolders()
        {
            string[] folders = transactionFolders.Split(',');

            //Dropsite folders of textfiles of every transaction type (AR, GI, etc.)
            foreach (var folder in folders)
            {
                //For dropsite folder of text files before compressing
                if (!Directory.Exists(Path.Combine(dropSitePath, folder)))
                    Directory.CreateDirectory(Path.Combine(dropSitePath, folder));

                //Dropsite folder of temporary textfiles (used for basis of incrementing number of filename).
                if (!Directory.Exists(Path.Combine(tempPath, folder)))
                    Directory.CreateDirectory(Path.Combine(tempPath, folder));

                //For dropsite folder of compressed files.
                if (!Directory.Exists(Path.Combine(syncFolders, folder)))
                    Directory.CreateDirectory(Path.Combine(syncFolders, folder));

            }
        }

        

        public void ClearFile()
        {
            string dropSitePath = @"C:\TestPath\";


            foreach (var checkfolders in Directory.GetDirectories(dropSitePath))
            {
                var checkdropSiteSubFolderFiles = Directory.GetFiles(checkfolders, "*.txt");
                foreach (var checknameFile in checkdropSiteSubFolderFiles)
                {
                    if (Path.GetExtension(checknameFile) == ".txt")
                    {
                        File.Delete(checknameFile);
                    }
                }
            }
        }



        public void GetZip(Label lbl = null)
        {
            
            string tempPath = @"C:\TempPath\";
            string tempFullPath = string.Empty;


            foreach (var folders in Directory.GetDirectories(dropSitePath))
            {
                string[] dropSiteSubFolderFiles = Directory.GetFiles(folders, "*.txt");

                try
                {
                    if (!Directory.Exists(Path.Combine(tempPath, folders.Split('\\').Last())))
                    {
                        Directory.CreateDirectory(Path.Combine(tempPath, folders.Split('\\').Last()));
                    }

                    tempFullPath = Path.Combine(tempPath, folders.Split('\\').Last());

                    //string[] dropSiteSubFolderFiles = Directory.GetFiles(folders, "*.txt");

                    if (dropSiteSubFolderFiles.Count() > 0)
                    {
                        foreach (var textFile in dropSiteSubFolderFiles)
                        {
                            string tempFilePath = tempFullPath;
                            string fileText = Path.GetFileNameWithoutExtension(textFile); // Textfile from dropSiteSubFolderFiles

                            string newTextFile = string.Empty;
                            FileInfo info = new FileInfo(textFile);
                            int count = 1;
                            string newTempFile = string.Empty;


                            string[] textSplitter = fileText.Split('_');
                            newTextFile = $"{textSplitter[0]}_{textSplitter[1]}_{textSplitter[2]}_{textSplitter[3]}_{textSplitter[4]}_{textSplitter[5]}";

                            var tempFolderFiles = Directory.GetFiles(tempFilePath, "*.txt");

                            foreach (var tempFile in tempFolderFiles)
                            {

                                string tempTextFile = Path.GetFileNameWithoutExtension(tempFile); // Textfile from tempFolderFiles

                                string[] tempSplitter = tempTextFile.Split('_');
                                newTempFile = $"{tempSplitter[0]}_{tempSplitter[1]}_{tempSplitter[2]}_{tempSplitter[3]}_{tempSplitter[4]}_{tempSplitter[5]}";


                                if (newTextFile == newTempFile)
                                {
                                    count++;
                                }

                            }

                            if (count > 1)
                            {
                                newTextFile = $"{newTextFile}_{count}.txt";
                                //info.MoveTo(Path.Combine(folders,newTextFile));
                                File.Move(textFile, Path.Combine(folders, newTextFile));
                                File.Copy(Path.Combine(folders, newTextFile), Path.Combine(tempFilePath, newTextFile));
                            }
                            else
                            {
                                File.Copy(textFile, Path.Combine(tempFilePath, Path.GetFileName(textFile)));
                            }

                        }



                        using (ZipFile zip = new ZipFile())
                        {

                            string fileName = string.Empty;

                            string compressedFilePath = Path.Combine(syncFolders, folders.Split('\\').Last());

                            foreach (var nameFile in dropSiteSubFolderFiles)
                            {

                                fileName = Path.GetFileNameWithoutExtension(nameFile);
                                //char[] delimiters = {'_'};
                                string[] splitter = fileName.Split('_');
                                fileName = $"{splitter[0]}_{splitter[1]}_{splitter[2]}_{splitter[3]}_{splitter[4]}";
                                break;


                            }

                            var zipFiles = Directory.GetFiles(compressedFilePath, "*.zip");

                            if (zipFiles.Count() > 0)
                            {
                                int count = zipFiles.Count() + 1;
                                compressedFileName = $"{fileName}_{count}.zip";
                            }
                            else
                            {
                                compressedFileName = $"{fileName}_1.zip";
                            }

                            zip.Password = zipPassword;

                            zip.AddDirectory(folders);

                            zip.Save(Path.Combine(compressedFilePath, compressedFileName));

                            dropSiteSubFolderFiles = Directory.GetFiles(folders, "*.txt");

                        }
                    }
                }
                finally
                {
                    foreach (var item in dropSiteSubFolderFiles)
                    {
                        File.Delete(item);
                    }
                }
            }

        }

        public List<Prefix> GetPrefixes()
        {
            try
            {
                var result = new List<Prefix>();

                db = new MySQLHelper();

                result = db.GetPrefixData();

                return result;
                
            }
            catch
            {

                throw;
            }
        }

        public void Extract(List<string> query, string date)
        {

            try
            {
               
                foreach (var item in query)
                {
                    var parameter = item.Split('_');

                    string[] scripts = storedProcedures[parameter[0]];

                    foreach (var _query in scripts)
                    {
                        db = new MySQLHelper();

                        string queryString = $"CALL {_query}('{parameter[1]}', '{date}');";

                        db.GetExtract(queryString);
                    }

                    GetZip();

                }

                

            }
            catch 
            {

                throw;
            }

        }
    }
}

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

namespace STRAINER_EXTRACT.Controller
{
    public class StoredProcController
    {
        MySQLHelper db = new MySQLHelper();
        //private string dropSitePath = @"C:\Projecterp\Pallocan";
        private string zipPassword = "m1s@dm1n";
        private string compressedFileName = string.Empty;
        public static List<string> FolderPath = new List<string>();


        public StoredProcController()
        {

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



        public void GetZip(Label lbl )
        {
            string dropSitePath = @"C:\TestPath\";
            string tempPath = @"C:\TempPath\";
            string tempFullPath = string.Empty;


            foreach (var folders in Directory.GetDirectories(dropSitePath))
            {
                if (!Directory.Exists(Path.Combine(tempPath, folders.Split('\\').Last())))
                {
                    Directory.CreateDirectory(Path.Combine(tempPath, folders.Split('\\').Last()));
                }

                tempFullPath = Path.Combine(tempPath, folders.Split('\\').Last());

                string[] dropSiteSubFolderFiles = Directory.GetFiles(folders,"*.txt");

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
                            

                            if (newTextFile==newTempFile)
                            {
                                count++;
                            }

                        }
                        
                      

                       

                        if (count>1)
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
                        lbl.Text = "Start Compressing files";
                        Thread.Sleep(1000);

                        string fileName = string.Empty;
                        //string compressedFilePath = Path.Combine(@"C:\Projecterp\Pallocan\", folders.Split('\\').Last());
                        string compressedFilePath = Path.Combine(@"C:\Projecterp\Pallocan\", folders.Split('\\').Last());

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

                        //dropSiteSubFolderFiles = Directory.GetFiles(folders, "*.txt");
                        //var tf = Directory.EnumerateFiles(folders, "*.txt").Select(Path.GetFileName);

                        //zip.AddFiles(dropSiteSubFolderFiles);

                        zip.AddDirectory(folders);


                        //zip.Save(Path.Combine(folders, compressedFileName));
                        zip.Save(Path.Combine(compressedFilePath, compressedFileName));

                        lbl.Text = "Successfully compressing files";

                        foreach (var nameFile in dropSiteSubFolderFiles)
                        {

                            if (Path.GetExtension(nameFile) == ".txt")
                            {
                                File.Delete(nameFile);
                            }

                        }


                    }
                }
            }

        }


        public void Extract()

        {

            try
            {
                

                db = new MySQLHelper();

                db.GetExtract(@"CALL ARStored_1(); 
                                CALL ARStored_2();
                                CALL ARStored_3();
                                CALL ARStored_4();
                                CALL GIStored_1();");

            }
            catch (Exception)
            {

                throw;
            }

        }



    }


    public enum AR
    {
        SI,CO,CG,VS,CI,CT,EC,FS,AP,SR,CS,PI
    }
}

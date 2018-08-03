using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Drawing;
using DevExpress.XtraScheduler.Internal.Implementations;
using Prefetch;
using JumpList;
using Lnk;
using Lnk.ExtraData;
using Lnk.ShellItems;
using ExtensionBlocks;
using Header = Lnk.Header;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Microsoft.Win32;


namespace WindowsTimelineAnalysis
{
    public partial class TimelineControl : DevExpress.XtraEditors.XtraUserControl
    {

        public TimelineControl(string n)
        {
            InitializeComponent();
            
            this.Text = n;
            this.Name = n + "TimelineControl";

            schedulerControl.Start = System.DateTime.Now;

            schedulerControl.ToolTipController = toolTipController1;
            toolTipController1.ShowBeak = true;
            toolTipController1.ToolTipType = ToolTipType.Standard;

            schedulerControl.OptionsCustomization.AllowDisplayAppointmentFlyout = false;
        }

        private void toolTipController1_BeforeShow(object sender, ToolTipControllerShowEventArgs e)
        {
            ToolTipController controller = sender as ToolTipController;
            AppointmentViewInfo aptViewInfo = controller.ActiveObject as AppointmentViewInfo;
            if (aptViewInfo == null) return;

            if (toolTipController1.ToolTipType == ToolTipType.Standard)
            {
                e.IconType = ToolTipIconType.Information;
                e.ToolTip = aptViewInfo.Description;
            }

            if (toolTipController1.ToolTipType == ToolTipType.SuperTip)
            {
                SuperToolTip SuperTip = new SuperToolTip();
                SuperToolTipSetupArgs args = new SuperToolTipSetupArgs();
                args.Title.Text = "Info";
                args.Title.Font = new Font("Times New Roman", 14);
                args.Contents.Text = aptViewInfo.Description;
                //args.Contents.Image = resImage;
                args.ShowFooterSeparator = true;
                args.Footer.Font = new Font("Comic Sans MS", 8);
                args.Footer.Text = "SuperTip";
                SuperTip.Setup(args);
                e.SuperTip = SuperTip;
            }
        }

        public void Analyize(string module)
        {
            switch (module)
            {
                case "Prefetch":
                {
                    AnalyizePrefetch();
                    break;
                }
                case "Jumplist":
                {
                    AnalyzeJumplist();
                    break;
                }
                case "Lnk":
                {
                    AnalyzeLnk();
                    break;
                }
                case "Shellbags":
                {
                    AnalyzeShellbags();
                    break;
                }
                case "Google Chrome":
                {
                    AnalyzeGoogleChrome();
                    break;
                }
                case "Firefox":
                {
                    AnalyzeFirefox();
                    break;
                }
            }
        }

        void AnalyizePrefetch()
        {
            schedulerStorage.Appointments.Clear();
            string[] prefetchFiles =
                Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Prefetch\\",
                    "*.pf");
            IPrefetch pf = null;

            DataTable prefetchDataTable = new DataTable();

            prefetchDataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Source File Name"),
                new DataColumn("Source Created"),
                new DataColumn("Source Modified"),
                new DataColumn("Source Accessed"),
                new DataColumn("Executable Name"),
                new DataColumn("Hash"),
                new DataColumn("Size"),
                new DataColumn("Version"),
                new DataColumn("Run Count"),
                new DataColumn("Last Run Times"),
                new DataColumn("Files Loaded")
            });

            foreach (string file in prefetchFiles)
            {
                try
                {
                    pf = Prefetch.PrefetchFile.Open(file);
                }
                catch (Exception ee)
                {
                    continue;
                }
                Appointment n = new AppointmentInstance();
                n.Subject = pf.SourceFilename;
                n.Start = DateTimeOffset.Parse(pf.SourceModifiedOn.ToString(), null).DateTime;
                n.End = DateTimeOffset.Parse(pf.SourceModifiedOn.ToString(), null).DateTime;

                string description = "Source Filename: " + pf.SourceFilename + "\nSourced Created on:" +
                                     pf.SourceAccessedOn + "\nSource Accessed on: " + pf.SourceAccessedOn +
                                     "\nSource Modified on:" + pf.SourceModifiedOn + "\nExecutable name "
                                     + pf.Header.ExecutableFilename + "\nHash:" + pf.Header.Hash + "\nSize" +
                                     pf.Header.FileSize.ToString() + "\nVersion:" + pf.Header.Version +
                                     "\nRun Count: " + pf.RunCount.ToString();


                n.Description = description;
                schedulerStorage.Appointments.Items.Add(n);


                string sourceFilename = pf.SourceFilename;
                DateTime sourceCreatedOn = DateTimeOffset.Parse(pf.SourceCreatedOn.ToString(), null).DateTime;
                DateTime sourceAccessedOn = DateTimeOffset.Parse(pf.SourceAccessedOn.ToString(), null).DateTime;
                DateTime sourceModifiedOn = DateTimeOffset.Parse(pf.SourceModifiedOn.ToString(), null).DateTime;

                string executableName = pf.Header.ExecutableFilename;
                string hash = pf.Header.Hash.ToString();
                int size = pf.Header.FileSize;

                Prefetch.Version version = pf.Header.Version;

                int runCount = pf.RunCount;

               

                /*foreach (var runTimes in pf.LastRunTimes)
                {
                     string runTimeDate = runTimes.ToString();

                 }*/
                List<DateTimeOffset> lastRunTimes = pf.LastRunTimes;
                List<DateTime> lrTimes = new List<DateTime>();
                foreach (var rt in lastRunTimes)
                {
                    lrTimes.Add(DateTimeOffset.Parse(rt.ToString(), null).DateTime);
                }
                string runTimes = string.Join(", ", lrTimes.ToArray());


                List<string> filesloaded = pf.Filenames;
                List<string> floaded = new List<string>();

                foreach (var files in filesloaded)
                {
                    floaded.Add(files.ToString());
                }

                string loadedFiles = string.Join(",", floaded.ToArray());


                /*  foreach (var loadedFilename in pf.Filenames)
                   {
                       Console.WriteLine(loadedFilename);
                   }*/






                prefetchDataTable.Rows.Add(sourceFilename, sourceCreatedOn, sourceAccessedOn, sourceModifiedOn,
                    executableName, hash, size, version, runCount, runTimes, loadedFiles);
            }
            gridControl1.DataSource = prefetchDataTable;
        }

        void AnalyzeJumplist()
        {
            schedulerStorage.Appointments.Clear();
            string[] jumpListFilesAutomatic =
                Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                   "\\Microsoft\\Windows\\Recent\\AutomaticDestinations\\",
                    "*.automaticDestinations-ms");



            DataTable jumpListDataTable = new DataTable();

            jumpListDataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Source File"),
                new DataColumn("App Id"),
                new DataColumn("File Path"),
                new DataColumn("Created Time"),
                new DataColumn("Modified Time"),
                new DataColumn("Accessed Time"),
                new DataColumn("Size"),
                new DataColumn("Entry Id"),
                new DataColumn("Machine Name"),
                new DataColumn("MAC Address"),
                new DataColumn("Pinned Status")
            });

            foreach (var autofiles in jumpListFilesAutomatic)
            {
                var autoDestFile = JumpList.JumpList.LoadAutoJumplist(autofiles);

                if (autoDestFile.DestListEntries.Count > 0)
                {
                    foreach (var automaticfileEntry in autoDestFile.DestListEntries)
                    {
                        Appointment n = new AppointmentInstance();
                        n.Subject = automaticfileEntry.Path;
                        n.Start = DateTimeOffset.Parse(automaticfileEntry.LastModified.ToString(), null).DateTime;
                        n.End = DateTimeOffset.Parse(automaticfileEntry.LastModified.ToString(), null).DateTime;

                        string description = "File Path: " + automaticfileEntry.Path + "\nModified On: " +
                                             automaticfileEntry.LastModified + "\nCreated On: " +
                                             automaticfileEntry.CreatedOn + "\nFile Size: " +
                                             automaticfileEntry.Lnk.Header.FileSize;

                        n.Description = description;
                        schedulerStorage.Appointments.Items.Add(n);


                        string sourceFile = autoDestFile.SourceFile;
                        AppIdInfo appId = autoDestFile.AppId;

                        int entryId = automaticfileEntry.EntryNumber;

                        string entryPath = automaticfileEntry.Path;

                        DateTimeOffset targetCreatedOn = automaticfileEntry.Lnk.Header.TargetCreationDate.DateTime;
                        DateTimeOffset targetModifiedOn = automaticfileEntry.Lnk.Header.TargetModificationDate.DateTime;
                        DateTimeOffset targetLastAccessedOn =
                            automaticfileEntry.Lnk.Header.TargetLastAccessedDate.DateTime;

                        uint fileSize = automaticfileEntry.Lnk.Header.FileSize;

                        string machineName = automaticfileEntry.Hostname;
                        string macAdd = automaticfileEntry.MacAddress;
                        bool pinnedStatus = automaticfileEntry.Pinned;

                        jumpListDataTable.Rows.Add(sourceFile, appId, entryPath, targetCreatedOn, targetModifiedOn,
                            targetLastAccessedOn, fileSize, entryId, machineName, macAdd, pinnedStatus);



                    }
                }
            }


            string[] jumpListFilesCustom = Directory.GetFiles(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\Microsoft\\Windows\\Recent\\CustomDestinations\\   ", "*.customDestinations-ms");

            foreach (var custfiles in jumpListFilesCustom)
            {
                try
                {
                    var custDestFiles = JumpList.JumpList.LoadCustomJumplist(custfiles);

                    string cSourceFile = custDestFiles.SourceFile;
                    AppIdInfo cAppId = custDestFiles.AppId;
                    int cEntryId = custDestFiles.Entries.Count;
                  


                    // Console.WriteLine("List Count " + custDestFiles.Entries.Count);
                    foreach (var custDestEntry in custDestFiles.Entries)
                    {
                        foreach (var c in custDestEntry.LnkFiles)
                        {
                            string cLocalPath = c.LocalPath;
                            DateTimeOffset cTargetCreatedOn = c.Header.TargetCreationDate.DateTime;

                            DateTimeOffset cTargetModifiedOn = c.Header.TargetModificationDate.DateTime;

                            DateTimeOffset cTargetAccessedOn = c.Header.TargetLastAccessedDate.DateTime;

                            uint cFileSize = c.Header.FileSize;

                            Appointment n = new AppointmentInstance();
                            n.Subject = cSourceFile;
                            n.Start = DateTimeOffset.Parse(cTargetModifiedOn.ToString(), null).DateTime;
                            n.End = DateTimeOffset.Parse(cTargetModifiedOn.ToString(), null).DateTime;


                            string description = "File Path: " + cLocalPath + "\nModified On: " +
                                                 cTargetModifiedOn + "\nCreated On: " +
                                                 cTargetCreatedOn + "\nFile Size: " +
                                                 cFileSize;

                            n.Description = description;
                            schedulerStorage.Appointments.Items.Add(n);

                            jumpListDataTable.Rows.Add(cSourceFile, cAppId, cLocalPath, cTargetCreatedOn,
                                cTargetModifiedOn,
                                cTargetAccessedOn, cFileSize, cEntryId, null, null, null);
                            //dataGridView1.Rows.Add(cSourceFile, cAppId, cLocalPath, cTargetCreatedOn, cTargetModifiedOn, cTargetAccessedOn, cFileSize, cEntryId, null, null, "True");

                        }
                    }
                    //  Console.WriteLine(custDestFiles.);

                    //Console.WriteLine(customDestFiles);
                }
                catch (Exception ee)
                {
                    continue;
                }
            }
            
            gridControl1.DataSource = jumpListDataTable;
        }

        void AnalyzeLnk()
        {
            schedulerStorage.Appointments.Clear();
            string[] lnkFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Recent", "*.lnk");

            DataTable lnkDataTable = new DataTable();

            lnkDataTable.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Filename"),
                    new DataColumn("Lnk File Path"),
                    new DataColumn("Created Time"),
                    new DataColumn("Modified Time"),
                    new DataColumn("Last Accessed Time"), 
                    new DataColumn("File Path"),
                    new DataColumn("Source Created Time"),
                    new DataColumn("Source Last Accessed Time"), 
                    new DataColumn("Source Modified Time"),
                    new DataColumn("File Size"),
                    new DataColumn("Data Flags"),
                    new DataColumn("File Attributes"),
                    new DataColumn("Drive Info"),
                    new DataColumn("Volume Information"),
                    new DataColumn("Machine name"),
                    new DataColumn("MAC Address")
                });

            List<LnkFile> _processedFiles = new List<LnkFile>();

            foreach (var file in lnkFiles)
            {
                var lnk = ProcessFile(file);
                if (lnk != null)
                {
                    _processedFiles.Add(lnk);
                }
            }

            foreach (var file in _processedFiles)
            {
                string temp1 = file.SourceFile.Substring(file.SourceFile.LastIndexOf(@"\") + 1);

                Appointment n = new AppointmentInstance();
                n.Subject = file.LocalPath;
                n.Start = DateTimeOffset.Parse(file.SourceModified.ToString(), null).DateTime;
                n.End = DateTimeOffset.Parse(file.SourceModified.ToString(), null).DateTime;

                string description = "Filename: " + temp1 + "\nSource Created: " + file.SourceCreated.ToString() +
                                     "\nSource File Path" + file.SourceFile +
                                     "\nSource Moified: " + file.SourceModified + "\nSource Accessed: " +
                                     file.SourceAccessed;

                n.Description = description;

                schedulerStorage.Appointments.Items.Add(n);



                string filename = temp1;
                string lnkFilePath = file.SourceFile;
                string localPath = file.LocalPath;
                DateTimeOffset targetCreatedOn = file.Header.TargetCreationDate.DateTime;
                DateTimeOffset targetModifiedOn = file.Header.TargetModificationDate.DateTime;
                DateTimeOffset targetLastAccessedOn = file.Header.TargetLastAccessedDate.DateTime;


                DateTimeOffset sourceCreatedOn = DateTimeOffset.Parse(file.SourceCreated.ToString(), null).DateTime;
                DateTimeOffset sourceLastAccessedOn =
                    DateTimeOffset.Parse(file.SourceAccessed.ToString(), null).DateTime;
                DateTimeOffset sourceModifiedOn = DateTimeOffset.Parse(file.SourceModified.ToString(), null).DateTime;

                uint fileSize = file.Header.FileSize;

                Header.DataFlag dataFlag = file.Header.DataFlags;

                Header.FileAttribute fileAttribute = file.Header.FileAttributes;

                string driveTypes = "";
                string volumeInfo = "";
                // string networkInfo = "";
                string temp3 = "";

                if (file.VolumeInfo != null)
                {

                    driveTypes = GetDescriptionFromEnumValue(file.VolumeInfo.DriveType);
                }
                else
                {
                    driveTypes = "Network share";
                }

                if (file.VolumeInfo != null)
                {
                    volumeInfo = "Volume Name: " + file.VolumeInfo.VolumeLabel + ", " + "Volume Sr No:" +
                                 file.VolumeInfo.VolumeSerialNumber;
                }

                string machineName = "";
                string macAdd = "";
                if (file.ExtraBlocks.Count > 0)
                {
                    foreach (var extraDataBase in file.ExtraBlocks)
                    {

                        switch (extraDataBase.GetType().Name)
                        {
                            case "TrackerDataBaseBlock":
                                var tdb = extraDataBase as TrackerDataBaseBlock;
                                  machineName = tdb.MachineId;
                                  macAdd= tdb.MacAddress;
                   
                                break;

                            default:
                                break;
                        }
                    }



                }






                lnkDataTable.Rows.Add(filename, lnkFilePath, targetCreatedOn, targetModifiedOn, targetLastAccessedOn, localPath, 
                                  sourceCreatedOn, sourceLastAccessedOn, 
                                   sourceModifiedOn, fileSize, dataFlag, fileAttribute, driveTypes,volumeInfo,machineName,macAdd);



            }
            gridControl1.DataSource = lnkDataTable;
        }

        private static LnkFile ProcessFile(string lnkFile)
        {
            var lnk = Lnk.Lnk.LoadFile(lnkFile);

            return lnk;

        }

        private static string GetDescriptionFromEnumValue(Enum value)
        {
            var attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute?.Description;
        }


        void AnalyzeShellbags()
        {
            schedulerStorage.Appointments.Clear();
            DirectoryInfo di = new DirectoryInfo("output/");

            if (di.Exists)
            {
                di.Delete(true);
            }
            

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "SBECmd.exe";
            info.Arguments = "-l --csv output";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            Process proc = new Process();

            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();


            DataTable dataTable = new DataTable();

            DirectoryInfo dir = new DirectoryInfo("output/");
            var myFile = dir.GetFiles("*.tsv").OrderByDescending(f => f.LastWriteTime).First();
            //MessageBox.Show(myFile.DirectoryName +@"\"+ myFile.Name);

            StreamReader streamreader = new StreamReader(myFile.DirectoryName + @"\" + myFile.Name);
            char[] delimiter = new char[] { '\t' };
            string[] columnheaders = streamreader.ReadLine().Split(delimiter);
            foreach (string columnheader in columnheaders)
            {
                dataTable.Columns.Add(columnheader); // I've added the column headers here.
            }

            while (streamreader.Peek() > 0)
            {
                DataRow datarow = dataTable.NewRow();
                datarow.ItemArray = streamreader.ReadLine().Split(delimiter);
                dataTable.Rows.Add(datarow);

                string value = datarow[6].ToString();
                string shellType = datarow[5].ToString();
                string absolutePath = datarow[4].ToString();
                //DateTime createdOn = DateTime.Parse(datarow[8].ToString(), null);
                Appointment n=null;
                string description = "";
                if (datarow[8].ToString()!="")
                {
                    DateTime createdOn = DateTime.ParseExact(datarow[8].ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);
                    DateTime modifiedOn = DateTime.ParseExact(datarow[8].ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);
                    DateTime lastAccessedOn = DateTime.ParseExact(datarow[8].ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);

                    n = new AppointmentInstance();
                    n.Subject = value;
                    n.Start = modifiedOn;
                    n.End = modifiedOn;

                    description = "Name: " + value + "\nPath " + absolutePath +
                                         "\nCreated Time" + createdOn +
                                         "\nModified Time: " + modifiedOn + "\nLast Accessed Time: " +
                                         lastAccessedOn;

                    n.Description = description;
                    schedulerStorage.Appointments.Items.Add(n);
                }
                
                //DateTimeOffset modifiedOn = DateTimeOffset.Parse(datarow[9].ToString(), null).DateTime; ;
                //DateTimeOffset lastAccessedOn = DateTimeOffset.Parse(datarow[10].ToString(), null).DateTime; ;

                


            }

            gridControl1.DataSource = dataTable;

            
        }


        void AnalyzeGoogleChrome()
        {

            historyButton.Visible = true;
            downloadButton.Visible = true;
            schedulerControl.Visible = true;
        }

        void AnalyzeFirefox()
        {
            historyButton.Visible = true;
            downloadButton.Visible = true;
            schedulerControl.Visible = true;
        }

        private void historyButton_Click(object sender, EventArgs e)
        {
            schedulerStorage.Appointments.Clear();
            if (this.Text=="Google Chrome")
            {
                string google = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                @"\Google\Chrome\User Data\Default\History";
                SQLiteConnection cn = new SQLiteConnection("Data Source=" + google + ";Version=3; New=false;Compress=True");
                cn.Open();

                SQLiteDataAdapter sd = new SQLiteDataAdapter("SELECT u.id,k.term,u.url,u.title,u.visit_count,DateTime(u.last_visit_time/1000000-11644473600, \"unixepoch\" ) as last_visited FROM keyword_search_terms k ,urls u WHERE u.id=k.url_id", cn);

                DataSet ds = new DataSet();
                sd.Fill(ds);

                foreach (DataTable table in ds.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Appointment n = null;
                        string description = "";
                        foreach (DataColumn column in table.Columns)
                        {
                            
                            var item = row[column];

                            

                            n = new AppointmentInstance();

                            n.Subject = "Chrome History";

                            if (column.Caption == "term")
                            {
                                

                                description += "Term: " + item.ToString();
                            }
                            else if (column.Caption == "title")
                            {
                                description += "\nTitle: " + item.ToString();
                            }
                            /*else if (column.Caption == "url")
                            {
                                description += "\nURL: " + item.ToString();
                            }*/
                            else if(column.Caption == "last_visited")
                            {
                                DateTime lastVisted = DateTime.ParseExact(item.ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);
                                n.Start = lastVisted;
                                n.End = lastVisted;
                                description += "\nVisited Time: " + lastVisted;
                            }
                            

                            
                        }
                        n.Description = description;
                        schedulerStorage.Appointments.Items.Add(n);
                    }
                    break;
                }

                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = ds.Tables[0];
                cn.Close();
            }
            else if (this.Text == "Firefox")
            {
                string firefox = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                 @"\Mozilla\Firefox\Profiles\bugw2jjk.default\places.sqlite";




                SQLiteConnection cn = new SQLiteConnection("Data Source=" + firefox + ";Version=3; New=false;Compress=True");
                cn.Open();

                SQLiteDataAdapter sd = new SQLiteDataAdapter("SELECT id,url,title,visit_count,DateTime(last_visit_date/1000000,\"unixepoch\") as last_visited FROM moz_places", cn);


                DataSet ds = new DataSet();
                sd.Fill(ds);
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = ds.Tables[0];
                cn.Close();
            }
            
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            schedulerStorage.Appointments.Clear();

            if (this.Text=="Google Chrome")
            {
                string google = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                @"\Google\Chrome\User Data\Default\History";
                SQLiteConnection cn = new SQLiteConnection("Data Source=" + google + ";Version=3; New=false;Compress=True");
                cn.Open();

                SQLiteDataAdapter sd = new SQLiteDataAdapter("SELECT id,current_path,target_path,DateTime(start_time/1000000-11644473600,\"unixepoch\") as start_time,received_bytes,total_bytes,DateTime(end_time/1000000-11644473600,\"unixepoch\") as end_time,referrer,site_url,tab_url FROM downloads", cn);

                DataSet ds = new DataSet();
                sd.Fill(ds);

                

                foreach (DataTable table in ds.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Appointment n = null;
                        string description = "";
                        foreach (DataColumn column in table.Columns)
                        {
                            var item = row[column];


                            n = new AppointmentInstance();

                            n.Subject = "Chrome Downloads";

                            if (column.Caption == "total_bytes")
                            {
                                description += "Size: " + item.ToString()+ " bytes";
                            }
                            
                            else if (column.Caption == "start_time")
                            {
                                DateTime startTime = DateTime.ParseExact(item.ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);

                                n.Start = startTime;
                                
                                description += "\nStart Time: " + startTime;
                                

                            }
                            else if (column.Caption == "end_time")
                            {
                                DateTime endTime = DateTime.ParseExact(item.ToString(), "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);
                                n.End = endTime;
                                description += "\nEnd Time: " + endTime;
                                //MessageBox.Show(item.ToString());
                            }
                            else if (column.Caption == "target_path")
                            {
                                description += "\nPath: " + item.ToString();
                                
                                //MessageBox.Show(item.ToString());
                            }
                            else if (column.Caption == "tab_url")
                            {
                                description += "\nURL: " + item.ToString();
                                break;
                            }


                            //n.Description = description;
                            //schedulerStorage.Appointments.Items.Add(n);
                        }
                        
                        n.Description = description;
                        schedulerStorage.Appointments.Items.Add(n);
                        
                    }
                    break;
                }

                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
                gridControl1.DataSource = ds.Tables[0];
                cn.Close();
            }
            else if(this.Text == "Firefox")
            {
               
            }

            
        }
    }


}




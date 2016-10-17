using System;using System.Windows;

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.Win32;
using System.ComponentModel;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Server srv = null;
        private Database db = null;

        private const String strBaseTitle = @"Miscrosoft SQL Server Tools";
        private const String strTitleSplit = @" - ";

        public MainWindow()
        {
            InitializeComponent();

            SetConnectedUI(false);
            SetDBSelectedUI(false);
            SetBackupedUI(false);
        }

        private void SetConnectedUI(bool isEnabled)
        {
            cbDB.IsEnabled = isEnabled;
            btnChooseDB.IsEnabled = isEnabled;

            if (isEnabled)
            {
                this.Title = strBaseTitle + strTitleSplit + @"Connected to <" + srv.Name + @">";
            }
            else
            {
                this.Title = strBaseTitle;
            }
        }

        private void SetDBSelectedUI(bool isEnabled)
        {
            tabBackup.IsEnabled = isEnabled;
            tabGenScript.IsEnabled = isEnabled;

            if (isEnabled)
            {
                this.Title = strBaseTitle + strTitleSplit + @"Connected to <" + db.Name + @"@" + srv.Name + @">";
            }
            else
            {
                this.Title = strBaseTitle;
            }
        }

        private void SetBackupedUI(bool isEnabled)
        {
            tabEncrypt.IsEnabled = isEnabled;

            if (isEnabled)
            {
                this.Title = strBaseTitle + strTitleSplit + @"Connected to <" + db.Name + @"@" + srv.Name + @">";
            }
            else
            {
                this.Title = strBaseTitle;
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerConnection srvConn2 = new ServerConnection(tbServerName.Text);
                srvConn2.LoginSecure = false;
                srvConn2.Login = tbUserName.Text;
                srvConn2.Password = tbPassword.Text;
                srv = new Server(srvConn2);

                foreach (Database itrDB in srv.Databases)
                {
                    cbDB.Items.Add(itrDB);
                }

                SetConnectedUI(true);
                SetDBSelectedUI(false);
                SetBackupedUI(false);
            }
            catch (Exception exp)
            {
                MessageBox.Show(@"Failed to open with error: " + exp.ToString());
            }
        }

        private void btnLoadConnect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Connection Information File|*.connex|All files|*.*";
            openFileDialog.Title = "Load connection informatio from a file";
            openFileDialog.Multiselect = false;

            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                Load_Connex(openFileDialog.FileName);
            }
        }

        private void lblDragToLoadConnect_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Load_Connex(files[0]);
            }
        }

        private void Load_Connex(String fileName)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);

                tbServerName.Text = lines[0];
                tbUserName.Text = lines[1];
                tbPassword.Text = lines[2];
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Failed to open with error: " + exception.ToString());
            }
        }

        private void btnSaveConnect_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Connection Information File|*.connex|All files|*.*";
            saveFileDialog1.Title = "Save connection informatio into a file";

            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName, false))
                    {
                        file.WriteLine(tbServerName.Text);
                        file.WriteLine(tbUserName.Text);
                        file.WriteLine(tbPassword.Text);
                    }
                }
            }
        }

        private void btnChooseDB_Click(object sender, RoutedEventArgs e)
        {
            db = (Database)cbDB.SelectedItem;
            SetDBSelectedUI(true);
        }

        private readonly DBProgramBackgroundWorker worker = new DBProgramBackgroundWorker();

        private void btnDump_Click(object sender, RoutedEventArgs e)
        {

            tabConnex.IsEnabled = false;
            tabBackup.IsEnabled = false;
            tabGenScript.IsEnabled = false;
            tabEncrypt.IsEnabled = false;

            worker.Database = db;

            worker.DBProgramOperation = DumpDBProgram;
            worker.WhenWorkDone = DumpDone;

            worker.FolderSelectTextBox = fsDumpDest;
            worker.StartButton = btnDump;
            worker.ProgressBar = pbDumpProgress;
            worker.Status = lblDumpStatus;

            worker.LogFilePrefix = "dump";

            try
            {
                worker.RunWorkerAsync();
            }catch (Exception exp)
            {
                tabConnex.IsEnabled = true;
                SetConnectedUI(true);
                SetDBSelectedUI(true);
                SetBackupedUI(false);

                MessageBox.Show("Cannot start with error " + exp.Message);
            }
        }

        private String DumpDBProgram(DBProgram dbp)
        {
            if (dbp.IsSystemObject)
            {
                return "Skipped system object";
            } else {

                String filePath;

                filePath = worker.WorkingPath;
                filePath += (dbp.ObjectType == DBProgram.Type.StoredProcedure) ? ("\\stored procedure\\") : ("\\user defined function\\");
                System.IO.Directory.CreateDirectory(filePath);
                filePath += dbp.Name + ".sql";
                
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, false))
                {
                    foreach (string line in dbp.Script())
                        file.WriteLine(line);
                }

                return "Successly processed " + dbp.Name + " without error.";
            }
        }

        private void DumpDone(object sender, RunWorkerCompletedEventArgs e)
        {
            // re-enable other specific controls
            tabConnex.IsEnabled = true;
            SetConnectedUI(true);
            SetDBSelectedUI(true);
            SetBackupedUI(true);
        }

        private void btnGen_Click(object sender, RoutedEventArgs e)
        {

            tabConnex.IsEnabled = false;
            tabBackup.IsEnabled = false;
            tabGenScript.IsEnabled = false;
            tabEncrypt.IsEnabled = false;

            worker.Database = db;

            worker.DBProgramOperation = GenEncryptedDBProgram;
            worker.WhenWorkDone = GenDone;

            worker.FolderSelectTextBox = fsGenScriptDest;
            worker.StartButton = btnGen;
            worker.ProgressBar = pbGenProgress;
            worker.Status = lblGenStatus;

            worker.LogFilePrefix = "gen";

            try
            {
                worker.RunWorkerAsync();
            }
            catch (Exception exp)
            {
                tabConnex.IsEnabled = true;
                SetConnectedUI(true);
                SetDBSelectedUI(true);
                SetBackupedUI(false);

                MessageBox.Show("Cannot start with error " + exp.Message);
            }
        }

        private String GenEncryptedDBProgram(DBProgram dbp)
        {
            if (dbp.IsSystemObject)
            {
                return "Skipped system object";
            }
            else
            {

                String filePath;

                filePath = worker.WorkingPath;
                filePath += (dbp.ObjectType == DBProgram.Type.StoredProcedure) ? ("\\stored procedure\\") : ("\\user defined function\\");
                System.IO.Directory.CreateDirectory(filePath);
                filePath += dbp.Name + ".sql";

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, false))
                {
                    dbp.TextMode = false;
                    dbp.IsEncrypted = true;
                    foreach (string line in dbp.Script())
                        file.WriteLine(line);
                    dbp.IsEncrypted = false;
                    dbp.TextMode = true;
                }

                return "Successly processed " + dbp.Name + " without error.";
            }
        }

        private void GenDone(object sender, RunWorkerCompletedEventArgs e)
        {
            // re-enable other specific controls
            tabConnex.IsEnabled = true;
            SetConnectedUI(true);
            SetDBSelectedUI(true);
            SetBackupedUI(true);
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {

            tabConnex.IsEnabled = false;
            tabBackup.IsEnabled = false;
            tabGenScript.IsEnabled = false;
            tabEncrypt.IsEnabled = false;

            worker.Database = db;

            worker.DBProgramOperation = EncryptDBProgram;
            worker.WhenWorkDone = EncDone;

            worker.FolderSelectTextBox = fsEncryptLogDest;
            worker.StartButton = btnEncrypt;
            worker.ProgressBar = pbEncryptProgress;
            worker.Status = lblEncryptStatus;

            worker.LogFilePrefix = "enc";

            try
            {
                worker.RunWorkerAsync();
            }
            catch (Exception exp)
            {
                tabConnex.IsEnabled = true;
                SetConnectedUI(true);
                SetDBSelectedUI(true);
                SetBackupedUI(false);

                MessageBox.Show("Cannot start with error " + exp.Message);
            }
        }

        private String EncryptDBProgram(DBProgram dbp)
        {
            if (dbp.IsSystemObject)
            {
                return "Skipped system object";
            }
            else
            {
                dbp.TextMode = false;
                dbp.IsEncrypted = true;
                dbp.Alter();

                return "Successly processed " + dbp.Name + " without error.";
            }
        }

        private void EncDone(object sender, RunWorkerCompletedEventArgs e)
        {
            // re-enable other specific controls
            tabConnex.IsEnabled = true;
            SetConnectedUI(true);
            SetDBSelectedUI(false);
            SetBackupedUI(true);
        }
    }
}
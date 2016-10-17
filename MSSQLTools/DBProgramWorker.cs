using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    class DBProgramBackgroundWorker
    {

        private BackgroundWorker worker;

        // Constructors
        public DBProgramBackgroundWorker() : base()
        {
            DBProgramOperation = null;
            WhenWorkDone = null;

            LogFilePrefix = null;

            Database = null;

            ProgressBar = null;
            Status = null;

            worker = new BackgroundWorker();
        }

        public Func<DBProgram, String> DBProgramOperation { get; set; }

        public Action<object, RunWorkerCompletedEventArgs> WhenWorkDone { get; set; }

        public String LogFilePrefix { get; set; }

        public Microsoft.SqlServer.Management.Smo.Database Database { get; set; }

        // UI Elements

        public FolderSelectTextBox FolderSelectTextBox { get; set; }

        public String WorkingPath { get; private set; }

        public Button StartButton { get; set; }

        public ProgressBar ProgressBar { get; set; }

        public Label Status { get; set; }

        public void RunWorkerAsync()
        {
            if (DBProgramOperation == null || LogFilePrefix == null || Database == null )
            {
                throw new ArgumentException();
            }
            else if (!this.FolderSelectTextBox.IsFileNameValidate)
            {
                throw new DirectoryNotFoundException();
            }
            else
            {
                this.FolderSelectTextBox.IsEnabled = false;
                this.StartButton.IsEnabled = false;

                this.ProgressBar.Minimum = 0;
                this.ProgressBar.Maximum = 100;

                this.Status.Content = "starting...";

                this.WorkingPath = FolderSelectTextBox.FileName;

                worker.DoWork += worker_DoWork;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.ProgressChanged += worker_HandleProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted2;
                worker.WorkerReportsProgress = true;

                worker.RunWorkerAsync();
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            // initialize log writers
            System.IO.StreamWriter log = new System.IO.StreamWriter(this.WorkingPath + "\\" + this.LogFilePrefix + DateTime.Now.ToString(".yyyyMMdd.HHmms") + ".log", false);
            System.IO.StreamWriter err = new System.IO.StreamWriter(this.WorkingPath + "\\" + this.LogFilePrefix + DateTime.Now.ToString(".yyyyMMdd.HHmms") + ".err", false);

            // load all sp and udf here
            List<DBProgram> programs = new List<DBProgram>();

            try
            {
                foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure sp in Database.StoredProcedures)
                    programs.Add(new MSSQLTools.StoredProcedure(sp));

                log.WriteLine("Successfully loaded " + Database.StoredProcedures.Count + " stored procedures.");

                foreach (Microsoft.SqlServer.Management.Smo.UserDefinedFunction udf in Database.UserDefinedFunctions)
                    programs.Add(new MSSQLTools.UserDefinedFunction(udf));

                log.WriteLine("Successfully loaded " + Database.UserDefinedFunctions.Count + " user defined functions.");

                log.WriteLine(programs.Count + " db programs will be processed.");
            }
            catch (Exception exp)
            {
                err.WriteLine("Failed to load db programs with error:");
                err.WriteLine(exp.Message);

                log.WriteLine("Failed to load db programs with error.");

                err.Close();
                log.Close();
                return;
            }

            // foreach loop here

            int i = 0;

            foreach (DBProgram dbp in programs)
            {
                try
                {
                    log.WriteLine(DBProgramOperation(dbp));
                }
                catch (Exception exp)
                {
                    log.WriteLine("Failed to process: " + dbp.Name + "with error.");
                    err.WriteLine("Failed to process: " + dbp.Name + "with error:");
                    err.WriteLine(exp.Message);
                }

                worker.ReportProgress( ++i * 100 / programs.Count, dbp.Name );
            }

            err.Close();
            log.Close();
        }

        private void worker_HandleProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressBar.Value = e.ProgressPercentage;
            this.Status.Content = e.UserState;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.ProgressBar.Value = 100;
            this.Status.Content = "Finished";

            this.StartButton.IsEnabled = true;
            this.FolderSelectTextBox.IsEnabled = true;
        }

        private void worker_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
        {
            WhenWorkDone(sender, e);
        }
    }
}

﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TaskManagment.Models;


namespace TaskManagment.Forms
{
    public partial class ManagerHome : Form
    {
        List<Worker> manager;
        List<Worker> workerToSelect;
        List<Worker> workerToAdd;
        Panel currentPanel;
        public ManagerHome()
        {
            InitializeComponent();
            AddProject();
            currentPanel = pnl_add_project;
            currentPanel.Visible = true;
        }

        private  void onlyNumbers(object sender, KeyPressEventArgs e)
        {
            Global.onlyNumbers(sender, e);

        }

        public void getAllWorkers()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Global.path);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync($"getAllWorkers").Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                workerList = JsonConvert.DeserializeObject<List<Worker>>(result);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);

            }
        }
       
        #region addProject

        public void AddProject()
        {

            btn_addProject.Enabled = false;
            data_start.MinDate = DateTime.Now;
            data_end.MinDate = data_start.Value;
            data_start.CustomFormat = "yyyy-MM-dd";
            data_start.CustomFormat = "yyyy-MM-dd";
            manager = Global.GetManagers();
            manager.ForEach(t => { txt_team_name.Items.Add(t.Name); });
            getAllWorkers();
        }
      
        public void checkProjectValidation(object sender, EventArgs e)
        {
               
            btn_addProject.Enabled =
                 Global.checkVaidationLength(2, 25, txt_projName) &&
                 Global.checkVaidationLength(2, 15, txt_customer_name) &&
                 Global.checkVaidationNumber(0, 0, txt_developer_hours) &&
                 Global.checkVaidationNumber(0, 0, txt_QI_houers) &&
                 Global.checkVaidationNumber(0, 0, txt_UIUX_hours);

        }

        private void data_start_ValueChanged(object sender, EventArgs e)
        {

            data_end.MinDate = (sender as DateTimePicker).Value;
        }
        private void txt_team_name_SelectedIndexChanged(object sender, EventArgs e)
        {

            getAllWorkers();
            int id = manager.Where(m => m.Name == txt_team_name.SelectedItem.ToString()).FirstOrDefault().Id;
            workerToSelect = new List<Worker>();
            workerToAdd = new List<Worker>();
            workerList.ForEach(w =>
            {
                if (w.ManagerId != null && w.JobId > 2 && w.ManagerId != id)
                    workerToSelect.Add(w);
            });
            dgvAddWorkers.DataSource = workerToSelect;
            dgvAddWorkers.Columns["Id"].Visible = false;
        }

        private void dgvAddWorkers_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvAddWorkers.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.Beige)
            {
                dgvAddWorkers.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.DarkGray;

                workerToAdd.Remove(workerToSelect[e.RowIndex]);

            }
            else
            {
                dgvAddWorkers.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Beige;
                workerToAdd.Add(workerToSelect[e.RowIndex]);
            }

        }

        public void addWorkersToProject(string name)
        {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"{Global.path}addWorkersToProject/{name}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            dynamic credential;

            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    credential = workerToAdd.Select(w => w.Id);
                    string credentialString = Newtonsoft.Json.JsonConvert.SerializeObject(credential, Formatting.None);
                    streamWriter.Write(credentialString);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("Can not add a project");
            }
        }

        private void btn_addProject_Click(object sender, EventArgs e)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Global.path + "addProject");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            dynamic credential;
            int id = manager.Where(m => m.Name == txt_team_name.SelectedItem.ToString()).FirstOrDefault().Id;

            Project proj = new Project()
            {
                Name = txt_projName.Text,
                Customer = txt_customer_name.Text,
                TeamLeaderId = id,
                QAHours = int.Parse(txt_QI_houers.Text),
                DevelopHours = int.Parse(txt_developer_hours.Text),
                UiUxHours = int.Parse(txt_UIUX_hours.Text),
                StartDate = data_start.Value.Date,
                EndDate = data_end.Value.Date
            };
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    credential = proj;
                    string credentialString = Newtonsoft.Json.JsonConvert.SerializeObject(credential, Formatting.None);
                    streamWriter.Write(credentialString);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();


                    MessageBox.Show($"The project {proj.Name} added successfully");
                    addWorkersToProject(proj.Name);

                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("Can not add a project");
            }
        }
        #endregion

        #region UserManagment

        List<Worker> workerList;
        HttpClient httpClient = new HttpClient();
        void GetAllWorker()
        {
            panelControlls.Controls.Clear();
            panelControlls.BorderStyle = BorderStyle.FixedSingle;
            ComboBox cb = new ComboBox() { Name = "cbWorkers" };
            cb.Items.AddRange(workerList.Select(w => w.UserName).ToArray());
            cb.Location = new Point(250, 50);
            panelControlls.Controls.Add(cb);
        }
        private void btn_delete_Click(string s)
        {
            GetAllWorker();
            Label l = new Label() { Text = $"Chooose worker to {s}:" };
            Label lbl_close = new Label() { Text = "X" };
            Button btn_delete_edit = new Button() { Text = s };
            l.Location = new Point(50, 50);
            lbl_close.Location = new Point(30, 30);
            btn_delete_edit.Location = new Point(150, 90);
            panelControlls.Controls.Add(l);
            panelControlls.Controls.Add(lbl_close);
            panelControlls.Controls.Add(btn_delete_edit);
            btn_delete_edit.Click += new EventHandler(btn_delete_edit_Click);
            lbl_close.Click += new EventHandler(lbl_close_click);
        }


        void lbl_close_click(object sender, EventArgs e)
        {
            panelControlls.Controls.Clear();
            panelControlls.BorderStyle = BorderStyle.None;
        }

        void btn_delete_edit_Click(object sender, EventArgs e)
        {
            Worker w1 = new Worker();
            int id = workerList.FirstOrDefault(w => w.UserName == (panelControlls.Controls["cbWorkers"] as ComboBox).Text).Id;
            w1 = workerList.FirstOrDefault(w => w.Id == id);
            if ((sender as Button).Text == "delete")
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(Global.path);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.DeleteAsync($"deleteWorker/{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                    workerList.Remove(w1);
                    (panelControlls.Controls["cbWorkers"] as ComboBox).Items.Clear();
                    (panelControlls.Controls["cbWorkers"] as ComboBox).Text = "";
                    (panelControlls.Controls["cbWorkers"] as ComboBox).Items.AddRange(workerList.Select(w => w.UserName).ToArray());



                }
            }
            else
            {
                lbl_close_click(sender, e);
                ShowPanel(pnl_add_worker);
                WorkerDeatails(false, w1);
            }

        }



        #endregion


        #region WorkerDeatails

        bool isAdd;
        public Worker w;
        public void WorkerDeatails(bool isAdd, Worker w)
        {
            txt_password.PasswordChar = '*';
            cmb_job.DataSource = Global.jobs.Select(j => j.Name).ToList();
            manager = Global.GetManagers();
            cmb_manager.Items.Clear();
            cmb_manager.Items.AddRange(manager.Select(m => m.Name).ToArray());
            this.isAdd = isAdd;
            lblTitle.Text = isAdd ? "add worker" : "edit worker";
            btn_Action.Text = isAdd ? "Add" : "Edit ";
            this.w = w;
            if (!isAdd)
            {
                fillWorkerDeatails();
                btn_Action.Enabled = true;
            }
            else
            {
                btn_Action.Enabled = false;
                foreach (Control c in pnl_add_worker.Controls)
                {
                    if (c is TextBox)
                    {
                        c.Text = "";
                    }
                }
            }

        }

        private void fillWorkerDeatails()
        {
            txt_name.Text = w.Name;
            txt_email.Text = w.EMail;
            lblPassword.Text = "";
            txt_password.Visible = false;
            txt_user_name.Text = w.UserName;
            cmb_job.SelectedItem = Global.jobs.Find(j => j.Id == w.JobId).Name;
            cmb_manager.SelectedItem = manager.FirstOrDefault(m => m.Id == w.ManagerId).Name;

        }
        public void checkWorkerValidation(object sender, EventArgs e)
        {
            btn_Action.Enabled =Global.checkVaidationLength(2, 15, txt_name) &&
                Global.checkVaidationLength(2, 10, txt_user_name) &&
                (isAdd ? Global.checkVaidationLength(6, 10, txt_password) : true) &&
                Global.checkVaidationLength(2, 30, txt_email) &&
                Global.checkValidEmail(txt_email);
        
        }

        private void btn_add_Click(object sender, EventArgs e)
        {

            int IdJob = Global.jobs.Find(j => j.Name == cmb_job.SelectedValue.ToString()).Id;
            int IdManager = manager.FirstOrDefault(m => m.Name == cmb_manager.Text).Id; //SelectedIndex

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Global.path + (isAdd ? "addWorker" : "updateWorker"));
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = isAdd ? "POST" : "PUT";


            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{" + (!isAdd ? "\"Id\":\"" + w.Id + "\"," : "") + "\"Name\":\"" + txt_name.Text + "\"," +
                   "\"UserName\":\"" + txt_user_name.Text + "\"," +
                   "\"Password\":\"" + (txt_password.Text != "" ?Global.sha256(txt_password.Text) : "") + "\"," +
                    "\"JobId\":\"" + IdJob + "\"," +
                   "\"EMail\":\"" + txt_email.Text + "\"," +
                   "\"ManagerId\":\"" + IdManager +
                   "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    var result = streamReader.ReadToEnd();

                    MessageBox.Show($"The worker {txt_name.Text} {(isAdd ? "added" : "changed")} successfully");
                    //tab_manager.Controls.Remove(tab_workerDeatrails);
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Can not {(isAdd ? "add" : "change")} a worker");
            }
        }

       

        #endregion

        #region reports
        List<dynamic> presences;
        public void GetPresences(int i)
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Global.path);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync($"getPresence").Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                presences = JsonConvert.DeserializeObject<List<dynamic>>(result);
                switch (i)
                {
                    case 1:
                        {
                            SelectByWorkerName();
                            lbl_report.Text = "report by worker";
                            break;
                        }
                    case 2:
                        {
                            SelectByProjectName();
                            lbl_report.Text = "report by project";

                            break;
                        }
                    case 3:
                        {
                            ShowPresences();
                            lbl_report.Text = "presences report";

                            break;
                        }
                }
                 

            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
        }

        private void ShowPresences()
        {
            dgv_presence.DataSource = presences;
            treeView1.Visible = false;
            dgv_presence.Visible = true;
        }

        private void SelectByWorkerName()
        {
            List<dynamic> projectsByName = new List<dynamic>();
            var names = presences.Select(p => p["WorkerName"].Value).GroupBy(p => p).ToArray();

            foreach (var n in names)
            {
                List<dynamic> projectsHours = new List<dynamic>();
                var projects = presences.FindAll(p => p["WorkerName"].Value == n.Key).Select(p => p["ProjectName"].Value).GroupBy(p => p).ToArray();
                foreach (var pro in projects)
                {
                    var hours = presences.FindAll(p => p["ProjectName"].Value == pro.Key && p["WorkerName"].Value == n.Key).Select(p => new
                    {
                        Date = p["Date"],
                        Start = p["Start"],
                        End = p["End"]
                    });
                    projectsHours.Add(new { pro.Key, hours });
                }
                projectsByName.Add(new { n.Key, projectsHours });
            }
            treeView1.Nodes.Clear();
            treeView1.BorderStyle = BorderStyle.None;
            foreach (var pbn in projectsByName)
            {

                TreeNode n = treeView1.Nodes.Add(pbn.Key);
                n.BackColor = Color.BurlyWood;
                foreach (var prh in pbn.projectsHours)
                {
                    TreeNode n1 = n.Nodes.Add(prh.Key);
                    n1.BackColor = Color.Coral;
                    foreach (var hour in prh.hours)
                    {
                        TreeNode n3 = n1.Nodes.Add($"date:{hour.Date.Value}, start:{hour.Start.Value}, end:{hour.End.Value}");
                        n3.BackColor = Color.Cyan;
                    }
                }
            }
            treeView1.Visible = true;
            dgv_presence.Visible = false;
        }
        private void SelectByProjectName()
        {

            List<dynamic> projectsByName = new List<dynamic>();
            var projects = presences.Select(p => p["ProjectName"].Value).GroupBy(p => p).ToArray();

            foreach (var n in projects)
            {
                List<dynamic> projectsHours = new List<dynamic>();
                var names = presences.FindAll(p => p["ProjectName"].Value == n.Key).Select(p => p["WorkerName"].Value).GroupBy(p => p).ToArray();
                foreach (var pro in names)
                {
                    var hours = presences.FindAll(p => p["WorkerName"].Value == pro.Key && p["ProjectName"].Value == n.Key).Select(p => new
                    {
                        Date = p["Date"],
                        Start = p["Start"],
                        End = p["End"]
                    });
                    projectsHours.Add(new { pro.Key, hours });
                }
                projectsByName.Add(new { n.Key, projectsHours });
            }
            treeView1.Nodes.Clear();
            treeView1.BorderStyle = BorderStyle.None;
            foreach (var pbn in projectsByName)
            {

                TreeNode n = treeView1.Nodes.Add(pbn.Key);
                n.BackColor = Color.BurlyWood;
                foreach (var prh in pbn.projectsHours)
                {
                    TreeNode n1 = n.Nodes.Add(prh.Key);
                    n1.BackColor = Color.Coral;
                    foreach (var hour in prh.hours)
                    {
                        TreeNode n3 = n1.Nodes.Add($"date:{hour.Date.Value}, start:{hour.Start.Value}, end:{hour.End.Value}");
                        n3.BackColor = Color.Cyan;


                    }
                }

            }
            treeView1.Visible = true;
            dgv_presence.Visible = false;


        }
        private void btnExportToExecl_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet workSheet = excel.ActiveSheet;
            try
            {
                workSheet.Cells[1, "A"] = "WorkerName";
                workSheet.Cells[1, "B"] = "ProjectName";
                workSheet.Cells[1, "C"] = "Date";
                workSheet.Cells[1, "D"] = "Start";
                workSheet.Cells[1, "E"] = "End";
                int row = 2;
                foreach (var car in presences)
                {
                    workSheet.Cells[row, "A"] = car["WorkerName"].Value;
                    workSheet.Cells[row, "B"] = car["ProjectName"].Value;
                    workSheet.Cells[row, "C"] = car["Date"].Value;
                    workSheet.Cells[row, "D"] = car["Start"].Value;
                    workSheet.Cells[row, "E"] = car["End"].Value;
                    row++;
                }
                workSheet.Range["A1"].AutoFormat(Microsoft.Office.Interop.Excel.XlRangeAutoFormat.xlRangeAutoFormatClassic1);
                string fileName = string.Format(@"{0}\Presences.xlsx", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                workSheet.SaveAs(fileName);
                MessageBox.Show(string.Format("The file '{0}' is saved successfully!", fileName));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception",
                    "There was a PROBLEM saving Excel file!\n" + exception.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                excel.Quit();
                if (excel != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                if (workSheet != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workSheet);
                excel = null;
                workSheet = null;
                GC.Collect();
            }

        }


        #endregion

        private void ShowPanel(Panel p)
        {
            currentPanel.Visible = false;
           p.Visible = true;
            currentPanel = p;

        }
        private void addProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(pnl_add_project);
        }

        private void byWorkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetPresences(1);
            ShowPanel(pnl_report);

        }

        private void byProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetPresences(2);
            ShowPanel(pnl_report);
        }
        private void preToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetPresences(3);
            ShowPanel(pnl_report);
        }
      

        private void updateWorkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getAllWorkers();
            ShowPanel(pnl_delete);
            btn_delete_Click("edit");
        }

        private void deleteWorkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getAllWorkers();
            ShowPanel(pnl_delete);
            btn_delete_Click("delete");
        }

        private void addWorkerToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ShowPanel(pnl_add_worker);
            WorkerDeatails(true, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.LogOut();
        }

        
    }
}

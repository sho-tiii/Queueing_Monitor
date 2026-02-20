using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Queueing_Monitor
{
    public partial class QueueMonitor : Form
    {
        // 1. Setup Database Connection & Background Timer
        string connectionString = "server=localhost;user=root;password=;database=triage_system";
        Timer dbTimer = new Timer();

        public QueueMonitor()
        {
            InitializeComponent();
        }

        private void QueueMonitor_Load(object sender, EventArgs e)
        {
            // --- VIDEO SETUP ---
            axWindowsMediaPlayer1.URL = @"C:\Users\xx\Downloads\Healthcare commercial video -Hospital promotional advertisement video Best video production company.mp4";
            axWindowsMediaPlayer1.uiMode = "none";
            axWindowsMediaPlayer1.settings.setMode("loop", true);
            axWindowsMediaPlayer1.settings.mute = true;
            axWindowsMediaPlayer1.stretchToFit = true;

            // --- DATABASE POLLING SETUP ---
            // Tell the timer to tick every 3000 milliseconds (3 seconds)
            dbTimer.Interval = 3000;
            dbTimer.Tick += DbTimer_Tick;
            dbTimer.Start();

            // Force it to grab data the exact moment the screen opens
            FetchLatestQueueData();
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        // 2. This triggers every 3 seconds
        private void DbTimer_Tick(object sender, EventArgs e)
        {
            FetchLatestQueueData();
        }

        // 3. The Magic Method: Pulls the newest 9 patients from the database
        private void FetchLatestQueueData()
        {
            // 1. Only grab people who are Calling or Waiting. 
            // 2. Put the 'Calling' person at the very top of the results.
            // 3. Sort everyone else by their ID so the oldest waiting patient is next.
            string query = @"SELECT patient_id, status 
                     FROM patient_registration 
                     WHERE status IN ('Calling', 'Waiting') 
                     ORDER BY FIELD(status, 'Calling', 'Waiting'), patient_id ASC 
                     LIMIT 9";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> waitingPatients = new List<string>();
                            string nowServing = "---";

                            while (reader.Read())
                            {
                                long rawId = Convert.ToInt64(reader["patient_id"]);
                                string status = reader["status"].ToString();

                                // Format the ID just like before
                                string formattedId = "N-" + rawId.ToString("D3");

                                // Separate the Calling patient from the Waiting patients
                                if (status == "Calling")
                                {
                                    nowServing = formattedId;
                                }
                                else if (status == "Waiting")
                                {
                                    waitingPatients.Add(formattedId);
                                }
                            }

                            // 4. Update the UI
                            lblNowServing.Text = nowServing;

                            // Update the 8 "Next in Line" sections safely from the waiting list
                            if (waitingPatients.Count > 0) lblNext1.Text = waitingPatients[0]; else lblNext1.Text = "---";
                            if (waitingPatients.Count > 1) lblNext2.Text = waitingPatients[1]; else lblNext2.Text = "---";
                            if (waitingPatients.Count > 2) lblNext3.Text = waitingPatients[2]; else lblNext3.Text = "---";
                            if (waitingPatients.Count > 3) lblNext4.Text = waitingPatients[3]; else lblNext4.Text = "---";
                            if (waitingPatients.Count > 4) lblNext5.Text = waitingPatients[4]; else lblNext5.Text = "---";
                            if (waitingPatients.Count > 5) lblNext6.Text = waitingPatients[5]; else lblNext6.Text = "---";
                            if (waitingPatients.Count > 6) lblNext7.Text = waitingPatients[6]; else lblNext7.Text = "---";
                            if (waitingPatients.Count > 7) lblNext8.Text = waitingPatients[7]; else lblNext8.Text = "---";
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silent fail so XAMPP connection drops don't freeze the screen
            }
        }

        // Empty events left exactly as you had them
        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}
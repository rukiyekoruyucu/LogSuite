using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace d
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort1 = new SerialPort();
        private string buffer = "";
        string connectionString = "Server=77.245.158.98,18954;Database=DoorDB;User Id=oztasstaj;Password=**ozt2025;Encrypt=True;TrustServerCertificate=True;";

        // KartID bazýnda son eriþim zamaný ve iþlem (true = giriþ, false = çýkýþ)
        private Dictionary<string, DateTime> lastAccessTimes = new Dictionary<string, DateTime>();
        private Dictionary<string, bool> lastOperations = new Dictionary<string, bool>();

        public Form1()
        {
            InitializeComponent();
            label1.Text = "";
            label2.Text = "";
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Veritabanýndan kapý listesini çek
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT DoorID, DoorName FROM Doors", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        comboBox1.DisplayMember = "DoorName";
                        comboBox1.ValueMember = "DoorID";
                        comboBox1.DataSource = dt;
                        comboBox1.SelectedIndex = 0;
                    }
                    else
                    {
                        label2.Text = "Kapý listesi boþ.";
                    }
                }

                // Serial port ayarlarý
                serialPort1.PortName = "COM3";  // Arduino'nun baðlý olduðu port
                serialPort1.BaudRate = 9600;    // Arduino'nun baud hýzý ile eþleþmeli
                serialPort1.DataReceived += SerialPort1_DataReceived;
                serialPort1.Open();
            }
            catch (Exception ex)
            {
                label2.Text = "Hata: " + ex.Message;
            }
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort1.ReadExisting();
                foreach (char c in data)
                {
                    if (c == '\r' || c == '\n')
                    {
                        string cardID = buffer.Trim();
                        buffer = "";
                        if (!string.IsNullOrEmpty(cardID))
                            this.Invoke(new Action(() => KartOkutmaIslemi(cardID)));
                    }
                    else
                    {
                        buffer += c;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(async () =>
                {
                    await ShowTemporaryMessage("Serial port okuma hatasý: " + ex.Message, Color.Red);
                }));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                string cardID = buffer.Trim();
                buffer = "";
                KartOkutmaIslemi(cardID);
                return true;
            }
            else
            {
                buffer += Convert.ToChar(keyData);
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private async void KartOkutmaIslemi(string cardID)
        {
            if (string.IsNullOrEmpty(cardID))
            {
                if (serialPort1.IsOpen)
                    serialPort1.Write("1"); // Hata kodu
                return; // Mesaj yok
            }

            if (comboBox1.SelectedValue == null)
            {
                if (serialPort1.IsOpen)
                    serialPort1.Write("1"); // Hata kodu
                return; // Mesaj yok
            }

            int selectedDoorID = Convert.ToInt32(comboBox1.SelectedValue);
            string doorName = comboBox1.Text; // Kapý ismini combobox'tan alýyoruz
            DateTime now = DateTime.Now;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Kullanýcý bilgilerini al (UserID, IsActive, UserFullName)
                    string userQuery = "SELECT UserID, IsActive, FirstName, LastName FROM Users WHERE CardID = @CardID";
                    int userID;
                    bool isActive;
                    string userFullName;

                    using (SqlCommand cmdUser = new SqlCommand(userQuery, conn))
                    {
                        cmdUser.Parameters.Add("@CardID", System.Data.SqlDbType.NVarChar, 50).Value = cardID;
                        using (SqlDataReader reader = await cmdUser.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                if (serialPort1.IsOpen)
                                    serialPort1.Write("1");
                                return;
                            }

                            userID = reader.GetInt32(reader.GetOrdinal("UserID"));
                            isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));

                            string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                            string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                            userFullName = firstName + " " + lastName;
                        }
                    }

                    // Yetki kontrolü
                    string accessQuery = "SELECT AccessGranted FROM UsersDoors WHERE UserID = @UserID AND DoorID = @DoorID";
                    using (SqlCommand cmdAccess = new SqlCommand(accessQuery, conn))
                    {
                        cmdAccess.Parameters.Add("@UserID", System.Data.SqlDbType.Int).Value = userID;
                        cmdAccess.Parameters.Add("@DoorID", System.Data.SqlDbType.Int).Value = selectedDoorID;
                        object accessObj = await cmdAccess.ExecuteScalarAsync();

                        if (accessObj == null || !Convert.ToBoolean(accessObj))
                        {
                            if (serialPort1.IsOpen)
                                serialPort1.Write("1");
                            await LogKaydet(userID, cardID, true, selectedDoorID, true, userFullName, doorName);
                            return;
                        }
                    }

                    bool isEntry = true; // Baþlangýçta giriþ varsayalým

                    if (lastAccessTimes.ContainsKey(cardID))
                    {
                        TimeSpan diff = now - lastAccessTimes[cardID];
                        if (diff.TotalSeconds < 10)
                        {
                            // 10 saniye içinde tekrar basýlmýþ, hata olarak logla, mesaj yok
                            if (serialPort1.IsOpen)
                                serialPort1.Write("1");
                            await LogKaydet(userID, cardID, true, selectedDoorID, lastOperations[cardID], userFullName, doorName);
                            await ShowTemporaryMessage("Üst üste iþlem.", Color.Red);
                            return;
                        }
                        // Son iþlem giriþse, bu çýkýþ olacak, deðilse giriþ
                        isEntry = !lastOperations[cardID];
                    }

                    // Baþarýlý iþlem log ve mesaj
                    if (serialPort1.IsOpen)
                        serialPort1.Write("0");

                    string message = isEntry ? "Giriþ baþarýlý" : "Çýkýþ baþarýlý";
                    await ShowTemporaryMessage(message, Color.Green);

                    await LogKaydet(userID, cardID, false, selectedDoorID, isEntry, userFullName, doorName);

                    // Son iþlem ve zamaný güncelle
                    lastAccessTimes[cardID] = now;
                    lastOperations[cardID] = isEntry;
                }
            }
            catch (Exception ex)
            {
                if (serialPort1.IsOpen)
                    serialPort1.Write("1");
                await ShowTemporaryMessage("Hata: " + ex.Message, Color.Red);
            }
        }

        private async Task LogKaydet(int userID, string cardID, bool errorStatus, int doorID, bool operation, string userFullName, string doorName)
        {
            string logInsertQuery = @"INSERT INTO Logs (UserID, CardID, ErrorStatus, DoorID, Operation, UserFullName, DoorName) 
                                    VALUES (@UserID, @CardID, @ErrorStatus, @DoorID, @Operation, @UserFullName, @DoorName)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(logInsertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@CardID", cardID);
                        cmd.Parameters.AddWithValue("@ErrorStatus", errorStatus);
                        cmd.Parameters.AddWithValue("@DoorID", doorID);
                        cmd.Parameters.AddWithValue("@Operation", operation);
                        cmd.Parameters.AddWithValue("@UserFullName", userFullName);
                        cmd.Parameters.AddWithValue("@DoorName", doorName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log kaydetme hatasý: " + ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();

            base.OnFormClosing(e);
        }

        private async Task ShowTemporaryMessage(string message, Color color)
        {
            label1.ForeColor = color;
            label1.Text = message;
            await Task.Delay(1000);
            label1.Text = "";
        }
    }
}

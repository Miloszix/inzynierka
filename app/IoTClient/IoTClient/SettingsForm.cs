using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace IoTClient
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadSensors();
            LoadGateways();
        }

        private HttpClient Client()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
            return c;
        }

        // ------------------------------------------------------------
        // LOAD SENSOR LIST
        // ------------------------------------------------------------
        private async void LoadSensors()
        {
            try
            {
                using var client = Client();
                var sensors = await client.GetFromJsonAsync<List<Sensor>>("http://3.70.126.6:1880/sensors");

                dataSensors.DataSource = sensors;
                foreach (DataGridViewColumn col in dataSensors.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sensor load error: " + ex.Message);
            }
        }

        // ------------------------------------------------------------
        // LOAD GATEWAYS LIST
        // ------------------------------------------------------------
        private async void LoadGateways()
        {
            try
            {
                using var client = Client();
                var gateways = await client.GetFromJsonAsync<List<UserGateway>>(
                    "http://3.70.126.6:1880/user/gateways");

                dataGateways.DataSource = gateways;
                foreach (DataGridViewColumn col in dataGateways.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway load error: " + ex.Message);
            }
        }

        // ------------------------------------------------------------
        // DELETE GATEWAY FROM USER
        // ------------------------------------------------------------
        private async void btnDeleteGateway_Click(object sender, EventArgs e)
        {
            if (dataGateways.SelectedRows.Count == 0)
                return;

            var gw = dataGateways.SelectedRows[0].DataBoundItem as UserGateway;

            if (gw == null)
                return;

            var confirm = MessageBox.Show($"Remove gateway {gw.gateway_id}?",
                                          "Confirm", MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    gateway_id = gw.gateway_id
                };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/user/remove_gateway", payload);

                if (res.IsSuccessStatusCode)
                {
                    LoadGateways();
                }
                else
                    MessageBox.Show("Gateway remove failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway delete error: " + ex.Message);
            }
        }

        // ------------------------------------------------------------
        // ADD GATEWAY → open AddGatewayForm
        // ------------------------------------------------------------
        private void btnAddGateway_Click(object sender, EventArgs e)
        {
            var addForm = new AddGatewayForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadGateways();
            }
        }

        // ------------------------------------------------------------
        // EDIT SENSOR NAME
        // ------------------------------------------------------------
        private async void btnEditSensor_Click(object sender, EventArgs e)
        {
            if (dataSensors.SelectedRows.Count == 0)
                return;

            var sensor = dataSensors.SelectedRows[0].DataBoundItem as Sensor;
            if (sensor == null)
                return;

            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "New sensor name:", "Rename Sensor", sensor.name);

            if (string.IsNullOrWhiteSpace(newName))
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    new_name = newName
                };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/rename_sensor", payload);

                if (res.IsSuccessStatusCode)
                    LoadSensors();
                else
                    MessageBox.Show("Rename failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rename sensor error: " + ex.Message);
            }
        }
    }

    // MODELE DO TABEL

    public class UserGateway
    {
        public string gateway_id { get; set; }
        public string name { get; set; }
        public string added_at { get; set; }
    }
}

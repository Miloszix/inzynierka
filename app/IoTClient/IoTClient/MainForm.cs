using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace IoTClient
{
    public partial class MainForm : Form
    {
        private string gatewayId;

        public MainForm(string gatewayId)
        {
            InitializeComponent();
            this.gatewayId = gatewayId;

            lblGateway.Text = "Gateway: " + gatewayId;

            LoadSensors();
        }

        private async void LoadSensors()
        {

            try
            {
                using var client = CreateClient();

                var sensors = await client.GetFromJsonAsync<List<Sensor>>("http://3.70.126.6:1880/sensors");

                dataGridSensors.DataSource = sensors;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sensors: " + ex.Message);
            }
        }

        private async void LoadMeasurements(string sensorMac)
        {
            MessageBox.Show("LoadMeasurements called with MAC: " + sensorMac);

            try
            {
                using var client = CreateClient();

                string url = $"http://3.70.126.6:1880/measurements?sensor_mac={sensorMac}";
                var measurements = await client.GetFromJsonAsync<List<Measurement>>(url);

                dataGridMeasurements.DataSource = measurements;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading measurements: " + ex.Message);
            }
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

            return client;
        }

        private void dataGridSensors_SelectionChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Clicked sensor");

            if (dataGridSensors.SelectedRows.Count > 0)
            {
                var sensor = dataGridSensors.SelectedRows[0].DataBoundItem as Sensor;
                if (sensor != null)
                {
                    txtSensorMac.Text = sensor.sensor_mac;
                    txtSensorName.Text = sensor.name;

                    LoadMeasurements(sensor.sensor_mac);
                }
            }
        }

        private async void btnAccept_Click(object sender, EventArgs e)
        {
            if (txtSensorMac.Text == "") return;

            var payload = new
            {
                gateway_id = gatewayId,
                sensor_mac = txtSensorMac.Text,
                name = txtSensorName.Text
            };

            try
            {
                using var client = CreateClient();
                var response = await client.PostAsJsonAsync("http://3.70.126.6:1880/accept_sensor", payload);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sensor accepted.");
                    LoadSensors();
                }
                else
                {
                    MessageBox.Show("Error: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Accept failed: " + ex.Message);
            }
        }

        private async void btnRename_Click(object sender, EventArgs e)
        {
            if (txtSensorMac.Text == "") return;

            var payload = new
            {
                gateway_id = gatewayId,
                sensor_mac = txtSensorMac.Text,
                new_name = txtSensorName.Text
            };

            try
            {
                using var client = CreateClient();
                var response = await client.PostAsJsonAsync("http://3.70.126.6:1880/rename_sensor", payload);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sensor renamed.");
                    LoadSensors();
                }
                else
                {
                    MessageBox.Show("Error: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rename failed: " + ex.Message);
            }
        }

        private void dataGridViewMeasurements_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

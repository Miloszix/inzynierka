using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace IoTClient
{
    public partial class SettingsForm : Form
    {
        public string? LastAddedGatewayId { get; private set; }
        public List<UserGateway> UpdatedGateways { get; private set; } = new();
        public bool GatewaysChanged { get; private set; } = false;
        public SettingsForm()
        {
            InitializeComponent();

            dataGateways.SelectionChanged += dataGateways_SelectionChanged;

            LoadGateways();
        }
        private HttpClient Client()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
            return c;
        }

        private async void LoadSensors(string gatewayId)
        {
            try
            {
                using var client = Client();

                var sensors = await client.GetFromJsonAsync<List<Sensor>>(
                    $"http://3.70.126.6:1880/sensors?gateway_id={gatewayId}");

                dataSensors.DataSource = sensors;

                foreach (DataGridViewColumn col in dataSensors.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sensor load error: " + ex.Message);
            }
        }

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

                if (gateways.Count > 0)
                {
                    dataGateways.Rows[0].Selected = true;
                    LoadSensors(gateways[0].gateway_id);
                }

                UpdatedGateways = gateways;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway load error: " + ex.Message);
            }
        }


        private void dataGateways_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGateways.SelectedRows.Count == 0)
                return;

            if (dataGateways.SelectedRows[0].DataBoundItem is UserGateway gw)
            {
                LoadSensors(gw.gateway_id);
            }
        }

        private async void btnDeleteGateway_Click(object sender, EventArgs e)
        {
            if (dataGateways.SelectedRows.Count == 0)
                return;

            var gw = dataGateways.SelectedRows[0].DataBoundItem as UserGateway;
            if (gw == null)
                return;

            var confirm = MessageBox.Show(
                $"Remove gateway {gw.gateway_id}?",
                "Confirm", MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using var client = Client();

                var payload = new { gateway_id = gw.gateway_id };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/user/remove_gateway", payload);

                if (res.IsSuccessStatusCode)
                {
                    GatewaysChanged = true;
                    LoadGateways();
                }
                else
                {
                    MessageBox.Show("Gateway remove failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway delete error: " + ex.Message);
            }
        }

        private void btnAddGateway_Click(object sender, EventArgs e)
        {
            var addForm = new AddGatewayForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                GatewaysChanged = true;
                LastAddedGatewayId = addForm.AddedGateway?.gateway_id;
                LoadGateways();
            }
        }

        private async void btnEditSensor_Click(object sender, EventArgs e)
        {
            if (dataSensors.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a sensor first.");
                return;
            }

            if (dataSensors.SelectedRows[0].DataBoundItem is not Sensor sensor)
            {
                MessageBox.Show("Invalid sensor data.");
                return;
            }

            // otwieramy okno RenameForm
            var f = new RenameForm(sensor.sensor_mac!, sensor.name ?? "");

            if (f.ShowDialog() != DialogResult.OK)
                return;

            string newName = f.NewName;
            if (string.IsNullOrWhiteSpace(newName))
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = sensor.gateway_id,
                    new_name = newName
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/rename_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    // przeładuj sensory wybranego gatewaya
                    if (dataGateways.SelectedRows.Count > 0 &&
                        dataGateways.SelectedRows[0].DataBoundItem is UserGateway gw)
                    {
                        LoadSensors(gw.gateway_id);
                    }
                }
                else
                {
                    MessageBox.Show("Rename failed: " + res.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rename sensor error: " + ex.Message);
            }
        }
        private async void btnAcceptSensor_Click(object sender, EventArgs e)
        {
            if (dataSensors.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a sensor first.");
                return;
            }

            var sensor = dataSensors.SelectedRows[0].DataBoundItem as Sensor;
            if (sensor == null)
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = sensor.gateway_id
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/accept_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    if (dataGateways.SelectedRows.Count > 0 &&
                        dataGateways.SelectedRows[0].DataBoundItem is UserGateway gw)
                    {
                        LoadSensors(gw.gateway_id);
                    }
                }
                else
                    MessageBox.Show("Accept failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Accept error: " + ex.Message);
            }
        }

        private async void btnIgnoreSensor_Click(object sender, EventArgs e)
        {
            if (dataSensors.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a sensor first.");
                return;
            }

            var sensor = dataSensors.SelectedRows[0].DataBoundItem as Sensor;
            if (sensor == null)
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = sensor.gateway_id
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/ignore_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    if (dataGateways.SelectedRows.Count > 0 &&
                        dataGateways.SelectedRows[0].DataBoundItem is UserGateway gw)
                    {
                        LoadSensors(gw.gateway_id);
                    }
                }
                else
                    MessageBox.Show("Ignore failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ignore error: " + ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (GatewaysChanged)
                DialogResult = DialogResult.OK;
        }

        private async void btnRenameGateway_Click(object sender, EventArgs e)
        {
            if (dataGateways.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select gateway first.");
                return;
            }

            if (dataGateways.SelectedRows[0].DataBoundItem is not UserGateway gw)
                return;

            var f = new RenameForm(gw.gateway_id, gw.name);
            if (f.ShowDialog() != DialogResult.OK)
                return;

            string newName = f.NewName;
            if (string.IsNullOrWhiteSpace(newName))
                return;

            try
            {
                using var client = Client();

                var payload = new
                {
                    gateway_id = gw.gateway_id,
                    new_name = newName
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/user/rename_gateway",
                    payload
                );

                if (res.IsSuccessStatusCode)
                {
                    GatewaysChanged = true;
                    LoadGateways();
                }
                else
                {
                    MessageBox.Show("Rename failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rename error: " + ex.Message);
            }
        }

    }

    public class UserGateway
    {
        public string gateway_id { get; set; }
        public string name { get; set; }
        public string added_at { get; set; }
    }
}

using ScottPlot;
using ScottPlot.WinForms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;

namespace IoTClient
{
    public partial class MainForm : Form
    {
        private readonly string gatewayId;

        public MainForm(string gatewayId)
        {
            InitializeComponent();
            this.gatewayId = gatewayId;

            lblGateway.Text = $"Gateway: {gatewayId}";

            LoadSensors();
            //LoadPendingSensors();

            PositionLastUpdateLabel();
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Session.Token);
            return client;
        }

        private async void LoadSensors()
        {
            try
            {
                using var client = CreateClient();

                // Pobierz wszystkie sensory (accepted + pending)
                var allSensors = await client.GetFromJsonAsync<List<Sensor>>(
                    "http://3.70.126.6:1880/sensors");

                // Accepted → główna tabelka
                var sensors = allSensors
                    .Where(s => s.status == "accepted")
                    .ToList();

                // Pending → osobna tabelka
                var pending = allSensors
                    .Where(s => s.status == "pending")
                    .ToList();

                dataGridPending.DataSource = pending;

                // Build visible accepted table
                var table = new List<SensorLatest>();

                foreach (var s in sensors)
                {
                    string url = $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(s.sensor_mac)}";
                    var meas = await client.GetFromJsonAsync<List<Measurement>>(url);

                    var latest = meas?
                        .Where(m => !string.IsNullOrWhiteSpace(m.timestamp))
                        .OrderByDescending(m => m.timestamp)
                        .FirstOrDefault();

                    table.Add(new SensorLatest
                    {
                        name = s.name,
                        sensor_mac = s.sensor_mac,
                        temperature = latest?.temperature ?? 0,
                        humidity = latest?.humidity ?? 0,
                        pressure = latest?.pressure ?? 0,
                        timestamp = latest?.timestamp != null
                            ? DateTime.Parse(latest.timestamp).ToString("yyyy-MM-dd HH:mm:ss")
                            : "-"
                    });
                }

                dataGridSensors.DataSource = table;

                // HYBRYDA Fill + AllCells
                dataGridSensors.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                foreach (DataGridViewColumn col in dataGridSensors.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sensor load error: {ex.Message}");
            }
        }
        private async void BtnAccept_Click(object sender, EventArgs e)
        {
            if (dataGridPending.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            var sensor = dataGridPending.SelectedRows[0].DataBoundItem as Sensor;

            if (sensor == null || string.IsNullOrWhiteSpace(sensor.sensor_mac))
            {
                MessageBox.Show("Brak sensor_mac – popraw klasę Sensor!");
                return;
            }
            
            // MessageBox.Show("MAC wysyłany: " + sensor.sensor_mac); // debug

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac
                };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/accept_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    LoadSensors();
                    // MessageBox.Show("Sensor zaakceptowany.");
                }
                else
                {
                    MessageBox.Show("Błąd podczas akceptacji.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Accept error: " + ex.Message);
            }
        }


        private async void BtnIgnore_Click(object sender, EventArgs e)
        {
            if (dataGridPending.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            var sensor = dataGridPending.SelectedRows[0].DataBoundItem as Sensor;
            if (sensor == null)
                return;

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/ignore_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    LoadSensors();
                    //MessageBox.Show("Sensor usunięty.");
                }
                else
                {
                    MessageBox.Show("Błąd podczas ignorowania.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ignore error: " + ex.Message);
            }
        }

        private void dataGridSensors_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridSensors.SelectedRows.Count == 0)
                return;

            var row = dataGridSensors.SelectedRows[0].DataBoundItem as SensorLatest;
            if (row != null)
            {
                DrawCharts(row.sensor_mac);
            }
        }

        private void ApplyDarkStyle(ScottPlot.Plot plt)
        {
            plt.FigureBackground.Color = ScottPlot.Colors.Black;
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");

            plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
            plt.Grid.MinorLineColor = ScottPlot.Color.FromARGB(0);

            plt.Axes.Color(ScottPlot.Colors.White);
            plt.Axes.Title.Label.ForeColor = ScottPlot.Colors.White;
            plt.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
            plt.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.White;

            plt.Legend.BackgroundColor = ScottPlot.Color.FromHex("#222222");
            plt.Legend.OutlineColor = ScottPlot.Colors.Gray;
            plt.Legend.FontColor = ScottPlot.Colors.White;
        }

        // ==============================
        // MAIN DRAW FUNCTION
        // ==============================
        private async void DrawCharts(string mac)
        {
            try
            {
                using var client = CreateClient();

                string url = $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(mac)}";
                var m = await client.GetFromJsonAsync<List<Measurement>>(url);

                if (m == null || m.Count == 0)
                {
                    ClearAllPlots();
                    return;
                }

                var ordered = m
                    .Where(x => DateTime.TryParse(x.timestamp, out _))
                    .OrderBy(x => DateTime.Parse(x.timestamp))
                    .ToList();

                DateTime[] dts = ordered
                    .Select(x => DateTime.Parse(x.timestamp))
                    .ToArray();

                double[] xs = dts
                    .Select(dt => dt.ToOADate())
                    .ToArray();

                double[] temps = ordered.Select(x => x.temperature).ToArray();
                double[] hums = ordered.Select(x => x.humidity).ToArray();
                double[] press = ordered.Select(x => x.pressure).ToArray();

                // TEMPERATURE
                var tplt = formsPlotTemp.Plot;
                tplt.Clear();
                var tline = tplt.Add.Scatter(dts, temps);
                tline.LegendText = "Temperature (°C)";
                tplt.Legend.IsVisible = true;
                tplt.Axes.AutoScale();
                tplt.Axes.DateTimeTicksBottom();

                // HUMIDITY
                var hplt = formsPlotHum.Plot;
                hplt.Clear();
                var hline = hplt.Add.Scatter(dts, hums);
                hline.LegendText = "Humidity (%)";
                hplt.Legend.IsVisible = true;
                hplt.Axes.AutoScale();
                hplt.Axes.DateTimeTicksBottom();

                // PRESSURE
                var pplt = formsPlotPress.Plot;
                pplt.Clear();
                var pline = pplt.Add.Scatter(dts, press);
                pline.LegendText = "Pressure (hPa)";
                pplt.Legend.IsVisible = true;
                pplt.Axes.AutoScale();
                pplt.Axes.DateTimeTicksBottom();

                ApplyDarkStyle(tplt);
                ApplyDarkStyle(hplt);
                ApplyDarkStyle(pplt);

                formsPlotTemp.Refresh();
                formsPlotHum.Refresh();
                formsPlotPress.Refresh();

                lblLastUpload.Text = "Last update: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                PositionLastUpdateLabel();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Plot error: " + ex.Message);
            }

        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            //var f = new SettingsForm();
            //f.ShowDialog();
        }

        private void BtnTable_Click(object sender, EventArgs e)
        {
            //var f = new AllMeasurementsForm();
            //f.Show();
        }

        private void ClearAllPlots()
        {
            formsPlotTemp.Plot.Clear();
            formsPlotHum.Plot.Clear();
            formsPlotPress.Plot.Clear();

            formsPlotTemp.Refresh();
            formsPlotHum.Refresh();
            formsPlotPress.Refresh();
        }

        private void PositionLastUpdateLabel()
        {
            // upewnij się, że etykieta policzy poprawnie rozmiar tekstu
            lblLastUpload.AutoSize = true;
            lblLastUpload.Refresh();

            // padding od prawej krawędzi panelu (zmień jeśli chcesz inną odległość)
            int rightPadding = 12;

            // ustaw X tak, aby etykieta była wyrównana do prawej wewnątrz panelTop
            int newLeft = panelTop.ClientSize.Width - lblLastUpload.Width - rightPadding;
            if (newLeft < 10) newLeft = 10; // minimalna lewa margines, zapobiega wyjściu poza okno

            lblLastUpload.Left = newLeft;

            // wyśrodkuj w pionie w panelTop
            lblLastUpload.Top = Math.Max((panelTop.ClientSize.Height - lblLastUpload.Height) / 2, 2);
        }

        private void lblLastUpload_Click(object sender, EventArgs e)
        {

        }
    }

    public class Sensor
    {
        public int id { get; set; }                // ← OBOWIĄZKOWE
        public string? gateway_id { get; set; }
        public string? sensor_mac { get; set; }    // ← WAŻNE!
        public string? name { get; set; }
        public string? status { get; set; }
        public string? timestamp { get; set; }
    }

    public class Measurement
    {
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public string? timestamp { get; set; }
    }

    public class SensorLatest
    {
        public string? name { get; set; }
        public string? sensor_mac { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public string? timestamp { get; set; }
    }
}
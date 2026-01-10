using ScottPlot;
using ScottPlot.WinForms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;

namespace IoTClient
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer autoRefreshTimer;

        private bool suppressSelectionChanged = false;

        private TimeZoneInfo localTimeZone;

        private DateTime lastTimestamp = DateTime.MinValue;
        private List<Measurement> chartHistory = new();

        private void StyleGrid(DataGridView g)
        {
            g.BackgroundColor = System.Drawing.Color.FromArgb(32, 32, 32);
            g.EnableHeadersVisualStyles = false;

            g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            g.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;

            g.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
            g.DefaultCellStyle.ForeColor = System.Drawing.Color.White;

            g.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(70, 70, 70);
            g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;

            g.RowHeadersVisible = false;
        }
        public MainForm()
        {
            InitializeComponent();

            this.FormClosed += MainForm_FormClosed;

            localTimeZone = TimeZoneInfo.Local;

            tabControlCharts.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlCharts.Appearance = TabAppearance.Normal;
            tabControlCharts.SizeMode = TabSizeMode.Fixed;

            tabControlCharts.GetType().GetProperty("UserPaint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(tabControlCharts, true);

            ApplyDarkTheme();
            StyleGrid(dataGridSensors);
            StyleGrid(dataGridPending);

            panelTop.Resize += (s, e) => PositionLastUpdateLabel();
            PositionLastUpdateLabel();

            comboGateway.SelectedIndexChanged += comboGateway_SelectedIndexChanged;

            _ = LoadGateways(); // async start

            autoRefreshTimer = new System.Windows.Forms.Timer();
            autoRefreshTimer.Interval = 5000;
            autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            autoRefreshTimer.Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private DateTime UtcToLocal(string utcTimestamp)
        {
            if (!DateTime.TryParse(
                utcTimestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var utc))
            {
                return DateTime.MinValue;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utc, localTimeZone);
        }
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Session.Token);
            return client;
        }

        private async Task LoadGateways()
        {
            try
            {
                using var client = CreateClient();

                var gateways = Session.Gateways;

                comboGateway.Items.Clear();
                comboGateway.DisplayMember = "name";
                comboGateway.ValueMember = "gateway_id";

                foreach (var g in gateways)
                    comboGateway.Items.Add(g);

                if (gateways.Count > 0)
                {
                    comboGateway.SelectedIndex = 0;

                    Session.GatewayId = gateways[0].gateway_id;

                    LoadSensors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway load error: " + ex.Message);
            }
        }

        private void comboGateway_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboGateway.SelectedItem is GatewayItem gw)
            {
                Session.GatewayId = gw.gateway_id;

                LoadSensors();  // load sensors for selected gateway
            }
        }

        private async void LoadSensors()
        {
            try
            {
                suppressSelectionChanged = true;

                if (string.IsNullOrWhiteSpace(Session.GatewayId))
                {
                    MessageBox.Show("No gateway selected.");
                    suppressSelectionChanged = false;
                    return;
                }

                string? selectedMac = null;
                if (dataGridSensors.SelectedRows.Count > 0 &&
                    dataGridSensors.SelectedRows[0].DataBoundItem is SensorLatest sel)
                {
                    selectedMac = sel.sensor_mac;
                }

                using var client = CreateClient();

                string endpoint = $"http://3.70.126.6:1880/sensors?gateway_id={Session.GatewayId}";
                var allSensors = await client.GetFromJsonAsync<List<Sensor>>(endpoint)
                                ?? new List<Sensor>();

                var sensors = allSensors.Where(s => s.status == "accepted").ToList();
                var pending = allSensors.Where(s => s.status == "pending").ToList();

                dataGridPending.DataSource = pending;

                var table = new List<SensorLatest>();

                foreach (var s in sensors)
                {
                    string endpointMeas =
                        $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(s.sensor_mac)}&gateway_id={Session.GatewayId}";

                    var meas = await client.GetFromJsonAsync<List<Measurement>>(endpointMeas);

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
                            ? UtcToLocal(latest.timestamp).ToString("yyyy-MM-dd HH:mm:ss")
                            : "-"

                    });
                }

                dataGridSensors.DataSource = table;

                dataGridSensors.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                foreach (DataGridViewColumn col in dataGridSensors.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                if (!string.IsNullOrWhiteSpace(selectedMac))
                {
                    foreach (DataGridViewRow row in dataGridSensors.Rows)
                    {
                        if (row.DataBoundItem is SensorLatest sl &&
                            sl.sensor_mac == selectedMac)
                        {
                            row.Selected = true;
                            break;
                        }
                    }
                }

                suppressSelectionChanged = false;
            }
            catch (Exception ex)
            {
                suppressSelectionChanged = false;
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
            if (sensor == null)
            {
                MessageBox.Show("Błąd – brak danych sensora.");
                return;
            }

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = Session.GatewayId
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/accept_sensor", payload);

                if (res.IsSuccessStatusCode)
                    LoadSensors();
                else
                    MessageBox.Show("Błąd akceptacji sensora.");
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

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = Session.GatewayId
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/ignore_sensor", payload);

                if (res.IsSuccessStatusCode)
                    LoadSensors();
                else
                    MessageBox.Show("Błąd ignorowania sensora.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ignore error: " + ex.Message);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            var sf = new SettingsForm();
            if (sf.ShowDialog() != DialogResult.OK)
                return;

            if (!sf.GatewaysChanged)
                return;

            Session.Gateways.Clear();
            foreach (var g in sf.UpdatedGateways)
            {
                Session.Gateways.Add(new GatewayItem
                {
                    gateway_id = g.gateway_id,
                    name = g.name
                });
            }

            ReloadGatewayCombo(sf.LastAddedGatewayId);
        }

        private void ReloadGatewayCombo(string? selectGatewayId = null)
        {
            comboGateway.Items.Clear();

            foreach (var g in Session.Gateways)
                comboGateway.Items.Add(g);

            if (Session.Gateways.Count == 0)
                return;

            int index = 0;

            if (!string.IsNullOrWhiteSpace(selectGatewayId))
            {
                index = Session.Gateways.FindIndex(g => g.gateway_id == selectGatewayId);
                if (index < 0)
                    index = 0;
            }

            comboGateway.SelectedIndex = index;
            Session.GatewayId = Session.Gateways[index].gateway_id;
            LoadSensors();
        }

        private void dataGridSensors_SelectionChanged(object sender, EventArgs e)
        {
            if (suppressSelectionChanged)
                return;

            if (dataGridSensors.SelectedRows.Count == 0)
                return;

            if (dataGridSensors.SelectedRows[0].DataBoundItem is SensorLatest row)
                DrawCharts(row.sensor_mac);
        }

        private async void DrawCharts(string mac)
        {
            try
            {
                using var client = CreateClient();

                var m = await client.GetFromJsonAsync<List<Measurement>>(
                    $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(mac)}&gateway_id={Session.GatewayId}");

                if (m == null || m.Count == 0)
                {
                    ClearAllPlots();
                    chartHistory.Clear();
                    lastTimestamp = DateTime.MinValue;
                    return;
                }

                chartHistory = m
                    .Where(x => !string.IsNullOrWhiteSpace(x.timestamp))
                    .OrderBy(x => DateTime.Parse(
                        x.timestamp,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal))
                    .ToList();

                lastTimestamp = DateTime.Parse(chartHistory.Last().timestamp);

                DateTime[] dts = chartHistory
                    .Select(x => UtcToLocal(x.timestamp))
                    .ToArray();
                double[] temps = chartHistory.Select(x => x.temperature).ToArray();
                double[] hums = chartHistory.Select(x => x.humidity).ToArray();
                double[] press = chartHistory.Select(x => x.pressure).ToArray();


                DateTime end = dts.Last();
                DateTime start = end.AddHours(-24);
                double xMin = start.ToOADate();
                double xMax = end.ToOADate();

                var tplt = formsPlotTemp.Plot;
                tplt.Clear();
                tplt.Add.Scatter(dts, temps).LegendText = "Temperature (°C)";
                tplt.Legend.IsVisible = true;
                tplt.Axes.AutoScaleY();              // Y auto
                tplt.Axes.DateTimeTicksBottom();
                tplt.Axes.SetLimitsX(xMin, xMax);

                var hplt = formsPlotHum.Plot;
                hplt.Clear();
                hplt.Add.Scatter(dts, hums).LegendText = "Humidity (%)";
                hplt.Legend.IsVisible = true;
                hplt.Axes.AutoScaleY();
                hplt.Axes.DateTimeTicksBottom();
                hplt.Axes.SetLimitsX(xMin, xMax);

                var pplt = formsPlotPress.Plot;
                pplt.Clear();
                pplt.Add.Scatter(dts, press).LegendText = "Pressure (hPa)";
                pplt.Legend.IsVisible = true;
                pplt.Axes.AutoScaleY();
                pplt.Axes.DateTimeTicksBottom();
                pplt.Axes.SetLimitsX(xMin, xMax);

                ApplyDarkStyle(tplt);
                ApplyDarkStyle(hplt);
                ApplyDarkStyle(pplt);

                formsPlotTemp.Refresh();
                formsPlotHum.Refresh();
                formsPlotPress.Refresh();

                lblLastUpload.Text =
                    "Last update: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                PositionLastUpdateLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Plot error: " + ex.Message);
            }
        }

        private int refreshCounter = 0;
        private async void AutoRefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Session.GatewayId))
                return;
            refreshCounter++;

            if (refreshCounter % 3 == 0)
            {
                LoadSensors();
                return;
            }

            await SharedIncrementalUpdate();
        }

        private async Task SharedIncrementalUpdate()
        {
            if (dataGridSensors.DataSource is not List<SensorLatest> sensors)
                return;

            using var client = CreateClient();

            SensorLatest? selected =
                dataGridSensors.SelectedRows.Count > 0
                ? dataGridSensors.SelectedRows[0].DataBoundItem as SensorLatest
                : null;

            Dictionary<string, List<Measurement>> newDataForSensor = new();

            foreach (var sensor in sensors)
            {
                if (string.IsNullOrWhiteSpace(sensor.sensor_mac)) continue;
                string lastTs =
                    chartHistory.Count > 0
                    ? chartHistory.Last().timestamp   // UTC from measurements
                    : null;

                if (lastTs == null)
                    continue;
                string mac = sensor.sensor_mac;

                string url =
                    $"http://3.70.126.6:1880/measurements_since?sensor_mac={Uri.EscapeDataString(mac)}" +
                    $"&gateway_id={Session.GatewayId}&after={Uri.EscapeDataString(lastTs)}";


                var res = await client.GetFromJsonAsync<List<Measurement>>(url) ?? new List<Measurement>();

                newDataForSensor[mac] = res;

                if (res.Count > 0)
                {
                    var newest = res
                        .Where(x => !string.IsNullOrWhiteSpace(x.timestamp))
                        .OrderByDescending(x => x.timestamp)
                        .First();

                    sensor.temperature = newest.temperature;
                    sensor.humidity = newest.humidity;
                    sensor.pressure = newest.pressure;
                    sensor.timestamp = UtcToLocal(newest.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            dataGridSensors.Refresh();

            if (selected != null)
            {
                string mac = selected.sensor_mac!;
                if (newDataForSensor.TryGetValue(mac, out var newMeas) && newMeas != null && newMeas.Count > 0)
                {
                    UpdateChartFromCache(newMeas);
                }
            }
        }


        private void UpdateChartFromCache(List<Measurement> newData)
        {
            foreach (var m in newData)
                chartHistory.Add(m);

            chartHistory = chartHistory
                .Where(x => DateTime.TryParse(x.timestamp, out _))
                .OrderBy(x => DateTime.Parse(x.timestamp))
                .ToList();

            lastTimestamp = DateTime.Parse(chartHistory.Last().timestamp);

            DateTime[] dts = chartHistory
                .Select(x => UtcToLocal(x.timestamp))
                .ToArray();

            double[] temps = chartHistory.Select(x => x.temperature).ToArray();
            double[] hums = chartHistory.Select(x => x.humidity).ToArray();
            double[] press = chartHistory.Select(x => x.pressure).ToArray();

            var tplt = formsPlotTemp.Plot;
            tplt.Clear();
            tplt.Add.Scatter(dts, temps).LegendText = "Temperature (°C)";

            var hplt = formsPlotHum.Plot;
            hplt.Clear();
            hplt.Add.Scatter(dts, hums).LegendText = "Humidity (%)";

            var pplt = formsPlotPress.Plot;
            pplt.Clear();
            pplt.Add.Scatter(dts, press).LegendText = "Pressure (hPa)";

            ApplyDarkStyle(tplt);
            ApplyDarkStyle(hplt);
            ApplyDarkStyle(pplt);

            formsPlotTemp.Refresh();
            formsPlotHum.Refresh();
            formsPlotPress.Refresh();

            lblLastUpload.Text = "Last update: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PositionLastUpdateLabel();
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
            lblLastUpload.AutoSize = true;
            lblLastUpload.Refresh();

            int rightPadding = 12;

            lblLastUpload.Left =
                panelTop.ClientSize.Width - lblLastUpload.Width - rightPadding;

            lblLastUpload.Top =
                Math.Max((panelTop.ClientSize.Height - lblLastUpload.Height) / 2, 2);
        }

        private void ApplyDarkStyle(ScottPlot.Plot plt)
        {
            plt.FigureBackground.Color = ScottPlot.Colors.Black;
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");

            plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
            plt.Grid.MinorLineColor = ScottPlot.Color.FromARGB(0);

            plt.Axes.Color(ScottPlot.Colors.White);

            plt.Legend.BackgroundColor = ScottPlot.Color.FromHex("#222222");
            plt.Legend.FontColor = ScottPlot.Colors.White;
        }

        private async void btn_rename_click(object sender, EventArgs e)
        {
            if (dataGridSensors.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            if (dataGridSensors.SelectedRows[0].DataBoundItem is not SensorLatest sensor)
            {
                MessageBox.Show("Błąd – brak danych sensora.");
                return;
            }

            var f = new RenameForm(sensor.sensor_mac!, sensor.name ?? "");
            if (f.ShowDialog() != DialogResult.OK)
                return;

            string newName = f.NewName;

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = Session.GatewayId,
                    new_name = newName
                };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/rename_sensor", payload);

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Nazwa zmieniona.");
                    LoadSensors();
                }
                else
                {
                    MessageBox.Show("Błąd zapisu nazwy: " + res.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd komunikacji: " + ex.Message);
            }
        }

        private void leftLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupPending_Enter(object sender, EventArgs e)
        {

        }

        private void ApplyDarkTheme()
        {
            System.Drawing.Color bg = System.Drawing.Color.FromArgb(30, 30, 30);
            System.Drawing.Color panel = System.Drawing.Color.FromArgb(38, 38, 38);
            System.Drawing.Color group = System.Drawing.Color.FromArgb(46, 46, 46);
            System.Drawing.Color text = System.Drawing.Color.White;

            // ─── FORM ───────────────────────────────────────────
            this.BackColor = bg;

            // ─── PANEL TOP ──────────────────────────────────────
            panelTop.BackColor = panel;
            lblLastUpload.ForeColor = System.Drawing.Color.Lime;
            comboGateway.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            comboGateway.ForeColor = text;
            comboGateway.FlatStyle = FlatStyle.Flat;

            // ─── LEFT PANELS ─────────────────────────────────────
            splitMain.Panel1.BackColor = bg;
            splitMain.Panel2.BackColor = bg;

            leftLayout.BackColor = bg;

            // ─── GROUPBOXES ──────────────────────────────────────
            StyleGroupBox(groupSensors);
            StyleGroupBox(groupPending);
            StyleGroupBox(groupCharts);

            // ─── BUTTONS ─────────────────────────────────────────
            StyleButton(btnSettings);
            StyleButton(btnTable);
            StyleButton(btn_rename);
            StyleButton(btnAccept);
            StyleButton(btnIgnore);

            // ─── TABS ────────────────────────────────────────────
            StyleTabs(tabControlCharts);

            // ─── STYLE GRIDS (z Twojego kodu) ────────────────────
            StyleGrid(dataGridSensors);
            StyleGrid(dataGridPending);
        }

        private void StyleGroupBox(GroupBox gb)
        {
            gb.ForeColor = System.Drawing.Color.White;
            gb.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
        }

        private void StyleButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(70, 70, 70);
            b.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(90, 90, 90);

            b.BackColor = System.Drawing.Color.FromArgb(55, 55, 55);
            b.ForeColor = System.Drawing.Color.White;

            // zaokrąglenie
            b.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, b.Width, b.Height, 10, 10)
            );

            b.Resize += (s, e) =>
            {
                b.Region = System.Drawing.Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, b.Width, b.Height, 10, 10)
                );
            };
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        private void StyleTabs(TabControl tabs)
        {
            tabs.Appearance = TabAppearance.Normal;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.ItemSize = new Size(120, 30);

            tabs.DrawItem += (s, e) =>
            {
                TabPage page = tabs.TabPages[e.Index];
                bool selected = (e.Index == tabs.SelectedIndex);

                System.Drawing.Color bg = selected ? System.Drawing.Color.FromArgb(55, 55, 55) : System.Drawing.Color.FromArgb(40, 40, 40);
                System.Drawing.Color fg = System.Drawing.Color.White;

                using (SolidBrush br = new SolidBrush(bg))
                    e.Graphics.FillRectangle(br, e.Bounds);

                TextRenderer.DrawText(e.Graphics, page.Text, tabs.Font, e.Bounds, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            tabs.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);

            foreach (TabPage tp in tabs.TabPages)
            {
                tp.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            }
        }

        private async void btn_measurments_clicked(object sender, EventArgs e)
        {
            if (dataGridSensors.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            if (dataGridSensors.SelectedRows[0].DataBoundItem is not SensorLatest sensor)
            {
                MessageBox.Show("Brak danych sensora.");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

            try
            {
                string url =
                    $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(sensor.sensor_mac!)}" +
                    $"&gateway_id={Session.GatewayId}";

                var meas = await client.GetFromJsonAsync<List<Measurement>>(url);

                if (meas == null || meas.Count == 0)
                {
                    MessageBox.Show("Brak pomiarów dla tego sensora.");
                    return;
                }

                var sorted = meas
                    .Where(m => DateTime.TryParse(m.timestamp, out _))
                    .OrderByDescending(m => DateTime.Parse(m.timestamp))
                    .ToList();
                var f = new MeasurementsForm(sorted, sensor.name ?? sensor.sensor_mac);
                f.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Measurement load error: " + ex.Message);
            }
        }
    }
}

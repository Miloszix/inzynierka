using System;
using System.Windows.Forms;

namespace IoTClient
{
    public partial class RenameForm : Form
    {
        public string NewName { get; private set; } = "";

        public RenameForm(string sensorMac, string currentName)
        {
            InitializeComponent();

            lblInfo.Text = $"Nowa nazwa dla sensora:\n{sensorMac}";
            txtName.Text = currentName;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            NewName = txtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("Nazwa nie może być pusta.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

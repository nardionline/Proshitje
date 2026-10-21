using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ProShitje13
{
    public sealed class MainForm : Form
    {
        private readonly TextBox txtDb = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtSearch = new TextBox();
        private readonly ComboBox cmbRole = new ComboBox();
        private readonly Button btnConnect = new Button();
        private readonly Button btnSearch = new Button();
        private readonly Button btnAdd = new Button();
        private readonly Button btnRemove = new Button();
        private readonly Button btnExport = new Button();
        private readonly DataGridView gridProducts = new DataGridView();
        private readonly DataGridView gridSale = new DataGridView();
        private readonly Label lblStatus = new Label();
        private readonly DataTable saleItems = new DataTable();
        private OleDbConnection connection;

        public MainForm()
        {
            Text = "ProShitje 1.3 - Prototip";
            Width = 1120;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);

            var top = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
            Controls.Add(top);

            top.Controls.Add(new Label { Text = "Databaza MDB:", Left = 10, Top = 12, Width = 100 });
            txtDb.SetBounds(115, 8, 440, 25);
            txtDb.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "marketdb.mdb");
            top.Controls.Add(txtDb);

            top.Controls.Add(new Label { Text = "Fjalëkalimi:", Left = 570, Top = 12, Width = 85 });
            txtPassword.SetBounds(655, 8, 130, 25);
            txtPassword.UseSystemPasswordChar = true;
            top.Controls.Add(txtPassword);

            top.Controls.Add(new Label { Text = "Roli:", Left = 800, Top = 12, Width = 35 });
            cmbRole.SetBounds(835, 8, 150, 25);
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Items.AddRange(new object[] { "Administrator", "Shitës" });
            cmbRole.SelectedIndex = 0;
            top.Controls.Add(cmbRole);

            btnConnect.Text = "Lidhu me MDB";
            btnConnect.SetBounds(10, 48, 130, 30);
            btnConnect.Click += (s, e) => ConnectDatabase();
            top.Controls.Add(btnConnect);

            btnSearch.Text = "Kërko";
            btnSearch.SetBounds(155, 48, 90, 30);
            btnSearch.Click += (s, e) => SearchProducts();
            top.Controls.Add(btnSearch);

            txtSearch.SetBounds(255, 50, 300, 25);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchProducts(); };
            top.Controls.Add(txtSearch);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 620 };
            Controls.Add(split);
            split.BringToFront();

            var left = new Panel { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(left);
            left.Controls.Add(new Label { Text = "Artikujt", Dock = DockStyle.Top, Height = 28, Font = new Font(Font, FontStyle.Bold) });
            gridProducts.Dock = DockStyle.Fill;
            gridProducts.ReadOnly = true;
            gridProducts.AllowUserToAddRows = false;
            gridProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProducts.MultiSelect = false;
            gridProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            left.Controls.Add(gridProducts);

            var right = new Panel { Dock = DockStyle.Fill };
            split.Panel2.Controls.Add(right);
            right.Controls.Add(new Label { Text = "Fatura provizore (test)", Dock = DockStyle.Top, Height = 28, Font = new Font(Font, FontStyle.Bold) });
            gridSale.Dock = DockStyle.Fill;
            gridSale.ReadOnly = true;
            gridSale.AllowUserToAddRows = false;
            gridSale.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            right.Controls.Add(gridSale);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };
            btnAdd.Text = "Shto artikull";
            btnAdd.Click += (s, e) => AddSelectedProduct();
            btnRemove.Text = "Hiq nga fatura";
            btnRemove.Click += (s, e) => RemoveSelectedSaleItem();
            btnExport.Text = "Gjenero AED TXT";
            btnExport.Click += (s, e) => ExportAed();
            bottom.Controls.AddRange(new Control[] { btnAdd, btnRemove, btnExport });
            right.Controls.Add(bottom);

            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 25;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(8, 0, 0, 0);
            Controls.Add(lblStatus);
            lblStatus.BringToFront();

            saleItems.Columns.Add("Artikulli", typeof(string));
            saleItems.Columns.Add("Sasia", typeof(decimal));
            saleItems.Columns.Add("Cmimi", typeof(decimal));
            saleItems.Columns.Add("DPT", typeof(int));
            gridSale.DataSource = saleItems;
            txtPassword.Text = "";
            lblStatus.Text = "Prototip: përdor kopje të MDB-së. Shitjet nuk ruhen ende në databazë.";
        }

        private void ConnectDatabase()
        {
            try
            {
                if (connection != null) connection.Dispose();
                string dbPath = txtDb.Text.Trim();
                if (!File.Exists(dbPath))
                {
                    MessageBox.Show("Nuk u gjet MDB: " + dbPath, "ProShitje 1.3");
                    return;
                }

                string cs = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbPath + ";";
                if (!string.IsNullOrEmpty(txtPassword.Text))
                    cs += "Jet OLEDB:Database Password=" + txtPassword.Text + ";";

                connection = new OleDbConnection(cs);
                connection.Open();
                lblStatus.Text = "Lidhja me MDB u hap me sukses.";
                SearchProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lidhja dështoi:\r\n" + ex.Message +
                    "\r\nKontrollo Jet 4.0 32-bit, rrugën e MDB-së dhe fjalëkalimin.",
                    "ProShitje 1.3 - Gabim");
            }
        }

        private void SearchProducts()
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                MessageBox.Show("Lidhu fillimisht me databazën.");
                return;
            }

            try
            {
                string term = txtSearch.Text.Trim();
                using (var cmd = new OleDbCommand(
                    "SELECT TOP 500 [Id], [Kodi], [Barcodi], [Artikulli], [Cmimi i daljes], [Cmimihyrjes], [Sasia Stock] " +
                    "FROM [Artikulli] WHERE [Artikulli] LIKE ? OR [Barcodi] LIKE ? ORDER BY [Artikulli]", connection))
                {
                    cmd.Parameters.Add("p1", OleDbType.VarWChar, 255).Value = "%" + term + "%";
                    cmd.Parameters.Add("p2", OleDbType.VarWChar, 255).Value = "%" + term + "%";
                    var dt = new DataTable();
                    using (var da = new OleDbDataAdapter(cmd)) da.Fill(dt);
                    gridProducts.DataSource = dt;
                    lblStatus.Text = "Artikuj të gjetur: " + dt.Rows.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë leximit të artikujve:\r\n" + ex.Message,
                    "ProShitje 1.3");
            }
        }

        private void AddSelectedProduct()
        {
            if (gridProducts.CurrentRow == null) return;
            var r = gridProducts.CurrentRow;
            string name = Convert.ToString(r.Cells["Artikulli"].Value);
            decimal price = ToDecimal(r.Cells["Cmimi i daljes"].Value);
            int dpt = 0;
            var row = saleItems.NewRow();
            row["Artikulli"] = name;
            row["Sasia"] = 1m;
            row["Cmimi"] = price;
            row["DPT"] = dpt;
            saleItems.Rows.Add(row);
        }

        private void RemoveSelectedSaleItem()
        {
            if (gridSale.CurrentRow == null) return;
            if (gridSale.CurrentRow.DataBoundItem is DataRowView view)
                saleItems.Rows.Remove(view.Row);
        }

        private void ExportAed()
        {
            if (saleItems.Rows.Count == 0)
            {
                MessageBox.Show("Shto të paktën një artikull.");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "Text file (*.txt)|*.txt", FileName = "fatura.txt" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("CLEAR");
                sb.AppendLine("KEY REG");
                foreach (DataRow row in saleItems.Rows)
                {
                    string desc = Convert.ToString(row["Artikulli"]).Replace("'", "''");
                    decimal qty = Convert.ToDecimal(row["Sasia"]);
                    decimal price = Convert.ToDecimal(row["Cmimi"]);
                    int dpt = Convert.ToInt32(row["DPT"]);
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "SALE DPT={0},qty={1},PRICE={2},DES='{3}'",
                        dpt, qty, price, desc));
                }
                sb.AppendLine("SUBTOT");
                sb.AppendLine("CLOSE TENDER=1");
                File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(false));
                MessageBox.Show("Skedari AED u krijua:\r\n" + sfd.FileName +
                    "\r\nKontrolloje me programin fiskal para përdorimit real.");
            }
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.CurrentCulture);
        }
    }
}

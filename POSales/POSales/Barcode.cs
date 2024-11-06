using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zen.Barcode;

namespace POSales
{
    public partial class Barcode : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        string fname;

        public Barcode()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());
            LoadProduct();
        }

        public void LoadProduct()
        {
            int i = 0;
            dgvBarcode.Rows.Clear();
            cm = new SqlCommand("SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty " +
                    "FROM tbProduct AS p " +
                    "INNER JOIN tbBrand AS b ON b.id = p.bid " +
                    "INNER JOIN tbCategory AS c ON c.id = p.cid " +
                    "WHERE CONCAT(p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty) " +
                    "LIKE '%' + @searchText + '%'", cn);

            cm.Parameters.AddWithValue("@searchText", txtSearch.Text);

            cn.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvBarcode.Rows.Add(i, dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString(), dr[5].ToString(), dr[6].ToString());
            }
            dr.Close();
            cn.Close();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProduct();
        }

        private void dgvBarcode_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvBarcode.Columns[e.ColumnIndex].Name;
            if (colName == "Select")
            {
                Code128BarcodeDraw barcode = BarcodeDrawFactory.Code128WithChecksum;
                picBarcode.Image = barcode.Draw(dgvBarcode.Rows[e.RowIndex].Cells[2].Value.ToString(), 60, 2);
                fname = dgvBarcode.Rows[e.RowIndex].Cells[1].Value.ToString();

            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Title = "Save Barcode Image As";
            savefile.FileName = fname;
            savefile.Filter = "Image File (*.jpg, *.png)|*.jpg;*.png";

            ImageFormat imageFormat = ImageFormat.Png;

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                string ftype = System.IO.Path.GetExtension(savefile.FileName);
                switch (ftype.ToLower())
                {
                    case ".jpg":
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case ".png":
                        imageFormat = ImageFormat.Png;
                        break;
                }

                // Save the barcode image with the specified format
                picBarcode.Image.Save(savefile.FileName, imageFormat);
            }
        }
    }
}

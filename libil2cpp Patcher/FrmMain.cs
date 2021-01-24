using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace libil2cpp_Patcher
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        #region Drag/Drop

        private void FrmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                Path.GetExtension(file);
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void FrmMain_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                txtFile.Text = files[0];
            }
            catch
            {
            }
        }

        private void FrmMain_DragOver(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                Path.GetExtension(file);
            }
        }

        #endregion Drag/Drop

        #region Settings

        private void btnFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            txtFile.Text = openFileDialog1.FileName;
        }

        #region Remove/Clear

        private void btnRemove_Click(object sender, EventArgs e)
        {
            RemoveRows();
        }

        private void dataList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveRows();
            }
        }

        private void RemoveRows()
        {
            try
            {
                if (dataList.RowCount <= 1 || dataList.CurrentCell.RowIndex == dataList.RowCount - 1) return;
                dataList.Rows.RemoveAt(dataList.SelectedCells[0].RowIndex);
            }
            catch
            {
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                while (dataList.RowCount > 1)
                {
                    dataList.Rows.RemoveAt(0);
                }
            }
            catch
            {
            }
        }

        #endregion Remove/Clear

        private void cbHelper_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbHelper.SelectedIndex)
            {
                case 0:
                    BeginInvoke(new Action(() => cbHelper.Text = @"01 00 A0 E3 1E FF 2F E1"));
                    break;

                case 1:
                case 2:
                    BeginInvoke(new Action(() => cbHelper.Text = @"00 00 A0 E3 1E FF 2F E1"));
                    break;
            }
        }

        #region Load/Save Xml

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void LoadData()
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            dataList.DataSource = null;
            dataList.Columns.Clear();
            try
            {
                var path = openFileDialog1.FileName;
                var dataSet = new DataSet();
                dataSet.ReadXml(path);
                dataList.DataSource = dataSet.Tables[0];
                dataList.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch
            {
            }
        }

        //https://stackoverflow.com/a/26335800/8902883
        private void SaveData()
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            var path = saveFileDialog1.FileName;

            var dt = new DataTable { TableName = "Patcher" };
            for (var i = 1; i < dataList.Columns.Count + 1; i++)
            {
                var column = new DataColumn(dataList.Columns[i - 1].HeaderText);
                dt.Columns.Add(column);
            }
            var columnCount = dataList.Columns.Count;
            for (var index = 0; index < dataList.Rows.Count - 1; index++)
            {
                var dr = dataList.Rows[index];
                var dataRow = dt.NewRow();
                for (var i = 0; i < columnCount; i++)
                {
                    dataRow[i] = dr.Cells[i].Value;
                }

                dt.Rows.Add(dataRow);
            }

            var ds = new DataSet();
            ds.Tables.Add(dt);
            var xmlSave = new XmlTextWriter(path, Encoding.UTF8);

            ds.WriteXml(xmlSave);
            xmlSave.Close();
        }

        #endregion Load/Save Xml

        #endregion Settings

        private void btnPatch_Click(object sender, EventArgs e)
        {
            try
            {
                File.Copy(txtFile.Text, txtFile.Text + ".bak");
            }
            catch 
            {
                
            }

            for (var i = 0; i < dataList.RowCount - 1; i++)
            {
                var bytes = StringToByteArray(DataValue(2, i));
                var offset = Convert.ToInt32(DataValue(1, i), 16);
                using var fs = new FileStream(txtFile.Text, FileMode.Open, FileAccess.ReadWrite)
                {
                    Position = offset
                };
                fs.Write(bytes);
            }
        }

        private string DataValue(int column, int row)
        {
            return dataList[column, row].Value.ToString()?.Replace(" ", "");
        }

        //https://stackoverflow.com/a/311179/8902883
        private static byte[] StringToByteArray(String hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }
    }
}
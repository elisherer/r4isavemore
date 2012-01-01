using System;
using System.Text;
using System.Windows.Forms;

namespace R4iSaveMore
{
    public partial class frmR4iSaveMore : Form
    {
        private readonly R4ISaveDongle _device;
        
        public frmR4iSaveMore()
        {
            InitializeComponent();
            _device = new R4ISaveDongle();
            _device.usbEvent += DeviceUsbEvent;
            _device.ProgressChanged += DeviceProgressChanged;
            _device.DownloadComplete += DeviceDownloadComplete;
            _device.findTargetDevice();
            cmbPre.SelectedIndex = 0;
        }

        void DeviceDownloadComplete(object sender, int value)
        {
            MessageBox.Show(value == 0 ? "Download complete!" : "Something went wrong!");
            pbDevice.Value = 0;
            btnDownload.Enabled = true;
        }

        void DeviceProgressChanged(object sender, int value)
        {
            pbDevice.Value = value;
        }

        private static string ByteArrayToHexEdString(byte[] array)
        {
            int i, l;
            var arraystring = string.Empty;
            var line = new byte[0x10];
            for (i = 0, l = 0; i < array.Length; i++, l++)
            {
                if ((i % 0x10) == 0)
                    arraystring += String.Format("0x{0:X4}  ", i);
                arraystring += String.Format("{0:X2} ", array[i]);
                line[i%0x10] = array[i] > 31 ? array[i] : (byte)' ';
                if (((i + 1) % 0x10) == 0) //line of text
                    arraystring += " " + Encoding.ASCII.GetString(line) + Environment.NewLine;
            }
            return arraystring;
        }

        private static byte[] ParseByteArray(string baString)
        {
            baString = baString.Replace(" ", ""); //delete all spaces
            if (baString.Length % 2 != 0)
                return null;
            try
            {
                var ret = new byte[baString.Length / 2];
                for (int i = 0, j = 0; i < baString.Length; i += 2, j++)
                    ret[j] = Convert.ToByte(baString.Substring(i, 2), 16);
                return ret;
            }
            catch
            {
                return null;
            }
        }

        void DeviceUsbEvent(object sender, EventArgs e)
        {
            var found = _device.IsDeviceAttached;
            grpSandbox.Enabled = found;
            Text = Application.ProductName + (found ? " - Connected" : " - Not connected");
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            var bytes = _device.SendCommand(ParseByteArray(txtSend.Text),(int)numRead.Value);
            txtOutput.Text = bytes != null ? ByteArrayToHexEdString(bytes) : "<Error>";
        }

        private void cmbPre_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbPre.SelectedIndex)
            {
                case 0:
                    numRead.Value = 3;
                    txtSend.Text = "a00000";
                    break;
                case 1:
                    numRead.Value = 10;
                    txtSend.Text = "222200";
                    break;
                case 2:
                    numRead.Value = 0;
                    txtSend.Text = "666603";
                    break;
                case 3:
                    numRead.Value = 0;
                    txtSend.Text = "111103";
                    break;
                case 4:
                    numRead.Value = 8;
                    txtSend.Text = "3333030000000000000000";
                    break;
                case 5:
                    numRead.Value = 0;
                    txtSend.Text = "2f2f03";
                    break;
            }
        }

        private void btnGameInfo_Click(object sender, EventArgs e)
        {
            var info = _device.R4IGetGameName();
            lblName.Text = info.Name;
            lblSaveSize.Text = info.SaveFlashSize.ToString();
        }

        private void btnFWVer_Click(object sender, EventArgs e)
        {
            lblFw.Text = _device.R4IGetVersion().ToString();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog() {Filter = "Save file (*.sav)|*.sav"};
            if (dlg.ShowDialog() != DialogResult.OK) return;
            btnDownload.Enabled = false;
            _device.R4IDownloadSave(dlg.FileName);
        }

    }
}

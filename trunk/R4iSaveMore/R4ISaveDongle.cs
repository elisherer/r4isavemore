using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using HIDCommunications;

namespace R4iSaveMore
{
    internal class R4ISaveDongle : HIDCommControl
    {
        public struct GameInformation
        {
            public string Name;
            public int SaveFlashSize;
        }

        private static readonly byte[] StaticBuffer = new byte[0x41];

        private const int R4ISaveDongleVID = 0x04D8;
        private const int R4ISaveDonglePID = 0x003F;

        private static readonly byte[] StartDownloadBytes = { 0x11, 0x11, 0x03};
        private const byte StartDownloadResponse = 0;

        private static readonly byte[] GetHeaderBytes = { 0x22, 0x22, 0x00 };
        private const byte GetHeaderResponse = 10;

        private static readonly byte[] StopBytes = { 0x2f, 0x2f, 0x00 };
        private const byte StopResponse = 0;

        private static readonly byte[] Get512Bytes = { 0x33, 0x33, 0x03, 0, 0, 0, 0, 0, 0, 0, 0 }; //and u24 - offset (BE-msB first)
        private const byte Get512Response = 8; //after 'Start' and before 'Stop'
        
        private static readonly byte[] WriteBytes = { 0x44, 0x44, 0x00, 0x02 }; //u24 - offset and 0x20 of data
        private const byte WriteResponse = 0; //after 'Start' and before 'Stop'

        private static readonly byte[] ClearRamBytes = { 0x66, 0x66, 0x00 , 0x01, 0x00};
        private const byte ClearRamResponse = 0;

        private static readonly byte[] CheckBytes = { 0xa0, 0x00, 0x00 };
        private const byte CheckResponse = 3;

        private BackgroundWorker _backgroundWorker;
        private bool _busy;

        public delegate void Int32EventHandler(Object sender, int value);

        public event Int32EventHandler ProgressChanged;
        public event Int32EventHandler DownloadComplete;

        public R4ISaveDongle(): base(R4ISaveDongleVID, R4ISaveDonglePID)
        {}

        public R4ISaveDongle(int vid, int pid) : base(vid, pid) //might be used to do the upgrade thing
        {}

        public byte[] SendCommand(byte[] array, int readBlocks)
        {
            var outputBuffer = new byte[(0x40+1) * readBlocks];
            var returnBuffer = new byte[0x40*readBlocks];
            StaticBuffer[0] = 0;
            var sizeToWrite = array.Length <= 0x40 ? array.Length : 0x40;
            Buffer.BlockCopy(array, 0, StaticBuffer, 1, sizeToWrite);
            var success = writeRawReportToDevice(StaticBuffer);
            if (success && readBlocks > 0)
            {   success = readMultipleReportsFromDevice(ref outputBuffer, readBlocks);
                if (success)
                    for (var i=0;i<readBlocks;i++)
                        Buffer.BlockCopy(outputBuffer,1+0x41*i,returnBuffer,i*0x40,0x40);
            }
            return success ? returnBuffer : null ;
        }

        public int R4IGetVersion()
        {
            var retBuffer = SendCommand(CheckBytes, CheckResponse);
            return retBuffer[2];
        }

        public GameInformation R4IGetGameName()
        {
            var info = new GameInformation();
            var retBuffer = SendCommand(GetHeaderBytes, GetHeaderResponse);
            var stringBytes = new byte[12];
            if (retBuffer == null)
                return info;
            Buffer.BlockCopy(retBuffer,0x40,stringBytes,0,stringBytes.Length);
            info.Name = stringBytes[0] != 0 ? Encoding.ASCII.GetString(stringBytes) : "0x" + retBuffer[0x240].ToString("X") + retBuffer[0x241].ToString("X") + retBuffer[0x242].ToString("X");
            if (stringBytes[0] == 0) //3DS ?
                info.SaveFlashSize = retBuffer[0x242] == 0x11 ? 0x20000 : retBuffer[0x242] == 0x13 ? 0x80000 : 0;
            else
            {
                info.SaveFlashSize = 512; //Don't know ???
            }
            
            return info;
        }

        public void R4IDownloadSave(string filePath)
        {
            if (_busy)
            {
                if (DownloadComplete != null)
                    DownloadComplete(this, -2); //send error -2
                return;
            }
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.ProgressChanged += BackgroundWorkerProgressChanged;
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync(filePath);
            _busy = true;
        }

        #region Background Worker methods

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument.ToString();
            // Get game's file size
            var gameInfo = R4IGetGameName();
            if (gameInfo.SaveFlashSize <= 0) { e.Result = -1; return; }          
            // Prepare buffers
            var commandBuffer = new byte[Get512Bytes.Length];
            Buffer.BlockCopy(Get512Bytes, 0, commandBuffer, 0, Get512Bytes.Length);
            if (File.Exists(filePath))
                File.Delete(filePath);
            var fs = File.OpenWrite(filePath);
            // Start the process
            SendCommand(StopBytes, StopResponse);
            SendCommand(ClearRamBytes, ClearRamResponse);
            SendCommand(StartDownloadBytes, StartDownloadResponse);
            while (fs.Length < gameInfo.SaveFlashSize)
            {
                commandBuffer[3] = (byte)((fs.Length >> 16) & 0xFF); // MSB - BigE
                commandBuffer[4] = (byte)((fs.Length >> 8) & 0xFF);
                commandBuffer[5] = (byte)(fs.Length & 0xFF);        //LSB
                var response = SendCommand(commandBuffer, Get512Response);
                if (response == null) { fs.Close(); e.Result = -1; return; } //Exit with error
                //Write the information to the file
                fs.Write(response, 0, response.Length);
                _backgroundWorker.ReportProgress((int)(100 * fs.Length / gameInfo.SaveFlashSize));
            }
            fs.Close();
            commandBuffer[0] = 0x2f;
            commandBuffer[1] = 0x2f;
            SendCommand(commandBuffer, StopResponse); // Stop (we don't care if it fails...)
            e.Result = 0;
        }

        private void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e.ProgressPercentage);
        }
        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _busy = false;
            if (DownloadComplete != null)
                DownloadComplete(this, (int)e.Result);
        }
        #endregion


    }
}
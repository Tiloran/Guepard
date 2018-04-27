using System;
using System.Linq;
using System.Threading;
using HidSharp;

namespace Guepard.HID
{
    class Usb
    {
        private readonly HidDeviceLoader _loader = new HidDeviceLoader();
        private HidDevice _guepard;
        private HidStream _stream;
        private int _vid; // Vendor ID
        private int _pid; // Product ID
        public bool Connection; // Переменная для хранения рельтута попытки подключиться к устройству
        private Timer _timer;

        public Usb(int vid, int pid)
        {
            _vid = vid;
            _pid = pid;
            Reconnect();
            CheckConnection();
            _timer = new Timer(Checker, null, 100, 0);
        }

        public void ChangeDevice(int vid, int pid)
        {
            _vid = vid;
            _pid = pid;
        }

        private void Checker(object sender)
        {
                Reconnect();
                CheckConnection();
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(Checker, null, 100, 0);
        }

        private void Reconnect() // Попытка подключиться к устройству
        {
            _stream = null;
            

            try
            {
                _guepard = _loader.GetDevices(_vid, _pid).FirstOrDefault();
                _guepard?.TryOpen(out _stream);
                if (_stream != null)
                {
                    _stream.ReadTimeout = 150;  // Реальное время операции приблизительно 8 мс
                    _stream.WriteTimeout = 150;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Block 1\n"  + e.Message);
            }
        }

        private void CheckConnection() // Получаем результаты попытки подключиться 
        {
            if (_stream != null)
            {
                Connection = true;
            }
            else
            {
                Connection = false;
            }
        }

        public void RestoreCheckConnectionTimer()
        {
            lock (_threadHolder)
            {
                _timer = new Timer(Checker, null, 100, 0);
            }
        }

        private object _threadHolder = new object();
        public bool Send(byte[] buffer) //Попытка отправить данные по юсб
        {
            lock (_threadHolder)
            {

                if (_timer != null)
                {
                    _timer.Dispose();
                }
                _timer = null;
                _guepard = null;
                Reconnect();
                CheckConnection();
                bool succes = false;
                if (Connection)
                {
                    try
                    {
                        _stream.Write(buffer);
                        succes = true;
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("BLock 2\n" + e.Message);
                        succes = false;
                    }
                }
                return succes;
            }
        }

        public (byte[] answerBuffer, bool succes) Receive() //Попытка получить данные по юсб
        {
            lock (_threadHolder)
            {
                bool succes = false;
                byte[] answerBuffer = new byte[32];
                if (Connection)
                {
                    try
                    {
                        answerBuffer = new byte[_guepard.MaxInputReportLength];
                        answerBuffer = _stream.Read();
                        succes = true;
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("Block 3\n" + e.Message);
                        succes = false;
                    }
                }
                _guepard = null;
                _stream = null;

                _timer = new Timer(Checker, null, 100, 0);
                var result = (answerBuffer, succes);
                return result;
            }
        }
    }
}

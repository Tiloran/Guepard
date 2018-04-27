using System;
using Guepard.HID;

namespace Guepard.Channels
{
    class Test
    {
        private Usb _usb;

        public Test(Usb myusb)
        {
            _usb = myusb;
        }

        public (byte[] transmitBuffer, bool succes) Send()
        {
            var random = new Random(Guid.NewGuid().GetHashCode()); // Для случайных чисел в тесте связи;
            bool succes = false;
            byte[] testArray = new byte[33];
            for (int i = 1; i < 5; i++)
            {
                testArray[i] = 0xFF;
            }
            testArray[5] = 32;
            testArray[6] = 0;
            for (int i = 7; i < 33; i++)
            {
                testArray[i] = Convert.ToByte(random.Next(0, 0xFF));
            }
            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(testArray))
                {
                    succes = true;
                }
            }
            return (testArray, succes);
        }

        public (byte[] Buffer, bool succes) Receive()
        {
            return _usb.Receive();
        }
    }
}
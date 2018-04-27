using Guepard.HID;

namespace Guepard.Channels
{
    class ControlChannel
    {
        private Usb _usb;

        public ControlChannel(Usb myusb)
        {
            _usb = myusb;
        }

        public (byte[] transmitBuffer, bool succes) Send(byte control)
        {
            byte[] transmitBuffer = new byte[21];
            bool succes = false;
            int controlSum = 0;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 0b00010100; // Длина кодограммы младший байт
            transmitBuffer[6] = 0; // Длина кодограммы старший байт
            transmitBuffer[15] = 18; //Смещение информации для 5 абонента от начала кодограммы
            transmitBuffer[16] = 1; //Длина информации абонента в байтах
            
            transmitBuffer[19] = control;
            foreach (var cell in transmitBuffer)
            {
                controlSum += cell;
            }
            transmitBuffer[20] = (byte) controlSum;
            
            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }
            _usb.RestoreCheckConnectionTimer();
            return (transmitBuffer, succes);
        }


        public (byte[] transmitBuffer, bool succes) Send_SetControl(byte control)
        {
            byte[] transmitBuffer = new byte[8];
            bool succes = false;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 50; // Длина кодограммы младший байт
            transmitBuffer[6] = 0;
            transmitBuffer[7] = control;

            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }
            _usb.RestoreCheckConnectionTimer();
            return (transmitBuffer, succes);
        }
    }
}

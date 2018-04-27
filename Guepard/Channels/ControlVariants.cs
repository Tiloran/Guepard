using Guepard.HID;

namespace Guepard.Channels
{
    class ControlVariants
    {
        private Usb _usb;

        public ControlVariants(Usb myusb)
        {
            _usb = myusb;
        }

        public (byte[] transmitBuffer, bool succes) Send(byte control)
        {
            byte[] transmitBuffer = new byte[7];
            bool succes = false;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = control;
            transmitBuffer[6] = 0;

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

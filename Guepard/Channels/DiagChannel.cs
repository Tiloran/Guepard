using Guepard.HID;

namespace Guepard.Channels
{
    class DiagChannel
    {
        private Usb _usb;

        public DiagChannel(Usb myusb)
        {
            _usb = myusb;
        }

        public (byte[] transmitBuffer, bool succes) Send(byte senderAdress, byte receiverAdress, byte[] freqArray)
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;
            int controlSum = 0;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }
            
            transmitBuffer[5] = 13; // Длина кодограммы младший байт
            transmitBuffer[6] = 0; // Длина кодограммы старший байт
            transmitBuffer[7] = receiverAdress; // Адрес получателя
            transmitBuffer[8] = senderAdress; // Адрес отправителя
            transmitBuffer[9] = freqArray[0]; //Младший байт кода частоты на передачу
            transmitBuffer[10] = freqArray[1]; //Старший байт кода частоты на передачу
            transmitBuffer[11] = freqArray[2]; //Младший байт кода частоты на прием
            transmitBuffer[12] = freqArray[3]; //Старший байт кода частоты на прием
            foreach (var cell in transmitBuffer)
            {
                controlSum += cell;
            }
            transmitBuffer[13] = (byte)controlSum; //Контрольная сумма
            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }
            
            return (transmitBuffer, succes);
        }

        public (byte[] Buffer, bool succes) Receive() 
        {
            return _usb.Receive();
        }
    }
}

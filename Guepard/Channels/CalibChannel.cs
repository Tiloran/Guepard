using Guepard.HID;

namespace Guepard.Channels
{
    class CalibChannel
    {
        private Usb _usb;

        public CalibChannel(Usb myusb)
        {
            _usb = myusb;
        }

        public (byte[] transmitBuffer, bool succes) SendFreq(byte[] freqArray)
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;
            int controlSum = 0;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 60; // Длина кодограммы младший байт
            transmitBuffer[6] = 0; // Длина кодограммы старший байт
            transmitBuffer[7] = 5; // Адрес получателя
            transmitBuffer[8] = 20; // Адрес отправителя
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

        public (byte[] transmitBuffer, bool succes) SendAt(byte block, byte type, byte[] atArray)
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }
            
            transmitBuffer[5] = block; // Команда блока
            transmitBuffer[6] = 0;
            transmitBuffer[7] = type;  // Команда типа (0 или -10 Дб)
            transmitBuffer[8] = atArray[0]; // младшие знач  1 Аттенюатора
            transmitBuffer[9] = atArray[1]; // старшие знач  1 Аттенюатора
            transmitBuffer[10] = atArray[2]; // младший знач  2 Аттенюатора
            transmitBuffer[11] = atArray[3]; // старшие знач 2 Аттенюатора
            transmitBuffer[12] = atArray[4]; // младшие знач 3 Аттенюатора
            transmitBuffer[13] = atArray[5]; // старшие знач 3 Аттенюатора
            
            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }

            return (transmitBuffer, succes);
        }


        public (byte[] transmitBuffer, bool succes) SendWriteEeprom(byte[] stringArray, byte command)
        {
            byte[] transmitBuffer = new byte[24];
            bool succes = false;
            
            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        }

            transmitBuffer[5] = command; //Номер команды
            transmitBuffer[6] = 0;
            if (command == 63 || command == 64)
            {
                for (int i = 7; i < 24; i++)
                {
                    transmitBuffer[i] = stringArray[i - 7];
                }
            }
            else
            {
                for (int i = 7; i < 13; i++)
                {
                    transmitBuffer[i] = stringArray[i - 7];
                }
            }

            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }

            return (transmitBuffer, succes);
        }


        public (byte[] transmitBuffer, bool succes) SendWriteCoefDb(byte ks1, byte ks2, byte get)
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;

            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 65; //Номер команды
            transmitBuffer[7] = ks1;
            transmitBuffer[8] = ks2;
            transmitBuffer[9] = get;


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

        public (byte[] transmitBuffer, bool succes) SendReadCoefDb()
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;

            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 66; //Номер команды
            


            if (_usb.Connection) // ПЕРЕДАЧА ЧЕРЕЗ ЮСБ
            {
                if (_usb.Send(transmitBuffer))
                {
                    succes = true;
                }
            }

            return (transmitBuffer, succes);
        }



        public (byte[] transmitBuffer, bool succes) SendRequestForCoefDb()
        {
            byte[] transmitBuffer = new byte[14];
            bool succes = false;

            for (int i = 1; i < 5; i++)
            {
                transmitBuffer[i] = 0xFF; // Признак начала кодограммы
            }

            transmitBuffer[5] = 66; //Номер команды

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

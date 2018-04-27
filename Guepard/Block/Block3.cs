using System;

namespace Guepard.Block
{
    public class Block3
    {
        public Block3(int freq, int temp, int atNes, int atKs10, int atKs20, int atKs110, int atKs210)
        {
            Freq = freq;
            Temp = temp;

            if (atNes > 4095)
            {
                throw new Exception();
            }
            else if (atNes < 0)
            {
                throw new Exception();
            }
            else
            {
                AtNes = atNes;
            }

            if (atKs10 > 4095)
            {
                throw new Exception();
            }
            else if (atKs10 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtKs10 = atKs10;
            }

            if (atKs20 > 4095)
            {
                throw new Exception();
            }
            else if (atKs20 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtKs20 = atKs20;
            }

            if (atKs110 > 4095)
            {
                throw new Exception();
            }
            else if (atKs110 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtKs110 = atKs110;
            }

            if (atKs210 > 4095)
            {
                throw new Exception();
            }
            else if (atKs210 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtKs210 = atKs210;
            }

            
            
            
            
        }

        private int freq;

        public int Freq
        {
            get => freq;
            set
            {
                var max = 3200;
                var min = 2805;
                var step = 5;
                if (value > max || value < min)
                {
                    throw new Exception();
                }
                var tempValue = min;
                while (true)
                {
                    if (tempValue == value)
                    {
                        freq = value;
                        break;
                    }
                    if(tempValue == max)
                    {
                        throw new Exception();
                    }
                    tempValue += step;
                }
            }
        }

        public int temp;

        public int Temp
        {
            get => temp;
            set
            {
                var max = 70;
                var min = -50;
                var step = 5;
                if (value > max || value < min)
                {
                    throw new Exception();
                }
                var tempValue = min;
                while (true)
                {
                    if (tempValue == value)
                    {
                        temp = value;
                        break;
                    }
                    if (tempValue == max)
                    {
                        throw new Exception();
                    }
                    tempValue += step;
                }
            }
        }


        private int _atNes;

        public int AtNes
        {
            get => _atNes;
            
            set
            {
                if (value > 4095)
                {
                    _atNes = 4095;
                }
                else if (value < 0)
                {
                    
                    _atNes = 0;
                }
                _atNes = value;
            }
        }

        private int _atKs10;

        public int AtKs10
        {
            get => _atKs10;

            set
            {
                if (value > 4095)
                {
                    _atKs10 = 4095;
                }
                else if (value < 0)
                {
                    _atKs10 = 0;
                }
                _atKs10 = value;
            }
        }
        private int _atKs20;

        public int AtKs20
        {
            get => _atKs20;

            set
            {
                if (value > 4095)
                {
                    _atKs20 = 4095;
                }
                else if (value < 0)
                {
                   _atKs20 = 0;
                }
                _atKs20 = value;
            }
        }
        private int _atKs110;

        public int AtKs110
        {
            get => _atKs110;

            set
            {
                if (value > 4095)
                {
                    _atKs110 = 4095;
                }
                else if (value < 0)
                {
                    _atKs110 = 0;
                }
                _atKs110 = value;
            }
        }
        private int _atKs210;

        public int AtKs210
        {
            get => _atKs210;

            set
            {
                if (value > 4095)
                {
                    _atKs210 = 4095;
                }
                else if (value < 0)
                {
                    _atKs210 = 0;
                }
                _atKs210 = value;
            }
        }
    }
}

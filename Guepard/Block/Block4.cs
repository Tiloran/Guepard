using System;

namespace Guepard.Block
{
    public class Block4
    {
        public Block4(int freq, int temp, int atFGet0, int atFGet6)
        {
            Freq = freq;
            Temp = temp;

            if (atFGet0 > 4095)
            {
                throw new Exception();
            }
            else if (atFGet0 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtFGet0 = atFGet0;
            }

            if (atFGet6 > 4095)
            {
                throw new Exception();
            }
            else if (atFGet6 < 0)
            {
                throw new Exception();
            }
            else
            {
                AtFGet6 = atFGet6;
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
                    if (tempValue == max)
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

        private int _atFGet0;
        public int AtFGet0
        {
            get => _atFGet0;

            set
            {
                if (value > 4095)
                {
                    _atFGet0 = 4095;
                }
                else if (value < 0)
                {
                    _atFGet0 = 0;
                }
                else
                {
                    _atFGet0 = value;
                }
            }
        }
        private int _atFGet6;
        public int AtFGet6
        {
            get => _atFGet6;

            set
            {
                if (value > 4095)
                {
                    _atFGet6 = 4095;
                }
                else if (value < 0)
                {
                    _atFGet6 = 0;
                }
                else
                {
                    _atFGet6 = value;
                }
            }
        }
    }
}

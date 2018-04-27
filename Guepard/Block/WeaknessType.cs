using System;
using System.Linq;

namespace Guepard.Block
{
    public class WeaknessType
    {
        public WeaknessType(string type, string freq,int temp, double weaknessInDb, int realWeakness)
        {
            string[] checkTypeArrrayString =
            {
                "КС1", "КС2", "ГЕТ", "2950-2999", "3000-3049", "3050-3099", "3100-3149", "3150-3200"
            };


            if (checkTypeArrrayString.Contains(type))
            {
                Type = type;
            }
            else
            {
                throw new Exception();
            }

            string[] checkFreqArrrayString =
            {
                "2800-2849", "2850-2899", "2900-2949", "2950-2999", "3000-3049", "3050-3099", "3100-3149", "3150-3200"
            };

            if (checkFreqArrrayString.Contains(freq))
            {
                Freq = freq;
            }
            else
            {
                throw new Exception();
            }
            


            var max = 70;
            var  min = -50;
            var step = 5;
            if (temp > max || temp < min)
            {
                throw new Exception();
            }
            var tempValue = min;
            while (true)
            {
                if (tempValue == temp)
                {
                    Temp = temp;
                    break;
                }
                if (tempValue == max)
                {
                    throw new Exception();
                }
                tempValue += step;
            }


            var maxD = 10D;
            var minD = 0D;
            var stepD = 0.5D;
            if (weaknessInDb > maxD || weaknessInDb < minD)
            {
                throw new Exception();
            }
            var tempValueD = minD;
            while (true)
            {
                if (tempValueD == weaknessInDb)
                {
                    WeaknessInDb = weaknessInDb;
                    break;
                }
                if (tempValueD == maxD)
                {
                    throw new Exception();
                }
                tempValueD += stepD;
            }

            if (realWeakness > 4095)
            {
                throw new Exception();
                //_realWeakness = 4095;
            }
            if (realWeakness < 0)
            {
                throw new Exception();
                //_realWeakness = 0;
            }
            RealWeakness = realWeakness;
        }

        public  string Type { get; }
        public string Freq { get;  }
        public int Temp { get; }
        public double WeaknessInDb { get; }
        private int _realWeakness;
        public int RealWeakness
        {
            get => _realWeakness;
            set
            {
                if (value > 4095)
                {
                    _realWeakness = 4095;
                }
                else if (value < 0)
                {
                    _realWeakness = 0;
                }
                else
                {
                    _realWeakness = value;
                }
            }
        }
    }

}

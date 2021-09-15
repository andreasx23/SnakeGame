using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class Utility
    {
        private static readonly Random _rand;

        static Utility()
        {
            _rand = new Random();
        }

        public static float NextFloat()
        {
            double mantissa = (_rand.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, _rand.Next(-126, 128));
            return (float)(mantissa * exponent);
        }


        public static float NextFloat(float min, float max)
        {
            double mantissa = _rand.NextDouble() * (max - min) + min;
            return (float)mantissa;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltorbFlipApp
{
    public class Helper
    {
        
        public static bool IsResetAll(Keys key) => key == Keys.Multiply;
    }

    public class Constraint
    {
        public int X2 { get; set; }

        public int X3 { get; set; }

        public int Bomb { get; set; }

        public int Coins { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.ProductionSystem
{
    public enum QualityState
    {
        Low = 0, 
        Normal = 1, 
        High = 2, 
        Highest = 3

    }

    public class WorkState
    {
        public int WorkTime = 0;
        public int PocessRate = 0;
        public int Quality = 0;
        public int Endurance = 0;
        public int ProductionForce = 0;
        public int InnerStatic = 0;
        public QualityState CurrentQuality = QualityState.Normal;

        public bool QualityBelow(QualityState s)
        {
            return (int)CurrentQuality < (int)s;
        }
    }
}

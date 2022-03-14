using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.ProductionSystem
{
    public enum SpecialEffect
    {
        None = 0, 
        HQOnly = 1,
        SuccTo100AfterObservation = 2,
        Eff50pIfNotEnoughEndurance = 4, 
        ForceTo18AfterProcessing = 8, 
        DivInnerStatic = 16, 
        OnlyWhenInnerStatic = 32, 
        NotInFrugal = 64, 
        OnlyFirst = 128, 
        AddInnerStatic = 256,
        ElementMarkApplied = 512, 
        DQualityAdd20ByInnerStatic = 1024,
        NoWorkTime = 2048, 
    }

    public class PersistentEffect
    {

    }

    public partial class Skill
    {
        // fields
        public string NameKey { get; private set; } = "null";
        public int LeastLevel { get; private set; } = 1;
        public int DProcess { get; private set; } = 0;
        public int DQuality { get; private set; } = 0;
        public double SuccessRate { get; private set; } = 1;
        public int DForce { get; private set; } = 0;
        public int DEndurance { get; private set; } = 0;
        public int DInnerStatic { get; private set; } = 0;
        public SpecialEffect SpecialEffect { get; private set; } = SpecialEffect.None;
        
    }
}

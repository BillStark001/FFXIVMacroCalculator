using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{


    public enum ActionEffect
    {
        None = 0, 
        // restrictions
        OnlyGoodCondition = 1,
        OnlyFirstStep = 2,
        OnlyInInnerQuiet = 4,
        OnlyInMaxInnerQuiet = 8,
        OnlyNotInWasteNot = 16,

        // effects
        MustSuccessAfterObserve = 64,
        Effect50pIfNotEnoughDurability = 128, 

        CpTo18AfterBasicTouch = 1024,
        CpTo18AfterStandardTouch = 2048,

        AddInnerQuiet = 8192,
        DivideOrDoubleInnerQuiet = 16384,
        ClearInnerQuiet = 32768,
        DQualityAdd20ByInnerQuiet = 65536,

        NoCraftStep = 1048576,
    }

    // TODO make a generative model if possible
    public partial class Action
    {
        // fields
        public string ActionKey { get; private set; } = "null";

        public int LeastLevel { get; private set; } = 1;
        public double SuccessRate { get; private set; } = 1;

        public int DProgress { get; private set; } = 0;
        public int DQuality { get; private set; } = 0;
        public int DCraftPoints { get; private set; } = 0;
        public int DDurability { get; private set; } = 0;
        public int DInnerQuiet { get; private set; } = 0;

        public ActionEffect Effects { get; private set; } = ActionEffect.None;

        public IDictionary<Effect, int> TimedEffects { get; private set; } 
            = new ReadOnlyDictionary<Effect, int>(new Dictionary<Effect, int>());

        private Action() { }

        public bool Contains(ActionEffect eff)
        {
            return (int)(Effects | eff) != 0;
        }

        public bool Contains(Effect state)
        {
            return TimedEffects.ContainsKey(state) && TimedEffects[state] > 0;
        }


        // generate sets

        public ICollection<Action> General = new ReadOnlyCollection<Action>( new[] 
        {
            BasicSynthesis, 
            BasicSynthesis2, 
            RapidSynthesis, 
            RapidSynthesis2,
            IntensiveSynthesis, 
            CarefulSynthesis, 
            CarefulSynthesis2, 
            FocusedSynthesis, 
            GroundWork, 
            GroundWork2, 
            DelicateSynthesis, 
            PrudentSynthesis, 

            BasicTouch, 
            StandardTouch, 
            AdvancedTouch, 
            HastyTouch, 
            PreciseTouch, 
            FocusedTouch, 
            PreparatoryTouch, 
            PrudentTouch, 
            TrainedFinesse, 

            MuscleMemory, 
            Reflect, 
            ByregotsBlessing, 
            WasteNot, 
            WasteNotII, 
            Innovation, 
            Veneration, 
            GreatStrides, 
            MastersMend, 
            Observe, 
            FinalAppraisal, 
            TricksOfTheTrade, 
            // Manipulation
        });

        public ICollection<Action> WithManipulation = new ReadOnlyCollection<Action>(new[]
        {
            Manipulation
        });
    }
}

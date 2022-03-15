using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    using REI = ReadOnlyDictionary<Effect, int>;
    using DEI = Dictionary<Effect, int>;

    public partial class Action
    {

        public static readonly Action MuscleMemory = new()
        {
            ActionKey = "坚信",
            LeastLevel = 54,
            DProgress = 300,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 6,
            DDurability = 10,
            DInnerQuiet = 0,
            Effects =
                ActionEffect.OnlyFirstStep,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Process200pForOnce] = 5
            }),
        };

        public static readonly Action Reflect = new()
        {
            ActionKey = "闲静",
            LeastLevel = 69,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 6,
            DDurability = 10,
            DInnerQuiet = 1,
            Effects =
                ActionEffect.OnlyFirstStep |
                ActionEffect.AddInnerQuiet,
        };

        public static readonly Action ByregotsBlessing = new()
        {
            ActionKey = "比尔格的祝福",
            LeastLevel = 50,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 24,
            DDurability = 10,
            DInnerQuiet = 0,
            Effects =
                ActionEffect.DQualityAdd20ByInnerQuiet |
                ActionEffect.OnlyInInnerQuiet |
                ActionEffect.ClearInnerQuiet,
        };

        public static readonly Action WasteNot = new()
        {
            ActionKey = "俭约",
            LeastLevel = 15,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 56,
            DDurability = 0,
            DInnerQuiet = 0,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Durability50p] = 4
            }),
        };

        public static readonly Action WasteNotII = new()
        {
            ActionKey = "长期俭约",
            LeastLevel = 48,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 98,
            DDurability = 0,
            DInnerQuiet = 0,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Durability50p] = 8
            }),
        };

        public static readonly Action Innovation = new()
        {
            ActionKey = "改革",
            LeastLevel = 26,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Quality150p] = 4
            }),
        };

        public static readonly Action Veneration = new()
        {
            ActionKey = "崇敬",
            LeastLevel = 15,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Process150p] = 4
            }),
        };

        public static readonly Action GreatStrides = new()
        {
            ActionKey = "阔步", 
            LeastLevel = 21,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 32,
            DDurability = 0,
            DInnerQuiet = 0,
            TimedEffects = new REI(new DEI()
            {
                [Effect.Quality200pForOnce] = 3
            }),
        };

        public static readonly Action MastersMend = new()
        {
            ActionKey = "精修",
            LeastLevel = 7,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 88,
            DDurability = -30,
        };


        public static readonly Action Observe = new()
        {
            ActionKey = "观察",
            LeastLevel = 13,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 7,
            DDurability = 0,
            DInnerQuiet = 0,
            TimedEffects = new REI(new DEI()
            {
                [Effect.AfterObservation] = 1
            }),
        };



        public static readonly Action FinalAppraisal = new()
        {
            ActionKey = "最终确认",
            LeastLevel = 42,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 1,
            DDurability = 0,
            DInnerQuiet = 0,
            Effects =
                ActionEffect.NoCraftStep,
            TimedEffects = new REI(new DEI()
            {
                [Effect.DoNotComplete] = 5
            }),
        };

        public static readonly Action TricksOfTheTrade = new()
        {
            ActionKey = "秘诀",
            LeastLevel = 13,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = -20,
            DDurability = 0,
            Effects = ActionEffect.OnlyGoodCondition,
        };

        public static readonly Action Manipulation = new()
        {
            ActionKey = "掌握",
            LeastLevel = 65,
            DProgress = 0,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 96,
            DDurability = 0,
            DInnerQuiet = 0,
            TimedEffects = new REI(new DEI()
            {
                [Effect.DurabilityAdd5] = 8
            }),
        };

    }
}

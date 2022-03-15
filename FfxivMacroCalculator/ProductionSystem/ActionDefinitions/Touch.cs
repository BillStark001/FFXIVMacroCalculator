using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.ProductionSystem
{
    using REI = ReadOnlyDictionary<Effect, int>;
    using DEI = Dictionary<Effect, int>;

    public partial class Action
    {
        public static Action BasicTouch = new()
        {
            ActionKey = "加工",
            LeastLevel = 5,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            DDurability = 10,
            DInnerQuiet = 1,
            TimedEffects = new REI(new DEI()
            {
                [Effect.AfterBasicTouch] = 1
            }),
        };

        public static Action StandardTouch = new()
        {
            ActionKey = "中级加工",
            LeastLevel = 18,
            DProgress = 0,
            DQuality = 125,
            SuccessRate = 1.0,
            DCraftPoints = 32,
            DDurability = 10,
            DInnerQuiet = 1,
            Effects = ActionEffect.CpTo18AfterBasicTouch,
            TimedEffects = new REI(new DEI()
            {
                [Effect.AfterStandardTouch] = 1
            }),
        };

        public static Action AdvancedTouch = new()
        {
            ActionKey = "上级加工",
            LeastLevel = 84,
            DProgress = 0,
            DQuality = 150,
            SuccessRate = 1.0,
            DCraftPoints = 46,
            DDurability = 10,
            DInnerQuiet = 1,
            Effects = ActionEffect.CpTo18AfterStandardTouch,
        };

        public static Action HastyTouch = new()
        {
            ActionKey = "仓促",
            LeastLevel = 9,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 0.6,
            DCraftPoints = 0,
            DDurability = 10,
            DInnerQuiet = 1,
        };

        public static Action PreciseTouch = new()
        {
            ActionKey = "集中加工",
            LeastLevel = 53,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            DDurability = 10,
            DInnerQuiet = 2,
            Effects = ActionEffect.OnlyGoodCondition,
        };

        public static Action FocusedTouch = new()
        {
            ActionKey = "注视加工",
            LeastLevel = 68,
            DProgress = 0,
            DQuality = 150,
            SuccessRate = 0.5,
            DCraftPoints = 18,
            DDurability = 10,
            DInnerQuiet = 1,
            Effects = ActionEffect.MustSuccessAfterObserve,
        };  

        public static Action PreparatoryTouch = new()
        {
            ActionKey = "坯料加工",
            LeastLevel = 71,
            DProgress = 0,
            DQuality = 200,
            SuccessRate = 1.0,
            DCraftPoints = 40,
            DDurability = 20,
            DInnerQuiet = 2,
        };

        public static Action PrudentTouch = new()
        {
            ActionKey = "俭约加工",
            LeastLevel = 66,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 25,
            DDurability = 5,
            DInnerQuiet = 1,
            Effects = ActionEffect.OnlyNotInWasteNot,
        };

        public static Action TrainedEye = new()
        {
            ActionKey = "工匠的神速技巧",
            LeastLevel = 80,
            DProgress = 0,
            DQuality = 1048576,
            SuccessRate = 1.0,
            DCraftPoints = 250,
            DDurability = 10,
            DInnerQuiet = 0,
            Effects = ActionEffect.OnlyFirstStep,
        };

        public static Action TrainedFinense = new()
        {
            ActionKey = "工匠的绝技",
            LeastLevel = 90,
            DProgress = 0,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 32,
            DDurability = 0,
            DInnerQuiet = 0,
            Effects = ActionEffect.OnlyInMaxInnerQuiet,
        };

    }
}

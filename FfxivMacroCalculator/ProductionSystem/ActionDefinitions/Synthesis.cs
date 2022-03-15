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
        public static Action BasicSynthesis = new()
        {
            ActionKey = "制作",
            LeastLevel = 1,
            DProgress = 100,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 0,
            DDurability = 10,
        };

        public static Action BasicSynthesis2 = new()
        {
            ActionKey = "制作",
            LeastLevel = 31,
            DProgress = 120,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 0,
            DDurability = 10,
        };


        public static Action RapidSynthesis = new()
        {
            ActionKey = "高速制作",
            LeastLevel = 9,
            DProgress = 250,
            DQuality = 0,
            SuccessRate = 0.5,
            DCraftPoints = 0,
            DDurability = 10,
        };

        public static Action RapidSynthesis2 = new()
        {
            ActionKey = "高速制作",
            LeastLevel = 63,
            DProgress = 500,
            DQuality = 0,
            SuccessRate = 0.5,
            DCraftPoints = 0,
            DDurability = 10,
        };


        public static Action IntensiveSynthesis = new()
        {
            ActionKey = "集中制作",
            LeastLevel = 78,
            DProgress = 300,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 6,
            DDurability = 10,
            Effects = ActionEffect.OnlyGoodCondition,
        };

        public static Action CarefulSynthesis = new()
        {
            ActionKey = "模范制作",
            LeastLevel = 62,
            DProgress = 150,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 7,
            DDurability = 10,
        };

        public static Action CarefulSynthesis2 = new()
        {
            ActionKey = "模范制作",
            LeastLevel = 82,
            DProgress = 180,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 7,
            DDurability = 10,
        };

        public static Action FocusedSynthesis = new()
        {
            ActionKey = "注视制作",
            LeastLevel = 67,
            DProgress = 200,
            DQuality = 0,
            SuccessRate = 0.5,
            DCraftPoints = 5,
            DDurability = 10,
            Effects = ActionEffect.MustSuccessAfterObserve,
        };

        public static Action GroundWork = new()
        {
            ActionKey = "坯料制作",
            LeastLevel = 72,
            DProgress = 300,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            DDurability = 20,
            Effects = ActionEffect.Effect50pIfNotEnoughDurability,
        };

        public static Action GroundWork2 = new()
        {
            ActionKey = "坯料制作",
            LeastLevel = 86,
            DProgress = 360,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            DDurability = 20,
            Effects = ActionEffect.Effect50pIfNotEnoughDurability,
        };

        public static Action DelicateSynthesis = new()
        {
            ActionKey = "精密制作",
            LeastLevel = 76,
            DProgress = 100,
            DQuality = 100,
            SuccessRate = 1.0,
            DCraftPoints = 32,
            DDurability = 10,
        };

        public static Action PrudentSynthesis = new()
        {
            ActionKey = "俭约制作",
            LeastLevel = 88,
            DProgress = 180,
            DQuality = 0,
            SuccessRate = 1.0,
            DCraftPoints = 18,
            DDurability = 5,
            Effects = ActionEffect.OnlyNotInWasteNot,
        };

    }
}

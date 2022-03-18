using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    using StateSet = List<(double, CraftState)>;

    public static class Evaluator
    {

        public static List<(CraftState, double, int)> Evaluate(
            Macro m,
            RecipeGoal g,
            double progressMult = 1, 
            double qualityMult = 1,
            int maxCount = 5000, 
            IDictionary<ConditionState, double>? hqRateDict = null
            )
        {
            StateSet s = new() { (1, new()) };
            List<(CraftState, double, int)> records = new();
            records.Add((s[0].Item2, -1, 1));
            foreach (var action in m.Content)
            {
                var res = Simulator.SimulateSet(action, s, g, maxCount, hqRateDict)!;
                records.Add((CraftState.Average(res.PossibleStates), res.FailedRate, s.Count));
                s = res.PossibleStates;
            }
            return records;
        } 

        public static Table Evaluate2(
            Macro m, 
            RecipeGoal g,
            double progressMult = 1,
            double qualityMult = 1,
            int maxCount = 5000,
            IDictionary<ConditionState, double>? hqRateDict = null
            )
        {
            var ans = new Table(
                "#", 
                "Action", 
                "Progress", 
                "Quality", 
                "Durability", 
                "CP", 
                "Failed Rate", 
                "Sampling Number"
                );

            var records = Evaluate(m, g, progressMult, qualityMult, maxCount, hqRateDict);  
            for (int i = 0; i < records.Count; ++i)
            {
                var (state, fr, sn) = records[i];
                ans.AddRow(
                    i == 0 ? "Init" : i,
                    i == 0 ? "N/A" : m.Content[i - 1].ActionKey,
                    state.Progress * progressMult,
                    state.Quality * qualityMult,
                    g.Durability - state.Durability,
                    g.CraftingPoints - state.CraftingPoints,
                    i == 0 ? "N/A" : fr,
                    sn
                    );
            }

            ans.AddRow(
                "Goal", 
                "N/A", 
                g.Progress * progressMult,
                g.Quality * qualityMult,
                g.Durability, 
                g.CraftingPoints, 
                records.Last().Item1.Progress >= g.Progress ? 
                "Success" : "Fail", 
                "N/A"
                );

            return ans;
        }


    }
}

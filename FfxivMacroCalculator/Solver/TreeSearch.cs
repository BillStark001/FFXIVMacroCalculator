using FfxivMacroCalculator.CraftingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = FfxivMacroCalculator.CraftingSystem.Action;

namespace FfxivMacroCalculator.Solver
{
    public class TreeSearchContent
    {
        public RecipeGoal Goal = new();
        public IDictionary<ConditionState, double>? HqRateDict = null;
        public int MaxMacroLength = 28;
        public List<(double, Macro)> SuccessfulMacros = new();
        public long Count = 0;
    }

    public static class TreeSearch
    {
        static List<Effect> OnlySynthesis = new()
        {
            Effect.Process150p,
        };
        static List<Effect> OnlyTouch = new()
        {
            Effect.Quality150p,
        };

        public static void Search(this TreeSearchContent content, 
            List<Action> macro, 
            CraftState curState, 
            double stateRate = 1
            )
        {
            ++content.Count;
            if (content.Count % 1000000 == 0)
                Console.WriteLine(content.Count);

            // halt condition
            if (curState.Progress >= content.Goal.Progress)
            {
                Macro m = new(macro);
                Console.WriteLine(m.ToString());
                Console.WriteLine(curState);
                content.SuccessfulMacros.Add((curState.Quality, m));
                return;
            }
            if (macro.Count >= content.MaxMacroLength)
                return;


            IEnumerable<Action> actionSet;
            if (macro.Count == 0)
                actionSet = Action.Initial;
            else
            {
                // TODO determine what actions are applicable
                var supp = Action.Supplymental
                    .Where(x => x.TimedEffects.Count == 0 || !curState.Effects.TryGetValue(x.TimedEffects.FirstOrDefault().Key, out var v) || v < 2);
                if (curState.Effects.TryGetValue(Effect.Process150p, out var v) && v > 1)
                    actionSet = Action.Synthesis.Concat(supp);
                else if (curState.Effects.TryGetValue(Effect.Quality150p, out var v2) && v2 > 1)
                    actionSet = Action.Touch.Concat(supp);
                else
                    actionSet = supp.Concat(Action.Touch).Concat(Action.Synthesis);
            }


            foreach (var action in actionSet)
            {

                if (Simulator.CanApply(curState, action, content.Goal)) {
                    macro.Add(action);
                    var nextStates = Simulator.ForceSimulate(curState, action, content.Goal, stateRate, content.HqRateDict);
                    foreach (var (nextRate, nextState) in nextStates.PossibleStates)
                    {
                        Search(content, macro, nextState, nextRate);
                    }
                    macro.RemoveAt(macro.Count - 1);
                } 
            }
        }
    }
}

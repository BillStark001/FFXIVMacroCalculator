using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    public enum ConditionState
    {
        Poor = 0, 
        Normal = 1, 
        Good = 2, 
        Excellent = 3

    }

    public record CraftState
    {
        //var (N, P, Q, E, Z, I, H, L) = state
        public int CraftStep = 0;
        public double Progress = 0;
        public double Quality = 0;
        public int Durability = 0;
        public int CraftingPoints = 0;
        public int InnerQuiet = 0;
        public ConditionState Condition = ConditionState.Normal;
        public readonly Dictionary<Effect, int> Effects = new();

        public CraftState() { }

        public CraftState(int N, double P, double Q, int E, int Z, int I, ConditionState H, Dictionary<Effect, int> L)
        {
            CraftStep = N;
            Progress = P;
            Quality = Q;
            Durability = E;
            CraftingPoints = Z;
            InnerQuiet = I;
            Condition = H;
            Effects = L;
        }

        public CraftState(CraftState template, ConditionState h)
        {
            CraftStep = template.CraftStep;
            Progress = template.Progress;
            Quality = template.Quality;
            Durability = template.Durability;
            CraftingPoints = template.CraftingPoints;
            InnerQuiet = template.InnerQuiet;
            Condition = h;
            Effects = template.Effects;
        }

        public bool QualityBelow(ConditionState s)
        {
            return (int)Condition < (int)s;
        }

        public bool ContainsState(Effect s)
        {
            return Effects.ContainsKey(s) && Effects[s] > 0;
        }

        public int StateTimeRemain(Effect s)
        {
            return Effects.TryGetValue(s, out var t) ? t : 0;
        }

        public Dictionary<Effect, int> EvolveOnce()
        {
            var ret = new Dictionary<Effect, int>();
            foreach (var (state, time) in Effects)
            {
                var rtime = time - 1;
                if (rtime < 0)
                    throw new Exception("damn");
                else if (rtime > 0)
                    ret[state] = rtime;
            }
            return ret;
        }

        public static CraftState Average(IEnumerable<CraftState> states)
        {
            CraftState avg = new();
            var l = states.Count();
            foreach (var state in states)
            {
                avg.CraftStep = Math.Max(avg.CraftStep, state.CraftStep);
                avg.Progress += state.Progress / l;
                avg.Quality += state.Quality / l;
                avg.Durability = Math.Max(avg.Durability, state.Durability);
                avg.CraftingPoints = Math.Max(avg.CraftingPoints, state.CraftingPoints);
                avg.InnerQuiet += state.InnerQuiet;
            }
            avg.InnerQuiet /= l;
            return avg;
        }
    }
}

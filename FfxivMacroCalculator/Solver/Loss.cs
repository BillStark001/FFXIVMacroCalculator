using FfxivMacroCalculator.CraftingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.Solver
{
    using Action = CraftingSystem.Action;
    using FDD = Func<double, double>;
    using GeneticLoss = Func<double, double, double, double, double>;
    public static class Loss
    {
        private static Random _random = new Random();

        public static FDD Sigmoid(double x0 = 0, double y0 = 0, double dy = 1, double k0 = 0.25)
        {
            var dx = dy * (0.25 / k0);
            return x => (1 / (1 + Math.Exp((x0 - x) / dx))) * dy + y0;
        }

        public static GeneticLoss GeneticLoss(double p, double q, double e, double z)
        {
            var lp1 = Sigmoid(p * 0.75, 0, 1.5, 1.5 / p);
            FDD lp2 = x => Math.Pow(Math.Min(x / p, 1), 2.5);
            var lpup = Math.Pow(lp1(p * 1.05), 3);
            FDD lp = x => Math.Min(Math.Pow(lp1(x), 3) / lpup, 1) * 1.2;
            var lq_ = Sigmoid(q * 0.8, 0, 0.7, 5 / q);
            FDD lq = x => 0.3 * x / q + lq_(x);
            var le_ = Sigmoid(e + 7.5, 0, 1, -1 / 7.5);
            FDD le = x => le_(x) + Math.Min(0, -(x - e - 7.5));
            var lz = Sigmoid(z * 1.25, 0, 1, -2.2 / z);
            return (P, Q, E, Z) => 
                lp(P) + 
                lq(Q) * lp2(P) * 6 + 
                le(E) + 
                lz(Z) + 
                4 * Math.Min(Math.Min(lp(P), lq(Q)), Math.Min(le(E), lz(Z)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="kc">action set</param>
        /// <param name="mr">mutation rate</param>
        /// <returns></returns>
        public static Macro ReproduceAgent(Macro a1, Macro a2, Func<Action> randomGenerator, double mr)
        {
            if (a1.Length != a2.Length)
                throw new InvalidDataException("Do not try to force 2 agents with reproductive isolation to cause an R-18 scene.");

            var mr2 = mr * 0.7;
            List<Action> ans = new();
            while (ans.Count < a1.Length)
            {
                ans.AddRange(
                    (_random.Next(2) == 0 ? a1 : a2).Content
                    .Skip(ans.Count)
                    .Take(_random.Next(6)));
            }
            ans = ans.Take(a1.Length).ToList();
    
            // mask = np.random.choice(3, size=len(a1), p=[(1-mr2)/2, (1-mr2)/2, mr2])
            // ans = [a[mask[i]][i] if mask[i] < 2 else np.random.randint(kc) for i in range(len(a1))]

            for (int i = 0; i < a1.Length; ++i)
            {
                if (_random.NextSingle() <= mr2)
                    continue; // possibility of mr2
                switch (_random.Next(4))
                {
                    case 0:
                        ans.RemoveAt(i);
                        ans.Add(randomGenerator());
                        break;
                    case 1:
                        ans.RemoveAt(i);
                        ans.Insert(0, randomGenerator());
                        break;
                    case 2:
                        ans.Insert(i, randomGenerator());
                        ans.RemoveAt(a1.Length);
                        break;
                    case 3:
                        ans.Insert(i, randomGenerator());
                        ans.RemoveAt(0);
                        break;
                    default:
                        throw new InvalidOperationException("WIF");
                }
            }

            for (int i = 0; i < a1.Length - 1; ++i)
            {
                if (_random.NextSingle() <= mr2)
                    continue; // possibility of mr2
                var x = ans[i];
                ans[i] = ans[i + 1];
                ans[i + 1] = x;
            }

            return new(ans);
        }

        public static (double, double) EvaluateAgent(
            Macro agent, 
            RecipeGoal goal, 
            GeneticLoss? loss = null, 
            IDictionary<ConditionState, double>? hqRateDict = null, 
            double t_fr= 0.9) {
            var sim = new List<(double, CraftState)>() { (1, new()) };
            double loss_fr = 0;
            foreach (var action in agent.Content)
            {
                var simRes = Simulator.SimulateSet(action, sim, goal, maxCount: 3000, hqRateDict: hqRateDict);
                sim = simRes.PossibleStates;
                if (simRes.FailedRate > t_fr)
                    ++loss_fr;
            }
            loss = loss ?? GeneticLoss(goal.Progress, goal.Quality, goal.Durability, goal.CraftingPoints);
            double ans1 = 0;
            double ans2 = 0;
            foreach (var (srate, res) in sim)
            {
                ans1 += srate;
                ans2 += srate * loss(res.Progress, res.Quality, res.Durability, res.CraftingPoints);
            }
            ans2 /= ans1;
            ans2 -= loss_fr * 0.01;
            return (ans1, ans2);

        }



    }
}

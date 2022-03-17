using FfxivMacroCalculator.CraftingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = FfxivMacroCalculator.CraftingSystem.Action;

namespace FfxivMacroCalculator.Solver
{

    using MacroLoss = Func<double, double, double, double, double>;


    public static class Genetic
    {

        private static Random _random = new Random();

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
                throw new InvalidDataException($"Do not try to force 2 agents with reproductive isolation({a1.Length} and {a2.Length}) to cause an R-18 scene.");

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

        public static double EvaluateAgent(
            Macro agent,
            RecipeGoal goal,
            int maxSampleCount = 3000, 
            MacroLoss? loss = null,
            IDictionary<ConditionState, double>? hqRateDict = null,
            double t_fr = 0.9)
        {
            var sim = new List<(double, CraftState)>() { (1, new()) };
            double loss_fr = 0;
            foreach (var action in agent.Content)
            {
                var simRes = Simulator.SimulateSet(action, sim, goal, maxCount: maxSampleCount, hqRateDict: hqRateDict);
                sim = simRes.PossibleStates;
                if (simRes.FailedRate > t_fr)
                    ++loss_fr;
            }
            loss = loss ?? Loss.Default(goal.Progress, goal.Quality, goal.Durability, goal.CraftingPoints);
            double ans1 = 0;
            double ans2 = 0;
            foreach (var (srate, res) in sim)
            {
                var l = loss(res.Progress, res.Quality, res.Durability, res.CraftingPoints);
                ans1 += srate;
                ans2 += srate * l;
            }
            ans2 /= ans1;
            ans2 -= loss_fr * 0.01;
            return ans2;

        }

        class MacroComparer: IComparer<(Macro, double)>
        {
            public int Compare((Macro, double) x, (Macro, double) y) 
            {
                if (x.Item2 > y.Item2)
                    return -1;
                else if (x.Item2 < y.Item2)
                    return 1;
                else
                    return 0;
            }

            private MacroComparer() { }
            public static MacroComparer Instance = new MacroComparer();
        }

        public class Arguments
        {
            public int PopulationCount = 5000;
            public int MacroLength = 28;
            public RecipeGoal Goal = new();
            public int SampleRate = 5000;
            public double FailedRateThreshold = 0.85;
            public double SelectRate = 0.15;
            public double PreserveRate = 0.15;
            public double MutateRate = 0.3;
            public IDictionary<ConditionState, double> HqRateDict = Simulator.HQRateDictPlain;

            public int SelectedPopulation => (int)(SelectRate * PopulationCount);
            public int PreservedPopulation => (int)(PreserveRate * PopulationCount);
        }

        public class PopulationDump
        {
            public Dictionary<string, int> Mapping = new();
            public List<(double, List<int>)> Population = new();

            private PopulationDump() { }

            public static PopulationDump Dump(Context context)
            {
                var ans = new PopulationDump();
                int i = 0;
                foreach (var action in context.ActionSet)
                    ans.Mapping[action.ActionKey] = i++;
                foreach (var (agent, score) in context.Population)
                    ans.Population.Add((score, agent.Content.Select(x => ans.Mapping[x.ActionKey]).ToList()));

                return ans;
            }

            public List<(Macro, double)> Recover(ICollection<Action> ActionSet)
            {
                Dictionary<int, Action> tmpDict = new();
                foreach (var action in ActionSet)
                    tmpDict[Mapping[action.ActionKey]] = action;
                List<(Macro, double)> ret = Population
                    .Select(x => (new Macro(x.Item2.Select(y => tmpDict[y])), x.Item1)).ToList();
                return ret;
            }
        }

        public class Context
        {
            // arguments that needs special process when dumping
            public List<(Macro, double)> Population;
            public ICollection<Action> ActionSet;
            public MacroLoss Loss;

            // GA arguments
            public Arguments Args;

            public Context(ICollection<Action> actionSet, Arguments args)
            {
                Args = args;
                ActionSet = actionSet;
                Loss = Solver.Loss.Default(Args.Goal.Progress, Args.Goal.Quality, Args.Goal.Durability, Args.Goal.CraftingPoints);

                Population = new();
                for (var _ = 0; _ < Args.PopulationCount; ++_)
                {
                    var m = new Macro(Args.MacroLength, actionSet);
                    Population.Add((m, EvaluateAgent(m, Args.Goal, Args.SampleRate, Loss, Args.HqRateDict, Args.FailedRateThreshold)));
                }
                SortPopulation();
            } 

            public void SortPopulation()
            {
                Population.Sort(MacroComparer.Instance);
            }

            public void SelectPopulation()
            {
                SortPopulation();
                Population = Population.Take(Args.PopulationCount).ToList();
            }

            public Action GetRandomAction()
            {
                return ActionSet.ElementAt(_random.Next(ActionSet.Count()));
            }

            public void Reproduce()
            {
                // select the top and select randomly from the rest, total (SR+PR)*P
                var arr = Population
                    .Take(Args.SelectedPopulation)
                    .Concat(Population
                        .Skip(Args.SelectedPopulation)
                        .OrderBy(_ => _random.Next(Args.PopulationCount))
                        .Take(Args.PreservedPopulation)
                        )
                    .OrderBy(_ => _random.Next(Args.PopulationCount)).ToArray()!;

                // discard the unselected and reproduce them
                for (int _ = 0; _ < Args.PopulationCount - Args.PreservedPopulation - Args.SelectedPopulation; ++_)
                {
                    var agent = ReproduceAgent(
                        arr[_random.Next(arr.Length)].Item1,
                        arr[_random.Next(arr.Length)].Item1,
                        GetRandomAction,
                        Args.MutateRate);
                    Population.Add((agent, EvaluateAgent(agent, Args.Goal, Args.SampleRate, Loss, Args.HqRateDict, Args.FailedRateThreshold)));
                }
                SelectPopulation();
            }

        }

    }
}

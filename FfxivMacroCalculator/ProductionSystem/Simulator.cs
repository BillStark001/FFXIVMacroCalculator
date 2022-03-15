namespace FfxivMacroCalculator.ProductionSystem
{
    using System.Collections.ObjectModel;
    using StateSet = List<(double, CraftState)>;
    public class SimulateResult
    {
        public bool Succeed { get; } = false;
        public StateSet PossibleStates { get; } = new();

        public SimulateResult(bool succeed, StateSet states)
        {
            Succeed = succeed;
            PossibleStates = states;
        }

    }

    public class SetSimulateResult
    {
        public double SuccessRate { get; } = 0;
        public StateSet PossibleStates { get; } = new();

        public SetSimulateResult(double successRate, StateSet states)
        {
            SuccessRate = successRate;
            PossibleStates = states;
        }

    }

    public class RecipeGoal
    {
        public int GP;
        public int GQ;
        public int GE;
        public int GZ;
    }

    public static class Simulator
    {
        public static readonly double DiscardThreshold = 1e-13;
        public static readonly int MaxInnerQuiet = 10; // TODO 6.0 modification, need check

        // TODO re-measure it
        public static readonly IList<double> InnerStaticRate = new List<double>()
        {
             1.0,
             1.2388804146556174,
             1.4932383207358164,
             1.7631515041131651,
             2.0476428983395563,
             2.3481392101508662,
             2.6622366663631123,
             2.9923390401602767,
             3.3379966912545913,
             3.698682193485718,
             4.074395546853657
        }.AsReadOnly();

        public static readonly IDictionary<ConditionState, double> HQDict =
            new ReadOnlyDictionary<ConditionState, double>(new Dictionary<ConditionState, double>()
            {
                [ConditionState.Poor] = 0.5,
                [ConditionState.Normal] = 1,
                [ConditionState.Good] = 1.5,
                [ConditionState.Excellent] = 2.5
            });

        public static readonly IDictionary<ConditionState, double> HQRateDictReal =
            new ReadOnlyDictionary<ConditionState, double>(new Dictionary<ConditionState, double>()
            {
                [ConditionState.Poor] = 0,
                [ConditionState.Normal] = 0.65,
                [ConditionState.Good] = 0.25,
                [ConditionState.Excellent] = 0.1
            });

        public static readonly IDictionary<ConditionState, double> HQRateDictPlain =
            new ReadOnlyDictionary<ConditionState, double>(new Dictionary<ConditionState, double>()
            {
                [ConditionState.Poor] = 0,
                [ConditionState.Normal] = 1,
                [ConditionState.Good] = 0,
                [ConditionState.Excellent] = 0
            });

        //  = (0, 0.65, 0.25, 0.1)
        public static SimulateResult Simulate(
            this CraftState state,
            Action opr,
            RecipeGoal goal,
            double totalRate = 1,
            IDictionary<ConditionState, double>? hqRateDict = null)
        {
            if (opr.LeastLevel <= 0)
                throw new InvalidDataException();
            if (hqRateDict == null)
                hqRateDict = HQRateDictReal;

            // judge if operation is legal

            var flag = true;
            if (opr.Contains(ActionEffect.OnlyFirstStep) && state.CraftStep > 0)
                flag = false;
            if (opr.Contains(ActionEffect.OnlyGoodCondition) && state.QualityBelow(ConditionState.Good))
                flag = false;
            // element mark related skills are removed in v6.0
            if (opr.Contains(ActionEffect.OnlyNotInWasteNot) && state.ContainsState(Effect.Durability50p))
                flag = false;
            if (opr.Contains(ActionEffect.OnlyInInnerQuiet) && state.InnerQuiet == 0)
                flag = false;
            if (opr.Contains(ActionEffect.OnlyInMaxInnerQuiet) && state.InnerQuiet == MaxInnerQuiet)
                flag = false;

            // judge if the state is halting
            var flag2 = true;
            if (state.Durability >= goal.GE ||
                state.Progress >= goal.GP ||
                state.CraftingPoints + opr.DCraftPoints >= goal.GZ)
                flag2 = false;

            flag = flag && flag2;

            if (!flag)
                return new(false, new() { (totalRate, state) });

            // calculate p q e s
            double pRate = 1;
            double qRate = 1;
            double eRate = 1;

            if (opr.Contains(ActionEffect.DQualityAdd20ByInnerQuiet))
                qRate += 0.2 * (state.InnerQuiet); 

            if (state.ContainsState(Effect.Process200pForOnce)) pRate += 1;
            if (state.ContainsState(Effect.Quality200pForOnce)) qRate += 1;
            if (state.ContainsState(Effect.Process150p)) pRate += 0.5;
            if (state.ContainsState(Effect.Quality150p)) qRate += 0.5;
            if (state.ContainsState(Effect.Durability50p)) eRate -= 0.5;

            qRate *= InnerStaticRate[state.InnerQuiet];
            qRate *= HQDict[state.Condition];

            // element mark related is removed

            if (opr.Contains(ActionEffect.Effect50pIfNotEnoughDurability))
                if (opr.DDurability * eRate + state.Durability > goal.GE)
                    pRate *= 0.5;

            double sRate = opr.SuccessRate;
            if (opr.Contains(ActionEffect.MustSuccessAfterObserve))
                if (state.ContainsState(Effect.AfterObservation))
                    sRate = 1;

            int df = opr.DCraftPoints;
            if (opr.Contains(ActionEffect.CpTo18AfterBasicTouch))
                if (state.ContainsState(Effect.AfterBasicTouch))
                    df = 18;
            // TODO 6.0 modification, need check
            if (opr.Contains(ActionEffect.CpTo18AfterStandardTouch))
                if (state.ContainsState(Effect.AfterStandardTouch))
                    df = 18;

            // execute current operation

            var P_ = state.Progress + opr.DProgress * pRate;
            var Q_ = state.Quality + opr.DQuality * qRate;
            var E = state.Durability + (int)(opr.DDurability * eRate);
            var Z = state.CraftingPoints + df;

            // calculate inner quiet change after progress is changed
            // 6.0 inner quiet modification, need check
            var I = state.InnerQuiet;
            var I_ = state.InnerQuiet + opr.DInnerQuiet;
            if (opr.Contains(ActionEffect.AddInnerQuiet))
                ++I_;

            // TODO BUG!!!
            // TODO but what kind... I can't recall it
            if (state.ContainsState(Effect.DurabilityAdd5) &&
                !opr.Contains(ActionEffect.NoCraftStep) &&
                !opr.Contains(Effect.DurabilityAdd5))
            {
                if (E < goal.GE)
                    E -= 5;
            }

            if (opr.Contains(ActionEffect.ClearInnerQuiet))
                I_ = 0;
            if (opr.Contains(ActionEffect.DivideOrDoubleInnerQuiet))
            {
                I_ = I * 2;
                I = (int)(I / 2) + 1; // TODO floor and + 1?
            }


            if (I_ > MaxInnerQuiet) I_ = MaxInnerQuiet;
            if (Z < 0) Z = 0;
            if (E < 0) E = 0;

            var N = state.CraftStep;
            var L = state.Effects;
            if (!opr.Contains(ActionEffect.NoCraftStep))
            {
                ++N;
                L = state.EvolveOnce();
            }

            var L_ = new Dictionary<Effect, int>(L);
            if (L_.ContainsKey(Effect.DoNotComplete) && P_ > goal.GP)
            {
                P_ = goal.GP - 1;
                L_.Remove(Effect.DoNotComplete);
            }
            if (L_.ContainsKey(Effect.Process200pForOnce) && opr.DProgress > 0)
                L_.Remove(Effect.Process200pForOnce);

            if (L_.ContainsKey(Effect.Quality200pForOnce) && opr.DQuality > 0)
                L_.Remove(Effect.Quality200pForOnce);

            // add effects

            foreach (var (eff, oe) in opr.TimedEffects)
            {
                if (oe > 0)
                    L_[eff] = oe;
            }

            // divide into success and fail
            StateSet diverged = new()
            {
                ((1 - sRate) * totalRate, new(N, state.Progress, state.Quality, E, Z, I, state.Condition, L)),
                (sRate * totalRate, new(N, P_, Q_, E, Z, I_, state.Condition, L_)),
            };

            // divide H

            StateSet qualityAdded = new();
            foreach (var (a, b) in diverged)
            {
                if (a <= DiscardThreshold)
                    continue;
                var h = b.Condition;

                if (h == ConditionState.Excellent)
                    qualityAdded.Add((a, new(b, ConditionState.Poor)));
                else if (h != ConditionState.Normal)
                    qualityAdded.Add((a, new(b, ConditionState.Normal)));
                else
                {
                    foreach (var (qState, rate) in hqRateDict)
                    {
                        if (rate > 0 && a * rate > DiscardThreshold)
                            qualityAdded.Add((a * rate, new(b, qState)));
                    }
                }
            }

            // update L by operation
            return new(flag, qualityAdded);
        }

        public static SetSimulateResult SimulateSet(
            Action opr,
            StateSet seq,
            RecipeGoal goal,
            int maxCount = 1000,
            IDictionary<ConditionState, double>? hqRateDict = null)
        {
            if (opr.LeastLevel <= 0)
                throw new InvalidDataException();
            if (hqRateDict == null)
                hqRateDict = HQRateDictReal;

            StateSet ans = new();
            int failedCount = 0;

            foreach (var (r, state) in seq)
            {
                var simRes = state.Simulate(opr, goal, r, hqRateDict);
                ans.AddRange(simRes.PossibleStates);
                if (!simRes.Succeed)
                    ++failedCount;
            }
            if (ans.Count > maxCount)
            {
                int i = 0;
                double accRate = 0;
                foreach (var x in ans.ChooseWithProbability(maxCount, x => x.Item1))
                {
                    accRate += x.Item1;
                    ans[i++] = x;
                }
                ans = ans.Take(maxCount).Select(x => (x.Item1 / accRate, x.Item2)).ToList();
            }

            return new(
                failedCount / seq.Count,
                ans
                );
        }
    }
}

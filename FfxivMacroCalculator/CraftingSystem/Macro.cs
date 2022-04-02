using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    public class Macro
    {
        private static Random _random = new Random();

        public Action[] Content { get; }
        public int Length { get { return Content.Length; } }


        public Macro(int length, ICollection<Action> actionSet)
        {
            Content = new Action[length];
            for (int i = 0; i < length; ++i)
                Content[i] = actionSet.ElementAt(_random.Next(actionSet.Count()));
        }

        public Macro(IEnumerable<Action> actionSequence)
        {
            Content = actionSequence.ToArray();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            if (Length <= 15)
            {
                foreach (var action in Content)
                    sb.AppendLine(Format(action));
            }
            else
            {
                var currentMacro = 1;
                while ((currentMacro - 1) * 14 < Length)
                {
                    sb.AppendLine($"Macro #{currentMacro}:");
                    sb.AppendLine("-------------------------------");
                    foreach (var action in Content.Skip((currentMacro - 1) * 14).Take(14))
                        sb.AppendLine(Format(action));
                    sb.AppendLine($"/e Macro #{currentMacro} Completed. <se.1>");
                    sb.AppendLine("-------------------------------");
                    ++currentMacro;
                }
            }

            return sb.ToString();
        }

        public Macro StripFailedActions(
            RecipeGoal g,
            int maxCount = 5000,
            double threshold = 0.25, 
            IDictionary<ConditionState, double>? hqRateDict = null
            )
        {
            var (records, fr) = Evaluator.Evaluate(this, g, 1, 1, maxCount, hqRateDict);
            return new(Enumerable.Range(0, Length).Where(i => records[i + 1].Item2 < threshold).Select(i => Content[i]));
        }


        public static string Format(Action a)
        {
            return $"/ac \"{a.ActionKey}\" <wait.{a.WaitSecond}>";
        }

    }
}

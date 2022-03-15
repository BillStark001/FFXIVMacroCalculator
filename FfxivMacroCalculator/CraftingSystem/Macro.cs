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

    }
}

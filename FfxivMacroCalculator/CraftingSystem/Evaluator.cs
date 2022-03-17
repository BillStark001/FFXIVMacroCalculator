using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    public static class Evaluator
    {
        public static Table Evaluate(
            Macro m, 
            RecipeGoal g,
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

            return ans;
        }


    }
}

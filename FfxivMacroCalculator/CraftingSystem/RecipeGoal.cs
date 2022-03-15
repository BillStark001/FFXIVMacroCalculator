using FfxivMacroCalculator.CraftingSystem.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem
{
    public record RecipeGoal
    {
        public int Progress;
        public int Quality;
        public int Durability;
        public int CraftingPoints;

        public static RecipeGoal Create(PlayerInfo player, RecipeInfo recipe, Dictionary<string, FoodInfo> foods)
        {
            var cp = player.CP;
            foreach (var foodKey in player.Foods)
                if (foods.TryGetValue(foodKey, out var food))
                {
                    cp += Math.Min((int)(food.PP * cp), food.MP);
                }
            RecipeGoal goal = new RecipeGoal()
            {
                Progress = 100 * recipe.TotalProgress / recipe.DProgress,
                Quality = 100 * recipe.TotalQuality / recipe.DQuality,
                Durability = recipe.Durability,
                CraftingPoints = cp
            };

            return goal;
        }
    }

}

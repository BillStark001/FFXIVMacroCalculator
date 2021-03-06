// See https://aka.ms/new-console-template for more information
using FfxivMacroCalculator.CraftingSystem;
using FfxivMacroCalculator.CraftingSystem.Args;
using FfxivMacroCalculator.Solver;
using Newtonsoft.Json;

var playerDir = "./player.json";
var recipeDir = "./recipe.json";
var foodDir = "./food.json";

var player = JsonConvert.DeserializeObject<PlayerInfo>(File.ReadAllText(playerDir))!;
var recipe = JsonConvert.DeserializeObject<RecipeInfo>(File.ReadAllText(recipeDir))!;
var food = JsonConvert.DeserializeObject<FoodInfo>(File.ReadAllText(foodDir))!;

var goal = RecipeGoal.Create(player, recipe, new() { [food.Name] = food });
Console.WriteLine(player);
Console.WriteLine(recipe);
Console.WriteLine(goal);
var actions = player.GetActionSet();

var argsGA = new Genetic.Arguments()
{
    Goal = goal,
    MacroLength = 28, 
};

// genetic


var context = new Genetic.Context(actions, argsGA);
var pop = Genetic.PopulationDump.Dump(context);
File.WriteAllText("./iter0.json", JsonConvert.SerializeObject(pop, Formatting.Indented));

// var pop = JsonConvert.DeserializeObject<Genetic.PopulationDump>(File.ReadAllText("./iter1600.json"))!;
// var context = new Genetic.Context(actions, argsGA);
// context.Population = pop.Recover(actions);

for (int _ = 0; _ < 1601; _++)
{
    context.Reproduce();
    if (_ % 10 == 0)
        Console.WriteLine(_);
}

pop = Genetic.PopulationDump.Dump(context);
File.WriteAllText("./iter1600.json", JsonConvert.SerializeObject(pop, Formatting.Indented));


Console.WriteLine(context.Population[0].Item1);
Console.WriteLine(context.Population[0].Item1.StripFailedActions(goal, hqRateDict: Simulator.HQRateDictPlain));

var table = Evaluator.Evaluate2(context.Population[0].Item1, goal, (double) recipe.DProgress / 100, (double) recipe.DQuality / 100, 10000, Simulator.HQRateDictPlain);
Console.WriteLine(table.ToString(12));



// tree
/*
var content = new TreeSearchContent()
{
    Goal = goal, 
};
content.Search(new(), new());
*/

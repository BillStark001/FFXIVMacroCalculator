using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = FfxivMacroCalculator.CraftingSystem.Action;

namespace FfxivMacroCalculator.CraftingSystem.Args
{
    public record PlayerInfo
    {
        public string Name = "";
        public string Description = "";
        public int Level;
        public int CP;
        public int CM;
        public int CT;
        public bool Manipulation;
        public bool Specialist;
        public List<string> Foods = new();

        public ReadOnlyCollection<Action> GetActionSet() 
        {
            Dictionary<string, Action> act = new();
            var actSet = Manipulation ? Action.General.Concat(Action.WithManipulation) : Action.General;
            // TODO add specialist skills
            foreach (var action in actSet)
            {
                if (action.LeastLevel <= Level)
                {
                    if (!act.TryGetValue(action.ActionKey, out var atmp) || atmp.LeastLevel < action.LeastLevel)
                        act[action.ActionKey] = action;
                }
            }
            return act.Select(a => a.Value).ToList().AsReadOnly();
        }
    }
}

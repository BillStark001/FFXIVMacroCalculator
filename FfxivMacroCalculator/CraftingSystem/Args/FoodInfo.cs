using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.CraftingSystem.Args
{
    public record FoodInfo
    {
        public string Name = "";
        public string Description = "";
        public double PM;
        public double PT;
        public double PP;
        public int MM;
        public int MT;
        public int MP;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.ProductionSystem
{
    public partial class Effect
    {
        public static Effect Durability50p { get; } = new();
        public static Effect Process200pForOnce { get; } = new();
        public static Effect Quality200pForOnce { get; } = new();
        public static Effect AfterObservation { get; } = new();
        public static Effect Process150p { get; } = new();
        public static Effect Quality150p { get; } = new();
        public static Effect DurabilityAdd5 { get; } = new();
        public static Effect AfterBasicTouch { get; } = new();
        public static Effect AfterStandardTouch { get; } = new();
        public static Effect DoNotComplete { get; } = new();

    }
}

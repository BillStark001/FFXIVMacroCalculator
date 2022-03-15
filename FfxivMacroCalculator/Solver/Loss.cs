using FfxivMacroCalculator.CraftingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator.Solver
{
    using Action = CraftingSystem.Action;
    using FDD = Func<double, double>;
    using MacroLoss = Func<double, double, double, double, double>;
    public static class Loss
    {


        public static FDD Sigmoid(double x0 = 0, double y0 = 0, double dy = 1, double k0 = 0.25)
        {
            var dx = dy * (0.25 / k0);
            return x => (1 / (1 + Math.Exp((x0 - x) / dx))) * dy + y0;
        }

        public static MacroLoss Default(double p, double q, double e, double z)
        {
            var lp1 = Sigmoid(p * 0.75, 0, 1.5, 1.5 / p);
            FDD lp2 = x => Math.Pow(Math.Min(x / p, 1), 2.5);
            var lpup = Math.Pow(lp1(p * 1.05), 3);
            FDD lp = x => Math.Min(Math.Pow(lp1(x), 3) / lpup, 1) * 1.2;
            var lq_ = Sigmoid(q * 0.8, 0, 0.7, 5 / q);
            FDD lq = x => 0.3 * x / q + lq_(x);
            var le_ = Sigmoid(e + 7.5, 0, 1, -1 / 7.5);
            FDD le = x => le_(x) + Math.Min(0, -(x - e - 7.5));
            var lz = Sigmoid(z * 1.25, 0, 1, -2.2 / z);
            return (P, Q, E, Z) => 
                lp(P) + 
                lq(Q) * lp2(P) * 6 + 
                le(E) + 
                lz(Z) + 
                4 * Math.Min(Math.Min(lp(P), lq(Q)), Math.Min(le(E), lz(Z)));
        }


    }
}

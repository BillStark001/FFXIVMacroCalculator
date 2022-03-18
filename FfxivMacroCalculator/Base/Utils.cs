using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator
{
    public static class Utils
    {
        /// <summary>
        /// nlogn
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="newListcount"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<T> ChooseWithProbability<T>(this IEnumerable<T> elements, int newListcount, Func<T, double> p)
        {
            var ans = new List<T>();

            double[] accRate = new double[elements.Count()];
            int curElem = 0;
            foreach (var e in elements)
            {
                if (curElem == 0)
                    accRate[curElem] = p(e);
                else
                    accRate[curElem] = p(e) + accRate[curElem - 1];
                ++curElem;
            }

            Random random = new Random();
            for (var _ = 0; _ < newListcount; ++_)
            {
                var target = random.NextDouble() * accRate.Last();
                // binary search
                int low = 0, high = accRate.Length - 1;
                int searchRes = -1;
                int mid = -1;
                while (low <= high)
                {
                    mid = (high - low) / 2 + low;
                    var num = accRate[mid];
                    if (num == target)
                        break;
                    else if (num > target)
                        high = mid - 1;
                    else
                        low = mid + 1;
                }
                if (searchRes == -1)
                    searchRes = mid; // need check
                if (accRate[searchRes] > target)
                    searchRes = Math.Max(high, 0);

                ans.Append(elements.ElementAt(searchRes));
            }
            return ans;
        }
    }
}

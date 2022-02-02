using MTG_ConsoleEngine.Card_Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTG_ConsoleEngine
{
    public partial class Player
    {
        Random rand = new Random();
        public List<int[]> GetAllPossibleAttackCombinationsIndexes(List<Creature> _availableTargets)
        {
            List<int[]> result = new List<int[]>();
			//Console.WriteLine("count "+availableTargets.Count);
            if (_availableTargets.Count == 0)
                return new List<int[]> { new int[1] { -1 } };

            int[] arr = _availableTargets.Select(x=>CombatField.IndexOf(x)).ToArray();

            for(int i = arr.Length-1; i>=0; i--)
            {
                int n = _availableTargets.Count;
                int k = _availableTargets.Count- i;

                result.Add(new int[k+1]);
                foreach (int[] combo in Combinations(k, n))
                {
                 //  Console.WriteLine(string.Join(", ", combo));
                   result.Add(combo.ToArray());
                }
            }

            var trimmedArr = new List<int[]>();
            foreach (int[] myindexes in result)
            {
                if (myindexes.Length > 0)
                {
                    if (myindexes[0] == 0 && myindexes[1] == 0)
                    {
                        continue;
                    }
                }
                trimmedArr.Add(myindexes);
            }

            trimmedArr.Add(new int[1] { -1 });
            return trimmedArr;
        }
        private static IEnumerable<int[]> Combinations(int k, int n)
        {
            var result = new int[k];
            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                var index = stack.Count - 1;
                var value = stack.Pop();

                while (value <= n)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index == k)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }
    }
}

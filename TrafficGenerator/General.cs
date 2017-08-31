using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benji
{
	static class MathFunctions
	{
		public static KeyValuePair<uint, T> BinarySearchForMaxima<T>(this Func<uint, T> f, uint min, uint max) where T : IComparable<T>
		{
			Dictionary<uint, T> dict = new Dictionary<uint, T>();
			Func<uint, KeyValuePair<uint, T>> func = (uint u) => {
				if (dict.TryGetValue(u, out T output))
					return new KeyValuePair<uint, T>(u, output);
				else
					return new KeyValuePair<uint, T>(u, dict[u] = f(u));
			};

			if (min > max)
				throw new Exception("min must be less then max");

			else if (min == max)
				return func(min);



			while(max - min > 4) {
				KeyValuePair<uint, T> lowGuess = func((max + min) / 2);
				KeyValuePair<uint, T> highGuess = func(lowGuess.Key + 1);

				var compare = lowGuess.Value.CompareTo(highGuess.Value);
				if (compare < 0) {
					//low guess comes before high guess ie: low guess < high guess
					min = highGuess.Key;
				} else if (compare > 0) {
					max = lowGuess.Key;
				} else {
					return lowGuess;
				}
			}

			KeyValuePair<uint, T> maxVal = func(min);
			for(uint i = min + 1; i <= max; i++) {
				var temp = func(i);
				if (temp.Value.CompareTo(maxVal.Value) > 0)
					maxVal = temp;
			}

			return maxVal;
		}
	}
}

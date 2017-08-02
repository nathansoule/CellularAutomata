using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;

namespace TrafficGenerator
{
	public static class Extensions
	{
		/// <summary>
		/// Swaps two elements in an array
		/// </summary>
		/// <typeparam name="T">Type of elements in array</typeparam>
		/// <param name="input">Array we are swapping on</param>
		/// <param name="indexA">first index</param>
		/// <param name="indexB">second index</param>
		/// <returns>input array</returns>
		public static T[] Swap<T>(this T[] input, int indexA, int indexB)
		{
			T t = input[indexA];
			input[indexA] = input[indexB];
			input[indexB] = t;
			return input;
		}

		/// <summary>
		/// Shuffles an array completely using a given rng, array is modified during this procedure
		/// </summary>
		/// <typeparam name="T">Type of array</typeparam>
		/// <param name="input">Array to be shuffled</param>
		/// <param name="rng">Random number generator</param>
		/// <returns>the input array shuffled</returns>
		public static T[] Shuffle<T>(this T[] input, IGenerator rng)
		{
			for(int i = 0; i < input.Length - 1; i++) {
				input.Swap(i, rng.Next(i, input.Length));
			}
			return input;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficGenerator
{
	static class SlowToStop_Additions
	{
		/// <summary>
		/// Get velocity printout of a single state as a string
		/// </summary>
		/// <param name="input">SlowToStop object to print</param>
		/// <returns>String form with velocity of each car in it's position</returns>
		public static string GetStandardOutput(this SlowToStop input)
		{
			if (input.MaxVelocity < 10) {
				char[] road = new char[input.RoadLength];
				for (int i = 0; i < road.Length; i++)
					road[i] = '_';
				foreach (var car in input.Cars)
					road[car.position] = (char)('0' + car.velocity);
				return new string(road);
			} else {
				throw new NotImplementedException("Does not support cars with greater then 10 velocity");
			}
		}
		
		public static string GetPositionlesVelocities(this SlowToStop input)
		{
			if(input.MaxVelocity < 10) {
				var Cars = input.Cars;
				char[] output = new char[Cars.Length];
				for (int i = 0; i < Cars.Length; i++) {
					output[i] = (char)('0' + Cars[i].velocity);
				}
				return new string(output);
			} else {
				throw new NotImplementedException("Does not support cars with greater then 10 velocity");
			}
		}

		public static string GetHistogramOutput(this SlowToStop input)
		{
			var Cars = input.Cars;
			char[,] output = new char[input.MaxVelocity, input.NumberOfCars];
			for (var j = 0; j < input.NumberOfCars; j++) {
				var v = 0;
				for (; v < Cars[j].velocity; v++) {
					output[v, j] = '*';
				}
				for (; v < input.MaxVelocity; v++) {
					output[v, j] = ' ';
				}
			}
			StringBuilder sb = new StringBuilder();
			for(int i = output.GetLength(0)-1 ;i >= 0; i--) {
				for (var j = 0; j < output.GetLength(1); j++) {
					sb.Append(output[i, j]);
				}
				sb.Append('\n');
			}
			return sb.ToString();
		}

		public static string GetMathematicaOutput(this SlowToStop input)
		{
			StringBuilder output = new StringBuilder("{");
			foreach (var item in input.Cars) {
				output.Append($"{{{item.position},{item.velocity}}},");
			}
			if (input.NumberOfCars != 0)
				output.Length--;
			output.Append('}');

			return output.ToString();
		}

		public static string GetMathematicaOutput(this IEnumerable<SlowToStop> input)
		{
			StringBuilder output = new StringBuilder("{");
			foreach(var item in input) {
				output.Append(item.GetMathematicaOutput()).Append(',');
			}
			if (input.Any())
				output.Length--;
			output.Append('}');

			return output.ToString();
		}
	}
}

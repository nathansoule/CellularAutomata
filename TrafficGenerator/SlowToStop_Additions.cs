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
					road[i] = ' ';
				foreach (var car in input.Cars)
					road[car.position] = (char)('0' + car.velocity);
				return new string(road);
			} else {
				throw new NotImplementedException();
			}
		}
	}
}

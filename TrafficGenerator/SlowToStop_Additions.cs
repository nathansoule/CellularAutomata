﻿using System;
using System.Collections.Generic;
using System.Drawing;
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

		public static Bitmap GetBitMap(this IEnumerable<SlowToStop> input)
		{
			return input.GetBitMap(Color.Blue);
		}

		public static Bitmap GetBitMap(this IEnumerable<SlowToStop> input, Color color)
		{
			return input.GetBitMap((a, b, c, d) => color);
		}

		/// <summary>
		/// Allows you to choose color based off of uinque identifier, absolute position, velocity, and if it is doing a wait before moving
		/// </summary>
		/// <param name="input">input</param>
		/// <param name="GetColor">Function of  Position in Cars array (UID), position on road, velocity, slow to stop condition</param>
		/// <returns></returns>
		public static Bitmap GetBitMap(this IEnumerable<SlowToStop> input, Func<uint, uint, uint, bool, Color> GetColor)
		{
			if (!input.Any())
				throw new Exception("Must have some elements in input");
			Bitmap output = new Bitmap((int)input.First().RoadLength, input.Count());


			{
				int y = 0;
				foreach (var item in input) {
					var cars = item.Cars;
					for (uint i = 0; i < cars.Length; i++) {
						output.SetPixel((int)cars[i].position, y, GetColor(i, cars[i].position, cars[i].velocity, cars[i].makeVelocityOne));
					}
					y++;
				}
			}


			return output;
		}

		public static uint PasspointMeasure(this IEnumerable<SlowToStop> source)
		{
			uint count = 0;
			var lastCar = source.First().IndexOfLastCar();
			foreach (var item in source) {
				var CarArray = item.Cars;
				while (CarArray[lastCar].position + CarArray[lastCar].velocity > item.RoadLength) {
					count++;
					lastCar += CarArray.Length - 1;
					lastCar %= CarArray.Length;
				}
			}
			return count;
		}

		public static uint TotalVelocityMeasure(this IEnumerable<SlowToStop> source)
		{
			uint sum = 0;
			foreach (var item in source)
				sum += item.TotalVelocityMeasure();
			return sum;
		}

		public static uint TotalVelocityMeasure(this SlowToStop source)
		{
			uint sum = 0;
			foreach (var item in source.Cars)
				sum += item.velocity;
			return sum;
		}

		public static void GetJams(this IEnumerable<SlowToStop> source, uint maxJamSpeed = 0)
		{ 
			
		}

		public struct Jam
		{
			JamInstant initial;
			
		}

		public static List<JamInstant> GetJams(this SlowToStop source)
		{
			var CarArray = source.Cars;
			List<JamInstant> output = new List<JamInstant>();

			//create jams
			for(uint i = 0; i < CarArray.Length;) {
				uint start, length;
				for (; i < CarArray.Length && CarArray[i].velocity > 0; i++) ;
				start = i;
				for (; i < CarArray.Length && CarArray[i].velocity == 0; i++) ;
				length = i - start;
				output.Add(new JamInstant(start, length));
			}

			//Combine jam if loops around edge of road
			if (output.Count > 1) {
				if (output.Last().Last == CarArray.Length - 1 && output.First().First == 0) {
					output[0].Prefix(output.Last());
					output.RemoveAt(output.Count - 1);
				}
			}

			return output;
		}

		public struct JamInstant
		{
			public JamInstant(uint start, uint length)
			{
				First = start;
				Length = length;
			}
			public uint First { get; set; }
			public uint Length { get; set; }
			public uint Last => First + Length - 1;
			public void Prefix(JamInstant prefix)
			{
				Length += prefix.Length;
				First = prefix.First;
			}
			public void Suffix(JamInstant suffix)
			{
				Length += suffix.Length;
			}
		}
	}
}

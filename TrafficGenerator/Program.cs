using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.IO;
using Troschuetz.Random;
using Benji;
using System.Threading;

namespace TrafficGenerator
{
	static class Program
	{
		private static ThreadLocal<IGenerator> rng = new ThreadLocal<IGenerator>(() => new Troschuetz.Random.Generators.XorShift128Generator());
		public static IGenerator Rand => rng.Value;

		static void Main(string[] args)
		{
			Console.WriteLine(SlowToStop.OptimalDensity(1000));
		}
	}
}

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
using System.Diagnostics;

namespace TrafficGenerator
{
	static class Program
	{
		private static ThreadLocal<IGenerator> rng = new ThreadLocal<IGenerator>(() => new Troschuetz.Random.Generators.XorShift128Generator(12));
		public static IGenerator Rand {
			get { return rng.Value; }
			//set { rng.Value = value; }
		}

		static void Main(string[] args)
		{
			const uint numb = 1000, steps = 10000, dump = 100, simul = 100000;
			Stopwatch timer = new Stopwatch();
			timer.Start();
			Console.WriteLine(SlowToStop.OptimalDensity(numb, SlowToStop.StandardInitilizer, SlowToStop_Additions.PasspointMeasure, steps, dump, simul));
			timer.Stop();
			Console.WriteLine("Time 1: " + timer.Elapsed);
		}
	}
}

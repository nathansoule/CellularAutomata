using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.IO;

namespace TrafficGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			const uint ROADSIZE = 1000;
			var rng = new Troschuetz.Random.Generators.MT19937Generator(12);
			var cars = SlowToStop.StandardInitilizer(rng, ROADSIZE, 200);
			SlowToStop simul = new SlowToStop(rng, cars, ROADSIZE);
			foreach (var i in simul.Simulate(1000).Cast<SlowToStop>().Select(SlowToStop_Additions.GetStandardOutput))
				Console.WriteLine(i);
		}
	}
}

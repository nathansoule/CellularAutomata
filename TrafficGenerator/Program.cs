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
			var cars = new uint[20];
			for (var i = 0; i < cars.Length; i++) { cars[i] = (uint)i; }
			SlowToStop simul = new SlowToStop(new Troschuetz.Random.Generators.MT19937Generator(12), cars, 80);
			simul.Step(1000000);
			File.WriteAllText("MathematicaOutput.m", simul.Simulate(100, false).Cast<SlowToStop>().GetMathematicaOutput());
		}
	}
}

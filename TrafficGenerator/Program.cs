using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace TrafficGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var cars = new uint[1000];
			for (var i = 0; i < cars.Length; i++) { cars[i] = (uint)i; }
			SlowToStop simul = new SlowToStop(new Troschuetz.Random.Generators.MT19937Generator(12),cars, 30000);
			simul.Step(100000);
			Console.WriteLine("Done");
		}
	}
}

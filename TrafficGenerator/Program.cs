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

namespace TrafficGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(SlowToStop.OptimalDensity(1000, new Troschuetz.Random.Generators.XorShift128Generator()));
		}
	}
}

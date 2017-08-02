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
			var rng = new Troschuetz.Random.Generators.MT19937Generator(12);
			var cars = SlowToStop.StandardInitilizer(rng, 80, 30);
			SlowToStop simul = new SlowToStop(rng, cars, 80);
			simul.Simulate(10000).Cast<SlowToStop>().GetBitMap().Save("Bitmap.bmp");
		}
	}
}

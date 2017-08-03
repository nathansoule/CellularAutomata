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
			const uint ROADSIZE = 5000;
			var rng = new Troschuetz.Random.Generators.MT19937Generator(12);
			var cars = SlowToStop.StandardInitilizer(rng, ROADSIZE, 30);
			SlowToStop simul = new SlowToStop(rng, cars, ROADSIZE);
			simul.Simulate(10000).Cast<SlowToStop>().GetBitMap().Save("Bitmap.bmp");
		}
	}
}

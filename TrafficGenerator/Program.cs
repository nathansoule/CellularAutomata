using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
			const uint ROADSIZE = 2000;
			var rng = new Troschuetz.Random.Generators.MT19937Generator();
			var cars = SlowToStop.StandardInitilizer(rng, ROADSIZE, 500);
			SlowToStop simul = new SlowToStop(rng, cars, ROADSIZE);
			simul.Step(1000);
			simul.Take(6000).Cast<SlowToStop>().GetBitMap(Color.Violet).Save("bitmap.bmp");
		}
	}
}

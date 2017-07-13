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
			var slow = new SlowToStop(new uint[] { 1, 2, 3 }, 50, 5, .5, .5, new Troschuetz.Random.Generators.MT19937Generator());

			var natrualNumbers = getNaturalNumbers();

			
		}

		static IEnumerable<uint> getNaturalNumbers()
		{
			for(uint i = 0; true; i++) {
				yield return i;
			}
		}
	}
}

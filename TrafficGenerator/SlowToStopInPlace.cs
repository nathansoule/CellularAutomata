using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;

namespace TrafficGenerator
{
	public class SlowToStop_InPlace : SlowToStop
	{
		/// <summary>
		/// Copy constructor, makes complete clone
		/// </summary>
		/// <param name="other">Object to copy</param>
		public SlowToStop_InPlace(SlowToStop_InPlace other) : base(other) { }

		/// <summary>
		/// Constructor, using constant probabilities
		/// </summary>
		/// <param name="randomNumberGenerator">Random number generator</param>
		/// <param name="initialRoadState">Array of cars representing the initial road state. The internal data will be instansiated as a copy of this Array.</param>
		/// <param name="roadLength">Number of cells the road is long, or how large the circular road is.</param>
		/// <param name="maxVelocity">The maximum velocity for a car</param>
		/// <param name="faultProbability">The probability a car randomly slows down by 1</param>
		/// <param name="slowProbability">The probability a stopped car waits a step before speeding up</param>
		public SlowToStop_InPlace(IGenerator randomNumberGenerator, uint[] initialRoadState, uint roadLength, uint maxVelocity = 5, double faultProbability = .1, double slowProbability = .2) : base(randomNumberGenerator, initialRoadState, roadLength, maxVelocity, faultProbability, slowProbability) { }

		public override object Clone()
		{
			return new SlowToStop_InPlace(this);
		}

		protected override CellularAutomata Step()
		{
			base.Step();
			var offset = cars[0].position;
			for (int i = 0; i < NumberOfCars; i++) {
				cars[i].position = (cars[i].position + RoadLength - offset) % RoadLength;
			}
			return this;
		}
	}
}

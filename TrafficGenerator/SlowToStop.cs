using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;
using Troschuetz.Random.Distributions.Continuous;

namespace TrafficGenerator
{
	/// <summary>
	/// Slow to stop traffic model described: https://link.springer.com/chapter/10.1007/978-3-642-02979-0_8
	/// </summary>
	public class SlowToStop : CellularAutomata
	{
		/// <summary>
		/// random number generator for the automata
		/// </summary>
		protected ContinuousUniformDistribution rand;

		/// <summary>
		/// Copy constructor, makes complete clone
		/// </summary>
		/// <param name="other">Object to copy</param>
		public SlowToStop(SlowToStop other)
		{
			MaxVelocity = other.MaxVelocity;
			cars = (Car[])other.cars.Clone();
			RoadLength = other.RoadLength;
			FaultProbability = other.FaultProbability;
			SlowProbability = other.SlowProbability;
			Time = other.Time;
			rand = other.rand;
		}

		/// <summary>
		/// Constructor, using constant probabilities
		/// </summary>
		/// <param name="randomNumberGenerator">Random number generator</param>
		/// <param name="initialCarPositions">Array of cars representing the initial road state. The internal data will be instansiated as a copy of this Array.</param>
		/// <param name="roadLength">Number of cells the road is long, or how large the circular road is.</param>
		/// <param name="maxVelocity">The maximum velocity for a car</param>
		/// <param name="faultProbability">The probability a car randomly slows down by 1</param>
		/// <param name="slowProbability">The probability a stopped car waits a step before speeding up</param>
		public SlowToStop(IGenerator randomNumberGenerator, uint[] initialCarPositions, uint roadLength, uint maxVelocity = 5, double faultProbability = .1, double slowProbability = .2)
		{
			// add checks for safty
			//-Two cars in same location
			//-Car off road
			//-Less then 2 cars
			MaxVelocity = maxVelocity;
			rand = new ContinuousUniformDistribution(randomNumberGenerator, 0, 1);
			Time = 0;
			RoadLength = roadLength;
			cars = new Car[initialCarPositions.Length + 1];
			for (int i = 0; i < initialCarPositions.Length; i++)
				cars[i] = new Car {
					velocity = 1,
					makeVelocityOne = false,
					position = initialCarPositions[i]
				};
			FaultProbability = (a, b, c, d) => faultProbability;
			SlowProbability = (a, b, c, d) => slowProbability;
		}
		
		public struct Car
		{
			public uint velocity;
			public uint position;
			public bool makeVelocityOne;
		}

		public uint MaxVelocity { get; }
		public uint Time { get; }
		protected Car[] cars;

		/// <summary>
		/// Modifying values in this will not effect Cars inside the actual object. Reflection must be used in order to do that.
		/// </summary>
		public Car[] Cars => cars.Take((int)NumberOfCars).ToArray();
		public uint RoadLength { get; }
		public uint NumberOfCars => (uint)cars.Length-1;
		public double OverallCarDensity => NumberOfCars / (double)RoadLength;


		//Car acting on, distance to next car, next car velocity, time, output
		protected Func<Car, uint, uint, uint, double> SlowProbability;
		protected Func<Car, uint, uint, uint, double> FaultProbability;

		public uint DistanceToNextCar(int carIndex)
		{
			uint car1 = cars[carIndex].position;
			uint car2 = cars[carIndex + 1].position;
			return (RoadLength + car2 - car1) % RoadLength;
		}

		public override ModificationType ModifiesSelf => ModificationType.BecomesOutput;

		public override object Clone()
		{
			return new SlowToStop(this);
		}

		protected override CellularAutomata Step()
		{
			cars[cars.Length - 1] = cars[0];

			for (int i = 0; i < NumberOfCars; i++) {
				var distance = DistanceToNextCar(i);
				var nextVelocity = cars[i + 1].velocity;

				if (cars[i].makeVelocityOne) {
					cars[i].velocity = 1;
					cars[i].makeVelocityOne = false;

					//maybe?
					cars[i].position++;
					cars[i].position %= RoadLength;
					continue;
				} else if (cars[i].velocity == 0 && distance > 1) {
					if (rand.NextDouble() < SlowProbability(cars[i], distance, nextVelocity, Time)) {
						cars[i].makeVelocityOne = true;
					} else {
						cars[i].velocity = 1;
					}
				} else if (distance <= cars[i].velocity) {
					if (cars[i].velocity <= 2 || cars[i].velocity < nextVelocity) {
						cars[i].velocity = distance - 1;
					} else {
						cars[i].velocity = Math.Min(distance - 1, cars[i].velocity - 2);
					}
				} else if(cars[i].velocity < distance && distance < cars[i].velocity * 2) {
					if (cars[i].velocity >= nextVelocity + 4) {
						cars[i].velocity -= 2;
					} else if (cars[i].velocity >= nextVelocity + 2) {
						cars[i].velocity--;
					} else {
						if (cars[i].velocity < MaxVelocity && distance > cars[i].velocity + 1)
							cars[i].velocity++;
					}
				} else {
					if (cars[i].velocity < MaxVelocity && distance > cars[i].velocity + 1)
						cars[i].velocity++;
				}

				if (cars[i].velocity > 0)
					if (rand.NextDouble() < FaultProbability(cars[i], distance, nextVelocity, Time))
						cars[i].velocity--;

				cars[i].position += cars[i].velocity;
				cars[i].position %= RoadLength;
			}

			return this;
		}
	}
}
using Benji;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;
using Troschuetz.Random.Distributions.Continuous;
using static TrafficGenerator.Program;

namespace TrafficGenerator
{
	/// <summary>
	/// Slow to stop traffic model described: https://link.springer.com/chapter/10.1007/978-3-642-02979-0_8
	/// </summary>
	public class SlowToStop : CellularAutomata
	{

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
		public SlowToStop(uint[] initialCarPositions, uint roadLength, uint maxVelocity = 5, double faultProbability = .1, double slowProbability = .2)
		{
			// add checks for safty
			//-Two cars in same location
			//-Car off road
			//-Less then 2 cars
			MaxVelocity = maxVelocity;
			Time = 0;
			RoadLength = roadLength;
			cars = new Car[initialCarPositions.Length + 1];
			Array.Sort(initialCarPositions);
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
		public double CarDensity => NumberOfCars / (double)RoadLength;


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
					if (Rand.NextDouble() < SlowProbability(cars[i], distance, nextVelocity, Time)) {
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
					if (Rand.NextDouble() < FaultProbability(cars[i], distance, nextVelocity, Time))
						cars[i].velocity--;

				cars[i].position += cars[i].velocity;
				cars[i].position %= RoadLength;
			}

			return this;
		}

		public int IndexOfLastCar()
		{
			//there is a more efficent way...
			//implement when I feel like it?
			int index = 0;
			for (int i = 1; i < NumberOfCars; i++)
				if (cars[i].position > cars[index].position)
					index = i;
			return index;

		}

		/// <summary>
		/// Generates standard initial positons for SlowToStop setup
		/// </summary>
		/// <param name="roadLength">Length of the road</param>
		/// <param name="numberOfCars">Number of cars on the road</param>
		/// <returns>Starting positions of cars</returns>
		public static uint[] StandardInitilizer(uint roadLength, uint numberOfCars)
		{
			uint[] positions = new uint[roadLength];
			for (uint i = 0; i < positions.Length; i++) {
				positions[i] = i;
			}
			positions.Shuffle();
			var output = new uint[numberOfCars];
			Array.Copy(positions, output, output.Length);
			return output;
		}

		public static uint[] StandardInitilizer(uint roadLength, double ratioOfCars)
		{
			return StandardInitilizer(roadLength, (uint)(ratioOfCars * roadLength));
		}

		public static double OptimalDensity(uint roadLength, Func<uint, uint, uint[]> initializer, Func<IEnumerable<SlowToStop>, uint> measure, uint steps = 5000, uint throwAwaySteps = 1000, uint simulations = 100)
		{
			if (roadLength < 2) {
				throw new Exception($"Have a bigger road length than 2 please, {roadLength} is to small.");
			}

			//generate road
			Func<uint, uint> f = (uint cars) => {
				uint sum = 0;
				for (uint i = 0; i < simulations; i++)
					sum += GetThrouputMeasure(cars, roadLength, steps, throwAwaySteps, StandardInitilizer, SlowToStop_Additions.PasspointMeasure);
				return sum;
			};

			Func<uint, uint> parallelF = (uint cars) => {
				ConcurrentBag<int> results = new ConcurrentBag<int>();
				Parallel.For(0, (int)simulations, (int i) => {
					results.Add((int)GetThrouputMeasure(cars, roadLength, steps, throwAwaySteps, StandardInitilizer, SlowToStop_Additions.PasspointMeasure));
				});
				return (uint)results.Sum();
			};

			//do binary search, start with 
			var output = f.BinarySearchForMaxima(2, roadLength).Key;


			simulations = 1;
			for (uint i = 2; i < roadLength; i++)
				Console.WriteLine($"{i}\t:\t{f(i)}");


			return output;
		}

		public static uint GetThrouputMeasure(uint cars, uint roadLength, uint steps, uint throwAwaySteps, Func<uint,uint,uint[]> initializer, Func<IEnumerable<SlowToStop>, uint> measure)
		{
			SlowToStop road = new SlowToStop(initializer(roadLength, cars), roadLength);
			road.Step(throwAwaySteps);

			return measure(road.Take((int)steps).Cast<SlowToStop>());
		}
	}
}
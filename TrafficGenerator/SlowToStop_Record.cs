﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;
using Troschuetz.Random.Distributions.Continuous;

namespace TrafficGenerator
{
	public class Time_Series
	{
		public uint[] all_velocity;
		public uint avg_velocity;

		public Time_Series(uint size)
		{
			all_velocity = new uint[size];
			avg_velocity = 0;
		}
	}
	/// <summary>
	/// Slow to stop traffic model described: https://link.springer.com/chapter/10.1007/978-3-642-02979-0_8
	/// </summary>
	public class SlowToStop_Record : SlowToStop
	{
		/// <summary>
		/// random number generator for the automata
		/// </summary>
		protected ContinuousUniformDistribution rand;

		/// <summary>
		/// Copy constructor, makes complete clone
		/// </summary>
		/// <param name="other">Object to copy</param>
		public SlowToStop_Record(SlowToStop_Record other) : base(other)	{	}

		/// <summary>
		/// Constructor, using constant probabilities
		/// </summary>
		/// <param name="randomNumberGenerator">Random number generator</param>
		/// <param name="initialCarPositions">Array of cars representing the initial road state. The internal data will be instansiated as a copy of this Array.</param>
		/// <param name="roadLength">Number of cells the road is long, or how large the circular road is.</param>
		/// <param name="maxVelocity">The maximum velocity for a car</param>
		/// <param name="faultProbability">The probability a car randomly slows down by 1</param>
		/// <param name="slowProbability">The probability a stopped car waits a step before speeding up</param>
		public SlowToStop_Record(IGenerator randomNumberGenerator, uint[] initialCarPositions, uint roadLength, uint maxVelocity = 5, double faultProbability = .1, double slowProbability = .2) : base(randomNumberGenerator, initialCarPositions, roadLength, maxVelocity, faultProbability, slowProbability)	{ }
		

		
		public List<Time_Series> data;

		

		public override object Clone()
		{
			return new SlowToStop_Record(this);
		}

		protected override CellularAutomata Step()
		{
			cars[cars.Length - 1] = cars[0];

			Time_Series tmp_data = new Time_Series(NumberOfCars);
			
			for (int i = 0; i < NumberOfCars; i++)
			{
				var distance = DistanceToNextCar(i);
				var nextVelocity = cars[i + 1].velocity;

				if (cars[i].makeVelocityOne)
				{
					cars[i].velocity = 1;
					cars[i].makeVelocityOne = false;

					//maybe?
					cars[i].position++;
					cars[i].position %= RoadLength;
					continue;
				}
				else if (cars[i].velocity == 0 && distance > 1)
				{
					if (rand.NextDouble() < SlowProbability(cars[i], distance, nextVelocity, Time))
					{
						cars[i].makeVelocityOne = true;
					}
					else
					{
						cars[i].velocity = 1;
					}
				}
				else if (distance <= cars[i].velocity)
				{
					if (cars[i].velocity <= 2 || cars[i].velocity < nextVelocity)
					{
						cars[i].velocity = distance - 1;
					}
					else
					{
						cars[i].velocity = Math.Min(distance - 1, cars[i].velocity - 2);
					}
				}
				else if (cars[i].velocity < distance && distance < cars[i].velocity * 2)
				{
					if (cars[i].velocity >= nextVelocity + 4)
					{
						cars[i].velocity -= 2;
					}
					else if (cars[i].velocity >= nextVelocity + 2)
					{
						cars[i].velocity--;
					}
					else
					{
						if (cars[i].velocity < MaxVelocity && distance > cars[i].velocity + 1)
							cars[i].velocity++;
					}
				}
				else
				{
					if (cars[i].velocity < MaxVelocity && distance > cars[i].velocity + 1)
						cars[i].velocity++;
				}

				if (cars[i].velocity > 0)
					if (rand.NextDouble() < FaultProbability(cars[i], distance, nextVelocity, Time))
						cars[i].velocity--;

				cars[i].position += cars[i].velocity;
				cars[i].position %= RoadLength;
				tmp_data.all_velocity[i] = cars[i].velocity;
			}

			for(int i = 0; i < NumberOfCars; i++)
			{
				tmp_data.avg_velocity += tmp_data.all_velocity[i];
			}
			tmp_data.avg_velocity /= NumberOfCars;
			data.Add(tmp_data);

			return this;
		}
	}

}

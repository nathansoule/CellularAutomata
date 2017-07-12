using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TrafficGenerator
{
	/// <summary>
	/// Base class for all cellular automata
	/// </summary>
	public abstract class CellularAutomata : ICloneable
	{
		/// <summary>
		/// Iterates one step and returns new state, may modify self based on implementation
		/// </summary>
		/// <returns>Cellular Automata one step later</returns>
		protected abstract CellularAutomata Step();

		/// <summary>
		/// Iterate a number of steps and returns.
		/// </summary>
		/// <param name="steps">Number of steps to interate</param>
		/// <returns>The new state of the automata</returns>
		public CellularAutomata Step(uint steps = 1)
		{
			CellularAutomata current = this;
			while (steps-- > 0) {
				current = current.Step();
			}
			return current;
		}

		/// <summary>
		/// Runs a simulation with a specified number of steps
		/// </summary>
		/// <param name="steps">Number of steps to run for.</param>
		/// <param name="includeOriginal">If the original value should be included in the output data</param>
		/// <returns>An array containing the state of the automata at each step</returns>
		public CellularAutomata[] Simulate(uint steps, bool includeOriginal = true)
		{
			int index;
			CellularAutomata[] output;
			if (includeOriginal) {
				output = new CellularAutomata[steps + 1];
				output[0] = this;
				index = 1;
			} else {
				output = new CellularAutomata[steps];
				index = 0;
			}

			CellularAutomata current = this;

			switch (ModifiesSelf) {
				case ModificationType.Other:
				case ModificationType.BecomesOutput:
					for(; index < output.Length; index++) {
						current = current.Step();
						output[index] = (CellularAutomata)current.Clone();
					}
					break;
				case ModificationType.None:
					for (; index < output.Length; index++) {
						current = current.Step();
						output[index] = current;
					}
					break;
			}
			return output;
		}

		/// <summary>
		/// Creates a complete clone of the Cellular Automata
		/// </summary>
		/// <returns>A copy of the Cellular Automata as an object</returns>
		public abstract object Clone();

		
		/// <summary>
		/// Enum to describe how the Step function modifies the CellularAutomata
		/// </summary>
		public enum ModificationType { None, BecomesOutput, Other }

		/// <summary>
		/// How the Step function modifies the CellularAutomata
		/// </summary>
		public abstract ModificationType ModifiesSelf { get; }
	}
}

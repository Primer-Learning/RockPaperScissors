using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using PrimerTools;

[Tool]
public partial class SimRunner : Node3D
{
	private bool _run = true;
	[Export] private bool Run {
		get => _run;
		set {
			var oldRun = _run;
			_run = value;
			if (_run && !oldRun && Engine.IsEditorHint()) { // Avoids running on build
				var stopwatch = Stopwatch.StartNew();
				// for (int i = 0; i < 10; i++)
				// {
					Initialize();
					Simulate();
				// }
				stopwatch.Stop();
				GD.Print("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
				// PrintResults();
			}
		}
	}

	private Rng _rng;
	
	// Final saved results. What do they look like?
	// By day
	// - Array of blob strategies
	// - Array of blob parents
	
	private const int NumDays = 100;
	private const int InitialBlobCount = 3200;
	// private const int NumTrees = 150;

	private static readonly int[][] StrategyResults = new int[NumDays + 1][];
	private static readonly int[][] ParentResults = new int[NumDays + 1][];

	// Reward matrix
	private const float WinMagnitude = 1.0f;
	private float[,] _rewardMatrix = new float[3, 3] {
		{ 1, 1 - WinMagnitude, 1 + WinMagnitude }, // Rock rewards
		{ 1 + WinMagnitude, 1, 1 - WinMagnitude }, // Paper rewards   
		{ 1 - WinMagnitude, 1 + WinMagnitude, 1 }  // Scissors rewards
	};

	// Daily blob arrays
	private int[] _blobStrategies;
	private float[] _blobRewards;
	private void Initialize()
	{
		// rng = new Rng(0);
		_rng = new Rng(System.Environment.TickCount);
		
		_blobStrategies = new int[InitialBlobCount];
		for (var i = 0; i < InitialBlobCount; i++)
		{
			// _blobStrategies[i] = i % 3; // Even mix
			// _blobStrategies[i] = rng.RangeInt(3); // Random mix
			
			var mapIndex = i % 4;
			_blobStrategies[i] = mapIndex > 2 ? 0 : mapIndex;
		}
		ParentResults[0] = new int[InitialBlobCount];
		StrategyResults[0] = _blobStrategies;
	}

	private void Simulate()
	{
		for (var i = 1; i <= NumDays; i++)
		{
			// To pair blobs randomly, we need to shuffle the array.
			// But we need to keep track of the original indices to record the parents,
			// so make this mapping array.
			var mappingArray = new int[_blobStrategies.Length];
			for (var j = 0; j < mappingArray.Length; j++)
			{
				mappingArray[j] = j;
			}
			mappingArray = mappingArray.Shuffle(rng: _rng).ToArray();
			
			var shuffledStrategies = new int[_blobStrategies.Length];
			for (var j = 0; j < _blobStrategies.Length; j++)
			{
				shuffledStrategies[j] = _blobStrategies[mappingArray[j]];
			}
			
			_blobRewards = new float[shuffledStrategies.Length];
			// Determine rewards based on matrix and strategies
			for (var j = 0; j < shuffledStrategies.Length; j += 2)
			{
				var strategy1 = shuffledStrategies[j];
				var strategy2 = shuffledStrategies[j + 1];
				_blobRewards[j] = _rewardMatrix[strategy1, strategy2];
				_blobRewards[j + 1] = _rewardMatrix[strategy2, strategy1];
			}
			
			var childStrategies = new List<int>();
			var parentIndices = new List<int>();
			for (var k = 0; k < shuffledStrategies.Length; k++)
			{
				var reward = _blobRewards[k];
				var wholeReward = Mathf.FloorToInt(reward);
				var fractionReward = reward - wholeReward;
				
				for (var j = 0; j < wholeReward; j++)
				{
					childStrategies.Add(shuffledStrategies[k]);
					// TODO: Check whether the parents are assigned correctly
					// once it's animation time. Could be a bug here.
					parentIndices.Add(Array.IndexOf(mappingArray, k));
				}
				if (_rng.RangeFloat(1) < fractionReward)
				{
					childStrategies.Add(shuffledStrategies[k]);
					parentIndices.Add(Array.IndexOf(mappingArray, k));
				}
			}
			
			// Store the new strategies as tomorrow's strategies
			_blobStrategies = childStrategies.ToArray();
			
			// Record the results
			StrategyResults[i] = _blobStrategies;
			ParentResults[i] = mappingArray;
		}
	}

	private void PrintResults()
	{
		// var index = 0;
		// foreach (var strategies in StrategyResults)
		foreach (var (index, strategies) in StrategyResults.WithIndex())
		{
			GD.Print(strategies.Length);
			GD.Print($"Day {index}");
			var fractionRock = strategies.Count(s => s == 0) / (float)strategies.Length;
			var fractionPaper = strategies.Count(s => s == 1) / (float)strategies.Length;
			var fractionScissors = strategies.Count(s => s == 2) / (float)strategies.Length;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");

			// index++;	
		}
	}
}

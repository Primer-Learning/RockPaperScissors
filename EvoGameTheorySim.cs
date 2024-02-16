using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using PrimerTools;
using System.Linq;
using EntityID = System.Int32;
using ParentID = System.Int32;

[Tool]
public partial class EvoGameTheorySim : Node
{
	#region Running toggle
    private bool _run = true;
	[Export] private bool Run {
		get => _run;
		set {
			var oldRun = _run;
			_run = value;
			if (_run && !oldRun && Engine.IsEditorHint()) { // Avoids running on build
				// Loop for speed testing
				// var stopwatch = Stopwatch.StartNew();
				// for (var i = 0; i < 100; i++)
				// {
				// }
				// stopwatch.Stop();
				// GD.Print("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
				Initialize();
				Simulate();
				PrintResults();
			}
		}
	}
	#endregion

	#region Entity Registry
	private struct EntityRegistry
	{
		private static EntityID _nextId;
		public static readonly List<RPSGame.Strategy> Strategies = new();
		public static readonly List<ParentID> Parents = new();
		
		public static EntityID CreateBlob(RPSGame.Strategy strategy, EntityID parent)
		{
			var id = _nextId++;
			Strategies.Add(strategy);
			Parents.Add(parent);
			return id;
		}
	}
	#endregion
	
	public static class RPSGame
	{
		public enum Strategy
		{
			Rock,
			Paper,
			Scissors
		}
		
		public static (float reward1, float reward2) GetRewards(Strategy strategy1, Strategy strategy2)
		{
			return (RewardMatrix[(int)strategy1, (int)strategy2], RewardMatrix[(int)strategy2, (int)strategy1]);
		}
		
		private const float WinMagnitude = 0.5f;
		private static readonly float[,] RewardMatrix = new float[3, 3] {
			{ 1, 1 - WinMagnitude, 1 + WinMagnitude }, // Rock rewards
			{ 1 + WinMagnitude, 1, 1 - WinMagnitude }, // Paper rewards   
			{ 1 - WinMagnitude, 1 + WinMagnitude, 1 }  // Scissors rewards
		};
	}
	
	#region Parameters
	private Rng _rng;
	[Export] public int Seed = -1;
	private const int NumDays = 20;
	private const int InitialBlobCount = 32;
	private const int NumTrees = 150;
	
	#endregion

	#region Simulation
	private static readonly List<EntityID>[] EntitiesByDay = new List<EntityID>[NumDays + 1];
	private void Initialize()
	{
		_rng = new Rng(Seed == -1 ? System.Environment.TickCount : Seed);
		
		var blobIDs = new List<EntityID>();
		for (var i = 0; i < InitialBlobCount; i++)
		{
			// var strategy = new Game.Player((Game.Strategy)(i % 3)); // Even mix
			// var strategy = new Game.Player((Game.Strategy)_rng.RangeInt(3)); // Random mix
			var strategy = (i % 4) switch
			{
				0 => RPSGame.Strategy.Rock,
				1 => RPSGame.Strategy.Paper,
				2 => RPSGame.Strategy.Scissors,
				3 => RPSGame.Strategy.Rock,
				_ => throw new System.Exception("This should never happen")
			};
			
			blobIDs.Add(EntityRegistry.CreateBlob(
				strategy,
				-1
			));
		}
		EntitiesByDay[0] = blobIDs;
	}
	private void Simulate()
	{
		for (var i = 1; i <= NumDays; i++)
		{
			var shuffledParents = EntitiesByDay[i - 1].ShuffleToList(rng: _rng).ToArray();

			var numGames = shuffledParents.Length - NumTrees;
			numGames = Mathf.Max(numGames, 0);
			numGames = Mathf.Min(numGames, NumTrees);
			
			var dailyChildren = new List<EntityID>();
			for (var j = 0; j < numGames * 2; j += 2)
			{
				var parent1Strategy = EntityRegistry.Strategies[shuffledParents[j]];
				var parent2Stategy = EntityRegistry.Strategies[shuffledParents[j+1]];
				
				var (reward1, reward2) = RPSGame.GetRewards(parent1Strategy, parent2Stategy);
				
				for (var k = 0; k < GetOffspringCount(reward1); k++)
				{
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parent1Strategy,
							shuffledParents[j]
						)
					);
				}
				for (var k = 0; k < GetOffspringCount(reward2); k++)
				{
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parent2Stategy,
							shuffledParents[j]
						)
					);
				}
			}

			for (var j = numGames * 2; j < shuffledParents.Length; j += 1)
			{
				if (numGames < NumTrees)
				{
					var parentStrategy = EntityRegistry.Strategies[shuffledParents[j]];
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parentStrategy,
							shuffledParents[j]
						)
					);
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parentStrategy,
							shuffledParents[j]
						)
					);
				}
				// Else they die, which is just not reproducing, so do nothing
			}
			

			
			EntitiesByDay[i] = dailyChildren;
		}
	}
	private int GetOffspringCount(float reward)
	{
		var offspringCount = 0;
		var wholeReward = Mathf.FloorToInt(reward);
		var fractionReward = reward - wholeReward;
		
		for (var j = 0; j < wholeReward; j++)
		{
			offspringCount++;
		}
		if (_rng.RangeFloat(1) < fractionReward)
		{
			offspringCount++;
		}
		
		return offspringCount;
	}
	#endregion

	private void PrintResults()
	{
		foreach (var (day, entitiesToday) in EntitiesByDay.WithIndex())
		{
			GD.Print($"Day {day} total: {entitiesToday.Count}");
			var fractionRock = entitiesToday.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Rock) / (float)entitiesToday.Count;
			var fractionPaper = entitiesToday.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Paper) / (float)entitiesToday.Count;
			var fractionScissors = entitiesToday.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Scissors) / (float)entitiesToday.Count;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
		}
	}
}
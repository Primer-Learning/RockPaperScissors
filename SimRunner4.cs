using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using PrimerTools;
using System.Linq;
using EntityID = System.Int32;
using ParentID = System.Int32;

[Tool]
public partial class SimRunner4 : Node
{
	#region Running toggle
    private bool _run = true;
	[Export] private bool Run {
		get => _run;
		set {
			var oldRun = _run;
			_run = value;
			if (_run && !oldRun && Engine.IsEditorHint()) { // Avoids running on build
				var stopwatch = Stopwatch.StartNew();
				for (var i = 0; i < 100; i++)
				{
					Initialize();
					Simulate();
				}
				stopwatch.Stop();
				GD.Print("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
				// PrintResults();
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
		
		private const float WinMagnitude = 1.0f;
		private static readonly float[,] RewardMatrix = new float[3, 3] {
			{ 1, 1 - WinMagnitude, 1 + WinMagnitude }, // Rock rewards
			{ 1 + WinMagnitude, 1, 1 - WinMagnitude }, // Paper rewards   
			{ 1 - WinMagnitude, 1 + WinMagnitude, 1 }  // Scissors rewards
		};
	}
	
	#region Parameters
	private Rng _rng;
	[Export] public int Seed = -1;
	private const int NumDays = 100;
	private const int InitialBlobCount = 3200;
	// private const int NumTrees = 150;
	
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
			
			// var childEntities = new List<EntityID>();
			var dailyChildren = new List<EntityID>();
			for (var j = 0; j < shuffledParents.Length; j += 2)
			{
				var parent1 = EntityRegistry.Strategies[shuffledParents[j]];
				var parent2 = EntityRegistry.Strategies[shuffledParents[j+1]];
				
				var (reward1, reward2) = RPSGame.GetRewards(parent1, parent2);
				
				for (var k = 0; k < GetOffspringCount(reward1); k++)
				{
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parent1, // weird name, but it's just the strategy
							shuffledParents[j]
						)
					);
				}
				for (var k = 0; k < GetOffspringCount(reward2); k++)
				{
					dailyChildren.Add(
						EntityRegistry.CreateBlob(
							parent2, // weird name, but it's just the strategy
							shuffledParents[j]
						)
					);
				}
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
		foreach (var (day, entities) in EntitiesByDay.WithIndex())
		{
			// GD.Print(entities.Length);
			GD.Print($"Day {day}");
			var fractionRock = entities.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Rock) / (float)entities.Count;
			var fractionPaper = entities.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Paper) / (float)entities.Count;
			var fractionScissors = entities.Count(s => EntityRegistry.Strategies[s] == RPSGame.Strategy.Scissors) / (float)entities.Count;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
		}
	}
}
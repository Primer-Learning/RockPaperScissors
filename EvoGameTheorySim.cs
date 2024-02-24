using System.Collections.Generic;
using Godot;
using PrimerTools;
using System.Linq;
using Primer;
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
				RunSim();
				PrintResults();
			}
		}
	}
	#endregion

	public void RunSim()
	{
		Initialize();
		Simulate();
	}

	#region Entity Registry
	public EntityRegistry Registry = new();
	public class EntityRegistry
	{
		private EntityID _nextId;
		public readonly List<RPSGame.Strategy> Strategies = new();
		public readonly List<ParentID> Parents = new();

		public EntityID CreateBlob(RPSGame.Strategy strategy, EntityID parent)
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
			return (RewardMatrix[(int)strategy1, (int)strategy2], RewardMatrix[(int)strategy2, (int)strategy1] - GlobalCost);
		}
		
		private const float GlobalCost = 0.0f;
		
		private const float WinMagnitude = 0.2f;
		private const float TieCost = 0.0f;
		private static readonly float[,] RewardMatrix = new float[3, 3] {
			{ 1 - TieCost, 1 - WinMagnitude, 1 + 1 * WinMagnitude }, // Rock rewards
			{ 1 + WinMagnitude, 1 - TieCost, 1 - WinMagnitude }, // Paper rewards   
			{ 1 - WinMagnitude, 1 + WinMagnitude, 1 - TieCost}  // Scissors rewards
		};
	}
	
	#region Parameters
	private Rng _rng;
	[Export] public int Seed = -1;
	public int NumDays = 20;
	public int InitialBlobCount = 32;
	public int NumTrees = 50;
	
	#endregion

	#region Simulation

	public List<EntityID>[] EntitiesByDay;// = new List<EntityID>[21];
	public List<EntityID>[] ShuffledParents;
     	private void Initialize()
     	{
     		EntitiesByDay = new List<EntityID>[NumDays + 1];
     		
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
     			
     			blobIDs.Add(Registry.CreateBlob(
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
			// Parents are already shuffled, since they were shuffled at the end of the last iteration of the loop
			var shuffledParents = EntitiesByDay[i - 1];

			var numGames = shuffledParents.Count - NumTrees;
			numGames = Mathf.Max(numGames, 0);
			numGames = Mathf.Min(numGames, NumTrees);
			
			var dailyChildren = new List<EntityID>();
			for (var j = 0; j < numGames * 2; j += 2)
			{
				var parent1Strategy = Registry.Strategies[shuffledParents[j]];
				var parent2Stategy = Registry.Strategies[shuffledParents[j+1]];
				
				var (reward1, reward2) = RPSGame.GetRewards(parent1Strategy, parent2Stategy);
				
				for (var k = 0; k < GetOffspringCount(reward1); k++)
				{
					dailyChildren.Add(
						Registry.CreateBlob(
							parent1Strategy,
							shuffledParents[j]
						)
					);
				}
				for (var k = 0; k < GetOffspringCount(reward2); k++)
				{
					dailyChildren.Add(
						Registry.CreateBlob(
							parent2Stategy,
							shuffledParents[j]
						)
					);
				}
			}

			for (var j = numGames * 2; j < shuffledParents.Count; j += 1)
			{
				if (numGames < NumTrees)
				{
					var parentStrategy = Registry.Strategies[shuffledParents[j]];
					dailyChildren.Add(
						Registry.CreateBlob(
							parentStrategy,
							shuffledParents[j]
						)
					);
					dailyChildren.Add(
						Registry.CreateBlob(
							parentStrategy,
							shuffledParents[j]
						)
					);
				}
				// Else they die, which is just not reproducing, so do nothing
			}
			
			// Shuffle at the end of the loop so entities are sorted by tree for the next day
			EntitiesByDay[i] = dailyChildren.ShuffleToList(rng: _rng);
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
			var fractionRock = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Rock) / (float)entitiesToday.Count;
			var fractionPaper = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Paper) / (float)entitiesToday.Count;
			var fractionScissors = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Scissors) / (float)entitiesToday.Count;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
		}
	}
	public Vector3[] GetStrategyFrequenciesByDay()
	{
		var frequencies = new Vector3[NumDays + 1];
		foreach (var (day, entitiesToday) in EntitiesByDay.WithIndex())
		{
			var fractionRock = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Rock) / (float)entitiesToday.Count;
			var fractionPaper = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Paper) / (float)entitiesToday.Count;
			var fractionScissors = entitiesToday.Count(s => Registry.Strategies[s] == RPSGame.Strategy.Scissors) / (float)entitiesToday.Count;
			frequencies[day] = new Vector3(fractionRock, fractionPaper, fractionScissors);
		}
		return frequencies;
	}
	
	public Dictionary<RPSGame.Strategy, Color> StrategyColors = new()
	{
		{ RPSGame.Strategy.Rock, PrimerColor.red },
		{ RPSGame.Strategy.Paper, PrimerColor.blue },
		{ RPSGame.Strategy.Scissors, PrimerColor.green }
	};
}
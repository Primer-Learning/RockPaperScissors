using System;
using System.Collections.Generic;
using Godot;
using PrimerTools;
using System.Linq;
using EntityID = System.Int32;
using ParentID = System.Int32;
using StrategyID = System.Int32;

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
		public readonly List<StrategyID> Strategies = new();
		public readonly List<ParentID> Parents = new();

		public EntityID CreateBlob(StrategyID strategy, EntityID parent)
		{
			var id = _nextId++;
			Strategies.Add(strategy);
			Parents.Add(parent);
			return id;
		}
	}
	#endregion
	
	public class RPSGame
	{
		public RPSGame(float winMagnitude, float tieCost)
		{
			this.winMagnitude = winMagnitude;
			this.tieCost = tieCost;
		}
		public RPSGame(){ GD.PushWarning("Creating default RPSGame.");}
		
		public enum Strategy
		{
			Rock,
			Paper,
			Scissors
		}

		public Strategy[] StrategyOptions = Enum.GetValues<Strategy>();
		
		public (float reward1, float reward2) GetRewards(int strategy1, int strategy2)
		{
			return (RewardMatrix[strategy1, strategy2] - GlobalCost, RewardMatrix[strategy2, strategy1] - GlobalCost);
		}
		
		private const float GlobalCost = 0.0f;
		
		private readonly float winMagnitude = 1f;
		private readonly float tieCost = 0.0f;
		private float[,] RewardMatrix => new float[3, 3] {
			{ 1 - tieCost, 1 - winMagnitude, 1 + 1 * winMagnitude }, // Rock rewards
			{ 1 + winMagnitude, 1 - tieCost, 1 - winMagnitude }, // Paper rewards   
			{ 1 - winMagnitude, 1 + winMagnitude, 1 - tieCost}  // Scissors rewards
		};
	}
	
	#region Parameters
	private Rng _rng;
	[Export] public int Seed = -1;
	public int NumDays = 20;
	public int InitialBlobCount = 32;
	public int NumTrees = 50;
	public float MutationRate = 0;
	public float[] InitialAlleleFrequencies;
	#endregion

	#region Simulation

	public List<EntityID>[] EntitiesByDay;// = new List<EntityID>[21];
	public List<EntityID>[] ShuffledParents;
	public RPSGame RpsGame;
	private void Initialize()
	{
     	_rng = new Rng(Seed == -1 ? System.Environment.TickCount : Seed);
		RpsGame ??= new RPSGame();
		
		if (InitialAlleleFrequencies is not { Length: 3 })
		{
			GD.PushWarning("No valid initial frequencies defined. Using even distribution.");
			InitialAlleleFrequencies = new[] { 1 / 3f, 1 / 3f, 1 / 3f };
		}

		var sum = InitialAlleleFrequencies.Sum();
		if ( Mathf.Abs(sum - 1) > 0.001f ) GD.PushWarning("Initial allele frequencies don't add to 1. Normalizing.");
		InitialAlleleFrequencies = InitialAlleleFrequencies.Select(x => x / sum).ToArray();
		
        EntitiesByDay = new List<EntityID>[NumDays + 1];
     	
     	var blobIDs = new List<EntityID>();
        var currentStratIndex = -1;
        var currentStratTargetTotal = 0;
        for (var i = 0; i < InitialBlobCount; i++)
        {
	        // while loop is here so we skip alleles with zero initial frequencies
	        while (i >= currentStratTargetTotal)
	        {
		        if (InitialAlleleFrequencies.Length == currentStratIndex + 1) break; // Prevent incrementing too far.
		        currentStratIndex++;
		        currentStratTargetTotal = Mathf.RoundToInt(currentStratTargetTotal + InitialAlleleFrequencies[currentStratIndex] * InitialBlobCount);
	        }
     		
     		blobIDs.Add(Registry.CreateBlob(
     			currentStratIndex,
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
				var parent2Strategy = Registry.Strategies[shuffledParents[j+1]];
				
				var (reward1, reward2) = RpsGame.GetRewards(parent1Strategy, parent2Strategy);
				
				for (var k = 0; k < GetOffspringCount(reward1); k++)
				{
					dailyChildren.Add(Reproduce(shuffledParents[j], parent1Strategy));
				}
				for (var k = 0; k < GetOffspringCount(reward2); k++)
				{
					dailyChildren.Add(Reproduce(shuffledParents[j], parent2Strategy));
				}
			}

			for (var j = numGames * 2; j < shuffledParents.Count; j += 1)
			{
				if (numGames < NumTrees)
				{
					var parentStrategy = Registry.Strategies[shuffledParents[j]];
					dailyChildren.Add(Reproduce(shuffledParents[j], parentStrategy));
					dailyChildren.Add(Reproduce(shuffledParents[j], parentStrategy));
				}
				// Else they die, which is just not reproducing, so do nothing
			}
			
			// Shuffle at the end of the loop so entities are sorted by tree for the next day
			EntitiesByDay[i] = dailyChildren.ShuffleToList(rng: _rng);
		}
	}

	private int Reproduce(int parentIndex, int strategy)
	{
		if (_rng.RangeFloat(1) < MutationRate / 2)
		{
			strategy = (strategy + 1) % RpsGame.StrategyOptions.Length;
		}
		else if (_rng.RangeFloat(1) < MutationRate)
		{
			strategy = (strategy + 2) % RpsGame.StrategyOptions.Length;
		}
		
		return Registry.CreateBlob(
			strategy,
			parentIndex
		);
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
			var fractionRock = entitiesToday.Count(s => Registry.Strategies[s] == 0) / (float)entitiesToday.Count;
			var fractionPaper = entitiesToday.Count(s => Registry.Strategies[s] == 1) / (float)entitiesToday.Count;
			var fractionScissors = entitiesToday.Count(s => Registry.Strategies[s] == 2) / (float)entitiesToday.Count;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
		}
	}
	public Vector3[] GetStrategyFrequenciesByDay()
	{
		var frequencies = new Vector3[NumDays + 1];
		foreach (var (day, entitiesToday) in EntitiesByDay.WithIndex())
		{
			var fractionRock = entitiesToday.Count(s => Registry.Strategies[s] == 0) / (float)entitiesToday.Count;
			var fractionPaper = entitiesToday.Count(s => Registry.Strategies[s] == 1) / (float)entitiesToday.Count;
			var fractionScissors = entitiesToday.Count(s => Registry.Strategies[s] == 2) / (float)entitiesToday.Count;
			frequencies[day] = new Vector3(fractionRock, fractionPaper, fractionScissors);
		}
		return frequencies;
	}
}
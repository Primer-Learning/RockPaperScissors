using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using PrimerTools;
using System.Linq;

using EntityID = System.Int32;
using Strategy = System.Int32;
using Parent = System.Int32;

[Tool]
public partial class SimRunner2 : Node
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
				for (int i = 0; i < 100; i++)
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

	private struct EntityRegistry
	{
		public static EntityID _nextID;
		public static Dictionary<EntityID, Strategy> _strategies = new();
		public static Dictionary<EntityID, Parent> _parents = new();
		
		public static EntityID CreateEntity(Strategy strategy, Parent parent)
		{
			var id = _nextID++;
			_strategies.Add(id, strategy);
			_parents.Add(id, parent);
			return id;
		}
	}
	
	#region Parameters
	private Rng _rng;
	private const int NumDays = 100;
	private const int InitialBlobCount = 3200;
	// private const int NumTrees = 150;
	
	// Reward matrix
	private const float WinMagnitude = 1.0f;
	private float[,] _rewardMatrix = new float[3, 3] {
		{ 1, 1 - WinMagnitude, 1 + WinMagnitude }, // Rock rewards
		{ 1 + WinMagnitude, 1, 1 - WinMagnitude }, // Paper rewards   
		{ 1 - WinMagnitude, 1 + WinMagnitude, 1 }  // Scissors rewards
	};
	#endregion
	
	private static readonly EntityID[][] EntitiesByDay = new EntityID[NumDays + 1][];

	private void Initialize()
	{
		// rng = new Rng(0);
		_rng = new Rng(System.Environment.TickCount);
		
		var blobIDs = new EntityID[InitialBlobCount];
		for (var i = 0; i < InitialBlobCount; i++)
		{
			blobIDs[i] = EntityRegistry.CreateEntity(
				// StrategyIndex = i % 3 // Even mix
				// rng.RangeInt(3) // Random mix
				i % 4 > 2 ? 0 : i % 4 // 50% rock, 25% paper, 25% scissors
				,
				-1
			);
		}
		EntitiesByDay[0] = blobIDs;
	}

	private List<EntityID> dailyChildren = new();
	private void Simulate()
	{
		for (var i = 1; i <= NumDays; i++)
		{
			var shuffledParents = EntitiesByDay[i - 1].ShuffleToList(rng: _rng).ToArray();
			
			// var childEntities = new List<EntityID>();
			dailyChildren.Clear();
			for (var j = 0; j < shuffledParents.Length; j += 2)
			{
				var strategy1 = EntityRegistry._strategies[shuffledParents[j]];
				var strategy2 = EntityRegistry._strategies[shuffledParents[j+1]];
				dailyChildren.AddRange(Reproduce(_rewardMatrix[strategy1, strategy2], shuffledParents[j]));
				dailyChildren.AddRange(Reproduce(_rewardMatrix[strategy2, strategy1], shuffledParents[j + 1]));
			}
			EntitiesByDay[i] = dailyChildren.ToArray();
		}
	}

	private EntityID[] Reproduce(float reward, EntityID parent)
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

		var children = new EntityID[offspringCount];
		for (var k = 0; k < offspringCount; k++)
		{
			children[k] = EntityRegistry.CreateEntity(
				EntityRegistry._strategies[parent],
				parent
			);
		}

		return children;
	}
	
	private void PrintResults()
	{
		foreach (var (day, entities) in EntitiesByDay.WithIndex())
		{
			// GD.Print(entities.Length);
			GD.Print($"Day {day}");
			var fractionRock = entities.Count(s => EntityRegistry._strategies[s] == 0) / (float)entities.Length;
			var fractionPaper = entities.Count(s => EntityRegistry._strategies[s] == 1) / (float)entities.Length;
			var fractionScissors = entities.Count(s => EntityRegistry._strategies[s] == 2) / (float)entities.Length;
			// Print the fractions formatted to two decimal places
			GD.Print($"Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
		}
	}
}
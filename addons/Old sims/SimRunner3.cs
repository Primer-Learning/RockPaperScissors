using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using PrimerTools;
using System.Linq;

namespace RockPaperScissors;

[Tool]
public partial class SimRunner3 : Node
{
    private struct Entity
    {
        public EntityId Id;
        public Strategy Strategy;
        public EntityId Parent;
    }

    public record struct EntityId(int Day, int IndexInTheDay)
    {
        public static readonly EntityId InvalidId = new(-1, -1);
    }

    public record struct Strategy(int Value)
    {
        public static readonly Strategy Rock = new(0);
        public static readonly Strategy Paper = new(1);
        public static readonly Strategy Scissors = new(2);
    };

    #region Parameters

    private Rng _rng;
    private const int NumDays = 100;
    private const int InitialBlobCount = 3200;
    // private const int NumTrees = 150;

    // Reward matrix
    private const float WinMagnitude = 1.0f;

    private float[,] _rewardMatrix = new float[3, 3]
    {
        { 1, 1 - WinMagnitude, 1 + WinMagnitude }, // Rock rewards
        { 1 + WinMagnitude, 1, 1 - WinMagnitude }, // Paper rewards   
        { 1 - WinMagnitude, 1 + WinMagnitude, 1 } // Scissors rewards
    };

    #endregion

    #region Running toggle

    private bool _run = true;

    [Export]
    private bool Run
    {
        get => _run;
        set
        {
            var oldRun = _run;
            _run = value;
            if (_run && !oldRun && Engine.IsEditorHint())
            {
                // Avoids running on build
                var stopwatch = Stopwatch.StartNew();

                var seed = System.Environment.TickCount;
                seed = 0;

                _rng = new Rng(seed);

                for (int i = 0; i < 100; i++)
                {
                    // If you want every run to be the same, uncomment the line below
                    // _rng = new Rng(seed);
                    
                    // Since entitiesPerDay is going to contain borrowed lists inside, it cannot be moved outside of this loop. Otherwise it's going to contain invalid data.
                    var entitiesPerDay = new List<Entity>[NumDays + 1];
                    
                    Initialize(entitiesPerDay);
                    Simulate(entitiesPerDay);

                    // important to print results before arena reclaims all the lists. Otherwise all the entity lists will be cleared.
                    // PrintResults(entitiesPerDay);
                    
                    // MemoryPool<Entity>.ReclaimAll();
                    // MemoryPool<EntityId>.ReclaimAll();
                    // MemoryPool<Strategy>.ReclaimAll();
                }
                
                stopwatch.Stop();
                GD.Print("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }
    }

    #endregion


    private void Initialize(List<Entity>[] entitiesPerDay)
    {
        // var blobs = new List<Entity>();
        var blobs = MemoryPool<Entity>.BorrowList();
        for (var i = 0; i < InitialBlobCount; i++)
        {
            // I think this is more clear than i % 4 > 2 ? 0 : i % 4
            var strategy = (i % 4) switch
            {
                0 => Strategy.Rock,
                1 => Strategy.Paper,
                2 => Strategy.Scissors,
                3 => Strategy.Rock,
                _ => throw new System.Exception("This should never happen")
            };

            blobs.Add(new Entity
            {
                Id = new EntityId(0, i),
                Strategy = strategy,
                Parent = EntityId.InvalidId,
            });
        }

        entitiesPerDay[0] = blobs;
    }

    private void Simulate(List<Entity>[] entitiesPerDay)
    {
        for (var i = 1; i <= NumDays; i++)
        {
            // Shuffle last day parents, but use a borrowed array to avoid allocations
            var lastDayParents = entitiesPerDay[i - 1];
            // var shuffledParents = new List<Entity>();
            var shuffledParents = MemoryPool<Entity>.BorrowList();
            shuffledParents.AddRange(lastDayParents);
            shuffledParents.Shuffle(_rng);

            // var todayChildren = new List<Entity>();
            var todayChildren = MemoryPool<Entity>.BorrowList();
            for (var j = 0; j < shuffledParents.Count; j += 2)
            {
                var parent1 = shuffledParents[j];
                var parent2 = shuffledParents[j + 1];

                for (var k = 0; k < GetOffspringCount(_rewardMatrix[parent1.Strategy.Value, parent2.Strategy.Value]); k++)
                {
                    todayChildren.Add(new Entity
                    {
                        Id = new EntityId(parent1.Id.Day + 1, todayChildren.Count),
                        Strategy = parent1.Strategy,
                        Parent = parent1.Id
                    });
                }

                for (var k = 0; k < GetOffspringCount(_rewardMatrix[parent2.Strategy.Value, parent1.Strategy.Value]); k++)
                {
                    todayChildren.Add(new Entity
                    {
                        Id = new EntityId(parent2.Id.Day + 1, todayChildren.Count),
                        Strategy = parent2.Strategy,
                        Parent = parent2.Id
                    });
                }
            }

            entitiesPerDay[i] = todayChildren;
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
    
    private void PrintResults(List<Entity>[] entitiesPerDay)
    {
        foreach (var (day, entities) in entitiesPerDay.WithIndex())
        {
            var fractionRock = entities.Count(s => s.Strategy == Strategy.Rock) / (float)entities.Count;
            var fractionPaper = entities.Count(s => s.Strategy == Strategy.Paper) / (float)entities.Count;
            var fractionScissors = entities.Count(s => s.Strategy == Strategy.Scissors) / (float)entities.Count;
            // Print the fractions formatted to two decimal places
            GD.Print($"Day {day}. Rock: {fractionRock:P1}, Paper: {fractionPaper:P1}, Scissors: {fractionScissors:P1}");
        }
    }
}
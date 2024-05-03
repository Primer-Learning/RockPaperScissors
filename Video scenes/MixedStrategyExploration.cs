using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;

[Tool]
public partial class MixedStrategyExploration : AnimationSequence
{
    protected override void Define()
    {
        var horizontalSpacing = 55;
        var verticalSpacing = 30;
        var simAnimators = new List<EvoGameTheorySimAnimator>();
        var baseSeed = System.Environment.TickCount;
        for (var i = 0; i < 1; i++)
        {
            var newAnimator = NewSimAnimatorParticularToTheFindStabilityScene(baseSeed + i);
            newAnimator.Position = new Vector3(horizontalSpacing * (i % 3), verticalSpacing * (i / 3), 0);
            simAnimators.Add(newAnimator);
        }
        RegisterAnimation(simAnimators.Select(x => x.AnimateAppearance()).RunInParallel());
        RegisterAnimation(simAnimators.Select(x => x.TernaryGraph.ScaleTo(20)).RunInParallel());
        foreach (var animator in simAnimators)
        {
            ((TernaryGraphWithBars)animator.TernaryGraph).Data = 
                Enumerable.Repeat(0f, ((TernaryGraphWithBars)animator.TernaryGraph).TotalNumberOfBars).ToArray();
        }
        RegisterAnimation(simAnimators.Select(x => ((TernaryGraphWithBars)x.TernaryGraph).Transition()).RunInParallel());
        RegisterAnimation(simAnimators.Select(x => x.AnimateAllDays()).RunInParallel());
    }
  
    private EvoGameTheorySimAnimator NewSimAnimatorParticularToTheFindStabilityScene(int seed)
    {
        // Run the simulation
        var sim = new EvoGameTheorySim();
        AddChild(sim);
        sim.RpsGame = new EvoGameTheorySim.RPSGame(
            winMagnitude: 1f,
            tieCost: 0.0f
        );
        // sim.InitialAlleleFrequencies = new[] { 1f, 1f, 1f };
        sim.NumAllelesPerBlob = 3;
        sim.InitializeByAlleleFrequency = false;
        sim.InitialMixedStrategyDistribution = new Dictionary<int[], float>
        {
            {new []{3, 0, 0}, 1}
        };
        sim.NumDays = 2000;
        sim.InitialBlobCount = 400;
        sim.MutationRate = 0.0001f;
        sim.NumTrees = 200;
        sim.Seed = seed;
        sim.RunSim();

        var simAnimator = EvoGameTheorySimAnimator.NewSimAnimator(new Vector2(20, 20));
        AddChild(simAnimator);
        simAnimator.AnimateBlobs = false;
        simAnimator.HasBars = true;
        // simAnimator.
        simAnimator.Name = "Sim animator";
        simAnimator.Owner = GetTree().EditedSceneRoot;
        simAnimator.Sim = sim;
		
        simAnimator.Ground.Position = new Vector3(15, 0, 0);

        simAnimator.SetUpTernaryPlot(makeCurve: false, makeBars: true);
        simAnimator.TernaryGraph.Scale = Vector3.Zero;
        simAnimator.TernaryGraph.Position = Vector3.Left * 22;

        return simAnimator;
    }
}
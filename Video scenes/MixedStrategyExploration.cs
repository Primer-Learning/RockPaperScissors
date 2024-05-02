using System.Collections.Generic;
using System.Linq;
using Godot;
using PrimerTools;
using PrimerTools.AnimationSequence;

[Tool]
public partial class MixedStrategyExploration : AnimationSequence
{
    protected override void Define()
    {
        var horizontalSpacing = 55;
        var verticalSpacing = 30;
        var simAnimators = new List<EvoGameTheorySimAnimator>();
        var baseSeed = System.Environment.TickCount;
        for (var i = 0; i < 9; i++)
        {
            var newAnimator = NewSimAnimatorParticularToTheFindStabilityScene(baseSeed + i);
            newAnimator.Position = new Vector3(horizontalSpacing * (i % 3), verticalSpacing * (i / 3), 0);
            simAnimators.Add(newAnimator);
        }
        RegisterAnimation(simAnimators.Select(x => x.AnimateAppearance()).RunInParallel());
        RegisterAnimation(simAnimators.Select(x => x.TernaryGraph.ScaleTo(20)).RunInParallel());
        RegisterAnimation(simAnimators.Select(x => x.AnimateAllDays()).RunInParallel());
    }
  
    private EvoGameTheorySimAnimator NewSimAnimatorParticularToTheFindStabilityScene(int seed)
    {
        // Run the simulation
        var sim = new EvoGameTheorySim();
        sim.RpsGame = new EvoGameTheorySim.RPSGame(
            winMagnitude: 0.5f,
            tieCost: 0f
        );
        sim.InitialAlleleFrequencies = new[] { 2f, 1f, 1f };
        AddChild(sim);
        sim.NumDays = 200;
        sim.InitialBlobCount = 400;
        sim.MutationRate = 0.001f;
        sim.NumTrees = 400;
        sim.Seed = seed;
        sim.RunSim();

        var simAnimator = EvoGameTheorySimAnimator.NewSimAnimator(new Vector2(20, 20));
        AddChild(simAnimator);
        simAnimator.AnimateBlobs = false;
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
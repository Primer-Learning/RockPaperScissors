using Godot;
using PrimerTools.AnimationSequence;

[Tool]
public partial class PlottedSimTest : AnimationSequence
{
    protected override void Define()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        
        // Run the simulation
        var sim = new EvoGameTheorySim();
        AddChild(sim);
        sim.NumDays = 5;
        sim.InitialBlobCount = 4;
        sim.NumTrees = 20;
        sim.RunSim();
        
        GD.Print("Sim time: " + stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();

        #region Animate the results

        var simAnimator = new EvoGameTheorySimAnimator();
        AddChild(simAnimator);
        simAnimator.Owner = GetTree().EditedSceneRoot;
        simAnimator.Sim = sim;
        simAnimator.NonAnimatedSetup();
        
        // Spawn and move blobs according to the results
        GD.Print("Non-animated time: " + stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();
        
        RegisterAnimation(simAnimator.AnimateDays());
        GD.Print("Total animation generation time" + stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();
        #endregion

        #region Plot the results
        RegisterAnimation(simAnimator.AnimateTernaryPlot());
        GD.Print("Plot time: " + stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();
        #endregion
    }
}

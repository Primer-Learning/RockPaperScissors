using Godot;
using PrimerTools.AnimationSequence;

[Tool]
public partial class PlottedSimTest : AnimationSequence
{
    protected override void Define()
    {
        var currentTime = System.Environment.TickCount;
        // var stopwatch = new System.Diagnostics.Stopwatch();
        // stopwatch.Start();
        
        // Run the simulation
        var sim = new EvoGameTheorySim();
        AddChild(sim);
        // sim.NumDays = 50; // 43ish seconds
        sim.NumDays = 5; // 15 ish seconds
        // sim.NumDays = 100; // Crash
        sim.InitialBlobCount = 40;
        sim.NumTrees = 5;
        sim.RunSim();
        
        // GD.Print("Sim time: " + stopwatch.ElapsedMilliseconds);
        // stopwatch.Restart();

        #region Animate the results

        var simAnimator = new EvoGameTheorySimAnimator();
        AddChild(simAnimator);
        simAnimator.Owner = GetTree().EditedSceneRoot;
        simAnimator.Sim = sim;
        simAnimator.IncludeTernaryPlot = true;
        simAnimator.NonAnimatedSetup();
        
        // Spawn and move blobs according to the results
        // GD.Print("Non-animated time: " + stopwatch.ElapsedMilliseconds);
        // stopwatch.Restart();
        
        RegisterAnimation(simAnimator.AnimateAllDays());
        // GD.Print("Total animation generation time: " + stopwatch.ElapsedMilliseconds);
        // stopwatch.Restart();
        #endregion
        
        // GD.Print("Total time: " + (System.Environment.TickCount - currentTime) + " ms");

        #region Plot the results
        // simAnimator.SetUpTernaryPlot();
        // RegisterAnimation(simAnimator.AnimateTernaryPlotToDay());
        // GD.Print("Plot time: " + stopwatch.ElapsedMilliseconds);
        // stopwatch.Restart();
        #endregion
    }
}

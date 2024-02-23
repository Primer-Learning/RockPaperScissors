using System.Linq;
using Godot;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;

[Tool]
public partial class PlottedSimTest : AnimationSequence
{
    protected override void Define()
    {
        var sim = new EvoGameTheorySim();
        AddChild(sim);
        sim.NumDays = 1000;
        sim.RunSim();
        var results = sim.GetStrategyFrequenciesByDay();
        
        var ternaryGraph = new TernaryGraph();
        AddChild(ternaryGraph);
        ternaryGraph.Owner = GetTree().EditedSceneRoot;
        ternaryGraph.Position = Vector3.Left / 2;
        ternaryGraph.CreateBounds();
        
        var plot = new CurvePlot2D();
        ternaryGraph.AddChild(plot);
        plot.SetData(results.Select(point => TernaryGraph.CoordinatesToPosition(point.X, point.Y, point.Z)).ToArray());
        plot.Width = 3;
        
        plot.Owner = GetTree().EditedSceneRoot;
        
        RegisterAnimation(plot.Transition(10));
    }
}
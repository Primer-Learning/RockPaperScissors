using Godot;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;

[Tool]
public partial class InitialSimScene : AnimationSequence
{
	protected override void Define()
	{
		#region Setup
		
		// Run the simulation
		var sim = new EvoGameTheorySim();
		AddChild(sim);
		sim.NumDays = 5;
		sim.InitialBlobCount = 40;
		sim.NumTrees = 5;
		sim.RunSim();

		#region Animate the results

		var simAnimator = new EvoGameTheorySimAnimator();
		AddChild(simAnimator);
		simAnimator.Name = "Sim animator";
		simAnimator.Owner = GetTree().EditedSceneRoot;
		simAnimator.Sim = sim;
		simAnimator.IncludeTernaryPlot = true;
		
		var table = new Table();
		AddChild(table);
		table.Owner = GetTree().EditedSceneRoot;
		table.Name = "Reward matrix";
		table.Position = new Vector3(1, 12, 0);
		table.Scale = Vector3.One * 0.85f;
		
		// Define the reward matrix
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable(@"\begin{center} Mango \\ Rewards \end{center}", 0, 0);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("vs Rock", 0, 1);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("vs Paper", 0, 2);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("vs Scissors", 0, 3);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("Rock", 1, 0);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("Paper", 2, 0);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("Scissors", 3, 0);
		// Rewards
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("1", 1, 1);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("0", 1, 2);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("2", 1, 3);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("2", 2, 1);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("1", 2, 2);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("0", 2, 3);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("0", 3, 1);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("2", 3, 2);
		table.AddLatexNodeToPositionWithDefaultSettingsForTheTable("1", 3, 3);
		// table.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		table.SetScaleOfAllChildren(Vector3.Zero);
		
		
		var graph = Graph.CreateAsOwnedChild(this);
		graph.Position = new Vector3(-14, 0.5f, 0);
		graph.XAxis.length = 9;
		graph.XAxis.showTicCylinders = false;
		graph.XAxis.showArrows = false;
		graph.XAxis.max = 3;
		graph.XAxis.Thiccness = 4;
		graph.XAxis.showRod = false;
		graph.YAxis.max = 1;
		graph.YAxis.ticStep = 1;
		graph.YAxis.length = 9;
		graph.YAxis.Thiccness = 4;
		graph.YAxis.Visible = false;
		graph.ZAxis.Visible = false;
		graph.ZAxis.length = 0;
		// graph.Transition();
		
		#endregion
		
		RegisterAnimation(simAnimator.MakeGroundAndAnimateAppearance(), 1);
		RegisterAnimation(simAnimator.MakeTreesAndHomesAndAnimateAppearance());
		RegisterAnimation(table.ScaleAllChildrenToDefault());
		
		var barPlot = graph.AddBarPlot();
		barPlot.ShowValuesOnBars = true;
		barPlot.SetData(1f/3, 1f/3, 1f/3);
		RegisterAnimation(graph.Transition());
        
		// Spawn and move blobs according to the results
		// GD.Print("Non-animated time: " + stopwatch.ElapsedMilliseconds);
		// stopwatch.Restart();
        
		// RegisterAnimation(simAnimator.AnimateDays());
		// GD.Print("Total animation generation time: " + stopwatch.ElapsedMilliseconds);
		// stopwatch.Restart();
		#endregion
        
		// GD.Print("Total time: " + (System.Environment.TickCount - currentTime) + " ms");
	}
}

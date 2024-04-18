using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;
using PrimerTools.LaTeX;

[Tool]
public partial class InitialSimScene : AnimationSequence
{
	private Rng rng = new Rng(0);
	protected override void Define()
	{
		#region Setup
		
		var strats = new [] {
			EvoGameTheorySim.RPSGame.Strategy.Rock,
			EvoGameTheorySim.RPSGame.Strategy.Paper,
			EvoGameTheorySim.RPSGame.Strategy.Scissors
		};
		
		var verticalSpacing = 7;
		var horizontalSpacing = 7;
		var conflictSites = new List<ExampleConflictTree>();
		for (var i = 0; i < 3; i++)
		{
			for (var j = 0; j < 3; j++)
			{
				var conflictSite = new ExampleConflictTree(
					this,
					position: new Vector3(horizontalSpacing * (-2 + j % 3), verticalSpacing * (2 - i % 3), 0),
					strats[i],
					strats[j],
					rng
				);
				conflictSites.Add(conflictSite);
			}
		}
		
		var camRig = GetParent().GetNode<CameraRig>("CameraRig");
		camRig.Distance = 18;
		camRig.Position = new Vector3(- 1 * horizontalSpacing, 2 * verticalSpacing + 3.5f, 0);
		camRig.RotationDegrees = new Vector3(-5, 0, 0);
		
		// Run the simulation
		var sim = new EvoGameTheorySim();
		AddChild(sim);
		sim.NumDays = 20;
		sim.InitialBlobCount = 40;
		sim.NumTrees = 40;
		sim.RunSim();

		var simAnimator = EvoGameTheorySimAnimator.NewSimAnimator(new Vector2(20, 20));
		AddChild(simAnimator);
		simAnimator.AnimateBlobs = false;
		simAnimator.Name = "Sim animator";
		simAnimator.Owner = GetTree().EditedSceneRoot;
		simAnimator.Sim = sim;
		
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
		
		// graph.Transition();
		
		// Winner take all text
		var winnerTakeAll = LatexNode.Create("Winner take all");
		AddChild(winnerTakeAll);
		// winnerTakeAll.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		
		// Winner take all text
		var balancedText = LatexNode.Create("Stay balanced?");
		AddChild(balancedText);
		balancedText.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		
		var oneTakeOverText = LatexNode.Create("One takes over?");
		AddChild(oneTakeOverText);
		oneTakeOverText.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);

		var cyclicalText = LatexNode.Create("Cyclical?");
		AddChild(cyclicalText);
		cyclicalText.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		
		#endregion

		// Show all trees, just for testing
		// var treeAnimations = new List<Animation>();
		// foreach (var conflictSite in conflictSites)
		// {
		// 	treeAnimations.Add(conflictSite.GrowTreeAndFruit());
		// }
		// RegisterAnimation(treeAnimations.RunInParallel());

		#region Reward explanation
		RegisterAnimation(conflictSites[1].GrowTreeAndFruit(), 1);
		RegisterAnimation(conflictSites[1].BlobsAppear());
		RegisterAnimation(conflictSites[1].Blob1.TransitionAnimationTreeCondition("smile", "blob_mouth_state_machine"));
		
		// Show the "Winner take all" text
		winnerTakeAll.Position = new Vector3(-1 * horizontalSpacing, 3 * verticalSpacing - 0.5f, 0);
		winnerTakeAll.Scale = Vector3.Zero;
		RegisterAnimation(winnerTakeAll.ScaleTo(Vector3.One), 7.5f);
		
		RegisterAnimation(conflictSites[1].BlobsFaceEachOther());
		RegisterAnimation(conflictSites[1].Showdown());
		RegisterAnimation(conflictSites[1].EatFruit());

		
		camRig.Position = new Vector3(- 1 * horizontalSpacing, 2 * verticalSpacing + 3.5f, 0);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				camRig.MoveTo(new Vector3(- 1.5f * horizontalSpacing, 2 * verticalSpacing + 3.5f, 0)),
				// camRig.ZoomTo(21),
				winnerTakeAll.MoveTo(new Vector3(-1.5f * horizontalSpacing, 3 * verticalSpacing - 0.5f, 0))
			)
		);
		
		RegisterAnimation(conflictSites[0].GrowTreeAndFruit(), 15);
		RegisterAnimation(conflictSites[0].BlobsAppear());
		RegisterAnimation(conflictSites[0].BlobsFaceEachOther());
		RegisterAnimation(conflictSites[0].Showdown());
		RegisterAnimation(conflictSites[0].EatFruit());

		var newCenterX = 5.95f;
		RegisterAnimation(
			AnimationUtilities.Parallel(
				camRig.MoveTo(new Vector3( newCenterX, 11, 0)),
				camRig.ZoomTo(57.5f),
				winnerTakeAll.MoveTo(new Vector3(newCenterX, 21.5f, 0)),
				winnerTakeAll.ScaleTo(1.5f)
			)
		);
		var theRestOfTheConflictSiteAnimations = new List<Animation>();
		for (var i = 2; i < 9; i++)
		{
			theRestOfTheConflictSiteAnimations.Add(
				AnimationUtilities.Series(
					conflictSites[i].GrowTreeAndFruit(),
					conflictSites[i].BlobsAppear(),
					conflictSites[i].BlobsFaceEachOther(),
					conflictSites[i].Showdown(),
					conflictSites[i].EatFruit()
				).WithDelay(0.1f * (i - 2))
			);
		}
		RegisterAnimation(theRestOfTheConflictSiteAnimations.RunInParallel());
		
		table.Position = new Vector3(8.8888f, 14.25f, 0);
		table.Scale = Vector3.One * 1.64f;
		RegisterAnimation(table.ScaleAllChildrenToDefault());
		#endregion

		#region Show the sim

		RegisterAnimation(
			AnimationUtilities.Parallel(
				winnerTakeAll.MoveTo(new Vector3(-9, 22, 0)),
				conflictSites.Select(x => x.Tree.ScaleTo(0)).RunInParallel(),
				conflictSites.Select(x => x.Blob1.ScaleTo(0)).RunInParallel(),
				conflictSites.Select(x => x.Blob2.ScaleTo(0)).RunInParallel(),
				table.MoveTo(new Vector3(7, 21.5f, 0))
			),
			66
		);
		
		// Ground appears as a size 20 plane with position 16 to the left
		RegisterAnimation(simAnimator.AnimateAppearance());
		simAnimator.Ground.Position = new Vector3(15, 0, 0);
		RegisterAnimation(simAnimator.MakeTreesAndHomesAndAnimateAppearance());

		#region Graph creation and settings

		var graph = Graph.CreateAsOwnedChild(this);
		graph.Position = new Vector3(-21, -2, 0);
		graph.XAxis.length = 18;
		graph.XAxis.showTicCylinders = false;
		graph.XAxis.showArrows = false;
		graph.XAxis.max = 3;
		graph.XAxis.manualTicks = new List<Axis.TicData>
		{
			new (1, "Rock"),
			new (2, "Paper"),
			new (3, "Scissors")
		};
		graph.XAxis.Thiccness = 4;
		graph.XAxis.showRod = false;
		// graph.XAxis.Visible = false;
		
		graph.YAxis.max = 100;
		graph.YAxis.ticStep = 100;
		graph.YAxis.length = 18;
		graph.YAxis.Thiccness = 4;
		graph.YAxis.Visible = false;
		graph.ZAxis.Visible = false;
		graph.ZAxis.length = 0;
		
		#endregion
		
		var barPlot = graph.AddBarPlot();
		barPlot.ShowValuesOnBars = true;
		barPlot.BarLabelSuffix = "\\%";
		barPlot.BarLabelScaleFactor = 0.6f;
		
		barPlot.SetData(100f/3, 100f/3, 100f/3);
		RegisterAnimation(graph.Transition());

		#endregion

		#region Pre-sim questions

		balancedText.Position = new Vector3(-9, 17.25f, 0);
		oneTakeOverText.Position = new Vector3(-9, 14.25f, 0);
		cyclicalText.Position = new Vector3(-9, 11.25f, 0);
		balancedText.Scale = Vector3.Zero;
		oneTakeOverText.Scale = Vector3.Zero;
		cyclicalText.Scale = Vector3.Zero;

		RegisterAnimation(
			balancedText.ScaleTo(Vector3.One),
			oneTakeOverText.ScaleTo(Vector3.One).WithDelay(0.1f),
			cyclicalText.ScaleTo(Vector3.One).WithDelay(0.2f)
		);
		RegisterAnimation(
			balancedText.ScaleTo(Vector3.Zero),
			oneTakeOverText.ScaleTo(Vector3.Zero).WithDelay(0.1f),
			cyclicalText.ScaleTo(Vector3.Zero).WithDelay(0.2f)
		);

		#endregion

		#region Run sim

		simAnimator.BarPlot = barPlot;
		RegisterAnimation(simAnimator.AnimateAllDays(), 87);

		#endregion

		// Old sim running code. Probably reusable but not certain.
		// RegisterAnimation(simAnimator.AnimateDays());
	}
}

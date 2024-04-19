using Godot;
using System;
using System.Linq;
using Primer;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;
using PrimerTools.LaTeX;

[Tool]
public partial class TernaryGraphExplanationScene : AnimationSequence
{
	protected override void Define()
	{
		#region Initial ternary graph
		var ternaryGraph = new TernaryGraph();
		ternaryGraph.Name = "Ternary Graph";
		AddChild(ternaryGraph);
		// ternaryGraph.Owner = GetTree().EditedSceneRoot;
		ternaryGraph.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		ternaryGraph.LabelStrings = new [] {"Rock", "Paper", "Scissors"};
		ternaryGraph.Colors = new []
		{
			EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Rock],
			EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper],
			EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors]
		};
		ternaryGraph.CreateBounds();
        foreach (var node in ternaryGraph.GetChildren())
        {
	        ((Node3D)node).Scale = Vector3.Zero;
        }

        var meshChildren = ternaryGraph.GetChildren().OfType<MeshInstance3D>();
        RegisterAnimation( meshChildren.Select(x => x.ScaleTo(1)).RunInParallel() );

        var examplePoint = new MeshInstance3D();
		var sphereMesh = new SphereMesh();
		sphereMesh.Height = 0.06f;
		sphereMesh.Radius = 0.03f;
		var mat = new StandardMaterial3D();
		mat.AlbedoColor = PrimerColor.orange;
		sphereMesh.SurfaceSetMaterial(0, mat);
		examplePoint.Mesh = sphereMesh;
        ternaryGraph.AddChild(examplePoint);
        examplePoint.Owner = GetTree().EditedSceneRoot;
        examplePoint.Position = TernaryGraph.CoordinatesToPosition(1f / 5, 2f / 5, 2f / 5);
        examplePoint.Scale = Vector3.Zero;
        
        RegisterAnimation(examplePoint.ScaleTo(1));

        var arrow = Arrow.CreateArrow();
        AddChild(arrow);
        arrow.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
        // arrow.Owner = GetTree().EditedSceneRoot;
        arrow.nodeThatHeadFollows = examplePoint;
        arrow.RotationDegrees = new Vector3(0, 0, 30);
        arrow.Chonk = 0.25f;
        arrow.Length = 0.5f;
        arrow.HeadPadding = 0.075f;
        RegisterAnimation(arrow.ScaleUpFromTail());

        // var coordinateLabel = LatexNode.Create("(0.2, 0.4, 0.4)");
        // coordinateLabel.Scale = Vector3.Zero;
        // coordinateLabel.ScaleTo(1);
        // AddChild(coordinateLabel);
        // coordinateLabel.Owner = GetTree().EditedSceneRoot;

        var coordinateLabelParent = new Node3D();
		var coordinateLabelGroup = RPSLabelGroup.Create("0.2", "0.4", "0.4");
		coordinateLabelGroup.HandOffChildrenToNode(coordinateLabelParent);
        AddChild(coordinateLabelParent);
        coordinateLabelParent.Name = "RPSLabel";
        coordinateLabelParent.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
        coordinateLabelParent.Position = new Vector3(1, 0.735f, 0);
        coordinateLabelParent.Scale = Vector3.One * 0.04f;
        foreach (var latexNode in coordinateLabelGroup.AllLabels)
        {
	        latexNode.Scale = Vector3.Zero;
        }

        RegisterAnimation(
	        AnimationUtilities.Series(
		        AnimationUtilities.Parallel(
			        ternaryGraph.Labels[0].ScaleTo(0.1f),
			        coordinateLabelGroup.RockLabel.ScaleTo(1),
			        coordinateLabelGroup.RockNumberLabel.ScaleTo(1)
		        ),
		        AnimationUtilities.Parallel(
			        ternaryGraph.Labels[1].ScaleTo(0.1f),
			        coordinateLabelGroup.PaperLabel.ScaleTo(1),
			        coordinateLabelGroup.PaperNumberLabel.ScaleTo(1)
		        ),
		        AnimationUtilities.Parallel(
			        ternaryGraph.Labels[2].ScaleTo(0.1f),
			        coordinateLabelGroup.ScissorsLabel.ScaleTo(1),
			        coordinateLabelGroup.ScissorsNumberLabel.ScaleTo(1)
		        )
	        )
        );

        RegisterAnimation(
	        ternaryGraph.ScaleTo(0),
	        arrow.ScaleTo(0),
	        coordinateLabelParent.ScaleTo(0)
        );

        #endregion

        var cartesianGraph = Graph.CreateInstance();
        AddChild(cartesianGraph);
        // cartesianGraph.Owner = GetTree().EditedSceneRoot;
        cartesianGraph.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
        cartesianGraph.ZAxis.length = 0;
        
        cartesianGraph.XAxis.length = 1.25f;
        cartesianGraph.XAxis.Chonk = 0.5f;
        cartesianGraph.XAxis.Max = 1;
        cartesianGraph.XAxis.TicStep = 1;
        cartesianGraph.XAxis.Padding = new Vector2(0.1f, 0.1f);
        
        cartesianGraph.YAxis.length = 1.25f;
        cartesianGraph.YAxis.Chonk = 0.5f;
        cartesianGraph.YAxis.TicStep = 1;
        cartesianGraph.YAxis.Max = 1;
        cartesianGraph.YAxis.Padding = new Vector2(0.1f, 0.1f);
        
        cartesianGraph.Transition();

        cartesianGraph.Position = Vector3.Down * 0.1f;
        cartesianGraph.Scale = Vector3.Zero;
        RegisterAnimation(cartesianGraph.ScaleTo(1));
        



        // var plot = new CurvePlot2D();
        // ternaryGraph.AddChild(plot);
        // plot.Owner = GetTree().EditedSceneRoot;
        // plot.Width = 10;

	}

	private class RPSLabelGroup
	{
		public LatexNode[] AllLabels => new []
		{
			RockLabel,
			RockNumberLabel,
			PaperLabel,
			PaperNumberLabel,
			ScissorsLabel,
			ScissorsNumberLabel
		};
		
		public LatexNode RockLabel;
		public LatexNode RockNumberLabel;
		public LatexNode PaperLabel;
		public LatexNode PaperNumberLabel;
		public LatexNode ScissorsLabel;
		public LatexNode ScissorsNumberLabel;
		
		public static RPSLabelGroup Create(string r, string p, string s)
		{
			var labelGroup = new RPSLabelGroup();

			labelGroup.RockLabel = LatexNode.Create("Rock:");
			labelGroup.PaperLabel = LatexNode.Create("Paper:");
			labelGroup.ScissorsLabel = LatexNode.Create("Scissors:");
			labelGroup.RockNumberLabel = LatexNode.Create(r);
			labelGroup.PaperNumberLabel = LatexNode.Create(p);
			labelGroup.ScissorsNumberLabel = LatexNode.Create(s);

			foreach (var label in labelGroup.AllLabels)
			{
				label.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
				label.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			}
			//
			// labelGroup.PaperLabel.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
			// labelGroup.PaperLabel.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			// labelGroup.ScissorsLabel.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
			// labelGroup.ScissorsLabel.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			//
			// labelGroup.RockNumberLabel.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
			// labelGroup.RockNumberLabel.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			// labelGroup.PaperNumberLabel.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
			// labelGroup.PaperNumberLabel.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			// labelGroup.ScissorsNumberLabel.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
			// labelGroup.ScissorsNumberLabel.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;

			const float verticalSpacing = 1.5f;
			
			labelGroup.RockLabel.Position = Vector3.Zero;
			labelGroup.RockNumberLabel.Position = new Vector3(4, 0, 0);
			labelGroup.PaperLabel.Position = new Vector3(0, -1 * verticalSpacing, 0);
			labelGroup.PaperNumberLabel.Position = new Vector3(4.6f, -1 * verticalSpacing, 0);
			labelGroup.ScissorsLabel.Position = new Vector3(0, -2 * verticalSpacing, 0);
			labelGroup.ScissorsNumberLabel.Position = new Vector3(5.7f, -2 * verticalSpacing, 0);

			return labelGroup;
		}

		public void HandOffChildrenToNode(Node3D node3D)
		{
			foreach (var label in AllLabels)
			{
				node3D.AddChild(label);
			}
			
			// node3D.AddChild(RockLabel);
			// node3D.AddChild(RockNumberLabel);
			// node3D.AddChild(PaperLabel);
			// node3D.AddChild(PaperNumberLabel);
			// node3D.AddChild(ScissorsLabel);
			// node3D.AddChild(ScissorsNumberLabel);
		}
	}
}
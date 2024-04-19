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

        MeshInstance3D ExamplePoint(Color color)
        {
	        var point = new MeshInstance3D();
	        var sphereMesh = new SphereMesh();
	        sphereMesh.Height = 0.06f;
	        sphereMesh.Radius = 0.03f;
	        var mat = new StandardMaterial3D();
	        mat.AlbedoColor = color;
	        sphereMesh.SurfaceSetMaterial(0, mat);
	        point.Mesh = sphereMesh;
	        point.Scale = Vector3.Zero;
	        return point;
        }
        Arrow ExamplePointArrow(Node3D nodeThatHeadFollows, float rotation)
        {
	        var arrow1 = Arrow.CreateArrow();
	        AddChild(arrow1);
	        arrow1.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
	        // arrow.Owner = GetTree().EditedSceneRoot;
	        arrow1.Name = "Arrow";
	        arrow1.nodeThatHeadFollows = nodeThatHeadFollows;
	        arrow1.XYPlaneRotation = rotation;
	        arrow1.Chonk = 0.25f;
	        arrow1.Length = 0.5f;
	        arrow1.HeadPadding = 0.075f;
	        return arrow1;
        }

        var examplePoint = ExamplePoint(PrimerColor.orange);
        ternaryGraph.AddChild(examplePoint);
        examplePoint.Owner = GetTree().EditedSceneRoot;
        examplePoint.Position = TernaryGraph.CoordinatesToPosition(0.2f, 0.4f, 0.4f);

        RegisterAnimation(examplePoint.ScaleTo(1));

        
        var arrow = ExamplePointArrow(examplePoint, rotation: 30);
        RegisterAnimation(arrow.ScaleUpFromTail());

        Node3D CoordinateLabelParent(out RPSLabelGroup rpsLabelGroup, float rockNumber, float paperNumber, float scissorsNumber)
        {
	        var node3D = new Node3D();
	        rpsLabelGroup = RPSLabelGroup.Create(rockNumber, paperNumber, scissorsNumber);
	        rpsLabelGroup.HandOffChildrenToNode(node3D);
	        AddChild(node3D);
	        node3D.Name = "RPSLabel";
	        node3D.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
	        foreach (var latexNode in rpsLabelGroup.AllLabels)
	        {
		        latexNode.Scale = Vector3.Zero;
	        }

	        return node3D;
        }

        // var coordinateLabel = LatexNode.Create("(0.2, 0.4, 0.4)");
        // coordinateLabel.Scale = Vector3.Zero;
        // coordinateLabel.ScaleTo(1);
        // AddChild(coordinateLabel);
        // coordinateLabel.Owner = GetTree().EditedSceneRoot;

        var coordinateLabelParent = CoordinateLabelParent(out var coordinateLabelGroup, 30, 40, 40);
        coordinateLabelParent.Position = new Vector3(1, 0.735f, 0);
        coordinateLabelParent.Scale = Vector3.One * 0.04f;

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
        cartesianGraph.ZAxis.length = 0;
        
        cartesianGraph.XAxis.length = 1.15f;
        cartesianGraph.XAxis.Chonk = 0.5f;
        cartesianGraph.XAxis.Max = 1;
        cartesianGraph.XAxis.TicStep = 1;
        cartesianGraph.XAxis.Padding = new Vector2(0.1f, 0.1f);
        cartesianGraph.XAxisLabel = "Paper";
        cartesianGraph.XAxisLabelOffset = 0.4f;
        cartesianGraph.XAxisLabelScale = 0.08f;
        cartesianGraph.XAxisAlignment = Graph.AxisLabelAlignmentOptions.End;
        
        cartesianGraph.YAxis.length = 1.15f;
        cartesianGraph.YAxis.Chonk = 0.5f;
        cartesianGraph.YAxis.TicStep = 1;
        cartesianGraph.YAxis.Max = 1;
        cartesianGraph.YAxis.Padding = new Vector2(0.1f, 0.1f);
        cartesianGraph.YAxisLabel = "Scissors";
        cartesianGraph.YAxisLabelOffset = 0.3f;
        cartesianGraph.YAxisLabelScale = 0.08f;
        cartesianGraph.YAxisAlignment = Graph.AxisLabelAlignmentOptions.End;
        
        cartesianGraph.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
        cartesianGraph.Transition();

        cartesianGraph.Position = Vector3.Down * 0.15f;
        cartesianGraph.Scale = Vector3.Zero;
        RegisterAnimation(cartesianGraph.ScaleTo(1));

        var allPaperPoint = ExamplePoint(EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper]);
        cartesianGraph.AddChild(allPaperPoint);
        // allPaperPoint.Owner = GetTree().EditedSceneRoot;
        allPaperPoint.Position = cartesianGraph.DataSpaceToPositionSpace(new Vector3(1, 0, 0));
		RegisterAnimation(allPaperPoint.ScaleTo(1));
		var allPaperArrow = ExamplePointArrow(allPaperPoint, 45);
		RegisterAnimation(allPaperArrow.ScaleUpFromTail());
		var allPaperLabelParent = CoordinateLabelParent(out var allPaperLabelGroup, 0, 100, 0);
		allPaperLabelParent.Position = new Vector3(1.45f, 0.3f, 0);
		allPaperLabelParent.Scale = Vector3.One * 0.04f;
		RegisterAnimation(allPaperLabelGroup.AllLabels.Select(x => x.ScaleTo(1)).RunInParallel());

		var allScissorsPoint = ExamplePoint(EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors]);
		cartesianGraph.AddChild(allScissorsPoint);
		// allScissorsPoint.Owner = GetTree().EditedSceneRoot;
		allScissorsPoint.Position = cartesianGraph.DataSpaceToPositionSpace(new Vector3(0, 1, 0));
		RegisterAnimation(allScissorsPoint.ScaleTo(1));
		var allScissorsArrow = ExamplePointArrow(allScissorsPoint, 15);
		RegisterAnimation(allScissorsArrow.ScaleUpFromTail());
		var allScissorsLabelParent = CoordinateLabelParent(out var allScissorsLabelGroup, 0, 0, 100);
		allScissorsLabelParent.Position = new Vector3(0.534f, 1.123f, 0);
		allScissorsLabelParent.Scale = Vector3.One * 0.04f;
		RegisterAnimation(allScissorsLabelGroup.AllLabels.Select(x => x.ScaleTo(1)).RunInParallel());

		var halfScissorsHalfPaperPoint = ExamplePoint(PrimerColor.JuicyInterpolate(
			EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors],
			EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper],
			0.5f
		));
		cartesianGraph.AddChild(halfScissorsHalfPaperPoint);
		// halfScissorsHalfPaperPoint.Owner = GetTree().EditedSceneRoot;
		halfScissorsHalfPaperPoint.Position = cartesianGraph.DataSpaceToPositionSpace(new Vector3(0.5f, 0.5f, 0));
		RegisterAnimation(halfScissorsHalfPaperPoint.ScaleTo(1));
		var halfScissorsHalfPaperArrow = ExamplePointArrow(halfScissorsHalfPaperPoint, 30);
		RegisterAnimation(halfScissorsHalfPaperArrow.ScaleUpFromTail());
		var halfScissorsHalfPaperLabelParent = CoordinateLabelParent(out var halfScissorsHalfPaperLabelGroup, 0, 50, 50);
		halfScissorsHalfPaperLabelParent.Position = new Vector3(1f, 0.722f, 0);
		halfScissorsHalfPaperLabelParent.Scale = Vector3.One * 0.04f;
		RegisterAnimation(halfScissorsHalfPaperLabelGroup.AllLabels.Select(x => x.ScaleTo(1)).RunInParallel());

		var paperScissorsLine = cartesianGraph.AddLine();
		paperScissorsLine.SetData(
			new Vector3(0, 1, 0),
			new Vector3(1, 0, 0)
		);
		paperScissorsLine.Width = 17;
		RegisterAnimation(paperScissorsLine.Transition());

		// halfScissorsHalfPaperLabelGroup.PaperNumberLabel.numberSuffix = "\\%";
		// halfScissorsHalfPaperLabelGroup.ScissorsNumberLabel.numberSuffix = "\\%";
		RegisterAnimation(
			halfScissorsHalfPaperPoint.MoveTo(cartesianGraph.DataSpaceToPositionSpace(new Vector3(0.2f, 0.8f, 0))),
			halfScissorsHalfPaperLabelGroup.PaperNumberLabel.AnimateNumericalExpression(80),
			halfScissorsHalfPaperLabelGroup.ScissorsNumberLabel.AnimateNumericalExpression(20),
			halfScissorsHalfPaperArrow.MoveTo(halfScissorsHalfPaperPoint.GlobalPosition)
		);
		RegisterAnimation(
			halfScissorsHalfPaperPoint.MoveTo(cartesianGraph.DataSpaceToPositionSpace(new Vector3(0.8f, 0.2f, 0))),
			halfScissorsHalfPaperLabelGroup.PaperNumberLabel.AnimateNumericalExpression(20),
			halfScissorsHalfPaperLabelGroup.ScissorsNumberLabel.AnimateNumericalExpression(80),
			halfScissorsHalfPaperArrow.MoveTo(halfScissorsHalfPaperPoint.GlobalPosition)
		);
		RegisterAnimation(
			halfScissorsHalfPaperPoint.MoveTo(cartesianGraph.DataSpaceToPositionSpace(new Vector3(0.5f, 0.5f, 0))),
			halfScissorsHalfPaperLabelGroup.PaperNumberLabel.AnimateNumericalExpression(50),
			halfScissorsHalfPaperLabelGroup.ScissorsNumberLabel.AnimateNumericalExpression(50),
			halfScissorsHalfPaperArrow.MoveTo(halfScissorsHalfPaperPoint.GlobalPosition)
		);

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
		
		public static RPSLabelGroup Create(float r, float p, float s)
		{
			var labelGroup = new RPSLabelGroup();

			labelGroup.RockLabel = LatexNode.Create("Rock:");
			labelGroup.PaperLabel = LatexNode.Create("Paper:");
			labelGroup.ScissorsLabel = LatexNode.Create("Scissors:");
			labelGroup.RockNumberLabel = LatexNode.Create("");
			labelGroup.RockNumberLabel.numberSuffix = "\\%";
			labelGroup.RockNumberLabel.NumericalExpression = r;
			labelGroup.PaperNumberLabel = LatexNode.Create("");
			labelGroup.PaperNumberLabel.numberSuffix = "\\%";
			labelGroup.PaperNumberLabel.NumericalExpression = p;
			labelGroup.ScissorsNumberLabel = LatexNode.Create("");
			labelGroup.ScissorsNumberLabel.numberSuffix = "\\%";
			labelGroup.ScissorsNumberLabel.NumericalExpression = s;

			foreach (var label in labelGroup.AllLabels)
			{
				label.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Left;
				label.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Baseline;
			}

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
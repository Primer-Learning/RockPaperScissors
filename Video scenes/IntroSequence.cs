using Godot;
using Primer;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.LaTeX;

[Tool]
public partial class IntroSequence : AnimationSequence
{
	private Rng rng = new Rng(0);
	
	protected override void Define()
	{
		#region Prep
		var camRig = GetParent().GetNode<CameraRig>("CameraRig");
		camRig.Distance = 14;
		camRig.Position = Vector3.Up * 2;
		camRig.RotationDegrees = new Vector3(-17, 0, 0);

		// Ground
		var ground = new MeshInstance3D();
		ground.Name = "ground";
		AddChild(ground);
		var groundMesh = new PlaneMesh();
		ground.Mesh = groundMesh;
		var groundMaterial = new StandardMaterial3D();
		groundMaterial.AlbedoColor = new Color(0, 0, 0, 0);
		groundMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		groundMesh.Material = groundMaterial;
		groundMesh.Size = new Vector2(100, 100);
		var groundStaticBody = new StaticBody3D();
		var groundCollisionShape3D = new CollisionShape3D();
		var groundCollisionShape = new WorldBoundaryShape3D();
		groundCollisionShape.Plane = new Plane(0, 1, 0, 0);
		groundStaticBody.AddChild(groundCollisionShape3D);
		groundCollisionShape3D.Shape = groundCollisionShape;
		ground.AddChild(groundStaticBody);
		// // ground.Owner = GetTree().EditedSceneRoot;


		var conflictSite1 = new ExampleConflictTree(
			this,
			position: Vector3.Zero,
			EvoGameTheorySim.RPSGame.Strategy.Rock,
			EvoGameTheorySim.RPSGame.Strategy.Scissors,
			rng
		);
		var conflictSite2 = new ExampleConflictTree(
			this,
			Vector3.Right * 9,
			EvoGameTheorySim.RPSGame.Strategy.Scissors,
			EvoGameTheorySim.RPSGame.Strategy.Paper,
			rng
		);
		var conflictSite3 = new ExampleConflictTree(
			this,
			Vector3.Right * 18,
			EvoGameTheorySim.RPSGame.Strategy.Paper,
			EvoGameTheorySim.RPSGame.Strategy.Rock,
			rng
		);
		
		#region Triangle diagram

		var triangleDiagram = new Node3D();
		triangleDiagram.Position = new Vector3(9, 9, 0);
		AddChild(triangleDiagram);
		// triangleDiagram.Owner = GetTree().EditedSceneRoot;
		triangleDiagram.Name = "TriangleDiagram";
		
		var rockText = new LatexNode();
		rockText.Name = "RockText";
		rockText.latex = "Rock";
		rockText.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		rockText.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		rockText.UpdateCharacters();
		rockText.SetColor(EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Rock]);
		triangleDiagram.AddChild(rockText);
		rockText.GlobalPosition = new Vector3(9, 12.5f, 0);
		rockText.Scale = Vector3.Zero;
		// rockText.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		// // rockText.Owner = GetTree().EditedSceneRoot;
		
		var scissorsText = new LatexNode();
		scissorsText.Name = "ScissorsText";
		scissorsText.latex = "Scissors";
		scissorsText.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		scissorsText.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		scissorsText.UpdateCharacters();
		scissorsText.SetColor(EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors]);
		triangleDiagram.AddChild(scissorsText);
		scissorsText.GlobalPosition = new Vector3(13.5f, 6.5f, 0);
		scissorsText.Scale = Vector3.Zero;
		// scissorsText.Owner = GetTree().EditedSceneRoot;
		
		var paperText = new LatexNode();
		paperText.Name = "PaperText";
		paperText.latex = "Paper";
		paperText.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		paperText.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		paperText.UpdateCharacters();
		paperText.SetColor(EvoGameTheorySimAnimator.StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper]);
		triangleDiagram.AddChild(paperText);
		paperText.GlobalPosition = new Vector3(4.5f, 6.5f, 0);
		paperText.Scale = Vector3.Zero;
		// paperText.Owner = GetTree().EditedSceneRoot;
		
		var rockBeatsScissors = new LatexNode();
		rockBeatsScissors.Name = "rockBeatsScissors";
		rockBeatsScissors.latex = "Beats";
		rockBeatsScissors.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		rockBeatsScissors.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		rockBeatsScissors.UpdateCharacters();
		triangleDiagram.AddChild(rockBeatsScissors);
		rockBeatsScissors.GlobalPosition = new Vector3(12.9f, 10.4f, 0);
		rockBeatsScissors.Scale = Vector3.Zero;
		// rockBeatsScissors.Owner = GetTree().EditedSceneRoot;
		
		var scissorsBeatsPaper = new LatexNode();
		scissorsBeatsPaper.Name = "scissorsBeatsPaper";
		scissorsBeatsPaper.latex = "Beats";
		scissorsBeatsPaper.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		scissorsBeatsPaper.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		scissorsBeatsPaper.UpdateCharacters();
		triangleDiagram.AddChild(scissorsBeatsPaper);
		scissorsBeatsPaper.GlobalPosition = new Vector3(9f, 5.5f, 0);
		scissorsBeatsPaper.Scale = Vector3.Zero;
		// scissorsBeatsPaper.Owner = GetTree().EditedSceneRoot;
		
		var paperBeatsRock = new LatexNode();
		paperBeatsRock.Name = "paperBeatsRock";
		paperBeatsRock.latex = "Beats";
		paperBeatsRock.HorizontalAlignment = LatexNode.HorizontalAlignmentOptions.Center;
		paperBeatsRock.VerticalAlignment = LatexNode.VerticalAlignmentOptions.Center;
		paperBeatsRock.UpdateCharacters();
		triangleDiagram.AddChild(paperBeatsRock);
		paperBeatsRock.GlobalPosition = new Vector3(5.1f, 10.4f, 0);
		paperBeatsRock.Scale = Vector3.Zero;
		// paperBeatsRock.Owner = GetTree().EditedSceneRoot;
		
		var arrow1 = Arrow.ArrowScene.Instantiate<Arrow>();
		triangleDiagram.AddChild(arrow1);
		// arrow1.Owner = GetTree().EditedSceneRoot;
		arrow1.nodeThatTailFollows = rockText;
		arrow1.nodeThatHeadFollows = scissorsText;
		arrow1.width = 3;
		arrow1.headPadding = 1f;
		arrow1.tailPadding = 1.5f;
		arrow1.Update();
		arrow1.Scale = Vector3.Zero;
		
		var arrow2 = Arrow.ArrowScene.Instantiate<Arrow>();
		triangleDiagram.AddChild(arrow2);
		// arrow2.Owner = GetTree().EditedSceneRoot;
		arrow2.nodeThatTailFollows = scissorsText;
		arrow2.nodeThatHeadFollows = paperText;
		arrow2.width = 3;
		arrow2.headPadding = 2;
		arrow2.tailPadding = 2.5f;
		arrow2.Update();
		arrow2.Scale = Vector3.Zero;
		
		var arrow3 = Arrow.ArrowScene.Instantiate<Arrow>();
		triangleDiagram.AddChild(arrow3);
		// arrow3.Owner = GetTree().EditedSceneRoot;
		arrow3.nodeThatTailFollows = paperText;
		arrow3.nodeThatHeadFollows = rockText;
		arrow3.width = 3;
		arrow3.headPadding = 1.5f;
		arrow3.tailPadding = 1f;
		arrow3.Update();
		arrow3.Scale = Vector3.Zero;
		#endregion
		
		#endregion
		
		#region First three trees
		
		RegisterAnimation(conflictSite1.BlobsAppear());
		RegisterAnimation(conflictSite1.GrowTreeAndFruit(), 0.9f);

		RegisterAnimation(
			AnimationUtilities.Parallel(
				AnimationUtilities.Parallel(
					// Actual intended movement
					camRig.MoveTo(new Vector3(9f, 6, 0)),
					camRig.RotateTo(0, 0, 0),
					camRig.ZoomTo(30)
				),
				AnimationUtilities.Parallel(
					conflictSite2.GrowTreeAndFruit(),
					conflictSite2.BlobsAppear()
				).WithDelay(0.25f),
				AnimationUtilities.Parallel(
					conflictSite3.GrowTreeAndFruit(),
					conflictSite3.BlobsAppear()
				).WithDelay(0.5f)
			),
			4.5f
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				// Rock beats scissors
				AnimationUtilities.Series(
					conflictSite1.BlobsFaceEachOther(),
					conflictSite1.Showdown()
				),
				rockText.ScaleTo(0.75f),
				scissorsText.ScaleTo(0.75f),
				paperText.ScaleTo(0.75f),
				arrow1.ScaleUpFromTail(),
				rockBeatsScissors.ScaleTo(0.5f),
				
				arrow2.ScaleUpFromTail().WithDelay(1.5f),
				scissorsBeatsPaper.ScaleTo(0.5f).WithDelay(1.5f),
				
				arrow3.ScaleUpFromTail().WithDelay(3f),
				paperBeatsRock.ScaleTo(0.5f).WithDelay(3f),
				
				AnimationUtilities.Series(
					conflictSite2.BlobsFaceEachOther(),
					conflictSite2.Showdown()
				).WithDelay(1.5f),
				
				AnimationUtilities.Series(
					conflictSite3.BlobsFaceEachOther(),
					conflictSite3.Showdown()
				).WithDelay(3)
			),
			6.5f
		);
		
		RegisterAnimation(triangleDiagram.MoveTo(new Vector3(2, 9, 0)), 13);
		
		var table = new Table();
		AddChild(table);
		table.Name = "Reward matrix";
		table.Position = new Vector3(10.85f, 11.5f, 0);
		table.Scale = Vector3.One * 0.85f;
		
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

		RegisterAnimation(
			table.ScaleCellToDefault(0, 0),
			table.ScaleCellToDefault(1, 0).WithDelay(0.1f),
			table.ScaleCellToDefault(0, 1).WithDelay(0.1f),
			table.ScaleCellToDefault(2, 0).WithDelay(0.2f),
			table.ScaleCellToDefault(0, 2).WithDelay(0.2f),
			table.ScaleCellToDefault(3, 0).WithDelay(0.3f),
			table.ScaleCellToDefault(0, 3).WithDelay(0.3f)
		);
		
		RegisterAnimation(
			AnimationUtilities.Parallel(
				table.ScaleCellToDefault(3, 1),
				table.ScaleCellToDefault(1, 3),
				conflictSite1.EatFruit()
			),
			26
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				table.ScaleCellToDefault(3, 2),
				table.ScaleCellToDefault(2, 3),
				conflictSite2.EatFruit()
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				table.ScaleCellToDefault(1, 2),
				table.ScaleCellToDefault(2, 1),
				conflictSite3.EatFruit()
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				table.ScaleCellToDefault(1, 1),
				table.ScaleCellToDefault(2, 2).WithDelay(0.1f),
				table.ScaleCellToDefault(3, 3).WithDelay(0.2f)
			),
			32
		);
		
		#endregion
		
		// Push everything beyond here back.
		// RegisterAnimation( new Animation(), 1000);
		
		// #region Up to six trees
		//
		// RegisterAnimation(
		// 	AnimationUtilities.Parallel(
		// 		// Actual intended movement
		// 		camRig.MoveTo(new Vector3(9f, 0, 0)),
		// 		camRig.ZoomTo(30)
		// 	)
		// );
		//
		// var conflictSite4 = new ExampleConflictTree(
		// 	this,
		// 	position: Vector3.Zero + Vector3.Down * 7,
		// 	EvoGameTheorySim.RPSGame.Strategy.Rock,
		// 	EvoGameTheorySim.RPSGame.Strategy.Rock,
		// 	rng
		// );
		// RegisterAnimation(conflictSite4.GrowTreeAndFruit());
		// RegisterAnimation(conflictSite4.BlobsAppear());
		// RegisterAnimation(conflictSite4.Showdown());
		// RegisterAnimation(conflictSite4.EatFruit());
		//
		//
		// var conflictSite5 = new ExampleConflictTree(
		// 	this,
		// 	Vector3.Right * 9 + Vector3.Down * 7,
		// 	EvoGameTheorySim.RPSGame.Strategy.Paper,
		// 	EvoGameTheorySim.RPSGame.Strategy.Paper,
		// 	rng
		// );
		// RegisterAnimation(conflictSite5.GrowTreeAndFruit());
		// RegisterAnimation(conflictSite5.BlobsAppear());
		// RegisterAnimation(conflictSite5.Showdown());
		// RegisterAnimation(conflictSite5.EatFruit());
		//
		// var conflictSite6 = new ExampleConflictTree(
		// 	this,
		// 	Vector3.Right * 18 + Vector3.Down * 7,
		// 	EvoGameTheorySim.RPSGame.Strategy.Scissors,
		// 	EvoGameTheorySim.RPSGame.Strategy.Scissors,
		// 	rng
		// );
		// RegisterAnimation(conflictSite6.GrowTreeAndFruit());
		// RegisterAnimation(conflictSite6.BlobsAppear());
		// RegisterAnimation(conflictSite6.Showdown());
		// RegisterAnimation(conflictSite6.EatFruit());
		//
		// #endregion
	}
}

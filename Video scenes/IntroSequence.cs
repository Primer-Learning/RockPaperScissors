using Godot;
using Primer;
using PrimerTools;
using PrimerTools.AnimationSequence;

[Tool]
public partial class IntroSequence : AnimationSequence
{
	private Rng rng = new Rng(0);
	
	protected override void Define()
	{
		var camRig = GetParent().GetNode<CameraRig>("CameraRig");
		camRig.Distance = 14;
		camRig.Position = Vector3.Up * 2;
		camRig.RotationDegrees = new Vector3(-17, 0, 0);

		#region First tree
		var tree1 = FruitTree.TreeScene.Instantiate<FruitTree>();
		tree1.rng = rng;
		AddChild(tree1);
		tree1.Owner = GetTree().EditedSceneRoot;
		tree1.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		tree1.Position = Vector3.Zero;
		tree1.Scale = Vector3.Zero;
		
		RegisterAnimation(tree1.ScaleTo(1));
		RegisterAnimation(
			tree1.GrowFruit(0),
			tree1.GrowFruit(1)
		);
		
		var paperBlob1 = Blob.BlobScene.Instantiate<Blob>();
		AddChild(paperBlob1);
		paperBlob1.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		paperBlob1.Scale = Vector3.Zero;
		paperBlob1.Position = new Vector3(3, 0, 0);
		paperBlob1.SetColor(PrimerColor.blue);
		
		var scissorsBlob1 = Blob.BlobScene.Instantiate<Blob>();
		scissorsBlob1.Name = "ScissorsBlob1";
		AddChild(scissorsBlob1);
		scissorsBlob1.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		scissorsBlob1.Scale = Vector3.Zero;
		scissorsBlob1.Position = new Vector3(-3, 0, 0);
		scissorsBlob1.SetColor(PrimerColor.yellow);

		RegisterAnimation(
			AnimationUtilities.Parallel(
				paperBlob1.ScaleTo(1),
				scissorsBlob1.ScaleTo(1)
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				paperBlob1.WalkTo(new Vector3(2, 0, 2)),
				scissorsBlob1.WalkTo(new Vector3(-2, 0, 2))
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				paperBlob1.RotateTo(new Vector3(0, -80, 0)),
				scissorsBlob1.RotateTo(new Vector3(0, 80, 0))
			)
		);
		
		RegisterAnimation(scissorsBlob1.ScissorsPaperShowdown(paperBlob1, true));
		#endregion
		
		RegisterAnimation(
			AnimationUtilities.Parallel(
				camRig.MoveTo(new Vector3(4.5f, 2, 0)),
				camRig.ZoomTo(22)
			)
		);

		#region Second tree

		var tree2 = FruitTree.TreeScene.Instantiate<FruitTree>();
		tree2.rng = rng;
		AddChild(tree2);
		tree2.Owner = GetTree().EditedSceneRoot;
		tree2.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		tree2.Position = Vector3.Right * 9;
		tree2.Scale = Vector3.Zero;
		
		RegisterAnimation(tree2.ScaleTo(1));
		RegisterAnimation(
			tree2.GrowFruit(0),
			tree2.GrowFruit(1)
		);
		
		var rockBlob1 = Blob.BlobScene.Instantiate<Blob>();
		AddChild(rockBlob1);
		rockBlob1.Name = "RockBlob1";
		rockBlob1.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		rockBlob1.Scale = Vector3.Zero;
		rockBlob1.Position = tree2.Position + new Vector3(3, 0, 0);
		rockBlob1.SetColor(PrimerColor.red);
		
		var rockBlob2 = Blob.BlobScene.Instantiate<Blob>();
		rockBlob2.Name = "RockBlob2";
		AddChild(rockBlob2);
		rockBlob2.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		rockBlob2.Scale = Vector3.Zero;
		rockBlob2.Position = tree2.Position + new Vector3(-3, 0, 0);
		rockBlob2.SetColor(PrimerColor.red);

		RegisterAnimation(
			AnimationUtilities.Parallel(
				rockBlob1.ScaleTo(1),
				rockBlob2.ScaleTo(1)
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				rockBlob1.WalkTo(tree2.Position + new Vector3(2, 0, 2)),
				rockBlob2.WalkTo(tree2.Position + new Vector3(-2, 0, 2))
			)
		);
		RegisterAnimation(
			AnimationUtilities.Parallel(
				rockBlob1.RotateTo(new Vector3(0, -80, 0)),
				rockBlob2.RotateTo(new Vector3(0, 80, 0))
			)
		);
		
		RegisterAnimation(rockBlob2.ScissorsPaperShowdown(rockBlob1, true));

		#endregion
	}
}

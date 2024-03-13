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
		var tree = FruitTree.TreeScene.Instantiate<FruitTree>();
		tree.rng = rng;
		AddChild(tree);
		tree.Owner = GetTree().EditedSceneRoot;
		tree.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
		tree.Position = Vector3.Zero;
		tree.Scale = Vector3.Zero;
		
		RegisterAnimation(tree.ScaleTo(1));

		RegisterAnimation(
			tree.GrowFruit(0),
			tree.GrowFruit(1)
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
		
		// RegisterAnimation(scissorsBlob1.SetUpPredefinedAnimation("cast_spell"));
		// RegisterAnimation(scissorsBlob1.ScissorsSnip());
		RegisterAnimation(scissorsBlob1.ScissorsPaperShowdown(paperBlob1, true));
	}
}

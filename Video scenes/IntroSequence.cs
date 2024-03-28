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
		ground.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);

		#region First tree

		var conflictSite1 = new ExampleConflictTree(
			this,
			position: Vector3.Zero,
			EvoGameTheorySim.RPSGame.Strategy.Rock,
			EvoGameTheorySim.RPSGame.Strategy.Scissors,
			rng
		);
		RegisterAnimation(conflictSite1.GrowTreeAndFruit());
		RegisterAnimation(conflictSite1.BlobsAppear());
		RegisterAnimation(conflictSite1.Showdown());
		RegisterAnimation(conflictSite1.EatFruit());
		#endregion
		
		#region Second tree
		
		RegisterAnimation(
			AnimationUtilities.Parallel(
				// Actual intended movement
				camRig.MoveTo(new Vector3(4.5f, 2, 0)),
				camRig.ZoomTo(22)
			)
		);
		
		var conflictSite2 = new ExampleConflictTree(
			this,
			Vector3.Right * 9,
			EvoGameTheorySim.RPSGame.Strategy.Scissors,
			EvoGameTheorySim.RPSGame.Strategy.Paper,
			rng
		);
		RegisterAnimation(conflictSite2.GrowTreeAndFruit());
		RegisterAnimation(conflictSite2.BlobsAppear());
		RegisterAnimation(conflictSite2.Showdown());
		RegisterAnimation(conflictSite2.EatFruit());
		
		#endregion
		
		#region Second tree
		
		RegisterAnimation(
			AnimationUtilities.Parallel(
				// Actual intended movement
				camRig.MoveTo(new Vector3(9f, 2, 0)),
				camRig.ZoomTo(30)
			)
		);
		var conflictSite3 = new ExampleConflictTree(
			this,
			Vector3.Right * 18,
			EvoGameTheorySim.RPSGame.Strategy.Paper,
			EvoGameTheorySim.RPSGame.Strategy.Rock,
			rng
		);
		RegisterAnimation(conflictSite3.GrowTreeAndFruit());
		RegisterAnimation(conflictSite3.BlobsAppear());
		RegisterAnimation(conflictSite3.Showdown());
		RegisterAnimation(conflictSite3.EatFruit());
		
		#endregion
	}
}

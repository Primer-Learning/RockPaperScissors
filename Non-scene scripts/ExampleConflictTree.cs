using System;
using System.ComponentModel;
using Godot;
using Primer;
using PrimerTools;

public class ExampleConflictTree
{
	private readonly Rng rng;
	private readonly Node parent;

	public readonly Blob Blob1;
	private EvoGameTheorySim.RPSGame.Strategy strategy1;
	public readonly Blob Blob2;
	private EvoGameTheorySim.RPSGame.Strategy strategy2;
	
	public readonly FruitTree Tree;

	public ExampleConflictTree(
		Node parent,
		Vector3 position,
		EvoGameTheorySim.RPSGame.Strategy strategy1,
		EvoGameTheorySim.RPSGame.Strategy strategy2,
		Rng rng = null
		)
	{
		if (rng == null)
		{
			rng = new Rng(0);
		}
		this.rng = rng;

		this.parent = parent;
		this.strategy1 = strategy1;
		this.strategy2 = strategy2;
		
		Tree = FruitTree.TreeScene.Instantiate<FruitTree>();
		Tree.rng = rng;
		parent.AddChild(Tree);
		Tree.Owner = parent.GetTree().EditedSceneRoot;
		Tree.MakeSelfAndChildrenLocal(parent.GetTree().EditedSceneRoot);
		Tree.Position = position;
		Tree.Scale = Vector3.Zero;

		Blob1 = CreateBlob(strategy1);
		Blob1.Scale = Vector3.Zero;
		Blob1.Position = position + new Vector3(-2, 0, 2);
		Blob2 = CreateBlob(strategy2);
		Blob2.Scale = Vector3.Zero;
		Blob2.Position = position + new Vector3(2, 0, 2);
	}

	private Blob CreateBlob(EvoGameTheorySim.RPSGame.Strategy strategy)
	{
		var blob = Blob.BlobScene.Instantiate<Blob>();
		blob.SetColor(EvoGameTheorySimAnimator.StrategyColors[strategy]);
		parent.AddChild(blob);
		blob.MakeSelfAndChildrenLocal(parent.GetTree().EditedSceneRoot);
		return blob;
	}

	public Animation GrowTreeAndFruit()
	{
		return AnimationUtilities.Series(
			Tree.ScaleTo(1),
			AnimationUtilities.Parallel(
				Tree.GrowFruit(0),
				Tree.GrowFruit(1)
			)
		);
	}
	
	public Animation BlobsAppear()
	{
		return AnimationUtilities.Series(
			AnimationUtilities.Parallel(
				Blob1.ScaleTo(1),
				Blob2.ScaleTo(1)
			),
			AnimationUtilities.Parallel(
				Blob1.RotateTo(new Vector3(0, 80, 0)),
				Blob2.RotateTo(new Vector3(0, -80, 0))
			)
		);
	}

	public Animation Showdown()
	{
		return strategy1 switch
		{
			EvoGameTheorySim.RPSGame.Strategy.Rock => strategy2 switch
			{
				EvoGameTheorySim.RPSGame.Strategy.Rock => Blob1.RockRockShowdown(Blob2),
				EvoGameTheorySim.RPSGame.Strategy.Paper => Blob2.PaperRockShowdown(Blob1),
				EvoGameTheorySim.RPSGame.Strategy.Scissors =>
					// This particular animation breaks the pattern of the winner being on the left
					// Which is Blob1. So that's why the order is reversed.
					Blob2.RockScissorsShowdown(Blob1),
				_ => throw new ArgumentOutOfRangeException()
			},
			EvoGameTheorySim.RPSGame.Strategy.Paper => strategy2 switch
			{
				EvoGameTheorySim.RPSGame.Strategy.Rock => Blob1.PaperRockShowdown(Blob2),
				EvoGameTheorySim.RPSGame.Strategy.Paper => Blob1.PaperPaperShowdown(Blob2),
				EvoGameTheorySim.RPSGame.Strategy.Scissors => Blob2.ScissorsPaperShowdown(Blob1),
				_ => throw new ArgumentOutOfRangeException()
			},
			EvoGameTheorySim.RPSGame.Strategy.Scissors => strategy2 switch
			{
				EvoGameTheorySim.RPSGame.Strategy.Rock =>
					// This particular animation breaks the pattern of the winner being on the left
					// Which is Blob1. So that's why the order is reversed.
					Blob1.RockScissorsShowdown(Blob2),
				EvoGameTheorySim.RPSGame.Strategy.Paper => Blob1.ScissorsPaperShowdown(Blob2),
				EvoGameTheorySim.RPSGame.Strategy.Scissors => Blob1.ScissorsScissorsShowdown(Blob2),
				_ => throw new ArgumentOutOfRangeException()
			},
			_ => throw new ArgumentOutOfRangeException()
		};
	}
	
	public Animation EatFruit()
	{
		switch (strategy1)
		{
			case EvoGameTheorySim.RPSGame.Strategy.Rock:
				switch (strategy2)
				{
					case EvoGameTheorySim.RPSGame.Strategy.Rock:
						return AnimationUtilities.Parallel(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Paper:
						return AnimationUtilities.Series(
							Blob2.Eat(Tree.FindNearestFruit(Blob2)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Scissors:
						return AnimationUtilities.Series(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob1.Eat(Tree.FindNearestFruit(Blob1))
						);
					default:
						throw new ArgumentOutOfRangeException();
				}
			case EvoGameTheorySim.RPSGame.Strategy.Paper:
				switch (strategy2)
				{
					case EvoGameTheorySim.RPSGame.Strategy.Rock:
						return AnimationUtilities.Series(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob1.Eat(Tree.FindNearestFruit(Blob1))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Paper:
						return AnimationUtilities.Parallel(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Scissors:
						return AnimationUtilities.Series(
							Blob2.Eat(Tree.FindNearestFruit(Blob2)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					default:
						throw new ArgumentOutOfRangeException();
				}
			case EvoGameTheorySim.RPSGame.Strategy.Scissors:
				switch (strategy2)
				{
					case EvoGameTheorySim.RPSGame.Strategy.Rock:
						return AnimationUtilities.Series(
							Blob2.Eat(Tree.FindNearestFruit(Blob2)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Paper:
						return AnimationUtilities.Series(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob1.Eat(Tree.FindNearestFruit(Blob1))
						);
					case EvoGameTheorySim.RPSGame.Strategy.Scissors:
						return AnimationUtilities.Parallel(
							Blob1.Eat(Tree.FindNearestFruit(Blob1)),
							Blob2.Eat(Tree.FindNearestFruit(Blob2))
						);
					default:
						throw new ArgumentOutOfRangeException();
				}
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}

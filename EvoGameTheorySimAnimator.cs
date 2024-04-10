using System.Collections.Generic;
using System.Linq;
using Godot;
using Primer;
using PrimerTools;
using PrimerTools.Graph;

public partial class EvoGameTheorySimAnimator : Node3D
{
    public bool AnimateBlobs = true;
    
    public EvoGameTheorySim Sim;
    public MeshInstance3D Ground;
    private Vector2 _groundSize = new (10, 10);
    private readonly List<Node3D> _trees = new();
    private readonly List<Node3D> _homes = new();
    private readonly Rng _simVisualizationRng = new Rng(2);
    
    private float _creatureSpeed = 10f;
    
    public static readonly Dictionary<EvoGameTheorySim.RPSGame.Strategy, Color> StrategyColors = new()
    {
        { EvoGameTheorySim.RPSGame.Strategy.Rock, PrimerColor.red },
        { EvoGameTheorySim.RPSGame.Strategy.Paper, PrimerColor.blue },
        { EvoGameTheorySim.RPSGame.Strategy.Scissors, PrimerColor.yellow }
    };

    public Animation MakeGroundAndAnimateAppearance(Vector2 size = default)
    {
        // Make the ground plane
        Ground = new MeshInstance3D();
        Ground.Name = "Ground";
        AddChild(Ground);
        Ground.Owner = GetTree().EditedSceneRoot;
        var planeMesh = new PlaneMesh();

        if (size != Vector2.Zero) _groundSize = size;
        planeMesh.Size = _groundSize;
        Ground.Mesh = planeMesh;
        
        Ground.Position = Vector3.Right * 6;
        var groundMaterial = new StandardMaterial3D();
        groundMaterial.AlbedoColor = new Color(0x035800ff);
        // groundMaterial.AlbedoColor = new Color(0xffffffff);
        Ground.Mesh.SurfaceSetMaterial(0, groundMaterial);
        
        Ground.Scale = Vector3.Zero;
        return Ground.ScaleTo(Vector3.One);
    }
    
    public Animation MakeTreesAndHomesAndAnimateAppearance()
    {
        var animations = new List<Animation>();
        
        // Add trees according to the max number of trees
        // var treeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Trees/Mango trees/Medium mango tree/Resources/mango tree medium.scn");
        for (var i = 0; i < Sim.NumTrees; i++)
        {
            var tree = FruitTree.TreeScene.Instantiate<FruitTree>();
            Ground.AddChild(tree);
            // tree.Owner = GetTree().EditedSceneRoot;
            tree.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
            tree.Name = "Tree";
            tree.Scale = Vector3.Zero;
            tree.Position = new Vector3(
                _simVisualizationRng.RangeFloat(-_groundSize.X / 2, _groundSize.X / 2),
                0,
                _simVisualizationRng.RangeFloat(-_groundSize.Y / 2, _groundSize.Y / 2)
            );
            _trees.Add(tree);
            animations.Add(tree.ScaleTo(Vector3.One * 0.1f));
        }
        
        // Add homes
        var homeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Rocks/rock_home_11.tscn");
        for (var i = 0; i < 6; i++)
        {
            var home = homeScene.Instantiate<Node3D>();
            Ground.AddChild(home);
            home.Owner = GetTree().EditedSceneRoot;
            home.Name = "Home";
            home.Scale = Vector3.Zero;
            home.Position = new Vector3(_simVisualizationRng.RangeFloat(-5, 5), 0, _simVisualizationRng.RangeFloat(-5, 5));
            _homes.Add(home);
            animations.Add(home.ScaleTo(Vector3.One * 0.5f));
        }

        return animations.RunInParallel();
    }

    public void NonAnimatedSetup()
    {
        if (IncludeTernaryPlot) SetUpTernaryPlot();
        MakeGroundAndAnimateAppearance();
        MakeTreesAndHomesAndAnimateAppearance();
    }

    public Animation AnimateAllDays()
    {
        // Initial blobs spawn in random homes
        // var blobScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Blob/Blobs/blob.tscn");
        var blobPool = new Pool<Blob>(Blob.BlobScene);
        var blobs = new Dictionary<int, Blob>();
        var parentPositions = new Dictionary<int, Vector3>();
        var dailyAnimations = new List<Animation>();
        var dayCount = 0;
        
        foreach (var entitiesToday in Sim.EntitiesByDay)
        {
            // The final day should not be animated, since it is just the final state
            // If we want to show the final blobs appearing, we'd have to include the
            // appearance step of the final day, but not needed for now.
            if (dayCount == Sim.EntitiesByDay.Length - 1) break;
            
            // Appearance step
            var appearanceAnimations = new List<Animation>();
            if (AnimateBlobs)
            {
                // Make the blobs
                foreach (var entityId in entitiesToday)
                {
                    var blob = blobPool.GetFromPool();
                    blobs.Add(entityId, blob);
                    if (blob.GetParent() == null) {Ground.AddChild(blob);}
                    blob.MakeSelfAndChildrenLocal(GetTree().EditedSceneRoot);
                    blob.Owner = GetTree().EditedSceneRoot;
                    blob.Name = "Blob";
                    blob.Scale = Vector3.Zero;

                    var parent = Sim.Registry.Parents[entityId];
                    var pos = parent == -1 ? _homes[_simVisualizationRng.RangeInt(_homes.Count)].Position : parentPositions[parent];
                    appearanceAnimations.Add(
                        AnimationUtilities.Parallel(
                            blob.MoveTo(pos, duration: 0),
                            blob.ScaleTo(Vector3.One * 0.1f),
                            blob.AnimateColor(StrategyColors[Sim.Registry.Strategies[entityId]])
                        )
                    );
                }
            }
            appearanceAnimations.Add(GraphAnimationToDay(dayCount++));
            
            // Move blobs to trees
            var toTreeAnimations = new List<Animation>();
            if (AnimateBlobs)
            {
                var numGames = entitiesToday.Count - Sim.NumTrees;
                numGames = Mathf.Max(numGames, 0);
                numGames = Mathf.Min(numGames, Sim.NumTrees);
                for (var i = 0; i < numGames; i++)
                {
                    var blob1 = blobs[entitiesToday[i * 2]];
                    var blob2 = blobs[entitiesToday[i * 2 + 1]];
                    
                    toTreeAnimations.Add(AnimateBlobMovementWithSpeed(blob1, _trees[i].Position));
                    toTreeAnimations.Add(AnimateBlobMovementWithSpeed(blob2, _trees[i].Position));
                }
                for (var i = numGames * 2; i < entitiesToday.Count; i++)
                {
                    var blob = blobs[entitiesToday[i]];
                    toTreeAnimations.Add(numGames < Sim.NumTrees
                        ? AnimateBlobMovementWithSpeed(blob, _trees[i - numGames].Position)
                        : blob.ScaleTo(Vector3.Zero));
                }
            }
            // A pause to make the total animation length close to the same whether we animate blobs or not 
            toTreeAnimations.Add(new Animation().WithDuration(0.5f));
            
            // Move the blobs home
            var toHomeAnimations = new List<Animation>();
            if (AnimateBlobs)
            {
                foreach (var entityId in entitiesToday)
                {
                    var blob = blobs[entityId];
                    toHomeAnimations.Add(
                        AnimationUtilities.Series(
                            AnimateBlobMovementWithSpeed(blob, _homes[_simVisualizationRng.RangeInt(_homes.Count)].Position),
                            blob.ScaleTo(Vector3.Zero)
                        )
                    );
                    parentPositions[entityId] = blob.Position;
                    blobPool.ReturnToPool(blob, unparent: false, makeInvisible: false);
                }
            }
            toHomeAnimations.Add(new Animation().WithDuration(0.5f));
            
            dailyAnimations.Add(AnimationUtilities.Series(
                    appearanceAnimations.RunInParallel(),
                    toTreeAnimations.RunInParallel(),
                    toHomeAnimations.RunInParallel()
                )
            );
        }
        
        // Show the final day's results in the graph
        dailyAnimations.Add(GraphAnimationToDay(dayCount));
        
        return AnimationUtilities.Series(dailyAnimations.ToArray());
    }

    private Animation AnimateBlobMovementWithSpeed(Blob blob, Vector3 destination)
    {
        var distance = (blob.Position - destination).Length();
        return blob.MoveTo(destination, duration: distance / _creatureSpeed);
    }

    private Animation GraphAnimationToDay(int dayCount)
    {
        if (IncludeTernaryPlot)
        {
            return AnimateTernaryPlotToDay(dayCount);
        }
        if (BarPlot != null)
        {
            return AnimateBarPlotToDay(dayCount);
        }
        
        // Sims don't necessarily all need a graph.
        return new Animation(); // Instead of null
    }
    
    public bool IncludeTernaryPlot;
    public TernaryGraph ternaryGraph;
    private CurvePlot2D plot;
    public void SetUpTernaryPlot()
    {
        if (ternaryGraph != null) return;
        ternaryGraph = new TernaryGraph();
        AddChild(ternaryGraph);
        ternaryGraph.Owner = GetTree().EditedSceneRoot;
        ternaryGraph.Scale = Vector3.One * 10;
        ternaryGraph.Position = Vector3.Left * 11;
        ternaryGraph.Labels = new [] {"Rock", "Paper", "Scissors"};
        ternaryGraph.Colors = new []
        {
            StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Rock],
            StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper],
            StrategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors]
        };
        ternaryGraph.CreateBounds();
        
        plot = new CurvePlot2D();
        ternaryGraph.AddChild(plot);
        plot.Owner = GetTree().EditedSceneRoot;
        plot.Width = 3;
    }
    public Animation AnimateTernaryPlotToDay(int dayIndex)
    {
        plot.SetData(Sim.GetStrategyFrequenciesByDay().Take(dayIndex + 1)
            .Select(population => TernaryGraph.CoordinatesToPosition(population.X, population.Y, population.Z)).ToArray());
        
        return plot.Transition(0.5f);
    }

    public BarPlot BarPlot;
    public Animation AnimateBarPlotToDay(int dayIndex)
    {
        var dataAsVector = Sim.GetStrategyFrequenciesByDay()[dayIndex];
        // We're doing percents, so multiply by 100
        BarPlot.SetData(dataAsVector.X * 100, dataAsVector.Y * 100, dataAsVector.Z * 100);        
        return BarPlot.Transition();
    }
}
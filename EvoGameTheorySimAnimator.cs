using System.Collections.Generic;
using System.Linq;
using Godot;
using Primer;
using PrimerTools;
using PrimerTools.Graph;

public partial class EvoGameTheorySimAnimator : Node3D
{
    public EvoGameTheorySim Sim;
    private MeshInstance3D _ground;
    private readonly List<Node3D> _trees = new();
    private readonly List<Node3D> _homes = new();
    private readonly Rng _simVisualizationRng = new Rng(2);
    
    private readonly Dictionary<EvoGameTheorySim.RPSGame.Strategy, Color> _strategyColors = new()
    {
        { EvoGameTheorySim.RPSGame.Strategy.Rock, PrimerColor.red },
        { EvoGameTheorySim.RPSGame.Strategy.Paper, PrimerColor.blue },
        { EvoGameTheorySim.RPSGame.Strategy.Scissors, PrimerColor.yellow }
    };
    
    public bool IncludeTernaryPlot = true;
    
    public void NonAnimatedSetup()
    {
        // Make the ground plane
        _ground = new MeshInstance3D();
        _ground.Name = "Ground";
        AddChild(_ground);
        _ground.Owner = GetTree().EditedSceneRoot;
        
        var planeMesh = new PlaneMesh();
        planeMesh.Size = new Vector2(10, 10);
        _ground.Mesh = planeMesh;
        // ground.Scale = new Vector3(1, 1, 1);
        _ground.Position = Vector3.Right * 6;
        var groundMaterial = new StandardMaterial3D();
        groundMaterial.AlbedoColor = new Color(0x1d6114ff);
        _ground.Mesh.SurfaceSetMaterial(0, groundMaterial);
        
        
        // Add trees according to the max number of trees
        var treeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Trees/Mango trees/Medium mango tree/Resources/mango tree medium.scn");
        
        for (var i = 0; i < Sim.NumTrees; i++)
        {
            var tree = treeScene.Instantiate<Node3D>();
            _ground.AddChild(tree);
            tree.Owner = GetTree().EditedSceneRoot;
            tree.Name = "Tree";
            tree.Scale = Vector3.One * 0.1f;
            tree.Position = new Vector3(_simVisualizationRng.RangeFloat(-5, 5), 0, _simVisualizationRng.RangeFloat(-5, 5));
            _trees.Add(tree);
        }
        
        // Add homes
        var homeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Rocks/rock_home_11.tscn");
        for (var i = 0; i < 6; i++)
        {
            var home = homeScene.Instantiate<Node3D>();
            _ground.AddChild(home);
            home.Owner = GetTree().EditedSceneRoot;
            home.Name = "Home";
            home.Scale = Vector3.One * 0.5f;
            home.Position = new Vector3(_simVisualizationRng.RangeFloat(-5, 5), 0, _simVisualizationRng.RangeFloat(-5, 5));
            _homes.Add(home);
        }
        
        if (IncludeTernaryPlot) SetUpTernaryPlot();
    }

    public Animation AnimateDays()
    {
        // Initial blobs spawn in random homes
        var blobScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Blob/Blobs/blob.tscn");
        var blobPool = new Pool<Blob>(blobScene);
        var blobs = new Dictionary<int, Blob>();
        var parentPositions = new Dictionary<int, Vector3>();
        var dailyAnimations = new List<Animation>();
        var dayCount = 0;
        
        foreach (var entitiesToday in Sim.EntitiesByDay)
        {
            // Make the blobs
            var appearanceAnimations = new List<Animation>();
            foreach (var entityId in entitiesToday)
            {
                var blob = blobPool.GetFromPool();
                blobs.Add(entityId, blob);
                if (blob.GetParent() == null) _ground.AddChild(blob);
                blob.MakeChildrenLocalRecursively(GetTree().EditedSceneRoot);
                blob.Owner = GetTree().EditedSceneRoot;
                blob.Name = "Blob";
                blob.Scale = Vector3.Zero;

                var parent = Sim.Registry.Parents[entityId];
                var pos = parent == -1 ? _homes[_simVisualizationRng.RangeInt(_homes.Count)].Position : parentPositions[parent];
                appearanceAnimations.Add(
                    AnimationUtilities.Parallel(
                        blob.MoveTo(pos, duration: 0.001f),
                        blob.ScaleTo(Vector3.One * 0.1f),
                        blob.AnimateColor(_strategyColors[Sim.Registry.Strategies[entityId]])
                    )
                );
            }
            
            if (IncludeTernaryPlot)
            {
                appearanceAnimations.Add(AnimateTernaryPlotToDay(dayCount));
            }
            dayCount++;
            
            // Move blobs to trees
            var numGames = entitiesToday.Count - Sim.NumTrees;
            numGames = Mathf.Max(numGames, 0);
            numGames = Mathf.Min(numGames, Sim.NumTrees);
            var toTreeAnimations = new List<Animation>();
            for (var i = 0; i < numGames; i++)
            {
                var blob1 = blobs[entitiesToday[i * 2]];
                var blob2 = blobs[entitiesToday[i * 2 + 1]];
                
                toTreeAnimations.Add(blob1.MoveTo(_trees[i].Position));
                toTreeAnimations.Add(blob2.MoveTo(_trees[i].Position));
            }
            for (var i = numGames * 2; i < entitiesToday.Count; i++)
            {
                var blob = blobs[entitiesToday[i]];
                toTreeAnimations.Add(numGames < Sim.NumTrees
                    ? blob.MoveTo(_trees[i - numGames].Position)
                    : blob.ScaleTo(Vector3.Zero));
            }
            
            // Move the blobs home
            var toHomeAnimations = new List<Animation>();
            foreach (var entityId in entitiesToday)
            {
                var blob = blobs[entityId];
                // toHomeAnimations.Add(blob.MoveTo(homes[simVisualizationRng.RangeInt(homes.Count)].Position));
                toHomeAnimations.Add(
                    AnimationUtilities.Series(
                        blob.MoveTo(_homes[_simVisualizationRng.RangeInt(_homes.Count)].Position),
                        blob.ScaleTo(Vector3.Zero)
                    )
                );
                parentPositions[entityId] = blob.Position;
                blobPool.ReturnToPool(blob, unparent: false, makeInvisible: false);
            }
            
            dailyAnimations.Add(AnimationUtilities.Series(
                    appearanceAnimations.RunInParallel(),
                    toTreeAnimations.RunInParallel(),
                    toHomeAnimations.RunInParallel()
                )
            );
        }
        if (IncludeTernaryPlot)
        {
            dailyAnimations.Add(AnimateTernaryPlotToDay(dayCount));
        }
        
        return AnimationUtilities.Series(dailyAnimations.ToArray());
    }

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
            _strategyColors[EvoGameTheorySim.RPSGame.Strategy.Rock],
            _strategyColors[EvoGameTheorySim.RPSGame.Strategy.Paper],
            _strategyColors[EvoGameTheorySim.RPSGame.Strategy.Scissors]
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
}
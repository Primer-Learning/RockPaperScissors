using System.Collections.Generic;
using System.Linq;
using Godot;
using PrimerTools;
using PrimerTools.AnimationSequence;
using PrimerTools.Graph;

[Tool]
public partial class PlottedSimTest : AnimationSequence
{
    protected override void Define()
    {
        // Run the simulation
        var sim = new EvoGameTheorySim();
        AddChild(sim);
        sim.NumDays = 10;
        sim.InitialBlobCount = 4;
        sim.NumTrees = 50;
        sim.RunSim();
        var results = sim.GetStrategyFrequenciesByDay();

        #region Animate the results

        // Make the ground plane
        var ground = new MeshInstance3D();
        AddChild(ground);
        ground.Owner = GetTree().EditedSceneRoot;
        
        var planeMesh = new PlaneMesh();
        planeMesh.Size = new Vector2(10, 10);
        ground.Mesh = planeMesh;
        // ground.Scale = new Vector3(1, 1, 1);
        ground.Position = Vector3.Right * 6;
        var groundMaterial = new StandardMaterial3D();
        groundMaterial.AlbedoColor = new Color(0x1d6114ff);
        ground.Mesh.SurfaceSetMaterial(0, groundMaterial);
        
        var simVisualizationRng = new Rng(2);
        // Add trees according to the max number of trees
        var treeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Trees/Mango trees/Medium mango tree/Resources/mango tree medium.scn");
        var trees = new List<Node3D>();
        for (var i = 0; i < sim.NumTrees; i++)
        {
            var tree = treeScene.Instantiate<Node3D>();
            ground.AddChild(tree);
            tree.Owner = GetTree().EditedSceneRoot;
            tree.Name = "Tree";
            tree.Scale = Vector3.One * 0.1f;
            tree.Position = new Vector3(simVisualizationRng.RangeFloat(-5, 5), 0, simVisualizationRng.RangeFloat(-5, 5));
            trees.Add(tree);
        }
        
        // Add homes
        var homeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Rocks/rock_home_11.tscn");
        var homes = new List<Node3D>();
        for (var i = 0; i < 6; i++)
        {
            var home = homeScene.Instantiate<Node3D>();
            ground.AddChild(home);
            home.Owner = GetTree().EditedSceneRoot;
            home.Name = "Home";
            home.Scale = Vector3.One * 0.5f;
            home.Position = new Vector3(simVisualizationRng.RangeFloat(-5, 5), 0, simVisualizationRng.RangeFloat(-5, 5));
            homes.Add(home);
        }
        
        // Spawn and move blobs according to the results
        
        // Initial blobs spawn in random homes
        var blobScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Blob/Blobs/blob.tscn");
        var blobs = new Dictionary<int, Blob>();
        // var blobAnimation = new Animation();
        var dailyAnimations = new List<Animation>();
        foreach (var entitiesToday in sim.EntitiesByDay)
        {
            // TODO: Idea: Make a single loop that processes a whole blob day.
            // In the old situation, we needed blobs to show up so the sim would know what to do next.
            // But now all the data is there already, so we could process a whole blob day in one go.
            // Animation lists for each stage would still need to exist.
            
            // Make the blobs
            var appearanceAnimations = new List<Animation>();
            foreach (var entityId in entitiesToday)
            {
                var blob = blobScene.Instantiate<Blob>();
                blobs.Add(entityId, blob);
                ground.AddChild(blob);
                blob.Owner = GetTree().EditedSceneRoot;
                blob.Name = "Blob";
                blob.Scale = Vector3.Zero;

                appearanceAnimations.Add(blob.ScaleTo(Vector3.One * 0.1f));
                blob.SetColor(sim.StrategyColors[sim.Registry.Strategies[entityId]]);
                
                var parent = sim.Registry.Parents[entityId];
                blob.Position = parent == -1 ? homes[simVisualizationRng.RangeInt(homes.Count)].Position : blobs[parent].Position;
            }
            
            // Move blobs to trees
            var numGames = entitiesToday.Count - sim.NumTrees;
            numGames = Mathf.Max(numGames, 0);
            numGames = Mathf.Min(numGames, sim.NumTrees);
            var toTreeAnimations = new List<Animation>();
            for (var i = 0; i < numGames; i++)
            {
                var blob1 = blobs[entitiesToday[i * 2]];
                var blob2 = blobs[entitiesToday[i * 2 + 1]];
                
                toTreeAnimations.Add(blob1.MoveTo(trees[i].Position));
                toTreeAnimations.Add(blob2.MoveTo(trees[i].Position));
            }
            for (var i = numGames * 2; i < entitiesToday.Count; i++)
            {
                var blob = blobs[entitiesToday[i]];
                toTreeAnimations.Add(numGames < sim.NumTrees
                    ? blob.MoveTo(trees[i - numGames].Position)
                    : blob.ScaleTo(Vector3.Zero));
            }
            
            // Move the blobs home
            var toHomeAnimations = new List<Animation>();
            foreach (var entityId in entitiesToday)
            {
                var blob = blobs[entityId];
                toHomeAnimations.Add(blob.MoveTo(homes[simVisualizationRng.RangeInt(homes.Count)].Position));
            }

            dailyAnimations.Add(AnimationUtilities.Series(
                    appearanceAnimations.RunInParallel(),
                    toTreeAnimations.RunInParallel(),
                    toHomeAnimations.RunInParallel()
                )
            );
        }

        RegisterAnimation(AnimationUtilities.Series(dailyAnimations.ToArray()));
        
        // Go to tree / eat, where only two blobs can be at a tree at a time
        // Go to random home, could be non-random at some point but who cares
        // Reproduce by spawning blobs from the next day in the same place as their parents

        #endregion

        #region Plot the results
        var ternaryGraph = new TernaryGraph();
        AddChild(ternaryGraph);
        ternaryGraph.Owner = GetTree().EditedSceneRoot;
        ternaryGraph.Scale = Vector3.One * 10;
        ternaryGraph.Position = Vector3.Left * 11;
        ternaryGraph.CreateBounds();
        
        var plot = new CurvePlot2D();
        ternaryGraph.AddChild(plot);
        plot.SetData(results.Select(point => TernaryGraph.CoordinatesToPosition(point.X, point.Y, point.Z)).ToArray());
        plot.Width = 3;
        
        plot.Owner = GetTree().EditedSceneRoot;
        RegisterAnimation(plot.Transition(10));
        #endregion
    }
}

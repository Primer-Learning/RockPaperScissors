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
        // sim.InitialBlobCount = 4;
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
        // var trees = new List<Node3D>();
        for (var i = 0; i < sim.NumTrees; i++)
        {
            var tree = treeScene.Instantiate<Node3D>();
            ground.AddChild(tree);
            tree.Owner = GetTree().EditedSceneRoot;
            tree.Scale = Vector3.One * 0.1f;
            tree.Position = new Vector3(simVisualizationRng.RangeFloat(-5, 5), 0, simVisualizationRng.RangeFloat(-5, 5));
            // trees.Add(tree);
        }
        
        // Add homes
        var homeScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Rocks/rock_home_11.tscn");
        var homes = new List<Node3D>();
        for (var i = 0; i < 6; i++)
        {
            var home = homeScene.Instantiate<Node3D>();
            ground.AddChild(home);
            home.Owner = GetTree().EditedSceneRoot;
            home.Scale = Vector3.One * 0.5f;
            home.Position = new Vector3(simVisualizationRng.RangeFloat(-5, 5), 0, simVisualizationRng.RangeFloat(-5, 5));
            homes.Add(home);
        }
        
        // Spawn and move blobs according to the results
        
        // Initial blobs spawn in random homes
        var blobScene = ResourceLoader.Load<PackedScene>("res://addons/PrimerAssets/Organized/Blob/Blobs/blob.tscn");
        foreach (var entitiesToday in sim.EntitiesByDay)
        {
            foreach (var entity in entitiesToday)
            {
                var blob = blobScene.Instantiate<Blob>();
                ground.AddChild(blob);
                blob.Owner = GetTree().EditedSceneRoot;
                blob.Scale = Vector3.One * 0.1f;
                
                // TODO: Color the blobs by their strategy
                
                blob.Position = homes[simVisualizationRng.RangeInt(homes.Count)].Position;
            }
            break;
        }
        
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

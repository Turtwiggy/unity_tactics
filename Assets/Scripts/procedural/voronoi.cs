using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using System.Collections.Generic;
using TriangleNet.Voronoi;
using TriangleNet;
using TriangleNet.Smoothing;
using UnityEngine;

namespace Wiggy
{
  public static class voronoi
  {
    public static VoronoiBase Generate(List<Vector2Int> cells, int width, int height, int smooth)
    {
      List<Vertex> points = new();

      foreach (var c in cells)
      {
        Vertex v = new();
        v.X = c.x;
        v.Y = c.y;
        points.Add(v);
      };

      // Choose: incremental, sweepline, or Dwyer
      var triangulator = new Dwyer();
      var config = new Configuration();

      TriangleNet.Mesh mesh = (TriangleNet.Mesh)triangulator.Triangulate(points, config);

      var smoother = new SimpleSmoother();
      smoother.Smooth(mesh, smooth);

      var r = new Rectangle(0, 0, width, height);
      var voronoi = new StandardVoronoi(mesh, r);

      return voronoi;
    }

    // The voronoi map is a representation where the lines are "walls"
    public static MapEntry[] GetVoronoiRepresentation(VoronoiBase graph, int width, int height, int size)
    {
      MapEntry[] voronoi_map = map_manager.CreateBlankMap(width * height);

      // Debug zone edges
      foreach (var e in graph.Edges)
      {
        var v0 = graph.Vertices[e.P0];
        var v1 = graph.Vertices[e.P1];
        var p0 = new Vector3((float)v0.X, 0, (float)v0.Y);
        var p1 = new Vector3((float)v1.X, 0, (float)v1.Y);
        var pos0 = Grid.WorldSpaceToGridSpace(p0, size, width);
        var pos1 = Grid.WorldSpaceToGridSpace(p1, size, width);

        var index0 = Grid.GetIndex(pos0, width);
        var index1 = Grid.GetIndex(pos1, width);
        var astar = map_manager.GameToAStar(voronoi_map, width, height);
        var obstacles = false;
        var l = a_star.generate_direct_with_diagonals(astar, index0, index1, width, obstacles);

        for (int i = 0; i < l.Length; i++)
        {
          var idx = Grid.GetIndex(l[i].pos, width);
          voronoi_map[idx].entities.Clear();
          voronoi_map[idx].entities.Add(EntityType.tile_type_wall);
        }
      }

      return voronoi_map;
    }

    // mapgen: 
    // poisson distribute a bunch of points
    // compute the voronoi on those points to turn in to zones
    public static List<Vector2Int> GeneratePoissonPoints(Vector2Int first_sample, int zone_size, int zone_seed, int width, int height, int cell_size)
    {
      List<Vector2Int> zones = new();
      poisson sampler = new poisson(width, height, zone_size);
      foreach (Vector2 sample in sampler.Samples(zone_seed, first_sample))
      {
        var pos = Grid.WorldSpaceToGridSpace(new Vector3(sample.x, 0, sample.y), cell_size, width);
        zones.Add(pos);
      }
      return zones;
    }

    public static List<List<int>> GetZones(List<Vector2Int> zone_center, MapEntry[] voronoi_map, int width, int height)
    {
      var astar_map = map_manager.GameToAStar(voronoi_map, width, height);

      List<List<int>> zones = new();

      for (int i = 0; i < zone_center.Count; i++)
      {
        List<int> tiles = new();

        Vector2Int center = zone_center[i];
        var index = Grid.GetIndex(center, width);
        var range = width * height;

        astar_cell[] area = a_star.generate_accessible_areas(astar_map, index, range, width, height);

        for (int j = 0; j < area.Length; j++)
          tiles.Add(Grid.GetIndex(area[j].pos, width));

        zones.Add(tiles);
      }

      return zones;
    }

  }
}
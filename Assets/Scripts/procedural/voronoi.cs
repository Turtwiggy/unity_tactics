using System.Data;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using System.Collections.Generic;
using TriangleNet.Meshing;
using TriangleNet.Voronoi;
using TriangleNet;
using TriangleNet.Smoothing;

namespace Wiggy
{
  public static class voronoi
  {
    public static VoronoiBase Generate(List<cell> cells, int width, int height, int smooth)
    {
      List<Vertex> points = new();

      foreach (var c in cells)
      {
        Vertex v = new Vertex();
        v.X = c.pos.x;
        v.Y = c.pos.y;
        points.Add(v);
      };

      // Choose: incremental, sweepline, or Dwyer
      var triangulator = new Dwyer();
      var config = new Configuration();

      Mesh mesh = (Mesh)triangulator.Triangulate(points, config);

      var smoother = new SimpleSmoother();
      smoother.Smooth(mesh, smooth);

      var r = new Rectangle(0, 0, width, height);
      var voronoi = new StandardVoronoi(mesh, r);

      return voronoi;
    }
  }
}
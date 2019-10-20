using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace ArcRhino_Module
{
   internal static class RhinoUtil
   {
      /// <summary>
      /// Write rhino object to ArcGIS feature layer
      /// </summary>
      /// <param name="mapLayer"></param>
      /// <param name="ro"></param>
      public static async void ThrowItOverTheFence(BasicFeatureLayer mapLayer, RhinoObject ro)
      {
         Mesh mesh = null;

         var createOperation = new EditOperation();

         var projection = mapLayer.GetSpatialReference();

         switch (ro.Geometry.ObjectType)
         {
            case ObjectType.Point:
               {
                  Point pt = ro.Geometry as Point;
                  MapPoint mp = ptToGis(pt.Location);
                  createOperation.Create(mapLayer, mp);
                  createOperation.ExecuteAsync();
                  break;
               }
            case ObjectType.Surface:
               {
                  Surface srf = ro.Geometry as Surface;
                  if (!srf.IsPlanar())
                  {
                     Console.Out.WriteLine($"Unable to send non-planar surfaces: Guid: ${ro.Id}");
                     break;
                  }
                  goto case ObjectType.Brep;
               }
            case ObjectType.Curve:
               {
                  Curve c = ro.Geometry as Curve;
                  var ptList = getPointsFromCurves(new List<Curve>(){ c });
                  var gisPts = ptList.Select(p => ptToGis(p)).ToList();
                  var polyline = PolylineBuilder.CreatePolyline(gisPts, projection);
                  createOperation.Create(mapLayer, polyline);
                  createOperation.ExecuteAsync();
                  break;
               }
            case ObjectType.Brep:
               {
                  Brep brep = ro.Geometry as Brep;
                  if (brep.IsSolid)
                  {
                     mesh = Mesh.CreateFromBrep(brep, MeshingParameters.Default)[0];
                     goto case ObjectType.Mesh;
                  }
                  else
                  {
                     var crvs = new List<Curve>();
                     foreach (BrepEdge ed in brep.Edges)
                     {
                        crvs.Add(ed.EdgeCurve);
                     }
                     var pts = getPointsFromCurves(crvs);
                     var gisPts = pts.Select(p => ptToGis(p)).ToList();
                     var polygon = new PolygonBuilder(gisPts).ToGeometry();
                     createOperation.Create(mapLayer, polygon);
                     createOperation.ExecuteAsync();
                     break;
                  }
               }
            case ObjectType.Extrusion:
               {
                  mesh = (ro.Geometry as Extrusion).GetMesh(MeshType.Default);
                  goto case ObjectType.Mesh;
               }
            case ObjectType.Mesh:
               {
                  mesh = (mesh == null) ? ro.Geometry as Mesh : mesh;
                  break;
               }
            default:
               {
                  Console.Out.WriteLine($"Unable to send geometry type: ${ro.Geometry.ObjectType}");
                  break;
               }
         }
      }
      
      /// <summary>
      /// Get the points on a list of curves
      /// </summary>
      /// <param name="crvs"></param>
      /// <returns></returns>
      internal static List<Point3d> getPointsFromCurves(IEnumerable<Curve> crvs)
      {
         var ptList = new List<Point3d>();
         foreach(Curve c in crvs)
         {
            PolylineCurve pl = null;
            if (c.HasNurbsForm() > 0)
            {
               pl = c.ToNurbsCurve().ToPolyline(1, Math.PI / 20, 1, double.MaxValue);
            }
            else
            {
               pl = c.ToPolyline(1, Math.PI / 20, 1, double.MaxValue);
            }
            for (int i = 0; i < pl.PointCount; i++)
            {
               ptList.Add(pl.Point(i));
            }
         }
         return ptList;
      }

      /// <summary>
      /// Convert Rhino Point3d to ArcGIS MapPoint (using tranformation)
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      internal static MapPoint ptToGis(Point3d pt)
      {
         // TODO: get transformation from cached doc properties or prompt user to set lat-lon first
         return MapPointBuilder.CreateMapPoint(pt.X + 1357671, pt.Y + 418736);
      }
   }
}

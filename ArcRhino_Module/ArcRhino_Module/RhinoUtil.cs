using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Runtime.InProcess;

namespace ArcRhino_Module
{
   internal static class RhinoUtil
   {



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

      internal static MapPoint ptToGis(Rhino.Geometry.Point3d pt)
      {
         return MapPointBuilder.CreateMapPoint(pt.X + 1357671, pt.Y + 418736);
      }
      






   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;

namespace ArcRhino_Module
{
   /// <summary>
   /// Static methods for pushing Rhino Object geometry to ArcGIS
   /// </summary>
   internal static class RhinoUtil
   {
      /// <summary>
      /// Write rhino object to ArcGIS feature layer
      /// </summary>
      /// <param name="mapLayer"></param>
      /// <param name="ro"></param>
      public static async void ThrowItOverTheFence(BasicFeatureLayer mapLayer, RhinoObject ro, RhinoDoc rhinoDoc)
      {
         Mesh mesh = null;

         var createOperation = new EditOperation();

         var projection = mapLayer.GetSpatialReference();
         var origin = getOrigin(rhinoDoc);

         switch (ro.Geometry.ObjectType)
         {
            case ObjectType.Point:
               {
                  Point pt = ro.Geometry as Point;
                  MapPoint mp = ptToGis(pt.Location, origin);
                  createOperation.Create(mapLayer, mp);
                  await createOperation.ExecuteAsync();
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
                  var gisPts = ptList.Select(p => ptToGis(p, origin)).ToList();
                  var polyline = PolylineBuilder.CreatePolyline(gisPts, projection);
                  createOperation.Create(mapLayer, polyline);
                  await createOperation.ExecuteAsync();
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
                     var gisPts = pts.Select(p => ptToGis(p, origin)).ToList();
                     var polygon = new PolygonBuilder(gisPts).ToGeometry();
                     createOperation.Create(mapLayer, polygon);
                     await createOperation.ExecuteAsync();
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
                  mesh = mesh ?? ro.Geometry as Mesh;
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
      /// Get the center of the map view
      /// </summary>
      /// <returns></returns>
      internal static Point3d getCenter()
      {
         var pt = new Point3d(0, 0, 0);
         QueuedTask.Run(() =>
         {
            var fl = MapView.Active.Map.Layers.OfType<FeatureLayer>().FirstOrDefault();
            if (fl != null)
            {
               var c = MapView.Active.Camera;
               pt = new Point3d(c.X, c.Y, 0);
            }
         }).Wait();
         return pt;
      }

      /// <summary>
      /// Get the origin of the Rhio doc in Map coordinates (default to center of map view)
      /// </summary>
      /// <param name="rhinoDoc"></param>
      /// <returns></returns>
      internal static Point3d getOrigin(RhinoDoc rhinoDoc)
      {
         try
         {
            var xyStr = rhinoDoc.Strings.GetValue("ArcRhinoXY");
            if (xyStr == null || xyStr.Length == 0 || xyStr.Split(',').Length != 3)
            {
               var xy = getCenter();
               // TODO: set z-0 if non-zero
               xyStr = $"{xy.X},{xy.Y},{0}";
               rhinoDoc.Strings.SetString("ArcRhinoXY", xyStr);
               return xy;
            }
            else
            {
               var vals = xyStr.Split(',').Select(i => Convert.ToDouble(i)).ToList();
               // TODO: set z-0 if non-zero
               return new Point3d(vals[0], vals[1], 0);
            }
         }
         catch (Exception ex)
         {
            System.Windows.MessageBox.Show(ex.Message + ex.StackTrace);
            return new Point3d(0, 0, 0);
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
      internal static MapPoint ptToGis(Point3d pt, Point3d origin)
      {
         // TODO: get transformation from cached doc properties or prompt user to set lat-lon first
         return MapPointBuilder.CreateMapPoint(pt.X + origin.X, pt.Y + origin.Y);
      }
   }
}

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

         switch (ro.Geometry.ObjectType)
         {
            case ObjectType.Point:
               {
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

                  var createOperation = new EditOperation();

                  // meant to work only on layer which has 3857 projection system
                  // with feature class support for polygons

                  // Create a spatial reference using the WKID (well-known ID) 
                  // for the Web Mercator coordinate system.
                  var mercatorSR = SpatialReferenceBuilder.CreateSpatialReference(3857);

                  // Create a list of all map points describing the polygon vertices.
                  var points = new List<MapPoint>();

                  foreach (BrepVertex vt in srf.ToBrep().Vertices)
                  {
                     MapPoint mp = MapPointBuilder.CreateMapPoint(vt.Location.X, vt.Location.Z, mercatorSR);
                     points.Add(mp);
                  }

                  // use the builder to create the polygon container
                  var polygon = new PolygonBuilder(points).ToGeometry();

                  var layer = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault();
                  createOperation.Create(layer, polygon);

                  createOperation.ExecuteAsync();
                  break;
               }
            case ObjectType.Curve:
               {

                  var createOperation = new EditOperation();

                  var projection = mapLayer.GetSpatialReference();
                  if (ro.Geometry is Rhino.Geometry.PolylineCurve polyline)
                  {
                     var ptList = getPointsFromPolylineCurve(polyline);
                     var gisPts = ptList.Select(p => ptToGis(p)).ToList();
                     var polygon = PolygonBuilder.CreatePolygon(gisPts, projection);
                     createOperation.Create(mapLayer, polygon);
                     createOperation.Execute();
                  }
                  break;
               }
            case ObjectType.Brep:
               {
                  if ((ro.Geometry as Brep).IsSurface) goto case ObjectType.Surface;
                  mesh = Mesh.CreateFromBrep(ro.Geometry as Brep, MeshingParameters.Default)[0];
                  goto case ObjectType.Mesh;
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

















      private static List<Rhino.Geometry.Point3d> getPointsFromPolylineCurve(Rhino.Geometry.PolylineCurve crv)
      {
         var ptList = new List<Rhino.Geometry.Point3d>();
         var ptCount = crv.PointCount;
         for (int i = 0; i < ptCount; i++)
         {
            var pt = crv.Point(i);
            ptList.Add(pt);
         }
         return ptList;
      }

      private static MapPoint ptToGis(Rhino.Geometry.Point3d pt)
      {
         return MapPointBuilder.CreateMapPoint(pt.X, pt.Y);
      }
      






   }
}

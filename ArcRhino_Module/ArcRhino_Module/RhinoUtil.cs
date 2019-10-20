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

      internal static void copySelectedObjects(RhinoDoc rhinoDoc)
      {
         foreach (RhinoObject ro in RhinoDoc.ActiveDoc.Objects)
         {
            ThrowItOverTheFence(ro);
         }

         MessageBox.Show("Done!");
      }







      internal static async void ThrowItOverTheFence(RhinoObject ro)
      {
         Mesh mesh = null;

         await QueuedTask.Run(() =>
         {
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

         });
      }


   }
}

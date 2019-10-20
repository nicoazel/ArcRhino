using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ArcRhino_Module
{
   internal static class GhUtil
   {
      /// <summary>
      /// Copy compatible GH canvas preview components into ArcGIS
      /// </summary>
      /// <param name="document"></param>
      internal static void showDocumentPreview()
      {
         try
         {
            // TODO: determine whether to purge existing GH_Preview geometry
            // on feature layers or append to that.

            var document = Grasshopper.Instances.DocumentServer.FirstOrDefault();
            if (document == null) return;
            var t = QueuedTask.Run(() =>
            {
               var previewObjects = document.Objects
                  .OfType<IGH_ActiveObject>()
                  .Where(o => !o.Locked && o is IGH_PreviewObject)
                  .Select(o => o as IGH_PreviewObject)
                  .Where(o => o.IsPreviewCapable)
                  .ToList();

               var componentParams = previewObjects
                  .Where(o => o is IGH_Component)
                  .Select(o => o as IGH_Component)
                  .Where(o => !o.Hidden)
                  .SelectMany(o => o.Params.Output)
                  .ToList();

               var otherParams = previewObjects
                  .Where(o => o is IGH_Param)
                  .Select(o => o as IGH_Param)
                  .ToList();

               var operation = new EditOperation();
               componentParams.ForEach(p => showParam(operation, p));
               otherParams.ForEach(p => showParam(operation, p));
            });
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message + ex.StackTrace);
         }
      }

      internal enum FeatureLayerType
      {
         GH_Preview_Polygon,
         GH_Preview_Polyline,
         GH_Preview_Point
         // TODO: add other types
      }

      /// <summary>
      /// Get the relevant GH Feature layer based on the type (Point, Line, Polygon)
      /// </summary>
      /// <param name="type">Feature layer type (point, line, polygon)</param>
      /// <returns>Feature Layer</returns>
      internal static BasicFeatureLayer getFeatureLayer(FeatureLayerType type)
      {
         var ghPreviewLayer = MapView.Active.Map.FindLayers(type.ToString("g")).FirstOrDefault();
         if (ghPreviewLayer != null) return ghPreviewLayer as BasicFeatureLayer;
         else return null;
      }

      /* TODO: complete this
       internal static FeatureLayer makeLayer(FeatureLayerType layerType)
       {
         var name layerType.ToString("g");
         ArcGIS.Desktop.Core.Geoprocessing.Geoprocessing ... ???
       }
      */

      /// <summary>
      /// Place param geometry content (if any exists) on appropriate feature layer
      /// </summary>
      /// <param name="operation">Edit operation</param>
      /// <param name="param">IGH Param</param>
      private static void showParam(EditOperation operation, IGH_Param param)
      {
         foreach (var value in param.VolatileData.AllData(true))
         {
            if (value is IGH_PreviewData)
            {
               switch (value.ScriptVariable())
               {
                  case Mesh mesh:
                     showMesh(operation, mesh);
                     break;
                  case Brep brep:
                     showBrep(operation, brep);
                     break;
                  case Curve curve:
                     showCurve(operation, curve);
                     break;
                  case Point3d point:
                     showPoint(operation, point);
                     break;
               }

            }
         }
      }

      /// <summary>
      /// Place mesh on feature layer
      /// </summary>
      /// <param name="operation"></param>
      /// <param name="mesh"></param>
      private static void showMesh(EditOperation operation, Mesh mesh)
      {
         var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polygon);
         if (layer == null) return;
         var projection = layer.GetSpatialReference();
         // TODO: complete this

         // operation.Create(layer, polyline);
         // operation.ExecuteAsync();
      }

      /// <summary>
      /// Place brep on feature layer
      /// </summary>
      /// <param name="operation"></param>
      /// <param name="brep"></param>
      private static void showBrep(EditOperation operation, Brep brep)
      {
         var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polygon);
         if (layer == null) return;
         var projection = layer.GetSpatialReference();
         // TODO: complete this

         // operation.Create(layer, polyline);
         // operation.ExecuteAsync();
      }

      /// <summary>
      /// Place curve on feature layer
      /// </summary>
      /// <param name="operation"></param>
      /// <param name="curve"></param>
      private static void showCurve(EditOperation operation, Curve curve)
      {
         try
         {
            // TODO: come up with way of determining whether to make a curve
            // into a polyline or polygon depending on context/user preferences
            // var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polyline);
            var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polygon);
            if (layer == null) return;
            var projection = layer.GetSpatialReference();
            var ptList = RhinoUtil.getPointsFromCurves(new List<Curve>() { curve });
            var gisPts = ptList.Select(p => RhinoUtil.ptToGis(p)).ToList();
            var polyline = new PolygonBuilder(gisPts).ToGeometry();
            // var polyline = PolylineBuilder.CreatePolyline(gisPts, projection);
            operation.Create(layer, polyline);
            operation.ExecuteAsync();
         }
         catch
         {

         }
      }


      /// <summary>
      /// Place point on feature layer
      /// </summary>
      /// <param name="operation">Edit operatio</param>
      /// <param name="point">Rhino Point3d</param>
      private static void showPoint(EditOperation operation, Point3d point)
      {
         try
         {
            var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Point);
            if (layer == null) return;
            MapPoint mp = RhinoUtil.ptToGis(point);
            operation.Create(layer, mp);
            operation.ExecuteAsync();
         }
         catch
         {

         }
      }
   }
}

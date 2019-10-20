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
      }
      
      internal static BasicFeatureLayer getFeatureLayer(FeatureLayerType type)
      {
         var ghPreviewLayer = MapView.Active.Map.FindLayers(type.ToString("g")).FirstOrDefault();
         if (ghPreviewLayer != null) return ghPreviewLayer as BasicFeatureLayer;
         else return null;
      }

      internal static void showParam(EditOperation operation, IGH_Param param)
      {
         foreach (var value in param.VolatileData.AllData(true))
         {
            if (value is IGH_PreviewData)
            {
               switch (value.ScriptVariable()) {
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

      internal static void showMesh(EditOperation operation, Mesh mesh)
      {
         var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polygon);
         if (layer == null) return;
         var projection = layer.GetSpatialReference();
         // TODO: complete this
         
         // operation.Create(layer, polyline);
         // operation.ExecuteAsync();
      }

      internal static void showBrep(EditOperation operation, Brep brep)
      {
         var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Polygon);
         if (layer == null) return;
         var projection = layer.GetSpatialReference();
         // TODO: complete this

         // operation.Create(layer, polyline);
         // operation.ExecuteAsync();
      }

      internal static void showCurve(EditOperation operation, Curve curve)
      {
         try
         {
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
         } catch
         {

         }
      }

      // internal static FeatureLayer makeLayer(string name)
      // {
      //    // ArcGIS.Desktop.Core.Geoprocessing.Geoprocessing.
      // }

      internal static void showPoint(EditOperation operation, Point3d point)
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

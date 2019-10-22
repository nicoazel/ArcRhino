using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace ArcRhino_Module
{
   internal static class GhUtil
   {
      /// <summary>
      /// Copy compatible GH canvas preview components into ArcGIS
      /// </summary>
      /// <param name="document"></param>
      internal static void showDocumentPreview(RhinoDoc rhinoDoc)
      {
         try
         {
            // TODO: determine whether to purge existing GH_Preview geometry
            // on feature layers or append to that.
            var document = Grasshopper.Instances.DocumentServer.FirstOrDefault();
            if (document == null) return;
            if (rhinoDoc == null) return;
            var origin = RhinoUtil.getOrigin(rhinoDoc);
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
            QueuedTask.Run(() =>
            {
               var operation = new EditOperation();
               componentParams.ForEach(p => showParam(operation, p, origin));
               otherParams.ForEach(p => showParam(operation, p, origin));
            });
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message + ex.StackTrace);
         }
      }

      /// <summary>
      /// Feature Layer Type (polygon, polyline, point, ...)
      /// </summary>
      private enum FeatureLayerType
      {
         GH_Preview_Polygon,
         GH_Preview_Polyline,
         GH_Preview_Point
         // TODO: add other types
      }

      internal static MapPoint getCenter()
      {
         return MapView.Active.Map.CalculateFullExtent().Center;
      }

      /// <summary>
      /// Get the relevant GH Feature layer based on the type (Point, Line, Polygon)
      /// </summary>
      /// <param name="type">Feature layer type (point, line, polygon)</param>
      /// <returns>Feature Layer</returns>
      private static BasicFeatureLayer getFeatureLayer(FeatureLayerType type)
      {
         var ghPreviewLayer = MapView.Active.Map.FindLayers(type.ToString("g")).FirstOrDefault();
         if (ghPreviewLayer != null) return ghPreviewLayer as BasicFeatureLayer;
         else
         {
            // TODO: uncomment once fully tested
            // ghPreviewLayer = makeLayer(type);
            if (ghPreviewLayer != null) return ghPreviewLayer as BasicFeatureLayer;
            else return null;
         }
      }


      /// <summary>
      /// Create feature layer of a selected type (if it doesn't already exist)
      /// TODO: test this funciton
      /// </summary>
      /// <param name="layerType"></param>
      /// <returns></returns>
      private static FeatureLayer makeLayer(FeatureLayerType layerType)
      {
         try
         {
            var name = layerType.ToString("g");
            var symRef = createSymbol(layerType);
            var rd = new SimpleRendererDefinition(symRef, label: name, description: name);
            // TODO: create new dataconnection for Rhino with attributes as needed
            var dc = MapView.Active.Map.Layers.FirstOrDefault().GetDataConnection();
            return LayerFactory.Instance.CreateFeatureLayer(dc, MapView.Active.Map, layerName: layerType.ToString("g"), rendererDefinition: rd);
         }
         catch
         {
            return null;
         }
      }

      private static ArcGIS.Core.CIM.CIMSymbolReference createSymbol(FeatureLayerType layerType)
      {
         var color = ArcGIS.Core.CIM.CIMColor.CreateRGBColor(0, 255, 255);
         switch (layerType)
         {
            case FeatureLayerType.GH_Preview_Point:
               return ArcGIS.Core.CIM.CIMSymbolReference.FromJson(SymbolFactory.Instance.ConstructPointSymbol(color).ToJson());
            case FeatureLayerType.GH_Preview_Polyline:
               return ArcGIS.Core.CIM.CIMSymbolReference.FromJson(SymbolFactory.Instance.ConstructLineSymbol(color).ToJson());
            case FeatureLayerType.GH_Preview_Polygon:
               return ArcGIS.Core.CIM.CIMSymbolReference.FromJson(SymbolFactory.Instance.ConstructPolygonSymbol(color).ToJson());
            default:
               return null;
         }
      }

      /// <summary>
      /// Place param geometry content (if any exists) on appropriate feature layer
      /// </summary>
      /// <param name="operation">Edit operation</param>
      /// <param name="param">IGH Param</param>
      private static void showParam(EditOperation operation, IGH_Param param, Point3d origin)
      {
         foreach (var value in param.VolatileData.AllData(true))
         {
            if (value is IGH_PreviewData)
            {
               switch (value.ScriptVariable())
               {
                  case Mesh mesh:
                     showMesh(operation, mesh, origin);
                     break;
                  case Brep brep:
                     showBrep(operation, brep, origin);
                     break;
                  case Curve curve:
                     showCurve(operation, curve, origin);
                     break;
                  case Point3d point:
                     showPoint(operation, point, origin);
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
      private static void showMesh(EditOperation operation, Mesh mesh, Point3d origin)
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
      private static void showBrep(EditOperation operation, Brep brep, Point3d origin)
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
      private static void showCurve(EditOperation operation, Curve curve, Point3d origin)
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
            var gisPts = ptList.Select(p => RhinoUtil.ptToGis(p, origin)).ToList();
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
      private static void showPoint(EditOperation operation, Point3d point, Point3d origin)
      {
         try
         {
            var layer = getFeatureLayer(FeatureLayerType.GH_Preview_Point);
            if (layer == null) return;
            MapPoint mp = RhinoUtil.ptToGis(point, origin);
            operation.Create(layer, mp);
            operation.ExecuteAsync();
         }
         catch
         {

         }
      }
   }
}

using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Rhino;
using System;

namespace ArcRhino_Module
{
   /// <summary>
   /// Static methods for handling GIS conversion to Rhino geometry 
   /// </summary>
   internal static class GisUtil
   {
      /// <summary>
      /// Copy selected GIS features into Rhino; assign objects to corresponding layers, and apply attribute values as user text
      /// </summary>
      /// <param name="rhinoDoc">Active Rhino Doc</param>
      internal static void copySelectedObjects(RhinoDoc rhinoDoc)
      {
         if (rhinoDoc == null) return;
         var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
         foreach (var layer in layers)
         {
            var t = QueuedTask.Run(() =>
            {
               var selectionfromMap = layer.GetSelection();
                  var count = selectionfromMap.GetCount();
               // MessageBox.Show($"Got layer {firstLayer.Name} with {count} selected features");
                  var filter = new QueryFilter { ObjectIDs = selectionfromMap.GetObjectIDs() };
               if (count > 0)
               {
                  

                  using (RowCursor rowCursor = layer.Search(filter))
                  {
                     while (rowCursor.MoveNext())
                     {
                        long oid = rowCursor.Current.GetObjectID();
                        // get the shape from the row
                        Feature feature = rowCursor.Current as Feature;
                        if (feature.GetShape() is Polygon polygon)
                        {
                           convertPolygon(layer, feature, polygon, rhinoDoc);
                        }
                        else if (feature.GetShape() is Polyline polyline)
                        {
                           convertPolyline(layer, feature, polyline, rhinoDoc);
                        }
                        else if (feature.GetShape() is MapPoint point)
                        {
                           convertPoint(layer, feature, point, rhinoDoc);
                        }
                        else if (feature.GetShape() is Multipoint multiPoint)
                        {
                           // TODO: treat multipoint as a group of points
                        }
                        else if (feature.GetShape() is Multipatch multiPatch)
                        {
                           // TODO: treat multipoint as a group of patches
                        }
                        else
                        {
                           // TODO: figure out other possible types inherited from ArcGIS.Core.Geometry
                        }
                        // MessageBox.Show("Found feature with attributes:\n" + string.Join("\n", feature.GetFields().Select(f => f.Name).ToList()));
                     }
                  }
               }
            });
         }
      }

      private static System.Drawing.Color getColor(FeatureLayer layer)
      {
         try
         {
            var r = layer.GetRenderer();
            var n = Newtonsoft.Json.Linq.JObject.Parse(r.ToJson());
            var x = (Newtonsoft.Json.Linq.JArray)n["symbol"]["symbol"]["symbolLayers"][0]["color"]["values"];
            var c = x.Select(i => (int)i).ToList();
            return System.Drawing.Color.FromArgb(c[0], c[1], c[2], 1);
         }
         catch
         {
            return System.Drawing.Color.Black;
         }
      }

      /// <summary>
      /// Convert ArcGIS MapPoint to Rhino Point3d
      /// </summary>
      /// <param name="layer">FeatureLayer</param>
      /// <param name="feature">Feature</param>
      /// <param name="point">MapPoint</param>
      /// <param name="rhinoDoc">RhinoDoc</param>
      private static void convertPoint(FeatureLayer layer, Feature feature, MapPoint point, RhinoDoc rhinoDoc)
      {
         var rhinoPoint = convertToRhinoPoint(point);
         var attrs = getLayerAttrs(layer, rhinoDoc);
         var guid = rhinoDoc.Objects.AddPoint(rhinoPoint, attrs);
         var obj = rhinoDoc.Objects.FindId(guid);
         bindAttrs(obj, feature);
      }

      /// <summary>
      /// Convert ArcGIS Polygon to Rhino Polyline
      /// </summary>
      /// <param name="layer">FeatureLayer</param>
      /// <param name="feature">Feature</param>
      /// <param name="polygon">Polygon</param>
      /// <param name="rhinoDoc">RhinoDoc</param>
      private static void convertPolygon(FeatureLayer layer, Feature feature, Polygon polygon, RhinoDoc rhinoDoc)
      {
         var rhinoPoints = polygon.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList();
         var attrs = getLayerAttrs(layer, rhinoDoc);
         var guid = rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
         var obj = rhinoDoc.Objects.FindId(guid);
         bindAttrs(obj, feature);
      }

      /// <summary>
      /// Convert ArcGIS Polyline to Rhino Polyline
      /// </summary>
      /// <param name="layer">FeatureLayer</param>
      /// <param name="feature">Feature</param>
      /// <param name="polygon">Polygon</param>
      /// <param name="rhinoDoc">RhinoDoc</param>
      private static void convertPolyline(FeatureLayer layer, Feature feature, Polyline polyline, RhinoDoc rhinoDoc)
      {
         var rhinoPoints = polyline.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList(); ;
         var attrs = getLayerAttrs(layer, rhinoDoc);
         var guid = rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
         var obj = rhinoDoc.Objects.FindId(guid);
         bindAttrs(obj, feature);
      }

      /// <summary>
      /// Get target Rhino layer (or create it if it doesn't already exist) and assign it to attributes
      /// </summary>
      /// <param name="layer">FeatureLayer</param>
      /// <param name="rhinoDoc">RhinoDoc</param>
      /// <returns></returns>
      private static Rhino.DocObjects.ObjectAttributes getLayerAttrs(FeatureLayer layer, RhinoDoc rhinoDoc)
      {
         if (!rhinoDoc.Layers.Any(l => l.Name == layer.Name))
         {
            var color = getColor(layer);

            // TODO: set layer color based on some logic
            rhinoDoc.Layers.Add(layer.Name, color);
         }
         var layerIndex = rhinoDoc.Layers.FindName(layer.Name).Index;
         var attrs = new Rhino.DocObjects.ObjectAttributes() { LayerIndex = layerIndex };
         return attrs;
      }

      /// <summary>
      /// Apply feature attributes (key-value pairs) as Rhino user text
      /// </summary>
      /// <param name="obj">RhinoObject</param>
      /// <param name="feature">Feature</param>
      private static void bindAttrs(Rhino.DocObjects.RhinoObject obj, Feature feature)
      {
         var fields = feature.GetFields();
         for (int i = 0; i < fields.Count; i++)
         {
            try
            {
               var name = fields[i].Name;
               var val = feature.GetOriginalValue(i);
               obj.Attributes.SetUserString(name, val.ToString());
            }
            catch { }
         }
      }

      /// <summary>
      /// Convert ArcGIS MapPoint to Rhino Point3d
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      internal static Rhino.Geometry.Point3d convertToRhinoPoint(MapPoint p) => 
         new Rhino.Geometry.Point3d(p.X - 1357671, p.Y - 418736, p.Z);
   }
}
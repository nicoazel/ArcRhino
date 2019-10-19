using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System.Windows.Forms;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Rhino;

namespace ArcRhino_Module
{
   static class GisUtil
   {
      internal static void getFirstLayer(RhinoDoc rhinoDoc)
      {
         var firstLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
         var count = 0;
         var t = QueuedTask.Run(() =>
         {
            var selectionfromMap = firstLayer.GetSelection();
            count = selectionfromMap.GetCount();
            MessageBox.Show($"Got layer {firstLayer.Name} with {count} selected features");

            if (count > 0)
            {
               var filter = new QueryFilter { ObjectIDs = selectionfromMap.GetObjectIDs() };
               using (RowCursor rowCursor = firstLayer.Search(filter))
               {
                  while (rowCursor.MoveNext())
                  {
                     long oid = rowCursor.Current.GetObjectID();
                     // get the shape from the row
                     Feature feature = rowCursor.Current as Feature;
                     if (feature.GetShape() is Polygon polygon)
                     {
                        convertPolygon(firstLayer, polygon, rhinoDoc);
                     }
                     if (feature.GetShape() is Polyline polyline)
                     {
                        convertPolyline(firstLayer, polyline, rhinoDoc);
                     }
                     if (feature.GetShape() is MapPoint point)
                     {
                        convertPoint(firstLayer, point, rhinoDoc);
                     }
                     if (feature.GetShape() is Multipoint multiPoint)
                     {
                        MessageBox.Show("FOUND A MULTIPOINT");
                     }
                     MessageBox.Show("Found feature with attributes:\n" + string.Join("\n", feature.GetFields().Select(f => f.Name).ToList()));
                  }
               }
            }
         });
      }

      private static void convertPoint(FeatureLayer featureLayer, MapPoint point, RhinoDoc rhinoDoc)
      {
         if (rhinoDoc != null)
         {
            var rhinoPoint = convertToRhinoPoint(point);
            if (!rhinoDoc.Layers.Any(l => l.Name == featureLayer.Name))
            {
               rhinoDoc.Layers.Add(featureLayer.Name, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            var layerIndex = rhinoDoc.Layers.FindName(featureLayer.Name).Index;
            var attrs = new Rhino.DocObjects.ObjectAttributes()
            {
               LayerIndex = layerIndex
            };
            rhinoDoc.Objects.AddPoint(rhinoPoint, attrs);
         }
      }

      private static void convertPolygon(FeatureLayer featureLayer, Polygon polygon, RhinoDoc rhinoDoc)
      {
         if (rhinoDoc != null)
         {
            var rhinoPoints = polygon.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList(); ;
            if (!rhinoDoc.Layers.Any(l => l.Name == featureLayer.Name))
            {
               rhinoDoc.Layers.Add(featureLayer.Name, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            var layerIndex = rhinoDoc.Layers.FindName(featureLayer.Name).Index;
            var attrs = new Rhino.DocObjects.ObjectAttributes()
            {
               LayerIndex = layerIndex
            };
            rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
         }
      }

      private static void convertPolyline(FeatureLayer featureLayer, Polyline polyline, RhinoDoc rhinoDoc)
      {
         if (rhinoDoc != null)
         {
            var rhinoPoints = polyline.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList(); ;
            if (!rhinoDoc.Layers.Any(l => l.Name == featureLayer.Name))
            {
               rhinoDoc.Layers.Add(featureLayer.Name, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            var layerIndex = rhinoDoc.Layers.FindName(featureLayer.Name).Index;
            var attrs = new Rhino.DocObjects.ObjectAttributes()
            {
               LayerIndex = layerIndex
            };
            rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
         }
      }

      internal static Rhino.Geometry.Point3d convertToRhinoPoint(MapPoint p)
      {
         return new Rhino.Geometry.Point3d(p.X - 1357671, p.Y - 418736, p.Z);
      }
   }
}



/*
var selectionfromMap = firstLayer.GetSelection();

ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter
{
   ObjectIDs = selectionfromMap.GetObjectIDs();
        };

        // get the row
        using (ArcGIS.Core.Data.RowCursor rowCursor = featureClass.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            long oid = rowCursor.Current.GetObjectID();

// get the shape from the row
ArcGIS.Core.Data.Feature feature = rowCursor.Current as ArcGIS.Core.Data.Feature;
Polygon polygon = feature.GetShape() as Polygon;

// get the attribute from the row (assume it's a double field)
double value = (double)rowCursor.Current.GetOriginalValue(fldIndex);

            // do something here
          }
        }
        */
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

namespace ArcRhino_Module
{
   static class GisUtil
   {
      internal static void getFirstLayer()
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
                        MessageBox.Show("FOUND A POLYGON");
                     }
                     if (feature.GetShape() is Polyline polyline)
                     {
                        var rhinoPoints = polyline.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList(); ;
                        MessageBox.Show("FOUND A POLYLINE with points:\n", string.Join("\n", rhinoPoints.Select(p => $"{p.X}, {p.Y}, {p.Z}").ToList()));
                       
                     }
                     if (feature.GetShape() is MapPoint point)
                     {
                        MessageBox.Show("FOUND A POINT");
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

      internal static Rhino.Geometry.Point3d convertToRhinoPoint(MapPoint p)
      {
         return new Rhino.Geometry.Point3d(p.X, p.Y, p.Z);
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
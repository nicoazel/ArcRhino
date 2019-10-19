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
         MessageBox.Show($"Got layer {firstLayer.Name}");
         var count = 0;
         var t = QueuedTask.Run(() =>
         {
            var selectionfromMap = firstLayer.GetSelection();
            count = selectionfromMap.GetCount();
            MessageBox.Show($"with {count} selected features");

         });
         //t.RunSynchronously();


      }

      internal static void getFirstLayerWorker()
      {
         

        //var selectionfromMap = firstLayer.GetSelection();
        //var count = selectionfromMap.GetCount();
        //MessageBox.Show($"with {count} selected features");
        //if (count > 0)
        //{
        //   var filter = new QueryFilter
        //   {
        //      ObjectIDs = selectionfromMap.GetObjectIDs()
        //   };
        //   using (RowCursor rowCursor = firstLayer.Search(filter))
        //
        //   {
        //      while (rowCursor.MoveNext())
        //      {
        //         long oid = rowCursor.Current.GetObjectID();
        //
        //         // get the shape from the row
        //
        //         Feature feature = rowCursor.Current as Feature;
        //
        //         Polygon polygon = feature.GetShape() as Polygon;
        //
        //         MessageBox.Show("Found feature with attributes:\n" + string.Join("\n", feature.GetFields().Select(f /=> /f.Name).ToList()));
        //      }
        //   }
        //}
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
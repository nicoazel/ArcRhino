using System.Linq;
using System.Collections.Generic;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Rhino;
using Rhino.Runtime.InProcess;
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
using ArcGIS.Desktop.Mapping;
namespace ArcRhino_Module
{
    static class GisUtil
    {
        internal static void copySelectedObjects(RhinoDoc rhinoDoc)
        {
            if (rhinoDoc == null) return;
            var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
            foreach (var firstLayer in layers)
            {
                var t = QueuedTask.Run(() =>
                {
                    var selectionfromMap = firstLayer.GetSelection();
                    var count = selectionfromMap.GetCount();
                // MessageBox.Show($"Got layer {firstLayer.Name} with {count} selected features");
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
                                    convertPolygon(firstLayer, feature, polygon, rhinoDoc);
                                }
                                else if (feature.GetShape() is Polyline polyline)
                                {
                                    convertPolyline(firstLayer, feature, polyline, rhinoDoc);
                                }
                                else if (feature.GetShape() is MapPoint point)
                                {
                                    convertPoint(firstLayer, feature, point, rhinoDoc);
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

        private static void convertPoint(FeatureLayer featureLayer, Feature feature, MapPoint point, RhinoDoc rhinoDoc)
        {
            var rhinoPoint = convertToRhinoPoint(point);
            var attrs = getLayerAttrs(featureLayer, rhinoDoc);
            var guid = rhinoDoc.Objects.AddPoint(rhinoPoint, attrs);
            var obj = rhinoDoc.Objects.FindId(guid);
            bindAttrs(obj, feature);
        }

        private static void convertPolygon(FeatureLayer featureLayer, Feature feature, Polygon polygon, RhinoDoc rhinoDoc)
        {
            var rhinoPoints = polygon.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList();
            var attrs = getLayerAttrs(featureLayer, rhinoDoc);
            var guid = rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
            var obj = rhinoDoc.Objects.FindId(guid);
            bindAttrs(obj, feature);
        }

        private static void convertPolyline(FeatureLayer featureLayer, Feature feature, Polyline polyline, RhinoDoc rhinoDoc)
        {
            var rhinoPoints = polyline.Points.ToList().Select(p => convertToRhinoPoint(p)).ToList(); ;
            var attrs = getLayerAttrs(featureLayer, rhinoDoc);
            var guid = rhinoDoc.Objects.AddPolyline(rhinoPoints, attrs);
            var obj = rhinoDoc.Objects.FindId(guid);
            bindAttrs(obj, feature);
        }

        private static Rhino.DocObjects.ObjectAttributes getLayerAttrs(FeatureLayer featureLayer, RhinoDoc rhinoDoc)
        {
            if (!rhinoDoc.Layers.Any(l => l.Name == featureLayer.Name))
            {
                // TODO: set layer color based on some logic
                rhinoDoc.Layers.Add(featureLayer.Name, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            var layerIndex = rhinoDoc.Layers.FindName(featureLayer.Name).Index;
            var attrs = new Rhino.DocObjects.ObjectAttributes() { LayerIndex = layerIndex };
            return attrs;
        }

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

        internal static Rhino.Geometry.Point3d convertToRhinoPoint(MapPoint p)
        {
            return new Rhino.Geometry.Point3d(p.X - 1357671, p.Y - 418736, p.Z);
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

        internal static void pushLayerToMap(RhinoDoc rhinoDoc)
        {
            var t = QueuedTask.Run(() =>
            {
                if (rhinoDoc != null)
                {
                    Rhino.DocObjects.ObjRef[] obref;
                    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetMultipleObjects("Select object", true, Rhino.DocObjects.ObjectType.AnyObject, out obref);

                    foreach (var obj in obref)
                    {

                        Rhino.DocObjects.RhinoObject rhobj = obj.Object();
                        int layerIndex = rhobj.Attributes.LayerIndex;
                        string layerName = rhinoDoc.Layers[layerIndex].Name;
                        MessageBox.Show($"Got layer {layerName} with selected features");
                        var thisLayer = ArcGIS.Desktop.Mapping.MapView.Active.Map.FindLayers(layerName).FirstOrDefault() as BasicFeatureLayer;
                        var projection = thisLayer.GetSpatialReference();

                        load(thisLayer, rhobj);

                    }//end for each
                }// end push layer to map
            });
            async void load(BasicFeatureLayer mapLayer, Rhino.DocObjects.RhinoObject obj)
            {

                switch (obj.Geometry.ObjectType)
                {

                    case Rhino.DocObjects.ObjectType.Curve:
                        {


                            var createOperation = new EditOperation()
                            {
                                Name = "Generate mesh"
                            };

                            var projection = mapLayer.GetSpatialReference();
                            if (obj.Geometry is Rhino.Geometry.PolylineCurve polyline)
                            {
                                var ptList = getPointsFromPolylineCurve(polyline);
                                var gisPts = ptList.Select(p => ptToGis(p)).ToList();
                                var polygon = PolygonBuilder.CreatePolygon(gisPts, projection);
                                createOperation.Create(mapLayer, polygon);
                                createOperation.Execute();
                            }
                            // Rhino.Geometry.NurbsCurve thisNurbsCurve = (obj.Geometry as Rhino.Geometry.NurbsCurve).ToNurbsCurve();
                            // Rhino.Geometry.Collections.NurbsCurvePointList theseControlPoints = thisNurbsCurve.Points;
                           // var vertices = new List<MapPoint>();
                           //
                           // foreach (Rhino.Geometry.ControlPoint thisPoint in theseControlPoints)
                           // {
                           //     vertices.Add(MapPointBuilder.CreateMapPoint(thisPoint.Location.X, thisPoint.Location.Y));
                           // }
                           //
                           // var polygon = PolygonBuilder.CreatePolygon(vertices, projection);
                           // createOperation.Create(mapLayer, polygon);
                           // createOperation.Execute();
                            break;
                        }
                    default: break;
                }
            }//end load

        }//end push layer to map
    }

}

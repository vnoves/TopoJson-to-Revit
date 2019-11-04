using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using View = Autodesk.Revit.DB.View;


namespace RoomsToTopoJson
{
    /// <summary>
    /// Interaction logic for MainForm.xaml
    /// </summary>
    public partial class MainForm : Window
    {


        private ExternalCommandData p_commanddata;

        public Document doc;

        public UIApplication uiApp;

        public MainForm(ExternalCommandData cmddata_p)
        {
            //Define Uiapp and current document
           
            InitializeComponent();
        
                p_commanddata = cmddata_p;  
                uiApp = cmddata_p.Application;
                UIDocument uiDoc = this.uiApp.ActiveUIDocument;
                doc = uiDoc.Document;
            }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            View activeView = doc.ActiveView;

            List<Autodesk.Revit.DB.SpatialElement> rooms = new FilteredElementCollector(doc, activeView.Id).OfClass(typeof(Autodesk.Revit.DB.SpatialElement)).Cast<Autodesk.Revit.DB.SpatialElement>().ToList();

            List<float[][]> FnlPoints = new List<float[][]>();
            List<string> roomNames = new List<string>();
            
            foreach (Autodesk.Revit.DB.Architecture.Room R in rooms)
            {
                
                IList<IList<Autodesk.Revit.DB.BoundarySegment>> segments = R.GetBoundarySegments(new SpatialElementBoundaryOptions());
                if (null != segments)  //the room may not be bound
                {
                    roomNames.Add(R.Name);
                    foreach (IList<Autodesk.Revit.DB.BoundarySegment> segmentList in segments)
                    {
                        //List to storage all points

                        List<XYZ> roomPoints = new List<XYZ>();
                        List<string> testtt = new List<string>();
                        foreach (Autodesk.Revit.DB.BoundarySegment boundarySegment in segmentList)
                        {
                            // Get curve start point
                            XYZ start = boundarySegment.GetCurve().GetEndPoint(0);
                            roomPoints.Add(start);
                            // Get curve end point
                            XYZ end = boundarySegment.GetCurve().GetEndPoint(1);
                            roomPoints.Add(end);
                        }

                        FnlPoints.Add(LocationPoints(roomPoints));

                    }
                }
            }


            Rootobject RacksList = new Rootobject();

            List<Feature> featuresArray = new List<Feature> { };

            //Define info of each room
            for (int i = 0; i < roomNames.Count; i++)
            { 
                //Define property
                Properties tempProperty = new Properties();

                //Define Coordinates
                float[][][] tempCoordinates = new float[1][][];
                float[][] tempFloat = new float[FnlPoints[i].Length][];
                tempFloat = FnlPoints[i];

                tempCoordinates[0] = tempFloat;

                //Define geometry
                Geometry temGeom = new Geometry();
                temGeom.type = "Polygon";
                temGeom.coordinates = tempCoordinates;


                //Define temGeom as current feature geomtry
                Feature tempFeature = new Feature();
                tempFeature.type = "Feature";
                tempFeature.properties = tempProperty;
                tempFeature.geometry = temGeom;

                //Add feature to array
                featuresArray.Add(tempFeature);

            }

            Feature[] currentFeatures = new Feature[roomNames.Count];
            currentFeatures = featuresArray.ToArray();


            RacksList = new Rootobject
            {
                type = "FeatureCollection",
                features = currentFeatures,
            };

            string path = @"C:\Users\V. Noves\Downloads\Test.json";
            export.serializeJason(RacksList, path);
        }

      


        public float[][] LocationPoints(List<XYZ> SegmentPoints)
        {
            List<float[]> resultList = new List<float[]>();

            //Get rid from every other number
            int pos = 0;
            for (int i = 0; i < SegmentPoints.Count; i += 2, pos++)
            {
                SegmentPoints[pos] = SegmentPoints[i];
            }
            SegmentPoints.RemoveRange(pos, SegmentPoints.Count - pos);

            List<XYZ> SegmentsComplete = SegmentPoints;

            //Add final number to complete poligon
            SegmentsComplete.Add(SegmentPoints[0]);

            

            int CurrentPointCount = SegmentsComplete.Count;

            // Add X and Y of each point in list
            for (int i = 0; i < CurrentPointCount; i++)
            {

                if (i != 0)
                {
                    double difXVar = Math.Abs(Math.Round(SegmentsComplete[i].X, 0) - Math.Round(SegmentsComplete[i - 1].X, 0));
                    double difYVar = Math.Abs(Math.Round(SegmentsComplete[i].Y, 0) - Math.Round(SegmentsComplete[i - 1].Y, 0));
 
                    int countDifX = (int)(difXVar / 5);
                    int countDifY = (int)(difYVar / 5);

                    List<double> xMultipliers = new List<double>();
                    List<double> yMultipliers = new List<double>();

                    double tempCountX = 5;


                    if (SegmentsComplete[i].X > SegmentsComplete[i - 1].X)
                    {
                        for (int ct = 0; ct < countDifX; ct++)
                        {

                            xMultipliers.Add(Math.Round(SegmentsComplete[i - 1].X + tempCountX, 0));
                            tempCountX += 5;
                        }
                    }
                    else
                    {
                        for (int ct = 0; ct < countDifX; ct++)
                        {
                            xMultipliers.Add(Math.Round(SegmentsComplete[i - 1].X - tempCountX, 0));
                            tempCountX += 5;
                        }
                    }

                    double tempCountY = 5;

                    if (SegmentsComplete[i].Y > SegmentsComplete[i - 1].Y)
                    {
                        for (int ct = 0; ct < countDifY; ct++)
                        {
                            yMultipliers.Add(Math.Round(SegmentsComplete[i - 1].Y + tempCountY, 0));
                            tempCountY += 5;
                        }
                    }
                    else
                    {
                        for (int ct = 0; ct < countDifY; ct++)
                        {
                            yMultipliers.Add(Math.Round(SegmentsComplete[i - 1].Y - tempCountY, 0));
                            tempCountY += 5;
                        }
                    }




                    if (difXVar > 5 && difYVar > 5)
                        {
                        for (int ct = 0; ct < countDifX; ct++)
                        {
                            resultList.Add(new float[] { (float)xMultipliers[ct], (float)yMultipliers[ct]  });
                            }                            
                        }

                        if (difXVar > 5 && difYVar < 5)
                        {
                            for (int ct = 0; ct < countDifX; ct++)
                            {
                            resultList.Add(new float[] { (float)xMultipliers[ct], (float)Math.Round(SegmentsComplete[i - 1].Y,0) });
                            } 
                        }

                        if (difXVar < 5 && difYVar > 5)
                        {
                            for (int ct = 0; ct < countDifY; ct++)
                            {
                            resultList.Add(new float[] { (float)Math.Round(SegmentsComplete[i - 1].X,0), (float)yMultipliers[ct] });
                            }
                        }                      
                }


                    double xVar = Math.Round(SegmentsComplete[i].X, 0);
                    double YVar = Math.Round(SegmentsComplete[i].Y, 0);
                    resultList.Add(new float[] { (float)xVar, (float)YVar });

            }


            float[][] result = resultList.ToArray();
            return result;
        }

    }
}

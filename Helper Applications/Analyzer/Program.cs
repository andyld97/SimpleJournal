using SimpleJournal.Common;
using SimpleJournal.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = SimpleJournal.Common.Data.Point;

namespace Analyzer
{
    public static class Program
    {
        private static readonly List<Shape> shapes = new List<Shape>();
        private static readonly List<Rect> rects = new List<Rect>();

        private static readonly string WRITING = "w";
        private static readonly string DRAWING = "d";
        private static readonly string TEXT_SEARCH = "ts";
        private static readonly string DELIMITTER = "|";

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check if arguments are set 
            if (Environment.GetCommandLineArgs().Count() >= 2)
            {                
                try
                {
                    var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal");
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal", "Analyser.temp");

                    // First argument is a StrokeCollection encoded in base64
                    string base64Content = System.IO.File.ReadAllText(tempPath); //Environment.GetCommandLineArgs()[1];
                    byte[] inkResult = Convert.FromBase64String(base64Content);

                    try
                    {
                        // Try to delete the file to clean everything up
                        System.IO.File.Delete(tempPath);
                    } 
                    catch
                    {
                        // ignore
                    }

                    // Second argument is optional (the operation whether the InkAnalyzer should analyze the given StrokeCollection as Text or as Shape
                    string operationMethod = Environment.GetCommandLineArgs()[1];
                    if (operationMethod == DRAWING)
                    {
                        // drawing analysis
                    }
                    else if (operationMethod == WRITING)
                    {
                        // writing analysis
                    }
                    else if (operationMethod == TEXT_SEARCH)
                    {
                        // writing analysis and text search
                    }
                    else
                    {
                        operationMethod = DRAWING;
                    }

                    // Convert bytes to StrokeCollection via MemoryStream
                    StrokeCollection collection = null;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(inkResult, 0, inkResult.Length);
                        ms.Position = 0;
                        collection = new StrokeCollection(ms);
                    }

                    if (collection != null)
                    {
                        if (operationMethod == TEXT_SEARCH && Environment.GetCommandLineArgs().Count() >= 3)
                        {
                            string searchString = Environment.GetCommandLineArgs()[2]; // base64
                            string encodedString = System.Text.Encoding.Default.GetString(Convert.FromBase64String(searchString));

                            // Search for string and print StrokeCollections as base64 where string is found
                            Search(collection, encodedString);
                        }
                        else
                        {
                            // Do analysis here 
                            InkAnalyzer analyzer = new InkAnalyzer();
                            analyzer.AddStrokes(collection);
                            analyzer.SetStrokesType(collection, (operationMethod == DRAWING ? StrokeType.Drawing : StrokeType.Writing));
                            analyzer.Analyze();

                            if (operationMethod == WRITING)
                            {
                                // Text
                                Queue<string> result = new Queue<string>();
                                AnalyzeNode(analyzer.RootNode, result);

                                string builder = string.Empty;
                                foreach (string sub in result)
                                    builder += sub;

                                Console.WriteLine(builder);
                            }
                            else if (operationMethod == DRAWING)
                            {
                                // Shape
                                AddShapes(analyzer.RootNode);

                                int counter = 0;
                                foreach (Shape s in shapes)
                                {
                                    string result = XamlWriter.Save(s);
                                    Rect currentRectangle = rects[counter++];

                                    var xmlRect = Serialization.SaveToString(currentRectangle, System.Text.Encoding.Default);
                                    Console.WriteLine($"{Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(result + DELIMITTER + xmlRect))}{Environment.NewLine}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Debug: System.Windows.MessageBox.Show("Folgender Fehler ist beim Analyisieren aufgetreten: " + ex.Message);
                }
            }
            System.Windows.Forms.Application.Exit();
        }

        private static void AnalyzeNode(ContextNode node, Queue<string> target)
        {
            if (node is WritingRegionNode)
            {
                WritingRegionNode textNode = node as WritingRegionNode;
                target.Enqueue(textNode.GetRecognizedString() + "\n\n");
            }

            ContextNodeCollection.ContextNodeCollectionEnumerator enumerator = node.SubNodes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    ContextNode current = enumerator.Current;
                    if (current is WritingRegionNode)
                    {
                        AnalyzeNode(current, target);
                    }
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        private static void Search(StrokeCollection sc, string text)
        {
            var analyzer = new InkAnalyzer();
            if (sc == null || sc.Count == 0)
                return;

            analyzer.AddStrokes(sc);
            analyzer.SetStrokesType(sc, StrokeType.Writing);
            analyzer.Analyze();

            Queue<ContextNode> nodesToSearch = new Queue<ContextNode>();
            nodesToSearch.Enqueue(analyzer.RootNode);

            while (nodesToSearch.Count != 0)
            {
                var currentNode = nodesToSearch.Dequeue();

                if (currentNode is WritingRegionNode textNode)
                {
                    textNode.PartiallyPopulated = true;
                    if (textNode.GetRecognizedString().AreEqual(text))
                    {
                        // Get position in text
                        string result = textNode.GetRecognizedString().ToLower();

                        int start = result.IndexOf(text.ToLower());
                        int length = text.Length;

                        var nNodes = textNode.GetNodesFromTextRange(ref start, ref length);
                        foreach (InkWordNode n in nNodes.OfType<InkWordNode>())
                        {
                            List<int> numbers = new List<int>();
                            foreach (Stroke st in n.Strokes)
                            {
                                Console.Write(sc.IndexOf(st) + ",");
                            }
                            Console.WriteLine("");
                        }

                        break;
                    }
                }

                foreach (var subNode in currentNode.SubNodes)
                {
                    nodesToSearch.Enqueue(subNode);
                }
            }
        }

        private static void AddShapes(ContextNode node)
        {
            if (node is InkDrawingNode drawingNode)
            {
                var location = drawingNode.Location.GetBounds();

                if (drawingNode.GetShapeName() != "Other")
                {
                    rects.Add(location);
                    Shape shapeToAdd = drawingNode.GetShape();
                    shapeToAdd.SnapsToDevicePixels = true;
                    shapeToAdd.UseLayoutRounding = true;

                    if (shapeToAdd is Polygon)
                    {
                        StrokeCollection sc = new StrokeCollection();
                        var poly = shapeToAdd as Polygon;

                        // Generate strokes from generated shape with convex hull to get a clean polygon
                        List<Point> convexHull = ConvexHull.GetConvexHull(poly.Points.Select(p => new SimpleJournal.Common.Data.Point(p.X, p.Y)).ToList());
                        for (int i = 0; i < convexHull.Count; i++)
                        {
                            int from = i;
                            int to = (i + 1) % convexHull.Count;

                            List<Point> points = new List<Point>() { convexHull[from], convexHull[to] };
                            sc.Add(new Stroke(new StylusPointCollection(points.Select(p => new StylusPoint(p.X, p.Y)))));
                        }

                        Rect bounds = sc.GetBounds();
                        PointCollection pc = new PointCollection();

                        // Shift all the points by the calculated extent of the strokes.
                        Matrix mat = new Matrix();
                        mat.Translate(-bounds.Left, -bounds.Top);

                        sc.Transform(mat, false);
                        foreach (Stroke s in sc)
                            foreach (System.Windows.Point p in s.StylusPoints)
                                pc.Add(p);

                        // Shift the polygon back to original location
                        poly.SetValue(InkCanvas.LeftProperty, bounds.Left);
                        poly.SetValue(InkCanvas.TopProperty, bounds.Top);
                        poly.Points = pc;
                    }
                    shapes.Add(shapeToAdd);
                }
            }

            foreach (ContextNode subNode in node.SubNodes)
                AddShapes(subNode);
        }
    }
}
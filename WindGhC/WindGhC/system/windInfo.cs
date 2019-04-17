using System;
using System.Linq;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC.system
{
    public class windInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the windInfo class.
        /// </summary>
        public windInfo()
          : base(
                "windInfo", 
                "wI",
                "Description",
                "STR.Wind", 
                "postProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Plug in geometry component as a tree", GH_ParamAccess.tree);
            pManager.AddPointParameter("Probes", "p", "Insert a tree of points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Point count", "p", "Assign the number of points per column",GH_ParamAccess.item);
            pManager.AddIntegerParameter("No. columns depth", "n_d", "Assign the number of columns in depth",GH_ParamAccess.item);
            pManager.AddIntegerParameter("No. columns width", "n_w", "Assign the number of columns in width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Point spacing", "d", "Assign the distance inbetween the points", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("windInfo", "w", "windInfo files",GH_ParamAccess.list);
            pManager.AddPointParameter("Probe points", "p", "Probe points for display", GH_ParamAccess.tree);
           
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Structure<GH_Brep> iGeometry;
            List<Point3d> iProbes = new List<Point3d>();
            int iNoPts = 0;
            int iNoCols = 0;
            int iNoColsWidth = 0;
            double iDist = 0;



            DA.GetDataTree(0, out iGeometry);
            DA.GetDataList(1, iProbes);
            DA.GetData(2, ref iNoPts);
            DA.GetData(3, ref iNoCols);
            DA.GetData(4, ref iNoColsWidth);
            DA.GetData(5, ref iDist);

            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();

            int x = 0;
            Brep convertedBrep = null;
            foreach (GH_Path path in iGeometry.Paths)
            {
                foreach (var geom in iGeometry.get_Branch(path))
                {
                    GH_Convert.ToBrep(geom, ref convertedBrep, 0);
                    convertedGeomTree.Add(convertedBrep, new GH_Path(x));
                    convertedBrep = null;
                }
                x += 1;
            }
            
            
            Point3d centerPt = GetCenterPt(convertedGeomTree.AllData());

            if (iNoPts == 0)
                iNoPts = 10;
            if (iDist == 0)
                iDist = 2 * centerPt.Z / 10;
            if (iNoCols == 0)
                iNoCols = 1;
            

            DataTree<Point3d> windInfoPts = new DataTree<Point3d>();
            if(iProbes.Count != 0)
                foreach(var point in iProbes)
                    windInfoPts.Add(point, new GH_Path(0, 0));

            for (int i = 1; i < iNoCols + 1; i++)
            {
                    for (int j = 1; j < iNoPts + 1; j++)                       
                        windInfoPts.Add(new Point3d(centerPt.X - 5 - iDist * (i-1), centerPt.Y , 2 * centerPt.Z / 10 * j), new GH_Path(i,0));
            }

            if(iNoColsWidth > 0)
            {
                for (int i = 1; i < iNoCols + 1; i++)
                {
                    for(int j = 1; j < iNoColsWidth + 1; j++)
                    { 
                        for (int k = 1; k < iNoPts + 1; k++)
                        { 
                            windInfoPts.Add(new Point3d(centerPt.X - 5 - iDist * (i - 1), centerPt.Y + iDist * j, 2 * centerPt.Z / 10 * k), new GH_Path(i, 2*j-1));
                            windInfoPts.Add(new Point3d(centerPt.X - 5 - iDist * (i - 1), centerPt.Y - iDist * j, 2 * centerPt.Z / 10 * k), new GH_Path(i, 2*j));
                        }
                    }
                }

            }

            List<TextFile> windInfoFiles = new List<TextFile>();
            foreach (var path in windInfoPts.Paths)
            {
                #region shellString
                string shellString =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                  "| =========                 |                                                 |\n" +
                  "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                  "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                  "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                  "|    \\\\/     M anipulation  |                                                 |\n" +
                  "\\*---------------------------------------------------------------------------*/\n" +
                    "windInfo_{0}\n" +
                    "   {{\n" +

                    "       // Where to load it from\n" +
                    "       functionObjectLibs (\"libsampling.so\");\n" +

                    "       type probes;\n" +

                    "       // Name of the directory for probe data\n" +
                    "       name windInfo_1;\n" +

                    "       // Write at same frequency as fields\n" +
                    "       outputControl timeStep;\n" +
                    "       outputInterval  1;\n" +

                    "       // Fields to be probed\n" +
                    "       fields\n" +
                    "       (\n" +
                    "           p U\n" +
                    "       );\n" +

                    "       //For Spectral analysis and velo profile\n" +
                    "       probeLocations\n" +
                    "       (\n" +
                    "       {1}\n" +
                    "       );\n" +
                    "   }}";
                #endregion

                string pathIndex = path[0].ToString() + "_" + path[1].ToString();

                string ptCoord = "";
                foreach (var pt in windInfoPts.Branch(path))
                    ptCoord += "(" + pt.X.ToString() + "   " + pt.Y.ToString() + "   " + pt.Z.ToString() + ")\n       ";


                string tempWindFile = string.Format(shellString, pathIndex, ptCoord);

                var oWindFile = new TextFile(tempWindFile, "windInfo_" + pathIndex);

                windInfoFiles.Add(oWindFile);
            }
            
            
            DA.SetDataList(0, windInfoFiles);
            DA.SetDataTree(1, windInfoPts);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("912b6c28-28c7-489d-8598-d2c090b39108"); }
        }

        public List<Point3d> PopulateEdges(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = new List<Point3d>();
            Point3d[] edgePoints;
            foreach (var brep in iGeometry)
                foreach (var edge in brep.Edges)
                {
                    edge.DivideByCount(10, true, out edgePoints);
                    foreach (var point in edgePoints)
                        pointsList.Add(point);
                }

            return pointsList;

        }

        public Point3d GetCenterPt(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = PopulateEdges(iGeometry);

            double xMin = pointsList.OrderBy(p => p.X).ToList()[0].X;
            double xMax = pointsList.OrderBy(p => p.X).ToList()[pointsList.Count - 1].X;
            double yMin = pointsList.OrderBy(p => p.Y).ToList()[0].Y;
            double yMax = pointsList.OrderBy(p => p.Y).ToList()[pointsList.Count - 1].Y;
            double zMin = pointsList.OrderBy(p => p.Z).ToList()[0].Z;
            double zMax = pointsList.OrderBy(p => p.Z).ToList()[pointsList.Count - 1].Z;
            return new Point3d((xMin + xMax)/2, (yMin + yMax)/2, (zMin + zMax)/2);
        }
                
    }
}
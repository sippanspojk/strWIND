using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC
{
    public class Domain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Domain()
          : base(
                "Domain",
                "D",
                "Defining the computational domain.",
                "STR.Wind",
                "preProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometry", "G", "Reference geometry from Rhino.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angle", "A", "Rotation angle in degrees.", GH_ParamAccess.item, 0.0);
            pManager.AddTextParameter("Patch Names", "N", "Define the names of the patches as a list.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Refinement Level", "R", "Define the refinement level of the geometry patches.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Patchedge Ref Level", "PR", "Define the refinement level of the patchedges.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Depth", "X", "Two values that defines how far the tunnel expands in +X and -X from the center of the object [m]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Y", "Width of the wind tunnel [m].", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Z", "Height of the wind tunnel [m]", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Zero plane", "0", "Zero plane", GH_ParamAccess.item);

            Params.Input[0].DataMapping = GH_DataMapping.Graft;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Domain", "D", "Computational domain", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> iGeometry = new GH_Structure<IGH_GeometricGoo>();
            double iAngle = 0.0;
            List<string> iPatchNames = new List<string>();
            List<int> iRefLvlGeom = new List<int>();
            List<int> ieMeshRefLvl = new List<int>();
            List<double> iX = new List<double>();
            double iY = 0.0;
            double iZ = 0.0;
            Plane iZeroPlane = new Plane();

            DA.GetDataTree(0, out iGeometry);
            DA.GetData(1, ref iAngle);
            DA.GetDataList(2, iPatchNames);
            DA.GetDataList(3, iRefLvlGeom);
            DA.GetDataList(4, ieMeshRefLvl);
            DA.GetDataList(5, iX);
            DA.GetData(6, ref iY);
            DA.GetData(7, ref iZ);
            DA.GetData(8, ref iZeroPlane);


            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();
            List<GH_Path> pathList = new List<GH_Path>();

            foreach (var path in iGeometry.Paths)
                for (int i = 0; i < iGeometry.get_Branch(path).Count; i++)
                    pathList.Add(path);

            var flattenedGeom = iGeometry.FlattenData();
            for (int i = 0; i < flattenedGeom.Count; i++)
            {
                flattenedGeom[i].CastTo(out Brep tempBrep);
                convertedGeomTree.Add(tempBrep, pathList[i]);
            }


            DataTree<Brep> rotatedGeomTree = convertedGeomTree;
            Point3d centerPt = GetCenterPt(rotatedGeomTree.AllData());
            foreach (GH_Path path in rotatedGeomTree.Paths)
            {
                foreach (var brep in rotatedGeomTree.Branch(path))
                    brep.Rotate(iAngle * Math.PI / 180, Vector3d.ZAxis, centerPt);
            }

            if (DA.GetData(8, ref iZeroPlane))
            {
                centerPt.Z = iZeroPlane.OriginZ;
            }

            double height = GetHeight(convertedGeomTree.AllData());
            double width = GetWidth(convertedGeomTree.AllData());
            double depth = GetDepth(convertedGeomTree.AllData());

            if (!DA.GetDataList(2, iPatchNames))
            {
                for (int i = 0; i < rotatedGeomTree.BranchCount; i++)
                    iPatchNames.Add("PATCH_" + i);
            }

            if (!DA.GetDataList(3, iRefLvlGeom))
            {
                for (int i = 0; i < rotatedGeomTree.BranchCount; i++)
                    iRefLvlGeom.Add(2);
            }

            if (!DA.GetDataList(4, ieMeshRefLvl))
            {
                for (int i = 0; i < rotatedGeomTree.BranchCount; i++)
                    ieMeshRefLvl.Add(2);
            }

            if (!DA.GetDataList(5, iX))
            {
                iX.Add(4 * height);
                iX.Add(12 * height);
            }

            if (!DA.GetData(6, ref iY))
            {
                iY = 6 * height;
            }

            if (!DA.GetData(7, ref iZ))
            {
                iZ = 6 * height;
            }

            if (iX[0] <= 0.5 * depth || iX[1] <= 0.5 * depth)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The depth of the wind tunnel is too small!");
            else if (iX[0] < 2.5 * height || iX[1] < 7.5 * height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The depth of the wind tunnel is small, be aware.");

            if (iY < 4 * height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The width of the wind tunnel is small, be aware.");
            else if (iY <= width)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The width of the wind tunnel is too small!");

            if (iZ < 6 * height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The height of the wind tunnel is small, be aware.");
            else if (iZ <= height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The height of the wind tunnel is too small!");

            List<Point3d> cornerPoints = new List<Point3d>
            {
                new Point3d(centerPt.X - iX[0], centerPt.Y - iY / 2, centerPt.Z),
                new Point3d(centerPt.X + iX[1], centerPt.Y - iY / 2, centerPt.Z),
                new Point3d(centerPt.X + iX[1], centerPt.Y + iY / 2, centerPt.Z),
                new Point3d(centerPt.X - iX[0], centerPt.Y + iY / 2, centerPt.Z),
                new Point3d(centerPt.X - iX[0], centerPt.Y - iY / 2, centerPt.Z + iZ),
                new Point3d(centerPt.X + iX[1], centerPt.Y - iY / 2, centerPt.Z + iZ),
                new Point3d(centerPt.X + iX[1], centerPt.Y + iY / 2, centerPt.Z + iZ),
                new Point3d(centerPt.X - iX[0], centerPt.Y + iY / 2, centerPt.Z + iZ)
            };


            List<Brep> surfaceList = new List<Brep>{
                Brep.CreateFromCornerPoints(cornerPoints[0], cornerPoints[4], cornerPoints[7], cornerPoints[3], 0.01),
                Brep.CreateFromCornerPoints(cornerPoints[1], cornerPoints[2], cornerPoints[6], cornerPoints[5], 0.01),
                Brep.CreateFromCornerPoints(cornerPoints[3], cornerPoints[7], cornerPoints[6], cornerPoints[2], 0.01),
                Brep.CreateFromCornerPoints(cornerPoints[0], cornerPoints[1], cornerPoints[5], cornerPoints[4], 0.01),
                Brep.CreateFromCornerPoints(cornerPoints[0], cornerPoints[3], cornerPoints[2], cornerPoints[1], 0.01),
                Brep.CreateFromCornerPoints(cornerPoints[4], cornerPoints[5], cornerPoints[6], cornerPoints[7], 0.01)
            };


            List<string> nameList = new List<string>
            {
                "INLET",
                "OUTLET",
                "LEFTSIDE",
                "RIGHTSIDE",
                "BOTTOM",
                "TOP"
            };

            DataTree<Brep> oGeometryList = new DataTree<Brep>();

            int j = 0;
            int k = 0;
            foreach (var surface in surfaceList)
            {
                surface.SetUserString("Name", nameList[j]);
                surface.SetUserString("RefLvl", "0");
                surface.SetUserString("eMeshLvl", "0");
                surface.SetUserString("RotAngle", iAngle.ToString());
                oGeometryList.Add(surface, new GH_Path(k));
                j++;
                k++;
            };

            j = 0;
            foreach (GH_Path path in rotatedGeomTree.Paths)
            {
                foreach (var brep in rotatedGeomTree.Branch(path))
                {
                    brep.SetUserString("Name", iPatchNames[j]);
                    brep.SetUserString("RefLvl", iRefLvlGeom[j].ToString());
                    brep.SetUserString("eMeshLvl", ieMeshRefLvl[j].ToString());
                    brep.SetUserString("RotAngle", iAngle.ToString());
                    oGeometryList.Add(brep, new GH_Path(k));
                }
                j++;
                k++;
            }

            DA.SetDataTree(0, oGeometryList);
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
                return Properties.Resources.domain;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("63a85692-89ab-4120-b1fc-8c9a0271b0bf"); }
        }

        //This function populates iGeometry with points along each edge and returns them in pointsList.
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

        //#return class    #input class  #input
        public double GetDepth(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = PopulateEdges(iGeometry);

            double xMin = pointsList.OrderBy(p => p.X).ToList()[0].X;
            double xMax = pointsList.OrderBy(p => p.X).ToList()[pointsList.Count - 1].X;

            return Math.Abs(xMax - xMin);
        }

        //#return class    #input class  #input
        public double GetWidth(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = PopulateEdges(iGeometry);

            double yMin = pointsList.OrderBy(p => p.Y).ToList()[0].Y;
            double yMax = pointsList.OrderBy(p => p.Y).ToList()[pointsList.Count - 1].Y;

            return Math.Abs(yMax - yMin);
        }

        //#return class    #input class  #input
        public double GetHeight(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = PopulateEdges(iGeometry);

            double zMin = pointsList.OrderBy(p => p.Z).ToList()[0].Z;
            double zMax = pointsList.OrderBy(p => p.Z).ToList()[pointsList.Count - 1].Z;

            return Math.Abs(zMax - zMin);
        }

        //#return class    #input class  #input
        public Point3d GetCenterPt(List<Brep> iGeometry)
        {
            List<Point3d> pointsList = PopulateEdges(iGeometry);

            double xMin = pointsList.OrderBy(p => p.X).ToList()[0].X;
            double xMax = pointsList.OrderBy(p => p.X).ToList()[pointsList.Count - 1].X;
            double yMin = pointsList.OrderBy(p => p.Y).ToList()[0].Y;
            double yMax = pointsList.OrderBy(p => p.Y).ToList()[pointsList.Count - 1].Y;
            double zMin = pointsList.OrderBy(p => p.Z).ToList()[0].Z;
            double zMax = pointsList.OrderBy(p => p.Z).ToList()[pointsList.Count - 1].Z;
            return new Point3d((xMin + xMax) / 2, (yMin + yMax) / 2, zMin);
        }


    }
}
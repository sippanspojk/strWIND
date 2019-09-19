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
    public class Geometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Geometry()
          : base(
                "Geometry", 
                "Geometry",
                "Assemble the geometry.",
                "STR.Wind",
                "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometry", "G", "Reference geometry from Rhino.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angle", "A", "Rotation angle in degrees.", GH_ParamAccess.item, 0.0);
            pManager.AddTextParameter("Patch Names", "N", "Input names of the geometry patches as a string list.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Refinement Level", "R", "Input refinement of the geometry patches as a list.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Depth", "X", "Two values that defines how far the tunnel expands in +X and -X from the center of the object [m]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "Y", "Width of the wind tunnel [m].", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Z", "Height of the wind tunnel [m]", GH_ParamAccess.item);
                        
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Assembled geometry", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> iGeometry = new GH_Structure<IGH_GeometricGoo>();
            double iAngle = 0.0;
            List<string> iNameGeom = new List<string>();
            List<int> iRefLvlGeom = new List<int>();
            List<double> iX = new List<double>();
            double iY = 0.0;
            double iZ = 0.0;
            
            DA.GetDataTree(0, out iGeometry);
            DA.GetData(1, ref iAngle);
            DA.GetDataList(2, iNameGeom);
            DA.GetDataList(3, iRefLvlGeom);
            DA.GetDataList(4, iX);
            DA.GetData(5, ref iY);
            DA.GetData(6, ref iZ);


            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();
            List<GH_Path> pathList = new List<GH_Path>();

            foreach (var path in iGeometry.Paths)
                for (int i = 0; i < iGeometry.get_Branch(path).Count; i++)
                    pathList.Add(path);

            var flattenedGeom =  iGeometry.FlattenData();
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

       
            double height = GetHeight(convertedGeomTree.AllData());
            double width = GetWidth(convertedGeomTree.AllData());
            double depth = GetDepth(convertedGeomTree.AllData());

            if (iX[0] * height < 2.5 * depth || iX[1] * height < 1.5 * depth)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The depth of the wind tunnel is too small, please specity a bigger number.");

            if (iY < 2.0 * width)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The width of the wind tunnel is too small, please specity a bigger number.");

            if (iZ < 2.0 * height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The hight of the wind tunnel is too small, please specity a bigger number.");

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
                surface.SetUserString("RotAngle", iAngle.ToString());
                oGeometryList.Add(surface,new GH_Path(k));
                j++;
                k++;
            };

            j = 0;
            foreach (GH_Path path in rotatedGeomTree.Paths)
            {
                foreach (var brep in rotatedGeomTree.Branch(path))
                {
                    brep.SetUserString("Name", iNameGeom[j]);
                    brep.SetUserString("RefLvl", iRefLvlGeom[j].ToString());
                    brep.SetUserString("RotAngle", iAngle.ToString());
                    oGeometryList.Add(brep,new GH_Path(k));
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
                return null;
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
            foreach(var brep in iGeometry)
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
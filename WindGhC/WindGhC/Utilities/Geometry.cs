 using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
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



            pManager.AddGeometryParameter("Geometry", "G", "Reference geometry from Rhino.", GH_ParamAccess.list);
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
            pManager.AddBrepParameter("Geometry", "G", "Assembled geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iGeometry = new List<Brep>();
            double iAngle = 0.0;
            List<String> iNameGeom = new List<String>();
            List<int> iRefLvlGeom = new List<int>();
            List<double> iX = new List<double>();
            double iY = 0.0;
            double iZ = 0.0;
            
            DA.GetDataList(0, iGeometry);
            DA.GetData(1, ref iAngle);
            DA.GetDataList(2, iNameGeom);
            DA.GetDataList(3, iRefLvlGeom);
            DA.GetDataList(4, iX);
            DA.GetData(5, ref iY);
            DA.GetData(6, ref iZ);

            Point3d centerPt = GetCenterPt(iGeometry);

            foreach (var brep in iGeometry)
            {
                brep.Rotate(iAngle * Math.PI / 180, Vector3d.ZAxis, centerPt);
            }

            centerPt = GetCenterPt(iGeometry);
                        
            double height = GetHeight(iGeometry);
            double width = GetWidth(iGeometry);
            double depth = GetDepth(iGeometry);

            if (iX[0] * height < 2.5 * depth || iX[1] * height < 1.5 * depth)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The depth of the wind tunnel is too small, please specity a bigger number.");

            if (iY < 2.0 * width)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The width of the wind tunnel is too small, please specity a bigger number.");

            if (iZ < 2.0 * height)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The hight of the wind tunnel is too small, please specity a bigger number.");

            List<Point3d> cornerPoints = new List<Point3d>{
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

            
            List<String> nameList = new List<String>
            {
                "INLET",
                "OUTLET",
                "LEFTSIDE",
                "RIGHTSIDE",
                "BOTTOM",
                "TOP"
            };
                        
            List<Brep> geometryList = new List<Brep>();

            int i = 0;
            foreach (var surface in surfaceList)
            {
                surface.SetUserString("Name", nameList[i]);
                surface.SetUserString("RefLvl", "0");
                surface.SetUserString("RotAngle", iAngle.ToString());
                geometryList.Add(surface);
                i++;
            };

            i = 0;
            foreach (var brep in iGeometry)
            {                
                brep.SetUserString("Name", iNameGeom[i]);
                brep.SetUserString("RefLvl", iRefLvlGeom[i].ToString());
                brep.SetUserString("RotAngle", iAngle.ToString());
                geometryList.Add(brep);
                i++;
            }


            DA.SetDataList(0, geometryList);
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
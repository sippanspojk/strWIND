using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC.Properties
{
    public class refinementBoxes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the refinementBoxes class.
        /// </summary>
        public refinementBoxes()
          : base(
                "refinementBoxes",
                "refBoxes",
                "Generate refinement boxes.",
                "STR.Wind",
                "Meshing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("refinementBox", "B", "Plug in the refinement box as a closed polysurface.", GH_ParamAccess.list);
            pManager.AddTextParameter("Name", "N", "Name of refinementbox.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("refinementLevel", "R", "Refinement level of the refinement box.", GH_ParamAccess.list, 4);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("refinementBox", "refBox", "refinementBox", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iRefBoxList = new List<Brep>();
            List<string> iNameList = new List<string>();
            List<int> iRefLevelList = new List<int>();

            DA.GetDataList(0, iRefBoxList);
            DA.GetDataList(1, iNameList);
            DA.GetDataList(2, iRefLevelList);


            List<Brep> oRefBoxList = new List<Brep>();

            int i = 0;
            foreach (var box in iRefBoxList)
            {
                List<Point3d> vertexList = new List<Point3d>();

                foreach (var vertex in box.Vertices)
                    vertexList.Add(vertex.Location);

                vertexList = vertexList.OrderBy(p => p.X).ThenBy(p => p.Y).ThenBy(p => p.Z).ToList();
                box.SetUserString("Name", iNameList[i]);
                box.SetUserString("RefLvl", iRefLevelList[i].ToString());
                box.SetUserString("MinCoord", vertexList[0].ToString().Replace(",", " "));
                box.SetUserString("MaxCoord", vertexList[vertexList.Count - 1].ToString().Replace(",", " "));
                oRefBoxList.Add(box);
                i++;
            }


            DA.SetDataList(0, oRefBoxList);
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
                return Properties.Resources.refBox;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d4e86076-7519-43c8-bd7f-c7ae8c837be7"); }
        }
    }
}
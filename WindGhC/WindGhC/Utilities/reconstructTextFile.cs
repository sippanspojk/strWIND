using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC.Utilities
{
    public class reconstructTextFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the reconstructTextFile class.
        /// </summary>
        public reconstructTextFile()
          : base(
                "reconstructTextFile", 
                "rTF",
                "Description",
                "STR.Wind", 
                "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("textFile", "F", "File to be reconstructed as wind text file", GH_ParamAccess.list);
            pManager.AddTextParameter("Name", "N", "Name of wind text file", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Reconstructed textFile", "rF", "Reconstructed wind text file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> iTextList = new List<string>();
            string iName = "";

            DA.GetDataList(0, iTextList);
            DA.GetData(1, ref iName);

            string fileString = "";
            foreach(var row in iTextList)
            {
                fileString += row + "\n";
            }

            var oWindFile = new TextFile(fileString, iName);

            DA.SetData(0,oWindFile);
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
                return Properties.Resources.reconstructFile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d23d1181-8fca-497f-b386-5d63325b07b9"); }
        }
    }
}
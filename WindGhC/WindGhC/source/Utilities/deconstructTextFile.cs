using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC.Utilities
{
    public class deconstructTextFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the deconstructTextFile class.
        /// </summary>
        public deconstructTextFile()
          : base(
                "deconstructTextFile",
                "dTF",
                "Deconstruct into editable text file.",
                "STR.Wind",
                "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("textFile", "F", "Input wind textFile to deconstruct", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Deconstructed File", "D", "Deconstructed wind textFile", GH_ParamAccess.list);
            pManager.AddTextParameter("File name", "N", "Name of deconstructed file, needed for reconstruction", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var iTextFile = new TextFile();

            DA.GetData(0, ref iTextFile);

            List<string> oDeconstructedFile = new List<string>();
            string textFile = iTextFile.GetFileText();


            string[] splitString = textFile.Split('\n');
            foreach (string row in splitString)
                oDeconstructedFile.Add(row);

            DA.SetDataList(0, oDeconstructedFile);
            DA.SetData(1, iTextFile.GetName());
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
                return Properties.Resources.deConstructFile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2b243b5d-194c-4528-bfb8-292c7e0cf1a7"); }
        }

    }
}
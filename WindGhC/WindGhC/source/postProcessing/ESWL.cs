using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC.source.postProcessing
{
    public class ESWL : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ESWL class.
        /// </summary>
        public ESWL()
          : base("ESWL", 
                "ESWL",
                "Genereate ESWL function file for post processing",
                "STR.Wind",
                "postProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Domain", "D", "Input domain.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("StatStart", "S", "At what time should the statistics start", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("gPeak", "g", "Peak factor", GH_ParamAccess.item, 3.2);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ESWL", "E", "ESWL function file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Brep> iDomain;
            var iStatStart = 0.0;
            var iGPeak = 0.0;

            DA.GetDataTree(0, out iDomain);
            DA.GetData(1, ref iStatStart);
            DA.GetData(2, ref iGPeak);

            if (iGPeak <= 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "g peak can't be negative or equal to zero, please assign a different value.");
                return;
            }

            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();

            int iPath = 0;
            Brep convertedBrep = null;
            foreach (GH_Path path in iDomain.Paths)
            {
                foreach (var geom in iDomain.get_Branch(path))
                {
                    GH_Convert.ToBrep(geom, ref convertedBrep, 0);
                    convertedGeomTree.Add(convertedBrep, new GH_Path(iPath));
                    convertedBrep = null;
                }
                iPath += 1;
            }

  
            string brepNames = "";
            for (int i = 6; i < convertedGeomTree.Paths.Count; i++)
                brepNames += "        " + convertedGeomTree.Branch(convertedGeomTree.Path(i))[0].GetUserString("Name") + "\n";
            string statStartString = iStatStart.ToString();
            string gPeakString = iGPeak.ToString();
            string ESWLString =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                    "| =========                 |                                                 |\n" +
                    "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                    "|  \\\\    /   O peration     | Website:  www.OpenFOAM.org                      |\n" +
                    "|   \\\\  /    A nd           | Version: 6                                      |\n" +
                    "|    \\\\/     M anipulation  |                                                 |\n" +
                    "\\*---------------------------------------------------------------------------*/\n" +
                    "ESWL\n" +
                    "{\n" +
                    "    type                ESWL;\n" +
                    "    functionObjectLibs(\"libESWL.so\");\n" +
                    "\n" +
                    "    statStartTime       " + statStartString + ";     //At what time should the statistics start?\n" +
                    "\n" +
                    "    gPeak               " + gPeakString + ";\n" +
                    "\n" +
                    "    patches\n" +
                    "    (\n" +
                    brepNames +
                    "    );\n" +
                    "}";

            var oESWLFile = new TextFile(ESWLString, "ESWL");

            DA.SetData(0, oESWLFile);

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
            get { return new Guid("02885c87-618e-4cdf-8192-21cc67519db2"); }
        }
    }
}
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
    public class windProbes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the windProbes class.
        /// </summary>
        public windProbes()
          : base(
                "windProbes",
                "wI",
                "Generate wind probes function file for post processing",
                "STR.Wind",
                "postProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Probes", "p", "Insert a tree of points", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("windProbes", "w", "windProbes files", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Structure<GH_Point> iProbes;

            DA.GetDataTree(0, out iProbes);


            DataTree<Point3d> convertedProbesTree = new DataTree<Point3d>();

            int x = 0;
            Point3d convertedPoint = new Point3d();
            foreach (GH_Path path in iProbes.Paths)
            {
                foreach (var pt in iProbes.get_Branch(path))
                {
                    GH_Convert.ToPoint3d(pt, ref convertedPoint, 0);
                    convertedProbesTree.Add(convertedPoint, new GH_Path(x));
                    convertedPoint = new Point3d();
                }
                x += 1;
            }


            List<TextFile> windProbesFiles = new List<TextFile>();
            foreach (var path in convertedProbesTree.Paths)
            {
                #region shellString
                string shellString =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Website:  www.OpenFOAM.org                      |\n" +
                "|   \\\\  /    A nd           | Version: 6                                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                    "{0}\n" +
                    "   {{\n" +

                    "       // Where to load it from\n" +
                    "       functionObjectLibs (\"libsampling.so\");\n" +

                    "       type probes;\n" +

                    "       // Name of the directory for probe data\n" +
                    "       name {1};\n" +

                    "       // Write at same frequency as fields\n" +
                    "       writeControl timeStep;\n" +
                    "       writeInterval  1;\n" +

                    "       // Fields to be probed\n" +
                    "       fields\n" +
                    "       (\n" +
                    "           p U\n" +
                    "       );\n" +

                    "       //For Spectral analysis and velo profile\n" +
                    "       probeLocations\n" +
                    "       (\n" +
                    "       {2}\n" +
                    "       );\n" +
                    "   }}";
                #endregion                

                string ptCoord = "";
                foreach (var pt in convertedProbesTree.Branch(path))
                    ptCoord += "(" + pt.X.ToString() + "   " + pt.Y.ToString() + "   " + pt.Z.ToString() + ")\n       ";

                string name = "windProbes_" + path.ToString().Replace("{", "").Replace("}", "");

                string tempWindFile = string.Format(shellString, name, name, ptCoord);

                var oWindFile = new TextFile(tempWindFile, name);

                windProbesFiles.Add(oWindFile);
            }

            DA.SetDataList(0, windProbesFiles);
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
                return Properties.Resources.windProbes;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("912b6c28-28c7-489d-8598-d2c090b39108"); }
        }
    }
}
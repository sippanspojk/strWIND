using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC._0
{
    public class nuSgs : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the nuSgs class.
        /// </summary>
        public nuSgs()
          : base(
                "nuSgs", 
                "nuSgs",
                "Description",
                "STR.Wind",
                "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Geometry.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("nuSgs", "nuSgs", "Assembled BC file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var iGeometry = new List<Brep>();

            DA.GetDataList(0, iGeometry);

            string nuSgsInsert = "";

            for(int i = 6; i < iGeometry.Count; i++)
            {
                nuSgsInsert += "   " + iGeometry[i].GetUserString("Name") + "\n" +
                    "    {\n" +
                    "        type           nutUSpaldingWallFunction;\n" +
                    "        value          $internalField;\n" + 
                    "    }\n" +
                    "\n";
            }

            string shellString =
                "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       volScalarField;\n" +
                "     location       \"0\";\n" +
                "     object      nut;\n" +
                "}}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "dimensions     [0 2 -1 0 0 0 0];\n\r" +

                "internalField  uniform 1e-05;\n\r" +

                "boundaryField\n" +
                "{{\n\r" +

                "    INLET\n" +
                "    {{\n" +
                "           type            zeroGradient;\n" +
                "    }}\n\r" +

                "    OUTLET\n" +
                "    {{\n" +
                "           type            zeroGradient;\n" +
                "    }}\n\r" +

                "    LEFTSIDE\n" +
                "    {{\n" +
                "           type            symmetry;\n" +
                "    }}\n\r" +

                "    RIGHTSIDE\n" +
                "    {{\n" +
                "           type            symmetry;\n" +
                "    }}\n\r" +

                "    BOTTOM\n" +
                "    {{\n" +
                "           type            zeroGradient;\n" +
                "           value            $internalField;\n" +
                "    }}\n\r" +

                "    TOP\n" +
                "    {{\n" +
                "           type            symmetry;\n" +
                "    }}\n\r" +

                "{0}\n" +
                "}}";

            string nuSgs = string.Format(shellString, nuSgsInsert);

            var oNuSgs = new TextFile(nuSgs, "nuSgs");

            DA.SetData(0, oNuSgs);

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
            get { return new Guid("44b95dc1-c932-46ea-8582-a3a9785d6e40"); }
        }
    }
}
using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC
{
    public class p : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the p class.
        /// </summary>
        public p()
          : base(
                "p", 
                "p",
                "Assemble the preassure boundary condition file.",
                "STR.Wind",
                "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("p", "p", "Assembled preassure boundary condition file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iGeometry = new List<Brep>();

            DA.GetDataList(0, iGeometry);

            string patchInsert = "";

            for(int i = 6; i < iGeometry.Count; i++ )
            {
                patchInsert += "    " + iGeometry[i].GetUserString("Name") + "\n" +
                    "    {\n" +
                    "           type           zeroGradient;\n" + 
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
              "     object      p;\n" +
              "}}\n" +
              "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
              "\n" +

              "dimensions       [0 2 -2 0 0 0 0];\n\r" +

              "internalField    uniform 0;\n\r" +

              "boundaryField\n\r" +
              "{{\n" +

              "    INLET\n" +
              "    {{\n" +
              "           type            zeroGradient;\n" +
              "    }}\n\r" +

              "    OUTLET\n" +
              "    {{\n" +
              "           type            fixedValue;\n" +
              "           value           uniform 0;\n" +
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
              "    }}\n\r" +

              "    TOP\n" +
              "    {{\n" +
              "           type            symmetry;\n" +
              "    }}\n\r" +

              "{0}\n" +
              
              "}}";

            string oPressureString = string.Format(shellString, patchInsert);

            var oPressureTextFile = new TextFile(oPressureString, "p");                                
           
            DA.SetData(0, oPressureTextFile);

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
            get { return new Guid("0619ab39-d0e4-479f-991d-07eafcb54855"); }
        }
                        
    }
}
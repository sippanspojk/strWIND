using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC
{
    public class U : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the U class.
        /// </summary>
        public U()
          : base(
                "U", 
                "U",
                "Assemble the velocity boundary condition file.",
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
            pManager.AddVectorParameter("Internal field vector", "V", "Insert a vector representing the internal field velocity.", GH_ParamAccess.item,Vector3d.XAxis);
            pManager.AddVectorParameter("Inlet velocity", "U", "Insert a vector representing the inlet velocity.", GH_ParamAccess.item, Vector3d.XAxis);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("U", "U", "Assembled velocity boundary condition file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iGeometry = new List<Brep>();
            Vector3d iVelocityVec = new Vector3d(0, 0, 0);
            Vector3d iInletVec = new Vector3d(0, 0, 0);

            DA.GetDataList(0, iGeometry);
            DA.GetData(1, ref iVelocityVec);
            DA.GetData(2, ref iInletVec);

            iGeometry[0].SetUserString("BC", iInletVec.ToString().Replace(",", " "));
            string geomInsert = "";

            
            for (int i = 6; i < iGeometry.Count; i++)
            {
                geomInsert += "   " + iGeometry[i].GetUserString("Name") + "\n" +
                    "    {\n" +
                    "        type           fixedValue;\n" +
                    "        value          uniform (0 0 0);\n" + 
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
                "     class       volVectorField;\n" +
                "     object      U;\n" +
                "}}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "dimensions     [0 1 -1 0 0 0 0];\n\r" +

                "internalField  uniform (" + iVelocityVec.ToString().Replace(","," ") + ");\n\r" +

                "boundaryField\n" +
                "{{\n\r" +

                "    INLET\n" + 
                "    {{\n" +
                "           type            timeVaryingMappedFixedValue;\n" +
                "           setAverage	    0;\n" +
                "           offset          (0 0 0);\n" +
                "           //type            fixedValue;\n" +
                "           //value           uniform (" + iInletVec.ToString().Replace(",", " ") + ");\n\r" +
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
                "           type            fixedValue;\n" +
                "           value           uniform (0 0 0);\n" +
                "    }}\n\r" +

                "    TOP\n" +
                "    {{\n" +
                "           type            symmetry;\n" +
                "    }}\n\r" +

                "{0}\n" +
                "}}";
             
            string oVelocityString = string.Format(shellString, geomInsert);

            var oVelocityTextFile = new TextFile(oVelocityString, "U");
                        
            DA.SetData(0, oVelocityTextFile);

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
            get { return new Guid("d42445a4-eec5-412a-9f41-7b4c1bfe4fd8"); }
        }

    }
}
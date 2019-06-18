using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC.system
{
    public class cutPlanesVTK : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the cutPlanesVTK class.
        /// </summary>
        public cutPlanesVTK()
          : base(
                "cutPlanesVTK", 
                "cP",
                "Genereate cutPlane function file for post processing.",
                "STR.Wind", 
                "postProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Cut plane", "p", "Input a list of planes to use as cut planes for post processing", GH_ParamAccess.list, new Plane(new Point3d(), Vector3d.YAxis));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("cutPlanesVTK", "p", "cutPlanesVTK file. Include in ctrlDict and Assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Plane> iPlane = new List<Plane>();

            DA.GetDataList(0, iPlane);

            string cutPlane = "";
            int i = 1;
            foreach (var plane in iPlane)
            {
                string xOrigin = plane.OriginX.ToString();
                string yOrigin = plane.OriginY.ToString();
                string zOrigin = plane.OriginZ.ToString();
                string originCoord = xOrigin + " " + yOrigin + " " + zOrigin;

                string xNormal = plane.Normal.X.ToString();
                string yNormal = plane.Normal.Y.ToString();
                string zNormal = plane.Normal.Z.ToString();
                string normalCoord = xNormal + " " + yNormal + " " + zNormal;

                cutPlane += "       Cutplane" + i + "_anim\n" +
                "       {\n" +
                "           type            cuttingPlane;\n" +
                "           planeType       pointAndNormal;\n" +
                "           pointAndNormalDict\n" +
                "           {\n" +
                "               basePoint       (" + originCoord + ");\n" +
                "               normalVector    (" + normalCoord + ");\n" +
                "           }\n" +
                "           interpolate     true;\n" +
                "       }\n" +
                "\n";  

                i += 1;

            }

            #region shellstring
            string shellString =
                   "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                  "| =========                 |                                                 |\n" +
                  "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                  "|  \\\\    /   O peration     |                                                 |\n" +
                  "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                  "|    \\\\/     M anipulation  |                                                 |\n" +
                  "\\*---------------------------------------------------------------------------*/\n" +
                    "cuttingPlaneVTK\n" +
                    "{{\n" +

                    "   type                surfaces;\n" +
                    "   functionObjectLibs  (\"libsampling.so\");\n" +
                    "   writeControl       timeStep;   //'timeStep' or 'outputTime'\n" +
                    "   writeInterval      1;\n" +
                    "\n" +
                    "   surfaceFormat       vtk;\n" +
                    "   fields              (p U);\n" +
                    "\n" +
                    "   interpolationScheme cellPoint;\n" +
                    "\n" +
                    "   surfaces\n" +
                    "   (\n" +
                    "{0}\n"+
                    "   );\n" +
                    "}}";
            #endregion

            string tempCutPlanesVTK = string.Format(shellString, cutPlane);

            var oCutPlanesVTK = new TextFile(tempCutPlanesVTK, "cutPlanesVTK_anim");

            DA.SetData(0, oCutPlanesVTK);
        
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
            get { return new Guid("0fd386e6-014c-4239-86c2-f0aca8c6544a"); }
        }
    }
}
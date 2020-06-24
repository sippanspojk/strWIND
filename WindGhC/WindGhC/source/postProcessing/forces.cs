using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC.source.postProcessing
{
    public class forces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the forces class.
        /// </summary>
        public forces()
          : base("forces",
                "f",
                "Generate forces function file for post processing.",
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
            pManager.AddIntegerParameter("Write control", "wC", "Write control", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Write interval", "wI", "Write interval", GH_ParamAccess.item, 1.0);

            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("forces", "f", "Forces function file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Brep> iDomain;
            var iWriteControl = 0;
            var iWriteInterval = 0.0;


            DA.GetDataTree(0, out iDomain);
            DA.GetData(1, ref iWriteControl);
            DA.GetData(2, ref iWriteInterval);

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


            if (iWriteControl < 1 || iWriteControl > 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entry unknown! To set write control add a ValueList component.");
                return;
            }

            string writeInterval = iWriteInterval.ToString();
            string writeControl = "";
            switch (iWriteControl)
            {
                case 1:
                    writeControl = "timeStep";
                    break;
                case 2:
                    writeControl = "outputTime";
                    break;
            }

            string forcesString = "";
            for (int i = 6; i < convertedGeomTree.Paths.Count; i++)
            {
                string brepName = convertedGeomTree.Branch(convertedGeomTree.Path(i))[0].GetUserString("Name").ToString();

                string CofR_X = VolumeMassProperties.Compute(convertedGeomTree.Branch(convertedGeomTree.Path(i))).Centroid.X.ToString();
                string CofR_Y = VolumeMassProperties.Compute(convertedGeomTree.Branch(convertedGeomTree.Path(i))).Centroid.Y.ToString();
                string CofR_Z = VolumeMassProperties.Compute(convertedGeomTree.Branch(convertedGeomTree.Path(i))).Centroid.Z.ToString();
                string CofR = CofR_X + " " + CofR_Y + " " + CofR_Z;

                forcesString +=
                        "forces_" + brepName + "\n" +
                        "{\n" +
                        "   type                forces;\n" +
                        "   functionObjectLibs  (\"libforces.so\");\n" +
                        "   writeControl        " + writeControl + ";   //'timeStep' or 'outputTime'\n" +
                        "   writeInterval       " + writeInterval + ";\n" +
                        "   name                forces_" + brepName +
                        "   \n" +
                        "   log                 yes;\n" +
                        "   \n" +
                        "   patches             (\"" + brepName + "\");\n" +

                        "   log                 true;\n" +

                        "   rho             rhoInf;\n" +
                        "   rhoInf              1.18;\n" +
                        "   CofR                (" + CofR + ");\n" +
                        "}\n" +
                        "\n";
            }

            string brepNames = "";
            for (int i = 6; i < convertedGeomTree.Paths.Count; i++)
                brepNames += "       " + convertedGeomTree.Branch(convertedGeomTree.Path(i))[0].GetUserString("Name") + "\n";

            convertedGeomTree.Flatten();
            List<Brep> flattenedList = convertedGeomTree.Branch(convertedGeomTree.Path(0));

            string geomXCoord = VolumeMassProperties.Compute(flattenedList.GetRange(6, flattenedList.Count - 6)).Centroid.X.ToString();
            string geomYCoord = VolumeMassProperties.Compute(flattenedList.GetRange(6, flattenedList.Count - 6)).Centroid.Y.ToString();
            string geomCentCoord = geomXCoord + " " + geomYCoord + " 0.0";

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
                    "\n" +
                    "forces_All\n" +
                    "{{\n" +
                    "   type                forces;\n" +
                    "   functionObjectLibs  (\"libforces.so\");\n" +
                    "   writeControl        {1};   //'timeStep' or 'outputTime'\n" +
                    "   writeInterval       {2};\n" +
                    "   name                forces_SK_All\n" +
                    "\n" +
                    "   log                 yes;\n" +
                    "\n" +
                    "   patches\n" +
                    "   (\n" +
                    "{3}" +
                    "   );\n" +
                    "   log                 true;\n" +

                    "   rho                 rhoInf;\n" +
                    "   rhoInf              1.20;\n" +
                    "   CofR                ({4});\n" +
                    "}}";
            #endregion
           
            string forcesFile = string.Format(shellString, forcesString, writeControl, writeInterval, brepNames, geomCentCoord);

            var oForcesFile = new TextFile(forcesFile, "forces");

            DA.SetData(0, oForcesFile);
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
            get { return new Guid("26c6524c-fd18-4741-be35-622679e5396f"); }
        }

        protected override void BeforeSolveInstance()
        {
            if (this.Params.Input[1].SourceCount > 0)
            {
                if (this.Params.Input[1].SourceCount == 1)
                {
                    if (this.Params.Input[1].Sources[0] is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        var ValueList = Params.Input[1].Sources[0] as Grasshopper.Kernel.Special.GH_ValueList;
                        ValueList.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;

                        if (
                            ValueList.ListItems[0].Name != "timeStep" ||
                            ValueList.ListItems[1].Name != "outputTime"
                           )
                        {
                            ValueList.ListItems.Clear();

                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("timeStep", "1"));
                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("outputTime", "2"));

                            //Refresh ValueList
                            ValueList.ExpireSolution(true);
                        }
                    }
                }
            }
        }
    }
}
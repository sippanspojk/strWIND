using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC
{
    public class ControlDict : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ControlDict class.
        /// </summary>
        public ControlDict()
          : base(
                "ControlDict",
                "ctrlDict",
                "Assemble the controlDict file",
                "STR.Wind",
                "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("StartFrom", "S", "Add a ValueList component to specify where to start from", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("StopAt", "S", "Add a ValueList component to specify where to stop at", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("StartTime", "T", "Specify a start time", GH_ParamAccess.item, "0");
            pManager.AddTextParameter("EndTime", "E", "Specify an end time", GH_ParamAccess.item, "60");
            pManager.AddTextParameter("DeltaT", "dT", "Specify the time step size", GH_ParamAccess.item, "0.1");
            pManager.AddTextParameter("WriteInterval", "I", "Specify how often the results should be written out", GH_ParamAccess.item, "10");
            pManager.AddGenericParameter("IncludeFunctions", "f", "Add functions to be included", GH_ParamAccess.list);

            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("controlDict", "ctrlDict", "Assembled controlDict file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            int iStartFrom = 0;
            int iStopAt = 0;
            string iStartTime = "";
            string iEndTime = "";
            string iDeltaT = "";
            string iWriteInterval = "";
            List<TextFile> iFunctions = new List<TextFile>();

            DA.GetData(0, ref iStartFrom);
            DA.GetData(1, ref iStopAt);
            DA.GetData(2, ref iStartTime);
            DA.GetData(3, ref iEndTime);
            DA.GetData(4, ref iDeltaT);
            DA.GetData(5, ref iWriteInterval);
            DA.GetDataList(6, iFunctions);

            string startFrom = "";
            string stopAt = "";

            if (iStartFrom < 1 || iStartFrom > 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entry unknown! To set where to start add ValueList component.");
                return;
            }
            switch (iStartFrom)
            {
                case 1:
                    startFrom = "startTime";
                    break;
                case 2:
                    startFrom = "latestTime";
                    break;
            }

            if (iStopAt < 1 || iStopAt > 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Entry unknown! To set where to stop add ValueList component.");
                return;
            }
            switch (iStopAt)
            {
                case 1:
                    stopAt = "endTime";
                    break;
                case 2:
                    stopAt = "writeNow";
                    break;
            }

            string functions = "";

            foreach (var function in iFunctions)
                functions += "    #include \"" + function.GetName() + "\"\n";

            #region shellstring
            string shellString =
                "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Website:  www.OpenFOAM.org                      |\n" +
                "|   \\\\  /    A nd           | Version: 6                                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     object      controlDict;\n" +
                "}}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +
                
                "application pimpleFoam;\n\r" +
                "startFrom {0};\n\r" +
                "startTime {1};\n\r" +
                "stopAt {2};\n\r" +
                "endTime {3};\n\r" +
                "deltaT {4};\n\r" +
                "writeControl runTime;\n\r" +
                "writeInterval {5};\n\r" +
                "writeFormat binary;\n\r" +
                "writePrecision 7;\n\r" +
                "//writeCompression compressed;\n\r" +
                "timeFormat general;\n\r" +
                "timePrecision 5;\n\r" +
                "runTimeModifiable yes;\n\r" +
                "adjustTimeStep no;\n\r" +
                "maxCo 100;\n\r" +
                "maxDeltaT 0.100;\n\r" +
                "maxCoMultiplier 2;\n\r" +
                "UBoundMin -500;\n\r" +
                "UBoundMax 500;\n\r" +
                "maxNonOrtho 60;\n\r" +
                "libs\n" +
                "(\n" +
                "    \"libOpenFOAM.so\"\n" +
                "    \"libincompressibleTurbulenceModels.so\"\n" +
                "    //\"libincompressibleTurbulenceModel.so\" FOR OLDER OpenFOAM versions\n" +
                "    //\"libincompressibleRASModels.so\" FOR OLDER OpenFOAM versions\n" +
                "    //\"libincompressibleLESModels.so\" FOR OLDER OpenFOAM versions\n" +
                ");\n" +
                "functions\n" +
                "{{\n" +
                "{6}\n" +
                "    #include \"forces\"\n" +
                "}}";

            #endregion

            string controlDict = string.Format(shellString, startFrom, iStartTime, stopAt, iEndTime, iDeltaT, iWriteInterval, functions);

            var oControlDictTextFile = new TextFile(controlDict, "controlDict");

            DA.SetData(0, oControlDictTextFile);

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
            get { return new Guid("2742a525-c3f0-4664-b414-3f8878a5ed91"); }
        }


        protected override void BeforeSolveInstance()
        {
            if (this.Params.Input[0].SourceCount > 0)
            {
                if (this.Params.Input[0].SourceCount == 1)
                {
                    if (this.Params.Input[0].Sources[0] is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        var ValueList = Params.Input[0].Sources[0] as Grasshopper.Kernel.Special.GH_ValueList;
                        ValueList.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;

                        if (
                            ValueList.ListItems[0].Name != "startTime" ||
                            ValueList.ListItems[1].Name != "latestTime"
                           )
                        {
                            ValueList.ListItems.Clear();

                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("startTime", "1"));
                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("latestTime", "2"));

                            //Refresh ValueList
                            ValueList.ExpireSolution(true);
                        }
                    }
                }
            }

            if (this.Params.Input[1].SourceCount > 0)
            {
                if (this.Params.Input[1].SourceCount == 1)
                {
                    if (this.Params.Input[1].Sources[0] is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        var ValueList = Params.Input[1].Sources[0] as Grasshopper.Kernel.Special.GH_ValueList;
                        ValueList.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;

                        if (
                            ValueList.ListItems[0].Name != "endTime" ||
                            ValueList.ListItems[1].Name != "writeNow"
                           )
                        {
                            ValueList.ListItems.Clear();

                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("endTime", "1"));
                            ValueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem("writeNow", "2"));

                            //Refresh ValueList
                            ValueList.ExpireSolution(true);
                        }
                    }
                }
            }
        }
    }
}
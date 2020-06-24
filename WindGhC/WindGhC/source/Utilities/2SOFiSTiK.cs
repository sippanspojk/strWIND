using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC.Utilities
{
    public class _2SOFiSTiK : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _2SOFiSTiK class.
        /// </summary>
        public _2SOFiSTiK()
          : base(
                "2SOFiSTiK",
                "2sof",
                "Generates a SOFiSTiK case for static structural analysis.",
                "STR.Wind",
                "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Button", "B", "Write to file", GH_ParamAccess.item, false);
            pManager.AddTextParameter("readPath", "rP", "Path to read from", GH_ParamAccess.item);
            pManager.AddTextParameter("writePath", "wP", "Path to write to", GH_ParamAccess.item);
            pManager.AddIntegerParameter("startNumber", "n", "Start of node numbering", GH_ParamAccess.item, 9000);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Control points to which the loads are applied", GH_ParamAccess.list);
            pManager.AddNumberParameter("Point numbers", "N", "Point numbers that are used for in the load file", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool iButton = false;
            string iReadPath = "";
            string iWritePath = "";
            int iStartNo = 0;

            DA.GetData(0, ref iButton);
            DA.GetData(1, ref iReadPath);
            DA.GetData(2, ref iWritePath);
            DA.GetData(3, ref iStartNo);

            string fileLocation = "";
            string folderLocation = "";

            if (iWritePath != "")
                folderLocation = iWritePath;
            else
            {
                fileLocation = OnPingDocument().FilePath;
                if (fileLocation == null)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Either save the GH definition or manually assign a path to write to.");
                else
                    folderLocation = new DirectoryInfo(fileLocation).Parent.FullName + @"\";
            }

            // Generate directory
            string sofistikPath = Path.Combine(folderLocation, "SOFiSTiK");
            Directory.CreateDirectory(sofistikPath);


            // Generate load nodes and node numbers
            List<Point3d> oLoadNodes = new List<Point3d>();

            int lvl = 0;
            List<List<string>> allDataAsList = new List<List<string>>();
            while (true)
            {
                string fileName = string.Format("level_{0}.dat", lvl.ToString());

                if (File.Exists(Path.Combine(iReadPath, fileName)) == true)
                {
                    List<string> tempCaseList = new List<string>();
                    tempCaseList = File.ReadLines(Path.Combine(iReadPath, fileName)).ToList();
                    allDataAsList.Add(tempCaseList);

                    string[] tempArray = tempCaseList[2].Split(' ');
                    List<float> pointCoords = new List<float>();
                    foreach (var part in tempArray)
                    {
                        if (Char.IsDigit(part, 0))
                        {
                            string tempString = part.Replace(',', ' ');
                            pointCoords.Add(float.Parse(tempString));
                        }
                    }
                    oLoadNodes.Add(new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]));

                    lvl++;
                }

                else
                    break;
            }


            List<int> oNodeNumbers = new List<int>();
            for (int i = 0; i < oLoadNodes.Count; i++)
                oNodeNumbers.Add(i + iStartNo);

            DA.SetDataList(0, oLoadNodes);
            DA.SetDataList(1, oNodeNumbers);




            // Generate load files         
            List<string> loadtypeList = new List<string> { "PXX", "PYY", "PZZ", "MXX", "MYY", "MZZ" };
            if (iButton)
            {
                List<List<List<float>>> invertedData = new List<List<List<float>>>();
                foreach (var panel in allDataAsList)
                {
                    List<List<float>> panelInvertedList = new List<List<float>>();
                    for (int i = 0; i < panel[7].Split(' ').Length; i++)
                    {
                        List<float> tempList = new List<float>();
                        foreach (var timeStep in panel.GetRange(7, panel.Count - 7))
                        {
                            string[] ss = timeStep.Split(' ');
                            tempList.Add(float.Parse(ss[i]));
                        }
                        panelInvertedList.Add(tempList);
                    }
                    invertedData.Add(panelInvertedList);
                }


                List<List<float>> meanValues = new List<List<float>>();
                foreach (var panel in invertedData)
                {
                    List<float> tempList = new List<float>();
                    for (int i = 1; i < panel.Count; i++)
                        tempList.Add(panel[i].Sum() / panel[i].Count);

                    meanValues.Add(tempList);
                }


                List<List<float>> standardDev = new List<List<float>>();
                int p = 0;
                foreach (var panel in invertedData)
                {
                    List<float> tempList = new List<float>();
                    for (int i = 1; i < panel.Count; i++)
                    {
                        float sumOfSqr = 0.0f;
                        foreach (var load in panel[i])
                            sumOfSqr += (float)Math.Pow((load - meanValues[p][i - 1]), 2);

                        tempList.Add((float)Math.Sqrt(sumOfSqr / (panel[i].Count - 1)));
                    }
                    p++;
                    standardDev.Add(tempList);
                }


                List<List<List<float>>> correlationMatrix = new List<List<List<float>>>();
                for (int i = 0; i < meanValues[0].Count; i++)
                {
                    List<List<float>> subCorrMatrix = new List<List<float>>();
                    for (int row = 0; row < invertedData.Count; row++)
                    {
                        List<float> rowList = new List<float>();
                        for (int col = 0; col < invertedData.Count; col++)
                        {
                            float tempSum = 0.0f;
                            float coVariance = 0.0f;
                            for (int timeStep = 0; timeStep < invertedData[row][i + 1].Count; timeStep++)
                            {
                                tempSum += ((invertedData[row][i + 1][timeStep] - meanValues[row][i]) * (invertedData[col][i + 1][timeStep] - meanValues[col][i]));
                            };
                            coVariance = tempSum / (invertedData[row][i + 1].Count - 1);
                            rowList.Add(coVariance / (standardDev[row][i] * standardDev[col][i]));
                        };
                        subCorrMatrix.Add(rowList);
                    };
                    correlationMatrix.Add(subCorrMatrix);
                };

                string oCorrMatrix = "";
                int LC = 0;
                foreach (var matrix in correlationMatrix)
                {
                    oCorrMatrix += " Correlation matrix for " + loadtypeList[LC] + "\n";
                    foreach (var row in matrix)
                    {
                        foreach (var col in row)
                        {
                            oCorrMatrix += col.ToString() + " ";
                        }
                        oCorrMatrix += "\n";
                    }
                    oCorrMatrix += "\n";
                    LC++;
                }
                File.WriteAllText(Path.Combine(sofistikPath, "correlationMatrix.dat"), oCorrMatrix);


                int peakFactor = 4;
                string oLoadFile = "";
                lvl = 0;
                List<List<float>> loadValues = new List<List<float>>();
                for (int row = 0; row < invertedData.Count; row++)
                {
                    oLoadFile += "NODE " + (lvl + iStartNo).ToString() + " VV ";
                    List<float> panelLoads = new List<float>();
                    for (int i = 0; i < meanValues[0].Count; i++)
                    {
                        float backgroundSum = 0.0f;
                        float loadValue = 0.0f;
                        for (int col = 0; col < invertedData.Count; col++)
                        {
                            backgroundSum += correlationMatrix[i][row][col] * standardDev[col][i];
                        }
                        loadValue = meanValues[row][i] + peakFactor * backgroundSum;
                        panelLoads.Add(loadValue);
                        oLoadFile += loadValue.ToString() + " ";
                    }
                    oLoadFile += "\n";
                    loadValues.Add(panelLoads);
                    lvl++;
                }
                File.WriteAllText(Path.Combine(sofistikPath, "windLoad.dat"), oLoadFile);

                /*                 
                string oDynaLoadFile = "";
                
                LC = 0;
                oDynaLoadFile = "";
                for (int i = 1; i < invertedData.Count; i++)
                {
                    oDynaLoadFile += "LC " + LC + " TYPE NONE \n" +
                        "NODE " + (lvl + iStartNo).ToString() + " TYPE " + loadtypeList[i - 1] + " P1 0.480*0.5\n";

                    for (int j = 0; j < 10; j++)
                    {
                        oDynaLoadFile += "FUNC T " + invertedData[0][j] + " F " + invertedData[i][j] + "\n";
                    }

                    oDynaLoadFile += "\n";
                    LC++;
                }
                string dynaFileName = string.Format("windLoadDyna_N{0}.dat", oNodeNumbers[lvl].ToString());
                File.WriteAllText(Path.Combine(sofistikPath, dynaFileName), oDynaLoadFile);*/

                // Generate teddy analysis case file
                #region teddyFile
                string teddyFile =
                    "-PROG AQUA urs:1 \n" +
                    "head start\n" +
                    "PAGE FIRS 1   UNII 0   FORM 0\n" +
                    "ECHO SECT YES\n" +
                    "ECHO MAT YES\n" +
                    "NORM EN 1992-2004\n" +
                    "NORM EN 1993-2005\n" +
                    "\n" +
                    "$ MATERIAL------------------------------------------------------------------------------\n" +
                    "\n" +
                    "$ CROSS SECTIONS -----------------------------------------------------------------------\n" +
                    "\n" +
                    "END\n" +
                    "\n" +
                    "-PROG SOFIMSHC urs:1.2 $ system definition\n" +
                    "HEAD system definition\n" +
                    "PAGE UNII\n" +
                    "syst SPAC gdiv 10000 gdir NEGZ\n" +
                    "#include $(PROJECT)_msh.dat\n" +
                    "END\n" +
                    "\n" +
                    "!#!Chapter Wind load\n" +
                    "$/////////////////////////////////////////////////////////////////\n" +
                    "\n" +
                    "-prog sofiload urs:4\n" +
                    "head 'Wind load'\n" +
                    "echo load extr\n" +
                    "LC 1 type none TITL 'Wind load'\n" +
                    "#include windLoad.dat\n" +
                    "END";

                File.WriteAllText(Path.Combine(sofistikPath, "analysisCase.dat"), teddyFile);
                #endregion

            }
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
                return Properties.Resources._2sofistik;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("96db54fd-6e8d-429b-8d39-330967b26ef5"); }
        }
    }
}
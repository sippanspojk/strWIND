﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;


namespace WindGhC
{
    public static class StaticTextFiles
    {

        public static string GetFoam(List<Brep> iGeometry)
        {            

            string jobName = iGeometry[6].GetUserString("RotAngle") + "deg_Solve";

            #region foamString
            string foamString =
                "#!/bin/sh\n" +
                "\n" +
                "# PBS -j oe\n" +
                "# PBS -N " + jobName  + "\n" +
                "# PBS -l nodes=5:ppn=6\n" +
                "# PBS -l walltime=1000000:00:00\n" +
                "###output files\n" +
                "###PBS -e err.log\n" +
                "###PBS -o out.log\n" +
                "###Mail to user\n" +
                "# PBS -m ae\n" +
                "# PBS -M bidier@str-ucture.com\n" +
                "\n" +
                "cd $PBS_O_WORKDIR\n" +
                "\n" +
                "LOGFILE = \"${PBS_O_WORKDIR}/out.log\"\n" +
                "ERRFILE = \"${PBS_O_WORKDIR}/err.log\"\n" +
                "NODEFILE = \"${PBS_O_WORKDIR}/nodes.log\"\n" +
                "\n" +
                "cat $PBS_NODEFILE >$NODEFILE\n" +
                "\n" +
                "export NUM_CPUS = 30\n" +
                "export NUM_NODES = 5\n" +
                "\n" +
                "#rm -r processor*\n" +
                "\n" +
                "# reconfigure decomposition method\n" +
                "\n" +
                "sed - i \"s/method ptscotch/method scotch/g\" system / decomposeParDict\n" +
                "\n" +
                "#\n" +
                "# deompose based on pimpleDyMFsiFoam\n" +
                "#\n" +
                "\n" +
                "sed - i \"s/pimpleDyMFoam/pimpleDyMFsiFoam/g\" system / controlDict\n" +
                "echo \"Decpompose Start\" > timeStamp\n" +
                "date >> timeStamp\n" +
                "decomposePar >>$LOGFILE 2 > &1\n" +
                "echo \"Decpompose end\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "sed - i \"s/pimpleDyMFsiFoam/pimpleFoam/g\" system / controlDict\n" +
                "\n" +
                "#\n" +
                "# now, run the solver\n" +
                "#\n" +
                "\n" +
                "echo \"solver start\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "mpirun - hostfile $PBS_NODEFILE - n $NUM_CPUS pimpleFoam -parallel >>$LOGFILE 2 > &1 && \\\n" +
                "echo \"solver end\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "\n" +
                "#echo \"reconstruct start\" >> timeStamp\n" +
                "#date >> timeStamp\n" +
                "#reconstructPar >>$LOGFILE 2>&1 && \\\n" +
                "#echo \"reconstruct end\" >> timeStamp\n" +
                "#date >> timeStamp\n" +
                "\n" +
                "echo \" **** FOAM JOB FINISHED *****\" >>$LOGFILE\n";
            #endregion

            return foamString;

        }
        public static string GetMesh(List<Brep> iGeometry)
        {

            string jobName = iGeometry[6].GetUserString("RotAngle") + "deg_Mesh";

            #region meshString
            string meshString =
                "#!/bin/sh\n" +
                "\n" +
                "# PBS -j oe\n" +
                "# PBS -N " + jobName + "\n" +
                "# PBS -l nodes=5:ppn=6\n" +
                "# PBS -l walltime=1000000:00:00\n" +
                "###output files\n" +
                "###PBS -e err.log\n" +
                "###PBS -o out.log\n" +
                "###Mail to user\n" +
                "# PBS -m ae\n" +
                "# PBS -M andersson@str-ucture.com\n" +
                "\n" +
                "cd $PBS_O_WORKDIR\n" +
                "\n" +
                "LOGFILE = \"${PBS_O_WORKDIR}/out.log\"\n" +
                "ERRFILE = \"${PBS_O_WORKDIR}/err.log\"\n" +
                "NODEFILE = \"${PBS_O_WORKDIR}/nodes.log\"\n" +
                "\n" +
                "cat $PBS_NODEFILE >$NODEFILE\n" +
                "\n" +
                "export NUM_CPUS = 30\n" +
                "export NUM_NODES = 5\n" +
                "\n" +
                "#rm -r processor*\n" +
                "\n" +
                "#\n" +
                "# run blockMesh\n" +
                "# \n" +
                "\n" +
                "blockMesh\n" +
                "\n" +
                "#\n" +
                "# deompose based on pimpleDyMFsiFoam\n" +
                "#\n" +
                "\n" +
                "sed - i \"s/pimpleDyMFoam/pimpleDyMFsiFoam/g\" system / controlDict\n" +
                "echo \"Decpompose Start\" > timeStamp\n" +
                "date >> timeStamp\n" +
                "decomposePar >>$LOGFILE 2 > &1\n" +
                "echo \"Decpompose end\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "sed - i \"s/pimpleDyMFsiFoam/pimpleFoam/g\" system / controlDict\n" +
                "\n" +
                "#\n" +
                "# now, run the mesh\n" +
                "#\n" +
                "\n" +
                "echo \"mesh start\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "mpirun - np $NUM_CPUS - hostfile $PBS_NODEFILE - mca pml ucx snappyHexMesh - parallel >>$LOGFILE 2 > &1 && \\\n" +
                "echo \"mesh end\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "echo \"reconstruct start\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "reconstructParMesh >>$LOGFILE 2 > &1 && \\\n" +
                "echo \"reconstruct end\" >> timeStamp\n" +
                "date >> timeStamp\n" +
                "\n" +
                "echo \" **** FOAM JOB ENDS *****\" >>$LOGFILE";
            #endregion

            return meshString;
        }

        public static string Get3c3dFile()
        {
            return TextFilesResources._3c3d_FOURN_structured;
        }

        public static string GetFVSchemes()
        {
            return TextFilesResources.fvSchemes;
        }
        public static string GetFVSolution()
        {
            return TextFilesResources.fvSolution;
        }
        public static string GetDecomposeParDict()
        {
            return TextFilesResources.decomposeParDict;
        }

        public static string GetRASProperties()
        {
            return TextFilesResources.RASProperties;
        }
        public static string GetLESProperties()
        {
            return TextFilesResources.LESProperties;
        }
        public static string GetTransportProperties()
        {
            return TextFilesResources.transportProperties;
        }
        public static string GetTurbulenceProperties()
        {
            return TextFilesResources.turbulenceProperties;
        }

        public static string GetP(List<Brep> iGeometry)
        {
            string patchInsert = "";

            for (int i = 6; i < iGeometry.Count; i++)
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
                "|  \\\\    /   O peration     | Website:  www.OpenFOAM.org                      |\n" +
                "|   \\\\  /    A nd           | Version: 6                                      |\n" +
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

            return string.Format(shellString, patchInsert);
        }

        public static string GetNut(List<Brep> iGeometry)
        {
            string nutInsert = "";

            for (int i = 6; i < iGeometry.Count; i++)
            {
                nutInsert += "   " + iGeometry[i].GetUserString("Name") + "\n" +
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
                "|  \\\\    /   O peration     | Website:  www.OpenFOAM.org                      |\n" +
                "|   \\\\  /    A nd           | Version: 6                                      |\n" +
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

            return string.Format(shellString, nutInsert);

        }

        public static string GetNuTilda(List<Brep> iGeometry)
        {
            string nuTildaInsert = "";


            for (int i = 6; i < iGeometry.Count; i++)
            {
                nuTildaInsert += "    " + iGeometry[i].GetUserString("Name") + "\n" +
                    "    {\n" +
                    "        type           zeroGradient;\n" +
                    "    }\n" +
                    "\n";
            }

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
                "     class       volScalarField;\n" +
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
                "           type            fixedValue;\n" +
                "           value           uniform 0.0001414;\n" +
                "    }}\n\r" +

                "    OUTLET\n" +
                "    {{\n" +
                "           type            fixedValue;\n" +
                "           value           uniform 0.0001414;\n" +
                "           inletValue      uniform 0.0001414;\n" +
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


            return string.Format(shellString, nuTildaInsert);

        }

        public static string GetSurfaceFeatureExtractDict(List<Brep> iGeometry)
        {
            string surfaceFeatureExtractDict = "";

            for (int i = 0; i < iGeometry.Count; i++)
            {
                surfaceFeatureExtractDict += iGeometry[i].GetUserString("Name") + ".stl\n" +
                    "{\n" +
                    "   // How to obtain raw features (extractFromFile || extractFromSurface)\n" +
                    "   extractionMethod extractFromSurface;\n\r" +

                    "   extractFromSurfaceCoeffs\n" +
                    "   {\n" +
                    "       // Mark edges whose adjacent surface normals are at an angle less\n" +
                    "       // than includedAngle as features\n" +
                    "       // - 0  : selects no edges\n" +
                    "       // - 180: selects all edges\n" +
                    "       includedAngle   150;\n" +
                    "   }\n\r" +

                    "   subsetFeatures\n" +
                    "   {\n" +
                    "       // Keep nonManifold edges (edges with >2 connected faces)\n" +
                    "       nonManifoldEdges    no;\n" +

                    "       // Keep open edges (edges with 1 connected face)\n" +
                    "       openEdges   yes;\n" +
                    "   }\n\r" +


                    "   // Write options\n\r" +

                    "   // Write features to obj format for postprocessing\n" +
                    "   writeObj    yes;\n" +
                    "}\n\r";
            }

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
                "     object      surfaceFeatureExtractDict;\n" +
                "}}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "{0}\n" +
                "}}";

            return string.Format(shellString, surfaceFeatureExtractDict);

        }

        public static string GetForcesFunction(DataTree<Brep> iGeometry)
        {
            string forcesString = "";

            for (int i = 6; i < iGeometry.Paths.Count; i++)
            {
                string brepName = iGeometry.Branch(iGeometry.Path(i))[0].GetUserString("Name").ToString();

                string CofR_X = VolumeMassProperties.Compute(iGeometry.Branch(iGeometry.Path(i))).Centroid.X.ToString();
                string CofR_Y = VolumeMassProperties.Compute(iGeometry.Branch(iGeometry.Path(i))).Centroid.Y.ToString();
                string CofR_Z = VolumeMassProperties.Compute(iGeometry.Branch(iGeometry.Path(i))).Centroid.Z.ToString();
                string CofR = CofR_X + " " + CofR_Y + " " + CofR_Z;

                forcesString +=
                        "forces_" + brepName + "\n" +
                        "{\n" +
                        "   type                forces;\n" +
                        "   functionObjectLibs  (\"libforces.so\");\n" +
                        "   writeControl        timeStep;   //'timeStep' or 'outputTime'\n" +
                        "   writeInterval       1;\n" +
                        "   name                forces_" + brepName +
                        "   \n" +
                        "   log                 yes;\n" +
                        "   \n" +
                        "   patches             (\"" + brepName + "\");\n" +
                        "   //pName               p;\n" +
                        "   //UName               U;\n" +
                        "   log                 true;\n" +
                        "   //rhoName             rhoInf;\n" +
                        "   rho             rhoInf;\n" +
                        "   rhoInf              1.18;\n" +
                        "   CofR                (" + CofR + ");\n" +
                        "   //liftDir             ( 0 0 1 );\n" +
                        "   //dragDir             ( 1 0 0 );\n" +
                        "   //pitchAxis           ( 0 1 0 );\n" +
                        "   //magUInf             24.90;\n" +
                        "   //lRef                154;\n" +
                        "   //Aref                15500;\n" +
                        "}\n" +
                        "\n";
            }

            string brepNames = "";
            for (int i = 6; i < iGeometry.Paths.Count; i++)
                brepNames += "       " + iGeometry.Branch(iGeometry.Path(i))[0].GetUserString("Name") + "\n";

            iGeometry.Flatten();
            List<Brep> flattenedList = iGeometry.Branch(iGeometry.Path(0));

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
                    "   writeControl        timeStep;   //'timeStep' or 'outputTime'\n" +
                    "   writeInterval       1;\n" +
                    "   name                forces_SK_All\n" +
                    "\n" +
                    "   log                 yes;\n" +
                    "\n" +
                    "   patches\n" +
                    "   (\n" +
                    "{1}" +
                    "   );\n" +
                    "   //pName               p;\n" +
                    "   //UName               U;\n" +
                    "   log                 true;\n" +
                    "   //rhoName             rhoInf;\n" +
                    "   rho                 rhoInf;\n" +
                    "   rhoInf              1.20;\n" +
                    "   CofR                ({2});\n" +
                    "   //liftDir             ( 0 0 1 );\n" +
                    "   //dragDir             ( 1 0 0 );\n" +
                    "   //pitchAxis           ( 0 1 0 );\n" +
                    "   //magUInf             24.90;\n" +
                    "   //lRef                154;\n" +
                    "   //Aref                15500;\n" +
                    "}}";
            #endregion


            return string.Format(shellString, forcesString, brepNames, geomCentCoord);
        }
   
    }
}

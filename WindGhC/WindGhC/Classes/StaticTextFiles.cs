using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace WindGhC
{
    public static class StaticTextFiles
    {

        public static string GetFoam()
        {
            return TextFilesResources.foam;
        }
        public static string GetMesh()
        {
            return TextFilesResources.mesh;
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
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
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
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
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

        public static string GetForcesFunction(List<Brep> iGeometry)
        {
            string forcesString = "";

            for (int i = 6; i < iGeometry.Count; i++)
            {
                string brepName = iGeometry[i].GetUserString("Name").ToString();

                string CofR_X = VolumeMassProperties.Compute(iGeometry[i]).Centroid.X.ToString();
                string CofR_Y = VolumeMassProperties.Compute(iGeometry[i]).Centroid.Y.ToString();
                string CofR_Z = VolumeMassProperties.Compute(iGeometry[i]).Centroid.Z.ToString();
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
            for (int i = 6; i < iGeometry.Count; i++)
                brepNames += "       " + iGeometry[i].GetUserString("Name") + "\n";


            string geomXCoord = VolumeMassProperties.Compute(iGeometry[6]).Centroid.X.ToString();
            string geomYCoord = VolumeMassProperties.Compute(iGeometry[6]).Centroid.Y.ToString();          
            string geomCentCoord = geomXCoord + " " + geomYCoord + " 0.0";

            #region shellString       
            string shellString =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                    "| =========                 |                                                 |\n" +
                    "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                    "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                    "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                    "|    \\\\/     M anipulation  |                                                 |\n" +
                    "\\*---------------------------------------------------------------------------*/\n" +
                    "{0}\n" +
                    "\n" +
                    "forces_SK_All\n" +
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


            return string.Format(shellString,forcesString,brepNames,geomCentCoord);
        }
    }
}

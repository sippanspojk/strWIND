using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace WindGhC
{
    public class Assembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Assembly class.
        /// </summary>
        public Assembly()
          : base(
                "Assembly", 
                "Ass",
                "Write Dict files and generate the folderstructure.",
                "STR.Wind",
                "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Path to where to write.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Button", "Btn", "Write to file and generate folder structure.", GH_ParamAccess.item,false);
            pManager.AddBrepParameter("Geometry", "Geom", "Input the geometry.", GH_ParamAccess.list);
            pManager.AddGenericParameter("\\constant", "\\constant", "Insert the the files that goes into the \"constant\" folder as a flat list", GH_ParamAccess.list);
            pManager.AddGenericParameter("\\system", "\\system", "Insert the the files that goes into the \"system\" folder as a flat list", GH_ParamAccess.list);
            pManager.AddGenericParameter("\\0", "\\0", "Insert the the files that goes into the \"0\" folder as a flat list", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string iPath = "";
            bool iButton = false;
            var iGeometry = new List<Brep>();
            var iConstantFolder = new List<TextFile>();
            var iSystemFolder = new List<TextFile>();
            var iZeroFolder = new List<TextFile>();

            DA.GetData(0, ref iPath);
            DA.GetData(1, ref iButton);
            DA.GetDataList(2, iGeometry);
            DA.GetDataList(3, iConstantFolder);
            DA.GetDataList(4, iSystemFolder);
            DA.GetDataList(5, iZeroFolder);


            if (iButton)
            {
                // Specify a name of top-level folder.
                string openFoamFolder = System.IO.Path.Combine(iPath, "OpenFoam");
                
                // Specify a name of sub folders
                string constantPath = System.IO.Path.Combine(openFoamFolder, "constant");
                string systemPath = System.IO.Path.Combine(openFoamFolder, "system");
                string zeroPath = System.IO.Path.Combine(openFoamFolder, "0");

                // Specify a name of subsub folders.
                string polyMeshPath = System.IO.Path.Combine(constantPath, "polyMesh\\");
                string triSurfacePath = System.IO.Path.Combine(constantPath, "triSurface\\");
                                               

                // Generate directories
                Directory.CreateDirectory(zeroPath);
                Directory.CreateDirectory(polyMeshPath);
                Directory.CreateDirectory(triSurfacePath);
                Directory.CreateDirectory(systemPath);

                foreach (var constantFile in iConstantFolder)
                    File.WriteAllText(System.IO.Path.Combine(polyMeshPath, constantFile.GetName()), constantFile.GetFileText());
                foreach (var systemFile in iSystemFolder)
                    File.WriteAllText(System.IO.Path.Combine(systemPath, systemFile.GetName()), systemFile.GetFileText());
                foreach (var bcFile in iZeroFolder)
                    File.WriteAllText(System.IO.Path.Combine(zeroPath, bcFile.GetName()), bcFile.GetFileText());

                foreach (var geom in iGeometry)
                    ExportStl(geom, triSurfacePath + geom.GetUserString("Name"));

                #region fvSolution

                string fvSolution =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     object      fvSolution;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "    SIMPLE\n" +
                "    {\n" +
                "        nNonOrthogonalCorrectors 10;\n" +
                "        pressureImplicitPorousity false;\n" +
                "        pRefCell        0;\n" +
                "        pRefValue       0;\n" +
                "    }\n\r" +

                "    PISO\n" +
                "    {\n" +
                "        nNonOrthogonalCorrectors 1;\n" +
                "        nCorrectors     4;\n" +
                "        pRefCell        0;\n" +
                "        pRefValue       0;\n" +
                "    }\n\r" +
                
                "    PIMPLE\n" +
                "    {\n" +
                "        nNonOrthogonalCorrectors 1;\n" +
                "        nCorrectors     4;\n" +
                "        nOuterCorrectors     1;\n" +
                "        pRefCell        0;\n" +
                "        pRefValue       0;\n" +
                "    }\n\r" +
                
                "    solvers\n" +
                "    {\n" +
                "        p\n" +
                "        {\n" +
                "            solver GAMG;\n" +
                "            agglomerator    faceAreaPair;\n" +
                "            mergeLevels     1;\n" +
                "            cacheAgglomeration true;\n" +
                "            nCellsInCoarsestLevel 200;\n" +
                "            tolerance       1.0e-20;\n" +
                "            relTol          0.01;\n" +
                "            smoother GaussSeidel;\n" +
                "            nPreSweeps      0;\n" +
                "            nPostSweeps     2;\n" +
                "            nFinestSweeps   2;\n" +
                "            maxIter         20;\n" +
                "        }\n\r" +

                "        pFinal\n" +
                "        {\n" +
                "            solver GAMG;\n" +
                "            agglomerator faceAreaPair;\n" +
                "            mergeLevels     1;\n" +
                "            cacheAgglomeration true;\n" +
                "            nCellsInCoarsestLevel 200;\n" +
                "            tolerance       1.0e-07;\n" +
                "            relTol          0.0;\n" +
                "            smoother GaussSeidel;\n" +
                "            nPreSweeps      0;\n" +
                "            nPostSweeps     2;\n" +
                "            nFinestSweeps   2;\n" +
                "            maxIter         20;\n" +
                "        }\n\r" +


                "/*		   U\n" +
                "        {\n" +
                "            solver          smoothSolver;\n" +
                "            smoother        GaussSeidel;\n" +
                "            tolerance       1.0e-08;\n" +
                "            relTol          0.0;\n" +
                "            minIter         1;\n" +
                "        }\n" +
                "*/\n\r" +

                "        U\n" +
                "        {\n" +
                "            solver PBiCG;\n" +
                "            preconditioner DILU;\n" +
                "            tolerance       1e-08;\n" +
                "		     relTol          0.0;\n" +
                "            //minIter         1;\n" +
                "    		 maxIter         100;\n" +
                "        }\n\r" +

                "        UFinal\n" +
                "        {\n" +
                "            solver PBiCG;\n" +
                "            preconditioner DILU;\n" +
                "            tolerance       1e-08;\n" +
                "            relTol          0.0;\n" +
                "            //minIter         1;\n" +
                "    		 maxIter         100;\n" +
                "        }\n\r" +

                "        nuTilda\n" +
                "        {\n" +
                "            solver PBiCG;\n" +
                "            preconditioner DILU;\n" +
                "            tolerance       1e-08;\n" +
                "            relTol          0.0;\n" +
                "            //minIter         1;\n" +
                "            maxIter         100;\n" +
                "        }\n\r" +

                "        nuTildaFinal\n" +
                "        {\n" +
                "            solver PBiCG;\n" +
                "            preconditioner DILU;\n" +
                "            tolerance       1e-08;\n" +
                "            relTol          0.0;\n" +
                "            //minIter         1;\n" +
                "    		 maxIter         100;\n" +
                "        }\n\r" +

                "        cellMotionUx\n" +
                "        {\n" +
                "            solver PCG;\n" +
                "            preconditioner DIC;\n" +
                "            tolerance       1e-08;\n" +
                "            relTol          0;\n" +
                "        }\n\r" +


                "    }\n\r" +

                "    relaxationFactors\n" +
                "    {\n" +
                "        default         0.7;\n" +
                "        p               0.7;\n" +
                "        U               0.7;\n" +
                "        nuTilda         0.7;\n" +
                "    }";




                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSolution"), fvSolution);
                #endregion

                #region fvSchemes

                string fvSchemes =
                "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     object      fvSchemes;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "   ddtSchemes\n" +
                "   {\n" +
                "       default        	    backward;\n" +
                "   }\n\r" +

                "   gradSchemes\n" +
                "   {\n" +
                "//       default             cellLimited Gauss linear 1.0;\n" +
               "        default             cellLimited leastSquares 1.0;\n" +
               "        grad(p)             cellLimited Gauss linear 1.0;\n" +
                "//       grad(U)             Gauss linear;\n" +
                "   }\n\r" +

                "   divSchemes\n" +
                "   {\n" +
                "       default             Gauss linear;\n" +
                "//       div(phi,U)          Gauss filteredLinear2V 1 0 ;\n" +
                "//         div(phi,U)        Gauss linearUpwind cellLimited Gauss linear 1.0;\n" +
                "       div(phi, U)         Gauss linearUpwind grad(U);\n" +
                "//       div(phi,nuTilda)    Gauss limitedLinear 1;\n" +
                "       div(phi, nuTilda)   Gauss upwind;\n" +
                "       div(phi, k)         Gauss limitedLinear 1;\n" +
                "       div(phi, epsilon)   Gauss limitedLinear 1;\n" +
                "       div(phi, omega)     Gauss limitedLinear 1;\n" +
                "       div(phi, R)         Gauss upwind;\n" +
                "       div(R)              Gauss linear;\n" +
                "       div((nuEff* dev(grad(U).T())))  Gauss linear;\n" +
                "       div((nuEff* dev(T(grad(U)))))   Gauss linear;\n" +
                "   }\n\r" +

                "   interpolationSchemes\n" +
                "   {\n" +
                "       default             linear;\n" +
                "   }\n\r" +

                "   laplacianSchemes\n" +
                "   {\n" +
                "       default             Gauss linear limited 0.5;\n" +
                "   }\n\r" +

                "   snGradSchemes\n" +
                "   {\n" +
                "       default             limited 0.5;\n" +
                "   }\n\r" +

                "   fluxRequired\n" +
                "   {\n" +
                "       default             no;\n" +
                "       p                   ;\n" +
                "   }";

                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSchemes"), fvSchemes);
                #endregion

                #region decomposeParDict

                string decomposeParDict =
                "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     object      decomposeParDict;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "numberOfSubdomains 18;\n\r" +

                "method scotch;\n";


                File.WriteAllText(System.IO.Path.Combine(systemPath, "decomposeParDict"), decomposeParDict);
                #endregion

                #region LESProperties

                string LESProperties =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     object      LESProperties;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "LESModel            SpalartAllmarasIDDES;\n" +
                "delta vanDriest;\n" +
                "printCoeffs on;\n" +
                "laminarCoeffs\n" +
                "{\n" +
                "}\n\r" +

                "oneEqEddyCoeffs\n" +
                "{\n" +
                "    ck               0.07;\n" +
                "    ce               1.05;\n" +
                "}\n\r" +

                "dynOneEqEddyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    filter simple;\n" +
                "}\n\r" +

                "locDynOneEqEddyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    filter simple;\n" +
                "}\n\r" +

                "SmagorinskyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    ck               0.07;\n" +
                "}\n\r" +

                "Smagorinsky2Coeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    ck               0.07;\n" +
                "    cD2              0.02;\n" +
                "}\n\r" +

                "spectEddyViscCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    cB               8.22;\n" +
                "    cK1              0.83;\n" +
                "    cK2              1.03;\n" +
                "    cK3              4.75;\n" +
                "    cK4              2.55;\n" +
                "}\n\r" +

                "dynSmagorinskyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    filter simple;\n" +
                "}\n\r" +

                "mixedSmagorinskyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    ck               0.07;\n" +
                "    filter simple;\n" +
                "}\n\r" +

                "dynMixedSmagorinskyCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    filter simple;\n" +
                "}\n\r" +

                "LRRDiffStressCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    ck               0.09;\n" +
                "    c1               1.8;\n" +
                "    c2               0.6;\n" +
                "}\n\r" +

                "DeardorffDiffStressCoeffs\n" +
                "{\n" +
                "    ce               1.05;\n" +
                "    ck               0.09;\n" +
                "    cm               4.13;\n" +
                "}\n\r" +

                "SpalartAllmarasCoeffs\n" +
                "{\n" +
                "    alphaNut         1.5;\n" +
                "    Cb1              0.1355;\n" +
                "    Cb2              0.622;\n" +
                "    Cw2              0.3;\n" +
                "    Cw3              2;\n" +
                "    Cv1              7.1;\n" +
                "    Cv2              5.0;\n" +
                "    CDES             0.65;\n" +
                "    ck               0.07;\n" +
                "}\n\r" +

                "SpalartAllmarasDDESCoeffs\n" +
                "{\n" +
                "    alphaNut         1.5;\n" +
                "    Cb1              0.1355;\n" +
                "    Cb2              0.622;\n" +
                "    Cw2              0.3;\n" +
                "    Cw3              2.0;\n" +
                "    Cv1              7.1;\n" +
                "    Cv2              5.0;\n" +
                "    CDES             0.65;\n" +
                "    ck               0.07;\n" +
                "}\n\r" +

                "SpalartAllmarasIDDESCoeffs\n" +
                "{\n" +
                "    alphaNut         1.5;\n" +
                "    kappa            0.4187;\n" +
                "    Cb1              0.1355;\n" +
                "    Cb2              0.622;\n" +
                "    Cw2              0.3;\n" +
                "    Cw3              2.0;\n" +
                "    Cv1              7.1;\n" +
                "    Cv2              5.0;\n" +
                "    CDES             0.65;\n" +
                "    ck               0.07;\n" +
                "    delta vanDriest;\n" +
                "    vanDriestCoeffs\n" +
                "    {\n" +
                "        delta cubeRootVol;\n" +
                "        cubeRootVolCoeffs\n" +
                "        {\n" +
                "            deltaCoeff      1;\n" +
                "        }\n" +
                "        smoothCoeffs\n" +
                "        {\n" +
                "            delta cubeRootVol;\n" +
                "            cubeRootVolCoeffs\n" +
                "            {\n" +
                "                deltaCoeff      1;\n" +
                "            }\n" +
                "            maxDeltaRatio   1.1;\n" +
                "        }\n" +
                "        Aplus            26;\n" +
                "        Cdelta           0.158;\n" +
                "    }\n" +
                "}\n\r" +

                "cubeRootVolCoeffs\n" +
                "{\n" +
                "    deltaCoeff      1;\n" +
                "}\n\r" +

                "PrandtlCoeffs\n" +
                "{\n" +
                "    delta cubeRootVol;\n" +
                "    cubeRootVolCoeffs\n" +
                "    {\n" +
                "        deltaCoeff      1;\n" +
                "    }\n" +
                "    smoothCoeffs\n" +
                "    {\n" +
                "        delta cubeRootVol;\n" +
                "        cubeRootVolCoeffs\n" +
                "        {\n" +
                "            deltaCoeff      1;\n" +
                "        }\n" +
                "        maxDeltaRatio   1.1;\n" +
                "    }\n" +
                "    Cdelta           0.158;\n" +
                "}\n\r" +

                "vanDriestCoeffs\n" +
                "{\n" +
                "    delta cubeRootVol;\n" +
                "    cubeRootVolCoeffs\n" +
                "    {\n" +
                "        deltaCoeff      1;\n" +
                "    }\n" +
                "    smoothCoeffs\n" +
                "    {\n" +
                "        delta cubeRootVol;\n" +
                "        cubeRootVolCoeffs\n" +
                "        {\n" +
                "            deltaCoeff      1;\n" +
                "        }\n" +
                "        maxDeltaRatio   1.1;\n" +
                "    }\n" +
                "    Aplus            26;\n" +
                "    Cdelta           0.158;\n" +
                "}\n\r" +

                "smoothCoeffs\n" +
                "{\n" +
                "    delta cubeRootVol;\n" +
                "    cubeRootVolCoeffs\n" +
                "    {\n" +
                "        deltaCoeff      1;\n" +
                "    }\n" +
                "    maxDeltaRatio   1.1;\n" +
                "}\n\r" +

                "kappa            0.4187;\n\r" +

                "wallFunctionCoeffs\n" +
                "{\n" +
                "    E                9;\n" + 
                "}";

                File.WriteAllText(System.IO.Path.Combine(constantPath, "LESProperties"), LESProperties);
                #endregion

                #region RASProperties

                string RASProperties =
                     "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     location    \"constant\";\n" +
                "     object      RASProperties;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "RASModel           kEpsilon;\n\r" +

                "turbulence         on;\n\r" +

                "printCoeffs        on;";

                File.WriteAllText(System.IO.Path.Combine(constantPath, "RASProperties"), RASProperties);
                #endregion

                #region turbulenceProperties

                string turbulenceProperties =
                     "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                "| =========                 |                                                 |\n" +
                "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                "|    \\\\/     M anipulation  |                                                 |\n" +
                "\\*---------------------------------------------------------------------------*/\n" +
                "FoamFile\n" +
                "{\n" +
                "     version     2.0;\n" +
                "     format      ascii;\n" +
                "     class       dictionary;\n" +
                "     location    \"constant\";\n" +
                "     object      turbulenceProperties;\n" +
                "}\n" +
                "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                "\n" +

                "simulationType LESModel;";


                File.WriteAllText(System.IO.Path.Combine(constantPath, "turbulenceProperties"), turbulenceProperties);
                #endregion

                #region transportProperties

                string transportProperties =
                    "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
                    "| =========                 |                                                 |\n" +
                    "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
                    "|  \\\\    /   O peration     | Version:  2.2.0                                 |\n" +
                    "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
                    "|    \\\\/     M anipulation  |                                                 |\n" +
                    "\\*---------------------------------------------------------------------------*/\n" +
                    "FoamFile\n" +
                    "{\n" +
                    "     version     2.0;\n" +
                    "     format      ascii;\n" +
                    "     class       dictionary;\n" +
                    "     object      transportProperties;\n" +
                    "}\n" +
                    "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
                    "\n" +

                    "transportModel	Newtonian;\n" +
                    "nu nu[0 2 - 1 0 0 0 0] 1.5e-05;";


                File.WriteAllText(System.IO.Path.Combine(constantPath, "transportProperties"), transportProperties);
                #endregion

                #region mesh.job

                string meshJob =
                    "#!/bin/sh\n\r" +

                    "# PBS -j oe\n" +
                    "# PBS -N Mesh Job\n" +
                    "# PBS -l nodes=1:ppn=6\n" +
                    "# PBS -l walltime=1000000:00:00\n" +
                    "###output files\n" +
                    "###PBS -e err.log\n" +
                    "###PBS -o out.log\n" +
                    "###Mail to user\n" +
                    "# PBS -m ae\n" +
                    "# PBS -M andersson@str-ucture.com\n\r" +

                    "cd $PBS_O_WORKDIR\n\r" +

                    "LOGFILE = \"${PBS_O_WORKDIR}/out.log\"\n" +
                    "ERRFILE = \"${PBS_O_WORKDIR}/err.log\"\n" +
                    "NODEFILE = \"${PBS_O_WORKDIR}/nodes.log\"\n\r" +

                    "cat $PBS_NODEFILE >$NODEFILE\n\r" +

                    "export NUM_CPUS = 6\n" +
                    "export NUM_NODES = 1\n\r" +

                    "#rm -r processor*\n\r" +


                    "#\n" +
                    "# deompose based on pimpleDyMFsiFoam\n" +
                    "#\n\r" +

                    "sed -i \"s/pimpleDyMFoam/pimpleDyMFsiFoam/g\" system/controlDict\n" +
                    "echo \"Decpompose Start\" > timeStamp\n" +
                    "date >> timeStamp\n" +
                    "decomposePar >>$LOGFILE 2 > &1\n" +
                    "echo \"Decpompose end\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "sed -i \"s/pimpleDyMFsiFoam/pimpleFoam/g\" system/controlDict\n\r" +

                    "#\n" +
                    "# now, run the mesh\n" +
                    "#\n\r" +

                    "echo \"mesh start\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "#impirun -machinefile $PBS_NODEFILE -genv I_MPI_DEBUG 4 -genv I_MPI_FABRICS shm:tcp -n $NUM_CPUS snappyHexMesh -parallel >>$LOGFILE 2>&1 && \\\n" +
                    "mpirun - hostfile $PBS_NODEFILE - n $NUM_CPUS snappyHexMesh -parallel >>$LOGFILE 2 > &1 && \\\n" +
                    "echo \"mesh end\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "echo \"reconstruct start\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "reconstructParMesh >>$LOGFILE 2 > &1 && \\\n" +
                    "echo \"reconstruct end\" >> timeStamp\n" +
                    "date >> timeStamp\n\r" +


                    "echo \" **** FOAM JOB ENDS *****\" >>$LOGFILE\n";

                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "mesh.job"), meshJob);
                #endregion

                #region foam.job 

                string foamJob =
                    "#!/bin/sh\n\r" +

                    "# PBS -j oe\n" +
                    "# PBS -N Solve Job\n" +
                    "# PBS -l nodes=3:ppn=6\n" +
                    "# PBS -l walltime=1000000:00:00\n" +
                    "###output files\n" +
                    "###PBS -e err.log\n" +
                    "###PBS -o out.log\n" +
                    "###Mail to user\n" +
                    "# PBS -m ae\n" +
                    "# PBS -M andersson@str-ucture.com\n\r" +

                    "cd $PBS_O_WORKDIR\n\r" +

                    "LOGFILE = \"${PBS_O_WORKDIR}/out.log\"\n" +
                    "ERRFILE = \"${PBS_O_WORKDIR}/err.log\"\n" +
                    "NODEFILE = \"${PBS_O_WORKDIR}/nodes.log\"\n\r" +

                    "cat $PBS_NODEFILE >$NODEFILE\n\r" +

                    "export NUM_CPUS = 18\n" +
                    "export NUM_NODES = 3\n\r" +

                    "#rm -r processor*\n\r" +

                    "# reconfigure decomposition method\n\r" +

                    "sed -i \"s/method ptscotch/method scotch/g\" system/decomposeParDict\n\r" +

                    "#\n" +
                    "# deompose based on pimpleDyMFsiFoam\n" +
                    "#\n\r" +

                    "sed -i \"s/pimpleDyMFoam/pimpleDyMFsiFoam/g\" system/controlDict\n" +
                    "echo \"Decpompose Start\" > timeStamp\n" +
                    "date >> timeStamp\n" +
                    "decomposePar >>$LOGFILE 2 > &1\n" +
                    "echo \"Decpompose end\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "sed -i \"s/pimpleDyMFsiFoam/pimpleFoam/g\" system/controlDict\n\r" +

                    "#\n" +
                    "# now, run the solver\n" +
                    "#\n\r" +

                    "echo \"solver start\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "#impirun -machinefile $PBS_NODEFILE -genv I_MPI_DEBUG 4 -genv I_MPI_FABRICS shm:tcp -n $NUM_CPUS pimpleFoam -parallel >>$LOGFILE 2>&1 && \\n" +
                    "mpirun -hostfile $PBS_NODEFILE - n $NUM_CPUS pimpleFoam -parallel >>$LOGFILE 2 > &1 && \\n" +
                    "echo \"solver end\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "echo \"reconstruct start\" >> timeStamp\n" +
                    "date >> timeStamp\n" +
                    "reconstructPar >>$LOGFILE 2 > &1 && \\n" +
                    "echo \"reconstruct end\" >> timeStamp\n" +
                    "date >> timeStamp\n\r" +


                    "echo \" **** FOAM JOB ENDS *****\" >>$LOGFILE\n";

                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "foam.job"), foamJob);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("75763d07-613b-4960-9963-5ee2ec3d30d1"); }
        }

        protected void ExportStl(Brep geom, string path)
        {
            List<Guid> guidList = new List<Guid>();
            Rhino.DocObjects.Tables.ObjectTable ot = Rhino.RhinoDoc.ActiveDoc.Objects;
                       
                if (geom == null || !geom.IsValid)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No object to bake, or brep not valid, aborted.");
                    return;
                }
                System.Guid guid = ot.AddBrep(geom);
                guidList.Add(guid);
            
            int nSelected = ot.Select(guidList);
            if (nSelected != guidList.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not all objects could be selected, aborted.");
                return;
            }

            string cmd = "-_Export " + "\"" + path + ".stl" + "\"" + " _Enter" + " _Enter";
            Rhino.RhinoApp.RunScript(cmd, false);

            ot.Delete(guidList, true);
        }
    }
}
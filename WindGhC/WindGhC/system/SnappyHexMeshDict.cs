using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace WindGhC
{
    public class SnappyHexMeshDict : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SnappyHexMeshDict()
          : base(
                "snappyHexMeshDict", 
                "sHMDict ",
                "Assemble the snappyHexMeshDict file.",
                "STR.Wind", 
                "preProcessing")
        {
        }


        /// Registers all the input parameters for this component.
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Plug in geometry component as a tree.", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Refinement Boxes", "R", "Plug in refinementBoxes component as a flattened list.", GH_ParamAccess.list);
            pManager.AddPointParameter("Location in mesh", "L", "Insert a point that is located within the mesh.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("no Cells between levels", "C", "Insert the number cell divisions you want to use between to refinement levels", GH_ParamAccess.item,2);

            pManager[1].Optional = true;
            
        }


        /// Registers all the output parameters for this component.
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("snappyHexMeshDict", "sHMDict", "snappyHexMeshDict", GH_ParamAccess.item);
        }


        /// This is the method that actually does the work.
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Structure<GH_Brep> iGeometry;
            List<Brep> iRefBoxes = new List<Brep>();
            Point3d iLocationInMesh = new Point3d();
            int iCellsBetweenLvls = 0;
            
            
            DA.GetDataTree(0, out iGeometry);
            DA.GetDataList(1, iRefBoxes);
            if (!DA.GetData(2, ref iLocationInMesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Specify a point in the mesh");
                return;
            }
            DA.GetData(3, ref iCellsBetweenLvls);


            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();

            int x = 0;
            Brep convertedBrep = null;
            foreach (GH_Path path in iGeometry.Paths)
            {
                foreach (var geom in iGeometry.get_Branch(path))
                {
                    GH_Convert.ToBrep(geom, ref convertedBrep, 0);
                    convertedGeomTree.Add(convertedBrep, new GH_Path(x));
                    convertedBrep = null;
                }
                x += 1;
            }

            string geomInsert = "";
            string geomRefInsert = "";
            foreach (GH_Path path in convertedGeomTree.Paths)
            {
                geomInsert += " " + convertedGeomTree.Branch(path)[0].GetUserString("Name") + ".stl\n" +
                    "   {\n" +
                    "       name " + convertedGeomTree.Branch(path)[0].GetUserString("Name") + ";\n" +
                    "       type triSurfaceMesh;\n" +
                    "   }\n";

                geomRefInsert += "  " + convertedGeomTree.Branch(path)[0].GetUserString("Name") + "\n" +
                    "   {\n" +
                    "       level\n" +
                    "       (\n" +
                    "         " + convertedGeomTree.Branch(path)[0].GetUserString("RefLvl") + "\n" +
                    "         " + convertedGeomTree.Branch(path)[0].GetUserString("RefLvl") + "\n" +
                    "       );\n" +
                    "       gapLevelIncrement 0;\n" +
                    "   }\n";
            }



            string refBoxInsert = "";
            string refBoxLvlInsert = "";
            List<string> refBoxNames = new List<string>();

            foreach (var box in iRefBoxes)
            {
                if (refBoxNames.Contains(box.GetUserString("Name")))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Some refinement boxes have the same name!");
                    return;
                }

                refBoxInsert += " " + box.GetUserString("Name") + "\n" +
                    "    {\n" +
                    "      type searchableBox;\n" +
                    "      min (" + box.GetUserString("MinCoord") + "); \n" +
                    "      max (" + box.GetUserString("MaxCoord") + "); \n" +
                    "    }\n";


                    refBoxLvlInsert += "        " + box.GetUserString("Name") + "\n" +
                    "           {\n" +
                    "               mode inside;\n" +
                    "               levels ((1E15 " + box.GetUserString("RefLvl") + "));\n" +
                    "           }\n";
                refBoxNames.Add(box.GetUserString("Name"));
            }

            string eMeshString = "";
            foreach (GH_Path path in convertedGeomTree.Paths)
            {
                eMeshString +=
                "       {\n" +
                "         file \"" + convertedGeomTree.Branch(path)[0].GetUserString("Name") + ".eMesh\";\n" +
                "         level 1;\n" +
                "       }\n\r";

            }
            string locationInMesh = "(" + iLocationInMesh.ToString().Replace(",", " ") + ");";

            #region shellString
            string shellString = 
              "/*--------------------------------*- C++ -*----------------------------------*\\\n" +
              "| =========                 |                                                 |\n" +
              "| \\\\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |\n" +
              "|  \\\\    /   O peration     |                                                 |\n" +
              "|   \\\\  /    A nd           | Web:      www.OpenFOAM.org                      |\n" +
              "|    \\\\/     M anipulation  |                                                 |\n" +
              "\\*---------------------------------------------------------------------------*/\n" +
              "FoamFile\n" +
              "{{\n" +
              "     version     2.0;\n" +
              "     format      ascii;\n" +
              "     class       dictionary;\n" +
              "     object      snappyHexMeshDict;\n" +
              "}}\n" +
              "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
              "\n" +

              "castellatedMesh true;\n\r" +
              "snap true;\n\r" +
              "addLayers false;\n\r" +

              "\n\r" +
              "//\n" +
              "// GEOMETRY\n" +
              "//\n\r" +

              "geometry\n" +
              "{{\n" +
              "{0}\n" +


              "{1}\n" +

              "}}\n\r" +

              "castellatedMeshControls\n" +
              "{{\n" +
              "  maxLocalCells 40000000;\n" +
              "  maxGlobalCells 70000000;\n" +
              "  minRefinementCells 0;\n" +
              "  maxLoadUnbalance 0.1;\n" +
              "  nCellsBetweenLevels {2};\n" +
              "  features\n" +
              "     {3}\n" +
              "     (\n\r" +

              "{4}\n" +    

              "     );\n\r" +


              "\n\r" +
              " //\n" +
              " // MESH REFINEMENT\n" +
              " //\n\r" +

              " refinementSurfaces\n" +
              " {{\n" +
              "{5}\n" +


              " }}\n\r" +

              " resolveFeatureAngle 30; \n\r" +

              " refinementRegions\n" +
              " {{\n" +
              "{6}" +
              " }}\n\r" +

              "\n\r" +
              " //\n" +
              " // MESH CONTROLS\n" +
              " //\n\r" +

              " locationInMesh\n" +
              " {7}\n\r" +

              " allowFreeStandingZoneFaces false;\n" +
              "}}\n\r" +

              "snapControls\n" +
              "{{\n" +
              "  nSmoothPatch 5;\n" +
              "  tolerance 010.0;\n" +
              "  nSolveIter 30;\n" +
              "  nRelaxIter 5;\n" +
              "  nFeatureSnapIter 10;\n" +
              "  implicitFeatureSnap true;\n" +
              "  explicitFeatureSnap true;\n" +
              "  multiRegionFeatureSnap true;\n" +
              "  globalFeatureEdges true;\n" +
              "}}\n\r" +

              "\n\r" +
              "//\n" +
              "// BOUNDARY LEVEL\n" +
              "//\n\r" +

              "addLayersControls\n" +
              "{{\n" +
              "  relativeSizes true;\n" +
              "  layers\n" +
              "  {{\n\r" +

              "  }}\n" +
              "  expansionRatio 1.0;\n" +
              "  finalLayerThickness 0.10;\n" +
              "  minThickness 0.05;\n" +
              "  nGrow 0;\n" +
              "  featureAngle 60;\n" +
              "  nRelaxIter 3;\n" +
              "  nSmoothSurfaceNormals 1;\n" +
              "  nSmoothNormals 3;\n" +
              "  nSmoothThickness 2;\n" +
              "  maxFaceThicknessRatio 0.5;\n" +
              "  maxThicknessToMedialRatio 0.3;\n" +
              "  minMedianAxisAngle 90;\n" +
              "  nBufferCellsNoExtrude 0;\n" +
              "  nLayerIter 30;\n" +
              "}}\n\r" +

              "\n\r" +
              "//\n" +
              "// MESH QUALITY\n" +
              "//\n\r" +

              "meshQualityControls\n" +
              "{{\n" +
              "  maxNonOrtho 70;\n" +
              "  maxBoundarySkewness 20;\n" +
              "  maxInternalSkewness 4;\n" +
              "  maxConcave 80;\n" +
              "  minFlatness 0.5;\n" +
              "  minVol 1e-13;\n" +
              "  minTetQuality -1;\n" +
              "  minArea -1;\n" +
              "  minTwist 0.02;\n" +
              "  minDeterminant 0.001;\n" +
              "  minFaceWeight 0.02;\n" +
              "  minVolRatio 0.01;\n" +
              "  minTriangleTwist -1;\n" +
              "  nSmoothScale 4;\n" +
              "  errorReduction 0.75;\n" +
              "}}\n\r" +

              "debug 0;\n\r" +

              "mergeTolerance 1e-07;";
            #endregion

            string snappyHexMeshDict = string.Format(shellString, geomInsert, refBoxInsert, iCellsBetweenLvls, convertedGeomTree.Branches.Count.ToString(), eMeshString, geomRefInsert, refBoxLvlInsert, locationInMesh);

            var oSnappyTextFile = new TextFile(snappyHexMeshDict, "snappyHexMeshDict");
                        
            DA.SetData(0, oSnappyTextFile);
        }


        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }


        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        public override Guid ComponentGuid
        {
            get { return new Guid("a629accb-01b4-470a-8f51-18f1fd55c087"); }
        }
    }
}

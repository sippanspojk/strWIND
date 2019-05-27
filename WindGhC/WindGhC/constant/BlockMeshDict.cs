using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC
{
    public class BlockMeshDict : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BlockMeshDict class.
        /// </summary>
        public BlockMeshDict()
          : base(
                "BlockMeshDict", 
                "blkMDict",
                "Assemble the blockMeshDict file.",
                "STR.Wind",
                "preProcessing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Input all geometry to generate the bounding block mesh.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("MeshSize", "M", "Specify the size of the block mesh [m].", GH_ParamAccess.item,10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("blockMeshDict", "blkMDict", "Assembled blockMeshDict file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Structure<IGH_GeometricGoo> iGeometry;
            int iMeshSize = 0;

            DA.GetDataTree(0, out iGeometry);
            DA.GetData(1, ref iMeshSize);
                        
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

            List<Point3d> vertexList = new List<Point3d>();
            string blockVertices = "";

            Point3d[] edgePoints;
            for(int i = 0; i < 6; i++)
            {
                foreach (var edge in convertedGeomTree.Branch(i)[0].Edges)
                {
                    edge.DivideByCount(Convert.ToInt32(edge.GetLength()), true, out edgePoints);
                    foreach (var point in edgePoints)
                        vertexList.Add(point);
                }
            }


            vertexList = vertexList.OrderBy(p => p.X).ToList();
            double xLength = Math.Abs(vertexList[0].X - vertexList[vertexList.Count - 1].X);
            double xMid = (vertexList[0].X + vertexList[vertexList.Count - 1].X) / 2;

            vertexList = vertexList.OrderBy(p => p.Y).ToList();
            double yLength = Math.Abs(vertexList[0].Y - vertexList[vertexList.Count - 1].Y);
            double yMid = (vertexList[0].Y + vertexList[vertexList.Count - 1].Y) / 2;

            vertexList = vertexList.OrderBy(p => p.Z).ToList();
            double zLength = Math.Abs(vertexList[0].Z - vertexList[vertexList.Count - 1].Z);
            double zMid = (vertexList[0].Z + vertexList[vertexList.Count - 1].Z) / 2;

            double xMin = xMid - Math.Ceiling(xLength / 2 / iMeshSize + 1) * iMeshSize;
            double xMax = xMid + Math.Ceiling(xLength / 2 / iMeshSize + 1) * iMeshSize;
            List<double> xValues = new List<double> { xMin, xMax };

            double yMin = yMid - Math.Ceiling(yLength / 2 / iMeshSize + 1) * iMeshSize;
            double yMax = yMid + Math.Ceiling(yLength / 2 / iMeshSize + 1) * iMeshSize;
            List<double> yValues = new List<double> { yMin, yMax };

            double zMin = zMid - Math.Ceiling(zLength / 2 / iMeshSize + 1) * iMeshSize;
            double zMax = zMid + Math.Ceiling(zLength / 2 / iMeshSize + 1) * iMeshSize;
            List<double> zValues = new List<double> { zMin, zMax };

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                        blockVertices += "    (" + xValues[k] + " " + yValues[j] + " " + zValues[i] + ")\n";

                    xValues.Reverse();
                }
            }

            int noBlocksX = Convert.ToInt32((xMax - xMin) / iMeshSize);
            int noBlocksY = Convert.ToInt32((yMax - yMin) / iMeshSize);
            int noBlocksZ = Convert.ToInt32((zMax - zMin) / iMeshSize);

            string noBlocks = noBlocksX + " " + noBlocksY + " " + noBlocksZ;

            #region shellstring
            string shellString =
              ("/*--------------------------------*- C++ -*----------------------------------*\\\n" +
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
              "     object      blockMeshDict;\n" +
              "}}\n" +
              "// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * //\n\r" +
              "\n" +

              "convertToMeters 1;\n" +
              "\n" +
              "vertices\n" +
              "(\n" + 
              "{0}" +
              ");\n" +
              "\nblocks\n" +
              "(\n" +
              "    hex (0 1 2 3 4 5 6 7) ({1}) simpleGrading (1 1 1)\n" +
              ");\n\r" +
              "edges\n" +
              "(\n" +
              ");\n\r" +
              "boundary\n" +
              "(\n" +
              ");\n\r" +
              "mergePatchPairs\n" +
              "(\n" +
              ");\n\r" +
              "// ************************************************************************* //");
            #endregion

            string blockMeshDict = string.Format(shellString, blockVertices, noBlocks);

            var oBlockMeshTextFile = new TextFile(blockMeshDict, "blockMeshDict");

            DA.SetData(0, oBlockMeshTextFile);                                  
            
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
            get { return new Guid("a85917b3-20ad-42f7-bc1c-8a74c69f82b6"); }
        }       

    }
}
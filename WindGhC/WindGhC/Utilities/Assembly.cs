using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
        /// 
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to write to.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Button", "B", "Write to file and generate folder structure.", GH_ParamAccess.item,false);
            pManager.AddBrepParameter("Geometry", "G", "Input the geometry.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("constant", "C", "Insert the the files that goes into the \"constant\" folder as a flat list", GH_ParamAccess.list);
            pManager.AddGenericParameter("system", "S", "Insert the the files that goes into the \"system\" folder as a flat list", GH_ParamAccess.list);
            pManager.AddGenericParameter("0", "0", "Insert the the files that goes into the \"0\" folder as a flat list", GH_ParamAccess.list);

            pManager[0].Optional = true;
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
            GH_Structure<GH_Brep> iGeometry;
            //GH_Structure<IGH_GeometricGoo> iGeometry = new GH_Structure<IGH_GeometricGoo>();
            var iConstantFolder = new List<TextFile>();
            var iSystemFolder = new List<TextFile>();
            var iZeroFolder = new List<TextFile>();

            DA.GetData(0, ref iPath);
            DA.GetData(1, ref iButton);
            DA.GetDataTree(2, out iGeometry);
            DA.GetDataList(3, iConstantFolder);
            DA.GetDataList(4, iSystemFolder);
            DA.GetDataList(5, iZeroFolder);

            /*DataTree<Brep> convertedGeomTree = new DataTree<Brep>();
            List<GH_Path> pathList = new List<GH_Path>();

            foreach (var path in iGeometry.Paths)
                for (int i = 0; i < iGeometry.get_Branch(path).Count; i++)
                    pathList.Add(path);

            var flattenedGeom = iGeometry.FlattenData();
            for (int i = 0; i < flattenedGeom.Count; i++)
            {
                flattenedGeom[i].CastTo(out Brep tempBrep);
                convertedGeomTree.Add(tempBrep, pathList[i]);
            }*/

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


            string fileLocation = "";
            string folderLocation = "";

            if (iPath != "")
                folderLocation = iPath;
            else
            {
                fileLocation = this.OnPingDocument().FilePath;
                if (fileLocation == null)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Either save the GH definition or manually assign a path to write to.");
                else
                    folderLocation = new DirectoryInfo(fileLocation).Parent.FullName + @"\";
            }

            if (iButton)
            {
                // Specify names for folders. add some more stuff

                string openFoamFolder = System.IO.Path.Combine(folderLocation, convertedGeomTree.Branch(0)[0].GetUserString("RotAngle") + "deg");

                string constantPath = System.IO.Path.Combine(openFoamFolder, "constant");
                string systemPath = System.IO.Path.Combine(openFoamFolder, "system");
                string zeroPath = System.IO.Path.Combine(openFoamFolder, "0");

                string polyMeshPath = System.IO.Path.Combine(constantPath, "polyMesh\\");
                string triSurfacePath = System.IO.Path.Combine(constantPath, "triSurface\\");

                // Generate directories
                Directory.CreateDirectory(zeroPath);
                Directory.CreateDirectory(polyMeshPath);
                Directory.CreateDirectory(triSurfacePath);
                Directory.CreateDirectory(systemPath);

                // Write text files
                foreach (var constantFile in iConstantFolder)
                    File.WriteAllText(System.IO.Path.Combine(polyMeshPath, constantFile.GetName()), constantFile.GetFileText());

                foreach (var systemFile in iSystemFolder)
                    File.WriteAllText(System.IO.Path.Combine(systemPath, systemFile.GetName()), systemFile.GetFileText());               

                foreach (var bcFile in iZeroFolder)
                    File.WriteAllText(System.IO.Path.Combine(zeroPath, bcFile.GetName()), bcFile.GetFileText());

                // Export .stl's
                foreach (GH_Path path in convertedGeomTree.Paths)                   
                    ExportStl(convertedGeomTree.Branch(path), triSurfacePath + convertedGeomTree.Branch(path)[0].GetUserString("Name"));

               

                // Write static text files.
                List<Brep> tempGeomList = new List<Brep>();
                foreach (GH_Path path in convertedGeomTree.Paths)
                    tempGeomList.Add(convertedGeomTree.Branch(path)[0]);

                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "foam.job"), StaticTextFiles.GetFoam());
                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "mesh.job"), StaticTextFiles.GetMesh());

                //File.WriteAllText(System.IO.Path.Combine(constantPath, "LESProperties"), StaticTextFiles.GetLESProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "RASProperties"), StaticTextFiles.GetRASProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "transportProperties"), StaticTextFiles.GetTransportProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "turbulenceProperties"), StaticTextFiles.GetTurbulenceProperties());

                File.WriteAllText(System.IO.Path.Combine(systemPath, "decomposeParDict"), StaticTextFiles.GetDecomposeParDict());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSolution"), StaticTextFiles.GetFVSolution());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSchemes"), StaticTextFiles.GetFVSchemes());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "surfaceFeatureExtractDict"), StaticTextFiles.GetSurfaceFeatureExtractDict(tempGeomList));
                File.WriteAllText(System.IO.Path.Combine(systemPath, "forces"), StaticTextFiles.GetForcesFunction(convertedGeomTree));

                File.WriteAllText(System.IO.Path.Combine(zeroPath, "p"), StaticTextFiles.GetP(tempGeomList));
                File.WriteAllText(System.IO.Path.Combine(zeroPath, "nut"), StaticTextFiles.GetNut(tempGeomList));
                File.WriteAllText(System.IO.Path.Combine(zeroPath, "nuTilda"), StaticTextFiles.GetNuTilda(tempGeomList));
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
        
        protected void ExportStl(List<Brep> geom, string path)
        {
            List<Guid> guidList = new List<Guid>();
            Rhino.DocObjects.Tables.ObjectTable ot = Rhino.RhinoDoc.ActiveDoc.Objects;

            foreach (var element in geom)
            {

                if (element == null || !element.IsValid)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No object to export, or brep not valid, aborted.");
                    return;
                }

                System.Guid guid = ot.AddBrep(element);
                guidList.Add(guid);
            }
            
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
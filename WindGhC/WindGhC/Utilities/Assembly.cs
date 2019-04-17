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
            pManager.AddBrepParameter("Geometry", "G", "Input the geometry.", GH_ParamAccess.list);
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
                // Specify names for folders

                string openFoamFolder = System.IO.Path.Combine(folderLocation, "OpenFoam_" + iGeometry[0].GetUserString("RotAngle") + "deg");

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
                foreach (var geom in iGeometry)                                       
                ExportStl(geom, triSurfacePath + geom.GetUserString("Name"));

                // Write static text files.
                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "foam.job"), StaticTextFiles.GetFoam());
                File.WriteAllText(System.IO.Path.Combine(openFoamFolder, "mesh.job"), StaticTextFiles.GetMesh());

                File.WriteAllText(System.IO.Path.Combine(constantPath, "LESProperties"), StaticTextFiles.GetLESProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "RASProperties"), StaticTextFiles.GetRASProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "transportProperties"), StaticTextFiles.GetTransportProperties());
                File.WriteAllText(System.IO.Path.Combine(constantPath, "turbulenceProperties"), StaticTextFiles.GetTurbulenceProperties());

                File.WriteAllText(System.IO.Path.Combine(systemPath, "decomposeParDict"), StaticTextFiles.GetDecomposeParDict());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSolution"), StaticTextFiles.GetFVSolution());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "fvSchemes"), StaticTextFiles.GetFVSchemes());
                File.WriteAllText(System.IO.Path.Combine(systemPath, "surfaceFeatureExtractDict"), StaticTextFiles.GetSurfaceFeatureExtractDict(iGeometry));
                File.WriteAllText(System.IO.Path.Combine(systemPath, "forces"), StaticTextFiles.GetForcesFunction(iGeometry));

                File.WriteAllText(System.IO.Path.Combine(zeroPath, "p"), StaticTextFiles.GetP(iGeometry));
                File.WriteAllText(System.IO.Path.Combine(zeroPath, "nuSgs"), StaticTextFiles.GetNuSgs(iGeometry));
                File.WriteAllText(System.IO.Path.Combine(zeroPath, "nuTilda"), StaticTextFiles.GetNuTilda(iGeometry));
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
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No object to export, or brep not valid, aborted.");
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
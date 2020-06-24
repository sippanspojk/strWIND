using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace WindGhC.Solving
{
    public class turbulentU : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the turbulentU class.
        /// </summary>
        public turbulentU()
          : base(
                "turbulentU",
                "tU",
                "Create files needed to generate turbulent wind input.",
                "STR.Wind",
                "Solving")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Domain", "D", "Plug in the domain component", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("N1exponent", "N1", "Specify exponent of the first dimension, N1", GH_ParamAccess.item);
            pManager.AddIntegerParameter("N2exponent", "N2", "Specify exponent of the second dimension, N2", GH_ParamAccess.item);
            pManager.AddIntegerParameter("N3exponent", "N3", "Specify exponent of the third dimension, N3", GH_ParamAccess.item);
            pManager.AddNumberParameter("time spacing", "d", "Specify the spacing of the wind lamellae.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("alpha", "a", "Specify exponent for power law", GH_ParamAccess.item, 0.16);
            pManager.AddNumberParameter("factor", "f", "Specify multiplication factor for power law", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("zRef", "zR", "Specify reference height", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("vRef", "vR", "Specify reference velocity", GH_ParamAccess.item, 25);
            pManager.AddNumberParameter("zRefSpec", "vRs", "Specify reference size of eddies", GH_ParamAccess.item);

            pManager[9].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "General info about the generated wind field.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Inlet", "I", "Inlet of the generated wind field", GH_ParamAccess.item);
            pManager.AddNumberParameter("z vector", "z", "z vector to be used for plotting", GH_ParamAccess.list);
            pManager.AddNumberParameter("wind profile", "U", "Normalized mean wind profile", GH_ParamAccess.list);
            pManager.AddNumberParameter("turbulence intensity", "I", "turbulence intensity", GH_ParamAccess.list);
            pManager.AddGenericParameter("3c3d_FOURN.f", "3c...", "File needed for the generation of the turbulent wind profile.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Structure<GH_Brep> iGeometry;
            int iN1exp = 0;
            int iN2exp = 0;
            int iN3exp = 0;
            double iTimeSpacing = 1;
            double iAlpha = 0.16;
            double iFactor = 1;
            double izRef = 10;
            double ivRef = 25;
            double iZRefBeonspek = 0;


            DA.GetDataTree(0, out iGeometry);
            DA.GetData(1, ref iN1exp);
            DA.GetData(2, ref iN2exp);
            DA.GetData(3, ref iN3exp);
            DA.GetData(4, ref iTimeSpacing);
            DA.GetData(5, ref iAlpha);
            DA.GetData(6, ref iFactor);
            DA.GetData(7, ref izRef);
            DA.GetData(8, ref ivRef);
            DA.GetData(9, ref iZRefBeonspek);

            DataTree<Brep> convertedGeomTree = new DataTree<Brep>();

            int newPath = 0;
            Brep convertedBrep = null;
            foreach (GH_Path path in iGeometry.Paths)
            {
                foreach (var geom in iGeometry.get_Branch(path))
                {
                    GH_Convert.ToBrep(geom, ref convertedBrep, 0);
                    convertedGeomTree.Add(convertedBrep, new GH_Path(newPath));
                    convertedBrep = null;
                }
                newPath += 1;
            }

            double N1 = Math.Pow(2, iN1exp);
            double N2 = Math.Pow(2, iN2exp);
            double N3 = Math.Pow(2, iN3exp);
            string Nstring = "N1=" + N1.ToString() + ",N2=" + N2.ToString() + ",N3=" + N3.ToString();




            List<Point3d> cornerPointList = new List<Point3d>();
            foreach (var cornerPoint in convertedGeomTree.Branch(0)[0].Vertices)
                cornerPointList.Add(new Point3d(cornerPoint.Location));

            double xMin = cornerPointList.OrderBy(p => p.X).ToList()[0].X;
            double yMin = cornerPointList.OrderBy(p => p.Y).ToList()[0].Y;
            double yMax = cornerPointList.OrderBy(p => p.Y).ToList()[cornerPointList.Count - 1].Y;
            double zMin = cornerPointList.OrderBy(p => p.Z).ToList()[0].Z;
            double zMax = cornerPointList.OrderBy(p => p.Z).ToList()[cornerPointList.Count - 1].Z;

            double N1out = N1;
            double N2out = Math.Ceiling((yMax - yMin) / iTimeSpacing * 1.2);
            double N3out = Math.Ceiling((zMax - zMin) / iTimeSpacing * 1.1);

            string inletCoordsString =
                "        xinlet = " + xMin + "\n" +
                "        yinlet = " + ((yMax + yMin) / 2 - N2out / 2 * iTimeSpacing) + "\n" +
                "        zinlet = " + zMin;

            var oInlet = Brep.CreateFromCornerPoints(new Point3d(xMin, (yMax + yMin) / 2 - N2out / 2 * iTimeSpacing, zMin),
                new Point3d(xMin, (yMax + yMin) / 2 - N2out / 2 * iTimeSpacing, zMin + N3out * iTimeSpacing),
                new Point3d(xMin, (yMax + yMin) / 2 + N2out / 2 * iTimeSpacing, zMin + N3out * iTimeSpacing),
                new Point3d(xMin, (yMax + yMin) / 2 + N2out / 2 * iTimeSpacing, zMin), 0.01);

            string NoutString = "N1out=" + N1out.ToString() + ",N2out=" + N2out.ToString() + ",N3out=" + N3out.ToString();

            double L1 = iTimeSpacing * N1;
            double L2 = iTimeSpacing * N2;
            double L3 = iTimeSpacing * N3;

            string Lstring =
                "        L1 = " + L1 + "\n" +
                "        L2 = " + L2 + "\n" +
                "        L3 = " + L3;



            List<Brep> flattedGeomTree = convertedGeomTree.AllData();
            flattedGeomTree.RemoveRange(0, 6);
            List<double> zCoordList = new List<double>();
            foreach (var part in flattedGeomTree)
                foreach (var vertex in part.Vertices)
                    zCoordList.Add(vertex.Location.Z);
            zCoordList.Sort();
            double geomHeigth = zCoordList[zCoordList.Count - 1] - zCoordList[0];


            if (!DA.GetData(9, ref iZRefBeonspek))
            {
                iZRefBeonspek = Math.Ceiling(1.5 * geomHeigth);
            }

            if (8 * 0.59 * iZRefBeonspek >= L1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The length of the generated wind field is too small, currently " + L1.ToString() + " and needs to be bigger than 8L = " + (8 * 0.59 * iZRefBeonspek).ToString());
            if (8 * 0.59 * iZRefBeonspek >= L2)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The width of the generated wind field is too small, currently " + L2.ToString() + " and needs to be bigger than 8L = " + (8 * 0.59 * iZRefBeonspek).ToString());
            if (8 * 0.59 * iZRefBeonspek >= L3)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The height of the generated wind field is too small, currently " + L3.ToString() + " and needs to be bigger than 8L = " + (8 * 0.59 * iZRefBeonspek).ToString());



            List<double> zList = new List<double>();

            int i = 0;
            while (iTimeSpacing * i < Math.Ceiling(geomHeigth))
            {
                i++;
                zList.Add(iTimeSpacing * i);
            }

            List<double> uList = new List<double>();
            foreach (var z in zList)
                uList.Add(iFactor * ivRef * Math.Pow(z / izRef, iAlpha));
            double uMean = uList.Average();

            List<double> turbIntensity = new List<double>();
            foreach (var z in zList)
                turbIntensity.Add(0.19 * Math.Pow(z / izRef, -iAlpha));



            #region shellString
            string shellString =
                "!---------------------------------------------------------------------\n" +
                "      PROGRAM wind FFT 3c3d\n" +
                "!     Program to generate multi correlated wind time series of the \n" +
                "!     3 components \n" +
                "!     using the spatial 3 dimensional FFT of Numerical Recipes \n" +
                "\n" +
                "!     Reference:\n" +
                "!     Jakob Mann: \"Wind field simulation\", 1998\n" +
                "\n" +
                "!     written by A. Michalski 2006/07\n" +
                "!     \n" +
                "!     AMI 12.02.2008 output file for OpenFOAM  \n" +
                "!     AMI 04.09.2008 only fourn parts active \n" +
                "!     AMI 14.12.2010 filter to detect maximum gust in defined box\n" +
                "!                    filter to detect gust with defined speed \n" +
                "!                    parameter iopt\n" +
                "!     AMI 16.12.2010 IFFT omp parallelized\n" +
                "!     AMI 12.01.2011 vtk_structured grid output file\n" +
                "!     AMI 22.02.2011 only output mean wind field \n" +
                "!                    parameter imeanwind\n" +
                "!     AMI 31.03.2011 Write OpenFOAM Wind files\n" +
                "!     AMI 06.04.2011 Filter to detect wind field with defined variance\n" +
                "!     DA  17.06.2019 Output adapted for OpenFOAM v.6\n" +
                "!---------------------------------------------------------------------\n" +
                "       use omp_lib\n" +
                "!     include 'omp_lib.h'\n" +
                "\n" +
                "!     Openmp parallel\n" +
                "      integer ithr,tnr \n" +
                "    \n" +
                "      integer*4 N1, N2, N3\n" +
                "      integer*4 NDAT,N1I8,N2I8,N3I8\n" +
                "      integer*4 N1out, N2out, N3out,N1vtk\n" +
                "!     PARAMETERS\n" +
                "       parameter(ndim=3)\n" +
                "       parameter({0})\n" +
                "       parameter(N1I8=N1,N2I8=N2,N3I8=N3)\n" +
                "       parameter(NDAT=2*N1I8*N2I8*N3I8)\n" +
                "       parameter({1})\n" +
                "       parameter(N1vtk=256)!     ARRAYS\n" +
                "!     - real\n" +
                "        real data1(NDAT)\n" +
                "        real data2(NDAT)\n" +
                "        real data3(NDAT)\n" +
                "      \n" +
                "        real matrix(N3out,N1out)\n" +
                "        \n" +
                "!     VARIABLES\n" +
                "!     - integer\n" +
                "        integer*8 il,nth\n" +
                "        integer*4 thp\n" +
                "        integer*4 ivalue, iflag, iout, iopt, imeanwind\n" +
                "        integer*8 n\n" +
                "        integer*4 nn(3)\n" +
                "        integer*4 j1,j2,j3\n" +
                "!xxxxxxxx used for debugging   xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx       \n" +
                "!       integer*4 i        \n" +
                "!xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx        \n" +
                "         \n" +
                "\n" +
                "!       for OpenFOAM\n" +
                "!     - string\n" +
                "        character(LEN=18) mkdirname\n" +
                "        character(LEN=14) cddirname\n" +
                "        character(LEN=18) mkdirname2\n" +
                "        character(LEN=14) cddirname2\n" +
                "        integer is,is2\n" +
                "!       real*8 ui\n" +
                "                \n" +
                "!     - real                                     \n" +
                "        real S1re, S1im, S2re, S2im, S3re, S3im\n" +
                "        real*8 sumen1, sumen2, sumen3\n" +
                "!xxxxxxxx used for debugging   xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx       \n" +
                "!        integer item,jtem,iktem\n" +
                "!        real tsimtem,data1tem\n" +
                "!xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n" +
                "\n" +
                "        real*8 L1, L2, L3\n" +
                "        real*8 d1,d2,d3 \n" +
                "        real*8 k1,k2,k3,k,ks,k1s,k2s,k3s,k0,k0s,k0ksfak,k30,k30s\n" +
                "\n" +
                "        real*8 z\n" +
                "        real*8 ustar\n" +
                "        real*8 E, En\n" +
                "      \n" +
                "        real F21re\n" +
                "        real*8 L\n" +
                "        real*8 fak\n" +
                "        real*8 Cfakt\n" +
                "        real fakhyp\n" +
                "        real*8 beta\n" +
                "        real*8 gam\n" +
                "        real*8 sita1, sita2, Ce1, Ce2\n" +
                "        real*8 atanfak, atanfak1\n" +
                "        \n" +
                "        real*8 xn1re, xn1im, xn2re, xn2im, xn3re, xn3im\n" +
                "        real*8 uref, alfa, zref, ubasic, aquer,bquer\n" +
                "        real*8 uz,umeanz,umzSBCi,umzECNA\n" +
                "        real fscale\n" +
                "      \n" +
                "        real*8 dt,tsim\n" +
                "        \n" +
                "!       real xlength1, xlength2, xlenght3\n" +
                "\n" +
                "!       gustbox\n" +
                "        real*8 ugust, ugustm, ugustms\n" +
                "        integer*4 n1b,n2b,n3b,n123b\n" +
                "        integer*4 j1a,j1e,j1am,j1em,j1ams,iter,maxiter\n" +
                "        integer*4 j1d\n" +
                "        integer*4 j2a,j2e,j3a,j3e\n" +
                "        real*8 B1,B2,B3,zgb\n" +
                "        real*8 lgs, ugs\n" +
                "    \n" +
                "        integer iterende\n" +
                "        integer checkl,usegust\n" +
                "        real*8 gustarray(N1)\n" +
                "        real*8 xlength,xinlet,yinlet,zinlet\n" +
                "        real*8 checkvar, checkvarlimit,dustar\n" +
                "\n" +
                "!     - complex             \n" +
                "        complex xn1, xn2, xn3\n" +
                "        complex xn2check(N1)\n" +
                "        complex dZiso1, dZiso2, dZiso3\n" +
                "        complex S1,S2,S3\n" +
                "        complex hypgeo\n" +
                "        complex ah, bh, ch, zh\n" +
                "        complex F21\n" +
                "\n" +
                "\n" +
                "!        Wind speed profile according to EC\n" +
                "!        Roughness length z0EC\n" +
                "         real*8 z0EC \n" +
                "!        Terrain factor krEC\n" +
                "         real*8 krEC\n" +
                "!        Roughness factor crzEC\n" +
                "         real*8  crzEC\n" +
                "!        Basic wind speed EC\n" +
                "         real*8 vbasicEC\n" +
                "!        Mean wind speed vmzEC\n" +
                "         real*8 umzEC\n" +
                "!        Standard deviation sdEC\n" +
                "         real*8 sdEC\n" +
                "!        Turbulence intensity IvEC\n" +
                "         real*8  IvEC\n" +
                "!        Peak velocity pressure\n" +
                "         real*8  qpzEC\n" +
                "!        Peak velocity\n" +
                "         real*8  vpzEC\n" +
                "            \n" +
                "!     COMMON BLOCKS\n" +
                "        common /dataar/ data1, data2, data3\n" +
                "        common /statist/ umean, vmean, wmean, uvar, vvar, wvar,uw,uv,vw \n" +
                "\n" +
                "!     OPEN FILES \n" +
                "        open(unit=58,file='log.dat')\n" +
                "        rewind(58)\n" +
                "        \n" +
                "        write(*,*)\n" +
                "        write(*,*) 'Software to generate multicorrelated wind time' \n" +
                "        write(*,*) 'histories of u,v,w wind components in x,y,z   '\n" +
                "        write(*,*) 'directions                                   '    \n" +
                "        write(*,*)\n" +
                "        write(*,*) 'Open files'\n" +
                "        write(*,*)\n" +
                "        \n" +
                "        write(58,*)\n" +
                "        write(58,*) 'Software to generate multicorrelated wind time' \n" +
                "        write(58,*) 'histories of u,v,w wind components in x,y,z   '\n" +
                "        write(58,*) 'directions                                   '    \n" +
                "        write(58,*)\n" +
                "        write(58,*) 'Open files'\n" +
                "        write(58,*)\n" +
                "        \n" +
                "        \n" +
                "               \n" +
                "!xxxxxxxx used for debugging   xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n" +
                "!       Temporre out files to check the direct access routines        \n" +
                "!        open(unit=8,file='u_out_temp.dat')\n" +
                "!        open(unit=9,file='v_out_temp.dat')\n" +
                "!        open(unit=10,file='w_out_temp.dat')       \n" +
                "\n" +
                "!     Temporary output files to check quality      \n" +
                "!         open(unit=14,file='checkdaccess.dat')\n" +
                "!         open(unit=20,file='bug.dat')\n" +
                "!          open(unit=21,file='input.dat')\n" +
                "!         open(unit=22,file='ifft.dat')       \n" +
                "!         rewind(8)\n" +
                "!         rewind(9)\n" +
                "!         rewind(10)\n" +
                "!         rewind(20)\n" +
                "!         rewind(21)\n" +
                "!         rewind(22)\n" +
                "!         rewind(14)\n" +
                "!xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n" +
                "      \n" +
                "!     FOURIER NORM \n" +
                "        nn(1) = N1\n" +
                "        nn(2) = N2\n" +
                "        nn(3) = N3\n" +
                "\n" +
                "       write(*,*) 'Read input values...'\n" +
                "       write(*,*)\n" +
                "       write(58,*) 'Read input values...'\n" +
                "       write(58,*)\n" +
                "             \n" +
                "!     INPUT VALUES\n" +
                "!     - Length of domain\n" +
                "{2}\n" +
                "        umeanz = {3}\n" +
                "!     - grid size       \n" +
                "        d1 = L1/N1\n" +
                "        d2 = L2/N2\n" +
                "        d3 = L3/N3\n" +
                "!     - gridpoints      \n" +
                "!       nth = N1 * N2 * N3\n" +
                "        nth = N1I8 * N2I8 * N3I8\n" +
                "!     - wavelenth interval\n" +
                "        dk1 = 6.2831853/ L1\n" +
                "        dk2 = 6.2831853/ L2\n" +
                "        dk3 = 6.2831853/ L3  \n" +
                "!     - spatial grid resolution  \n" +
                "        delta = d1*d2*d3         \n" +
                "!     - Mean wind profile\n" +
                "!       EC\n" +
                "         alfa  = {4}\n" +
                "         uref  = {5}d0*{6}\n" +
                "         zref  = {7}.d0\n" +
                "!      SBC\n" +
                "!        ubasic = 105.0\n" +
                "!        aquer  = 0.25\n" +
                "!        bquer  = 0.45\n" +
                "\n" +
                "!     - Coupling routine        \n" +
                "!       ivalue = 0: read fluctuations, ivalue=1: read mean + fluctuation        \n" +
                "        ivalue = 1\n" +
                "!       iflag = 0: linear interpolation in time, \n" +
                "!               1: bicubic spline interpolation in time        \n" +
                "        iflag  = 0 \n" +
                "\n" +
                "!     - OpenFOAM output files\n" +
                "!       iout =  1: OpenFOAM output additionally                  \n" +
                "        iout   = 1        \n" +
                "!       mean wind input file for OpenFOAM\n" +
                "!       imeanwind = 1 --> write meanwind file\n" +
                "        imeanwind = 0\n" +
                "{8}\n" +
                "\n" +
                "! MEANWIND inflow file  \n" +
                "        if (imeanwind.eq.1) then     \n" +
                "           write(*,*)\n" +
                "           write(*,*) 'Write MEANWIND inflow for OpenFOAM'\n" +
                "           write(*,*)\n" +
                "           write(58,*)\n" +
                "           write(58,*) 'Write MEANWIND inflow for OpenFOAM'\n" +
                "           write(58,*) \n" +
                "           open(unit=88,file='points',status='unknown')\n" +
                "           open(unit=99,file='U',status='unknown')\n" +
                "           call meanwind(N1,N2out,N3out,d1,d2,d3,nn,uref,zref,alfa, \n" +
                "     &                xinlet,yinlet,zinlet)\n" +
                "           write(*,*) '...DONE'\n" +
                "           write(*,*) \n" +
                "           write(58,*) '...DONE'\n" +
                "           write(58,*)\n" +
                "           close(88)\n" +
                "           close(99)\n" +
                "           goto 122\n" +
                "        endif\n" +
                "  \n" +
                "!     - Turbulence parameters \n" +
                "        ustar = 1.57d0\n" +
                "       \n" +
                "!     - Spectral energy fit to KAIMAL Spectrum (J. Mann)       \n" +
                "        z = {9}\n" +
                "        L = 0.59*z\n" +
                "        fak = 3.2 * (ustar**2.d0)/(z**0.6666)*(L**1.6666)\n" +
                "        fscale = 2.0\n" +
                "!        (scale total isotropic variance of wind field with fscaleý)\n" +
                "!     - Spectral tensor parameters (J.Mann 1998)\n" +
                "!     - Non-dimensional shear distortion parameter\n" +
                "!       gamma = 0 --> isotropic model       \n" +
                "        gam = 3.9\n\r";

            #endregion


            string turbFile = string.Format(shellString, Nstring, NoutString, Lstring, uMean.ToString(), iAlpha.ToString(), ivRef.ToString(), iFactor.ToString(), izRef.ToString(), inletCoordsString, iZRefBeonspek.ToString()) + "\n" + StaticTextFiles.Get3c3dFile();

            var o3c3dFile = new TextFile(turbFile, "3c3d_FOURN_structured.f");

            string oInfo = "Wind field duration = " + (Math.Ceiling(N1 * iTimeSpacing / uMean / 60 * 100) / 100).ToString() + "min";



            List<double> oUProfile = new List<double>();
            oUProfile = uList;

            List<double> oTurbIntensity = new List<double>();
            oTurbIntensity = turbIntensity;

            DA.SetData(0, oInfo);
            DA.SetData(1, oInlet);
            DA.SetDataList(2, zList);
            DA.SetDataList(3, oUProfile);
            DA.SetDataList(4, oTurbIntensity);
            DA.SetData(5, o3c3dFile);

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
                return Properties.Resources.turbulentWind;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("491df384-52f9-40f1-8861-7d87dadd9809"); }
        }
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }
    }
}
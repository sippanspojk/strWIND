﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WindGhC.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WindGhC.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap _2sofistik {
            get {
                object obj = ResourceManager.GetObject("_2sofistik", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !     CHECK DIMENSIONS OF THE BOX
        ///        write(*,*) &apos;Check dimensions of the Box&apos;
        ///        write(58,*) &apos;Check dimensions of the Box&apos;
        ///       
        ///        if (L2.lt.8*L) then 
        ///          write(*,*) &apos;Attention L2 lower than 8xL&apos;
        ///          write(58,*) &apos;Attention L2 lower than 8xL&apos;
        ///          stop
        ///        elseif (L3.lt.8*L) then
        ///          write(*,*) &apos;Attention L3 lower than 8xL&apos;
        ///          write(58,*) &apos;Attention L3 lower than 8xL&apos;
        ///          stop
        ///        else                     
        ///          write(*,*) &apos;dimen [rest of string was truncated]&quot;;.
        /// </summary>
        public static string _3c3d_FOURN_structured {
            get {
                return ResourceManager.GetString("_3c3d_FOURN_structured", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap assembly {
            get {
                object obj = ResourceManager.GetObject("assembly", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap blockMesh {
            get {
                object obj = ResourceManager.GetObject("blockMesh", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap constructFile {
            get {
                object obj = ResourceManager.GetObject("constructFile", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap controlDict {
            get {
                object obj = ResourceManager.GetObject("controlDict", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap cutPlanes {
            get {
                object obj = ResourceManager.GetObject("cutPlanes", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // -*- C++ -*-
        ///// File generated by PyFoam - sorry for the ugliness
        ///
        ///FoamFile
        ///{
        /// version 2.0;
        /// format ascii; 	
        /// class dictionary; 	
        /// object decomposeParDict; 	
        ///}
        ///
        ///numberOfSubdomains 30;
        ///
        ///method scotch; 	
        ///
        ///.
        /// </summary>
        public static string decomposeParDict {
            get {
                return ResourceManager.GetString("decomposeParDict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap deConstructFile {
            get {
                object obj = ResourceManager.GetObject("deConstructFile", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap domain {
            get {
                object obj = ResourceManager.GetObject("domain", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #!/bin/sh
        ///
        ///#PBS -j oe
        ///#PBS -N Solver1
        ///#PBS -l nodes=5:ppn=6
        ///#PBS -l walltime=1000000:00:00
        ///###output files
        ///###PBS -e err.log
        ///###PBS -o out.log
        ///###Mail to user
        ///#PBS -m ae
        ///#PBS -M andersson@str-ucture.com
        ///
        ///cd $PBS_O_WORKDIR
        ///
        ///LOGFILE=&quot;${PBS_O_WORKDIR}/out.log&quot;
        ///ERRFILE=&quot;${PBS_O_WORKDIR}/err.log&quot;
        ///NODEFILE=&quot;${PBS_O_WORKDIR}/nodes.log&quot;
        ///
        ///cat $PBS_NODEFILE &gt;$NODEFILE
        ///
        ///export NUM_CPUS=30
        ///export NUM_NODES=5
        ///
        ///#rm -r processor*
        ///
        ///# reconfigure decomposition method
        ///
        ///sed -i &quot;s/method ptscotch/me [rest of string was truncated]&quot;;.
        /// </summary>
        public static string foam {
            get {
                return ResourceManager.GetString("foam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Version:  6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string fvSchemes {
            get {
                return ResourceManager.GetString("fvSchemes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Version:  6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string fvSolution {
            get {
                return ResourceManager.GetString("fvSolution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap laminarWind {
            get {
                object obj = ResourceManager.GetObject("laminarWind", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Verson:   6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string LESProperties {
            get {
                return ResourceManager.GetString("LESProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #!/bin/sh
        ///
        ///#PBS -j oe
        ///#PBS -N Mesh
        ///#PBS -l nodes=5:ppn=6
        ///#PBS -l walltime=1000000:00:00
        ///###output files
        ///###PBS -e err.log
        ///###PBS -o out.log
        ///###Mail to user
        ///#PBS -m ae
        ///#PBS -M andersson@str-ucture.com
        ///
        ///cd $PBS_O_WORKDIR
        ///
        ///LOGFILE=&quot;${PBS_O_WORKDIR}/out.log&quot;
        ///ERRFILE=&quot;${PBS_O_WORKDIR}/err.log&quot;
        ///NODEFILE=&quot;${PBS_O_WORKDIR}/nodes.log&quot;
        ///
        ///cat $PBS_NODEFILE &gt;$NODEFILE
        ///
        ///export NUM_CPUS=30
        ///export NUM_NODES=5
        ///
        ///#rm -r processor*
        ///
        ///#
        ///# run blockMesh
        ///# 
        ///
        ///blockMesh
        ///
        ///#
        ///# deompose based on pimpleD [rest of string was truncated]&quot;;.
        /// </summary>
        public static string mesh {
            get {
                return ResourceManager.GetString("mesh", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Version:  6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string RASProperties {
            get {
                return ResourceManager.GetString("RASProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap refBox {
            get {
                object obj = ResourceManager.GetObject("refBox", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap snappyHexMesh {
            get {
                object obj = ResourceManager.GetObject("snappyHexMesh", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Version:  6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string transportProperties {
            get {
                return ResourceManager.GetString("transportProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*--------------------------------*- C++ -*----------------------------------*\
        ///| =========                 |                                                 |
        ///| \\      /  F ield         | OpenFOAM: The Open Source CFD Toolbox           |
        ///|  \\    /   O peration     | Website:  www.OpenFOAM.org                      |
        ///|   \\  /    A nd           | Version:  6                                     |
        ///|    \\/     M anipulation  |                                                 |
        ///\*------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string turbulenceProperties {
            get {
                return ResourceManager.GetString("turbulenceProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap turbulentWind {
            get {
                object obj = ResourceManager.GetObject("turbulentWind", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap U {
            get {
                object obj = ResourceManager.GetObject("U", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap windProbes {
            get {
                object obj = ResourceManager.GetObject("windProbes", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}

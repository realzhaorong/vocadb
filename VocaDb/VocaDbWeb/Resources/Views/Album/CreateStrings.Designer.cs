﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.33440
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ViewRes.Album {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class CreateStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CreateStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VocaDb.Web.Resources.Views.Album.CreateStrings", typeof(CreateStrings).Assembly);
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
        ///   Looks up a localized string similar to Please make sure the entry you&apos;re about to submit hasn&apos;t been added before. You need to enter at least one name and artist.&lt;br /&gt;&lt;br /&gt;
        ///Note that VocaDB is not UtaiteDB. The album needs to have at least one track with Vocaloid vocals to be allowed on VocaDB (other voice synthsizers such as UTAU are also allowed, but human-sung covers are not). Unofficial fan-made compilations (bootlegs) are also not allowed. For more information, see the Help/Guidelines..
        /// </summary>
        public static string AlbumInfo {
            get {
                return ResourceManager.GetString("AlbumInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Producers and vocalists associated with this album..
        /// </summary>
        public static string ArtistDesc {
            get {
                return ResourceManager.GetString("ArtistDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Artists (need at least 1).
        /// </summary>
        public static string ArtistsInfo {
            get {
                return ResourceManager.GetString("ArtistsInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disc type.
        /// </summary>
        public static string DiscType {
            get {
                return ResourceManager.GetString("DiscType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Need at least one artist..
        /// </summary>
        public static string NeedArtist {
            get {
                return ResourceManager.GetString("NeedArtist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Submit a new album.
        /// </summary>
        public static string SubmitAlbum {
            get {
                return ResourceManager.GetString("SubmitAlbum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create album. Correct the errors and try again..
        /// </summary>
        public static string UnableToCreateAlbum {
            get {
                return ResourceManager.GetString("UnableToCreateAlbum", resourceCulture);
            }
        }
    }
}

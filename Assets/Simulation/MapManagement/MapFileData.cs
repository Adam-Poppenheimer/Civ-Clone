using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Assets.Simulation.MapManagement {

    public class MapFileData {

        #region instance fields and properties

        public readonly SerializableMapData MapData;

        public readonly string FileName;

        public readonly DateTime LastModified;

        #endregion

        #region constructors

        public MapFileData(SerializableMapData mapData, string fileName, DateTime lastModified) {
            MapData      = mapData;
            FileName     = fileName;
            LastModified = lastModified;
        }

        #endregion

    }

}

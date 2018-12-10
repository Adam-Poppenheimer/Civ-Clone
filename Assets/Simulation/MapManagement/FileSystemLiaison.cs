using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class FileSystemLiaison : IFileSystemLiaison {

        #region static fields and properties

        private static List<Type> KnownSerializableTypes = new List<Type>() {
            typeof(Dictionary<string, int>),
            typeof(List<string>)
        };

        #endregion

        #region instance fields and properties

        public ReadOnlyCollection<MapFileData> SavedGames {
            get { return savedGames.AsReadOnly(); }
        }
        private List<MapFileData> savedGames = new List<MapFileData>();

        public ReadOnlyCollection<MapFileData> AvailableMaps {
            get { return availableMaps.AsReadOnly(); }
        }
        private List<MapFileData> availableMaps = new List<MapFileData>();

        private DirectoryInfo MapDirectory {
            get {
                if(_mapDirectory == null) {
                    _mapDirectory = Directory.CreateDirectory(
                        string.Format("{0}\\{1}", Application.persistentDataPath, MapPath)
                    );
                }
                return _mapDirectory;
            }
        }
        private DirectoryInfo _mapDirectory;




        private string MapPath;

        #endregion

        #region constructors

        [Inject]
        public FileSystemLiaison(
            [Inject(Id = "Map Path")] string mapPath
        ){
            MapPath = mapPath;
        }

        #endregion

        #region instance methods

        #region from IFileSystemLiaison

        public void WriteMapDataToFile(SerializableMapData mapData, string filename) {
            WriteMapToFile(mapData, string.Format(
                "{0}\\{1}\\{2}.xml",
                Application.persistentDataPath, MapPath, filename
            ));
        }

        public void DeleteMap(string filename) {
            var filesToDelete = MapDirectory.GetFiles(filename + ".xml");
            foreach(var file in filesToDelete) {
                file.Delete();
            }
        }

        public void RefreshMaps() {
            savedGames.Clear();

            foreach(var file in MapDirectory.GetFiles()) {
                if(file.Extension.Equals(".xml")) {
                    var savedGameOfFile = ReadMapFromFile(file);
                    savedGames.Add(new MapFileData(savedGameOfFile, file.Name, file.LastWriteTime));
                }
            }
        }

        #endregion

        private void WriteMapToFile(SerializableMapData map, string path) {
            using(var fileStream = new FileStream(path, FileMode.Create)) {
                using(var xmlWriter = XmlDictionaryWriter.CreateTextWriter(fileStream)) {

                    var serializer = new DataContractSerializer(typeof(SerializableMapData), KnownSerializableTypes);
                    serializer.WriteObject(fileStream, map);
                }
            }
        }

        private SerializableMapData ReadMapFromFile(FileInfo file) {
            using(var fileStream = file.OpenRead()) {
                using(var xmlReader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas())) {

                    var serializer = new DataContractSerializer(typeof(SerializableMapData), KnownSerializableTypes);
                    var mapInFile = (SerializableMapData)serializer.ReadObject(xmlReader, true);

                    return mapInFile;
                }
            }
        }

        #endregion

    }

}

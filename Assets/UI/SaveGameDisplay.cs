using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;

namespace Assets.UI {

    public class SaveGameDisplay : MonoBehaviour {

        #region internal types

        public enum MapSaveMode {
            AsSavedGame, AsMap
        }

        #endregion

        #region instance fields and properties

        [SerializeField] private RectTransform FileRecordContainer;
        [SerializeField] private InputField    NewFileNameInput;
        [SerializeField] private MapSaveMode   SaveMode;

        private List<GameMapFileRecord> InstantiatedFileRecords =
            new List<GameMapFileRecord>();



        private IMapComposer       MapComposer;
        private IFileSystemLiaison FileSystemLiaison;
        private GameMapFileRecord  FileRecordPrefab;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapComposer mapComposer, IFileSystemLiaison fileSystemLiaison,
            GameMapFileRecord fileRecordPrefab
        ){
            MapComposer       = mapComposer;
            FileSystemLiaison = fileSystemLiaison;
            FileRecordPrefab  = fileRecordPrefab;
        }

        #region Unity messages

        private void OnEnable() {
            IEnumerable<MapFileData> filesToDisplay;

            if(SaveMode == MapSaveMode.AsMap) {
                FileSystemLiaison.RefreshMaps();
                filesToDisplay = FileSystemLiaison.AvailableMaps;
            }else {
                FileSystemLiaison.RefreshSavedGames();
                filesToDisplay = FileSystemLiaison.SavedGames;
            }

            foreach(var savedGame in filesToDisplay) {
                var newRecord = Instantiate(FileRecordPrefab);

                newRecord.transform.SetParent(FileRecordContainer, false);
                newRecord.Refresh(savedGame);

                InstantiatedFileRecords.Add(newRecord);
            }
        }

        private void OnDisable() {
            foreach(var fileRecord in new List<GameMapFileRecord>(InstantiatedFileRecords)) {
                Destroy(fileRecord.gameObject);
            }
            InstantiatedFileRecords.Clear();
        }

        #endregion

        public void SaveMapToFile() {
            var fileName = NewFileNameInput.text;

            var mapToSave = MapComposer.ComposeRuntimeIntoData();

            if(SaveMode == MapSaveMode.AsMap) {
                FileSystemLiaison.WriteMapDataAsMapToFile(mapToSave, fileName);
                FileSystemLiaison.RefreshMaps();
            }else {
                FileSystemLiaison.WriteMapDataAsSavedGameToFile(mapToSave, fileName);
                FileSystemLiaison.RefreshSavedGames();
            }
        }

        #endregion

    }

}

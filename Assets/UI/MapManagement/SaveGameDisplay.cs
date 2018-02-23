using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;

namespace Assets.UI.MapManagement {

    public class SaveGameDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform     FileRecordContainer;
        [SerializeField] private InputField        NewFileNameInput;
        [SerializeField] private MapFileType       SaveMode;
        [SerializeField] private GameMapFileRecord FileRecordPrefab;

        private List<GameMapFileRecord> InstantiatedFileRecords =
            new List<GameMapFileRecord>();



        private IMapComposer       MapComposer;
        private IFileSystemLiaison FileSystemLiaison;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IMapComposer mapComposer, IFileSystemLiaison fileSystemLiaison){
            MapComposer       = mapComposer;
            FileSystemLiaison = fileSystemLiaison;
        }

        #region Unity messages

        private void OnEnable() {
            IEnumerable<MapFileData> filesToDisplay;

            if(SaveMode == MapFileType.Map) {
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

            if(SaveMode == MapFileType.Map) {
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

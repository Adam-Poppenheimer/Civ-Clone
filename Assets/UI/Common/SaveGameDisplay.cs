using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;

namespace Assets.UI.Common {

    public class SaveGameDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform     FileRecordContainer;
        [SerializeField] private InputField        NewFileNameInput;
        [SerializeField] private GameMapFileRecord FileRecordPrefab;
        [SerializeField] private ToggleGroup       SelectionGroup;
        [SerializeField] private RectTransform     OverwriteConfirmDisplay;

        private List<GameMapFileRecord> InstantiatedFileRecords =
            new List<GameMapFileRecord>();



        private IMapComposer       MapComposer;
        private IFileSystemLiaison FileSystemLiaison;
        private Animator           UIAnimator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapComposer mapComposer, IFileSystemLiaison fileSystemLiaison,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ){
            MapComposer       = mapComposer;
            FileSystemLiaison = fileSystemLiaison;
            UIAnimator        = uiAnimator;
        }

        #region Unity messages

        private void OnEnable() {
            OverwriteConfirmDisplay.gameObject.SetActive(false);

            IEnumerable<MapFileData> filesToDisplay;

            FileSystemLiaison.RefreshMaps();
            filesToDisplay = FileSystemLiaison.AvailableMaps;

            foreach(var savedGame in filesToDisplay) {
                var newRecord = Instantiate(FileRecordPrefab);

                newRecord.transform.SetParent(FileRecordContainer, false);
                newRecord.Refresh(savedGame);
                newRecord.BindSelectionToggle(SelectionGroup, OnMapDataSelected);

                InstantiatedFileRecords.Add(newRecord);
            }
        }

        private void OnDisable() {
            foreach(var fileRecord in new List<GameMapFileRecord>(InstantiatedFileRecords)) {
                Destroy(fileRecord.gameObject);
            }
            InstantiatedFileRecords.Clear();

            OverwriteConfirmDisplay.gameObject.SetActive(false);
        }

        #endregion

        public void SaveRequested() {
            var newMapName = NewFileNameInput.text;

            bool hasMapOfName = FileSystemLiaison.AvailableMaps.Any(
                map => Path.GetFileNameWithoutExtension(map.FileName).Equals(newMapName)
            );

            if(!hasMapOfName) {
                SaveMapToFile();
            }else {
                OverwriteConfirmDisplay.gameObject.SetActive(true);
            }
        }

        public void SaveMapToFile() {
            var fileName = NewFileNameInput.text;

            var mapToSave = MapComposer.ComposeRuntimeIntoData();

            FileSystemLiaison.WriteMapDataToFile(mapToSave, fileName);
            FileSystemLiaison.RefreshMaps();

            UIAnimator.SetTrigger("Return Requested");
        }

        private void OnMapDataSelected(MapFileData mapData) {
            NewFileNameInput.text = Path.GetFileNameWithoutExtension(mapData.FileName);
        }

        #endregion

    }

}

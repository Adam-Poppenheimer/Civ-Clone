using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapManagement;

namespace Assets.UI.MapManagement {

    public class LoadSavedGameDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform     FileRecordContainer;
        [SerializeField] private Text              FileNameField;
        [SerializeField] private ToggleGroup       SelectionGroup;
        [SerializeField] private MapFileType       LoadMode;
        [SerializeField] private GameMapFileRecord FileRecordPrefab;

        private List<GameMapFileRecord> InstantiatedFileRecords = 
            new List<GameMapFileRecord>();

        private MapFileData CurrentlySelectedFile;



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
            IEnumerable<MapFileData> filesToDisplay;

            if(LoadMode == MapFileType.Map) {
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
                newRecord.BindSelectionToggle(SelectionGroup, OnSelectedFileChanged);

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

        public void LoadSelectedFileIntoRuntime() {
            if(CurrentlySelectedFile != null) {
                MapComposer.DecomposeDataIntoRuntime(
                    CurrentlySelectedFile.MapData,
                    () => UIAnimator.SetTrigger("Play Mode Requested")
                );
            }
        }

        private void OnSelectedFileChanged(MapFileData fileData) {
            CurrentlySelectedFile = fileData;

            FileNameField.text = CurrentlySelectedFile.FileName;
        }

        #endregion

    }

}

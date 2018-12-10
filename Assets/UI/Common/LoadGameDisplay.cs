using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapManagement;

namespace Assets.UI.Common {

    public class LoadGameDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform     FileRecordContainer;
        [SerializeField] private Text              FileNameField;
        [SerializeField] private ToggleGroup       SelectionGroup;
        [SerializeField] private GameMapFileRecord FileRecordPrefab;
        [SerializeField] private Text              TitleText;
        [SerializeField] private Text              LoadButtonText;

        public string TitleLabel {
            get { return TitleText.text; }
            set { TitleText.text = value; }
        }

        public string AcceptButtonLabel {
            get { return LoadButtonText.text; }
            set { LoadButtonText.text = value; }
        }

        public Action LoadAction { get; set; }

        private List<GameMapFileRecord> InstantiatedFileRecords = 
            new List<GameMapFileRecord>();

        private MapFileData CurrentlySelectedFile;



        private IMapComposer       MapComposer;
        private IFileSystemLiaison FileSystemLiaison;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapComposer mapComposer, IFileSystemLiaison fileSystemLiaison
        ){
            MapComposer       = mapComposer;
            FileSystemLiaison = fileSystemLiaison;
        }

        #region Unity messages

        private void OnEnable() {
            IEnumerable<MapFileData> filesToDisplay;

            FileSystemLiaison.RefreshMaps();
            filesToDisplay = FileSystemLiaison.SavedGames;

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
                    CurrentlySelectedFile.MapData, LoadAction
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

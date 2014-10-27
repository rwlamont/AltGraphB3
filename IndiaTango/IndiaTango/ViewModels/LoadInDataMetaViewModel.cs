using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Caliburn.Micro;
using IndiaTango.Models;
using System.IO;

namespace IndiaTango.ViewModels
{
    class LoadInDataMetaViewModel : BaseViewModel
    {
         private IWindowManager _windowManager;
        private SimpleContainer _container;
        private String _metaFile;
        private String _dataFile;
        private String _lastPath;
        private String _fullMetaPath;
        private String _fullDataPath;
        private bool _success; 
        public LoadInDataMetaViewModel(IWindowManager windowManager, SimpleContainer container)
        {
            _windowManager = windowManager;
            _container = container;
            _dataFile = "Please load a data file";
            _metaFile = "Please load a meta file";
            _lastPath = Environment.SpecialFolder.MyDocuments.ToString();
        }

        #region View Properties
        public string Title
        {
            get { return "Files To Import"; }
        }

        public bool Success
        {
            get{ return _success;}
        }


        public string DataFile
        {
            get { return _dataFile; }
            set { _dataFile = value; NotifyOfPropertyChange(() => DataFile); }
        }

        public string MetaPath { get { return _fullMetaPath; } }
        public string DataPath { get { return _fullDataPath; } }
        public string MetaFile
        {
            get { return _metaFile; }
            set { _metaFile = value; NotifyOfPropertyChange(() => MetaFile); }
        }
        public string IntroTitle
        {
            get { return "Load Site in from Data and Meta"; }
            
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Button to import the files
        /// </summary>
        public void btnImport()
        {
            //Checks to see if both a meta file and a data file have been selected
            if(!String.IsNullOrWhiteSpace(MetaPath) && !String.IsNullOrWhiteSpace(DataPath))
            {
                _success = true;
                this.TryClose();
            }
            else
            {
                Common.ShowMessageBox("Form not completed", "You have not seleceted both a meta file and a data file, please try again", true,false);
            }
            
        }

        /// <summary>
        /// Loads a new data file
        /// </summary>
        public void btnData()
        {
            var dialog = new OpenFileDialog{ Filter = @"All B3 Data Files|*.csv;*.txt;*.gln|CSV Files|*.csv|TSV Files|*.txt|GLEON files|*.gln" };
            dialog.Title = "Open the data file";
            dialog.InitialDirectory = _lastPath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _fullDataPath = dialog.FileName.ToString();
                DataFile = Path.GetFileName(dialog.FileName);
                _lastPath = Path.GetDirectoryName(dialog.FileName);
            }
        }

        /// <summary>
        /// loads a new meta file
        /// </summary>
        public void btnMeta()
        {
            var dialog = new OpenFileDialog { Filter = @"All B3 Meta Files|*.txt;|Gleon Meta Files|*.txt"};
            dialog.Title = "Open the meta file";
            dialog.InitialDirectory = _lastPath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _fullMetaPath = dialog.FileName.ToString();
                MetaFile = Path.GetFileName(dialog.FileName);
                _lastPath = Path.GetDirectoryName(dialog.FileName);
            }
        }

        public void btnCancel()
        {
            this.TryClose();
        }

        #endregion


        
    }
}

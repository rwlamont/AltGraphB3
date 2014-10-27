using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Caliburn.Micro;
using IndiaTango.Models;

namespace IndiaTango.ViewModels
{
    class ExportViewModel : BaseViewModel
    {
        private IWindowManager _windowManager;
        private SimpleContainer _container;
        private Dataset _dataset;
        private bool _includeEmptyLines = true;
        private bool _includeMetaData = false;
        private bool _includeChangeLog = false;
        private bool _exportRaw = false;
        private DateColumnFormat _dateColumnFormat;
        private ExportedPoints _exportedPoints;
        private string _exportFormat;

        public ExportViewModel(IWindowManager windowManager, SimpleContainer container)
        {
            _windowManager = windowManager;
            _container = container;

            _dateColumnFormat = DateColumnFormat.TwoDateColumn;
            _exportedPoints = ExportedPoints.AllPoints;
            _exportFormat = "Tab Seperated Values, .txt";
        }

        #region View Properties
        public string Title
        {
            get { return string.Format("[{0}] Export Options", (Dataset != null ? Dataset.IdentifiableName : Common.UnknownSite)); }
        }

        public string IntroTitle
        {
            get { return "Choose which data to export"; }
        }

        public List<DateColumnFormat> DateColumnFormatOptions
        {
            get { return new List<DateColumnFormat>(new[] { DateColumnFormat.TwoDateColumn, DateColumnFormat.OneDateColumn }); }
        }

        public List<string> ExportFormatOptions
        {
            get { return new List<string>(new[] { "GLEON File Format, .gln", "Tab Seperated Values, .txt" }); }
        }

        public List<ExportedPoints> ExportedPointsOptions
        {
            get
            {
                return new List<ExportedPoints>(new[] { ExportedPoints.AllPoints,ExportedPoints.HourlyPoints,
                    ExportedPoints.DailyPoints,ExportedPoints.WeeklyPoints });
            }
        }

        public DateColumnFormat DateColumnFormat
        {
            get { return _dateColumnFormat; }
            set { _dateColumnFormat = value; NotifyOfPropertyChange(() => DateColumnFormat); }
        }

        public ExportedPoints ExportedPoints
        {
            get { return _exportedPoints; }
            set { _exportedPoints = value; NotifyOfPropertyChange(() => ExportedPoints); }
        }

        public bool IncludeEmptyLines
        {
            get { return _includeEmptyLines; }
            set { _includeEmptyLines = value; NotifyOfPropertyChange(() => IncludeEmptyLines); }
        }

        public bool IncludeMetaData
        {
            get { return _includeMetaData; }
            set { _includeMetaData = value; NotifyOfPropertyChange(() => IncludeMetaData); }
        }

        public bool IncludeChangeLog
        {
            get { return _includeChangeLog; }
            set { _includeChangeLog = value; NotifyOfPropertyChange(() => IncludeChangeLog); }
        }

        public bool ExportRawData
        {
            get { return _exportRaw; }
            set { _exportRaw = value; NotifyOfPropertyChange(() => ExportRawData); }
        }

        public Dataset Dataset
        {
            get { return _dataset; }
            set { _dataset = value; }
        }

        public string ExportFor
        {
            get {return _exportFormat;}
            set {_exportFormat = value;}
        }

        #endregion

        #region Event Handlers
        public void btnExport()
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Common.UserExportRoot;
            dialog.Description = "Set the folder to export to";
                        
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string startDate = Dataset.StartTimeStamp.Year.ToString() + Dataset.StartTimeStamp.Month.ToString();
                string endDate = Dataset.EndTimeStamp.Year.ToString() + Dataset.EndTimeStamp.Month.ToString();
                if (endDate.Length == 5)
                {
                    endDate = Dataset.EndTimeStamp.Year.ToString() + "0" + Dataset.EndTimeStamp.Month.ToString();
                }
                if (startDate.Length == 5)
                {
                    startDate = Dataset.StartTimeStamp.Year.ToString() + "0" + Dataset.StartTimeStamp.Month.ToString();
                }
                
                var countryCode = CountriesHelper.GetCode(Dataset.Site.CountryName);
                var filePath = dialog.SelectedPath + "\\" + Dataset.Site.Name + countryCode + startDate + "_" + endDate + ExportFor.Substring(ExportFor.Length - Math.Min(4,ExportFor.Length));
                try
                {
                    DatasetExporter.Export(Dataset, filePath, ExportFormat.CSV, IncludeEmptyLines,
                                           IncludeMetaData, IncludeChangeLog, ExportedPoints, DateColumnFormat,
                                           ExportRawData, true, true);
                }catch(Exception e)
                {
                    Common.ShowMessageBoxWithException("Failed to Export", "Sorry something went wrong with exporting",
                                                       false, true, e);
                }
                this.TryClose();
            }
        }

        public void btnCancel()
        {
            this.TryClose();
        }

        #endregion
    }


}

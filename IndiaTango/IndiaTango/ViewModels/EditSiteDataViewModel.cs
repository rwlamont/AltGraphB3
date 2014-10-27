using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Caliburn.Micro;
using IndiaTango.Models;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;

namespace IndiaTango.ViewModels
{
    public class EditSiteDataViewModel : BaseViewModel
    {
        public EditSiteDataViewModel(IWindowManager windowManager, SimpleContainer container)
        {
            _windowManager = windowManager;
            _container = container;

            AllContacts = Contact.ImportAll();
            AllCountries = CountriesHelper.CountriesNames();
            //Hack used to force the damn buttons to update
            DoneCancelVisible = Visibility.Visible;
            DoneCancelVisible = Visibility.Collapsed;
            DoneCancelEnabled = true;
            _cantSave = false;
            AllContacts.CollectionChanged += (o, e) =>
                                                 {
                                                     if (e.Action != NotifyCollectionChangedAction.Add || _contactTypeToUpdate <= -1)
                                                         return;

                                                     if (_contactTypeToUpdate == 0)
                                                     {
                                                         PrimaryContact = e.NewItems[0] as Contact;
                                                     }
                                                     else if (_contactTypeToUpdate == 1)
                                                     {
                                                         SecondaryContact = e.NewItems[0] as Contact;
                                                     }
                                                 };
        }

        #region Private Parameters

        private readonly IWindowManager _windowManager;
        private readonly SimpleContainer _container;
        private Dataset _dataSet;
        private bool _isNewSite;
        private bool _siteControlsEnabled;
        private ObservableCollection<Contact> _allContacts;
        private ObservableCollection<string> _allCountries;
        private bool _hasSelectedPrimaryContact;
        private bool _hasSelectedSecondaryContact;
        private bool _hasSelectedUniversityContact;
        private bool _doneCancelEnabled;
        private Visibility _doneCancelVisible;
        private Visibility _createEditDeleteVisible;
        private int _selectedImage;
        private int _contactTypeToUpdate = -1;
        private bool _cantSave;

        #region Site Details

        private string _siteName;
        private string _countryName;
        private string _owner;
        private string _notes;
        private string _elevation;
        private Contact _primaryContact;
        private Contact _secondaryContact;
        private string _latitude;
        private string _longitude;
        private List<NamedBitmap> _siteImages;
        private string _gridsystem;

        #endregion

        #endregion

        #region Public Parameters

        public bool WasCompleted;

        #endregion

        #region Private Properties

        private List<NamedBitmap> SiteImages
        {
            get { return _siteImages; }
            set
            {
                _siteImages = value;
                NotifyOfPropertyChange(() => SiteImages);
                NotifyOfPropertyChange(() => StackPanelSiteImages);
            }
        }

        #endregion

        #region Public Properties

        public Dataset DataSet
        {
            get { return _dataSet; }
            set
            {
                _dataSet = value;

                if (DataSet.Site != null)
                {
                    SiteName = DataSet.Site.Name;
                    Owner = DataSet.Site.Owner;
                    Notes = DataSet.Site.SiteNotes;
                    PrimaryContact = DataSet.Site.PrimaryContact;
                    CountryCode = DataSet.Site.CountryName;
                    Elevation = DataSet.Site.Elevation.ToString();
                    if (DataSet.Site.GpsLocation != null)
                    {
                        Latitude = DataSet.Site.GpsLocation.DecimalDegreesLatitude.ToString();
                        Longitude = DataSet.Site.GpsLocation.DecimalDegreesLongitude.ToString();
                        GridSystem = DataSet.Site.GpsLocation.GridSystem;
                    }
                    else
                    {
                        Latitude = "0";
                        Longitude = "0";
                        GridSystem = "WGS84";
                    }

                    SiteImages = DataSet.Site.Images ?? new List<NamedBitmap>();
                }
                else
                {
                    
                    SiteName = "";
                    Owner = "";
                    CountryCode = "";
                    Notes = "";
                    Elevation = "";
                    Latitude = "0";
                    Longitude = "0";
                    GridSystem = "WGS84";
                    PrimaryContact = null;
                    SiteImages = new List<NamedBitmap>();
                }
            }
        }

        public bool IsNewSite
        {
            get { return _isNewSite; }
            set
            {
                _isNewSite = value;
                if (IsNewSite != true)
                    return;
                BtnSiteEdit();
            }
        }

        public string Title
        {
            get { return string.Format("Site: {0}", SiteName); }
        }

        public bool SiteControlsEnabled
        {
            get { return _siteControlsEnabled; }
            set
            {
                _siteControlsEnabled = value;
                NotifyOfPropertyChange(() => SiteControlsEnabled);
            }
        }

        public ObservableCollection<Contact> AllContacts
        {
            get { return _allContacts; }
            set { _allContacts = value; NotifyOfPropertyChange(() => AllContacts); }
        }
        public ObservableCollection<string> AllCountries
        {
            get { return _allCountries; }
            set { _allCountries = value; NotifyOfPropertyChange(() => AllContacts); }
        }

        public bool HasSelectedPrimaryContact
        {
            get { return _hasSelectedPrimaryContact; }
            set
            {
                _hasSelectedPrimaryContact = value;
                NotifyOfPropertyChange(() => HasSelectedPrimaryContact);
            }
        }

        public bool HasSelectedSecondaryContact
        {
            get { return _hasSelectedSecondaryContact; }
            set
            {
                _hasSelectedSecondaryContact = value;
                NotifyOfPropertyChange(() => HasSelectedSecondaryContact);
            }
        }

        public bool HasSelectedUniversityContact
        {
            get { return _hasSelectedUniversityContact; }
            set
            {
                _hasSelectedUniversityContact = value;
                NotifyOfPropertyChange(() => HasSelectedUniversityContact);
            }
        }


        public bool DoneCancelEnabled
        {
            get { return _doneCancelEnabled; }

            set
            {
                _doneCancelEnabled = value;
                NotifyOfPropertyChange(() => DoneCancelEnabled);
            }
        }

        public Visibility DoneCancelVisible
        {
            get { return _doneCancelVisible; }
            set
            {
                _doneCancelVisible = value;
                NotifyOfPropertyChange(() => DoneCancelVisible);
                NotifyOfPropertyChange(() => SiteListEnabled);
                NotifyOfPropertyChange(() => CanDragDropImages);
            }
        }

        public bool EditDeleteEnabled
        {
            get { return true; }
        }

        public Visibility EditDeleteVisible
        {
            get { return Visibility.Visible; }
        }

        public Visibility CreateEditDeleteVisible
        {
            get { return _createEditDeleteVisible; }
            set
            {

                _createEditDeleteVisible = value;
                NotifyOfPropertyChange(() => CreateEditDeleteVisible);
            }
        }

        public bool SiteListEnabled
        {
            get { return DoneCancelVisible != Visibility.Visible; }
        }

        public bool CanDragDropImages
        {
            get { return DoneCancelVisible == Visibility.Visible; }
        }

        public int SelectedImage
        {
            get { return _selectedImage; }
            set { _selectedImage = value; NotifyOfPropertyChange(() => SelectedImage); }
        }

        #region Site Details

        public string SiteName
        {
            get { return _siteName; }
            set
            {
                _siteName = value;
                    NotifyOfPropertyChange(() => SiteName);
  
            }
        }
        /// <summary>
        /// CHecks to see wether the site name is unique, returns true if it is or false if it is not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool siteNameIsUnique(string name)
        {
            int i = 0;
            if (name == "New Site" || name == DataSet.Site.Name)
            {
                return true;
            }
            string[] siteNames = Dataset.GetAllDataSetFileNames();
            if (siteNames == null)
                return true;
            var siteNamesList = siteNames.Select(x => x.Substring(x.LastIndexOf('\\') + 1, x.Length - x.LastIndexOf('\\') - 4)).ToList();
            foreach (string s in siteNamesList)
            {
                string j = s.Substring(5);
                if (j == name)
                    i++;
            }

            if (i < 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public string Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                NotifyOfPropertyChange(() => Owner);
            }
        }

        public string CountryCode
        {
            get { return _countryName; }
            set
            {
                _countryName = value;
                NotifyOfPropertyChange(() => CountryCode);
            }
        }

        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                NotifyOfPropertyChange(() => Notes);
            }
        }

        public string Elevation
        {
            get { return _elevation; }
            set
            {
                _elevation = value;
                NotifyOfPropertyChange(() => Elevation);
            }
        }

        public string GridSystem
        {
            get { return _gridsystem; }
            set
            {
                _gridsystem = value;
                NotifyOfPropertyChange(() => GridSystem);
            }
        }



        public Contact PrimaryContact
        {
            get { return _primaryContact; }
            set
            {
                _primaryContact = value;
                NotifyOfPropertyChange(() => PrimaryContact);

                if (PrimaryContact != null)
                    HasSelectedPrimaryContact = true;
            }
        }

        public Contact SecondaryContact
        {
            get { return _secondaryContact; }
            set
            {
                _secondaryContact = value;
                NotifyOfPropertyChange(() => SecondaryContact);

                if (SecondaryContact != null)
                    HasSelectedSecondaryContact = true;
            }
        }

        public string Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                NotifyOfPropertyChange(() => Latitude);
            }
        }

        public string Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                NotifyOfPropertyChange(() => Longitude);
            }
        }

        public List<StackPanel> StackPanelSiteImages
        {
            get
            {
                var list = new List<StackPanel>();

                if (_siteImages != null)
                {
                    list.AddRange(_siteImages.Select(MakeImageItem));
                }

                return list;
            }
        }

        public ObservableCollection<string> Owners
        {
            get { return OwnerHelper.Owners; }
        }

        #endregion

        #endregion

        #region Private Methods

        private static StackPanel MakeImageItem(NamedBitmap bitmap)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            var image = new Image { Source = Common.BitmapToImageSource(bitmap.Bitmap), Height = 50, Width = 50 };
            image.MouseLeftButtonUp += delegate
            {
                var path = Path.Combine(Common.TempDataPath, bitmap.Name);
                if (!File.Exists(path))
                    bitmap.Bitmap.Save(path);
                System.Diagnostics.Process.Start(path);
            };
            image.Cursor = Cursors.Hand;
            image.Margin = new Thickness(3);
            var text = new TextBlock
                           {
                               Text = bitmap.Name,
                               Margin = new Thickness(5, 0, 0, 0),
                               VerticalAlignment = VerticalAlignment.Center
                           };
            panel.Children.Add(image);
            panel.Children.Add(text);

            return panel;
        }

        private void InsertImagesForSite(IEnumerable<string> fileNames)
        {
            if (_siteImages == null)
                _siteImages = new List<NamedBitmap>();

            foreach (var img in from fileName in fileNames where File.Exists(fileName) select new NamedBitmap(new Bitmap(fileName), Path.GetFileName(fileName)))
            {
                _siteImages.Add(img);
            }

            NotifyOfPropertyChange(() => SiteImages);
            NotifyOfPropertyChange(() => StackPanelSiteImages);
        }

        private void NewContact()
        {
            var editor =
                _container.GetInstance(typeof(ContactEditorViewModel), "ContactEditorViewModel") as
                ContactEditorViewModel;

            if (editor == null) return;

            editor.Contact = null;
            editor.AllContacts = AllContacts;

            _windowManager.ShowDialog(editor);
        }

        private void DeleteContact(Contact c)
        {
            if (!Common.Confirm("Confirm Delete", "Are you sure you want to delete this contact?")) return;

            if (c == null) return;
            var allContacts = AllContacts;
            allContacts.Remove(c);

            AllContacts = allContacts;

            Contact.ExportAll(AllContacts);

            Common.ShowMessageBox("Success", "Contact successfully removed.", false, false);
        }

        private void EditContact(Contact c)
        {
            var editor =
                _container.GetInstance(typeof(ContactEditorViewModel), "ContactEditorViewModel") as
                ContactEditorViewModel;

            if (editor == null) return;

            editor.Contact = c;
            editor.AllContacts = AllContacts;

            _windowManager.ShowDialog(editor);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Event Handlers

        #region Primary Contact

        public void BtnNewPrimary()
        {
            _contactTypeToUpdate = 0;
            NewContact();
            _contactTypeToUpdate = -1;
        }

        public void BtnEditPrimary()
        {
            EditContact(PrimaryContact);
        }

        public void BtnDelPrimary()
        {
            DeleteContact(PrimaryContact);
        }

        #endregion

        //#endregion

        #region Images

        public void BtnNewImage()
        {
            var dlg = new OpenFileDialog
                          {
                              Filter =
                                  @"Image Files|*.jpeg;*.jpg;*.png;*.gif;*.bmp|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|GIF Files (*.gif)|*.gif|Bitmap Files|*.bmp",
                              Title = @"Select images to add",
                              Multiselect = true
                          };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                InsertImagesForSite(dlg.FileNames);
            }
        }

        public void BtnDeleteImage()
        {
            if (SelectedImage == -1 || !Common.ShowMessageBox("Confirm Delete", "Are you sure you wish to delete this image?", true, false))
                return;

            _siteImages.RemoveAt(SelectedImage);
            NotifyOfPropertyChange(() => SiteImages);
        }

        #region Image Dragging

        public void StartImageDrag(DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void DoImageDrag(DragEventArgs e)
        {
            var data = (string[])e.Data.GetData(DataFormats.FileDrop);

            var acceptedExtensions = new List<string> { ".jpeg", ".jpg", ".png", ".gif", ".bmp" };

            InsertImagesForSite((from file in data let extension = Path.GetExtension(file) where extension != null && acceptedExtensions.Contains(extension.ToLower()) select file).ToArray());
        }

        #endregion

        #endregion

        public void BtnSiteDone()
        {
                try
                {
                    if (IsNewSite)
                    {
                        if (!siteNameIsUnique(SiteName))
                        {
                            Microsoft.Windows.Controls.MessageBox.Show("Site names neeed to be unique, please select a unique site name", "Unique site name needed");
                            return;
                        }
                    }
                            DataSet.Site.GpsLocation = GPSCoords.Parse(Latitude, Longitude);
                            DataSet.Site.GpsLocation.GridSystem = GridSystem;
                            DataSet.Site.Owner = Owner;
                            DataSet.Site.CountryName = CountryCode;
                            OwnerHelper.Add(Owner);
                            DataSet.Site.SiteNotes = Notes;
                            DataSet.Site.PrimaryContact = PrimaryContact;
                            DataSet.Site.Elevation = float.Parse(Elevation);
                            DataSet.Site.Images = _siteImages.ToList();
                            
                            var bw = new BackgroundWorker();

                            bw.DoWork += (o, e) =>
                                             {
                                                 var oldFile = DataSet.SaveLocation;
                                                 var oldName = DataSet.Site.Name;
                                                 DataSet.Site.Name = SiteName;
                                                 DataSet.SaveToFile(false);

                                                 if (SiteName.CompareTo(oldName) != 0)
                                                 {
                                                     File.Delete(oldFile);
                                                 }
                                             };

                            bw.RunWorkerCompleted += (o, e) =>
                                                         {
                                                             EventLogger.LogInfo(DataSet, GetType().ToString(), "Site saved. Site name: " + DataSet.Site.Name);

                                                             CreateEditDeleteVisible = Visibility.Visible;
                                                             DoneCancelVisible = Visibility.Collapsed;
                                                             DoneCancelEnabled = true;
                                                             SiteControlsEnabled = false;
                                                             ApplicationCursor = Cursors.Arrow;

                                                             if (!IsNewSite) return;

                                                             WasCompleted = true;
                                                             //this.TryClose();
                                                         };

                            ApplicationCursor = Cursors.Wait;
                            DoneCancelEnabled = false;
                            bw.RunWorkerAsync();

                        
                    
                }
                catch (Exception e)
                {
                    Common.ShowMessageBox("Error", e.Message, false, true);
                    EventLogger.LogError(DataSet, GetType().ToString(), "Tried to create site but failed. Details: " + e.Message);
                }
            }
        

        public void BtnSiteCancel()
        {

            DataSet = DataSet;

            CreateEditDeleteVisible = Visibility.Visible;
            DoneCancelVisible = Visibility.Collapsed;
            SiteControlsEnabled = false;

            if (IsNewSite)
                TryClose();
        }

        public void BtnSiteEdit()
        {
            CreateEditDeleteVisible = Visibility.Collapsed;
            DoneCancelVisible = Visibility.Visible;
            SiteControlsEnabled = true;
        }

        public void BtnSiteClose()
        {
            TryClose();
        }

        public void BtnGoToGPS()
        {
            if (!String.IsNullOrWhiteSpace(Longitude) && !String.IsNullOrWhiteSpace(Latitude))
            {
                var webAddy = "https://maps.google.com/maps?q=" + Latitude + "," + Longitude;
                System.Diagnostics.Process.Start(webAddy);
            }
        }

        public void BtnSiteLoad()
        {
             if (DataSet != null)
            {
                
                OpenFileDialog openMeta = new OpenFileDialog();
                openMeta.Filter = @"Meta Files|*.txt|All Files|*.*;";
                if (openMeta.ShowDialog() == DialogResult.OK)
                {
                    LoadFromMeta(openMeta.FileName);
                }
             }
        }

       public void LoadFromMeta(string filename)
       {
           MetaFileReader readIn = new MetaFileReader();
           Dataset oldDataSet = DataSet;
           DataSet = readIn.readMeta(oldDataSet, filename);
           int j = oldDataSet.ActualDataPointCount;

        }
        
       

        public string CleanMetaIn(string raw, int toRemove)
        {
            string clean;
            clean = raw.Substring(toRemove);
            return clean;
        }
        #endregion


    }
}

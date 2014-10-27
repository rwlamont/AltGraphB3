using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IndiaTango.Models
{
    /// <summary>
    /// Helper class for retrieving the abreviations
    /// </summary>
    public static class AbrevsHelper
    {
        public static string FileLocation
        {
            get { return Path.Combine(Common.AppDataPath, "Abrev.csv"); }
        }

        private static ObservableCollection<string> _abrevs;
        /// <summary>
        /// The collection of units
        /// </summary>
        public static ObservableCollection<string> Abrevs
        {
            get
            {
                if (_abrevs == null)
                    LoadAbrevs();
                return _abrevs;
            }
        }
        /// <summary>
        /// Adds a new abreviation  
        /// </summary>
        /// <param name="abrevs">the abreviation</param>
        public static void Add(string abrevs)
        {
            if (!_abrevs.Contains(abrevs))
                _abrevs.Add(abrevs);
            SaveAbrevs();
        }

        private static void LoadAbrevs()
        {
            if (!File.Exists(FileLocation))
            {
                _abrevs = GenerateAbrevs();
                SaveAbrevs();
            }
            else
            {
                var units = new List<string>();
                var file = File.ReadAllText(FileLocation, Encoding.UTF8);
                units.AddRange(file.Split(','));
                _abrevs = new ObservableCollection<string>(units);
            }
        }

        private static void SaveAbrevs()
        {
            using (var fileStream = File.CreateText(FileLocation))
            {
                for (var i = 0; i < _abrevs.Count; i++)
                {
                    if (i > 0)
                        fileStream.Write(',');
                    fileStream.Write(_abrevs[i]);
                }
            }
        }

        private static ObservableCollection<string> GenerateAbrevs()
        {
            var abrevs = new List<string>();

            var abrevsFile = Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("B3.exe", ""), "Resources", "Abrev.csv");

            if (File.Exists(abrevsFile))
            {
                abrevs.AddRange(File.ReadAllText(abrevsFile, Encoding.UTF8).Split(','));
                abrevs = abrevs.Distinct().ToList();
                abrevs.Sort();
            }

            return new ObservableCollection<string>(abrevs);
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IndiaTango.Models
{
    public struct countryCodes
    {
        public string countryName;
        public string countryCode;
    }

    class CountriesHelper
    {
        /// <summary>
    /// Helper class for retrieving the abreviations
    /// </summary>
    /// 
        public static string FileLocation
        {
            get { return Path.Combine(Common.AppDataPath, "Countries.txt"); }
        }

        private static ObservableCollection<countryCodes> _countries;
       
        /// <summary>
        /// The collection of units
        /// </summary>
        public static ObservableCollection<countryCodes> Countries
        {
            get
            {
                if (_countries == null)
                    LoadCountries();
                return _countries;
            }
        }


        private static void LoadCountries()
        {
            if (!File.Exists(FileLocation))
            {
                GenerateCountries();
            }
              _countries = new ObservableCollection<countryCodes>();
                string line;
                string[] lineparts;
                try
                {
                    StreamReader reader = new StreamReader(FileLocation);
                    int i = 0;
                    line = reader.ReadLine();
                    do
                    {
                        
                        countryCodes newCountry = new countryCodes();
                        lineparts = line.Split(',');
                        if (lineparts.Count() >= 2)
                        {
                            newCountry.countryName = lineparts[1];
                            newCountry.countryCode = lineparts[0];
                            _countries.Add(newCountry);
                        }
                        i++;
                        line = reader.ReadLine();
                    } while (line != null);
                    
                }
                catch (System.Exception excep)
                {
                    System.Windows.MessageBox.Show(excep.ToString());
                }
            
        }

       
        private static void GenerateCountries()
        {
            var abrevs = new List<countryCodes>();
            _countries = new ObservableCollection<countryCodes>();
            var abrevsFile = Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("B3.exe", ""), "Resources", "Countries.txt");

            if (File.Exists(abrevsFile))
            {
                if (File.Exists(FileLocation))
                    File.Delete(FileLocation);

                File.Copy(abrevsFile,FileLocation);
            }

     }

        public static ObservableCollection<string> CountriesNames()
        {
            List<string> cLong = new List<string>();
            foreach(countryCodes c in Countries)
            {
                cLong.Add(c.countryName);
            }

            return new ObservableCollection<string>(cLong);
        }

        public static string GetCode(string p)
        {
       foreach (countryCodes c in Countries)
            {
                if (c.countryName.Equals(p))
                {
                    return  "(" + c.countryCode + ")";
                    
                }
            }

            return "()";     

        }
    }
        
    


}

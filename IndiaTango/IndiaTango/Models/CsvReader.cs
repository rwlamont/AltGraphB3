using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IndiaTango.Models
{
    /// <summary>
    /// Reads CSV files
    /// </summary>
    public class CSVReader : IDataReader
    {
        private readonly string _filename;
        private readonly char _delimitChar;
        private Sensor[] _sensors;
        public event ReaderProgressChanged ProgressChanged;
        
        public CSVReader(string fileName)
        {
            if (!fileName.EndsWith(".csv") && !fileName.EndsWith(".gln") && !fileName.EndsWith(".txt"))
               throw new ArgumentException("Can't tell if this is a csv/ tsv (filetype not .csv/.txt)");

            //Series of if statments to work out the delimiting character
            if(fileName.EndsWith(".csv"))
            {
                _delimitChar = ',';
            }
            if(fileName.EndsWith(".txt"))
            {
                _delimitChar = '\t';
                            }
            if (fileName.EndsWith(".gln"))
            {
                _delimitChar = '\t';
                
            }
            if (!File.Exists(fileName))
                throw new ArgumentException("File wasn't found");

            _filename = fileName;

            //ProgressChanged += OnProgressChanged;
        }

        public List<Sensor> ReadSensors()
        {
            return ReadSensors(null, null);
        }

        public List<Sensor> ReadSensors(BackgroundWorker asyncWorker, Dataset owner)
        {
            if (_sensors != null)
                return _sensors.ToList();

            var linesInFile = File.ReadLines(_filename).Count();
            var linesRead = 0d;
            var oldProgresValue = 0;

            using (var reader = new StreamReader(_filename))
            {
                var csvHeaderLine = reader.ReadLine();
                if (csvHeaderLine == null)
                    throw new FileFormatException("Couldn't read the header line!");

                linesRead++;

                var headers = csvHeaderLine.Split(_delimitChar);

                if (headers.Distinct().Count() < headers.Count())
                    throw new FileFormatException("There are duplicate headers!");

                var dateTimeComponents = new List<DateTimeComponent>();
                var sensorIndexers = new List<SensorIndexer>();

                foreach (var header in headers)
                {
                    DateTimeComponentType dateTimeComponent;
                    var cleansedHeader = header.Replace(" ", "").Replace("-", "").Replace("/", "").Replace(":", "");
                    var isDateTimeComponent = Enum.TryParse(cleansedHeader, out dateTimeComponent);
                    Debug.Print("{0} is a dateTimeComponent: {1}", header, isDateTimeComponent);
                    if (isDateTimeComponent)
                    {
                        dateTimeComponents.Add(new DateTimeComponent
                        {
                            Index = Array.IndexOf(headers, header),
                            Type = dateTimeComponent
                        });
                    }
                    else
                    {
                        Debug.Print("{0} is not a dateTimeComponent", header);

                        if (header.Contains("_") && header.Substring(2, 5).Contains('_'))
                        {

                        string[] hdrSplit = header.Split('_');
                        if (!String.IsNullOrWhiteSpace(hdrSplit[0]) || !String.IsNullOrWhiteSpace(hdrSplit[1]) && hdrSplit[1].Contains('('))
                        {
                            String[] hdrSplitTwo = hdrSplit[1].Split('(');
                            string sen;
                            if (!String.IsNullOrWhiteSpace(hdrSplitTwo[1]))
                            {
                                hdrSplitTwo[1] = hdrSplitTwo[1].TrimEnd(')'); // If it gets to here has unit and a header name of some sort
                                if (!String.IsNullOrWhiteSpace(hdrSplitTwo[1]))
                                {

                                    sensorIndexers.Add(new SensorIndexer
                                    {
                                        Index = Array.IndexOf(headers, header),
                                        Sensor = new Sensor(header, hdrSplitTwo[1], owner)
                                    });
                                    if(!string.IsNullOrWhiteSpace(hdrSplitTwo[0]))
                                    {
                                    sen = hdrSplitTwo[0][0].ToString();

                                    if (!String.IsNullOrEmpty(sen))
                                    {

                                        hdrSplitTwo[0] = hdrSplitTwo[0].TrimStart(sen[0]);


                                        sensorIndexers.Last().Sensor.SensorTypeAbrev = hdrSplit[0];
                                        sensorIndexers.Last().Sensor.Location = hdrSplitTwo[0];
                                        sensorIndexers.Last().Sensor.Position = ParameterValues.getFullPosition(sen);
                                        float elev;
                                        if (float.TryParse(hdrSplitTwo[0], out elev))
                                        {
                                            sensorIndexers.Last().Sensor.Elevation = float.Parse(hdrSplitTwo[0]); // If it gets to here it has a full Gleon set up
                                        }
                                        ParameterAbrev abrevIn;
                                        if (Enum.TryParse(hdrSplit[0], out abrevIn))
                                        {

                                            sensorIndexers.Last().Sensor.SensorType = ParameterValues.getFullName(abrevIn);
                                        }
                                    }
                                    }
                                }
                                else
                                {
                                    sensorIndexers.Add(new SensorIndexer
                                   {
                                       Index = Array.IndexOf(headers, header),
                                       Sensor = new Sensor(header, null, owner)
                                   });
                                    if (!String.IsNullOrWhiteSpace(hdrSplit[0])) // If it gets to here it has a name of some sort, but no unit. Name may
                                    // be a parameter abreviation as per gleon format
                                    {
                                        ParameterAbrev abrevIn;
                                        if (Enum.TryParse(hdrSplit[0], out abrevIn))
                                        {
                                            sensorIndexers.Last().Sensor.SensorTypeAbrev = hdrSplit[0];


                                            sensorIndexers.Last().Sensor.SensorType = ParameterValues.getFullName(abrevIn);
                                        }
                                    }
                                }

                            }
                            else
                            {
                                sensorIndexers.Add(new SensorIndexer // Here its just added as the sensor name, no other details 
                                        {
                                            Index = Array.IndexOf(headers, header),
                                            Sensor = new Sensor(header, null, owner)
                                        });
                            }
                        }
                        }
                    

                        else
                        {
                            sensorIndexers.Add(new SensorIndexer // Here its just added as the sensor name, no other details 
                            {
                                Index = Array.IndexOf(headers, header),
                                Sensor = new Sensor(header, null, owner)
                            });
                        }

                        
                    }
                }
                    string lineRead;
                    while ((lineRead = reader.ReadLine()) != null)
                    {
                        if (asyncWorker != null && asyncWorker.CancellationPending)
                            return null;
                        linesRead++;

                        var progress = (int)(linesRead / linesInFile * 100);

                        if (progress > oldProgresValue)
                        {
                            OnProgressChanged(this, new ReaderProgressChangedArgs(progress));
                            oldProgresValue = progress;
                        }

                        var lineComponents = lineRead.Split(_delimitChar);

                        var cleansedLineComponenets = lineRead.Split(_delimitChar);
                        for (var i = 0; i < cleansedLineComponenets.Length; i++)
                        {
                            cleansedLineComponenets[i] = cleansedLineComponenets[i].Replace(" ", "").Replace("-", "").Replace("/", "").Replace(":", "");
                        }

                        var dateTime = DateTime.MinValue;
                        var hasYear = false;
                        var hasMonth = false;
                        var hasDay = false;
                        var hasHour = false;
                        var hasMinute = false;
                        foreach (var dateTimeComponent in dateTimeComponents)
                        {
                            if (dateTimeComponent.Index > cleansedLineComponenets.Length)
                                throw new FileFormatException("There aren't enough values for the date-time components");
                            try
                            {
                                #region Date Parsers
                                if (dateTimeComponent.Type == DateTimeComponentType.DD)
                                {
                                    dateTime = new DateTime(dateTime.Year, dateTime.Month, int.Parse(cleansedLineComponenets[dateTimeComponent.Index]), dateTime.Hour, dateTime.Minute, 0);
                                    hasDay = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.DDMM)
                                {
                                    dateTime = new DateTime(dateTime.Year, int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), dateTime.Hour, dateTime.Minute, 0);
                                    hasDay = true;
                                    hasMonth = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.DDMMYYYY)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), dateTime.Hour, dateTime.Minute, 0);
                                    hasDay = true;
                                    hasMonth = true;
                                    hasYear = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.DDMMYYYYhhmm)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(10, 2)), 0);
                                    hasDay = true;
                                    hasMonth = true;
                                    hasYear = true;
                                    hasHour = true;
                                    hasMinute = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.MM)
                                {
                                    dateTime = new DateTime(dateTime.Year, int.Parse(cleansedLineComponenets[dateTimeComponent.Index]), dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                                    hasMonth = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.MMDD)
                                {
                                    dateTime = new DateTime(dateTime.Year, int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2)), dateTime.Hour, dateTime.Minute, 0);
                                    hasMonth = true;
                                    hasDay = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.MMDDYYYY)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2, 2)), dateTime.Hour, dateTime.Minute, 0);
                                    hasMonth = true;
                                    hasDay = true;
                                    hasYear = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.MMDDYYYYhhmm)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(10, 2)), 0);
                                    hasMonth = true;
                                    hasDay = true;
                                    hasYear = true;
                                    hasHour = true;
                                    hasMinute = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYY)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index]), dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                                    hasYear = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYDDMM)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), dateTime.Hour, dateTime.Minute, 0);
                                    hasYear = true;
                                    hasDay = true;
                                    hasMonth = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYDDMMhh)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8)), dateTime.Minute, 0);
                                    hasYear = true;
                                    hasDay = true;
                                    hasMonth = true;
                                    hasHour = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYDDMMhhmm)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(10)), 0);
                                    hasYear = true;
                                    hasDay = true;
                                    hasMonth = true;
                                    hasHour = true;
                                    hasMinute = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYMM)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4)), dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                                    hasYear = true;
                                    hasMonth = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYMMDD || dateTimeComponent.Type == DateTimeComponentType.Date)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6)), dateTime.Hour, dateTime.Minute, 0);
                                    hasYear = true;
                                    hasMonth = true;
                                    hasDay = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYMMDDhh)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8)), dateTime.Minute, 0);
                                    hasYear = true;
                                    hasMonth = true;
                                    hasDay = true;
                                    hasHour = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.YYYYMMDDhhmm || dateTimeComponent.Type == DateTimeComponentType.DateTime)
                                {
                                    dateTime = new DateTime(int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 4)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(4, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(6, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(8, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(10)), 0);
                                    hasYear = true;
                                    hasMonth = true;
                                    hasDay = true;
                                    hasHour = true;
                                    hasMinute = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.hh)
                                {
                                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, int.Parse(cleansedLineComponenets[dateTimeComponent.Index]), dateTime.Minute, 0);
                                    hasHour = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.hhmm || dateTimeComponent.Type == DateTimeComponentType.Time)
                                {
                                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(0, 2)), int.Parse(cleansedLineComponenets[dateTimeComponent.Index].Substring(2)), 0);
                                    hasHour = true;
                                    hasMinute = true;
                                }
                                else if (dateTimeComponent.Type == DateTimeComponentType.mm)
                                {
                                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, int.Parse(cleansedLineComponenets[dateTimeComponent.Index]), 0);
                                    hasMinute = true;
                                }
                                #endregion
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new FileFormatException(string.Format("The date time component for {0} on line {1} is not formatted correctly\r\n\nWe have read it as:\r\n{2}", headers[dateTimeComponent.Index], linesRead, lineComponents[dateTimeComponent.Index]));
                            }

                        }
                        if (!hasYear || !hasMonth || !hasDay || !hasHour || !hasMinute)
                        {
                            var errorMessage = "Date wasn't complete\r\n";

                            if (!hasYear)
                                errorMessage += "\r\nMissing Year!";
                            if (!hasMonth)
                                errorMessage += "\r\nMissing Month!";
                            if (!hasDay)
                                errorMessage += "\r\nMissing Day!";
                            if (!hasHour)
                                errorMessage += "\r\nMissing Hour!";
                            if (!hasMinute)
                                errorMessage += "\r\nMissing Minute!";

                            errorMessage += "\r\nPlease reformat for the ISO 8601 standard\r\n\nDate Headers Recognized:";
                            errorMessage = dateTimeComponents.Aggregate(errorMessage,
                                                                        (current, component) =>
                                                                        string.Format("{0}\r\n{1}", current, headers[component.Index]));

                            errorMessage += "\r\n\r\nFailing Line (Line Number " + linesRead + "):\r\n" + lineRead;
                            throw new FileFormatException(errorMessage);
                        }

                        foreach (var sensorIndexer in sensorIndexers)
                        {
                            if (sensorIndexer.Index > lineComponents.Length)
                                throw new FileFormatException("There aren't enough values for all the sensors");

                            float value;
                            if (float.TryParse(lineComponents[sensorIndexer.Index], out value))
                                sensorIndexer.Sensor.RawData.Values[dateTime] = value;
                        }
                    }

                    //Convert SensorIndexes to Array
                    _sensors = sensorIndexers.OrderBy(x => x.Index).Select(x => x.Sensor).ToArray();
                }

                if (_sensors == null)
                    throw new FileFormatException("No sensors were read! File is of bad format");
                return _sensors.ToList();
            }
        


        public Site ReadBuoy()
        {
            throw new NotImplementedException();
        }

        void OnProgressChanged(object o, ReaderProgressChangedArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(o, e);
        }
    }

    public class ReaderProgressChangedArgs : EventArgs
    {
        public readonly int Progress;

        public ReaderProgressChangedArgs(int progress)
        {
            Progress = progress;
        }
    }

    public delegate void ReaderProgressChanged(object o, ReaderProgressChangedArgs e);

    public enum DateTimeComponentType
    {
        YYYY,
        MM,
        DD,
        hh,
        mm,
        YYYYMM,
        YYYYMMDD,
        YYYYMMDDhhmm,
        YYYYMMDDhh,
        YYYYDDMM,
        YYYYDDMMhhmm,
        YYYYDDMMhh,
        MMDD,
        DDMM,
        DDMMYYYY,
        DDMMYYYYhhmm,
        MMDDYYYY,
        MMDDYYYYhhmm,
        hhmm,
        Date,
        DateTime,
        Time
    }

    public class DateTimeComponent
    {
        public int Index;
        public DateTimeComponentType Type;
    }

    public class SensorIndexer
    {
        public int Index;
        public Sensor Sensor;
    }
    /// <summary>
    /// Class for returning of parameter abrev or full siteNames
    /// </summary>
    public class ParameterValues
    {
        public static string getFullPosition(string input)
        {
            string toReturn = "";
            switch (input)
            {
                case "d":
                    toReturn = "Depth";
                    break;
                case "e":
                    toReturn = "Elevation";
                    break;
                case "v":
                    toReturn = "Variable";
                    break;
                case "m":
                    toReturn = "MASL";
                    break;
                case "h":
                    toReturn = "Height";
                    break;


            }
            return toReturn;
        }

        public static string getAbrevPosition(string input)
        {
            string toReturn = "";
            switch (input)
            {
                case "Depth":
                    toReturn = "d";
                    break;
                case "Elevation":
                    toReturn = "e";
                    break;
                case "Variable":
                    toReturn = "v";
                    break;
                case "MASL":
                    toReturn = "m";
                    break;
                case "Height":
                
                    toReturn = "h";
                    break;

            }
            return toReturn;
        }

        /// <summary>
        /// given a abreviation returns a full parameter name
        /// </summary>
        /// <param name="abrev">the abreviation we want to get the full name for</param>
        /// <returns></returns>
        public static string getFullName(ParameterAbrev abrev)
        {
           
            if (Enum.IsDefined(typeof(ParameterAbrev), abrev) | abrev.ToString().Contains(","))
            {
                var val = Convert.ChangeType(abrev, abrev.GetTypeCode());
                var oval = Enum.GetName(typeof(ParameterFull), val);
                return oval;
            }
            else
            {
                
                return "";
            }
        }
        /// <summary>
        /// Given the full parameter name returns the abreviation
        /// </summary>
        /// <param name="fullName">The full name we want to find the abreviation for</param>
        /// <returns></returns>
        public static string getAbrev (ParameterFull fullName)
        {
            
            if (Enum.IsDefined(typeof(ParameterFull), fullName) | fullName.ToString().Contains(","))
            {
                var val = Convert.ChangeType(fullName, fullName.GetTypeCode());
                var oval = Enum.GetName(typeof(ParameterAbrev), val);
               
                return oval;
            }
            else
            {
                
                return "";
            }
        }
        /// <summary>
        /// Gets the unit from the full parameter name by converting it to a abrev and then
        /// sending it to getUnitAbrev
        /// </summary>
        /// <param name="fullParam">The paramater you want to find the abreviation for</param>
        /// <returns></returns>
       public string getUnitFull(ParameterFull fullParam)

        {
           ParameterAbrev abrev;
           Enum.TryParse(getAbrev(fullParam), out abrev);
           return getUnitAbrev(abrev);
        }
        /// <summary>
        /// gets the unit from the abreviated paramater
        /// </summary>
        /// <param name="toCompare">the abreviation we want to find the suggested unit for</param>
        /// <returns></returns>
        public string getUnitAbrev(ParameterAbrev toCompare)
       {
           #region SensorParsers
           string toReturn = "";
            switch(toCompare)
            {
                case ParameterAbrev.TmpWtr:
                case ParameterAbrev.TmpDew:
                case ParameterAbrev.TmpAir:
                case ParameterAbrev.SoilT:
                    toReturn = "degC";
                    break;
                case ParameterAbrev.RadLWD:
                case ParameterAbrev.RadSWD:
                case ParameterAbrev.RadTD:
                case ParameterAbrev.HFlxEv:
                case ParameterAbrev.HFlxSn:
                    toReturn = "W/m^2";
                    break;
                case ParameterAbrev.VPSat:
                case ParameterAbrev.VPDef:
                case ParameterAbrev.VP:
                case ParameterAbrev.BaroP:
                    toReturn = "hPa";
                    break;
                case ParameterAbrev.WndMin:
                case ParameterAbrev.WndSpd:
                case ParameterAbrev.WtrVlV:
                case ParameterAbrev.WtrVlH:
                    toReturn = "m/s";
                    break;
                case ParameterAbrev.PpT:
                case ParameterAbrev.PpRain:
                case ParameterAbrev.Snow:
                    toReturn = "mm";
                    break;
                case ParameterAbrev.DOconc:
                case ParameterAbrev.DOC:
                case ParameterAbrev.DCO2:
                    toReturn = "mg/L";
                    break;
                case ParameterAbrev.VBatLg:
                case ParameterAbrev.VBatR:
                case ParameterAbrev.VSol:
                    toReturn = "V";
                    break;
                case ParameterAbrev.RadPAR:
                    toReturn = "umol/m^2/s";
                    break;
                case ParameterAbrev.CDOM:
                    toReturn = "ug/L";
                    break;
                case ParameterAbrev.WavePd:
                    toReturn = "s";
                    break;
                case ParameterAbrev.FlPhy:
                case ParameterAbrev.FlChl:
                    toReturn = "RFU";
                        break;
                case ParameterAbrev.CO2A:
                        toReturn = "ppm";
                        break;
                case ParameterAbrev.RadUV:
                        toReturn = "mW/cm^2";
                        break;
                case ParameterAbrev.ORP:
                        toReturn = "mV";
                        break;
                case ParameterAbrev.Cond:
                case ParameterAbrev.CondSp:
                        toReturn = "mS/cm";
                        break;
                case ParameterAbrev.FlwVol:
                    toReturn = "m^3";
                    break;
                case ParameterAbrev.Kd:
                    toReturn = "m^-1";
                    break;
                case ParameterAbrev.WaveHt:
                case ParameterAbrev.LvlDpt:
                    toReturn = "m";
                    break;
                case ParameterAbrev.PpHail:
                    toReturn = "hits/cm^2";
                    break;
                case ParameterAbrev.PO4:
                case ParameterAbrev.NH4:
                case ParameterAbrev.NO3:
                case ParameterAbrev.Cl:
                    toReturn = "g/m^3, mg/m^3";
                    break;
                case ParameterAbrev.Turb:
                    toReturn = "FTU";
                    break;
                case ParameterAbrev.WndDrP:
                case ParameterAbrev.WndMax:
                case ParameterAbrev.WndDir:
                    toReturn = "deg";
                    break;
                case ParameterAbrev.BmAtt:
                    toReturn = "1/m";
                    break;
                case ParameterAbrev.DOsat:
                    toReturn = "%sat";
                    break;
                case ParameterAbrev.RelH:
                case ParameterAbrev.BmTran:
                    toReturn = "%";
                    break;
           #endregion
            }

            return toReturn;

        } 
    }
    // Full siteNames of all of the paramaters. Order is the same as in ParameterAbrev, if modyfing make sure it stays the same
    public enum  ParameterFull
    {
        Barometric_Pressure,
        Beam_Attenuation,
        Beam_Transmission,
        Colored_Dissolved_Organic_Matter,
        Chloride,
        Atmospheric_Carbon_Dioxide,
        Conductivity,
        Specific_Conductance,
        Dissolved_Carbon_Dioxide,
        Dissolved_Organic_Carbon,
        Dissolved_Oxygen_Concentration,
        Dissolved_Oxygen_Saturation,
        Chlorophyll_Fluorescence,
        Phycocyanin_Fluorescence,
        Heat_Flux_Evaporative,
        Heat_Flux_Sensible,
        Light_Attenuation_Coefficient,
        Ammonium,
        Nitrate,
        Reduction_Oxidation_Potential,
        pH,
        Phosphate,
        Precipitation_Hail,
        Precipitation_Rainfall,
        Precipitation_total,
        Longwave_Radiation_Downwelling,
        Photosynthetically_Active_Radiation,
        Shortwave_Radiation_Downwelling,
        Shortwave_Light_Penetration,
        Total_Radiation_Downwelling,
        Ultraviolet_Radiation,
        Relative_Humidity,
        Salinity,
        Precipitation_Snow,
        Soil_Temperature,
        Air_Temperature,
        Dewpoint_Temperature,
        Water_Temperature,
        Turbidity,
        Datalogger_Battery_Voltage,
        Radio_Battery_Voltage,
        Vapor_Pressure,
        Vapor_Pressure_Deficit,
        Vapor_Pressure_Saturation,
        Solar_Panel_Voltage,
        Wave_Height,
        Wave_Period,
        Wind_Direction,
        Wind_Direction_at_Peak_Speed,
        Wind_Speed_Peak,
        Wind_Speed_Minimum,
        Wind_Speed_Average,
        Water_Column_Depth,
        Water_Velocity_Horizontal,
        Water_Velocity_Vertical,
        Flow_Volume
    };

    // Abbreviated siteNames of all of the paramaters. Order is the same as in ParameterFull, if modyfing make sure it stays the same
    public enum ParameterAbrev { BaroP, BmAtt, BmTran, CDOM, Cl, CO2A, Cond, CondSp, DCO2, DOC, DOconc, DOsat, FlChl, FlPhy, HFlxEv, HFlxSn, Kd, NH4, NO3, ORP, pH, PO4, PpHail, PpRain, PpT, RadLWD, RadPAR, RadSWD, RadSWP, RadTD, RadUV, RelH, Sal, Snow, SoilT, TmpAir, TmpDew, TmpWtr, Turb, VBatLg, VBatR, VP, VPDef, VPSat, VSol, WaveHt, WavePd, WndDir, WndDrP, WndMax, WndMin, WndSpd, LvlDpt, WtrVlH, WtrVlV, FlwVol }; // Abreviated siteNames of all of the parameters
        
}
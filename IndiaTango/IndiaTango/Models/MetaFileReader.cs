using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

namespace IndiaTango.Models
{
    class MetaFileReader
    {

        public string CleanMetaIn(string raw, int toRemove)
        {
            string clean;
            clean = raw.Substring(toRemove);
            return clean;
        }

        public Dataset readMeta(Dataset input, string filename)
        {

            string siteOwner, siteName, siteCountry, siteGPSLat, siteGPSLong, siteGPSGrid, siteElevation, siteContactName, siteContactNumber, siteContactEmail, siteContactOrginisation;
            try
            {

                int iss;
                string numSensors, loopStr;
                StreamReader reader = new StreamReader(filename);
                reader.ReadLine(); // Throwing away asscoiated file
                siteName = CleanMetaIn(reader.ReadLine(), 11);
                siteOwner = CleanMetaIn(reader.ReadLine(), 7);
                siteGPSLat = CleanMetaIn(reader.ReadLine(), 19);
                siteGPSLong = CleanMetaIn(reader.ReadLine(), 19);
                siteGPSGrid = CleanMetaIn(reader.ReadLine(), 17);
                siteElevation = CleanMetaIn(reader.ReadLine(), 18);
                siteCountry = CleanMetaIn(reader.ReadLine(), 9);
                reader.ReadLine(); // Throwing away contact header
           
                siteContactName = CleanMetaIn(reader.ReadLine(), 6);
                siteContactOrginisation = CleanMetaIn(reader.ReadLine(), 14);
                siteContactNumber = CleanMetaIn(reader.ReadLine(), 7);
                siteContactEmail = CleanMetaIn(reader.ReadLine(), 7);
                numSensors = reader.ReadLine();

                if (String.IsNullOrWhiteSpace(siteContactName))
                {
                    siteContactName = ". .";
                }
                if (String.IsNullOrWhiteSpace(siteContactNumber))
                {
                    siteContactNumber = " ";
                }
                if (String.IsNullOrWhiteSpace(siteContactEmail))
                {
                    siteContactEmail = " ";
                }
                if (String.IsNullOrWhiteSpace(siteContactOrginisation))
                {
                    siteContactOrginisation = " ";
                }
                iss = Int32.Parse(CleanMetaIn(numSensors, 19));
                if (iss == input.Sensors.Count) // check to see if the number of sensors matches whats in the meta file
                {

                    string header = reader.ReadLine();
                    string[] arr4 = new string[iss + 1];
                    for (int i = 0; i < iss; i++)
                    {
                        header = reader.ReadLine();


                        int ndx = input.Sensors.FindIndex(delegate(Sensor toFind)
                        {
                            return toFind.Name == header;
                        }
                        );
                        loopStr = reader.ReadLine();
                        if (ndx >= 0)
                        {
                            do // Works out what meta data is attached to each sensor
                                //and adds it into the correct place
                            {

                                if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Desc")
                                {
                                    input.Sensors[ndx].Description = CleanMetaIn(loopStr, 13);
                                    loopStr = reader.ReadLine();
                                }
                                if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Seri")
                                {
                                    input.Sensors[ndx].CurrentMetaData.SerialNumber = CleanMetaIn(loopStr, 15);
                                    loopStr = reader.ReadLine();
                                }
                                if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Manu")
                                {
                                    input.Sensors[ndx].CurrentMetaData.Manufacturer = CleanMetaIn(loopStr, 14);
                                    loopStr = reader.ReadLine();
                                }
                                if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Date")
                                {
                                    input.Sensors[ndx].CurrentMetaData.DateOfInstallation = DateTime.Parse(CleanMetaIn(loopStr, 16));
                                    loopStr = reader.ReadLine();
                                }
                                if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Cali")
                                {
                                    if (loopStr.Substring(10, 2) == "n ")
                                    {

                                        input.Sensors[ndx].CurrentMetaData.IdealCalibrationFrequency = TimeSpan.FromDays(Double.Parse(CleanMetaIn(loopStr, 29)));
                                        loopStr = reader.ReadLine();
                                    }
                                    if (!string.IsNullOrEmpty(loopStr) && loopStr.Substring(0, 4) == "Cali" && loopStr.Substring(10, 2) == "n:")
                                    {
                                        var calibStr = CleanMetaIn(loopStr, 12);
                                        DateTime calibTime = new DateTime(int.Parse(calibStr.Substring(0, 4)), int.Parse(calibStr.Substring(5, 2)), int.Parse(calibStr.Substring(8, 2)));
                                        string[] first = calibStr.Substring(16).Split(' ');
                                        string[] preNum = first[0].Split('-');
                                        string[] postNum = first[2].Split('-');
                                        postNum[1].Remove(0, 6);
                                        input.Sensors[ndx].Calibrations.Add(new Calibration(calibTime, float.Parse(preNum[0].TrimStart('[')), float.Parse(preNum[1]), float.Parse(preNum[2].TrimEnd(']')), float.Parse(postNum[0].TrimStart('[')), float.Parse(postNum[1]), float.Parse(postNum[2].TrimEnd(']'))));
                                        loopStr = reader.ReadLine();
                                    }
                                }

                            } while (!string.IsNullOrEmpty(loopStr));
                        }
                    }

                }

                else
                {
                    Microsoft.Windows.Controls.MessageBox.Show("Could not load sensor data as meta file did not match actual number of sensors");
                    //Skip through the sensors until the notes
                    do
                    {
                        reader.ReadLine();
                    }
                    while (reader.Peek() != 'D');

                }

                string checkNext = reader.ReadLine();
                if (checkNext.Equals("Dataset Notes")) // Reasd
                {
                    if (input.Site.DataEditingNotes == null)
                        input.Site.DataEditingNotes = new Dictionary<DateTime, string>();
                    loopStr = reader.ReadLine();
                    while (!string.IsNullOrEmpty(loopStr))
                    {
                        input.Site.DataEditingNotes.Add(DateTime.Now, loopStr);
                        loopStr = reader.ReadLine();

                    }
                }

                checkNext = reader.ReadLine();
                if (checkNext.Equals("Site Notes")) // Rerads and add the site notes
                {
                    if (input.Site.SiteNotes == null)
                        input.Site.SiteNotes = " ";
                    loopStr = reader.ReadLine();
                    while (!string.IsNullOrEmpty(loopStr))
                    {
                        input.Site.SiteNotes = input.Site.SiteNotes + loopStr;
                        loopStr = reader.ReadLine();

                    }

                }

                var oldFile = input.SaveLocation;
                File.Delete(oldFile);
                var names = siteContactName.Split(' ');

                Contact siteContact = new Contact(names[0], names[1], siteContactEmail, siteContactOrginisation, siteContactNumber, 12);
                OwnerHelper.Add(siteOwner);
                ObservableCollection<Contact> contactList = Contact.ImportAll();
                if (!contactList.Contains(siteContact))
                {
                    contactList.Add(siteContact);
                    Contact.ExportAll(contactList);
                }
                input.Site.PrimaryContact = siteContact;
                input.Site.GpsLocation = new GPSCoords(decimal.Parse(siteGPSLat), decimal.Parse(siteGPSLong), siteGPSGrid);
                input.Site.Name = siteName;
                input.Site.Elevation = float.Parse(siteElevation);
                input.Site.Owner = siteOwner;
                input.Site.CountryName = siteCountry;

               
            }
            catch (Exception excep)
            {
                System.Windows.MessageBox.Show("There was an error importing the meta file, please make sure that it is correctly formated and all parts are filled in" );
            }
            return input;
        }
    }
}

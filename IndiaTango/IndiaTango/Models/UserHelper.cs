using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;


namespace IndiaTango.Models
{
            /// <summary>
        /// Helper class for retrieving the users. Just basic user set up to help log who did what change, as susch not encrypted or anything
        /// </summary>
    public static class UserHelper
    {


            public static string FileLocation
            {
                get { return Path.Combine(Common.AppDataPath, "Users.csv"); }
            }

            public static bool DoesItExist
            {
                get { return File.Exists(FileLocation); }
            }
            
            private static ObservableCollection<string> _users;
            /// <summary>
            /// The collection of units
            /// </summary>
            public static ObservableCollection<string> Users
            {
                get
                {
                    if (_users == null)
                        LoadUsers();
                    return _users;
                }
            }

            public static void Add(string user)
            {
                if (_users == null)
                    LoadUsers();
                if (!_users.Contains(user))
                    _users.Add(user);
                SaveUsers();
                CurrentUser = user;
            }

            private static void LoadUsers()
            {
                var usersIn = new List<string>();
                if (!File.Exists(FileLocation))
                {

                    _users = SetUpUsers();
                    SaveUsers();
                }
                else
                {
                    
                    var file = File.ReadAllText(FileLocation, Encoding.UTF8);
                    usersIn.AddRange(file.Split(','));
                    _users = new ObservableCollection<string>(usersIn);
                }
            }

            public static string ShowCurrentUser
            {
                get
                {
                    var toReturn = "You are currently logged in as " + CurrentUser;
                    return toReturn;
                }
            }

           public static void SaveUsers()
            {
                using (var fileStream = File.CreateText(FileLocation))
                {
                    for (var i = 0; i < _users.Count; i++)
                    {
                        if (i > 0)
                            fileStream.Write(',');
                        fileStream.Write(_users[i]);
                    }
                }
            }

            public static string CurrentUser
            {
                get
                {
                    if (_users == null)
                        LoadUsers();
                    return Users.First();
                }
                set
                {
                    Users.Remove(value);
                    Users.Insert(0, value);
                    SaveUsers();
                }

            }

            public static void ChangeCurrent(int user)
            {
                CurrentUser = Users.ElementAt(user);
            }

            private static ObservableCollection<string> SetUpUsers()
            {
                var users = new List<string>();

                var abrevsFile = Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("B3.exe", ""), "Resources", "Users.csv");

                if (File.Exists(abrevsFile))
                {
                    users.AddRange(File.ReadAllText(abrevsFile, Encoding.UTF8).Split(','));
                    users = users.Distinct().ToList();
                    users.Sort();
                }

                return new ObservableCollection<string>(users);
            }
    }
    }


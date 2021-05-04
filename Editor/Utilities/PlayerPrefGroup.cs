using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace OnionCollections.DataEditor.Editor
{
    [OpenWithOnionDataEditor(true)]
    [CreateAssetMenu(menuName = "Onion Data Editor/Player Pref Group", fileName = "PlayerPrefGroup")]
    internal class PlayerPrefGroup : ScriptableObject
    {

        [SerializeField]
        public string[] regexFilter = new string[0];

        [NodeIcon]
        Texture Icon => EditorGUIUtility.IconContent("d_Folder Icon").image;


        [NodeCustomElement]
        IEnumerable<TreeNode> Nodes
        {
            get
            {
                return FilterPlayerPrefByRegexs()
                    .Select(n =>
                    {
                        ValuePackage valuePackage = new ValuePackage();

                        TreeNode node = new TreeNode()
                        {
                            displayName = $"{n.Key} : {n.Value}",
                            description = GetPrefType(n.Value),
                            icon = OnionDataEditor.GetIconTexture("Dot"),
                            OnInspectorAction = GetPrefInspector(n.Key, n.Value, valuePackage),
                        };

                        node.NodeActions = new List<OnionAction>
                        {
                            new OnionAction(() =>
                            {
                                PlayerPrefs.DeleteKey(n.Key);
                                PlayerPrefs.Save();
                                OnionDataEditorWindow.RebuildNode();
                            },
                            $"Delete", OnionDataEditor.GetIconTexture("Trash")),
                            new OnionAction(() =>
                            {
                                object v = null;
                                switch (n.Value)
                                {
                                    case int i:
                                        PlayerPrefs.SetInt(n.Key, valuePackage.i);
                                        v = valuePackage.i;
                                        break;

                                    case float f:
                                        PlayerPrefs.SetFloat(n.Key, valuePackage.f);
                                        v = valuePackage.f;
                                        break;

                                    case string str:
                                        PlayerPrefs.SetString(n.Key, valuePackage.str);
                                        v = valuePackage.str;
                                        break;
                                }
                                node.displayName = $"{n.Key} : {v}";
                                PlayerPrefs.Save();
                            },
                            $"Save"),
                        };

                        return node;
                    });
            }
        }

        class ValuePackage
        {
            public int i;
            public float f;
            public string str;

        }


        OnionAction GetPrefInspector(string key, object value, ValuePackage valuePackage)
        {
            EditorGUIUtility.labelWidth = 150F;
            switch (value)
            {
                case int i:
                    return new OnionAction(() =>
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            i = EditorGUILayout.IntField(new GUIContent(key), i);
                            valuePackage.i = i;
                        }
                    });

                case float f:
                    return new OnionAction(() =>
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            f = EditorGUILayout.FloatField(new GUIContent(key), f);
                            valuePackage.f = f;
                        }
                    });

                case string str:
                    return new OnionAction(() =>
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            str = EditorGUILayout.TextField(new GUIContent(key), str);
                            valuePackage.str = str;
                        }
                    });

                default:
                    return null;
            }            
        }

        string GetPrefType(object value)
        {
            switch (value)
            {
                case int _:
                    return "Int";

                case float _:
                    return "Float";

                case string _:
                    return "String";

                default:
                    return $"Unknown({value.GetType().Name})";
            }
        }


        [NodeAction(actionName = "Add Random (Test)", iconName = "Edit")]
        void AddRandom()
        {
            int times = UnityEngine.Random.Range(1, 5);
            for (int i = 0; i < times; i++)
            {
                int choose = UnityEngine.Random.Range(0, 3);
                if (choose == 0)
                    PlayerPrefs.SetInt($"KEY{UnityEngine.Random.Range(0, 100)}", UnityEngine.Random.Range(0, 100));
                else if (choose == 1)
                    PlayerPrefs.SetFloat($"KEY{UnityEngine.Random.Range(0, 100)}", UnityEngine.Random.Range(0F, 100F));
                else if (choose == 2)
                    PlayerPrefs.SetString($"KEY{UnityEngine.Random.Range(0, 100)}", UnityEngine.Random.Range(0F, 100F).ToString());
            }
            PlayerPrefs.Save();

            OnionDataEditorWindow.RebuildNode();
        }



        [NodeAction(actionName = "Delete All", iconName = "Trash")]
        void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            OnionDataEditorWindow.RebuildNode();
        }



        IEnumerable<PlayerPrefPair> FilterPlayerPrefByRegexs()
        {
            var regexs = regexFilter
                .Where(n => string.IsNullOrEmpty(n) == false)
                .Select(n => new Regex(n));
            
            return GetAllPlayerPrefPair()
                .Where(n => regexs.All(r => r.Match(n.Key).Success));
        }



        [Serializable]
        public struct PlayerPrefPair
        {
            public string Key { get; set; }
            public object Value { get; set; }
        }


        
        static PlayerPrefPair[] GetAllPlayerPrefPair()
        {
            string companyName = PlayerSettings.companyName;
            string productName = PlayerSettings.productName;

            var result = GetAll_OSX_PlayerPrefPair() ?? GetAll_WIN_PlayerPrefPair();

            if (result == null)
                throw new NotSupportedException("PlayerPrefsEditor doesn't support this Unity Editor platform");

            return result;


            PlayerPrefPair[] GetAll_OSX_PlayerPrefPair()
            {
#if (UNITY_EDITOR_OSX)

            // From Unity docs: On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players.

            // Construct the plist filename from the project's settings
            string plistFilename = string.Format("unity.{0}.{1}.plist", companyName, productName);
            // Now construct the fully qualified path
            string playerPrefsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"),
                plistFilename);

            // Parse the player prefs file if it exists
            if (File.Exists(playerPrefsPath))
            {
                // Parse the plist then cast it to a Dictionary
                object plist = Plist.readPlist(playerPrefsPath);

                Dictionary<string, object> parsed = plist as Dictionary<string, object>;

                // Convert the dictionary data into an array of PlayerPrefPairs
                PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[parsed.Count];
                int i = 0;
                foreach (KeyValuePair<string, object> pair in parsed)
                {
                    if (pair.Value.GetType() == typeof(double))
                    {
                        // Some float values may come back as double, so convert them back to floats
                        tempPlayerPrefs[i] = new PlayerPrefPair() { Key = pair.Key, Value = (float)(double)pair.Value };
                    }
                    else
                    {
                        tempPlayerPrefs[i] = new PlayerPrefPair() { Key = pair.Key, Value = pair.Value };
                    }

                    i++;
                }

                // Return the results
                return tempPlayerPrefs;
            }
            else
            {
                // No existing player prefs saved (which is valid), so just return an empty array
                return new PlayerPrefPair[0];
            }
#else
                return null;

#endif
            }

            PlayerPrefPair[] GetAll_WIN_PlayerPrefPair()
            {

#if (UNITY_EDITOR_WIN)
                // From Unity docs: On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
#if UNITY_5_5_OR_NEWER
                // From Unity 5.5 editor player prefs moved to a specific location
                Microsoft.Win32.RegistryKey registryKey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Unity\\UnityEditor\\" + companyName + "\\" + productName);
#else
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\" + companyName + "\\" + productName);
#endif

            // Parse the registry if the specified registryKey exists
            if (registryKey != null)
            {
                // Get an array of what keys (registry value names) are stored
                string[] valueNames = registryKey.GetValueNames();

                // Create the array of the right size to take the saved player prefs
                PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[valueNames.Length];

                // Parse and convert the registry saved player prefs into our array
                int i = 0;
                foreach (string valueName in valueNames)
                {
                    string key = valueName;

                    // Remove the _h193410979 style suffix used on player pref keys in Windows registry
                    int index = key.LastIndexOf("_");
                    key = key.Remove(index, key.Length - index);

                    // Get the value from the registry
                    object ambiguousValue = registryKey.GetValue(valueName);

                    // Unfortunately floats will come back as an int (at least on 64 bit) because the float is stored as
                    // 64 bit but marked as 32 bit - which confuses the GetValue() method greatly! 
                    if (ambiguousValue.GetType() == typeof(int))
                    {
                        // If the player pref is not actually an int then it must be a float, this will evaluate to true
                        // (impossible for it to be 0 and -1 at the same time)
                        if (PlayerPrefs.GetInt(key, -1) == -1 && PlayerPrefs.GetInt(key, 0) == 0)
                        {
                            // Fetch the float value from PlayerPrefs in memory
                            ambiguousValue = PlayerPrefs.GetFloat(key);
                        }
                    }
                    else if (ambiguousValue.GetType() == typeof(byte[]))
                    {
                        // On Unity 5 a string may be stored as binary, so convert it back to a string
                        ambiguousValue = System.Text.Encoding.Default.GetString((byte[])ambiguousValue);
                    }

                    // Assign the key and value into the respective record in our output array
                    tempPlayerPrefs[i] = new PlayerPrefPair() { Key = key, Value = ambiguousValue };
                    i++;
                }

                // Return the results
                return tempPlayerPrefs;
            }
            else
            {
                // No existing player prefs saved (which is valid), so just return an empty array
                return new PlayerPrefPair[0];
            }
#else
                return null;
#endif

            }

        }


    }

}
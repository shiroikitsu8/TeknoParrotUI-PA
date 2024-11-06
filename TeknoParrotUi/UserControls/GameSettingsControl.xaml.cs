using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TeknoParrotUi.Common;
using TeknoParrotUi.Helpers;
using TeknoParrotUi.Views;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Data;
using System.Collections;

namespace TeknoParrotUi.UserControls
{
    /// <summary>
    /// Interaction logic for GameSettingsControl.xaml
    /// </summary>
    public partial class GameSettingsControl : UserControl
    {
        public GameSettingsControl()
        {
            InitializeComponent();
        }

        private GameProfile _gameProfile;
        private ListBoxItem _comboItem;
        private ContentControl _contentControl;
        private Library _library;
        private InputApi _inputApi = InputApi.DirectInput;
        private bool SubmissionNameBad;
        private string OldWindowed;
        private string OldWBorderless;

        public void LoadNewSettings(GameProfile gameProfile, ListBoxItem comboItem, ContentControl contentControl, Library library)
        {
            _gameProfile = gameProfile;
            _comboItem = comboItem;

            GamePathBox.Text = _gameProfile.GamePath;
            GamePathBox2.Text = _gameProfile.GamePath2;
            GamePathBox3.Text = _gameProfile.GamePath3;

            GameSettingsList.ItemsSource = gameProfile.ConfigValues;
            _contentControl = contentControl;
            _library = library;

            string exeName = "";

            if (!string.IsNullOrEmpty(_gameProfile.ExecutableName))
                exeName = $" ({_gameProfile.ExecutableName})".Replace(";", " or ");

            GameExecutableText.Text = $"Game Executable{exeName}:";

            if (_gameProfile.HasTwoExecutables)
            {
                exeName = "";

                if (!string.IsNullOrEmpty(_gameProfile.ExecutableName2))
                    exeName = $" ({_gameProfile.ExecutableName2})".Replace(";", " or ");

                GameExecutable2Text.Text = $"Second Game Executable{exeName}:";

                GameExecutable2Text.Visibility = Visibility.Visible;
                GamePathBox2.Visibility = Visibility.Visible;

                if (_gameProfile.HasThreeExecutables)
                {
                    exeName = "";

                    if (!string.IsNullOrEmpty(_gameProfile.ExecutableName3))
                        exeName = $" ({_gameProfile.ExecutableName3})".Replace(";", " or ");

                    GameExecutable3Text.Text = $"Third Game Executable{exeName}:";

                    GameExecutable3Text.Visibility = Visibility.Visible;
                    GamePathBox3.Visibility = Visibility.Visible;
                }
                else
                {
                    GameExecutable3Text.Visibility = Visibility.Collapsed;
                    GamePathBox3.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                GameExecutable2Text.Visibility = Visibility.Collapsed;
                GamePathBox2.Visibility = Visibility.Collapsed;

                GameExecutable3Text.Visibility = Visibility.Collapsed;
                GamePathBox3.Visibility = Visibility.Collapsed;
            }

            if (_gameProfile.EmulationProfile != EmulationProfile.NamcoWmmt3)
            {
                // Save Old Value
                if (!string.IsNullOrEmpty(_gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed").FieldValue))
                {
                    OldWindowed = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed").FieldValue;

                }
                if (!string.IsNullOrEmpty(_gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen").FieldValue))
                {
                    OldWBorderless = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen").FieldValue;
                }

                // Clear All Previous IP to prevent duplicate print
                _gameProfile.ConfigValues.Find(cv => cv.FieldName == "NetworkAdapterIP").FieldOptions.Clear();

                // Get Local Network, and Softether VPN IP for VS stuff
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                            (ni.Description.Contains("VPN Client Adapter - VPN") && ni.OperationalStatus == OperationalStatus.Up))
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                _gameProfile.ConfigValues.Find(cv => cv.FieldName == "NetworkAdapterIP").FieldOptions.Add(ip.Address.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void SelectExecutableForTextBox(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Title = Properties.Resources.GameSettingsSelectGameExecutable
            };

            if (!string.IsNullOrEmpty(_gameProfile.ExecutableName))
            {
                openFileDialog.Filter = $"{Properties.Resources.GameSettingsGameExecutableFilter} ({_gameProfile.ExecutableName})|{_gameProfile.ExecutableName}|All files (*.*)|*.*";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ((TextBox)sender).Text = openFileDialog.FileName;
            }
        }

        private void SelectExecutable2ForTextBox(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Title = Properties.Resources.GameSettingsSelectGameExecutable
            };

            if (!string.IsNullOrEmpty(_gameProfile.ExecutableName2))
            {
                openFileDialog.Filter = $"{Properties.Resources.GameSettingsGameExecutableFilter} ({_gameProfile.ExecutableName2})|{_gameProfile.ExecutableName2}|All files (*.*)|*.*";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ((TextBox)sender).Text = openFileDialog.FileName;
            }
        }

        private void SelectExecutable3ForTextBox(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Title = Properties.Resources.GameSettingsSelectGameExecutable
            };

            if (!string.IsNullOrEmpty(_gameProfile.ExecutableName3))
            {
                openFileDialog.Filter = $"{Properties.Resources.GameSettingsGameExecutableFilter} ({_gameProfile.ExecutableName3})|{_gameProfile.ExecutableName3}|All files (*.*)|*.*";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ((TextBox)sender).Text = openFileDialog.FileName;
            }
        }

        public static string Filter(string input, string[] badWords)
        {
            var re = new Regex(
                @"\b("
                + string.Join("|", badWords.Select(word =>
                    string.Join(@"\s*", word.ToCharArray())))
                + @")\b", RegexOptions.IgnoreCase);
            return re.Replace(input, match =>
            {
                return new string('*', match.Length);
            });
        }

        private void BtnSaveSettings(object sender, RoutedEventArgs e)
        {
            string inputApiString = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Input API")?.FieldValue;

            if (inputApiString != null)
                _inputApi = (InputApi)Enum.Parse(typeof(InputApi), inputApiString);

            foreach (var t in _gameProfile.JoystickButtons)
            {
                if (_inputApi == InputApi.DirectInput)
                    t.BindName = t.BindNameDi;
                else if (_inputApi == InputApi.XInput)
                    t.BindName = t.BindNameXi;
                else if (_inputApi == InputApi.RawInput)
                    t.BindName = t.BindNameRi;
            }

            string NameString = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Submission Name")?.FieldValue;

            if (NameString != null)
            {
                if (_gameProfile.ConfigValues.Any(x => x.FieldName == "Enable Submission (Patreon Only)" && x.FieldValue == "1"))
                {
                    bool CheckName = String.IsNullOrWhiteSpace(_gameProfile.ConfigValues.Find(cv => cv.FieldName == "Submission Name").FieldValue);
                    if (CheckName)
                    {
                        SubmissionNameBad = true;
                        MessageBox.Show("Score Submission requires a name!");
                    }
                    else
                        SubmissionNameBad = false;
                }
                else
                    SubmissionNameBad = false;

                string[] badWords = new[] { "fuck", "cunt", "fuckwit", "fag", "dick", "shit", "cock", "pussy", "ass", "asshole", "bitch", "homo", "faggot", "@ss", "f@g", "fucker", "fucking", "fuk", "fuckin", "fucken", "teknoparrot", "tp", "arse", "@rse", "@$$", "bastard", "crap", "effing", "god", "hell", "motherfucker", "whore", "twat", "gay", "g@y", "ash0le", "assh0le", "a$$hol", "anal", };

                NameString = Filter(NameString, badWords);
                _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Submission Name").FieldValue = NameString;
            }

            if (_gameProfile.EmulationProfile.ToString() != "NamcoWmmt3")
            {
                // Cannot enable both windowed and borderless feature
                string WindowedValue = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed")?.FieldValue;
                string BorderlessValue = _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen")?.FieldValue;

                if (WindowedValue == "1" && BorderlessValue == "1" && OldWindowed == "0" && OldWBorderless == "0")
                {
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed").FieldValue = "1";
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen").FieldValue = "0";
                }
                else if (BorderlessValue == "1" && OldWindowed == "1")
                {
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen").FieldValue = "1";
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed").FieldValue = "0";
                }
                else if (WindowedValue == "1" && OldWBorderless == "1")
                {
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Windowed").FieldValue = "1";
                    _gameProfile.ConfigValues.Find(cv => cv.FieldName == "Borderless Fullscreen").FieldValue = "0";
                }
            }


            if (!SubmissionNameBad)
            {
                JoystickHelper.SerializeGameProfile(_gameProfile);
                _gameProfile.GamePath = GamePathBox.Text;
                _gameProfile.GamePath2 = GamePathBox2.Text;
                _gameProfile.GamePath3 = GamePathBox3.Text;
                JoystickHelper.SerializeGameProfile(_gameProfile);
                _comboItem.Tag = _gameProfile;
                Application.Current.Windows.OfType<MainWindow>().Single().ShowMessage(string.Format(Properties.Resources.SuccessfullySaved, System.IO.Path.GetFileName(_gameProfile.FileName)));
                _library.ListUpdate(_gameProfile.GameName);
                _contentControl.Content = _library;
            }
        }

        private void BtnGoBack(object sender, RoutedEventArgs e)
        {
            // Reload library to discard changes
            _library.ListUpdate(_gameProfile.GameName);

            _contentControl.Content = _library;
        }
    }
}

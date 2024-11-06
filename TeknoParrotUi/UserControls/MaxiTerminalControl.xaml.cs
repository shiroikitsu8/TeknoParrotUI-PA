using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeknoParrotUi.Common;
using TeknoParrotUi.Helpers;
using TeknoParrotUi.Views;
using System.Text.RegularExpressions;
using System.IO;

namespace TeknoParrotUi.UserControls
{
    /// <summary>
    /// Interaction logic for MaxiTerminalControl.xaml
    /// </summary>
    public partial class MaxiTerminalControl : UserControl
    {
        public MaxiTerminalControl()
        {
            InitializeComponent();
        }

        private GameProfile _gameProfile;
        private ContentControl _contentControl;
        private Library _library;
        private string maxiTerminalPath;

        public void LoadNewSettings(GameProfile gameProfile, ContentControl contentControl, Library library)
        {
            _gameProfile = gameProfile;
            _contentControl = contentControl;
            _library = library;

            // Get Maxi Terminal Path
            maxiTerminalPath = _gameProfile.GamePath3;

            // Event Mode - Player
            playerChx.Items.Clear();
            playerChx.Items.Add(1);
            playerChx.Items.Add(2);
            playerChx.Items.Add(3);
            playerChx.Items.Add(4);

            bool checkNull = String.IsNullOrEmpty(maxiTerminalPath);
            if (checkNull == false)
            {
                playerChx.SelectedIndex = 0;

                maxiTerminalPath = maxiTerminalPath.ToLower().Replace("maxiterminal.exe", "config.json");

                if (File.Exists(maxiTerminalPath))
                {
                    string json = File.ReadAllText(maxiTerminalPath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    // Cost
                    freePlayChx.IsChecked = Convert.ToBoolean(Convert.ToInt32(jsonObj["freeplay"]));
                    cointChuteTxt.Text = jsonObj["coin_chute"];
                    buyCardTxt.Text = jsonObj["buycard_cost"];
                    continueTxt.Text = jsonObj["continue_cost"];
                    fullcourseTxt.Text = jsonObj["fullcourse_cost"];
                    gameCostTxt.Text = jsonObj["game_cost"];

                    // Event Mode
                    if (jsonObj["event_mode"] != 0)
                    {
                        eventModeChx.IsChecked = true;
                    }
                    tagteamChx.IsChecked = Convert.ToBoolean(Convert.ToInt32(jsonObj["event_2on2"]));
                    doubleDistChx.IsChecked = Convert.ToBoolean(Convert.ToInt32(jsonObj["event_double"]));
                    playerChx.SelectedValue = (int)jsonObj["event_mode_count"];
                    eventSerialTxt.Text = jsonObj["event_serial"];

                    // Feature Update
                    featureMonthTxt.Text = jsonObj["feature_month"];
                    featureYearTxt.Text = jsonObj["feature_year"];
                    featurePlusesTxt.Text = jsonObj["feature_pluses"];
                    featureReleaseAtTxt.Text = jsonObj["feature_release_at"];
                    packetIntervalTxt.Text = jsonObj["packet_interval"];
                }
            }
        }

        public static string IsNull(string str)
        {
            bool checkNull = String.IsNullOrEmpty(str);
            if (checkNull == false)
            {
                return str;
            }
            else
            {
                return "0";
            }
        }

        private void BtnSaveSettings(object sender, RoutedEventArgs e)
        {
            // If null, don't save
            bool checkNull = String.IsNullOrEmpty(maxiTerminalPath);
            if (checkNull == true)
            {
                MessageBoxHelper.ErrorOK("Saving Failed. Please Select MaxiTerminal Executable First in Game Settings");
                _contentControl.Content = _library;
            }
            else
            {
                string json = File.ReadAllText(maxiTerminalPath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                // Cost
                jsonObj["freeplay"] = (bool)freePlayChx.IsChecked ? "1" : "0";
                jsonObj["coin_chute"] = Int32.Parse(IsNull(cointChuteTxt.Text));
                jsonObj["buycard_cost"] = Int32.Parse(IsNull(buyCardTxt.Text));
                jsonObj["continue_cost"] = Int32.Parse(IsNull(continueTxt.Text));
                jsonObj["fullcourse_cost"] = Int32.Parse(IsNull(fullcourseTxt.Text));
                jsonObj["game_cost"] = Int32.Parse(IsNull(gameCostTxt.Text));

                // Event Mode
                jsonObj["event_mode"] = (bool)eventModeChx.IsChecked ? "2" : "0";
                jsonObj["event_2on2"] = (bool)tagteamChx.IsChecked ? "1" : "0";
                jsonObj["event_double"] = (bool)doubleDistChx.IsChecked ? "1" : "0";
                jsonObj["event_mode_count"] = IsNull(playerChx.Text);
                jsonObj["event_serial"] = IsNull(eventSerialTxt.Text);

                // Feature Update
                jsonObj["feature_month"] = IsNull(featureMonthTxt.Text);
                jsonObj["feature_year"] = IsNull(featureYearTxt.Text);
                jsonObj["feature_pluses"] = IsNull(featurePlusesTxt.Text);
                jsonObj["feature_release_at"] = IsNull(featureReleaseAtTxt.Text);
                jsonObj["packet_interval"] = IsNull(packetIntervalTxt.Text);

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(maxiTerminalPath, output);

                Application.Current.Windows.OfType<MainWindow>().Single().ShowMessage("Successfully Saved MaxiTerminal Config!");
                _contentControl.Content = _library;
            }
        }

        private void BtnGoBack(object sender, RoutedEventArgs e)
        {
            _contentControl.Content = _library;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
    }
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Transformers2_Configurator
{
    public partial class WndMain : Form
    {
        #region WIN32

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {

            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;

        }

        #endregion

        private const string GAMESETTINGS_INI_PATH = @"..\ShellData\GameSettings.ini";
        private const string SHELLDATA_INI_PATH =@"..\ShellData\ShellData.ini";
        private const string LAUNCHER_INI_PATH = @".\Transformers2_Launcher.ini";
        private INIFile _GameSettings_IniFile;
        private INIFile _ShellData_IniFile;
        private INIFile _Launcher_IniFile;
        
        public WndMain()
        {
            InitializeComponent();
            this.Text = "Transformers Shadow Rising - System Menu v" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            _GameSettings_IniFile = new INIFile(GAMESETTINGS_INI_PATH);
            _ShellData_IniFile = new INIFile(SHELLDATA_INI_PATH);
            _Launcher_IniFile = new INIFile(LAUNCHER_INI_PATH);

            ListAvailableScreenResolutions();
        }

        #region Resolution Listing

        private void ListAvailableScreenResolutions()
        {
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                string res = vDevMode.dmPelsWidth + "x" + vDevMode.dmPelsHeight;
                if (!CheckIfResolutionAlreadyExists(res))
                    Cbox_Resolution.Items.Add(res);
                i++;
            }
        }

        private bool CheckIfResolutionAlreadyExists(string Resolution)
        {
            for (int i = 0; i < Cbox_Resolution.Items.Count; i++)
            {
                if (Cbox_Resolution.Items[i].ToString().Equals(Resolution))
                    return true;
            }
            return false;
        }

        #endregion

        private void WndMain_Load(object sender, EventArgs e)
        {         
            DisplayDefaultValues();

            if (File.Exists(_GameSettings_IniFile.FInfo.FullName))
            {
                DisplayGameSettings();                
            }            
            else
            {
                MessageBox.Show("TEST MENU config file not found :\n\n" + _GameSettings_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (File.Exists(_ShellData_IniFile.FInfo.FullName))
            {
                DisplayShellSettings();
            }
            else
            {
                MessageBox.Show("SHELL config file not found :\n\n" + _ShellData_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (File.Exists(_Launcher_IniFile.FInfo.FullName))
            {
                DisplayLauncherSettings();
            }
            else
            {
                MessageBox.Show("LAUNCHER config file not found :\n\n" + _Launcher_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayDefaultValues()
        {
            DisplayComboBoxValue(Cbox_Language, Cbox_Language.Items[0].ToString());
            DisplayComboBoxValue(Cbox_Difficulty, Cbox_Difficulty.Items[2].ToString());
            DisplayComboBoxValue(Cbox_Advertise, Cbox_Advertise.Items[0].ToString());
            DisplayComboBoxValue(Cbox_Revival, Cbox_Revival.Items[0].ToString());
            DisplayComboBoxValue(Cbox_P1Recoil, Cbox_P1Recoil.Items[1].ToString());
            DisplayComboBoxValue(Cbox_P2Recoil, Cbox_P2Recoil.Items[1].ToString());
            DisplayComboBoxValue(Cbox_ContinueCountdown, Cbox_ContinueCountdown.Items[1].ToString());
            DisplayComboBoxValue(Cbox_EnnemyBoost, Cbox_EnnemyBoost.Items[0].ToString());
            DisplayComboBoxValue(Cbox_FirstMn, Cbox_FirstMn.Items[0].ToString());
            DisplayComboBoxValue(Cbox_KidsMode, Cbox_KidsMode.Items[0].ToString());
            DisplayComboBoxValue(Cbox_StageSelect, Cbox_StageSelect.Items[1].ToString());
            DisplayComboBoxValue(Cbox_EnglishSubtitles, Cbox_EnglishSubtitles.Items[1].ToString());
            DisplayComboBoxValue(Cbox_Swipe, Cbox_Swipe.Items[0].ToString());
            Cbox_FreePlay.Text = Cbox_FreePlay.Items[1].ToString();
            Cbox_EntryType.Text = Cbox_EntryType.Items[0].ToString();

            Cbox_Resolution.Text = Cbox_Resolution.Items[Cbox_Resolution.Items.Count - 1].ToString();
            Cbox_ScreenMode.Text = Cbox_ScreenMode.Items[1].ToString();
        }

        private void DisplayGameSettings()
        {
            try
            {
                DisplayComboBoxValue(Cbox_Language, _GameSettings_IniFile.IniReadValue("GameSettings", "LANGUAGE"));
                DisplayComboBoxValue(Cbox_Difficulty, _GameSettings_IniFile.IniReadValue("GameSettings", "GAME DIFFICULTY"));
                DisplayComboBoxValue(Cbox_Advertise, _GameSettings_IniFile.IniReadValue("GameSettings", "ADVERTISE SOUND"));
                DisplayComboBoxValue(Cbox_Revival, _GameSettings_IniFile.IniReadValue("GameSettings", "REVIVAL"));
                DisplayComboBoxValue(Cbox_P1Recoil, _GameSettings_IniFile.IniReadValue("GameSettings", "PLAYER1 CONTROLLER REACTION"));
                DisplayComboBoxValue(Cbox_P2Recoil, _GameSettings_IniFile.IniReadValue("GameSettings", "PLAYER2 CONTROLLER REACTION"));
                DisplayComboBoxValue(Cbox_ContinueCountdown, _GameSettings_IniFile.IniReadValue("GameSettings", "CONTINUE COUNTDOWN"));
                DisplayComboBoxValue(Cbox_EnnemyBoost, _GameSettings_IniFile.IniReadValue("GameSettings", "ENEMY BOOST"));
                DisplayComboBoxValue(Cbox_FirstMn, _GameSettings_IniFile.IniReadValue("GameSettings", "1ST MIN GAME PLAY"));
                DisplayComboBoxValue(Cbox_KidsMode, _GameSettings_IniFile.IniReadValue("GameSettings", "KIDS MODE"));
                DisplayComboBoxValue(Cbox_StageSelect, _GameSettings_IniFile.IniReadValue("GameSettings", "SELECT STAGE"));
                DisplayComboBoxValue(Cbox_EnglishSubtitles, _GameSettings_IniFile.IniReadValue("GameSettings", "ENGLISH SUBTITLES"));
                DisplayComboBoxValue(Cbox_Swipe, _GameSettings_IniFile.IniReadValue("GameSettings", "SWIPE CARD TO PLAY"));
            }
            catch (Exception Ex)
            {
                MessageBox.Show("GameSettings : Invalid value found: \n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayShellSettings()
        {
            try
            {
                int iIndex  = int.Parse(_ShellData_IniFile.IniReadValue("Credit", "Freeplay"));
                Cbox_FreePlay.Text = Cbox_FreePlay.Items[iIndex].ToString();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(SHELLDATA_INI_PATH + "\n: Invalid value found for Freeplay : \n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                int iIndex = int.Parse(_ShellData_IniFile.IniReadValue("Credit", "EntryType"));
                Cbox_EntryType.Text = Cbox_EntryType.Items[iIndex].ToString();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(SHELLDATA_INI_PATH + "\n: Invalid value found for EntryType: \n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayLauncherSettings()
        {
            try
            {
                //Resolution:
                string res = _Launcher_IniFile.IniReadValue("Video", "WIDTH") + "x" + _Launcher_IniFile.IniReadValue("Video", "HEIGHT");
                if (CheckIfResolutionAlreadyExists(res))
                    Cbox_Resolution.Text = res;
                //Mode
                int iIndex = int.Parse(_Launcher_IniFile.IniReadValue("Video", "FULLSCREEN"));
                Cbox_ScreenMode.Text = Cbox_ScreenMode.Items[iIndex].ToString();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Launcher Settings : Invalid value found: \n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayComboBoxValue(ComboBox Target, String sValue)
        {
            for (int i = 0; i < Target.Items.Count; i++)
            {
                if (Target.Items[i].ToString().Equals(sValue))
                {
                    Target.Text = sValue;
                    return;
                }
            }
            MessageBox.Show(GAMESETTINGS_INI_PATH + "\n: Invalid value found for " + Target.Name + " : \n\n" + sValue, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DisplayTrackBarValue(TrackBar Target, String sValue)
        {
            int iValue = 0;
            if (int.TryParse(sValue, out iValue))
            {
                if (iValue >= Target.Minimum && iValue <= Target.Maximum)
                {
                    Target.Value = iValue;
                }
                else
                {
                    MessageBox.Show(SHELLDATA_INI_PATH + " :\n" + sValue + " out of bound for " + Target.Name, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(SHELLDATA_INI_PATH + " :\n" + sValue + " is not valid value for " + Target.Name, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveGameSettings()
        {
            if (!_GameSettings_IniFile.FInfo.Exists)
                Directory.CreateDirectory(_GameSettings_IniFile.FInfo.Directory.FullName);

            try
            {
                _GameSettings_IniFile.IniWriteValue("GameSettings", "LANGUAGE", Cbox_Language.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "GAME DIFFICULTY", Cbox_Difficulty.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "ADVERTISE SOUND", Cbox_Advertise.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "REVIVAL", Cbox_Revival.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "PLAYER1 CONTROLLER REACTION", Cbox_P1Recoil.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "PLAYER2 CONTROLLER REACTION", Cbox_P2Recoil.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "CONTINUE COUNTDOWN", Cbox_ContinueCountdown.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "ENEMY BOOST", Cbox_EnnemyBoost.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "1ST MIN GAME PLAY", Cbox_FirstMn.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "KIDS MODE", Cbox_KidsMode.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "SELECT STAGE", Cbox_StageSelect.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "ENGLISH SUBTITLES", Cbox_EnglishSubtitles.Text);
                _GameSettings_IniFile.IniWriteValue("GameSettings", "SWIPE CARD TO PLAY", Cbox_Swipe.Text);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error saving GameSettings to disk : " + _GameSettings_IniFile.FInfo.FullName + "\n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show("GameSettings successfully saved to : \n\n" + _GameSettings_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void SaveShellSettings()
        {
            if (!_ShellData_IniFile.FInfo.Exists)
                Directory.CreateDirectory(_ShellData_IniFile.FInfo.Directory.FullName);

            try
            {
                _ShellData_IniFile.IniWriteValue("Credit", "Freeplay", Cbox_FreePlay.SelectedIndex.ToString());
                _ShellData_IniFile.IniWriteValue("Credit", "EntryType", Cbox_EntryType.SelectedIndex.ToString());
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error saving ShellSettings to disk : " + _ShellData_IniFile.FInfo.FullName + "\n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show("ShellData successfully saved to : \n\n" + _ShellData_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void SaveLauncherSettings()
        {
            if (!_Launcher_IniFile.FInfo.Exists)
                Directory.CreateDirectory(_Launcher_IniFile.FInfo.Directory.FullName);

            try
            {
                string[] sBuffer = Cbox_Resolution.Text.Split('x');
                _Launcher_IniFile.IniWriteValue("Video", "WIDTH", sBuffer[0]);
                _Launcher_IniFile.IniWriteValue("Video", "HEIGHT", sBuffer[1]);
                _Launcher_IniFile.IniWriteValue("Video", "FULLSCREEN", Cbox_ScreenMode.SelectedIndex.ToString());                
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error saving Launcher Settings to disk : " + _Launcher_IniFile.FInfo.FullName + "\n\n" + Ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show("Launcher Settings successfully saved to : \n\n" + _Launcher_IniFile.FInfo.FullName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            SaveGameSettings();
            SaveShellSettings();
            SaveLauncherSettings();
        }

        private void Btn_Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #region INI class

        public class INIFile
        {
            private string _RelativePath = string.Empty;
            public FileInfo FInfo { get; private set; }

            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

            public INIFile(string INIPath)
            {
                _RelativePath = INIPath;
                FInfo = new FileInfo(_RelativePath);
            }
            public long IniWriteValue(string Section, string Key, string Value)
            {
                return WritePrivateProfileString(Section, Key, Value, this._RelativePath);
            }

            public string IniReadValue(string Section, string Key)
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, "", temp, 255, this._RelativePath);
                return temp.ToString();
            }
        }

        #endregion



    }
}

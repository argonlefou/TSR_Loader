using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Debugger;

namespace Transformers2_Launcher
{
    public class Transformers2_Launcher
    {
        private const string MEMORY_DATA_FOLDER = "MemoryData";
        private const string TARGET_EXE_NAME = @"Transformers2.exe";

        private int _ProcessId = 0;
        private Process _Process;
        private IntPtr _Process_MemoryBaseAddress = IntPtr.Zero;
        private IntPtr _ProcessHandle = IntPtr.Zero;

        //Memory Hacks
        private UInt32 _DisableCShell_Offset = 0x001E70E4;
        private UInt32 _RestoreHiddenConfig_Offset = 0x00240C80;
        private UInt32 _SetWindowedMode_Offset = 0x002409F5;
        private UInt32 _RemoveOriginalWindowedMode_Offset = 0x00240A1C;
        private UInt32 _ForceResolutionIndex_Offset = 0x00260AD5;
        private UInt32 _ResolutionTableHd_Offset = 0x008A5008;
        private UInt32 _DisableLEDBoardCreation_Offset = 0x002EB93C;
        private UInt32 _DisableSAEBoardCreation_Offset = 0x0017A4A2;
        private UInt32 _CGunMgrForceInputMouse_Offset1 = 0x000F54F9;
        private UInt32 _CGunMgrForceInputMouse_Offset2 = 0x000F551C;
        private UInt32 _CGunMgrForceInputMouse_Offset3 = 0x000F5534;

        //MD5 check of target binaries, may help to know if it's the wrong version or not compatible
        protected Dictionary<string, string> _KnownMd5Prints;
        protected String _TargetProcess_Md5Hash = string.Empty;

        //Config values
        private const string LAUNCHER_INI_PATH = @".\Transformers2_Launcher.ini";
        private INIFile _Launcher_IniFile;
        private UInt32 _Cfg_ScreenWidth = 0;
        private UInt32 _Cfg_ScreenHeight = 0;
        private byte _Cfg_Windowed = 0;

        Debugger.QuickDebugger _Qdb;

        public Transformers2_Launcher(bool EnableLogs)
        {
            Logger.InitLogFileName();
            Logger.IsEnabled = EnableLogs;

            _Launcher_IniFile = new INIFile(AppDomain.CurrentDomain.BaseDirectory + LAUNCHER_INI_PATH);

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + TARGET_EXE_NAME))
            {
                Logger.IsEnabled = true;
                Logger.WriteLog("Transformers2_Launcher() => Transformers2.exe not found. Abording...");
                Environment.Exit(0);
            }

            if (!File.Exists(_Launcher_IniFile.FInfo.FullName))
            {
                Logger.IsEnabled = true;
                Logger.WriteLog("Transformers2_Launcher() => No config file found. Abording...");
                Environment.Exit(0);
            }

            try
            {
                _Cfg_ScreenWidth = UInt32.Parse(_Launcher_IniFile.IniReadValue("Video", "WIDTH"));
                _Cfg_ScreenHeight = UInt32.Parse(_Launcher_IniFile.IniReadValue("Video", "HEIGHT"));
                if (_Launcher_IniFile.IniReadValue("Video", "FULLSCREEN").Equals("0"))
                    _Cfg_Windowed = 1;
                else
                    _Cfg_Windowed = 0;
            }
            catch (Exception Ex)
            {
                Logger.IsEnabled = true;
                Logger.WriteLog("Transformers2_Launcher() =>  Error reading config file : " + _Launcher_IniFile.FInfo.FullName);
                Logger.WriteLog(Ex.Message.ToString());
                Logger.WriteLog("Abording...");
                Environment.Exit(0);
            }

            _KnownMd5Prints = new Dictionary<String, String>();
            _KnownMd5Prints.Add("Transformers Shadow Rising v180605 - Original Dump", "b3b1f4ad6408d6ee946761a00f761455");
        }

        /// <summary>
        /// Create the process with DEBUG attributes, so that we can stop it to inject the code, and then resume it
        /// Inspired from a hand-made debugger https://www.codeproject.com/Articles/43682/Writing-a-basic-Windows-debugger
        /// </summary>
        public void RunGame()
        {
            _Qdb = new QuickDebugger(AppDomain.CurrentDomain.BaseDirectory + TARGET_EXE_NAME);
            _Qdb.OnDebugEvent += new Debugger.QuickDebugger.DebugEventHandler(Qdb_OnDebugEvent);
            _Qdb.StartProcess();
        }
        private void Qdb_OnDebugEvent(object sender, Debugger.DebugEventArgs e)
        {
            switch (e.Dbe.dwDebugEventCode)
            {
                case DebugEventType.CREATE_PROCESS_DEBUG_EVENT:
                    {
                        Logger.WriteLog("RunGame() => Process created");
                        _Qdb.ContinueDebugEvent();
                    } break;


                case DebugEventType.CREATE_THREAD_DEBUG_EVENT:
                    {
                        DEBUG_EVENT.CREATE_THREAD_DEBUG_INFO ti = new DEBUG_EVENT.CREATE_THREAD_DEBUG_INFO();
                        ti = e.Dbe.CreateThread;
                        Logger.WriteLog("Thread 0x" + ti.hThread.ToString("X8") + " (Id: " + e.Dbe.dwThreadId.ToString() + ") created");
                        _Qdb.ContinueDebugEvent();
                    } break;


                //The game has a breakpoint installed at start (!), we can use it to search for information, block the process to insert our code
                case DebugEventType.EXCEPTION_DEBUG_EVENT:
                    {
                        DEBUG_EVENT.EXCEPTION_DEBUG_INFO Ex = new DEBUG_EVENT.EXCEPTION_DEBUG_INFO();
                        Ex = e.Dbe.Exception;

                        if (Ex.ExceptionRecord.ExceptionCode == QuickDebugger.STATUS_BREAKPOINT)
                        {
                            Logger.WriteLog("RunGame() => Breakpoint reached !");
                            Process p = Process.GetProcessById(e.Dbe.dwProcessId);
                            _Process = p;
                            _ProcessId = _Process.Id;
                            _ProcessHandle = _Process.Handle;
                            _Process_MemoryBaseAddress = _Process.MainModule.BaseAddress;
                            Logger.WriteLog("RunGame() => Process ID: " + _ProcessId.ToString());
                            Logger.WriteLog("RunGame() => Process Memory Base Address: 0x" + _Process_MemoryBaseAddress.ToString("X8"));
                            Logger.WriteLog("RunGame() => Process Handle: 0x" + _ProcessHandle.ToString("X8"));

                            CheckExeMd5();
                            ReadGameDataFromMd5Hash();
                            Apply_Hacks();

                            Logger.WriteLog("RunGame() => Hack complete, leaving the game to run on its own now....");
                            _Qdb.DetachDebugger();
                            _Qdb.ContinueDebugEvent();
                        }
                    } break;

                default:
                    {
                        _Qdb.ContinueDebugEvent();
                    } break;
            }
        }

        /// <summary>
        /// Creating the Process without DEBUG attributes can allow another debugger to go in and analyse what's going on
        /// </summary>
        public void Run_Game_Debug()
        {
            FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + TARGET_EXE_NAME);

            _Process = new Process();
            _Process.StartInfo.FileName = fi.FullName;
            _Process.Start();

            try
            {
                ProcessTools.SuspendProcess(_Process);
                _ProcessId = _Process.Id;
                _ProcessHandle = _Process.Handle;
                _Process_MemoryBaseAddress = _Process.MainModule.BaseAddress;
                Apply_Hacks();

                ProcessTools.ResumeProcess(_Process);
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception)
            {
            }
        }

        #region Hacks

        public void Apply_Hacks()
        {
            //Disabling the CShell necessity, if not the game is shutting down if no correct Keep-alive reply from the Shell.exe program
            WriteByte((UInt32)_Process_MemoryBaseAddress + _DisableCShell_Offset, 0x00);

            //GetConfigIniDir() function is stripped. Putting back a fixed name (./App.ini) to restore the possibility to change some settings
            WriteBytes((UInt32)_Process_MemoryBaseAddress + _RestoreHiddenConfig_Offset, new byte[] { 0xB8, 0x86, 0x0C, 0x64, 0x00, 0xC3, 0x2E, 0x2F, 0x41, 0x70, 0x70, 0x2E, 0x69, 0x6E, 0x69, 0x00 });

            //LED Board is connected through COM port
            //Setting COM access result to (-1) instead of trying to open COM port for real
            WriteBytes((UInt32)_Process_MemoryBaseAddress + _DisableLEDBoardCreation_Offset, new byte[] { 0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 0xEB, 0x1D, 0x90, 0x90, 0x90, 0x90 });

            //Force not to create SAEBoard
            WriteByte((UInt32)_Process_MemoryBaseAddress + _DisableSAEBoardCreation_Offset, 0xEB);

            //CGunMgr() init input mode by checking first if JVS is enabled, forcing it to false
            WriteByte((UInt32)_Process_MemoryBaseAddress + _CGunMgrForceInputMouse_Offset1, 0xEB);
            //Then checking if SAEBoard is active, forcing it to false also
            WriteByte((UInt32)_Process_MemoryBaseAddress + _CGunMgrForceInputMouse_Offset2, 0xEB);
            //Finally forcing input mode to '1' (mouse) for P2 (already 1 for P1 normally)
            WriteByte((UInt32)_Process_MemoryBaseAddress + _CGunMgrForceInputMouse_Offset3, 0x1);

            //Replacing the original default value for Windowed mode by our own 
            WriteByte((UInt32)_Process_MemoryBaseAddress + _SetWindowedMode_Offset, _Cfg_Windowed);
            //Disabling the default Windowed mode set after reading the "hidden" config file
            WriteBytes((UInt32)_Process_MemoryBaseAddress + _RemoveOriginalWindowedMode_Offset, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            //Screen Size Hack :
            //First step is to force the game to choose index 0xF in the resolution table, whatever SWITCH or option is used
            WriteBytes((UInt32)_Process_MemoryBaseAddress + _ForceResolutionIndex_Offset, new byte[] { 0x90, 0x90, 0xB8, 0x0F });
            //Second step is to replace Width and Height values for the desires Resolution by our own
            for (uint i = 0; i < 3; i++)
            {
                WriteBytes((UInt32)_Process_MemoryBaseAddress + _ResolutionTableHd_Offset + (i * 8), BitConverter.GetBytes(_Cfg_ScreenWidth));
                WriteBytes((UInt32)_Process_MemoryBaseAddress + _ResolutionTableHd_Offset + (i * 8) + 4, BitConverter.GetBytes(_Cfg_ScreenHeight));
            }

            // Not needed
            //Credits force value to 0 instead of -1 (-1 won't update value)    ?????? to confirm when set_credit hack is done
            //WriteByte((UInt32)_Process_MemoryBaseAddress + 0x66953, 0x00);

        }

        #endregion

        #region MD5 Verification

        /// <summary>
        /// Compute the MD5 hash of the target executable and compare it to the known list of MD5 Hashes
        /// This can be usefull if people are using some unknown dump with different memory, 
        /// or a wrong version of emulator
        /// This is absolutely not blocking, just for debuging with output log
        /// </summary>
        protected void CheckExeMd5()
        {
            CheckMd5(_Process.MainModule.FileName);
        }
        protected void CheckMd5(String TargetFileName)
        {
            GetMd5HashAsString(TargetFileName);
            Logger.WriteLog("CheckMd5() => MD5 hash of " + TargetFileName + " = " + _TargetProcess_Md5Hash);

            String FoundMd5 = String.Empty;
            foreach (KeyValuePair<String, String> pair in _KnownMd5Prints)
            {
                if (pair.Value == _TargetProcess_Md5Hash)
                {
                    FoundMd5 = pair.Key;
                    break;
                }
            }

            if (FoundMd5 == String.Empty)
            {
                Logger.WriteLog(@"CheckMd5() => /!\ MD5 Hash unknown, the mod may not work correctly with this target /!\");
            }
            else
            {
                Logger.WriteLog("CheckMd5() => MD5 Hash is corresponding to a known target = " + FoundMd5);
            }

        }

        /// <summary>
        /// Compute the MD5 hash from the target file.
        /// </summary>
        /// <param name="FileName">Full  filepath of the targeted executable.</param>
        private void GetMd5HashAsString(String FileName)
        {
            if (File.Exists(FileName))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(FileName))
                    {
                        var hash = md5.ComputeHash(stream);
                        _TargetProcess_Md5Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }

        #endregion

        #region MemoryData Loading

        /// <summary>
        /// Read memory values in .cfg file, whose name depends on the MD5 hash of the targeted exe.
        /// Mostly used for PC games
        /// </summary>
        /// <param name="GameData_Folder"></param>
        protected virtual void ReadGameDataFromMd5Hash()
        {
            String ConfigFile = AppDomain.CurrentDomain.BaseDirectory + MEMORY_DATA_FOLDER + @"\" + _TargetProcess_Md5Hash + ".cfg";
            if (File.Exists(ConfigFile))
            {
                Logger.WriteLog("ReadGameDataFromMd5Hash() => Reading game memory setting from " + ConfigFile);
                using (StreamReader sr = new StreamReader(ConfigFile))
                {
                    String line;
                    String FieldName = String.Empty;
                    line = sr.ReadLine();
                    while (line != null)
                    {
                        String[] buffer = line.Split('=');
                        if (buffer.Length > 1)
                        {
                            try
                            {
                                FieldName = "_" + buffer[0].Trim();
                                if (buffer[0].Contains("Nop"))
                                {
                                    NopStruct n = new NopStruct(buffer[1].Trim());
                                    this.GetType().GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).SetValue(this, n);
                                    Logger.WriteLog(FieldName + " successfully set to following NopStruct : 0x" + n.MemoryOffset.ToString("X8") + "|" + n.Length.ToString());
                                }
                                else if (buffer[0].Contains("Injection"))
                                {
                                    InjectionStruct i = new InjectionStruct(buffer[1].Trim());
                                    this.GetType().GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).SetValue(this, i);
                                    Logger.WriteLog(FieldName + " successfully set to following InjectionStruct : 0x" + i.Injection_Offset.ToString("X8") + "|" + i.Length.ToString());
                                }
                                else
                                {
                                    UInt32 v = UInt32.Parse(buffer[1].Substring(3).Trim(), NumberStyles.HexNumber);
                                    this.GetType().GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).SetValue(this, v);
                                    Logger.WriteLog(FieldName + " successfully set to following value : 0x" + v.ToString("X8"));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteLog("ReadGameDataFromMd5Hash() => Error reading game data for " + FieldName + " : " + ex.Message.ToString());
                            }
                        }
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
            }
            else
            {
                Logger.WriteLog("ReadGameDataFromMd5Hash() => Memory File not found : " + ConfigFile);
            }
        }

        #endregion

        #region Memory Hack x86

        /// <summary>
        /// Defines how many NOP to write at a given Memory offset
        /// </summary>
        public struct NopStruct
        {
            public UInt32 MemoryOffset;
            public UInt32 Length;

            public NopStruct(UInt32 Offset, UInt32 NopLength)
            {
                MemoryOffset = Offset;
                Length = NopLength;
            }

            public NopStruct(String OffsetAndNumber)
            {
                MemoryOffset = 0;
                Length = 0;
                if (OffsetAndNumber != null)
                {
                    try
                    {
                        Length = UInt32.Parse((OffsetAndNumber.Split('|'))[1]);
                        MemoryOffset = UInt32.Parse((OffsetAndNumber.Split('|'))[0].Substring(2).Trim(), System.Globalization.NumberStyles.HexNumber);
                    }
                    catch
                    {
                        Logger.WriteLog("Impossible to load NopStruct from following String : " + OffsetAndNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Defines an injection Memory zone and it's length
        /// </summary>
        public struct InjectionStruct
        {
            public UInt32 Injection_Offset;
            public UInt32 Injection_ReturnOffset;
            public UInt32 Length;

            public InjectionStruct(UInt32 Offset, UInt32 InjectionLength)
            {
                Injection_Offset = Offset;
                Length = InjectionLength;
                Injection_ReturnOffset = Offset + Length;
            }

            public InjectionStruct(String OffsetAndNumber)
            {
                Injection_Offset = 0;
                Length = 0;
                Injection_ReturnOffset = 0;
                if (OffsetAndNumber != null)
                {
                    try
                    {
                        Length = UInt32.Parse((OffsetAndNumber.Split('|'))[1]);
                        Injection_Offset = UInt32.Parse((OffsetAndNumber.Split('|'))[0].Substring(2).Trim(), System.Globalization.NumberStyles.HexNumber);
                        Injection_ReturnOffset = Injection_Offset + Length;
                    }
                    catch
                    {
                        Logger.WriteLog("Impossible to load InjectionStruct from following String : " + OffsetAndNumber);
                    }
                }
            }
        }

        protected Byte ReadByte(UInt32 Address)
        {
            byte[] Buffer = { 0 };
            UInt32 bytesRead = 0;
            if (!Win32API.ReadProcessMemory(_ProcessHandle, Address, Buffer, 1, ref bytesRead))
            {
                Logger.WriteLog("Cannot read memory at address 0x" + Address.ToString("X8"));
            }
            return Buffer[0];
        }

        protected Byte[] ReadBytes(UInt32 Address, UInt32 BytesCount)
        {
            byte[] Buffer = new byte[BytesCount];
            UInt32 bytesRead = 0;
            if (!Win32API.ReadProcessMemory(_ProcessHandle, Address, Buffer, (UInt32)Buffer.Length, ref bytesRead))
            {
                Logger.WriteLog("Cannot read memory at address 0x" + Address.ToString("X8"));
            }
            return Buffer;
        }

        protected UInt32 ReadPtr(UInt32 PtrAddress)
        {
            byte[] Buffer = ReadBytes(PtrAddress, 4);
            return BitConverter.ToUInt32(Buffer, 0);
        }

        protected UInt32 ReadPtrChain(UInt32 BaseAddress, UInt32[] Offsets)
        {
            byte[] Buffer = ReadBytes(BaseAddress, 4);
            UInt32 Ptr = BitConverter.ToUInt32(Buffer, 0);

            if (Ptr == 0)
            {
                return 0;
            }
            else
            {
                for (int i = 0; i < Offsets.Length; i++)
                {
                    Buffer = ReadBytes(Ptr + Offsets[i], 8);
                    Ptr = BitConverter.ToUInt32(Buffer, 0);

                    if (Ptr == 0)
                        return 0;
                }
            }

            return Ptr;
        }

        protected bool WriteByte(UInt32 Address, byte Value)
        {
            UInt32 bytesWritten = 0;
            Byte[] Buffer = { Value };
            if (Win32API.WriteProcessMemory(_ProcessHandle, Address, Buffer, 1, ref bytesWritten))
            {
                if (bytesWritten == 1)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        protected bool WriteBytes(UInt32 Address, byte[] Buffer)
        {
            UInt32 bytesWritten = 0;
            if (Win32API.WriteProcessMemory(_ProcessHandle, Address, Buffer, (UInt32)Buffer.Length, ref bytesWritten))
            {
                if (bytesWritten == Buffer.Length)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        protected void SetNops(UInt32 BaseAddress, NopStruct Nop)
        {
            for (UInt32 i = 0; i < Nop.Length; i++)
            {
                UInt32 Address = (UInt32)BaseAddress + Nop.MemoryOffset + i;
                if (!WriteByte(Address, 0x90))
                {
                    Logger.WriteLog("Impossible to NOP address 0x" + Address.ToString("X8"));
                    break;
                }
            }
        }

        #endregion    
    }
}

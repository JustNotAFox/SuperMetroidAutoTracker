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
            "snes9x-x64"
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
                    { 6856704, 0x78528C } //1.56.2
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
            return versions[proc.ProcessName][proc.MainModule.ModuleMemorySize];
        }

        static void PrintYN(bool yes)
        {
            if(yes)
            {
                Console.WriteLine("Yes");
            }
            else
            {
                Console.WriteLine("No");
            }
        }

        public int getBytes(int offset, byte[] buffer, int size)
        {
            if(this.process.HasExited)
            {
                this.process = null;
                this.init();
            }
            int ret = 0;
            bool read = ReadProcessMemory(this.handle, this.baseOffset + offset, buffer, new IntPtr(size), out ret);
            if(read)
            {
                return ret;
            }
            return 0;
        }

        private void init()
        {
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

		/*static void Main(string[] args)
		{
            while (true)
            {
                Process process = findProcess();
                process = null;
                while (process == null) //ensure that a process is found before continuing
                {
                    process = findProcess();
                }
                IntPtr handle = OpenProcess(0x10, false, new IntPtr(process.Id)); //get a handle to the process, read only
                if (handle == null) //check if the handle was obtained properly
                {
                    Console.WriteLine("Unable to get handle.");
                }
                else //handle obtained, start data
                {
                    byte[] buffer = new byte[4];
                    int bytesRead = 0;
                    Int64 offset = getAddress(process);
                    bool works = ReadProcessMemory(handle, offset, buffer, new IntPtr(4), out bytesRead);
                    if (works)
                    {
                        Console.WriteLine("Memory read: " + BitConverter.ToString(buffer));
                        int ramOffset = BitConverter.ToInt32(buffer, 0);
                        while (works)
                        {
                            Console.Clear();
                            works = ReadProcessMemory(handle, ramOffset + 0x09C2, buffer, new IntPtr(2), out bytesRead);
                            if (works)
                            {
                                Console.WriteLine("Current Health: " + (buffer[1] * 256 + buffer[0]));
                                Console.WriteLine("Identifiable by Memory? - " + process.MainModule.ModuleMemorySize);
                            }
                            works = ReadProcessMemory(handle, ramOffset + 0x09A4, buffer, new IntPtr(2), out bytesRead);
                            if (works)
                            {
                                bool varia = (buffer[0] & 0x1) != 0;
                                bool spring = (buffer[0] & 0x2) != 0;
                                bool morph = (buffer[0] & 0x4) != 0;
                                bool screw = (buffer[0] & 0x8) != 0;
                                bool grav = (buffer[0] & 0x20) != 0;
                                bool hjb = (buffer[1] & 0x1) != 0;
                                bool space = (buffer[1] & 0x2) != 0;
                                bool bomb = (buffer[1] & 0x10) != 0;
                                bool speed = (buffer[1] & 0x20) != 0;
                                bool grapple = (buffer[1] & 0x40) != 0;
                                bool xray = (buffer[1] & 0x80) != 0;
                                Console.Write("Collected Varia Suit: ");
                                PrintYN(varia);
                                Console.Write("Collected Spring Ball: ");
                                PrintYN(spring);
                                Console.Write("Collected Morph Ball: ");
                                PrintYN(morph);
                                Console.Write("Collected Screw Attack: ");
                                PrintYN(screw);
                                Console.Write("Collected Gravity Suit: ");
                                PrintYN(grav);
                                Console.Write("Collected Hi-Jump Boots: ");
                                PrintYN(hjb);
                                Console.Write("Collected Space Jump: ");
                                PrintYN(space);
                                Console.Write("Collected Bomb: ");
                                PrintYN(bomb);
                                Console.Write("Collected Speed Booster: ");
                                PrintYN(speed);
                                Console.Write("Collected Grappling Beam: ");
                                PrintYN(grapple);
                                Console.Write("Collected X-Ray Scope: ");
                                PrintYN(xray);
                            }
                            works = ReadProcessMemory(handle, ramOffset + 0x09A8, buffer, new IntPtr(2), out bytesRead);
                            if (works)
                            {
                                bool wave = (buffer[0] & 0x1) != 0;
                                bool ice = (buffer[0] & 0x2) != 0;
                                bool spazer = (buffer[0] & 0x4) != 0;
                                bool plasma = (buffer[0] & 0x8) != 0;
                                bool charge = (buffer[1] & 0x10) != 0;
                                Console.Write("Collected Wave Beam: ");
                                PrintYN(wave);
                                Console.Write("Collected Ice Beam: ");
                                PrintYN(ice);
                                Console.Write("Collected Spazer: ");
                                PrintYN(spazer);
                                Console.Write("Collected Plasma Beam: ");
                                PrintYN(plasma);
                                Console.Write("Collected Charge Beam: ");
                                PrintYN(charge);
                            }
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
            }
		}*/
	}
}
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace AutoTracker
{
	class Program
	{
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
			IntPtr NumberofBytesRead
		);

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

		static void Main(string[] args)
		{
			Process[] processes = Process.GetProcessesByName("snes9x-x64");

			for(int i = 0; i < processes.Length; i++)
			{
				Console.WriteLine(processes[i].ProcessName);
				IntPtr handle = OpenProcess(0x10, false, new IntPtr(processes[i].Id));
				if (handle == null)
				{
					Console.WriteLine("Open process is not working");
				}
				else
				{
					byte[] buffer = new byte[4];
					IntPtr bytesRead = new IntPtr(0);
                    Int64 offset = 0x1404DAF18;
					bool works = ReadProcessMemory(handle, offset, buffer, new IntPtr(4), bytesRead);
					if(works)
					{
						Console.WriteLine("Memory read: " + BitConverter.ToString(buffer));
                        int ramOffset = BitConverter.ToInt32(buffer, 0);
                        while(true)
                        {
                            Console.Clear();
                            works = ReadProcessMemory(handle, ramOffset+0x09C2, buffer, new IntPtr(2), bytesRead);
                            if(works)
                            {
                                Console.WriteLine("Current Health: " + (buffer[1] * 256 + buffer[0]));
                            }
                            works = ReadProcessMemory(handle, ramOffset + 0x09A4, buffer, new IntPtr(2), bytesRead);
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
                            System.Threading.Thread.Sleep(1000);
                        }
					}
				}
			}
		}
	}
}

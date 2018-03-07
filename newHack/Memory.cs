using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace newHack
{
    public class Memory
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte lpBuffer, int nSize, int lpNumberOfBytesWritten);

        private Process process;
        public IntPtr processHandle;

        static int DELETE = 0x00010000;
        static int READ_CONTROL = 0x00020000;
        static int WRITE_DAC = 0x00040000;
        static int WRITE_OWNER = 0x00080000;
        static int SYNCHRONIZE = 0x00100000;

        static int END = 0xFFF;

        public static int PROCESS_ALL_ACCESS = (DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END);

        public Memory(string process)
        {
            PROCESS_ALL_ACCESS = (DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END);
            var p = Process.GetProcessesByName(process);
            if (p.Length > 0)
                this.process = p[0];
        }

        public int Read(IntPtr pointer)
        {
            if (this.processHandle == IntPtr.Zero)
            {
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            }
            int numRead = 0;
            byte[] buffer = new byte[4];
            ReadProcessMemory(processHandle, pointer, buffer, buffer.Length, ref numRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public byte[] Read(IntPtr pointer, uint Size)
        {
            if (this.processHandle == IntPtr.Zero)
            {
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            }
            int numRead = 0;
            byte[] buffer = new byte[Size];
            ReadProcessMemory(processHandle, pointer, buffer, buffer.Length, ref numRead);
            return buffer;
        }

        public byte[] ReadMatrix(int pointer, int rows, int columns)
        {
            if (this.processHandle == IntPtr.Zero)
            {
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            }
            int length = sizeof(float) * rows * columns;
            byte[] data = new byte[length];
            int read = 0;
            ReadProcessMemory(processHandle, (IntPtr)pointer, data, length, ref read);
            return data;
        }
    }
}

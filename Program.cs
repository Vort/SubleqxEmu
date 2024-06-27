using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubleqxEmu
{
    class BitStream
    {
        int bitIndex = 0;
        int byteIndex = 0;
        public byte[] Data;

        public ulong ReadValue(int size)
        {
            ulong value = 0;
            for (int i = 0; i < size; i++)
            {
                value += value;
                if ((Data[byteIndex] >> bitIndex & 1) != 0)
                    value++;
                bitIndex++;
                if (bitIndex > 7)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return value;
        }

        public void WriteValue(long value, int size)
        {
            if (value >= 1L << size)
                throw new Exception();
            for (int i = size - 1; i > -1; i--)
            {
                int bit = 1 << bitIndex;
                Data[byteIndex] &= (byte)~bit;
                if ((value & 1 << i) != 0)
                    Data[byteIndex] |= (byte)bit;
                bitIndex++;
                if (bitIndex > 7)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
        }

        public void Seek(int address)
        {
            byteIndex = address / 8;
            bitIndex = address % 8;
        }

        public ulong Position()
        {
            return (ulong)(byteIndex * 8 + bitIndex);
        }
    }

    class Program
    {
        const uint memorySizeBytes = 128 * 1024;
        const uint memorySizeBits = memorySizeBytes * 8;

        BitStream bs;

        int GetSizeLog(int value)
        {
            int result = 0;
            int thr = 1;
            for (int i = 1; i < 32; i++)
            {
                if (value > thr)
                    result = i;
                thr <<= 1;
            }
            return result;
        }

        string GetZeroes(int count)
        {
            return new string('0', count);
        }

        string GetBinary(int value)
        {
            return Convert.ToString(value, 2);
        }

        int ReadGammaCode()
        {
            int zeroCount = 0;
            while (bs.ReadValue(1) == 0)
            {
                zeroCount++;
                if (zeroCount > 30)
                    throw new Exception();
            }
            return (1 << zeroCount) | (int)bs.ReadValue(zeroCount);
        }

        void Emulate(string inputFile)
        {
            byte[] memory = new byte[memorySizeBytes];
            byte[] fileData = File.ReadAllBytes(inputFile);
            Array.Copy(fileData, memory, fileData.Length);

            bs = new BitStream();
            bs.Data = memory;

            Console.WriteLine($"Program '{Path.GetFileName(inputFile)}' is loaded ({fileData.Length} bytes)");

            bool halt = false;
            int operationCount = 0;

            for (ulong ip = 0; ;)
            {
                if (ip >= memorySizeBits)
                    throw new Exception();
                bs.Seek((int)ip);

                int aw = ReadGammaCode();
                int dwm1w = ReadGammaCode();

                if (aw > 64)
                    throw new Exception($"Address width {aw} support is not implemented");
                if (dwm1w > 64)
                    throw new Exception($"dwm1w {dwm1w} support is not implemented");
                ulong dwm1 = bs.ReadValue(dwm1w);
                if (dwm1 > 63)
                    throw new Exception($"Data width {dwm1 + 1} support is not implemented");

                int dw = (int)dwm1 + 1;
                ulong pa = bs.ReadValue(aw);
                ulong pb = bs.ReadValue(aw);
                ulong pc = bs.ReadValue(aw);

                ulong nextInstruction = bs.Position();

                Func<ulong, ulong> read = addr => {
                    if (addr == 1)
                    {
                        halt = true;
                        return 0;
                    }
                    else if (addr == 2)
                    {
                        return (ulong)Console.Read();
                    }
                    else if (addr == 3)
                    {
                        return 0;
                    }
                    if (addr >= memorySizeBits)
                        throw new Exception();
                    bs.Seek((int)addr);
                    return bs.ReadValue(dw);
                };

                int sxs = 64 - dw;
                long a = (long)read(pa) << sxs >> sxs;
                long b = (long)read(pb) << sxs >> sxs;

                if (halt)
                    break;

                unchecked { b = b - a; }
                if (pb == 0 || pb > 3)
                {
                    bs.Seek((int)pb);
                    bs.WriteValue(b, dw);
                }
                else if (pb == 3)
                {
                    Console.Write((char)((ulong)b << sxs >> sxs));
                }

                if (b <= 0)
                    ip = pc;
                else
                    ip = nextInstruction;

                operationCount++;
            }

            if (Console.CursorLeft != 0)
                Console.WriteLine();

            Console.WriteLine($"Operation count: {operationCount}");
        }

        static void ShowUsage()
        {
            Console.WriteLine(
                "usage: SubleqxEmu program.bin");
        }

        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Globalization.CultureInfo.InvariantCulture;

            if (args.Length != 1)
            {
                ShowUsage();
                return;
            }

            string inputFile = args[0];
            new Program().Emulate(inputFile);
        }
    }
}

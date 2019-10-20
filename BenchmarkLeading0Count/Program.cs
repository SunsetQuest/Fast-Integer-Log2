// Created by Ryan S. White (sunsetquest) on 10/13/2019
// Sharing under the MIT License 
// Goals: 
//   (1) benchmark a large set of known methods
//   (2) see if I can get 1st place (the fun part) 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


// Also known as:    
//   "SIZE - leading zero Count"   UInt32[1->31, 2->30 3->30, 4->29]
//   "Floor Log2"  [1->0, 2->1 3->1, 4->2]
//   "Bit Scan Reverse"  [1->0, 2->1 3->1, 4->2]
//   "find last set" 
//   "most significant set bit"


namespace BenchmarkLeading0Count
{
    class Program
    {
        static void Main()
        {
            // Configuration (also adjust Linq query below as needed)
            const int TEST_ITEMS = 100000;
            const int MAX_BIT_SIZE = 31; // 1-31

            CheckEnvForProperBenchmarking();
            FillAnswers(TEST_ITEMS, out uint[] tests, out uint[] answers, MAX_BIT_SIZE);

            // Lets run benchmark without doing anything to establish a baseline
            var res = RunBenchmark(tests, answers, 0, ForTiming);
            long baseline = res.ticks;

            var resultList = new List<BenchmarkResults>
            {
                RunBenchmark(tests, answers, baseline, Log2_SunsetQuest0),        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/13/2019 - mostly created just to get the correct results
                RunBenchmark(tests, answers, baseline, Log2_SunsetQuest1),        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/13/2019 - 1st attempt
                RunBenchmark(tests, answers, baseline, Log2_SunsetQuest2),        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/15/2019 - 2nd attempt - not that fast
                RunBenchmark(tests, answers, baseline, Log2_SunsetQuest3),        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/15/2019 - 3rd attempt - Using Exponent in float idea was inspired by SPWorley 3/22/09 - https://stackoverflow.com/a/671826/2352507
                RunBenchmark(tests, answers, baseline, Log2_SunsetQuest4),        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/15/2019 - 3rd attempt - Using Exponent in float idea was inspired by SPWorley 3/22/09 - https://stackoverflow.com/a/671826/2352507
                RunBenchmark(tests, answers, baseline, Log2_SPWorley),            // Source: https://stackoverflow.com/a/8462598/2352507  SPWorley        3/22/2009   (Converted from c++ to C#, modified for 32 bit) (added 10/18/2019)
                RunBenchmark(tests, answers, baseline, Msb_Protagonist),          // Source: https://stackoverflow.com/a/671905/2352507   Protagonist     3/23/2009   (converted to C# from c++ by Ryan White)
                RunBenchmark(tests, answers, baseline, Log2_Flynn1179),           // Source: https://stackoverflow.com/a/8970250/2352507  Flynn1179       1/23/2012
                RunBenchmark(tests, answers, baseline, UsingStrings_Rob),         // Source: https://stackoverflow.com/a/10439357/2352507 Rob             5/3/2012    (modified from leading zero count to Log2)
                RunBenchmark(tests, answers, baseline, MostSigBit_spender),       // Source: https://stackoverflow.com/a/10439333/2352507 spender         5/3/2012 
                RunBenchmark(tests, answers, baseline, log2floor_greggo),         // Source: https://stackoverflow.com/a/12886303/2352507 greggo          10/14/2012  (converted to C# from c++ by Ryan White)
                RunBenchmark(tests, answers, baseline, FloorLog2_Matthew_Watson), // Source: https://stackoverflow.com/a/15967635/2352507 Matthew Watson  4/12/2013
                RunBenchmark(tests, answers, baseline, Log2_WiegleyJ),            // Source: https://stackoverflow.com/a/20342282/2352507 WiegleyJ        12/3/2013
                RunBenchmark(tests, answers, baseline, getMsb_user3177100),       // Source: https://stackoverflow.com/a/27101794/2352507 user3177100     11/24/2014  (converted to C# from c++ by Ryan White)
                RunBenchmark(tests, answers, baseline, Log2_DanielSig),           // Source: https://stackoverflow.com/a/30643928/2352507 DanielSig       4/5/2015
                RunBenchmark(tests, answers, baseline, Log2_HarrySvensson),       // Source: https://stackoverflow.com/a/44221387/2352507 Harry Svensson  5/27/2017   (converted to C# and 32 bit use by Ryan White)
                RunBenchmark(tests, answers, baseline, BitScanReverse2),          // Source: https://stackoverflow.com/a/47049483/2352507 Derek Ziemba    11/1/2017
                RunBenchmark(tests, answers, baseline, Log2_Papayaved),           // Source: https://stackoverflow.com/a/50718255/2352507 Papayaved       6/6/2016
                RunBenchmark(tests, answers, baseline, FloorLog2_SN17),           // Source: https://stackoverflow.com/a/56556550/2352507 SN17            6/12/2019   (slightly modified for 32 bit vs 16-bit)
                RunBenchmark(tests, answers, baseline, highestBitUnrolled_Kaz),   // Source: https://stackoverflow.com/a/8462598/2352507  Kaz Kylheku     12/11/11    (Converted from c++ to C#, modified for 32 bit, lowed output value by 1)
                //RunBenchmark(tests, answers, baseline, Log2_ChuckCottrill1),    // Source: https://stackoverflow.com/a/33189337/2352507 Chuck Cottrill  8/17/2015   (converted to C# from c++ by Ryan White) 
                //RunBenchmark(tests, answers, baseline, Log2_ChuckCottrill2),    // Source: https://stackoverflow.com/a/33189337/2352507 Chuck Cottrill  8/17/2015   (converted to C# from c++ by Ryan White)
                //RunBenchmark(tests, answers, baseline, Log2_ChuckCottrill3),    // Source: https://stackoverflow.com/a/33189337/2352507 Chuck Cottrill  8/17/2015   (converted to C# from c++ by Ryan White)
                //RunBenchmark(tests, answers, baseline, log2_quirinpa),          // Source: https://stackoverflow.com/a/37769223/2352507 quirinpa        6/11/2016   (slightly modified for c# from C++, changed return to int) - looks like this one gets the first bit set and not most significant bit


            };

            var result = from r in resultList
                         //where r.errors < TEST_ITEMS / 2
                         //where r.supports32Bits  // Some functions support a full 32 bit uint and other only support a 31 bit ulong. (top bit will fail)
                         //where r.supportsZero  // When there is a zero as input the the function should indicate an error as a neg number.
                         orderby r.ticks ascending
                         select r.results;

            foreach (var r in result)
            {
                Console.WriteLine(r);
            }

            Console.WriteLine("Benchmark" + res.results);
        }


        ////////////////////////  The Methods  ////////////////////////

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SunsetQuest0(uint x)
        {
            return (int)Math.Log(x, 2);
        }


        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/13/2019
        // This is some of the most ugly code I have written, but it is fast! (curly braces left off or code would be long) 
        // The code may look long, however, the CPU will only touch a small fraction of the instructions.
        // Since it does run a small fraction of the lines of code, this should not take up a lot of cache memory.
        // With Release, no-debugger attached, execution is very fast however, debug mode performance is avg.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SunsetQuest1(uint val)
        {
            int b = (int)val;
            if ((b & 0xFFFF0000) == 0)
            {
                if (b < (1 << 8))
                {
                    if (b < (1 << 4))
                    {
                        if (b < (1 << 2))
                            if (b == 0) return -1;
                            else return b >> 1;
                        else
                            return 2 + (b >> 3);
                    }
                    else
                    {
                        if (b < (1 << 6))
                            return 4 + (b >> 5);
                        else
                            return 6 + (b >> 7);
                    }
                }
                else
                {
                    if (b < (1 << 12))
                    {
                        if (b < (1 << 10))
                            return 8 + (b >> 9);
                        else
                            return 10 + (b >> 11);
                    }
                    else
                    {
                        if (b < (1 << 14))
                            return 12 + (b >> 13);
                        else
                            return 14 + (b >> 15);
                    }
                }
            }
            else
            {
                if (val < (1 << 24))
                {
                    if (b < (1 << 20))
                    {
                        if (b < (1 << 18))
                            return 16 + (b >> 17);
                        else
                            return 18 + (b >> 19);
                    }
                    else
                    {
                        if (b < (1 << 22))
                            return 20 + (b >> 21);
                        else
                            return 22 + (b >> 23);
                    }
                }
                else
                {
                    if (val < (1 << 28))
                    {
                        if (b < (1 << 26))
                            return 24 + (b >> 25);
                        else
                            return 26 + (b >> 27);
                    }
                    else
                    {
                        if (val < (1 << 30))
                            return 28 + (b >> 29);
                        else
                            return (int)(30 + (val >> 31));
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SunsetQuest2(uint val)
        {
            //int b = (int)val;

            switch (val >> 28)
            {
                case 0:
                    switch (val >> 24)
                    {
                        case 0:
                            switch (val >> 20)
                            {
                                case 0:
                                    switch (val >> 16)
                                    {
                                        case 0:
                                            switch (val >> 12)
                                            {
                                                case 0:
                                                    switch (val >> 8)
                                                    {
                                                        case 0:
                                                            switch (val >> 4)
                                                            {
                                                                case 0:
                                                                    switch (val )
                                                                    {
                                                                        case 0: return -1;
                                                                        case 1: return 0;
                                                                        case 2: return 1;
                                                                        case 3: return 1;
                                                                        case 4: return 2;
                                                                        case 5: return 2;
                                                                        case 6: return 2;
                                                                        case 7: return 2;
                                                                        case 8: return 3;
                                                                        case 9: return 3;
                                                                        case 10: return 3;
                                                                        case 11: return 3;
                                                                        case 12: return 3;
                                                                        case 13: return 3;
                                                                        case 14: return 3;
                                                                        case 15: return 3;
                                                                    }
                                                                    break;
                                                                case 1: return 4;
                                                                case 2: return 5;
                                                                case 3: return 5;
                                                                case 4: return 6;
                                                                case 5: return 6;
                                                                case 6: return 6;
                                                                case 7: return 6;
                                                                case 8: return 7;
                                                                case 9: return 7;
                                                                case 10: return 7;
                                                                case 11: return 7;
                                                                case 12: return 7;
                                                                case 13: return 7;
                                                                case 14: return 7;
                                                                case 15: return 7;
                                                            }
                                                            break;
                                                        case 1: return 8;
                                                        case 2: return 9;
                                                        case 3: return 9;
                                                        case 4: return 10;
                                                        case 5: return 10;
                                                        case 6: return 10;
                                                        case 7: return 10;
                                                        case 8: return 11;
                                                        case 9: return 11;
                                                        case 10: return 11;
                                                        case 11: return 11;
                                                        case 12: return 11;
                                                        case 13: return 11;
                                                        case 14: return 11;
                                                        case 15: return 11;
                                                    }
                                                    break;
                                                case 1: return 12;
                                                case 2:
                                                case 3: return 13;
                                                case 4:
                                                case 5:
                                                case 6:
                                                case 7: return 14;
                                                case 8:
                                                case 9:
                                                case 10:
                                                case 11:
                                                case 12:
                                                case 13:
                                                case 14:
                                                case 15: return 15;
                                            }
                                            break;
                                        case 1: return 16;
                                        case 2:
                                        case 3: return 17;
                                        case 4:
                                        case 5:
                                        case 6:
                                        case 7: return 18;
                                        case 8:
                                        case 9:
                                        case 10:
                                        case 11:
                                        case 12:
                                        case 13:
                                        case 14:
                                        case 15: return 19;
                                    }
                                    break;
                                case 1: return 20;
                                case 2:
                                case 3: return 21;
                                case 4:
                                case 5:
                                case 6:
                                case 7: return 22;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15: return 23;
                            }
                            break;

                        case 1: return 24;
                        case 2:
                        case 3: return 25;
                        case 4:
                        case 5:
                        case 6:
                        case 7: return 26;
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15: return 27;
                    }
                    break;

                case 1: return 28;
                case 2:
                case 3: return 29;
                case 4:
                case 5:
                case 6:
                case 7: return 30;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15: return 31;
            }
            return -1;
        }

        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/15/2019 - 3rd attempt - Thought of using exponent of float inspired by SPWorley 3/22/09 - https://stackoverflow.com/a/671826/2352507
        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterStruct
        {
            [FieldOffset(0)] public int asInt;
            [FieldOffset(0)] public float asFloat;
        }

        // How I came up with this? I thought I came up with the idea of using the exponent from a integer to float conversion and was super excited. But after then I re-read a post by SPWorley 3/22/09 - https://stackoverflow.com/a/671826/2352507 (c++ code) and now I think this is what seeded the idea in my brain. 
        // My version is little different then SPWorley's "double ff=(double)(v|1);return ((*(1+(uint32_t*)&ff))>>20)-1023;"        
        // Integer Log2 created by Ryan S. White (Sunsetquest) on 10/15/2019 - 3rd attempt - Inspired by SPWorley 3/22/09 - https://stackoverflow.com/a/671826/2352507
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SunsetQuest3(uint val)
        {
            ConverterStruct a;  a.asInt = 0; a.asFloat = val;
            return ((a.asInt >> 23 )+ 1) & 0x1F;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterStruct2
        {
            [FieldOffset(0)] public ulong asLong;
            [FieldOffset(0)] public double asDouble;
        }

        // Same as Log2_SunsetQuest3 except
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SunsetQuest4(uint val)
        {
            ConverterStruct2 a;  a.asLong = 0; a.asDouble = val;
            return (int)((a.asLong >> 52) + 1) & 0xFF;
        }

        // added for Log2_SPWorley()
        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterStruct3
        {
            [FieldOffset(0)] public double asDouble;
            [FieldOffset(4)] public uint asUInt;  
        }
        // Source: https://stackoverflow.com/a/671826/2352507 SPWorley 3/22/2009 (Converted from c++ to C#, modified for 32 bit) (added here 10/18/2019)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_SPWorley(uint v)
        {
            //double ff = (double)(v | 1);
            //return ((*(1 + (uint32_t*)&ff)) >> 20) - 1023;

            ConverterStruct3 a; a.asUInt = 1; a.asDouble = (v | 1);

            //Console.WriteLine(IntToBinaryString(a.asUInt));
            //Console.WriteLine(IntToBinaryString(1 + a.asUInt));
            //Console.WriteLine(IntToBinaryString((1 + a.asUInt) >> 20));
            //Console.WriteLine(IntToBinaryString((((1 + a.asUInt) >> 20) - 1023)));

            return (int)(((1 + a.asUInt) >> 20) - 1023);
        }

        // Source: https://stackoverflow.com/a/8462598/2352507 Kaz Kylheku 12/11/11 (Converted from c++ to C#, modified for 32 bit, lowed output value by 1)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int highestBitUnrolled_Kaz(uint n)
        {
            if ((n & 0xFFFF0000) > 0)
            {
                if ((n & 0xFF000000) > 0)
                {
                    if ((n & 0xF0000000) > 0)
                    {
                        if ((n & 0xC0000000) > 0)
                            return ((n & 0x80000000) > 0) ? 31 : 30;
                        else
                            return ((n & 0x20000000) > 0) ? 29 : 29;
                    }
                    else
                    {
                        if ((n & 0x0C000000) > 0)
                            return ((n & 0x08000000) > 0) ? 27 : 26;
                        else
                            return ((n & 0x02000000) > 0) ? 25 : 24;
                    }
                }
                else
                {
                    if ((n & 0x00F00000) > 0)
                    {
                        if ((n & 0x00C00000) > 0)
                            return ((n & 0x00800000) > 0) ? 23 : 22;
                        else
                            return ((n & 0x00200000) > 0) ? 21 : 20;
                    }
                    else
                    {
                        if ((n & 0x000C0000) > 0)
                            return ((n & 0x00080000) > 0) ? 19 : 18;
                        else
                            return ((n & 0x00020000) > 0) ? 17 : 16;
                    }
                }
            }
            else
            {
                if ((n & 0x0000FF00) > 0)
                {
                    if ((n & 0x0000F000) > 0)
                    {
                        if ((n & 0x0000C000) > 0)
                            return ((n & 0x00008000) > 0) ? 15 : 14;
                        else
                            return ((n & 0x00002000) > 0) ? 13 : 12;
                    }
                    else
                    {
                        if ((n & 0x00000C00) > 0)
                            return ((n & 0x00000800) > 0) ? 11 : 10;
                        else
                            return ((n & 0x00000200) > 0) ? 9 :  8;
                    }
                }
                else
                {
                    if ((n & 0x000000F0) > 0)
                    {
                        if ((n & 0x000000C0) > 0)
                            return ((n & 0x00000080) > 0) ? 7 :  6;
                        else
                            return ((n & 0x00000020) > 0) ? 5 :  4;
                    }
                    else
                    {
                        if ((n & 0x0000000C) > 0)
                            return ((n & 0x00000008) > 0) ? 3 :  2;
                        else
                            return ((n & 0x00000002) > 0) ? 1 : ((n>0) ? 0 : -1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        // Source: https://stackoverflow.com/a/15967635/2352507 Matthew Watson 4/12/2013
        public static int FloorLog2_Matthew_Watson(uint x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);

            return NumBitsSet(x) - 1;
        }
        public static int NumBitsSet(uint x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);

            return (int)(x & 0x0000003f);
        }


        // Source: https://stackoverflow.com/a/30643928/2352507 DanielSig 4/5/2015
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_DanielSig(uint d)
        {
            int v = (int)d;
            int r = 0xFFFF - v >> 31 & 0x10;
            v >>= r;
            int shift = 0xFF - v >> 31 & 0x8;
            v >>= shift;
            r |= shift;
            shift = 0xF - v >> 31 & 0x4;
            v >>= shift;
            r |= shift;
            shift = 0x3 - v >> 31 & 0x2;
            v >>= shift;
            r |= shift;
            r |= (v >> 1);
            return r;
        }

        // Source: https://stackoverflow.com/a/56556550/2352507 SN17 6/12/2019 (slightly modified for 32 bit vs 16-bit)
        [MethodImpl(MethodImplOptions.NoInlining)]
        static int FloorLog2_SN17(uint value)
        {
            for (byte i = 0; i < 31; ++i)
            {
                if ((value >>= 1) < 1)
                {
                    return i;
                }
            }
            return 31;
        }

        // Source: https://stackoverflow.com/a/37769223/2352507 quirinpa 6/11/2016 (slightly modified for c# from C++, changed return to int)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int log2_quirinpa(uint n)
        {
            int i;
            for (i = 0; (n & 0x01) > 0; n >>= 1, i++) ;
            return i;
        }

        // Based on spender(5-3-2012), and modified by sunsetquest, this is slightly modified for the most significant bit. 
        // Source: https://stackoverflow.com/a/10439333/2352507  spender 5/3/2012 
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int MostSigBit_spender(uint x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            //count the ones
            x -= x >> 1 & 0x55555555;
            x = (x >> 2 & 0x33333333) + (x & 0x33333333);
            x = (x >> 4) + x & 0x0f0f0f0f;
            x += x >> 8;
            x += x >> 16;
            return (int)((x & 0x0000003f)-1); //subtract # of 1s from 32
        }

        // Source: https://stackoverflow.com/a/47049483/2352507  Derek Ziemba 11/1/2017
        [MethodImpl(MethodImplOptions.NoInlining)] public static int BitScanReverse2(uint mask) => _BitScanReverse32(mask);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        private static TDelegate GenerateX86Function<TDelegate>(byte[] x86AssemblyBytes)
        {
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint ALLOCATIONTYPE_MEM_COMMIT = 0x1000;
            const uint ALLOCATIONTYPE_RESERVE = 0x2000;
            const uint ALLOCATIONTYPE = ALLOCATIONTYPE_MEM_COMMIT | ALLOCATIONTYPE_RESERVE;
            IntPtr buf = VirtualAlloc(IntPtr.Zero, (uint)x86AssemblyBytes.Length, ALLOCATIONTYPE, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(x86AssemblyBytes, 0, buf, x86AssemblyBytes.Length);
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(buf, typeof(TDelegate));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int32 BitScan32Delegate(uint inValue);

        static BitScan32Delegate _BitScanReverse32 = (new Func<BitScan32Delegate>(() =>
        { //IIFE   
            BitScan32Delegate del = null;
            if (IntPtr.Size == 4)
            {
                del = GenerateX86Function<BitScan32Delegate>(
                   x86AssemblyBytes: new byte[20] {
            //18: int BitScanReverse(unsigned int inValue) {
            0x51,                                       //51                   push        ecx  
            //19:    unsigned long i;
            //20:    return _BitScanReverse(&i, inValue) ? i : -1;
            0x0F, 0xBD, 0x44, 0x24, 0x08,               //0F BD 44 24 08       bsr         eax,dword ptr [esp+8] 
            0x89, 0x04, 0x24,                           //89 04 24             mov         dword ptr [esp],eax 
            0xB8, 0xFF, 0xFF, 0xFF, 0xFF,               //B8 FF FF FF FF       mov         eax,-1  
            0x0F, 0x45, 0x04, 0x24,                     //0F 45 04 24          cmovne      eax,dword ptr [esp]  
            0x59,                                       //59                   pop         ecx 
            //21: }
            0xC3,                                       //C3                   ret  
                   });
            }
            else if (IntPtr.Size == 8)
            {
                del = GenerateX86Function<BitScan32Delegate>(
                      //This code also will work for UInt64 bitscan.
                      // But I have it limited to UInt32 via the delegate because UInt64 bitscan would fail in a 32bit dotnet process. 
                      x86AssemblyBytes: new byte[13] {
            //23:    unsigned long i;
            //24:    return _BitScanReverse64(&i, inValue) ? i : -1; 
            0x48, 0x0F, 0xBD, 0xD1,            //48 0F BD D1          bsr         rdx,rcx 
            0xB8, 0xFF, 0xFF, 0xFF, 0xFF,      //B8 FF FF FF FF       mov         eax,-1
            0x0F, 0x45, 0xC2,                  //0F 45 C2             cmovne      eax,edx  
            //25: }
            0xC3                              //C3                   ret 
                      });
            }
            return del;
        }))();


        // Source: https://stackoverflow.com/a/50718255/2352507  Papayaved 6/6/2016
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_Papayaved(uint value)
        {
            int i;
            for (i = -1; value != 0; i++)
                value >>= 1;

            return ((i == -1) ? 0 : i);
        }

        // Source: https://stackoverflow.com/a/8970250/2352507 Flynn1179 1/23/2012
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_Flynn1179(uint n)
        {
            int bits = 0;
            for (int b = 16; b >= 1; b /= 2)
            {
                int s = 1 << b;
                if (n >= s) { n >>= b; bits += b; }
            }
            return bits;
        }

        // Source: https://stackoverflow.com/a/20342282/2352507 WiegleyJ 12/3/2013
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_WiegleyJ(uint n)
        {
            int bits = 0;

            if (n > 0xffff)
            {
                n >>= 16;
                bits = 0x10;
            }

            if (n > 0xff)
            {
                n >>= 8;
                bits |= 0x8;
            }

            if (n > 0xf)
            {
                n >>= 4;
                bits |= 0x4;
            }

            if (n > 0x3)
            {
                n >>= 2;
                bits |= 0x2;
            }

            if (n > 0x1)
            {
                bits |= 0x1;
            }
            return bits;
        }

        // Source:  https://stackoverflow.com/a/671905/2352507 Protagonist 3/23/2009 (converted from c++ to C# by Ryan White )
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Msb_Protagonist(uint v)
        {
            byte[] pos = {0, 1, 28, 2, 29, 14, 24, 3,
                        30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19,
                        16, 7, 26, 12, 18, 6, 11, 5, 10, 9};
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v = (v >> 1) + 1;
            return pos[(v * 0x077CB531) >> 27];
        }

        // Source: https://stackoverflow.com/a/27101794/2352507 user3177100     11/24/2014  (converted to C# from c++ by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)]  
        public static int getMsb_user3177100(uint n)
        {
            int msb = sizeof(uint) * 4;
            int step = msb;
            while (step > 1)
            {
                step /= 2;
                if (n >> msb > 0)
                    msb += step;
                else
                    msb -= step;
            }
            if (n >> msb > 0)
                msb++;
            return msb - 1;
        }

        // Source: https://stackoverflow.com/a/12886303/2352507  // greggo 10/14/2012  (converted to C# by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int log2floor_greggo(uint x)
        {
            sbyte[] wtab = { -1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };
            int r = 0;
            uint xk = x >> 16;
            if (xk != 0)
            {
                r = 16;
                x = xk;
            }
            // x is 0 .. 0xFFFF
            xk = x >> 8;
            if (xk != 0)
            {
                r += 8;
                x = xk;
            }
            // x is 0 .. 0xFF
            xk = x >> 4;
            if (xk != 0)
            {
                r += 4;
                x = xk;
            }
            // now x is 0..15; x=0 only if originally zero.
            return r + wtab[x];
        }


        // Source: https://stackoverflow.com/a/33189337/2352507  ChuckCottrill 8/17/2015  (converted to C# by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)]  //incorrect answers maybe because of my conversion
        public static int Log2_ChuckCottrill1(uint x)
        {
            uint n = x;
            int bits = sizeof(uint) * 8;
            int step; int k = 0;
            for (step = 1; step < bits;)
            {
                n |= (n >> step);
                step *= 2; ++k;
            }
            return (int)(x - (n >> 1));
        }

        // Source: https://stackoverflow.com/a/33189337/2352507  ChuckCottrill 8/17/2015  (converted to C# by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)] //incorrect answers maybe because of my conversion
        public static int Log2_ChuckCottrill2(uint x)
        {
            int step, step2;

            for (step2 = 0; x > 1L << step2 + 8;)
            {
                step2 += 8;
            }
            for (step = 0; x > 1L << (step + step2);)
            {
                step += 1;
            }
            return (step + step2);
        }


        // Source: https://stackoverflow.com/a/33189337/2352507  ChuckCottrill 8/17/2015  (converted to C# by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)] //incorrect answers maybe because of my conversion
        public static int Log2_ChuckCottrill3(uint x)
        {
            int bits = sizeof(uint) * 8;
            int hbit = bits - 1;
            int lbit = 0;
            int guess = bits / 2;

            while (hbit - lbit > 1)
            {
                //when value between guess..lbit
                if ((x <= (1 << guess)))
                {
                    hbit = guess;
                    guess = (hbit + lbit) / 2;
                }
                //when value between hbit..guess
                //else
                if ((x > (1 << guess)))
                {
                    lbit = guess;
                    guess = (hbit + lbit) / 2;
                }
            }
            if ((x > (1 << guess))) ++guess;
            return guess;
        }


        // Source: https://stackoverflow.com/a/44221387/2352507  Harry Svensson 5/27/2017  (converted to C# and 32 bit use by Ryan White)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2_HarrySvensson(uint value)
        {
            int result = 0;//could be a char or int8_t instead

            if ((0xFFFF0000 & value) > 0) { value >>= (1 << 4); result |= (1 << 4); }
            if ((0xFF00 & value) > 0) { value >>= (1 << 3); result |= (1 << 3); }
            if ((0x00F0 & value) > 0) { value >>= (1 << 2); result |= (1 << 2); }
            if ((0x000C & value) > 0) { value >>= (1 << 1); result |= (1 << 1); }
            if ((0x0002 & value) > 0) { result |= (1 << 0); }

            return result;
        }

        // Source: https://stackoverflow.com/a/10439357/2352507 Rob 5/3/2012   (modified for Log2)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int UsingStrings_Rob(uint val)
        {
            return Convert.ToString(val, 2).Count() - 1;
        }
        


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ForTiming(uint x)
        {
            return (int)x;
        }


        private static BenchmarkResults RunBenchmark(uint[] testValues, uint[] answers, long baseline, Func<uint, int> methodToRun)
        {
            BenchmarkResults res;
            res.errors = 0;
            GCClear();
            res.MethodName = methodToRun(1).ToString()[0].ToString().TrimStart('0');
            res.MethodName += methodToRun.Method.Name;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < testValues.Length; i++)
                if (methodToRun(testValues[i]) != answers[i])
                    res.errors++;
            sw.Stop();
            res.ticks = sw.ElapsedTicks;
            res.supports32Bits = (methodToRun(0xF0000000) ==31);
            res.supportsZero = (methodToRun(0)<0);
            res.results = res.MethodName + ": " + (res.ticks - baseline) * 1000 / testValues.Length 
                + " with " + res.errors + " errors." 
                + " (Supports full 32-bit:" + (res.supports32Bits?"Y":"N") + ")" 
                + " (Supports Neg Return on Zero:" + (res.supportsZero ? "Y" : "N") + ")";
            return res;
        }

        private static void FillAnswers(uint TEST_ITEMS, out uint[] testValues, out uint[] answers, int MAX_BIT_SIZE)
        {
            Random rand = new Random();

            //create a random list with answers
            testValues = new uint[TEST_ITEMS];
            answers = new uint[TEST_ITEMS];

            // Lets create the answers
            int bits = 1;
            for (int i = 0; i < TEST_ITEMS; i++)
            {
                testValues[i] = (uint)rand.Next((1 << (bits - 1)), (1 << bits) - 1);
                if (bits > MAX_BIT_SIZE)
                    bits = 1;
                else
                    bits++;

                answers[i] = (uint)(Math.Log(testValues[i], 2));
            }
        }

        // source: David Zych Dec. 2013  https://davidzych.com/converting-an-int-to-a-binary-string-in-c/ 
        public static string IntToBinaryString(uint number)
        {
            const uint mask = 1;
            var binary = string.Empty;
            while (number > 0)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & mask) + binary;
                number = number >> 1;
            }

            return binary;
        }

        // source https://stackoverflow.com/a/17307700/2352507
        private static void GCClear()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        private static void CheckEnvForProperBenchmarking()
        {
            if (Debugger.IsAttached)
                Console.WriteLine("Warning: A debugger is attached - benchmarks may not be accurate.");
#if DEBUG
            Console.WriteLine("Warning: Code was compiled in Debug mode - benchmarks may not be accurate.");
#endif
        }

    }
    public struct BenchmarkResults
    {
        public string MethodName;
        public int errors;
        public long ticks;
        public string results;
        public bool supports32Bits;
        public bool supportsZero;
    }
}



// Additional Sources:
// for [MethodImpl(MethodImplOptions.NoInlining)] idea https://stackoverflow.com/a/17307712/2352507  (June 25 2013)

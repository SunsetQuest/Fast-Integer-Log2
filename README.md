# Fast Integer Log2 in C#
Benchmark several others and also some of my own.

Created by Ryan S. White (sunsetquest) on 10/13/2019

Licensed under MIT  

## Goals: 
1. benchmark a large set of known methods 
2. see if I can get 1st place (the fun part) 


## Also known as
   "SIZE - leading zero Count"   UInt32[1->31, 2->30 3->30, 4->29]
   "Floor Log2"  [1->0, 2->1 3->1, 4->2]
   "Bit Scan Reverse"  [1->0, 2->1 3->1, 4->2]
   "find last set" 
   "most significant set bit"

## Benchmark results
Using: (AMD Ryzen CPU, Release mode, no-debugger attached, .net core 2.1

    Function                 Time1  Time2    Errors  Full-32-Bit Zero_Support
    Log2_SPWorley:            18     18       0        (Y)        (N)
    Log2_SunsetQuest3:        18     18       0        (Y)        (N)
    Log2_SunsetQuest4:        18     18       0        (Y)        (N)
    MostSigBit_spender:       20     19       0        (Y)        (Y)
    Log2_HarrySvensson:       26     29       0        (Y)        (N)
    Log2_WiegleyJ:            27     23       0        (Y)        (N)
    Log2_DanielSig:           28     24    3125        (N)        (N)
    FloorLog2_Matthew_Watson: 29     25       0        (Y)        (Y)
    Log2_SunsetQuest1:        31     28       0        (Y)        (Y)
    HighestBitUnrolled_Kaz:   33     33    3125        (Y)        (Y)
    Log2_Flynn1179:           58     52       0        (Y)        (N)
    GetMsb_user3177100:       58     53       0        (Y)        (N)
    Log2floor_greggo:         89    101       0        (Y)        (Y)
    FloorLog2_SN17:          102     43       0        (Y)        (N)
    Log2_SunsetQuest2:       118    140       0        (Y)        (Y)
    Log2_Papayaved:          125     60       0        (Y)        (N)
    Msb_Protagonist:         136    118       0        (Y)        (N)
    Log2_SunsetQuest0:       206    202       0        (Y)        (Y)
    BitScanReverse2:         228    240    3125        (N)        (Y)
    UsingStrings_Rob:       2346   1494       0        (Y)        (N)
    
    Zero_Support = Supports Neg Return on Zero
    Full-32-Bit  = Supports full 32-bit (some just support 31 bits)
    Time1 = benchmark for sizes up to 32-bit (same number tried for each size)
    Time2 = benchmark for sizes up to 16-bit (for measuring perf with small numbers)
	

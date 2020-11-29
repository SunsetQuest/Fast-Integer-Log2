# Fast Integer Log2 in C#
Benchmark several others and also some of my own.

Created by Ryan S. White (sunsetQuest) on 10/13/2019, 11/29/2020

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

                                      1-2^32               32-Bit  Zero 
    Function                Time1  Time2  Time3  Time4 Errors Support Support 
    Log2_SunsetQuest3        18     18    79167    19   255      N       N
    Log2_SunsetQuest4        18     18    86976    19     0      Y       N
    LeadingZeroCount_Sunsetq  -      -        -    30     0      Y       Y
    Log2_SPWorley            18     18    90976    32  4096      N       Y
    MostSigBit_spender       20     19    86083    89     0      Y       Y
    Log2_HarrySvensson       26     29    93592    34     0      Y       Y
    Log2_WiegleyJ            27     23    95347    38     0      Y       N
    Leading0Count_phuclv      -      -        -    33   10M      N       N
    Log2_SunsetQuest1        31     28    78385    39     0      Y       Y
    HighestBitUnrolled_Kaz   33     33   284112    35  2.5M      N       Y
    Log2_Flynn1179           58     52    96381    48     0      Y       Y
    BitOperationsLog2Sunsetq  -      -        -    49     0      Y       Y
    GetMsb_user3177100       58     53   100932    60     0      Y       Y
    Log2_Papayaved          125     60   119161    90     0      Y       Y
    FloorLog2_SN17          102     43   121708    97     0      Y       Y
    Log2_DanielSig           28     24   960357   102    2M      N       Y
    FloorLog2_Matthew_Watson 29     25    94222   104     0      Y       Y
    Log2_SunsetQuest2       118    140   163483   184     0      Y       Y
    Msb_Protagonist         136    118  1631797   212     0      Y       Y
    Log2_SunsetQuest0       206    202   128695   212     0      Y       Y
    BitScanReverse2         228    240  1132340   215    2M      N       Y
    Log2floor_greggo         89    101   2x10^7   263     0      Y       Y
    UsingStrings_Rob       2346   1494   2x10^7  2079     0      Y       Y
    
                            1-2^32              32-Bit     Zero
    Name                    ticks     Errors     Support  Support
    =============================================================
    LeadingZeroCountSunset   2         0         Yes       Yes
    BitOpsLog2SunsetQuest    2         0         Yes       Yes
    Log2_SunsetQuest5        15        0         Yes       No
    Log2_SunsetQuest4        17        0         Yes       No
    Log2_SunsetQuest3        17        0         Yes       No
    MostSigBit_spender       18        0         Yes       Yes
    Log2_SPWorley            18        2         Yes       Yes
    FloorLg2_Matthew_Watson  22        0         Yes       Yes
    Log2_DanielSig           27        312500    No        Yes
    Log2_HarrySvensson       30        0         Yes       Yes
    Log2_WiegleyJ            31        0         Yes       Yes
    Log2_SunsetQuest1        34        0         Yes       Yes
    highestBitUnrolled_Kaz   37        312500    Yes       Yes
    getMsb_user3177100       68        0         Yes       Yes
    Msb_Protagonist          69        0         Yes       Yes
    Log2_Flynn1179           70        0         Yes       Yes
    Log2_Papayaved           87        0         Yes       Yes
    log2floor_greggo         90        0         Yes       Yes
    FloorLog2_SN17           98        0         Yes       Yes
    Log2_SunsetQuest2        133       0         Yes       Yes
    Log2_SunsetQuest0        212       0         Yes       Yes
    BitScanReverse_Other     828       0         Yes       Yes
    UsingStrings_Other       2116      0         Yes       Yes


    
        
    Zero_Support = Supports Zero if the result is 0 or less
    Full-32-Bit  = Supports full 32-bit (some just support 31 bits)
    Time4 = .Net 5.0 (note: because random was not used some compiler optimization might have been applied so this result might not be accurate) 

	

## And the winner is...
Fast, safe, and portable

        BitOperations.Log2(x);
        


[1]: https://stackoverflow.com/a/671826/2352507


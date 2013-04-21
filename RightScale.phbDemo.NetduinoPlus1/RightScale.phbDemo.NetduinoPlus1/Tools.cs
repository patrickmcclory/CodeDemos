using System;
using Microsoft.SPOT;
/*
 * Copyright 2011-2012 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace RightScale.phbDemo.NetduinoPlus1
{
    class Tools
    {
        public static string HardwareProvider
        {
            get
            {
                if (Tools._HardwareProvider == "")
                {
                    string[] HP = Microsoft.SPOT.Hardware.HardwareProvider.HwProvider.ToString().Split(new char[] { '.' });
                    Tools._HardwareProvider = HP[HP.Length - 2];
                }
                return Tools._HardwareProvider;
            }
        }

        private static string _HardwareProvider = "";

        public static string Escape(string Input)
        {
            if (Input == null) return "";
            char[] Buffer = Input.ToCharArray();
            string RetValue = "";
            for (int i = 0; i < Buffer.Length; ++i)
            {
                if (Buffer[i] == 13)
                    RetValue += "\\r";
                else if (Buffer[i] == 10)
                    RetValue += "\\n";
                else if (Buffer[i] == 92)
                    RetValue += "\\\\";
                else if (Buffer[i] < 32 || Buffer[i] > 126)
                    RetValue += "\\" + Tools.Dec2Hex((int)Buffer[i], 2);
                else
                    RetValue += Buffer[i];
            }

            return RetValue;
        }

        public static uint Hex2Dec(string HexNumber)
        {
            // Always in upper case
            HexNumber = HexNumber.ToUpper();
            // Contains all Hex posibilities
            string ConversionTable = "0123456789ABCDEF";
            // Will contain the return value
            uint RetVal = 0;
            // Will increase
            uint Multiplier = 1;

            for (int Index = HexNumber.Length - 1; Index >= 0; --Index)
            {
                RetVal += (uint)(Multiplier * (ConversionTable.IndexOf(HexNumber[Index])));
                Multiplier = (uint)(Multiplier * ConversionTable.Length);
            }

            return RetVal;
        }

        public static char[] Bytes2Chars(byte[] Input)
        {
            char[] Output = new char[Input.Length];
            for (int Counter = 0; Counter < Input.Length; ++Counter)
                Output[Counter] = (char)Input[Counter];
            return Output;
        }

        public static byte[] Chars2Bytes(char[] Input)
        {
            byte[] Output = new byte[Input.Length];
            for (int Counter = 0; Counter < Input.Length; ++Counter)
                Output[Counter] = (byte)Input[Counter];
            return Output;
       }

        public static string ZeroFill(string Number, int Digits, char Character = '0')
        {
            bool Negative = false;

            if (Number.Substring(0, 1) == "-")
            {
                Negative = true;
                Number = Number.Substring(1);
            }

            for (int Counter = Number.Length; Counter < Digits; ++Counter)
            {
                Number = Character + Number;
            }
            if (Negative) Number = "-" + Number;
            return Number;
        }

        public static string ZeroFill(int Number, int MinLength, char Character = '0')
        {
            return ZeroFill(Number.ToString(), MinLength, Character);
        }

        public static string RawUrlEncode(string Input)
        {
            string RetValue = "";
            for (int Counter = 0; Counter < Input.Length; ++Counter)
            {
                byte CharCode = (byte)(Input.ToCharArray()[Counter]);

                if (
                   CharCode == 0x2d                        // -
                   || CharCode == 0x5f                     // _
                   || CharCode == 0x2e                     // .
                   || CharCode == 0x7e                     // ~
                   || (CharCode > 0x2f && CharCode < 0x3a) // 0-9
                   || (CharCode > 0x40 && CharCode < 0x5b) // A-Z
                   || (CharCode > 0x60 && CharCode < 0x7b) // a-z
                   )
                {
                    RetValue += Input.Substring(Counter, 1);
                }
                else
                {
                    // Calculates the hex value in some way
                    RetValue += "%" + Dec2Hex(CharCode, 2);
                }
            }

            return RetValue;
        }

        public static string RawUrlDecode(string Input)
        {
            string RetValue = "";
            for (int Counter = 0; Counter < Input.Length; ++Counter)
            {
                string Char = Input.Substring(Counter, 1);
                if (Char == "%")
                {
                    // Encoded character
                    string HexValue = Input.Substring(++Counter, 2);
                    ++Counter;
                    RetValue += (char)Hex2Dec(HexValue);
                }
                else
                {
                    // Normal character
                    RetValue += Char;
                }
            }
            return RetValue;
        }

        public static string Base64Encode(string Input)
        {
            // Pairs of 3 8-bit bytes will become pairs of 4 6-bit bytes
            // That's the whole trick of base64 encoding :-)

            int Blocks = Input.Length / 3;           // The amount of original pairs
            if (Blocks * 3 < Input.Length) ++Blocks; // Fixes rounding issues; always round up
            int Bytes = Blocks * 4;                  // The length of the base64 output

            // These characters will be used to represent the 6-bit bytes in ASCII
            char[] Base64_Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".ToCharArray();

            // Converts the input string to characters and creates the output array
            char[] InputChars = Input.ToCharArray();
            char[] OutputChars = new char[Bytes];

           // Converts the blocks of bytes
            for (int Block = 0; Block < Blocks; ++Block)            {
                // Fetches the input pairs
                byte Input0 = (byte)(InputChars.Length > Block * 3 ? InputChars[Block * 3] : 0);
                byte Input1 = (byte)(InputChars.Length > Block * 3 + 1 ? InputChars[Block * 3 + 1] : 0);
                byte Input2 = (byte)(InputChars.Length > Block * 3 + 2 ? InputChars[Block * 3 + 2] : 0);

                // Generates the output pairs
                byte Output0 = (byte)(Input0 >> 2);                           // The first 6 bits of the 1st byte
                byte Output1 = (byte)(((Input0 & 0x3) << 4) + (Input1 >> 4)); // The last 2 bits of the 1st byte followed by the first 4 bits of the 2nd byte
                byte Output2 = (byte)(((Input1 & 0xf) << 2) + (Input2 >> 6)); // The last 4 bits of the 2nd byte followed by the first 2 bits of the 3rd byte
                byte Output3 = (byte)(Input2 & 0x3f);                         // The last 6 bits of the 3rd byte

                // This prevents 0-bytes at the end
                if (InputChars.Length < Block * 3 + 2) Output2 = 64;
                if (InputChars.Length < Block * 3 + 3) Output3 = 64;

                // Converts the output pairs to base64 characters
                OutputChars[Block * 4] = Base64_Characters[Output0];
               OutputChars[Block * 4 + 1] = Base64_Characters[Output1];
               OutputChars[Block * 4 + 2] = Base64_Characters[Output2];
               OutputChars[Block * 4 + 3] = Base64_Characters[Output3];
            }
           return new string(OutputChars);       }



        public static string Dec2Hex(int Input, int MinLength = 0)        {
#if MF_FRAMEWORK_VERSION_V4_2
            // Since NETMF 4.2 int.toString() exists, so we can do this:
            return Input.ToString("x" + MinLength.ToString());
#else
                // Contains all Hex posibilities
                string ConversionTable = "0123456789ABCDEF";
                // Starts the conversion
                string RetValue = "";
                int Current = 0;
                int Next = Input;
                do
                {
                    if (Next >= ConversionTable.Length)
                    {
                        // The current digit
                        Current = (Next / ConversionTable.Length);
                        if (Current * ConversionTable.Length > Next) --Current;
                        // What's left
                        Next = Next - (Current * ConversionTable.Length);
                    }
                    else
                    {
                        // The last digit
                        Current = Next;
                        // Nothing left
                        Next = -1;
                    }
                    RetValue += ConversionTable[Current];
                } while (Next != -1);

                return Tools.ZeroFill(RetValue, MinLength);
#endif
        }

        public static byte[] UShortsToBytes(ushort[] Data)
        {

            byte[] RetVal = new byte[Data.Length * 2];



            int BytePos = 0;

            for (int ShortPos = 0; ShortPos < Data.Length; ++ShortPos)
            {

                RetVal[BytePos++] = (byte)(Data[ShortPos] >> 8);

                RetVal[BytePos++] = (byte)(Data[ShortPos] & 0x00ff);

            }

            return RetVal;

        }

        public static ushort[] BytesToUShorts(byte[] Data)
        {
            ushort[] RetVal = new ushort[Data.Length / 2];

            int BytePos = 0;
            for (int ShortPos = 0; ShortPos < RetVal.Length; ++ShortPos)
            {
                RetVal[ShortPos] = (ushort)((Data[BytePos++] << 8) + Data[BytePos++]);
            }
            return RetVal;
        }

        public static byte XorChecksum(string Data)
        {
            return Tools.XorChecksum(Tools.Chars2Bytes(Data.ToCharArray()));
        }

        public static byte XorChecksum(byte[] Data)        
        {
            byte Checksum = 0;
            for (int Pos = 0; Pos < Data.Length; ++Pos)
                Checksum ^= Data[Pos];

            return Checksum;
        }

        public static string MetricPrefix(float Number, bool BinaryMultiple = false)
        {
            float Multiplier = BinaryMultiple ? 1024 : 1000;
            if (Number > (Multiplier * Multiplier * Multiplier * Multiplier))
                return Tools.Round(Number / Multiplier / Multiplier / Multiplier / Multiplier) + "T";
            if (Number > (Multiplier * Multiplier * Multiplier))
                return Tools.Round(Number / Multiplier / Multiplier / Multiplier) + "G";
            if (Number > (Multiplier * Multiplier))
                return Tools.Round(Number / Multiplier / Multiplier) + "M";
            if (Number > Multiplier)
                return Tools.Round(Number / Multiplier) + "k";
            else
                return Tools.Round(Number).ToString();
        }

        public static string Round(float Input, int Digits = 2)
        {
            int Multiplier = 1;
            for (int i = 0; i < Digits; ++i) Multiplier *= 10;
            string Rounded = ((int)(Input * Multiplier)).ToString();

            return (Rounded.Substring(0, Rounded.Length - 2) + "." + Rounded.Substring(Rounded.Length - 2)).TrimEnd(new char[] { '0', '.' });
        }

        public static int[] ColorToRgb(int Color)
        {
            byte Red = (byte)((Color & 0xff0000) >> 16);
            byte Green = (byte)((Color & 0xff00) >> 8);
            byte Blue = (byte)(Color & 0xff);
            return new int[] { Red, Green, Blue };
        }
    }
}

/*
*  Copyright (c) 2008 Jonathan Wagner
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;

namespace Framework.Network
{
    public class Converter
    {
        public static int GetBigEndian(int value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static ushort GetBigEndian(ushort value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static uint GetBigEndian(uint value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static long GetBigEndian(long value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static double GetBigEndian(double value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static float GetBigEndian(float value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder((int) value) : value;
        }

        public static int GetLittleEndian(int value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static uint GetLittleEndian(uint value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static ushort GetLittleEndian(ushort value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static double GetLittleEndian(double value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        private static int SwapByteOrder(int value)
        {
            var dvalue = (long) value;
            var swap = (int) (0x000000FF & (dvalue >> 24)
                              | 0x0000FF00 & (dvalue >> 8)
                              | 0x00FF0000 & (dvalue << 8)
                              | 0xFF000000 & (dvalue << 24));
            return swap;
        }

        private static long SwapByteOrder(long value)
        {
            var uvalue = (ulong) value;
            var swap = 0x00000000000000FF & (uvalue >> 56)
                       | 0x000000000000FF00 & (uvalue >> 40)
                       | 0x0000000000FF0000 & (uvalue >> 24)
                       | 0x00000000FF000000 & (uvalue >> 8)
                       | 0x000000FF00000000 & (uvalue << 8)
                       | 0x0000FF0000000000 & (uvalue << 24)
                       | 0x00FF000000000000 & (uvalue << 40)
                       | 0xFF00000000000000 & (uvalue << 56);

            return (long) swap;
        }

        private static ushort SwapByteOrder(ushort value)
        {
            return (ushort) ((0x00FF & (value >> 8))
                             | (0xFF00 & (value << 8)));
        }

        private static uint SwapByteOrder(uint value)
        {
            var swap = 0x000000FF & (value >> 24)
                       | 0x0000FF00 & (value >> 8)
                       | 0x00FF0000 & (value << 8)
                       | 0xFF000000 & (value << 24);
            return swap;
        }

        private static double SwapByteOrder(double value)
        {
            var buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer, 0, buffer.Length);
            return BitConverter.ToDouble(buffer, 0);
        }
    }
}
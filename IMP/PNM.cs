﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace UAM.PTO
{
    public abstract class PNM
    {
        private static char[] whitespace = new char[] { ' ', '\t', '\r', '\n' };

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public byte[] Bitmap { get; protected set; }
        public int Stride { get { return Width * 3; } }

        public static PNM LoadFile(string path)
        {
            using (TextReader reader = new StreamReader(path))
            {
                string header = ReadToken(reader);
                switch (header)
                {
                    case "P1":
                        return new PlainPBM(reader);
                    case "P2":
                        return new PlainPGM(reader);
                    case "P3":
                        return new PlainPPM(reader);
                    case "P4":
                        return new RawPBM(reader);
                    case "P5":
                        return new RawPGM(reader);
                    case "P6":
                        return new RawPPM(reader);
                    default:
                        throw new MalformedFileException("Malformed header");
                }
            }
        }

        public static void SaveFile(string path)
        {

        }

        // read token, ignore comments, throw MalformedFileException if there is no token to read
        // comment hash in ascii is 35
        protected static string ReadToken(TextReader reader)
        {
            StringBuilder builder = new StringBuilder();
            // Skip starting whitespace
            int temp;
            while(true)
            {
                temp = reader.Peek();
                if(temp == -1)
                    throw new MalformedFileException();
                if (temp == 35)
                {
                    reader.ReadLine();
                    continue;
                }
                if(!char.IsWhiteSpace((char)temp))
                    break;
                //comment - read to the end of line
                reader.Read();
            }
            // Read actual token
            builder.Append((char)reader.Read());
            while (true)
            {
                temp = reader.Peek();
                if (temp == -1 || char.IsWhiteSpace((char)temp))
                    break;
                builder.Append((char)reader.Read());
            }
            // Return
            return builder.ToString();
        }

        protected int ParseNumber(string token)
        {
            int result;
            if (!Int32.TryParse(token, System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out result))
                throw new MalformedFileException();
            return result;
        }

        protected static string ReadLineSkipComments(TextReader reader)
        {
            string line = null;
            while ((line = reader.ReadLine()) != null && line.Length != 0 && line[0] == '#');
            return line;
        }

        protected static string[] Split(string str)
        {
            return str.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
        }

        // 0,0 is upper left corner, indices are postitive
        protected void ColorPixel(int index, byte r, byte g, byte b)
        {
            if (index >= (Width * Height))
                throw new ArgumentException();
            int realIndex = index * 3;
            Buffer.SetByte(Bitmap, realIndex, r);
            Buffer.SetByte(Bitmap, ++realIndex, r);
            Buffer.SetByte(Bitmap, ++realIndex, r);
        }

        // 0,0 is upper left corner, indices are postitive
        protected void ColorPixel(int x, int y, byte r, byte g, byte b)
        {
            if (x >= Width)
                throw new ArgumentException();
            if (y >= Height)
                throw new ArgumentException();
            int index = (y * Height * 3) + (x * 3);
            Buffer.SetByte(Bitmap, index, r);
            Buffer.SetByte(Bitmap, ++index, r);
            Buffer.SetByte(Bitmap, ++index, r);
        }
    }
}
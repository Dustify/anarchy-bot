using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AoLib.Utils;

namespace AoLib.Mdb
{
    public class Descrambler
    {
        public static DescrambledMessage Decode(string scrambledString) { return Decode(scrambledString, false, null); }
        public static DescrambledMessage Decode(string scrambledString, bool recursive, MdbReader reader)
        {
            if (reader == null)
                reader = new MdbReader();

            if (scrambledString.Length < 2)
                throw new Exception("Invalid message length!");

            if (!scrambledString.StartsWith("~"))
                throw new Exception("Invalid message start! Expecting: ~");

            if (!scrambledString.EndsWith("~") && !recursive)
                throw new Exception("Invalid message end! Expecting: ~");

            scrambledString = scrambledString.Substring(1);
            if (!recursive)
                scrambledString = scrambledString.Substring(0, scrambledString.Length - 1);

            DescrambledMessage aoDescrambledMessage = new DescrambledMessage(0, 0, null);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(scrambledString));

            while (true)
            {
                if (stream.Position >= stream.Length)
                    break;

                //string type = char.ConvertFromUtf32(stream.ReadByte()); /* Not Supported on Mono */
                string type = Convert.ToChar(stream.ReadByte()).ToString();
                switch (type)
                {
                    case "&": // Message start, Category ID and Entry ID
                        aoDescrambledMessage = new DescrambledMessage(
                            Base85.Decode(Read(stream, 5)),
                            Base85.Decode(Read(stream, 5)),
                            "~" + scrambledString + "~"
                        );
                        break;
                    case "s": // String
                        Int32 textSize = stream.ReadByte();
                        String text = Read(stream, textSize - 1);
                        aoDescrambledMessage.Append(new DescrambledArgument(text));
                        break;
                    case "i": // Integer
                        Int32 integer = Base85.Decode(Read(stream, 5));
                        aoDescrambledMessage.Append(new DescrambledArgument(integer));
                        break;
                    case "u": // Unsigned Integer
                        UInt32 unsignedInteger = (UInt32)Base85.Decode(Read(stream, 5));
                        aoDescrambledMessage.Append(new DescrambledArgument(unsignedInteger));
                        break;
                    case "f": // Float
                        Single single = (Single)Base85.Decode(Read(stream, 5));
                        aoDescrambledMessage.Append(new DescrambledArgument(single));
                        break;
                    case "R": // Reference, Category ID and Entry ID
                        String referenceMessage = string.Empty;
                        Int32 referenceCategoryID = Base85.Decode(Read(stream, 5));
                        Int32 referenceEntryID = Base85.Decode(Read(stream, 5));
                        if (reader != null)
                        {
                            MdbEntry referenceEntry = reader.GetEntry(referenceCategoryID, referenceEntryID);
                            if (referenceEntry != null)
                                referenceMessage = referenceEntry.Message;
                        }
                        DescrambledArgument reference = new DescrambledArgument(
                            referenceCategoryID,
                            referenceEntryID,
                            referenceMessage
                        );
                        aoDescrambledMessage.Append(reference);
                        break;
                    case "F": // Recursive, Complete new message
                        Int32 recursiveSize = stream.ReadByte();
                        String recursiveText = Read(stream, recursiveSize - 1);
                        aoDescrambledMessage.Append(new DescrambledArgument(Decode(recursiveText, true, reader)));
                        break;
                    default:
                        throw new Exception("Unknown type detected: " + type);
                }
            }
            if (reader != null)
            {
                MdbEntry entry = reader.GetEntry(aoDescrambledMessage.CategoryID, aoDescrambledMessage.EntryID);
                if (entry != null)
                    try { aoDescrambledMessage.Message = String.Format(PrintfToFormatString(entry.Message), aoDescrambledMessage.Arguments); }
                    catch { }
            }
            return aoDescrambledMessage;
        }

        private static string Read(MemoryStream stream, Int32 length)
        {
            if (stream.Length < stream.Position + length)
                throw new Exception("Trying to read beyond stream size! Size=" + stream.Length + " Position=" + stream.Position + " Length=" + length);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static string PrintfToFormatString(string text)
        {
            Int32 counter = 0;
            string[] types = { "%s", "%u", "%d", "%f" };
            while (true)
            {
                bool typesLeft = false;
                foreach (string type in types)
                    if (text.Contains(type))
                        typesLeft = true;
                if (typesLeft == false)
                    break;

                Int32 index = text.Length;
                foreach (string type in types)
                {
                    Int32 tmpIndex = text.IndexOf(type);
                    if (tmpIndex < index && tmpIndex != -1)
                        index = tmpIndex;
                }
                text = text.Substring(0, index) + "{" + counter + "}" + text.Substring(index + 2);
                counter++;
            }
            return text;
        }
    }
}

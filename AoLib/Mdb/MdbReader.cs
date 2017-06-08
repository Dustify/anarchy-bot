using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AoLib.Mdb
{
    [XmlRoot("MMDB")]
    public class MdbReader
    {
        private string _file;
        private Int32 _endOfCategories = 0;
        private Int32 _endOfEntries = 0;
        private List<MdbCategory> _categories;
        private bool _isLoaded;
        private MdbMode _mode = MdbMode.Unknown;

        [XmlIgnore()]
        public string MmdbFile { get { return this._file; } }
        [XmlElement("Category")]
        public MdbCategory[] Categories
        {
            get { return this._categories.ToArray(); }
            set
            {
                this._categories = new List<MdbCategory>();
                foreach (MdbCategory category in value)
                    this._categories.Add(category);
            }
        }
        [XmlIgnore()]
        public bool IsLoaded { get { return this._isLoaded; } }
        [XmlIgnore()]
        public MdbMode Mode { get { return this._mode; } }

        public MdbReader()
        {
            this._categories = new List<MdbCategory>();
            this._file = "text.mdb";
        }

        public MdbReader(string file)
        {
            this._categories = new List<MdbCategory>();
            this._file = file;
        }

        public bool Read()
        {
            FileStream stream = this.Open(this._file);
            if (stream == null)
                return false;

            try
            {
                this.DetectType(stream);
                if (this._mode == MdbMode.MMDB)
                    return this.MmdbRead(stream);
                else if (this._mode == MdbMode.MLDB)
                    return this.MldbRead(stream);
            }
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        public MdbEntry SpeedRead(Int32 categoryID, Int32 entryID)
        {
            FileStream stream = this.Open(this._file);
            if (stream == null)
                return null;

            try
            {
                this.DetectType(stream);
                if (this._mode != MdbMode.MMDB)
                    throw new NotImplementedException("Speed Reading is only available for MMDB files!");

                stream.Seek(8, SeekOrigin.Begin);

                while (true)
                {
                    Int32 catID = 0;
                    Int32 catOffset = 0;

                    if (!this.MmdbReadKey(stream, ref catID, ref catOffset))
                        return null;

                    if (catID == -1)
                        return null;

                    if (catID == categoryID)
                    {
                        stream.Seek(catOffset, SeekOrigin.Begin);
                        while (true)
                        {
                            Int32 entID = 0;
                            Int32 entOffset = 0;

                            if (!this.MmdbReadKey(stream, ref entID, ref entOffset))
                                return null;

                            if (entID == entryID)
                            {
                                String message = String.Empty;
                                if (!this.MmdbReadString(stream, entOffset, ref message))
                                    return null;

                                return new MdbEntry(entID, entOffset, message);
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return null;
        }

        private FileStream Open(string file)
        {
            FileStream stream;
            try
            {
                stream = File.OpenRead(file);
            }
            catch
            {
                Console.WriteLine("ERROR: Unable to open " + this._file);
                return null;
            }

            // Some Checks
            if (!stream.CanRead)
            {
                Console.WriteLine("ERROR: Unable to read " + this._file);
                return null;
            }
            if (!stream.CanSeek)
            {
                Console.WriteLine("ERROR: Unable to seek " + this._file);
                return null;
            }
            return stream;
        }

        private void DetectType(FileStream stream)
        {
            try
            {
                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                String type = Encoding.UTF8.GetString(buffer);
                if (type.ToLower() == "mmdb")
                {
                    this._mode = MdbMode.MMDB;
                }
                if (type.ToLower() == "mldb")
                {
                    this._mode = MdbMode.MLDB;
                }
            }
            catch { }
        }

        #region MMDB Reader
        protected bool MmdbRead(FileStream stream)
        {
            // Go to start position
            stream.Seek(8, SeekOrigin.Begin);
            try
            {
                // Read categories
                Int32 currentLocation = 8;
                bool notFinishedCategories = true;
                while (notFinishedCategories)
                {
                    Int32 categoryID = 0;
                    Int32 offset = 0;

                    if (!this.MmdbReadKey(stream, ref categoryID, ref offset))
                        throw new Exception();

                    currentLocation += 8;

                    if (categoryID == -1)
                    {
                        this._endOfCategories = currentLocation;
                        this._endOfEntries = offset;
                        notFinishedCategories = false;
                    }
                    else
                    {
                        MdbCategory category = new MdbCategory(categoryID, offset);
                        this._categories.Add(category);
                    }

                }

                // Read Entries
                bool notFinishedEntries = true;
                while (notFinishedEntries)
                {
                    stream.Seek(currentLocation, SeekOrigin.Begin);

                    Int32 entryID = 0;
                    Int32 offset = 0;
                    string message = String.Empty;

                    if (!this.MmdbReadKey(stream, ref entryID, ref offset))
                        throw new Exception();

                    if (!this.MmdbReadString(stream, offset, ref message))
                        throw new Exception();

                    MdbEntry entry = new MdbEntry(entryID, offset, message);

                    MdbCategory category = null;
                    foreach (MdbCategory cat in this._categories)
                    {
                        if (cat.Offset > currentLocation)
                            break;
                        category = cat;
                    }
                    if (category != null)
                        category.Add(entry);

                    currentLocation += 8;
                    if (currentLocation >= this._endOfEntries)
                        break;
                }
            }
            catch
            {
                Console.WriteLine("ERROR: Error during reading " + this._file);
                return false;
            }
            if (stream != null)
                stream.Close();

            this._isLoaded = true;
            return true;
        }

        protected bool MmdbReadKey(FileStream stream, ref Int32 ID, ref Int32 Offset)
        {
            try
            {
                byte[] buffer = new byte[4];

                // Read ID
                stream.Read(buffer, 0, buffer.Length);
                ID = BitConverter.ToInt32(buffer, 0);

                // Read Offset
                stream.Read(buffer, 0, buffer.Length);
                Offset = BitConverter.ToInt32(buffer, 0);

                return true;
            }
            catch { return false; }
        }

        protected bool MmdbReadString(FileStream stream, Int32 offset, ref string Message)
        {
            try
            {
                byte[] buffer = new byte[1];

                stream.Seek(offset, SeekOrigin.Begin);
                while (true)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    if ((Int32)buffer[0] == 0)
                        break;

                    Message += Encoding.UTF8.GetString(buffer);
                }
                return true;
            }
            catch { return false; }
        }
        #endregion

        #region MLDB Reader
        protected bool MldbRead(FileStream stream)
        {
            stream.Seek(4, SeekOrigin.Begin);
            try
            {
                while (true)
                {
                    Int32 categoryID = 0;
                    if (!this.MldbReadInteger(stream, ref categoryID))
                        return false;

                    if (categoryID == 0)
                    {
                        this._isLoaded = true;
                        return true;
                    }

                    MdbCategory category = this.GetCategory(categoryID);
                    if (category == null)
                    {
                        category = new MdbCategory(categoryID, 0);
                        this._categories.Add(category);
                    }

                    Int32 entryID = 0;
                    if (!this.MldbReadInteger(stream, ref entryID))
                        return false;

                    Int32 size = 0;
                    if (!this.MldbReadInteger(stream, ref size))
                        return false;

                    byte[] buffer = new byte[size];
                    stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer);

                    MdbEntry entry = new MdbEntry(entryID, 0, message);
                    category.Add(entry);
                }
            }
            catch { }
            return false;
        }

        protected bool MldbReadInteger(FileStream stream, ref Int32 Integer)
        {
            try
            {
                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                Integer = BitConverter.ToInt32(buffer, 0);
                return true;
            }
            catch { return false; }
        }
        #endregion

        public MdbCategory GetCategory(Int32 categoryID)
        {
            if (this._categories != null)
                lock (this._categories)
                    foreach (MdbCategory category in this._categories)
                        if (category.CategoryID == categoryID)
                            return category;

            return null;
        }

        public MdbEntry GetEntry(Int32 categoryID, Int32 entryID)
        {
            if (!this._isLoaded)
                return this.SpeedRead(categoryID, entryID);

            MdbCategory category = this.GetCategory(categoryID);
            if (category == null)
                return null;

            return category.GetEntry(entryID);
        }

        public void ToXml(ref Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MdbReader));
            serializer.Serialize(stream, this);
        }
    }
}

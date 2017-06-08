using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using AoLib.Net;

namespace AoLib.Utils
{
    public static class XML
    {
        public static readonly string URL_OfficialWhois = "http://people.anarchy-online.com/character/bio/d/{1}/name/{0}/bio.xml";
        public static readonly string URL_OfficialServerStatus = "http://probes.funcom.com/ao.xml";
        public static readonly string URL_OfficialOrganization = "http://people.anarchy-online.com/org/stats/d/{0}/name/{1}/basicstats.xml";
        public static readonly string URL_AunoWhois = "http://auno.org/ao/char.php?output=xml&dimension={1}&name={0}";

        private static string _xmlCachePath = "xmlcache";
        public static string XmlCachePath { get { return _xmlCachePath; } }
        public static readonly string WhoisPath = "whois";
        public static readonly string OrganizationPath = "organization";

        private static Int32 CacheDuration = 8;
        private static bool VerbalMode = false;

        #region Whois
        public static WhoisResult GetWhois(string character, Server server) { return GetWhois(character, server, true, true); }
        public static WhoisResult GetWhois(string character, Server server, bool readCache, bool writeCache) { return GetWhois(character, server, readCache, writeCache, false); }
        internal static WhoisResult GetWhois(string character, Server server, bool readCache, bool writeCache, bool shutup)
        {
            if (server == "Test")
                return null;

            WhoisResult result = null;
            character = character.ToLower();
            if (readCache)
            {
                result = GetCachedWhois(character, server, false);
                if (result != null)
                {
                    if (!shutup)
                        Output("Retrieved {0}@{1} from whois cache", character, server.ToString());
                    return result;
                }
            }

            result = GetWhois(String.Format(URL_OfficialWhois, character, (int)server), 15000);
            if (result != null)
            {
                if (writeCache)
                    SetCachedWhois(result, server);
                if (!shutup)
                    Output("Retrieved {0}@{1} from funcom's whois server", character, server.ToString());
                return result;
            }

            result = GetWhois(String.Format(URL_AunoWhois, character, (int)server), 35000);
            if (result != null)
            {
                if (writeCache)
                    SetCachedWhois(result, server);
                if (!shutup)
                    Output("Retrieved {0}@{1} from auno's whois server", character, server.ToString());
                return result;
            }

            if (readCache)
            {
                result = GetCachedWhois(character, server, true);
                if (!shutup && result != null && result.Success)
                    Output("Retrieved {0}@{1} from old whois cache", character, server.ToString());
                else
                    Output("Unable to retrieve whois data for {0}@{1}", character, server.ToString());
                return result;
            }
            if (!shutup)
                Output("Unable to retrieve whois data for {0}@{1}", character, server.ToString());
            return null;
        }

        public static WhoisResult GetWhois(string url, Int32 timeout)
        {
            string xml = HTML.GetHtml(url, timeout);
            if (xml != null)
            {
                Output("Downloaded {0}", url);
                return ParseWhois(xml);
            }
            Output("Unable to download {0}", url);
            return null;
        }

        private static WhoisResult GetCachedWhois(string character, Server server, bool ignoreDate)
        {
            character = character.ToLower();
            WhoisResult result = null;
            try
            {
                string path = XmlCachePath + Path.DirectorySeparatorChar + WhoisPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string file = path + Path.DirectorySeparatorChar + (int)server + "." + character + ".xml";
                if (File.Exists(file))
                {
                    TimeSpan time = DateTime.Now - File.GetLastWriteTime(file);
                    if (time.TotalHours > CacheDuration && ignoreDate == false)
                        return null;

                    FileStream stream = null;
                    StreamReader reader = null;
                    try
                    {
                        stream = File.OpenRead(file);
                        reader = new StreamReader((Stream)stream);
                        string xml = reader.ReadToEnd();
                        result = ParseWhois(xml);
                    }
                    catch { }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                        }
                        if (stream != null)
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        private static void SetCachedWhois(WhoisResult whois, Server server)
        {
            if (whois == null)
                return;
            if (whois.Name == null)
                return;
            if (whois.Name.Nickname == null)
                return;
            if (whois.Stats == null)
                return;
            if (whois.Stats.Level == 0)
                return;
            if (whois.Stats.Profession == null)
                return;

            string xml = WhoisToXml(whois);
            if (string.IsNullOrEmpty(xml))
                return;

            FileStream stream = null;
            StreamWriter writer = null;
            try
            {
                string path = XmlCachePath + Path.DirectorySeparatorChar + WhoisPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string file = path + Path.DirectorySeparatorChar + (int)server + "." + whois.Name.Nickname.ToLower() + ".xml";
                if (File.Exists(file))
                    File.Delete(file);
                stream = File.Create(file);
                writer = new StreamWriter((Stream)stream);
                writer.Write(xml);
            }
            catch { }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public static WhoisResult ParseWhois(string xml)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                XmlSerializer serializer = new XmlSerializer(typeof(WhoisResult));
                WhoisResult result = (WhoisResult)serializer.Deserialize(stream);
                stream.Close();
                stream.Dispose();
                return result;
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return null;
        }

        private static string WhoisToXml(WhoisResult whois)
        {
            string xml = null;
            MemoryStream stream = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WhoisResult));
                stream = new MemoryStream();
                serializer.Serialize(stream, whois);
                xml = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return xml;
        }
        #endregion

        #region Organization
        public static OrganizationResult GetOrganization(Int32 organizationID, Server server) { return GetOrganization(organizationID, server, true, true); }
        public static OrganizationResult GetOrganization(Int32 organizationID, Server server, bool readCache, bool writeCache) { return GetOrganization(organizationID, server, readCache, writeCache, 120000); }
        public static OrganizationResult GetOrganization(Int32 organizationID, Server server, bool readCache, bool writeCache, Int32 timeout)
        {
            if (server == "Test")
                return null;

            OrganizationResult result = null;
            if (readCache)
                result = GetCachedOrganization(organizationID, server, false);
            if (result != null)
            {
                Output("Retrieved {0}@{1} from organization cache", result.Name, server.ToString());
                return result;
            }

            result = GetOrganization(String.Format(URL_OfficialOrganization, (int)server, organizationID), timeout);
            if (result != null)
            {
                if (writeCache)
                    SetCachedOrganization(result, server);
                Output("Retrieved {0}@{1} from funcom's organization server", result.Name, server.ToString());
                return result;
            }

            result = GetCachedOrganization(organizationID, server, true);
            if (result != null)
            {
                Output("Retrieved {0}@{1} from old organization cache", result.Name, server.ToString());
                return result;
            }

            Output("Unable to retrieve organization data for ID:{0}@{1}", organizationID, server.ToString());
            return null;
        }

        public static OrganizationResult GetOrganization(string url, Int32 timeout)
        {
            string xml = HTML.GetHtml(url, timeout);
            if (xml != null)
            {
                Output("Downloaded {0}", url);
                return ParseOrganization(xml);
            }
            Output("Unable to download {0}", url);
            return null;
        }

        private static OrganizationResult GetCachedOrganization(Int32 organizationID, Server server, bool ignoreDate)
        {
            OrganizationResult result = null;
            try
            {
                string path = XmlCachePath + Path.DirectorySeparatorChar + OrganizationPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string file = path + Path.DirectorySeparatorChar + (int)server + "." + organizationID + ".xml";
                if (File.Exists(file))
                {
                    TimeSpan time = DateTime.Now - File.GetLastWriteTime(file);
                    if (time.TotalHours > CacheDuration && ignoreDate == false)
                        return null;

                    FileStream stream = null;
                    StreamReader reader = null;
                    try
                    {
                        stream = File.OpenRead(file);
                        reader = new StreamReader((Stream)stream);
                        string xml = reader.ReadToEnd();
                        OrganizationCache cache = new OrganizationCache();
                        cache = ParseOrganizationCache(xml);
                        result = cache.ToOrganizationResult(server);

                    }
                    catch { }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                        }
                        if (stream != null)
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        private static void SetCachedOrganization(OrganizationResult organization, Server server)
        {
            if (organization.ID < 1)
                return;
            if (string.IsNullOrEmpty(organization.Name))
                return;
            if (organization.Members == null || organization.Members.Items == null || organization.Members.Items.Length == 0)
                return;

            OrganizationCache cache = new OrganizationCache();
            cache.FromOrganizationResult(organization);
            string xml = OrganizationCacheToXml(cache);

            if (string.IsNullOrEmpty(xml))
                return;

            FileStream stream = null;
            StreamWriter writer = null;
            try
            {
                string path = XmlCachePath + Path.DirectorySeparatorChar + OrganizationPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string file = path + Path.DirectorySeparatorChar + (int)server + "." + organization.ID + ".xml";
                if (File.Exists(file))
                    File.Delete(file);
                stream = File.Create(file);
                writer = new StreamWriter((Stream)stream);
                writer.Write(xml);

                foreach (OrganizationMember member in organization.Members.Items)
                {
                    WhoisResult whois = member.ToWhoisResult(organization);
                    SetCachedWhois(whois, server);
                }
            }
            catch { }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public static OrganizationResult ParseOrganization(string xml)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationResult));
                OrganizationResult result = (OrganizationResult)serializer.Deserialize(stream);
                stream.Close();
                stream.Dispose();
                return result;
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return null;
        }

        private static string OrganizationToXml(OrganizationResult whois)
        {
            string xml = null;
            MemoryStream stream = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationResult));
                stream = new MemoryStream();
                serializer.Serialize(stream, whois);
                xml = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return xml;
        }

        private static OrganizationCache ParseOrganizationCache(string xml)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationCache));
                OrganizationCache result = (OrganizationCache)serializer.Deserialize(stream);
                stream.Close();
                stream.Dispose();
                return result;
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return null;
        }

        private static string OrganizationCacheToXml(OrganizationCache whois)
        {
            string xml = null;
            MemoryStream stream = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OrganizationCache));
                stream = new MemoryStream();
                serializer.Serialize(stream, whois);
                xml = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return xml;
        }
        #endregion

        #region History
        public static HistoryResult GetHistory(string character, Server server)
        {
            if (server == "Test")
                return null;

            character = character.ToLower();
            string xml = HTML.GetHtml(String.Format(URL_AunoWhois, character, (int)server));
            if (xml != null)
            {
                HistoryResult result = ParseHistory(xml);
                if (result != null)
                {
                    Output("Retrieved {0}@{1} from auno's history server", character, server.ToString());
                    return result;
                }
                Output("Unable to parse history data for {0}@{1}", character, server.ToString());
                return null;
            }
            Output("Unable to retrieve history data for {0}@{1}", character, server.ToString());
            return null;
        }

        public static HistoryResult ParseHistory(string xml)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                XmlSerializer serializer = new XmlSerializer(typeof(HistoryResult_Root));
                HistoryResult_Root result = (HistoryResult_Root)serializer.Deserialize(stream);
                if (result != null)
                    return result.History;
            }
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return null;
        }
        #endregion

        #region Server Status
        public static ServerStatusResult GetServerStatus()
        {
            string xml = HTML.GetHtml(URL_OfficialServerStatus);
            if (xml != null)
                return ParseServerStatus(xml);
            else
                return null;
        }

        public static ServerStatusResult ParseServerStatus(string xml)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                XmlSerializer serializer = new XmlSerializer(typeof(ServerStatusResult));
                ServerStatusResult result = (ServerStatusResult)serializer.Deserialize(stream);
                if (result != null)
                    return result;
            }
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return null;
        }
        #endregion

        #region Configuration
        public static void SetCacheDuration(Int32 hours)
        {
            CacheDuration = hours;
        }

        public static void SetVerbalMode(bool enabled)
        {
            VerbalMode = enabled;
        }

        public static void SetXmlCachePath(string path)
        {
            _xmlCachePath = path;
        }
        #endregion

        private static void Output(string message)
        {
            if (VerbalMode)
                Console.WriteLine("[{0}] [XML] {1}", DateTime.Now.ToLongTimeString(), message);
        }

        private static void Output(string message, params object[] arg)
        {
            if (VerbalMode)
                Console.WriteLine("[{0}] [XML] {1}", DateTime.Now.ToLongTimeString(), String.Format(message, arg));
        }
    }
}

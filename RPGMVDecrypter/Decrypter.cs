using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace RPGMVDecrypter
{
    public class Decrypter
    {
        string programpath = ".";
        public Decrypter()
        {

        }
        public Decrypter(string path)
        {
            programpath = path;
        }
        int _headerlength = 16;
        List<string> _encryptionKey = new List<string>();
        string SIGNATURE = "5250474d56000000";
        string VER = "000301";
        string REMAIN = "0000000000";
        string sysConfig = "/www/data/System.json";
        private string pathHead = "Dump";
        private string GetNewPath(string fullpath)
        {
            string relativepath = extToBase(fullpath).Substring(programpath.Length);
            return pathHead + relativepath;
        }
        private string extToBase(string fullpath)
        {
            string ext = Path.GetExtension(fullpath);
            switch (ext)
            {
                case ".rpgmvo":
                    return fullpath.Replace(ext, ".ogg");
                case ".rpgmvm":
                    return fullpath.Replace(ext, ".m4a");
                case ".rpgmvp":
                    return fullpath.Replace(ext, ".png");
            }
            return fullpath;
        }

        public bool readEncryptionkey()
        {
            if (File.Exists(programpath + sysConfig))
            {
                var str = File.ReadAllText(programpath + sysConfig);
                JObject jobject = JObject.Parse(str);
                JProperty jp = jobject.Property("encryptionKey");
                if (jp != null)
                {
                    _encryptionKey = Regex.Matches(jobject["encryptionKey"].ToString(), ".{2}").Cast<Match>().Select(m => m.Value).ToList();
                    return true;
                }
            }
            return false;
        }

        public bool decryptArrayBuffer(string filePath)
        {
            try
            {
                byte[] arrayBuffer = File.ReadAllBytes(filePath);
                List<byte> bl = arrayBuffer.ToList();
                var header = bl.Take(_headerlength).ToArray();

                var reft = this.SIGNATURE + this.VER + this.REMAIN;

                var refBytes = new List<int>(16);
                for (int i = 0; i < this._headerlength; i++)
                {
                    refBytes.Insert(i, Convert.ToInt32(reft.Substring(i * 2, 2), 16));
                }
                for (int i = 0; i < this._headerlength; i++)
                {
                    if (header[i] != refBytes[i])
                    {
                        return false;
                    }
                }
                var byteArray = bl.Skip(_headerlength).ToList();
                for (int i = 0; i < this._headerlength; i++)
                {
                    byteArray[i] = (byte)(byteArray[i] ^ Convert.ToInt32(_encryptionKey[i], 16));
                }
                File.WriteAllBytes(filePath, byteArray.ToArray());
                File.Delete(extToBase(filePath));
                File.Move(filePath, extToBase(filePath));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
    public static class DirectoryInfoExtension
    {
        public static List<FileInfo> GetCustomFilesInfos(this DirectoryInfo value, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            string[] ps = pattern.Split('|');
            List<FileInfo> list = new List<FileInfo>();
            foreach (var s in ps)
            {
                list.AddRange(value.GetFiles(s, option));
            }
            return list.OrderBy(r => r.Name).ToList();
        }
        public static List<string> GetCustomFilesPaths(this DirectoryInfo value, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return value.GetCustomFilesInfos(pattern, option).Select(r => r.FullName).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mrivai.Pelitabangsa
{
    public class Loaders
    {
        private static string AppsPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        private string LoaderConfig = AppsPath + "Loader.json";
		public List<MerekHP> ListLoader = new List<MerekHP>();

        public Loaders()
        {
            if (!File.Exists(LoaderConfig))
                WriteFile(LoaderConfig, "");
            Task.Run(() => ReloadData());
        }

        public void ReloadData()
        {
            try
            {
                var data = ReadFile(LoaderConfig);
                ListLoader = JsonConvert.DeserializeObject<List<MerekHP>>(data);
            }
            catch (Exception ex)
            {

            }
        }

        public string GetLoader(string brand, string model)
        {
            string result = "Not Found";
            foreach (MerekHP x in ListLoader)
            {
                if (x.Name == brand)
                {
                    foreach (ModelHP y in x.Models)
                    {
                        if (y.Name == model)
                        {
                            result = y.ProgrammerPath;
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        public void AddOemHash(string brand, string hash)
        {
            OemHash H = new OemHash();
            H.Hash = hash;

            foreach(MerekHP M in ListLoader){
                M.Hashs.Add(H);
            }
            Save();
        }

        public void AddLoader(string Brand, string Model, string Cpumodel, string ProgrammerFilePath)
        {
            ModelHP M = new ModelHP();
            M.Name = Model;
            M.Cpu = Cpumodel;
            M.ProgrammerPath = ProgrammerFilePath;
            MerekHP Hp = new MerekHP();
            Hp.Name = Brand;

            foreach (MerekHP X in ListLoader)
            {
                if (X.Name == Brand)
                {
                    foreach (ModelHP Y in X.Models)
                    {
                        if (Y.Name != Model)
                        {
                            X.Models.Add(M);
                        }
                    }
                }
                else
                {
                    
                    Hp.Models.Add(M);
                }
            }
            ListLoader.Add(Hp);
            Save();
        }

        public void Save(bool immediate = false)
        {
            try
            {
                var data = JsonConvert.SerializeObject(ListLoader);
                if (immediate)
                    WriteFile(LoaderConfig, data);
                else
                    WriteFileAsync(LoaderConfig, data);
            }
            catch (Exception ex)
            {

            }
        }

        private static void WriteFile(string path, string contents)
        {
            using (var str = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var fileBytes = Encoding.ASCII.GetBytes(contents);
                str.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        private static void WriteFileAsync(string path, string contents)
        {
            var t = new Thread(() => WriteFile(path, contents));
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();
        }

        public static string ReadFile(string url, string onError = null)
        {
            int maxFileSize = 20 * 1024 * 1024;
            using (var str = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var utf8 = new UTF8Encoding();
                byte[] fileBytes;
                int numBytes;
                do
                {
                    fileBytes = new byte[maxFileSize];
                    numBytes = str.Read(fileBytes, 0, maxFileSize);

                    if (numBytes >= maxFileSize)
                        maxFileSize *= 2;

                } while (numBytes == maxFileSize);
                return utf8.GetString(fileBytes, 0, numBytes);
            }
        }
    }
    public class MerekHP
    {
        public string Name { get; set; } //brand name
        public List<ModelHP> Models = new List<ModelHP>(); // model name
        public List<OemHash> Hashs = new List<OemHash>(); 
    }

    public class ModelHP
    {
        public string Name { get; set; }
        public string Cpu { get; set; }
        public string ProgrammerPath { get; set; } = string.Empty;
    }

    public class OemHash
    {
        public string Hash { get; set; }
    }
}


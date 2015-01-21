using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace HaZera
{
    class AppStorage
    {
        private const string DEFAULT_FILENAME = "storage.json";

        static public void Save(List<SeedsBag> bags, string filename = DEFAULT_FILENAME)
        {
            string jsonString = JsonConvert.SerializeObject(bags, Formatting.Indented);
            File.WriteAllText(filename, jsonString);
        }

        static public List<SeedsBag> Load(string filename = DEFAULT_FILENAME)
        {
            List<SeedsBag> _plans = new List<SeedsBag>();
            if (File.Exists(filename))
            {
                _plans = JsonConvert.DeserializeObject<List<SeedsBag>>(File.ReadAllText(filename));
            }
            return _plans;
        }
    }
}

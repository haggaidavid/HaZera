using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaZera
{
    class SeedsBag
    {
        public string BagName { set; get; }
        public string FieldName { set; get; }
        public string Comment { set; get; }
        public int SeedsToPlant { set; get; }
        public int SeedsToSample { set; get; }
        public List<string> Samples { set; get; }

        public SeedsBag()
        {
            BagName = "";
            FieldName = "";
            Comment = "";
            SeedsToPlant = -1;
            SeedsToSample = -1;
            Samples = null;
        }

        public SeedsBag(string name, string field, int toPlant, int toSample, string samples, string comment)
        {
            BagName = name;
            FieldName = field;
            Comment = comment;
            SeedsToPlant = toPlant;
            SeedsToSample = toSample;
            Samples = new List<string>();
            foreach (string s in samples.Split(','))
            {
                if (!string.IsNullOrEmpty(s.Trim()))
                {
                    Samples.Add(s.Trim());
                }
            }
        }

        public static readonly SeedsBag EmptyBag = new SeedsBag();
    }
}

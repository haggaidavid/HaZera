using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaZera
{
    class SamplePlate
    {
        public const int SamplesCapacity = 92;

        private SortedSet<string> m_Samples;
        private HashSet<Tray> m_sourceTrays;
        private Dictionary<SeedsBag, int> m_sourceBags;

        private int m_nSeedsCount;
        public string Name { set; get; }

        public SamplePlate(string name)
        {
            Name = name;
            m_Samples = new SortedSet<string>();
            m_sourceTrays = new HashSet<Tray>();
            m_sourceBags = new Dictionary<SeedsBag, int>();
            m_nSeedsCount = 0;
        }

        public bool isFull()
        {
            return m_nSeedsCount >= SamplesCapacity;
        }

        public int AddSource(Tray tray, SeedsBag bag, int count)
        {
            int added = 0;

            // validate params
            if (bag == SeedsBag.EmptyBag || count == 0)
            {
                return 0;
            }

            // add samples
            m_Samples.UnionWith(bag.Samples);

            // add source
            added = count;
            if (m_nSeedsCount + added > SamplesCapacity)
            {
                added = SamplesCapacity - m_nSeedsCount;
            }
            m_sourceTrays.Add(tray);
            if (!m_sourceBags.ContainsKey(bag))
            {
                m_sourceBags.Add(bag, added);
            }
            else
            {
                m_sourceBags[bag] += added;
            }
            m_nSeedsCount += added;

            return added;
        }

        public int NumberOfSourceTrays
        {
            get
            {
                return m_sourceTrays.Count;
            }
        }

        public int SeedsCount
        {
            get
            {
                return m_nSeedsCount;
            }
        }

        public int SamplesToMake
        {
            get
            {
                return SeedsCount * m_Samples.Count;
            }
        }

        public string AsString()
        {
            string str = "";

            str += "{";
            str += "Name=" + Name + ", ";
            str += "count=" + SeedsCount + ", ";
            str += "[" + string.Join(",", m_Samples) + "]";
            str += " [";
            foreach (Tray t in m_sourceTrays)
            {
                str += "<tray: " + t.Name + ">, ";
            }
            foreach (KeyValuePair<SeedsBag, int> kvp in m_sourceBags)
            {
                str += "<" + kvp.Key.BagName + ": " + kvp.Value + ">, ";
            }
            str += "] ";
            str += "}";

            return str;
        }
    }
}

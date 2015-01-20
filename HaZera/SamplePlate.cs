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
        private List<Tuple<Tray, SeedsBag, int>> m_sourceBags;
        private int m_nSeedsCount;
        public string Name { set; get; }

        public SamplePlate(string name)
        {
            Name = name;
            m_Samples = new SortedSet<string>();
            m_sourceBags = new List<Tuple<Tray, SeedsBag, int>>();
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
            added = m_nSeedsCount + count;
            if (added > SamplesCapacity)
            {
                added = SamplesCapacity - m_nSeedsCount;
            }
            m_sourceBags.Add(new Tuple<Tray,SeedsBag,int>(tray, bag, added));

            return added;
        }

        public int NumberOfSourceTrays
        {
            get
            {
                return m_sourceBags.Count;
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

            str += "SamplePlate " + Name + ": ";
            str += "Count=" + SeedsCount + ", ";
            str += "Samples=[" + string.Join(",", m_Samples) +"], ";

            return str;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaZera
{
    class Tray
    {
        public const int NumberOfRows = 17;
        public const int CellsInRow = 11;

        private List<SeedsBag> m_rows;
        public List<SeedsBag> Rows 
        {
            get { return m_rows; } 
        }

        public Tray()
        {
            m_rows = new List<SeedsBag>();
        }

        public bool isFull()
        {
            return m_rows.Count >= NumberOfRows;
        }

        public int AddSeedsFromBag(SeedsBag bag)
        {
            return AddSeedsFromBag(bag, bag.SeedsToPlant);
        }

        public int AddSeedsFromBag(SeedsBag bag, int seedsCount)
        {
            int seedsAdded = 0;
            while (!isFull() && seedsAdded < seedsCount)
            {
                // add a new row of seeds from this bag
                m_rows.Add(bag);
                seedsAdded += CellsInRow;
            }

            if (!isFull())
            {
                // add an empty row
                m_rows.Add(SeedsBag.EmptyBag);
            }

            return seedsAdded;
        }

        public string AsString()
        {
            string str = "";

            str += "{";
            for (int row = 0; row < NumberOfRows && row < Rows.Count; ++row)
            {
                SeedsBag sb = Rows[row];
                str += sb.BagName + ",";
            }

            str += "}";
            return str;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaZera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            List<SeedsBag> bags = new List<SeedsBag>();
            for (int i = 1; i <= 10; ++i)
            {
                string name = "bag" + i;
                string field = "field" + i;
                int seeds = random.Next(1, 12) * Tray.CellsInRow;
                int samples_count = random.Next(0, 4);
                string samples = "";
                for (int s = 0; s < samples_count; ++s)
                {
                    samples += "s" + random.Next(0, 9) + ",";
                }
                SeedsBag bg = new SeedsBag(name, field, seeds, seeds, samples, "");
                bags.Add(bg);
                Console.WriteLine(bg.AsString());
            }

            int tray_count = 0;
            string tray_name = "tray" + tray_count;
            List<Tray> trays = new List<Tray>();
            Tray tray = new Tray(tray_name);
            foreach (SeedsBag sb in bags)
            {
                int added = tray.AddSeedsFromBag(sb);
                if (added < sb.SeedsToPlant)
                {
                    ++tray_count;
                    tray_name = "tray" + tray_count;
                    Console.WriteLine(tray.AsString());
                    trays.Add(tray);
                    tray = new Tray(tray_name);
                    tray.AddSeedsFromBag(sb, sb.SeedsToPlant - added);
                }
            }
            Console.WriteLine(tray.AsString());
            trays.Add(tray);

            int plate_number = 1;
            string plateName = "plate" + plate_number;
            SamplePlate sample_plate = new SamplePlate(plateName);
            foreach (Tray t in trays)
            {
                foreach (SeedsBag sb in t.Rows)
                {
                    if (!string.IsNullOrEmpty(sb.BagName))
                    {
                        int added = sample_plate.AddSource(t, sb, Tray.CellsInRow);
                        if (added < Tray.CellsInRow)
                        {
                            Console.WriteLine(sample_plate.AsString());
                            plate_number++;
                            plateName = "Plate" + plate_number;
                            sample_plate = new SamplePlate(plateName);
                            sample_plate.AddSource(t, sb, Tray.CellsInRow - added);
                        }
                    }
                }
            }
            Console.WriteLine(sample_plate.AsString());
        }
    }
}

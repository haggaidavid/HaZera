using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GAF;
using GAF.Operators;
using GAF.Extensions;

namespace HaZera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PopulateTestPlan();
        }

        private static List<SeedsBag> _bags;
        private static int _worst = 0;

        private void LoadTestPlan()
        {
            SortedSet<string> allsamples = new SortedSet<string>();
            int allseeds = 0;

            _bags = new List<SeedsBag>();

            _bags = AppStorage.Load();
            foreach (SeedsBag sb in _bags)
            {
                allseeds += sb.SeedsToPlant;
                allsamples.UnionWith(sb.Samples);
            }
            _worst = allseeds * allsamples.Count;
        }

        private void SaveTestPlan()
        {
            AppStorage.Save(_bags);
        }

        private void PopulateTestPlan()
        {
            Random random = new Random();
            SortedSet<string> allsamples = new SortedSet<string>();
            int allseeds = 0;

            _bags = new List<SeedsBag>();
            for (int i = 1; i <= 20; ++i)
            {
                string name = "bag" + i;
                string field = "field" + i;
                int seeds2plant = random.Next(1, 12) * Tray.CellsInRow;
                int seeds2sample = seeds2plant;
                int samples_count = random.Next(0, 4);
                string samples = "";
                for (int s = 0; s < samples_count; ++s)
                {
                    samples += "s" + random.Next(0, 9) + ",";
                }
                if (random.Next(0, 3) == 0)
                {
                    seeds2sample = 0;
                }
                SeedsBag bg = new SeedsBag(name, field, seeds2plant, seeds2sample, samples, "");
                _bags.Add(bg);

                Console.WriteLine(bg.AsString());

                allsamples.UnionWith(bg.Samples);
                allseeds += seeds2sample;
            }

            foreach (SeedsBag sb in _bags)
            {
                allseeds += sb.SeedsToPlant;
                allsamples.UnionWith(sb.Samples);
            }
            _worst = allseeds * allsamples.Count;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numberOfGenes = _bags.Count;
            int populationSize = 10;
            Population pop = new Population(populationSize);
            for (int i = 0; i < populationSize; ++i)
            {
                Chromosome ch = new Chromosome();
                for (int g = 0; g < numberOfGenes; g++)
                {
                    ch.Genes.Add(new Gene(g));
                }
                ch.Genes.Shuffle();
                pop.Solutions.Add(ch);
            }

            Elite elite = new Elite(10);

            Crossover crossover = new Crossover(0.8)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            SwapMutate mutate = new SwapMutate(0.02);

            GeneticAlgorithm ga = new GeneticAlgorithm(pop, CalculateFitness);
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            ga.Run(Terminate);

        }

        void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            Chromosome fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Console.WriteLine(_bags[(int)gene.RealValue].BagName);
            }
            CalculateFitness(fittest, true);
        }

        void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            Chromosome ch = e.Population.GetTop(1)[0];
            var fitness = CalculateFitness(ch);
            Console.WriteLine("Generation: {0}, Fitness: {1}, chromosome: {2}", e.Generation, ch.Fitness, PrintChromosome(ch));
        }

        private static Tray NewTray(int n)
        {
            string name = "" + n;
            return new Tray(name);
        }

        private static Object _lock = new Object();
        public static string PrintChromosome(Chromosome ch)
        {
            string str = "";
            foreach (var gene in ch.Genes)
            {
                str += _bags[(int)gene.RealValue].BagName + ",";
            }

            return str;
        }

        public static double CalculateFitness(Chromosome ch)
        {
            return CalculateFitness(ch, false);
        }

        public static double CalculateFitness(Chromosome ch, bool verbose)
        {
            double fitness = 0.0;

            List<Tray> trays = new List<Tray>();
            Tray tray = NewTray(trays.Count);

            if (verbose) { Console.WriteLine("Seeds Bags:"); }
            foreach (Gene g in ch.Genes)
            {
                SeedsBag sb = _bags[(int)g.RealValue];
                if (verbose) { Console.WriteLine(sb.AsString()); }

                int added = tray.AddSeedsFromBag(sb);
                if (added < sb.SeedsToPlant)
                {
                    trays.Add(tray);
                    tray = NewTray(trays.Count);
                    tray.AddSeedsFromBag(sb, sb.SeedsToPlant - added);
                }
            }
            if (tray.Rows.Count > 0)
            {
                trays.Add(tray);
            }

            if (verbose) { Console.WriteLine("Trays:"); }
            List<SamplePlate> allplates = new List<SamplePlate>();
            SamplePlate sp = new SamplePlate("P" + allplates.Count);
            foreach (Tray t in trays)
            {
                if (verbose) { Console.WriteLine(t.AsString()); }

                foreach (SeedsBag sb in t.Rows)
                {
                    if (!string.IsNullOrEmpty(sb.BagName))
                    {
                        if (sb.SeedsToSample > 0)
                        {
                            int added = sp.AddSource(t, sb, Tray.CellsInRow);
                            if (added < Tray.CellsInRow)
                            {
                                allplates.Add(sp);
                                sp = new SamplePlate("P" + allplates.Count);
                                int toadd = Tray.CellsInRow - added;
                                added = sp.AddSource(t, sb, toadd);
                                if (added != toadd)
                                {
                                    throw new Exception("ERROR: failed to add seeds to new plate: " + added + " - " + toadd );
                                }
                            }
                        }
                    }
                }
            }
            if (sp.SamplesToMake > 0)
            {
                allplates.Add(sp);
            }

            // count the samples required
            if (verbose) { Console.WriteLine("Sample Plates:"); }
            int allsamples = 0;
            foreach (SamplePlate sp2 in allplates)
            {
                if (verbose) { Console.WriteLine(sp2.AsString()); }

                allsamples += sp2.SamplesToMake;
            }
            fitness = _worst - allsamples;
            if (verbose)
            {
                Console.WriteLine("Fitness: {0} = {1} - {2}", fitness, _worst, allsamples);
                Console.WriteLine("");
            }

            //Console.WriteLine("Fitness: {0} of chromosome {1}", fitness, PrintChromosome(ch));

            return fitness;

        }

        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 100;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using GAF;
using GAF.Operators;
using GAF.Extensions;

//using NPOI.HSSF.Model;
//using NPOI.HSSF.UserModel;
using NPOI.XSSF.Model;
using NPOI.XSSF.UserModel;


namespace HaZera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PopulateTestPlan();
        }

        public const int FIELD_COLUMN_NUMBER = 0;
        public const int BAGNAME_COLUMN_NUMBER = 1;
        public const int SEEDSTOPLANT_COLUMN_NUMBER = 2;
        public const int SEEDSTOSAMPLE_COLUMN_NUMBER = 3;
        public const int SAMPLES_COLUMN_NUMBER = 4;
        public const int COMMENT_COLUMN_NUMBER = 5;

        public const int MAX_GENERATIONS = 100;
        public const int POPULATION_SIZE = 1000;
        public const int ELITE_PERCENTAGE = 5;
        public const double CROSSOVER_PROBABILITY = 0.8;
        public const double MUTATE_PROBABLITY = 0.1;


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

        private int[] CreateSuffledArray(int count)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            int[] arr = new int[count];

            for (int i = 0; i < count; ++i)
            {
                arr[i] = i;
            }

            for (int i = 0; i < count; ++i)
            {
                int a = random.Next(0, count - 1);
                int b = random.Next(0, count - 1);
                int tmp = arr[a];
                arr[a] = arr[b];
                arr[b] = tmp;
            }

            return arr;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            doWork();
        }

        private void doWork()
        {
            int numberOfGenes = _bags.Count;
            int populationSize = POPULATION_SIZE;
            Population pop = new Population(populationSize);
            for (int i = 0; i < populationSize; ++i)
            {
                Console.WriteLine("Adding chromosome number " + i);
                int[] shuffled = CreateSuffledArray(numberOfGenes);
                Chromosome ch = new Chromosome();
                for (int g = 0; g < numberOfGenes; g++)
                {
                    if (i == 0)
                    {
                        ch.Genes.Add(new Gene(g));
                    }
                    else
                    {
                        ch.Genes.Add(new Gene(shuffled[g]));
                    }
                }
                //ch.Genes.Shuffle();
                pop.Solutions.Add(ch);
            }

            Elite elite = new Elite(ELITE_PERCENTAGE);

            Crossover crossover = new Crossover(CROSSOVER_PROBABILITY)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            SwapMutate mutate = new SwapMutate(MUTATE_PROBABLITY);

            GeneticAlgorithm ga = new GeneticAlgorithm(pop, CalculateFitness);
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            progressBar1.Minimum = 0;
            progressBar1.Maximum = MAX_GENERATIONS;
            progressBar1.Value = 0;

            ga.Run(Terminate);

        }

        void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            Chromosome fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Console.WriteLine(_bags[(int)gene.RealValue].BagName);
            }
            //CalculateFitness(fittest, true);
            SaveSolutionToExcel(fittest);
        }

        void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            progressBar1.Value = (e.Generation < progressBar1.Maximum)?e.Generation:progressBar1.Maximum;
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
            /*
            foreach (var gene in ch.Genes)
            {
                str += _bags[(int)gene.RealValue].BagName + ",";
            }
             */ 

            return str;
        }

        public static double CalculateFitness(Chromosome ch)
        {
            return CalculateFitness(ch, false);
        }

        public static double CalculateFitness(Chromosome ch, bool verbose)
        {
            //Console.WriteLine("calculating fitness... ");

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
            //Console.WriteLine("Fitness: {0} ", fitness);

            return fitness;

        }

        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > MAX_GENERATIONS;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm|All Files|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputFile.Text = dlg.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename = inputFile.Text;

            if (string.IsNullOrWhiteSpace(filename))
            {
                return;
            }

            if (!File.Exists(filename))
            {
                return;
            }

            XSSFWorkbook workbook = new XSSFWorkbook(new FileStream(filename, FileMode.Open, FileAccess.Read));
            XSSFSheet sheet = (XSSFSheet)workbook.GetSheetAt(0);

            listView.Items.Clear();
            _bags.Clear();

            XSSFRow row = null;
            for (int r = 1; (row = (XSSFRow)sheet.GetRow(r)) != null; ++r)
            {
                string strFieldName;
                XSSFCell cell;
                cell = (XSSFCell)row.GetCell(FIELD_COLUMN_NUMBER);
                if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
                {
                    strFieldName = cell.NumericCellValue.ToString();
                }
                else
                {
                    strFieldName = cell.StringCellValue;
                }
                string strBagName = row.GetCell(BAGNAME_COLUMN_NUMBER).StringCellValue;
                int nSeedsToPlant = (int)row.GetCell(SEEDSTOPLANT_COLUMN_NUMBER).NumericCellValue;
                int nSeedsToSample = (int)row.GetCell(SEEDSTOSAMPLE_COLUMN_NUMBER).NumericCellValue;

                //cell = (XSSFCell)row.GetCell(SAMPLES_COLUMN_NUMBER, NPOI.SS.UserModel.MissingCellPolicy.CREATE_NULL_AS_BLANK);
                string strSamples = row.GetCell(SAMPLES_COLUMN_NUMBER, NPOI.SS.UserModel.MissingCellPolicy.CREATE_NULL_AS_BLANK).StringCellValue;
                string strComment = row.GetCell(COMMENT_COLUMN_NUMBER, NPOI.SS.UserModel.MissingCellPolicy.CREATE_NULL_AS_BLANK).StringCellValue;

                ListViewItem lvi = new ListViewItem(strFieldName);
                lvi.SubItems.Add(strBagName);
                lvi.SubItems.Add(nSeedsToPlant.ToString());
                lvi.SubItems.Add(nSeedsToSample.ToString());
                lvi.SubItems.Add(strSamples);
                lvi.SubItems.Add(strComment);

                listView.Items.Add(lvi);

                SeedsBag sb = new SeedsBag(strBagName, strFieldName, nSeedsToPlant, nSeedsToSample, strSamples, strComment);
                _bags.Add(sb);
            }

            SaveTestPlan();
            LoadTestPlan();

        }

        private void SaveSolutionToExcel(Chromosome ch)
        {
            string filename = inputFile.Text + ".solution.xlsx";
            FileStream output = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);

            XSSFWorkbook wb = new XSSFWorkbook();
            XSSFSheet sheet = (XSSFSheet)wb.CreateSheet("Solution");
            XSSFRow row = (XSSFRow)sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("חלקה");
            row.CreateCell(1).SetCellValue("שקית זרעים");
            row.CreateCell(2).SetCellValue("זריעה");
            row.CreateCell(3).SetCellValue("PCR");
            row.CreateCell(4).SetCellValue("עמידות");
            row.CreateCell(5).SetCellValue("הערות");


            for (int r = 0; r < ch.Genes.Count; ++r)
            {
                Gene gene = ch.Genes[r];
                row = (XSSFRow)sheet.CreateRow(r + 1);
                SeedsBag sb = _bags[(int)gene.RealValue];
                row.CreateCell(0).SetCellValue(sb.FieldName);
                row.CreateCell(1).SetCellValue(sb.BagName);
                row.CreateCell(2).SetCellValue(sb.SeedsToPlant);
                row.CreateCell(3).SetCellValue(sb.SeedsToSample);
                row.CreateCell(4).SetCellValue(string.Join(",", sb.Samples));
                row.CreateCell(5).SetCellValue(sb.Comment);
            }

            wb.Write(output);
            output.Close();
        }
    }
}

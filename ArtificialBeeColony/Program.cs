using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ArtificialBeeColony
{
    class Program
    {

        public static Random rand = new Random();
        public static List<String> toFile = new List<String>();

        public struct Bee
        {
            //status: true -> zadeklarowana, false -> niezadeklarowana
            public bool status;
            public List<UInt32> path;


            public void SetPath(List<UInt32> newPath)
            {
                path = newPath;
            }
        }

        static void Main(string[] args)
        {
            string pathFile = @"TSP\kroA100.tsp";
            Point[] points = TSPFileReader.ReadTspFile(pathFile);
            Graph graph = new Graph(points);
            toFile.Add("Artificial Bee Colony for " + pathFile);
            Console.WriteLine("Set count of bees: ");
            UInt32 bees = UInt32.Parse(Console.ReadLine());
            toFile.Add("Count of bee: |" + bees);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Solve(graph, bees);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            toFile.Add("Time: " + elapsedTime);
            SaveToFile(toFile, "ABC", "kroA100");
            Console.ReadKey();

        }

        public static void Solve(Graph graph, UInt32 bees)
        {
            List<UInt32> bestPath = new List<UInt32>();
            UInt32 epoch = 0;
            UInt32 maxEpochs = graph.dimension - 1;

            for (int k = 0; k < graph.dimension; k++)
            {
                bestPath.Add((UInt32)k);
            }
            double bestPathLength = graph.CalculateRouteLength(bestPath);
            Console.WriteLine("Best path is: " + bestPathLength);

            Bee[] hive = new Bee[bees]; 
            for(int i = 0; i < hive.Length / 2; i++)
            {
                hive[i].status = true;
                hive[i].path = new List<UInt32>();
            }

            for(int j = hive.Length / 2; j < hive.Length; j++)
            {
                hive[j].status = false;
                hive[j].path = new List<UInt32>();
            }

            
            while(epoch < maxEpochs)
            {
                double newPath = 0;
                //int j = 0;
                foreach (Bee bee in hive)
                {
                    bee.path.Add(epoch);

                }

                for(int k = 0; k < graph.dimension - 1; k++)
                {
                    foreach (Bee bee in hive)
                    {
                        if (!bee.status) continue;
                        UInt32 next = ChooseNextNode(bee, graph.dimension - 1);
                        bee.path.Add(next);
                    }

                    double average = Average(hive, graph);

                    for (int i = 0; i < hive.Length; i++)
                    {
                        if (average < graph.CalculateRouteLength(hive[i].path))
                        {
                            hive[i].status = false;
                        }
                        else if (graph.CalculateRouteLength(hive[i].path) <= (average / 2))
                        {
                            Dancing(hive[i], hive);
                        }
                    }
                }
             

                newPath = FindBestPath(hive, graph);

                if(newPath < bestPathLength)
                {
                    bestPathLength = newPath;
                    Console.WriteLine("New best found in iteration: " + epoch + ", new best is: " + bestPathLength);
                    toFile.Add("-|New best |" + epoch +"|" + bestPathLength);
                }

                foreach (Bee bee in hive)
                {
                    bee.path.Clear();
                }

                epoch++;
            }
            Console.WriteLine("I ended");

        }

        public static bool Tabu(UInt32 nextNode, List<UInt32> nodeList)
        {
            bool wasHere = false;
            foreach(UInt32 node in nodeList)
            {
                if(nextNode == node)
                {
                    wasHere = true;
                }
            }

            return wasHere;
        }

        public static UInt32 ChooseNextNode(Bee bee, UInt32 graphDimension)
        {
            List<UInt32> nodeList = new List<UInt32>();
            for(UInt32 i = 0; i < graphDimension; i++)
            {
                nodeList.Add(i);
            }

            foreach(UInt32 node in bee.path)
            {
                nodeList.Remove(node);
            }

            foreach(UInt32 node in nodeList)
            {
                Console.Write(node);
                Console.Write(", ");
            }
            Console.WriteLine("___________________");

            int index = rand.Next(0,nodeList.Count);

            UInt32 nextNode = nodeList[index];

            return nextNode;
        }





        public static double Average(Bee [] bees, Graph graph)
        {
            double summary = 0;
            foreach(Bee bee in bees)
            {
                summary += graph.CalculateRouteLength(bee.path);
            }

            return summary / bees.Length;
        }

        public static void Dancing(Bee bee, Bee [] hive)
        {
            for(int i = 0; i < hive.Length; i++)
            {
                if (hive[i].status) continue;

                hive[i].SetPath(bee.path);
                hive[i].status = true;
            }
        }

        public static double FindBestPath(Bee [] hive, Graph graph)
        {
            double best = graph.CalculateRouteLength(hive[0].path);
            for(int i = 1; i < hive.Length; i++)
            {
                double nextBeePath = graph.CalculateRouteLength(hive[i].path);
                if (best > nextBeePath)
                {
                    best = nextBeePath;
                }
            }
            return best;

        }

        public static void SaveToFile(List<String> tofile, string algorithm, string startFile)
        {
            DateTime dt = DateTime.Now;
            string fileName = String.Format("{0:y yy yyy yyyy}", dt) + "-" + algorithm + "-" + startFile;
            using (StreamWriter sw = new StreamWriter(@"Files\" + fileName + ".txt"))
            {
                foreach (string line in tofile)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}

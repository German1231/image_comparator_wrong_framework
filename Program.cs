using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace ImageComparator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            string path = @"D:\Документы\Скриншоты\"; //Console.ReadLine();
            List<Tuple<Image, List<string>>> repetitions = new List<Tuple<Image, List<string>>>();
            repetitions = checkDirectory(path, repetitions);
            foreach (var i in repetitions)
            {
                Console.WriteLine("Original: ", i.Item2[0]);
                for (int j = 1; j < i.Item2.Count(); ++j)
                {
                    FileInfo f = new FileInfo(i.Item2[j]);
                    f.Delete();
                    Console.WriteLine(i.Item2[j], " - deleted");
                }
                Console.WriteLine();
            }
            Console.Write("Completed");
            Console.ReadKey();
        }

        static List<Tuple<Image, List<string>>> checkDirectory(string path, List<Tuple<Image, List<string>>> repetitions)
        {
            List<string> paths = new List<string>();
            List<Image> images = new List<Image>();
            List<bool> check = new List<bool>();
            foreach (string f in Directory.GetFiles(path))
            {
                try
                {
                    Image im = Image.FromFile(f);
                    int point = checkInRepetitions(im, repetitions);
                    if (point == -1)
                    {
                        images.Add(im);
                        paths.Add(f);
                        check.Add(true);
                    }
                    else
                    {
                        repetitions[point].Item2.Add(f);
                    }
                }
                catch
                {
                }
            }

            for (int i = 0; i < paths.Count; ++i)
                if (check[i])
                    for (int j = i + 1; j < paths.Count; ++j)
                        if (check[j])
                            if (equalImages(images[i], images[j]))
                            {
                                check[j] = false;
                                if (check[i] == false)
                                    repetitions[repetitions.Count() - 1].Item2.Add(paths[j]);
                                else
                                    repetitions.Add(new Tuple<Image, List<string>>(images[i], new List<string>() { paths[i], paths[j] }));
                                check[i] = false;
                            }
            foreach (Image im in images)
                im.Dispose();
            return (repetitions);
        }

        static int checkInRepetitions(Image im, List<Tuple<Image, List<string>>> repetitions)
        {
            List<Thread> threads = new List<Thread>();
            bool[] res = new bool[repetitions.Count()];
            int i;
            Bitmap btm1 = new Bitmap(im);
            for (i = 0; i < repetitions.Count(); i++)
                threads.Add(new Thread(() =>
                {
                    bool mid_res = true;
                    if (im.Size != repetitions[i].Item1.Size)
                        mid_res = false;
                    else
                    {

                        Bitmap btm2 = new Bitmap(repetitions[i].Item1);

                        for (int j = 0; j < btm1.Width; ++j)
                            for (int k = 0; k < btm1.Height; ++k)
                                if (btm1.GetPixel(j, k) != btm2.GetPixel(j, k))
                                    mid_res = false;
                        res[i] = mid_res;
                    }
                }));
            i = 0;
            foreach (Thread th in threads)
                th.Start();
            foreach (Thread th in threads)
                th.Join();

            for (i = 0; i < repetitions.Count(); ++i)
                if (res[i])
                    return (i);
            return (-1);
        }

        static bool equalImages(Image im1, Image im2)
        {
            if (im1.Size != im2.Size)
                return (false);
            else
            {
                Bitmap btm1 = new Bitmap(im1);
                Bitmap btm2 = new Bitmap(im2);

                for (int i = 0; i < btm1.Width; ++i)
                    for (int j = 0; j < btm1.Height; ++j)
                        if (btm1.GetPixel(i, j) != btm2.GetPixel(i, j))
                            return (false);
                return (true);
            }
        }
    }
}
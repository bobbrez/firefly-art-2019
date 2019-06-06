using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ReduceLines
{
	class Program
	{
		struct Line
		{
			public double x1, y1, x2, y2;
			public double Length
			{
				get
				{
					double dx = x1 - x2;
					double dy = y1 - y2;
					return Math.Sqrt(dx * dx + dy * dy);
				}
			}
		}

		static List<Line> ReadFile(string filename)
		{
			List<double> nums = new List<double>();
			if (File.Exists(filename))
			{
				using (TextReader tr = new StreamReader(filename))
				{
					//string[] split = Regex.Split(tr.ReadToEnd(), @"(?m)\s+");
					string[] split = tr.ReadToEnd().Split(new char[] { ',', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (string num in split)
					{
						Double d;
						if (Double.TryParse(num, out d))
							nums.Add(d);
					}
				}
			}
			if ((nums.Count & 3) != 0)
			{
				Console.WriteLine("Error - value count not multiple of four");
				return null; // 
			}
			List<Line> lines = new List<Line>();
			for (int i = 0; i < nums.Count; i += 4)
				lines.Add(new Line() { x1 = nums[i], y1 = nums[i + 1], x2 = nums[i + 2], y2 = nums[i + 3] });
			return lines;
		}

		static double tolerance = 0.00001;

		static List<Line> RemoveDuplicates(List<Line> lines)
		{
			var remove = new List<Line>();
			for (int i = 0; i < lines.Count; ++i)
			{
				var l1 = lines[i];
				for (int j = i + 1; j < lines.Count; ++j)
				{
					var l2 = lines[j];
					double dx11 = (l1.x1 - l2.x1), dy11 = (l1.y1 - l2.y1);
					double dx22 = (l1.x2 - l2.x2), dy22 = (l1.y2 - l2.y2);
					double dx12 = (l1.x1 - l2.x2), dy12 = (l1.y1 - l2.y2);
					double dx21 = (l1.x2 - l2.x1), dy21 = (l1.y2 - l2.y1);
					double d11 = Math.Sqrt(dx11 * dx11 + dy11 * dy11);
					double d22 = Math.Sqrt(dx22 * dx22 + dy22 * dy22);
					double d12 = Math.Sqrt(dx12 * dx12 + dy12 * dy12);
					double d21 = Math.Sqrt(dx21 * dx21 + dy21 * dy21);
					if ((d11 + d22 < tolerance) || (d21 + d12 < tolerance))
					{
						remove.Add(l2);
					}
				}
			}
			foreach (var line in remove)
				lines.Remove(line);
			return lines;
		}

		// interpolate line to get parameter of point x,y on line starting at x0,y0
		// with slope dx,dy
		static double Linear(double dx, double dy, double x0, double y0, double x, double y)
		{
			if (Math.Abs(dx) > Math.Abs(dy))
				return (x - x0) / dx;
			return (y - y0) / dy;
		}

		static List<Line> MergeCollinearLines(List<Line> lines)
		{
			var remove = new List<Line>();
			bool someMergedThisPass;
			int merges = 0;
			do
			{
				someMergedThisPass = false;
				for (int i = 0; i < lines.Count; ++i)
				{
					var l1 = lines[i];
					double dx1 = l1.x1 - l1.x2; // slope
					double dy1 = l1.y1 - l1.y2;

					for (int j = i + 1; j < lines.Count; ++j)
					{
						var l2 = lines[j];
						double dx2 = l2.x1 - l2.x2;
						double dy2 = l2.y1 - l2.y2;
						// check slope
						if ((Math.Abs(dy1 * dx2 - dy2 * dx1) > tolerance))
							continue; // cannot be collinear

						// y-y1=m1(x-x1) is a line eqn, expand and see if point from second line on this line
						// via l2.y1-l1.y1 == dy1/dx1 * (l2.x1-l1.x1)
						if (Math.Abs((l2.y1 - l1.y1) * dx1 - (l2.x1 - l1.x1) * dy1) > tolerance)
							continue;

						// compute parametric form of line1, and see if either endpoint of line2 is within line1 span
						// form is (dx,dy)*s+(x2,y2) where s=0 to 1 for endpoints xy2 to xy1.
						// compute 2 s values a2, b2 on line 2, using parametrization of line 1. Line 1 has parameters 0 and 1
						double a = Linear(dx1, dy1, l1.x2, l1.y2, l2.x1, l2.y1);
						double b = Linear(dx1, dy1, l1.x2, l1.y2, l2.x2, l2.y2);

						bool overlaps = (-tolerance < a) && (a < 1 + tolerance);
						overlaps |= (-tolerance < b) && (b < 1 + tolerance);
						if (overlaps)
						{ // make one long line containing parameters 0,1,a,b, remove l1 and l2, and restart merging
							double min = Math.Min(Math.Min(Math.Min(0, 1), a), b);
							double max = Math.Max(Math.Max(Math.Max(0, 1), a), b);
							Line newLine = new Line();
							newLine.x1 = dx1 * min + l1.x2;
							newLine.y1 = dy1 * min + l1.y2;
							newLine.x2 = dx1 * max + l1.x2;
							newLine.y2 = dy1 * max + l1.y2;
							lines.Remove(l2);
							lines.Remove(l1);
							lines.Add(newLine);
							someMergedThisPass = true;
							++merges;
							goto matched;
						}
					}
				}
			matched: ; // Stupid :) C# required this semicolon, or another statement.
			} while (someMergedThisPass);

			Console.WriteLine("{0} merges done", merges);
			return lines;
		} // Merge

		static List<Line> RemoveZeroLength(List<Line> lines)
		{
			return lines.Where(a => a.Length > tolerance).ToList();
		}


		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage: infile, outfile");
				Console.WriteLine("Parse file, remove redundancies, merge collinear");
				Console.WriteLine("If output has .DXF extension, lines are output to DXF");
				return;
			}
			var lines = ReadFile(args[0]);
			Console.WriteLine("{0} lines read", lines.Count);

			lines = RemoveZeroLength(lines);
			Console.WriteLine("{0} lines after zero length ones removed", lines.Count);

			lines = RemoveDuplicates(lines);
			Console.WriteLine("{0} lines after duplicates removed", lines.Count);

			lines = MergeCollinearLines(lines);
			Console.WriteLine("{0} lines after lines merged", lines.Count);

			if (args[1].ToLower().EndsWith(".dxf"))
			{
				DXFWriter writer = new DXFWriter();
				List<double> vals = new List<double>();
				foreach (var line in lines)
				{
					vals.Add(line.x1);
					vals.Add(line.y1);
					vals.Add(line.x2);
					vals.Add(line.y2);
				}
				writer.Write(args[1], vals);
			}
			else
			{ // write normal text output
				using (var file = File.CreateText(args[1]))
				{
					bool firstPass = true;
					foreach (var line in lines)
					{
						if (!firstPass)
							file.WriteLine(',');
						file.Write("{0},{1},{2},{3}",
							line.x1, line.y1, line.x2, line.y2);
						firstPass = false;
					}
				}
			}
			Console.WriteLine("{0} written with output",args[1]);
		}
	}
}

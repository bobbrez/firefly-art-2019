using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ReduceLines
{
	class DXFWriter
	{
		static string Header = 
			"999"+Environment.NewLine+
			"Created by Chris Lomont's DXF writer 2010" + Environment.NewLine+
			"0\nSETION\n2\nHEADER\n9\n$ACADVER\n1\nAC1009\n0\nENDSEC\n0\nSECTION\n2\nENTITIES\n";
		/* header
999
Created by Wolfram Mathematica 7.0 : www.wolfram.com
0
SECTION
2
HEADER
9
$ACADVER
1
AC1009
0
ENDSEC
0
SECTION
2
ENTITIES
		 */
/* each line looks like
0
LINE
8
0
10
1.7320508075688772
20
1.
30
0.
11
0.9604221727969424
21
1.4455
31
0.
		 * */
		static string Footer = "0\nENDSEC\n0\nEOF\n";
		static string LineFormat = "0\nLINE\n8\n0\n10\n{0}\n20\n{1}\n30\n0\n11\n{2}\n21\n{3}\n31\n0\n";

		/* footer
0
ENDSEC
0
EOF
		 */
		/// <summary>
		/// Write a DXF consisting of lines
		/// Each line is 4 consecutive doubles 
		/// in order x1,y1,x2,y2
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="lines"></param>
		public void Write(string filename, List<double> lines)
		{
			if ((lines.Count&3)!=0)
				throw new ArgumentException("Lines needs to contain a multiple of 4 entries to define 2D lines");
				using (var file = File.CreateText(filename))
				{
					file.Write(Header);
					for (int i = 0; i < lines.Count; i+=4)
						file.Write(LineFormat,lines[i],lines[i+1],lines[i+2],lines[i+3]);
					file.Write(Footer);
				}
		}
	}
}

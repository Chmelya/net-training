using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace IOStreams
{

	public static class TestTasks
	{
		/// <summary>
		/// Parses Resourses\Planets.xlsx file and returns the planet data: 
		///   Jupiter     69911.00
		///   Saturn      58232.00
		///   Uranus      25362.00
		///    ...
		/// See Resourses\Planets.xlsx for details
		/// </summary>
		/// <param name="xlsxFileName">source file name</param>
		/// <returns>sequence of PlanetInfo</returns>
		public static IEnumerable<PlanetInfo> ReadPlanetInfoFromXlsx(string xlsxFileName)
		{
			string[] names;
			string[] radius;
			using (var package = Package.Open(xlsxFileName))
			{
				using (var stream = package.GetPart(new Uri(@"/xl/sharedStrings.xml", UriKind.Relative)).GetStream())
				{
					names = XDocument
						.Load(stream)
						.Root
						.Descendants()
						.Where(x => x.Name.LocalName == "t")
						.Select(x => x.Value)
						.ToArray();
				}
				using (var stream = package.GetPart(new Uri(@"/xl/worksheets/sheet1.xml", UriKind.Relative)).GetStream())
				{
					radius = XDocument
						.Load(stream)
						.Root
						.Descendants()
						.Where(x => x.Name.LocalName == "v" && x.Parent.Attribute("r").Value.ToString()[0] == 'B')
						.Skip(1)
						.Select(x => x.Value)
						.ToArray();
				}
			}

			return Enumerable .Zip(names, radius, (x, y) =>
				new PlanetInfo { Name = x, MeanRadius = Double.Parse(y, CultureInfo.InvariantCulture) }
			);
		}


		/// <summary>
		/// Calculates hash of stream using specifued algorithm
		/// </summary>
		/// <param name="stream">source stream</param>
		/// <param name="hashAlgorithmName">hash algorithm ("MD5","SHA1","SHA256" and other supported by .NET)</param>
		/// <returns></returns>
		public static string CalculateHash(this Stream stream, string hashAlgorithmName)
		{
			HashAlgorithm hash = HashAlgorithm.Create(hashAlgorithmName);

			return hash != null ? BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "") : throw new ArgumentException();
		}


		/// <summary>
		/// Returns decompressed stream from file. 
		/// </summary>
		/// <param name="fileName">source file</param>
		/// <param name="method">method used for compression (none, deflate, gzip)</param>
		/// <returns>output stream</returns>
		public static Stream DecompressStream(string fileName, DecompressionMethods method)
		{
			var stream = new FileStream(fileName, FileMode.Open);

			if (method == DecompressionMethods.Deflate)
			{
				return new DeflateStream(stream, CompressionMode.Decompress);
			}

			if (method == DecompressionMethods.GZip)
			{
				return new GZipStream(stream, CompressionMode.Decompress);
			}

			if (method == DecompressionMethods.None)
			{
				return stream;
			}

			throw new ArgumentException();
		}


		/// <summary>
		/// Reads file content econded with non Unicode encoding
		/// </summary>
		/// <param name="fileName">source file name</param>
		/// <param name="encoding">encoding name</param>
		/// <returns>Unicoded file content</returns>
		public static string ReadEncodedText(string fileName, string encoding)
		{
			return File.ReadAllText(fileName, Encoding.GetEncoding(encoding));
		}
	}


	public class PlanetInfo : IEquatable<PlanetInfo>
	{
		public string Name { get; set; }
		public double MeanRadius { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", Name, MeanRadius);
		}

		public bool Equals(PlanetInfo other)
		{
			return Name.Equals(other.Name)
				&& MeanRadius.Equals(other.MeanRadius);
		}
	}



}

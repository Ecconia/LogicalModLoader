using System.IO;
using System.Security.Cryptography;
using System.Text;
using LogicAPI;

namespace LogicalModLoader.Util
{
	public static class Hashing
	{
		public static string hashModFile(ModFile file)
		{
			SHA256 sha256 = SHA256.Create();
			using(Stream stream = file.OpenRead())
			{
				return bytesToString(sha256.ComputeHash(stream));
			}
		}

		public static string bytesToString(byte[] bytes)
		{
			StringBuilder builder = new StringBuilder(bytes.Length * 2);
			foreach(byte b in bytes)
			{
				builder.Append(b.ToString("x2"));
			}
			return builder.ToString();
		}
	}
}

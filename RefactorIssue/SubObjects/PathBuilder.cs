using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorIssue.SubObjects
{
	public static class PathBuilder
	{
		public static string BasePath { get; private set; }

		public static void SetBasePath(string basePath)
		{
			BasePath = basePath;
		}

		public static string BuildPath(string relativePath)
		{
			return System.IO.Path.Combine(BasePath, relativePath ?? "");
		}
	}
}

//----------------------------------------------
//
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	public class Spreadsheet : System.Object
	{

		protected List<List<string>> spreadsheet = new List<List<string>>();

		public List<string> GetRow(int row)
		{
			return spreadsheet[row];
		}

		public List<string> GetRow(string row)
		{
			return (from r in spreadsheet where r[0] == row select r).FirstOrDefault();
		}

		public string GetValue(int row, int column)
		{
			try
			{
				return spreadsheet[row][column];
			}
			catch
			{
				return "";
			}
		}

		public string GetValue(int row, string column)
		{
			return spreadsheet[row][spreadsheet[0].IndexOf(column)];
		}

		public string GetValue(string row, int column)
		{
			try
			{
				return (from r in spreadsheet where r[0] == row select r).FirstOrDefault()[column];
			}
			catch
			{
				return "";
			}
		}

		public string GetValue(string row, string column)
		{
			return (from r in spreadsheet where r[0] == row select r).FirstOrDefault()[spreadsheet[0].IndexOf(column)];
		}

		public string GetVariableName(int column)
		{
			string variableName = spreadsheet[0][column].Split('_')[0];
			return variableName;
		}

		public int GetIndexOf(string column)
		{
			int index = spreadsheet[0].IndexOf(column);
			if (index == -1) throw new System.Exception(string.Format("Colum {0} was not found", column));
			return index;
		}

		public int GetNbRows()
		{
			return spreadsheet.Count;
		}

		public int GetNbColumns()
		{
			return spreadsheet[0].Count;
		}

		public void Load(string content)
		{
			spreadsheet.Clear();
			string[] lines = content.Split('\n');
			foreach (string line in lines)
			{
				spreadsheet.Add(line.Split('	').ToList());
			}

			foreach (List<string> line in spreadsheet)
			{
				while (line.Count < spreadsheet[0].Count)
				{
					line.Add("");
				}
			}
		}
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace PokerProject
{
	[Serializable]
	class OneVersusOnelookupTable
	{
		public double[,] lookupTable;
		public uint i, j, k, l;
		public int alreadyDone;
		public OneVersusOnelookupTable()
		{
			lookupTable = new double[52 * 52, 52 * 52];
			i = 0;
			j = 1;
			k = 2;
			l = 3;
			alreadyDone = 0;
		}
		public OneVersusOnelookupTable(double [,] LT, uint I, uint J, uint K, uint L, int AD)
		{
			lookupTable = LT;
			i = I;
			j = J;
			k = K;
			l = L;
			alreadyDone = AD;
		}
		public static OneVersusOnelookupTable ReadFromFile(string filepath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			Stream s = File.Open(filepath, FileMode.Open);
			OneVersusOnelookupTable local = bf.Deserialize(s) as OneVersusOnelookupTable;
			s.Close();

			return local;
		}
		public static void writeToFile(OneVersusOnelookupTable loacl, string filepath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			Stream s = File.Open(filepath, FileMode.Create);
			bf.Serialize(s, loacl);
			s.Close();
		}
	}


	static class Serializer
	{
		static Mutex mutex = new Mutex();
		static bool timeToEnd = false;


		//createTwoPlayerLookupTable("C:\\Users\\Artur\\Desktop\\pokerData");
		public static void serialize(string filepath)
		{
			OneVersusOnelookupTable local;
			try
			{
				local = OneVersusOnelookupTable.ReadFromFile(filepath);
			}
			catch 
			{
				local = new OneVersusOnelookupTable();
			}

			Task<OneVersusOnelookupTable> t = new Task<OneVersusOnelookupTable>(() => create1v1LookupTable(local));
			Console.WriteLine("Table is beginning to be done, type q for it to stop");
			string s = "?";
			t.Start();
			while (s[0] != 'q')
				s = Console.ReadLine();
			mutex.WaitOne();
			timeToEnd = true;
			mutex.ReleaseMutex();
			OneVersusOnelookupTable a = t.Result;
			OneVersusOnelookupTable.writeToFile(a, filepath);
			Console.WriteLine("Already done: {0}", a.alreadyDone);
		}
		static OneVersusOnelookupTable create1v1LookupTable(OneVersusOnelookupTable local)//takes a couple of days, 58 409 001 050 400 card combinations to evaluate
		{
			int alreadyDone = local.alreadyDone;
			double[,] lookupTable = local.lookupTable;
			uint i = 0, j = 0, k = 0, l = 0;
			for (i = 0; i < 49; i++)
			{
				if(local.i!=0)
				{
					i = local.i;
					local.i = 0;
				}
				for ( j =i + 1; j < 50; j++)
				{
					if (local.j != 0)
					{
						j = local.j;
						local.j = 0;
					}
					for (k = j + 1; k < 51; k++)
					{
						if (local.k != 0)
						{
							k = local.k;
							local.k = 0;
						}
						for ( l = k + 1; l < 52; l++)
						{
							if (local.l != 0)
							{
								l = local.l;
								local.l = 0;
							}
							mutex.WaitOne();

							if (timeToEnd)
							{
								mutex.ReleaseMutex();
								return new OneVersusOnelookupTable(lookupTable, i, j, k, l, alreadyDone);
							}
							mutex.ReleaseMutex();
							addToLookupTable(lookupTable, i, j, k, l);
							alreadyDone++;
						}
					}
				}
			}
			return new OneVersusOnelookupTable(lookupTable, i, j, k, l, alreadyDone);
		}
		static void addToLookupTable(double[,] lookupTable, uint i, uint j, uint k, uint l)
		{
			//Console.WriteLine(DateTime.Now.Second);
			uint[] table = new uint[5];
			uint[][] players = new uint[2][];
			players[0] = new uint[2];
			players[1] = new uint[2];

			players[0][0] = i;
			players[0][1] = j;
			players[1][0] = k;
			players[1][1] = l;
			double[] result = PokerCalculator.calculateChances(table, 0, players, 48);

			lookupTable[i * 52 + j, k * 52 + l] = result[0];
			lookupTable[j * 52 + i, k * 52 + l] = result[0];
			lookupTable[i * 52 + j, l * 52 + k] = result[0];
			lookupTable[j * 52 + i, l * 52 + k] = result[0];

			lookupTable[k * 52 + l, i * 52 + j] = result[1];
			lookupTable[k * 52 + l, j * 52 + i] = result[1];
			lookupTable[l * 52 + k, i * 52 + j] = result[1];
			lookupTable[l * 52 + k, j * 52 + i] = result[1];

			players[0][0] = i;
			players[0][1] = k;
			players[1][0] = j;
			players[1][1] = l;
			result = PokerCalculator.calculateChances(table, 0, players, 48);

			lookupTable[i * 52 + k, j * 52 + l] = result[0];
			lookupTable[k * 52 + i, j * 52 + l] = result[0];
			lookupTable[i * 52 + k, l * 52 + j] = result[0];
			lookupTable[k * 52 + i, l * 52 + j] = result[0];

			lookupTable[j * 52 + l, i * 52 + k] = result[1];
			lookupTable[j * 52 + l, k * 52 + i] = result[1];
			lookupTable[l * 52 + j, i * 52 + k] = result[1];
			lookupTable[l * 52 + j, k * 52 + i] = result[1];

			players[0][0] = i;
			players[0][1] = l;
			players[1][0] = j;
			players[1][1] = k;
			result = PokerCalculator.calculateChances(table, 0, players, 48);

			lookupTable[i * 52 + l, k * 52 + j] = result[0];
			lookupTable[l * 52 + i, k * 52 + j] = result[0];
			lookupTable[i * 52 + l, j * 52 + k] = result[0];
			lookupTable[l * 52 + i, j * 52 + k] = result[0];

			lookupTable[k * 52 + j, i * 52 + l] = result[1];
			lookupTable[k * 52 + j, l * 52 + i] = result[1];
			lookupTable[j * 52 + k, i * 52 + l] = result[1];
			lookupTable[j * 52 + k, l * 52 + i] = result[1];
		}
	}
}

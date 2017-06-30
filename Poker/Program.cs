using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PokerProject
{
	class Program
	{
		static void Main(string[] args)
		{
			Serializer.serialize("C:\\Users\\Artur\\Desktop\\pokerData");
			//test();
			//interactive();
			//createTwoPlayerLookupTable("C:\\Users\\Artur\\Desktop\\pokerData");
		}
		static void test()
		{
			uint[] table = new uint[5];
			uint[][] players = new uint[2][];
			players[0] = new uint[2];
			players[1] = new uint[2];

			players[0][0] = 10;
			players[1][0] = 28;
			players[1][1] = 29;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < 10; i++)
				PokerCalculator.calculateChances(table, 0, players, 48);
			Console.WriteLine(sw.ElapsedMilliseconds);
		}
		

		static void interactive()
		{
			Stopwatch sw = new Stopwatch();
			uint[] table = new uint[5];
			List<uint[]> playerList = new List<uint[]>();
			int threadCount = 48;
			int tableSize = 0;

			Console.WriteLine("Please type in number of threads to be used, default is 48");
			string s = Console.ReadLine();
			int.TryParse(s, out threadCount);

			if (threadCount == 0)
				threadCount = 48;

			Console.WriteLine("Please type in the table cards, type i for instructions");
			s = Console.ReadLine();
			if (s.Length == 1 && s[0] == 'i')
			{
				giveInstructions();
				s = Console.ReadLine();
			}
			for (; tableSize < s.Length / 2; tableSize += 1)
				table[tableSize] = CardConverter.textToNumber(s[2 * tableSize], s[2 * tableSize + 1]);

			Console.WriteLine("Please type in the players cards, type end when finished");
			s = Console.ReadLine();
			while (s != "end")
			{
				uint[] player = new uint[2];
				player[0] = CardConverter.textToNumber(s[0], s[1]);
				player[1] = CardConverter.textToNumber(s[2], s[3]);
				playerList.Add(player);
				s = Console.ReadLine();
			}
			sw.Start();

			uint[][] players = playerList.ToArray();
			double[] result = PokerCalculator.calculateChances(table, tableSize, players, threadCount);

			Console.WriteLine(sw.ElapsedMilliseconds);

			for (int i = 0; i < result.Length; i++)
				Console.WriteLine(result[i]);
		}
		static void giveInstructions()
		{

		}
	}
}

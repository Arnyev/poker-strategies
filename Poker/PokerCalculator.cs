using System;
using System.Threading;
using System.Threading.Tasks;

namespace PokerProject
{
	/// <summary>
	/// The general idea is to use bitwise operators to make the calculator run fast.
	/// The first 4 bits represent the hand, pair, two pair etc
	/// Next 8 bits are used to represent the height of the cards used in the hand, so 2 pairs of 4s and 10s will show 10 on the first 4 of the 8 bits, and show 4 on the last 4
	/// The remaining 20 bits are used to show the heights of all cards, in case the previous bits aren't enough to decide who wins, for example 2 players have the same pair
	/// and the remaining 3 cards will be used to decide who wins
	/// Example:
	/// Card A-J-10-7-7, using 4 bit groups:
	/// 1(pair) - 0(only 4 bits needed to show pair height) - 5 (2s are 0, so 7s are 5) - 12(A) - 9(J) - 8(10) - 5(7) - 5(7)
	/// Two pairs, 6s and Js with a king:
	/// 2 - 9 - 4 - 11 - 9 - 9 - 4 - 4
	/// </summary>
	public static class PokerCalculator
	{
		static readonly uint pairBase = 1 << 28;
		static readonly uint twoPairBase = 2 << 28;
		static readonly uint threeBase = 3 << 28;
		static readonly uint straightBase = 4 << 28;
		static readonly uint flushBase = 5 << 28;
		static readonly uint fullhouseBase = 6 << 28;
		static readonly uint fourBase = 7 << 28;
		static readonly uint straightFlushBase = (uint)8 << 28;

		/// <summary>
		/// Calculates the chance of winning for each player
		/// </summary>
		public static double[] calculateChances(uint[] table, int tableSize, uint[][] players, int numberOfThreads)
		{
			bool[] cardsTaken = new bool[52];//array containing data about which cards cant't be used
			for (int j = 0; j < tableSize; j++)
				cardsTaken[table[j]] = true;
			for (int j = 0; j < players.Length; j++)
			{
				cardsTaken[players[j][0]] = true;
				cardsTaken[players[j][1]] = true;
			}
			for (int j = 0; j < players.Length; j++)//sort player cards
				if (players[j][0] < players[j][1])
				{
					uint tmp = players[j][0];
					players[j][0] = players[j][1];
					players[j][1] = tmp;
				}

			double[] chances = new double[players.Length];
			int analysedPossibilities = 1;
			//methods deciding winners add 1 to the winner chance, or 1/tied if there is a tie, so we need to keep track of the number of analysed possibilities
			//it is much faster to count it here, especially with multiple threads used

			switch (tableSize)
			{
				case 0:
					analysedPossibilities = ((52 - players.Length * 2) * (51 - players.Length * 2) * (50 - players.Length * 2) * (49 - players.Length * 2) * (48 - players.Length * 2)) / 120;
					threadedEvaluate(chances, table, players, cardsTaken, numberOfThreads);//preflop chances are calculated using multiple threads, rest is single threaded
					break;
				case 1:
					analysedPossibilities = ((51 - players.Length * 2) * (50 - players.Length * 2) * (49 - players.Length * 2) * (48 - players.Length * 2)) / 24;
					evaluate(chances, table, players, cardsTaken, 0, 49, 1);
					break;
				case 2:
					analysedPossibilities = ((50 - players.Length * 2) * (49 - players.Length * 2) * (48 - players.Length * 2)) / 6;
					evaluate(chances, table, players, cardsTaken, 0, 50, 2);
					break;
				case 3:
					analysedPossibilities = ((49 - players.Length * 2) * (48 - players.Length * 2)) / 2;
					evaluate(chances, table, players, cardsTaken, 0, 51, 3);
					break;
				case 4:
					analysedPossibilities = 48 - players.Length * 2;
					evaluate(chances, table, players, cardsTaken, 0, 52, 4);
					break;
				case 5:
					analysedPossibilities = 1;
					findWinners(table, players, chances);
					break;
			}

			for (int i = 0; i < players.Length; i++)
				chances[i] /= analysedPossibilities;

			return chances;
		}
		/// <summary>
		///Analyses all possible table card combinations using multiple threads 
		/// </summary>
		/// <param name="numberOfThreads">for some reason 48 threads work faster than less</param>
		static void threadedEvaluate(double[] chances, uint[] table, uint[][] players, bool[] taken, int numberOfThreads)
		{
			int threadJobSize = (47 + numberOfThreads) / numberOfThreads;//ceiling
			uint[][][] playersThreadCopies = new uint[numberOfThreads][][];
			double[][] chancesThreadCopies = new double[numberOfThreads][];
			bool[][] takenThreadCopies = new bool[numberOfThreads][];
			uint[][] tableThreadCopies = new uint[numberOfThreads][];

			Task[] threads = new Task[numberOfThreads];
			for (int i = 0; i < numberOfThreads; i++)
			{
				playersThreadCopies[i] = new uint[players.Length][];
				for (int j = 0; j < players.Length; j++)
				{
					playersThreadCopies[i][j] = new uint[2];
					playersThreadCopies[i][j][0] = players[j][0];
					playersThreadCopies[i][j][1] = players[j][1];
				}

				chancesThreadCopies[i] = new double[players.Length];
				takenThreadCopies[i] = new bool[52];
				for (int j = 0; j < 52; j++)
					takenThreadCopies[i][j] = taken[j];

				tableThreadCopies[i] = new uint[5];
				int end = (1 + i) * threadJobSize < 48 ? (1 + i) * threadJobSize : 48;

				int localI = i;//necessary
				threads[localI] = new Task(() => evaluate(chancesThreadCopies[localI], tableThreadCopies[localI], playersThreadCopies[localI], takenThreadCopies[localI], localI * threadJobSize, end, 0));
			}
			for (int i = 0; i < numberOfThreads; i++)
				threads[i].Start();

			Task.WaitAll(threads);
			for (int i = 0; i < numberOfThreads; i++)
				for (int j = 0; j < players.Length; j++)
					chances[j] += chancesThreadCopies[i][j];
		}
		//helper function for analysing possibilities
		static void evaluate(double[] chances, uint[] table, uint[][] players, bool[] taken, int start, int end, int index)
		{
			if (index == 4)
				for (int i = start; i < 52; i++)
				{
					if (taken[i]) continue;
					table[4] = (uint)i;
					findWinners(table, players, chances);
				}

			else
				for (int i = start; i < end; i++)
				{
					if (taken[i]) continue;
					table[index] = (uint)i;
					evaluate(chances, table, players, taken, i + 1, 48 + index, index + 1);
				}
		}
		//decides which players win/tie and adds 1 or 1/tied to their general chances
		static void findWinners(uint[] table, uint[][] players, double[] chances)
		{
			int winnersCount = 1;
			uint winningCard = 0;//the best card of all players
			uint[] maxes = new uint[players.Length];
			uint[] sortedTable = new uint[5];
			uint[] heights = new uint[7];
			uint[] baseCard = new uint[7];

			for (int p = 0; p < 5; p++)
				sortedTable[p] = table[4 - p];//usually the table will be sorted in a reverse way because of the way it was generated

			for (int i = 1; i < 5; i++)//still needs to be sorted in case it was user input
			{
				uint tmp = sortedTable[i];
				int j = i - 1;
				for (; j >= 0 && sortedTable[j] < tmp; j--)
					sortedTable[j + 1] = sortedTable[j];
				sortedTable[j + 1] = tmp;
			}

			for (int i = 0; i < players.Length; i++)
			{
				int index1 = 0, index2 = 0;
				for (int j = 0; j < 7; j++)//merge sort of player cards and table cards
				{
					if (index2 == 5 || (index1 < 2 && players[i][index1] > sortedTable[index2]))
					{
						baseCard[j] = players[i][index1];
						index1++;
					}
					else
					{
						baseCard[j] = sortedTable[index2];
						index2++;
					}
					heights[j] = baseCard[j] >> 2;
				}

				maxes[i] = findPlayerMax(heights, baseCard);

				if (maxes[i] > winningCard)
				{
					winningCard = maxes[i];
					winnersCount = 1;
				}
				else if (maxes[i] == winningCard)
					++winnersCount;
			}
			for (int z = 0; z < maxes.Length; z++)
				if (maxes[z] == winningCard)
					chances[z] += (double)1 / winnersCount;
		}

		/// <summary>
		/// Analyses what is the best 5 card combination for a player with the current table
		/// It works much faster with the 21 combinations written by hand than using for loops
		/// </summary>
		static uint findPlayerMax(uint[] heights, uint[] baseCard)
		{
			uint best = 0;
			uint mask = 3;

			uint color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[3] & mask) << 2) | (baseCard[4] & mask);//combining all colors to 1 uint
			uint tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[3], heights[4]);
			if (tmp2 > best) best = tmp2;//without 6 7

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[3] & mask) << 2) | (baseCard[5] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[3], heights[5]);
			if (tmp2 > best) best = tmp2;//without 5 7

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[5] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[4], heights[5]);
			if (tmp2 > best) best = tmp2; //without 4 7

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[5] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[3], heights[4], heights[5]);
			if (tmp2 > best) best = tmp2; //without 3 7

			color = (baseCard[0] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[5] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[2], heights[3], heights[4], heights[5]);
			if (tmp2 > best) best = tmp2;//without 2 7

			color = (baseCard[1] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[5] & mask);
			tmp2 = evaluateCard(color, heights[1], heights[2], heights[3], heights[4], heights[5]);
			if (tmp2 > best) best = tmp2;//without 1 7

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[3] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[3], heights[6]);
			if (tmp2 > best) best = tmp2;//without 5 6

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[4], heights[6]);
			if (tmp2 > best) best = tmp2;//without 4 6

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[3], heights[4], heights[6]);
			if (tmp2 > best) best = tmp2;//without 3 6

			color = (baseCard[0] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[2], heights[3], heights[4], heights[6]);
			if (tmp2 > best) best = tmp2;//without 2 6

			color = (baseCard[1] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[4] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[1], heights[2], heights[3], heights[4], heights[6]);
			if (tmp2 > best) best = tmp2;//without 1 6

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[2] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[2], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 4 5

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[3], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 3 5

			color = (baseCard[0] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[2], heights[3], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 2 5

			color = (baseCard[1] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[3] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[1], heights[2], heights[3], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 1 5

			color = (baseCard[0] << 8) | ((baseCard[1] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[1], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 3 4

			color = (baseCard[0] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[2], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 2 4

			color = (baseCard[1] << 8) | ((baseCard[2] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[1], heights[2], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 1 4

			color = (baseCard[0] << 8) | ((baseCard[3] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[0], heights[3], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 2 3

			color = (baseCard[1] << 8) | ((baseCard[3] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[1], heights[3], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 1 3

			color = (baseCard[2] << 8) | ((baseCard[3] & mask) << 6) | ((baseCard[4] & mask) << 4) | ((baseCard[5] & mask) << 2) | (baseCard[6] & mask);
			tmp2 = evaluateCard(color, heights[2], heights[3], heights[4], heights[5], heights[6]);
			if (tmp2 > best) best = tmp2;//without 1 2

			return best;
		}

		/// <summary>
		/// Evaluates the current 5 card combination
		/// </summary>
		/// <param name="colors">Colors of all five cards combined to a single uint using bitwise operations each color takes 2 bits</param>
		/// <returns></returns>
		static uint evaluateCard(uint colors, uint height1, uint height2, uint height3, uint height4, uint height5)
		{
			bool firstPair = height1 == height2;
			bool secondPair = height2 == height3;
			bool thirdPair = height3 == height4;
			bool fourthPair = height4 == height5;

			uint allheights = (height1 << 16) | (height2 << 12) | (height3 << 8) | (height4 << 4) | height5;//heights of all the cards combined to a single uint

			if (firstPair || secondPair || thirdPair || fourthPair)//makes application faster
			{
				if ((firstPair && secondPair && thirdPair) || (secondPair && thirdPair && fourthPair))//four of a kind
					return fourBase | (height2 << 20) | allheights;

				if ((firstPair && secondPair && fourthPair) || (firstPair && thirdPair && fourthPair))//high full house
					return fullhouseBase | (height3 << 20) | allheights;

				if ((firstPair && secondPair) || (secondPair && thirdPair) || (thirdPair && fourthPair))//three of a kind
					return threeBase | (height3 << 20) | allheights;

				if ((firstPair && thirdPair) || (firstPair && fourthPair) || (secondPair && fourthPair)) //2 pair
					return twoPairBase | (height2 << 24) | (height4 << 20) | allheights;

				if (firstPair || secondPair)//pairs
					return pairBase | (height2 << 20) | allheights;

				if (thirdPair || fourthPair)
					return pairBase | (height4 << 20) | allheights;
			}

			bool flush = ((colors << 24) ^ (colors << 22)) < (1 << 24);//if all colors are the same XOR will zero out the first 8 bits
			bool straightA = height1 == height5 + 4;
			bool straightB = height1 == 12 & height2 == 3;
			if (flush || straightA || straightB)
			{
				if (flush && straightA)
					return straightFlushBase | allheights;

				if (flush && straightB)
					return straightFlushBase;

				if (flush)
					return flushBase | allheights;

				if (straightA)
					return straightBase | allheights;

				if (straightB)
					return straightBase;
			}
			return allheights;//nothing
		}
	}
}

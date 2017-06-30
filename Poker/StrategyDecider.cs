using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerProject
{
	static class StrategyDecider
	{
		public static equilibrium EquilibriumFinder(int blindDifference, int smallBlindStack, int bigBlindStack, int pot, double[,] lookupTable)
		{
			//dla kazdego kombo stratow 1000 losow
			//i szukamy gdzie lepiej na zmiane
			//tak dlugo az sa zmiany

			bool change = true;




			strategy sb = new strategy();
			strategy bb = new strategy();
		
			return new equilibrium(sb, bb);
		}
		public static double strategyCompare(strategy SBStrat, strategy BBStrat, int blindDifference, int pot, int maxPot, double lookupTable)
		{
			double sbWins = 0;
			double bbWind = 0;

			bool bbAllin;
			bool sbAllin;
			bool[] taken = new bool[52];
			Random rand = new Random();
			int testCount = 100;

			uint[][] players = new uint[2][];
			players[0] = new uint[2];
			players[1] = new uint[2];

			for (int i = 0; i < testCount; i++)
			{
				int cardsAdded = 0;
				while (cardsAdded != 4)
				{
					int card = rand.Next(52);
					if (taken[card]) continue;
					players[cardsAdded / 2][cardsAdded % 2] = (uint)card;
					taken[card] = true;
					cardsAdded++;
				}
				sbAllin =
				   players[0][0] / 4 == players[0][1] / 4 && players[0][0] / 4 >= SBStrat.pairHeight
				|| players[0][0] % 4 == players[0][1] % 4 && players[0][0] / 4 + players[0][1] / 4 > SBStrat.suitedHeight
				|| players[0][0] / 4 + players[0][1] / 4 > SBStrat.offsuitHeight;

				bbAllin =
				   players[1][0] / 4 == players[1][1] / 4 && players[1][0] / 4 >= SBStrat.pairHeight
				|| players[1][0] % 4 == players[1][1] % 4 && players[1][0] / 4 + players[1][1] / 4 > SBStrat.suitedHeight
				|| players[1][0] / 4 +  players[1][1] / 4 > SBStrat.offsuitHeight;


			}
			return 0;
		}
	}
	class equilibrium
	{
		strategy bigBlind;
		strategy smallBlind;
		//int blindDifference;
		//int smallBlindStack;
		//int bigBlindStack;
		//int pot;

		public equilibrium(strategy BB, strategy SB)//, int BD, int SBS, int BBS, int P)
		{
			bigBlind = BB;
			smallBlind = SB;
			//blindDifference = BD;
			//smallBlindStack = SBS;
			//pot = P;
			//bigBlindStack = BBS;
		}
	}
	public struct strategy
	{
		public int pairHeight;
		public int suitedHeight;
		public int offsuitHeight;
	}
}

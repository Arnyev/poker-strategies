using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerProject
{
	static class CardConverter
	{
		public static uint textToNumber(char a, char b)
		{
			uint result = 0;
			switch (a)
			{
				case 'A':
					result += 48;
					break;
				case 'K':
					result += 44;
					break;
				case 'Q':
					result += 40;
					break;
				case 'J':
					result += 36;
					break;
				case '0':
					result += 32;
					break;
				case '9':
					result += 28;
					break;
				case '8':
					result += 24;
					break;
				case '7':
					result += 20;
					break;
				case '6':
					result += 16;
					break;
				case '5':
					result += 12;
					break;
				case '4':
					result += 8;
					break;
				case '3':
					result += 4;
					break;
				case '2':
					break;
				default:
					break;
			}
			switch (b)
			{
				case 'C':
					result += 3;
					break;
				case 'S':
					result += 2;
					break;
				case 'D':
					result += 1;
					break;
				case 'H':
					break;
				default:
					break;
			}
			return result;
		}
		public static string numberToText(this int c)
		{
			string s = "";
			int height = c / 4;
			int color = c % 4;
			switch (height)
			{
				case 0:
					s += "2";
					break;
				case 1:
					s += "3";
					break;
				case 2:
					s += "4";
					break;
				case 3:
					s += "5";
					break;
				case 4:
					s += "6";
					break;
				case 5:
					s += "7";
					break;
				case 6:
					s += "8";
					break;
				case 7:
					s += "9";
					break;
				case 8:
					s += "10";
					break;
				case 9:
					s += "J";
					break;
				case 10:
					s += "Q";
					break;
				case 11:
					s += "K";
					break;
				case 12:
					s += "A";
					break;
				default:
					break;
			}
			switch (color)
			{
				case 0:
					s += "H";
					break;
				case 1:
					s += "D";
					break;
				case 2:
					s += "S";
					break;
				case 3:
					s += "C";
					break;
				default:
					break;
			}
			return s;
		}
	}
}

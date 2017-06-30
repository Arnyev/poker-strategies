# poker-strategies
The goal of the application is to find the optimal strategies and Nash equilibria in selected poker scenarios. At this point it contains a poker calculator, and I have started developing the method to find the best strategy in this scenario:
 - game is 9 player Sit and Go Hyper Turbo
 - game is preflop
 - all the players except small blind and big blind have folded
 - the pot is big compared to the stacks of the players left, for example the stacks are only 3 times the pot size
 - because of that the small blind player doesn't really have good options except folding or all-in, (calling most likely results in big blind player going all-in anyway)
 - what i describe as small blind player strategy in this scenario is the threshold at which he stops folding and starts to go all-in
 - big blind player strategy is the threshold at which he starts calling the allin

Nash equilibrium is the point at which no player can improve his strategy, there is no other strategy that does better against the opponents strategy. Nash equilibrium can be found for a certain pot/stack combination, and is described as 2 strategies.

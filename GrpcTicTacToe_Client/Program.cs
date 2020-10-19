using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcTicTacToe_Server;

namespace GrpcTicTacToe_Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new TicTacToeServ.TicTacToeServClient(channel);

                //Greet user and prompt to start game session
                Console.Write("Would you like to play Tic Tac Toe? (yes or no): ");
                var input = Console.ReadLine();     //Get their choice
                while (input.ToLower()[0] != 'y')    //If they don't say yes, let them know that this does nothing else and reprompt until they accept
                {
                    Console.Write("That's a shame, since all this computer does is play Tic Tac Toe.\nDo you want to play? (yes or no): ");
                    input = Console.ReadLine();
                }//while

                //Get user choice of X or O
                Console.Write("Excellent!\nDo you want to play as X's or O's? (X or O): ");
                var Player = Console.ReadLine();                                //Get choice
                while (Player.ToLower() != "x" && Player.ToLower() != "o")       //make sure it's X or O, reprompt until they choose X or O
                {
                    Console.Write("Invalid choice. Please choose X or O. (X or O): ");
                    Player = Console.ReadLine();
                }//while

                await PlayGameAsync(Player, client);

                Console.WriteLine("\nThank you for playing!\nPress ENTER to end.");
                Console.ReadLine(); 
            }
        }//main

        /// <summary>
        /// Actual game functionality
        /// </summary>
        /// <param name="Player">Either X or O depending on choice</param>
        private static async Task PlayGameAsync(string Player, TicTacToeServ.TicTacToeServClient client)
        {
            var GameState = await client.ShowBoardAsync(new Null { });
            var result = await client.CheckStateAsync(new Null { });
            Random rng = new Random();

            if (Player.ToLower() == "x")
            {
                result = await client.CheckStateAsync(new Null { });
                do          //Turn loop. Start with X (player), check for victory conditions, then continue to O and repeat.
                {
                    await TurnAsync(Player, client);
                    result = await client.CheckStateAsync(new Null { });
                    if (result.Result_ != "N") { break; }

                    await TurnAsync("O", client, rng);
                    result = await client.CheckStateAsync(new Null { });
                    if (result.Result_ != "N") { break; }
                } while (result.Result_ == "N");
            }//if player is X
            else                    //If the player chose to play O
            {
                result = await client.CheckStateAsync(new Null { });

                do
                {
                    await TurnAsync("X", client, rng);
                    result = await client.CheckStateAsync(new Null { });
                    if (result.Result_ != "N") { break; }

                    await TurnAsync(Player, client);
                    result = await client.CheckStateAsync(new Null { });
                    if (result.Result_ != "N") { break; }
                } while (result.Result_ == "N");
            }//else

            var board = await client.ShowBoardAsync(new Null { });
            Console.WriteLine(board.Board);

            // Generate and display winner messages
            switch (result.Result_)
            {
                case "X":
                    Console.WriteLine("!!!!\t\t X wins the game\t\t!!!!");
                    break;
                case "O":
                    Console.WriteLine("!!!!\t\t O wins the game\t\t!!!!");
                    break;
                case "D":
                    Console.WriteLine("\t\t:( :( :(\tThe game is a draw.\t:( :( :(");
                    break;
            }//switch

            await client.ResetAsync(new Null { });
        }//PlayGame

        /// <summary>
        /// Turn functionality.
        /// If user turn, prompts for row, then column, then attempts to insert at chosen location. 
        /// If AI turn, generates 2 random integers from 0 to 3 for row and column, then attempts to insert at that location.
        /// </summary>
        /// <param name="player">X or O to place</param>
        /// <param name="rng">Randum number generator. If null, it is user turn. If it exists, AI turn.</param>
        private async static Task TurnAsync(string player, TicTacToeServ.TicTacToeServClient client, Random rng = null)
        {
            int row = -1, col = -1;
            bool valid = false;
            PlaceUnit toPlace = new PlaceUnit();
            
            toPlace.Unit = player;

            do                  //attempt to place token, repeat until valid location chosen
            {
                var board = await client.ShowBoardAsync(new Null { });
                Console.WriteLine(board.Board);
                if (rng == null) //if rng is null, we're processing player actions
                {
                    while (!valid)
                    {
                        Console.Write("Please enter the row you wish to place in: ");
                        valid = Int32.TryParse(Console.ReadLine(), out row);
                    }
                    valid = valid ^ valid;
                    while (!valid)
                    {
                        Console.Write("Please enter the column you wish to place in: ");
                        valid = Int32.TryParse(Console.ReadLine(), out col);
                    }
                    toPlace.Row = row;
                    toPlace.Col = col;
                }//if
                else            //if rng is not null, it's ai move
                {
                    row = rng.Next(0, 3);
                    col = rng.Next(0, 3);
                    toPlace.Row = row;
                    toPlace.Col = col;
                }//else

                var gamestate = await client.PlayTurnAsync(toPlace);
                valid = gamestate.Valid;
                if (!valid)
                {
                    if (rng != null) Console.WriteLine("Invalid choice by AI. Rechoosing..)");
                    else Console.WriteLine("Invalid choice. Please choose again.");
                }
            } while (!valid);
        }//Turn(Socket, TicTacToeBoard, string, Random)

    }//Program
}//namespace

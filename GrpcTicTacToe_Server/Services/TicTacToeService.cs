using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TicTacToeApp.Logic;

namespace GrpcTicTacToe_Server.Services
{
    public class TicTacToeService : TicTacToeServ.TicTacToeServBase
    {
        private static TicTacToeBoard _board = new TicTacToeBoard();

        public override Task<GameState> ShowBoard(Null non, ServerCallContext context)
        {
            return Task.FromResult(new GameState
            {
                Board = _board.ToString()
            });
        }//ShowBoard

        public override Task<GameState> PlayTurn(PlaceUnit unit, ServerCallContext context)
        {
            return Task.FromResult(new GameState
            {
                Valid = _board.Insert(unit.Row, unit.Col, unit.Unit),
                Board = _board.ToString()
            });
        }//PlayTurn

        public override Task<Result> CheckState(Null non, ServerCallContext context)
        {
            return Task.FromResult(new Result
            {
                Result_ = _board.ReportResult().ToString()
            });
        }//CheckState

        public override Task<Null> Reset(Null non, ServerCallContext context)
        {
            _board = new TicTacToeBoard();
            return Task.FromResult(new Null { });
        }
    }//service
}//namespace

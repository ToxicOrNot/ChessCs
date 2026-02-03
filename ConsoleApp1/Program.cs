// using System;

var board = new Board();
void PlacePieces()
{
    for (int file = 0; file < 8; file++)
    {
        board.PlacePiece(new Pawn(PieceColor.White), new Position(file, 1));
        board.PlacePiece(new Pawn(PieceColor.Black), new Position(file, 6));
    }
    board.PlacePiece(new Rook(PieceColor.White), new Position(0, 0));
    board.PlacePiece(new Rook(PieceColor.White), new Position(7, 0));
    board.PlacePiece(new Rook(PieceColor.Black), new Position(0, 7));
    board.PlacePiece(new Rook(PieceColor.Black), new Position(7, 7));
    board.PlacePiece(new Knight(PieceColor.White), new Position(1, 0));
    board.PlacePiece(new Knight(PieceColor.White), new Position(6, 0));
    board.PlacePiece(new Knight(PieceColor.Black), new Position(1, 7));
    board.PlacePiece(new Knight(PieceColor.Black), new Position(6, 7));
    board.PlacePiece(new Bishop(PieceColor.White), new Position(2, 0));
    board.PlacePiece(new Bishop(PieceColor.White), new Position(5, 0));
    board.PlacePiece(new Bishop(PieceColor.Black), new Position(2, 7));
    board.PlacePiece(new Bishop(PieceColor.Black), new Position(5, 7));
    board.PlacePiece(new Queen(PieceColor.White), new Position(3, 0));
    board.PlacePiece(new Queen(PieceColor.Black), new Position(3, 7));
    board.PlacePiece(new King(PieceColor.White), new Position(4, 0));
    board.PlacePiece(new King(PieceColor.Black), new Position(4, 7));

}

PlacePieces();

// board.MovePiece(new Position(3, 1), new Position(3, 3));
// board.MovePiece(new Position(2, 0), new Position(5, 3));
// board.MovePiece(new Position(4, 4), new Position(3, 3));

// board.MovePiece(new Position(4, 1), new Position(4, 3));
// board.MovePiece(new Position(4, 1), new Position(4, 3)); 
// board.MovePiece(new Position(4, 3), new Position(4, 2));
// board.MovePiece(new Position(5, 1), new Position(6, 2));
// Console.WriteLine(board.GetPieceAt(new Position(5, 3)));

PieceColor currentTurn = PieceColor.White;

while (true)
{
    Console.WriteLine($"Сейчас ходит {currentTurn}");
    
    var input = Console.ReadLine();

    if (string.IsNullOrEmpty(input))
    {
        continue;
    }

    if (input == "exit")
    {
        break;
    }

    try
    {
        var turn = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (turn.Length != 2)
            throw new Exception("Введите ход в формате е2 е4");

        var from = InputParser.ParsePosition(turn[0]);
        var to = InputParser.ParsePosition(turn[1]);

        var piece = board.GetPieceAt(from);
        if (piece == null)
            throw new Exception("На выбранном поле нет фигуры");
        
        if (piece.Color != currentTurn)
            throw new Exception("Вы не можете двигать чужие фигуры");
        
        board.MovePiece(from, to);
        
        
        // if(board.GetPieceAt(to) == null)
        //     board.MovePiece(from, to);
        // else board.TakePiece(from, to);
        
        currentTurn = PieceColor.White == currentTurn ? PieceColor.Black : PieceColor.White;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    
}

class Board
{
    private Piece?[,] _pieces = new Piece[8,8];

    public Piece? GetPieceAt(Position pos)
    {
        return _pieces[pos.File, pos.Rank];
    }
    public void PlacePiece(Piece piece, Position pos)
    {
        _pieces[pos.File, pos.Rank] =  piece;
    }


    public Position FindKing(PieceColor color)
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                var piece = _pieces[file, rank];
                if (piece is King &&  piece.Color == color)
                {
                    return new Position(file, rank);
                }
                
            }
        }

        throw new Exception("Король не найден");
    }

    public bool IsSquareAttacked(Position square, PieceColor bycolor)
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                var piece =  _pieces[file, rank];
                
                if(piece == null || piece.Color != bycolor)
                    continue;
                
                if (piece.CanTake(this, new Position(file, rank), new Position(square.File, square.Rank)))
                    return true;
                
            }
        }
        
        return false;
    }

    public bool IsInCheck(PieceColor color)
    {
        var kingpos = FindKing(color);
        PieceColor oponentColor = PieceColor.White == color ? PieceColor.Black : PieceColor.White;
        return IsSquareAttacked(kingpos, oponentColor);
    }

    private MoveState MakeRawMove(Position from, Position to)
    {
        var move = new MoveState
        {
            From = from,
            To = to,
            MovedPiece = _pieces[from.File, from.Rank],
            CapturedPiece = _pieces[to.File, to.Rank]
        };
        
        _pieces[to.File, to.Rank] = _pieces[from.File, from.Rank];
        _pieces[from.File, from.Rank] = null;
        
        return move;
    }

    private void UndoRawMove(MoveState move)
    {
        _pieces[move.From.File, move.From.Rank] = move.MovedPiece;
        _pieces[move.To.File, move.To.Rank] = move.CapturedPiece;
    }

    public bool IsMoveLegal(Position from, Position to)
    {
        var piece = _pieces[from.File, from.Rank];
        if (piece == null)
            return false;
        
        if (_pieces[to.File, to.Rank] == null)
        {
            if (!piece.CanMove(this, from, to))
                return false;
        }
        else
        {
            if(!piece.CanTake(this, from, to))
                return false;
        }
        
        var move = MakeRawMove(from, to);

        bool kingInCheck = IsInCheck(piece.Color);
        
        UndoRawMove(move);
        
        return !kingInCheck;
    }

    public void MovePiece(Position from, Position to)
    {
        var piece = GetPieceAt(from);
        
        if (from.File == to.File && from.Rank == to.Rank)
            throw new Exception("Вы не можете ходить на месте");

        if (!IsMoveLegal(from, to))
            throw new Exception("Недопустимый ход");
        
        if (GetPieceAt(to) != null)
        {
            if (!piece.CanTake(this, from, to))
            {
                throw new InvalidOperationException("Такой ход невозможен");
            }

            if (_pieces[to.File, to.Rank].Color == piece.Color)
            {
                throw new InvalidOperationException("Вы не можете брать свои фигуры");
            }
        
            _pieces[to.File, to.Rank] = null;
            _pieces[to.File, to.Rank] = piece;
            _pieces[from.File, from.Rank] = null;
            return;
        }
        
        if(!piece.CanMove(this, from, to))
            throw new Exception("Фигура не может совершить такой ход");
        
        _pieces[to.File, to.Rank] = piece;
        _pieces[from.File, from.Rank] = null;
        
    }
    
}
    
struct Position
{
    public int File { get; }
    public int Rank { get;}

    public Position(int file, int rank)
    {
        if (file < 0 || file >= 8)
            throw new ArgumentOutOfRangeException(nameof(file));
        if (rank < 0 || rank >= 8)
            throw new ArgumentOutOfRangeException(nameof(rank));
        File = file;
        Rank = rank;
    }
}

struct MoveState
{
    public Position From;
    public Position To;
    public Piece? MovedPiece;
    public Piece? CapturedPiece;
}

static class InputParser
{
    public static Position ParsePosition(string s)
    {
        if (s.Length != 2)
            throw new Exception("Введите ход в формате е2 е4");
        
        char fileChar = char.ToLower(s[0]);
        char rankChar = s[1];
        
        int file = fileChar - 'a';
        int rank = rankChar - '1';
        
        return new Position(file, rank);
    }
}


abstract class Piece
{
    public PieceColor Color {get; }
    protected Piece(PieceColor color)
    {
        Color = color;
    }

    public override string ToString()
    {
        return $"{Color} {GetType().Name}";
    }

    public abstract bool CanMove(Board board, Position from, Position to);

    public virtual bool CanTake(Board board, Position from, Position to)
    {
        return CanMove(board, from, to);
    }
}

enum PieceColor
{
    White,
    Black
}


class Pawn : Piece
{
    public Pawn(PieceColor color) : base(color) { }

    public override bool CanMove(Board board, Position from, Position to)
    {
        int side = (Color == PieceColor.White) ? 1 : -1;
        bool isTwoStepAllowed = (side == 1 && from.Rank == 1 || side == -1 && from.Rank == 6);
        Position target = new Position(from.File, from.Rank+side);
        // Position twoStepTarget = new Position(from.File, from.Rank+side*2);
        
        if (Math.Abs(to.Rank - from.Rank) > 2 || from.File != to.File)
        {
            return false;
        }

        if (board.GetPieceAt(target) != null)
        {
            return false;
        }

        if (Math.Sign((to.Rank - from.Rank) * side) != 1)
        {
            return false;
        }

        if (!isTwoStepAllowed && Math.Abs(to.Rank - from.Rank) == 2)
        {
            return false;
        }
        
        return true;
    }

    public override bool CanTake(Board board, Position from, Position to)
    {
        int side = (Color == PieceColor.White) ? 1 : -1;
        if (to.File != from.File + 1 && to.File != from.File - 1)
        {
            return false;
        }
        if (to.Rank != from.Rank + side)
        {
            return false;
        }
        return true;
    }
}

class Knight : Piece
{
    public Knight(PieceColor color) : base(color) { }

    public override bool CanMove(Board board, Position from, Position to)
    {
        Position target = new Position(to.File, to.Rank);
        
        if (from.File == to.File && from.Rank == to.Rank)
        {
            return false;
        }
        
        if(!(Math.Abs(target.File - from.File) == 2 && Math.Abs(target.Rank - from.Rank) == 1) && !(Math.Abs(target.File - from.File) == 1 && Math.Abs(target.Rank - from.Rank) == 2))
        {
            return false;
        }
        return true;
    }
    
}

class Bishop : Piece
{
    public Bishop(PieceColor color) : base(color) { }
    public override bool CanMove(Board board, Position from, Position to)
    {
        int stepRank = Math.Sign(to.Rank - from.Rank);
        int stepFile = Math.Sign(to.File - from.File);
        int squaresMove = Math.Abs(to.Rank - from.Rank);
        
        if (Math.Abs(from.File - to.File) != Math.Abs(from.Rank - to.Rank))
        {
            return false;
        }
        for (int i = 1; i != squaresMove; i++)
        {
            if (board.GetPieceAt(new Position(from.File + stepFile * i, from.Rank + stepRank * i)) != null)
            {
                return false;
            }
        }
        
        return true;
    }
    
}

class Rook : Piece
{
    public Rook(PieceColor color) : base(color) { }

    public override bool CanMove(Board board, Position from, Position to)
    {
        
        if (from.File != to.File && from.Rank != to.Rank)
            return false;
        
        if (from.File - to.File == 0)
        {
            int step = Math.Sign(to.Rank - from.Rank);
            for (int i = from.Rank + step; i != to.Rank; i += step)
            {
                if (board.GetPieceAt(new Position(from.File, i)) != null)
                {
                    return false;
                }
            }

        }
        else
        {
            int step = Math.Sign(to.File - from.File);
            for (int i = from.File + step; i != to.File; i += step)
                if (board.GetPieceAt(new Position(i, from.Rank)) != null)
                {
                    return false;
                }
        }
        return true;
    }
    
}

class Queen : Piece
{
    public Queen(PieceColor color) : base(color) { }

    public override bool CanMove(Board board, Position from, Position to)
    {
        int stepRank = Math.Sign(to.Rank - from.Rank);
        int stepFile = Math.Sign(to.File - from.File);
        int squaresMove = Math.Abs(to.Rank - from.Rank);

        bool CheckMove()
        {
            if (from.File == to.File) return Vertical();
            if (from.Rank == to.Rank) return Horizontal();
            if (Math.Abs(from.File - to.File) == Math.Abs(from.Rank - to.Rank)) return Diagonal();
            return false;
        }
        
        bool Vertical()
        {
            int step = Math.Sign(to.Rank - from.Rank);
            for (int i = from.Rank + step; i != to.Rank; i += step)
            {
                if (board.GetPieceAt(new Position(from.File, i)) != null)
                {
                    return false;
                }
            }

            return true;
        }
        
        bool Horizontal()
        {
            int step = Math.Sign(to.File - from.File);
            for (int i = from.File + step; i != to.File; i += step)
                if (board.GetPieceAt(new Position(i, from.Rank)) != null)
                {
                    return false;
                }

            return true;
        }

        bool Diagonal()
        {
            for (int i = 1; i != squaresMove; i++)
            {
                if (board.GetPieceAt(new Position(from.File + stepFile * i, from.Rank + stepRank * i)) != null)
                {
                    return false;
                }
            }
            return true;
        }

        return CheckMove();
    }

}

class King : Piece
{
    public King(PieceColor color) : base(color) { }

    public override bool CanMove(Board board, Position from, Position to)
    {
        if (Math.Abs(from.File - to.File) >= 2 || Math.Abs(from.Rank - to.Rank) >= 2)
        {
            return false;
        }
        
        return true;
    }

}






using System;

namespace RPSArena.Core.Models
{
    public class Player
    {
        public string Id { get; }
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public string ConnectionId { get; set; }
        public DateTime JoinedAt { get; }
        
        public Player(string id, string name)
        {
            Id = id ?? Guid.NewGuid().ToString();
            Name = name ?? "Jugador";
            Wins = 0;
            Losses = 0;
            Draws = 0;
            JoinedAt = DateTime.UtcNow;
        }
        
        public int TotalGames => Wins + Losses + Draws;
        
        public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
        
        public void RecordWin() => Wins++;
        public void RecordLoss() => Losses++;
        public void RecordDraw() => Draws++;
        
        public override string ToString() => $"{Name} (W: {Wins}, L: {Losses}, D: {Draws})";
        
        public override bool Equals(object obj) => obj is Player other && Id == other.Id;
        
        public override int GetHashCode() => Id.GetHashCode();
    }
}
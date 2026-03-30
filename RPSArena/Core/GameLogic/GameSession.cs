using System;
using System.Collections.Generic;
using System.Linq;
using RPSArena.Core.Models;

namespace RPSArena.Core.GameLogic
{
    public class GameSession
    {
        public string SessionId { get; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public GameState State { get; private set; }
        public int RoundsPlayed { get; private set; }
        public int RoundsToWin { get; set; } = 3;
        
        private readonly Dictionary<string, GameChoice> _currentChoices;
        private readonly List<RoundHistory> _roundHistory;
        
        public event Action<GameSession> OnGameStateChanged;
        public event Action<RoundResult, RoundHistory> OnRoundCompleted;
        public event Action<Player> OnGameFinished;
        
        public GameSession(string sessionId)
        {
            SessionId = sessionId ?? Guid.NewGuid().ToString();
            State = GameState.WaitingForPlayers;
            _currentChoices = new Dictionary<string, GameChoice>();
            _roundHistory = new List<RoundHistory>();
        }
        
        public bool JoinPlayer(Player player)
        {
            if (State != GameState.WaitingForPlayers)
                return false;
            
            if (Player1 == null)
            {
                Player1 = player;
                return true;
            }
            
            if (Player2 == null)
            {
                Player2 = player;
                State = GameState.Choosing;
                OnGameStateChanged?.Invoke(this);
                return true;
            }
            
            return false;
        }
        
        public bool SubmitChoice(Player player, GameChoice choice)
        {
            if (State != GameState.Choosing)
                return false;
            
            if (player.Id != Player1.Id && player.Id != Player2.Id)
                return false;
            
            _currentChoices[player.Id] = choice;
            
            if (_currentChoices.Count == 2)
            {
                ProcessRound();
            }
            
            return true;
        }
        
        private void ProcessRound()
        {
            var choice1 = _currentChoices[Player1.Id];
            var choice2 = _currentChoices[Player2.Id];
            
            var result = GameRules.DetermineWinner(choice1, choice2);
            
            var round = new RoundHistory
            {
                RoundNumber = ++RoundsPlayed,
                Player1Choice = choice1,
                Player2Choice = choice2,
                Result = result,
                Timestamp = DateTime.UtcNow
            };
            
            _roundHistory.Add(round);
            
            switch (result)
            {
                case RoundResult.Player1Wins:
                    Player1.RecordWin();
                    Player2.RecordLoss();
                    break;
                case RoundResult.Player2Wins:
                    Player1.RecordLoss();
                    Player2.RecordWin();
                    break;
                case RoundResult.Draw:
                    Player1.RecordDraw();
                    Player2.RecordDraw();
                    break;
            }
            
            CheckForGameWinner();
            
            OnRoundCompleted?.Invoke(result, round);
            
            if (State == GameState.Playing)
            {
                _currentChoices.Clear();
                OnGameStateChanged?.Invoke(this);
            }
        }
        
        private void CheckForGameWinner()
        {
            if (Player1.Wins >= RoundsToWin)
            {
                State = GameState.Finished;
                OnGameFinished?.Invoke(Player1);
            }
            else if (Player2.Wins >= RoundsToWin)
            {
                State = GameState.Finished;
                OnGameFinished?.Invoke(Player2);
            }
            else if (RoundsPlayed >= 10)
            {
                State = GameState.Finished;
                OnGameFinished?.Invoke(Player1.Wins > Player2.Wins ? Player1 : 
                                      Player2.Wins > Player1.Wins ? Player2 : null);
            }
        }
        
        public void LeaveGame(Player player)
        {
            if (Player1?.Id == player.Id || Player2?.Id == player.Id)
            {
                State = GameState.Finished;
                OnGameFinished?.Invoke(player);
            }
        }
        
        public List<RoundHistory> GetGameHistory() => new List<RoundHistory>(_roundHistory);
        
        public Player GetOpponent(Player player)
        {
            if (player.Id == Player1?.Id) return Player2;
            if (player.Id == Player2?.Id) return Player1;
            return null;
        }
        
        public bool HasPlayerMadeChoice(Player player) => 
            _currentChoices.ContainsKey(player.Id);
    }
    
    public enum GameState
    {
        WaitingForPlayers,
        Choosing,
        Processing,
        Playing,
        Finished
    }
    
    public class RoundHistory
    {
        public int RoundNumber { get; set; }
        public GameChoice Player1Choice { get; set; }
        public GameChoice Player2Choice { get; set; }
        public RoundResult Result { get; set; }
        public DateTime Timestamp { get; set; }
        
        public override string ToString()
        {
            return $"Ronda {RoundNumber}: {GameRules.GetChoiceEmoji(Player1Choice)} vs " +
                   $"{GameRules.GetChoiceEmoji(Player2Choice)} = {Result}";
        }
    }
}
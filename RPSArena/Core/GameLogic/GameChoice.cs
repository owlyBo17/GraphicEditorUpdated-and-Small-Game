namespace RPSArena.Core.GameLogic
{
    public enum GameChoice
    {
        Rock,      // 🪨 Piedra
        Paper,     // 📄 Papel
        Scissors   // ✂️ Tijera
    }
    
    public enum RoundResult
    {
        Player1Wins,
        Player2Wins,
        Draw
    }
    
    public static class GameRules
    {
        public static RoundResult DetermineWinner(GameChoice choice1, GameChoice choice2)
        {
            if (choice1 == choice2)
                return RoundResult.Draw;
            
            switch (choice1)
            {
                case GameChoice.Rock:
                    return choice2 == GameChoice.Scissors ? RoundResult.Player1Wins : RoundResult.Player2Wins;
                
                case GameChoice.Paper:
                    return choice2 == GameChoice.Rock ? RoundResult.Player1Wins : RoundResult.Player2Wins;
                
                case GameChoice.Scissors:
                    return choice2 == GameChoice.Paper ? RoundResult.Player1Wins : RoundResult.Player2Wins;
                
                default:
                    return RoundResult.Draw;
            }
        }
        
        public static string GetChoiceEmoji(GameChoice choice)
        {
            return choice switch
            {
                GameChoice.Rock => "🪨",
                GameChoice.Paper => "📄",
                GameChoice.Scissors => "✂️",
                _ => "❓"
            };
        }
        
        public static string GetChoiceName(GameChoice choice)
        {
            return choice switch
            {
                GameChoice.Rock => "Piedra",
                GameChoice.Paper => "Papel",
                GameChoice.Scissors => "Tijera",
                _ => "Desconocido"
            };
        }
        
        public static GameChoice ParseChoice(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Texto no puede estar vacío");
            
            return text.ToLower() switch
            {
                "rock" or "piedra" or "r" or "🪨" => GameChoice.Rock,
                "paper" or "papel" or "p" or "📄" => GameChoice.Paper,
                "scissors" or "tijera" or "s" or "✂️" => GameChoice.Scissors,
                _ => throw new ArgumentException($"Elección no válida: {text}")
            };
        }
    }
}


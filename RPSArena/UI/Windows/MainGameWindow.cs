using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RPSArena.Core.Models;
using RPSArena.Core.GameLogic;
using RPSArena.Core.Networking;

namespace RPSArena.UI.Windows
{
    public partial class MainGameWindow : Form
    {
        private Player _currentPlayer;
        private IGameClient _gameClient;
        private GameSession _currentGame;
        private Dictionary<string, GameChoice> _currentChoices = new Dictionary<string, GameChoice>();
        
        // CONTROLES DE UI - AÑADIR ESTOS CAMPOS
        private Label _statusLabel;
        private TextBox _chatBox;
        private TextBox _chatInput;
        
        public MainGameWindow(Player player, IGameClient gameClient)
        {
            _currentPlayer = player ?? throw new ArgumentNullException(nameof(player));
            _gameClient = gameClient ?? throw new ArgumentNullException(nameof(gameClient));
            
            InitializeWindow();
            SetupEventHandlers();
        }
        
        private void InitializeWindow()
        {
            // Configuración básica de ventana
            this.Text = $"RPS Arena - {_currentPlayer.Name}";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 40);
            this.ForeColor = Color.White;
            
            // Panel superior: Información del jugador
            var infoPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(580, 60),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var playerLabel = new Label
            {
                Text = $"Jugador: {_currentPlayer.Name}",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            
            var statsLabel = new Label
            {
                Text = $"W: {_currentPlayer.Wins} | L: {_currentPlayer.Losses} | D: {_currentPlayer.Draws}",
                Location = new Point(10, 35),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.White
            };
            
            infoPanel.Controls.Add(playerLabel);
            infoPanel.Controls.Add(statsLabel);
            
            // Panel central: Opciones del juego
            var gamePanel = new Panel
            {
                Location = new Point(150, 80),
                Size = new Size(300, 200),
                BackColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Botones de elección
            var btnRock = CreateChoiceButton("🪨 Piedra", 30, 30, GameChoice.Rock);
            var btnPaper = CreateChoiceButton("📄 Papel", 30, 100, GameChoice.Paper);
            var btnScissors = CreateChoiceButton("✂️ Tijera", 180, 30, GameChoice.Scissors);
            
            // Etiqueta de estado
            _statusLabel = new Label
            {
                Text = "Elige una opción",
                Location = new Point(30, 160),
                Size = new Size(240, 30),
                Font = new Font("Arial", 11, FontStyle.Bold),
                ForeColor = Color.Yellow,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            gamePanel.Controls.Add(btnRock);
            gamePanel.Controls.Add(btnPaper);
            gamePanel.Controls.Add(btnScissors);
            gamePanel.Controls.Add(_statusLabel);
            
            // Panel inferior: Chat
            var chatPanel = new Panel
            {
                Location = new Point(10, 290),
                Size = new Size(580, 180),
                BackColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var chatLabel = new Label
            {
                Text = "Chat:",
                Location = new Point(10, 10),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            
            _chatBox = new TextBox
            {
                Location = new Point(10, 35),
                Size = new Size(560, 100),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical
            };
            
            _chatInput = new TextBox
            {
                Location = new Point(10, 145),
                Size = new Size(480, 25),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                PlaceholderText = "Escribe un mensaje..."
            };
            
            var btnSend = new Button
            {
                Text = "Enviar",
                Location = new Point(500, 145),
                Size = new Size(70, 25),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White
            };
            btnSend.Click += (s, e) => SendChatMessage();
            
            chatPanel.Controls.Add(chatLabel);
            chatPanel.Controls.Add(_chatBox);
            chatPanel.Controls.Add(_chatInput);
            chatPanel.Controls.Add(btnSend);
            
            // Agregar todos los controles a la ventana
            this.Controls.Add(infoPanel);
            this.Controls.Add(gamePanel);
            this.Controls.Add(chatPanel);
            
            // Evento Enter para chat
            _chatInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SendChatMessage();
                    e.Handled = e.SuppressKeyPress = true;
                }
            };
        }
        
        private Button CreateChoiceButton(string text, int x, int y, GameChoice choice)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 60),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(70, 70, 90),
                ForeColor = Color.White,
                Tag = choice,
                FlatStyle = FlatStyle.Flat
            };
            
            button.Click += (s, e) => MakeChoice(choice);
            
            return button;
        }
        
        private void MakeChoice(GameChoice choice)
        {
            try
            {
                // Guardar elección
                _currentChoices[_currentPlayer.Id] = choice;
                
                // Actualizar UI
                _statusLabel.Text = $"Elegiste: {GameRules.GetChoiceEmoji(choice)}";
                _statusLabel.ForeColor = Color.LightBlue;
                
                // Mostrar en chat
                _chatBox.AppendText($"[Tú]: Elegí {GameRules.GetChoiceName(choice)}\r\n");
                _chatBox.ScrollToCaret();
                
                // Enviar al servidor/CPU
                var choiceMsg = GameMessage.CreateChoiceMessage(_currentPlayer.Id, choice.ToString());
                _gameClient.SendMessageAsync(choiceMsg.ToJson());
            }
            catch (Exception ex)
            {
                _chatBox.AppendText($"[Error]: {ex.Message}\r\n");
            }
        }
        
        private void SetupEventHandlers()
        {
            // Configurar eventos del cliente
            _gameClient.OnMessageReceived += OnGameMessageReceived;
            _gameClient.OnConnected += OnConnected;
            _gameClient.OnDisconnected += OnDisconnected;
        }
        
        private void OnGameMessageReceived(string jsonMessage)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    var message = GameMessage.FromJson(jsonMessage);
                    
                    if (message.Type == MessageType.ChatMessage)
                    {
                        _chatBox.AppendText($"[{message.SenderId}]: {message.Data}\r\n");
                        _chatBox.ScrollToCaret();
                    }
                    else if (message.Type == MessageType.MakeChoice && message.SenderId != _currentPlayer.Id)
                    {
                        // Respuesta de la CPU
                        var cpuChoice = GameRules.ParseChoice(message.Data);
                        _chatBox.AppendText($"[CPU]: Elegí {GameRules.GetChoiceName(cpuChoice)}\r\n");
                        _chatBox.ScrollToCaret();
                        
                        // Mostrar resultado (simplificado)
                        if (_currentChoices.ContainsKey(_currentPlayer.Id))
                        {
                            var playerChoice = _currentChoices[_currentPlayer.Id];
                            var result = GameRules.DetermineWinner(playerChoice, cpuChoice);
                            
                            string resultText = result switch
                            {
                                RoundResult.Player1Wins => "¡Ganaste esta ronda! 🎉",
                                RoundResult.Player2Wins => "CPU gana esta ronda 😔",
                                RoundResult.Draw => "Empate 🤝",
                                _ => "Resultado desconocido"
                            };
                            
                            _chatBox.AppendText($"[Sistema]: {resultText}\r\n");
                            _chatBox.AppendText($"[Sistema]: {GameRules.GetChoiceEmoji(playerChoice)} vs {GameRules.GetChoiceEmoji(cpuChoice)}\r\n");
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignorar errores en demo
                }
            });
        }
        
        private void OnConnected()
        {
            this.Invoke((MethodInvoker)delegate
            {
                _chatBox.AppendText("[Sistema]: Conectado al modo demo\r\n");
                _chatBox.ScrollToCaret();
            });
        }
        
        private void OnDisconnected()
        {
            this.Invoke((MethodInvoker)delegate
            {
                _chatBox.AppendText("[Sistema]: Modo demo activo\r\n");
                _chatBox.ScrollToCaret();
            });
        }
        
        private void SendChatMessage()
        {
            if (!string.IsNullOrWhiteSpace(_chatInput.Text))
            {
                try
                {
                    var chatMsg = GameMessage.CreateChatMessage(_currentPlayer.Id, _chatInput.Text);
                    _gameClient.SendMessageAsync(chatMsg.ToJson());
                    
                    _chatInput.Clear();
                }
                catch (Exception ex)
                {
                    _chatBox.AppendText($"[Error]: {ex.Message}\r\n");
                }
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _gameClient?.DisconnectAsync().Wait(500);
                _gameClient?.Dispose();
            }
            catch { }
            
            base.OnFormClosing(e);
        }
    }
}
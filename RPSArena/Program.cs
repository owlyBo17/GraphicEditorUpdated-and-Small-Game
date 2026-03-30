using System;
using System.Windows.Forms;
using RPSArena.Core.Models;
using RPSArena.Core.Networking;
using RPSArena.Infrastructure.Network;
using RPSArena.UI.Windows;

namespace RPSArena
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Crear jugador
            var playerName = ShowNameDialog();
            if (string.IsNullOrEmpty(playerName))
                return;
                
            var player = new Player(
                id: Guid.NewGuid().ToString(),
                name: playerName
            );
            
            // Usar cliente DEMO (sin servidor real)
            IGameClient gameClient = new DemoGameClient();
            
            // Mostrar ventana principal
            var mainWindow = new MainGameWindow(player, gameClient);
            Application.Run(mainWindow);
        }
        
        static string ShowNameDialog()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "RPS Arena - Ingresa tu nombre";
                dialog.Size = new System.Drawing.Size(300, 150);
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                
                var label = new Label
                {
                    Text = "Nombre del jugador:",
                    Location = new System.Drawing.Point(20, 20),
                    Size = new System.Drawing.Size(250, 20)
                };
                
                var textBox = new TextBox
                {
                    Location = new System.Drawing.Point(20, 50),
                    Size = new System.Drawing.Size(240, 20),
                    Text = "Jugador_" + new Random().Next(1000, 9999)
                };
                
                var btnOk = new Button
                {
                    Text = "Jugar",
                    Location = new System.Drawing.Point(80, 80),
                    Size = new System.Drawing.Size(100, 30),
                    DialogResult = DialogResult.OK
                };
                
                dialog.Controls.AddRange(new Control[] { label, textBox, btnOk });
                dialog.AcceptButton = btnOk;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return string.IsNullOrWhiteSpace(textBox.Text) 
                        ? "Jugador" 
                        : textBox.Text.Trim();
                }
                
                return null;
            }
        }
    }
}
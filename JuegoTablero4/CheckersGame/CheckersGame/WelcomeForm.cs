using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckersGame
{
    public partial class WelcomeForm : Form
    {
        public WelcomeForm()
        {
            // Configuración general
            this.Text = "¡Bienvenido!";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(245, 245, 245); // Fondo claro

            // Panel decorativo
            Panel panel = new Panel();
            panel.Size = new Size(580, 460);
            panel.Location = new Point(10, 10);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panel);

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = "🎮 ¡Bienvenido al juego de Damas!";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(40, 40, 40);
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Size = new Size(560, 40);
            lblTitulo.Location = new Point(10, 20);
            panel.Controls.Add(lblTitulo);

            // Reglas
            Label lblReglas = new Label();
            lblReglas.Text =
                "- Presiona 'Iniciar' para comenzar.\n" +
                "- Turno por ficha: empieza la blanca.\n" +
                "- Comer es obligatorio si es posible.\n" +
                "- El tablero muestra movimientos válidos.\n" +
                "- El reloj inicia al mover: si se vence, pierdes.\n" +
                "- Debes pasar el turno al finalizar.\n" +
                "- Gana quien tenga más fichas si el tiempo se vence.\n" +
                "- Puedes coronar: como Reina/Rey, mueve 1 casilla en cualquier dirección.";
            lblReglas.Font = new Font("Segoe UI", 10);
            lblReglas.ForeColor = Color.FromArgb(60, 60, 60);
            lblReglas.Size = new Size(540, 280);
            lblReglas.Location = new Point(20, 70);
            lblReglas.TextAlign = ContentAlignment.TopLeft;
            panel.Controls.Add(lblReglas);

            // Botón ¡VAMOS!
            Button btnVamos = new Button();
            btnVamos.Text = "¡VAMOS!";
            btnVamos.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnVamos.Size = new Size(120, 45);
            btnVamos.Location = new Point(230, 370);
            btnVamos.BackColor = Color.FromArgb(0, 120, 215);
            btnVamos.ForeColor = Color.White;
            btnVamos.FlatStyle = FlatStyle.Flat;
            btnVamos.FlatAppearance.BorderSize = 0;
            btnVamos.Cursor = Cursors.Hand;
            btnVamos.Click += (s, e) => this.Close();
            panel.Controls.Add(btnVamos);


        }

        private void WelcomeForm_Load(object sender, EventArgs e)
        {

        }
    }
}

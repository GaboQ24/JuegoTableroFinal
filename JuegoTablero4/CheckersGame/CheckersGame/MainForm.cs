// Importa funcionalidades básicas del sistema
using System;
// Permite usar listas y colecciones genéricas
using System.Collections.Generic;
// Proporciona tipos gráficos como Point y Color
using System.Drawing;
// Proporciona conteo real al contador
using System.Diagnostics;
// Permite usar formularios Windows Forms
using System.Windows.Forms;

using System.Threading.Tasks;

namespace CheckersGame
{
    // Clase principal que representa el formulario del juego de damas
    public partial class MainForm : Form
    {
        // Constante que define el tamaño del tablero (8x8)
        const int BoardSize = 8;
        // Constante que define el tamaño de cada celda del tablero en pixeles
        const int TileSize = 60;

        // Matriz bidimensional que representa el tablero con botones
        private Button[,] tiles = new Button[BoardSize, BoardSize];
        // String que indica el jugador actual ("W" para blanco, "B" para negro)
        private string currentPlayer = "W";
        // Botón que representa la ficha seleccionada actualmente por el jugador
        private Button selectedPiece = null;
        // Booleano que indica si el jugador debe continuar una captura múltiple obligatoria
        private bool mustContinueCapture = false;
        // Etiqueta que muestra el reloj blanco
        private Label lblWhiteClock;
        // Etiqueta que muestra el reloj negro
        private Label lblBlackClock;
        // Etiqueta que muestra el jugador que tiene el turno
        private Label lblTurn;
        // Etiqueta que muestra el tiempo restante de la partida
        private Label lblTimer;

        // Temporizador para controlar el tiempo límite del white
        private Timer whiteTimer;
        // Temporizador para controlar el tiempo límite del black
        private Timer blackTimer;
        // Variable que almacena el tiempo del jugador white (3 minutos)
        private int whiteTimeLeft = 180;
        // Variable que almacena el tiempo del jugador black (3 minutos)
        private int blackTimeLeft = 180;

        private Stopwatch whiteStopwatch;
        private Stopwatch blackStopwatch;
        private int whiteTimeLimit = 180; // 3 minutos
        private int blackTimeLimit = 180;
        private Timer displayTimer;
        //private Label lblWhiteClock;
        //private Label lblBlackClock;
        private Button btnWhiteSwitch;
        private Button btnBlackSwitch;


        // Boton que servira para inciar el juego y el temporalizador de los 2 jugadores
        private Button startBtn;

        // Boton que sirve para pasar de turno y asi solo se gasta el tiempo de 1 solo jugador por turno
        private Button passBtn;

        // Constructor del formulario principal
        public MainForm()
        {
            InitializeComponent(); // Inicializa componentes visuales predeterminados
            new WelcomeForm().ShowDialog(); //Es el menu con las reglas del juego
            InitializeBoard();     // Crea y configura el tablero de juego
            InitializeLabels();    // Crea etiquetas para mostrar turno y tiempo
            InitializeTimers();     // Configura el temporizador de la partida
            InitializeGameControls();
        }

        private void SetBoardEnabled(bool enabled)
        {
            foreach (var btn in tiles)
                btn.Enabled = enabled;
        }

        private string GetTileNotation(Point pos)
        {
            char col = (char)('A' + pos.X);
            int row = BoardSize - pos.Y;
            return $"{col}{row}";
        }

        private List<string> moveHistory = new List<string>();



        // Inicializa el tablero de damas con botones y las piezas en sus posiciones iniciales
        private void InitializeBoard()
        {
            this.Text = "Juego de Damas"; // Establece el título de la ventana
            // Define el tamaño de la ventana considerando el tamaño del tablero y margen extra
            this.ClientSize = new Size(BoardSize * TileSize + 20, BoardSize * TileSize + 100);
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Establece borde fijo para no poder redimensionar
            this.MaximizeBox = false; // Deshabilita la opción de maximizar la ventana

            // Ciclo anidado para crear cada botón que representa una casilla del tablero
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Button tile = new Button(); // Crea un nuevo botón
                    tile.Size = new Size(TileSize, TileSize); // Define tamaño del botón
                    tile.Location = new Point(col * TileSize + 10, row * TileSize + 10); // Ubicación en el formulario
                    // Colorea la casilla según patrón clásico (blanco/beige y marrón)
                    tile.BackColor = (row + col) % 2 == 0 ? Color.Beige : Color.Brown;
                    tile.Tag = new Point(row, col); // Guarda la posición como etiqueta para facilitar referencia
                    tile.Font = new Font("Arial", 14, FontStyle.Bold); // Define fuente y estilo del texto en el botón
                    tile.Click += Tile_Click; // Asocia el evento click al método Tile_Click

                    // Coloca las fichas negras en las primeras 3 filas, solo en casillas oscuras
                    if ((row + col) % 2 != 0 && row < 3)
                        tile.Text = "B";
                    // Coloca las fichas blancas en las últimas 3 filas, solo en casillas oscuras
                    else if ((row + col) % 2 != 0 && row > 4)
                        tile.Text = "W";

                    tiles[row, col] = tile; // Guarda el botón en la matriz
                    this.Controls.Add(tile); // Agrega el botón al formulario para que sea visible
                }
            }

            // Crea un botón para reiniciar la partida
            Button resetBtn = new Button();
            resetBtn.Text = "Reiniciar"; // Texto del botón
            resetBtn.Location = new Point(10, BoardSize * TileSize + 15); // Posición debajo del tablero
            resetBtn.Click += (s, e) => Application.Restart(); // Evento click reinicia la aplicación
            this.Controls.Add(resetBtn); // Agrega el botón al formulario
            SetBoardEnabled(false); // Desactiva todas las casillas al inicio
            moveHistory.Clear();

        }

    // Inicializa las etiquetas que indican el jugador actual y el tiempo restante
    private void InitializeLabels()
        {
            lblTurn = new Label(); // Crea la etiqueta para mostrar el turno
            lblTurn.Text = "Turno: Blanco"; // Texto inicial indicando que comienza el jugador blanco
            lblTurn.Location = new Point(120, BoardSize * TileSize + 20); // Posición junto al botón reiniciar
            lblTurn.AutoSize = true; // Ajusta tamaño automáticamente
            this.Controls.Add(lblTurn); // Agrega al formulario
        }

        private void UpdateStopwatchDisplay()
        {
            if (!whiteStopwatch.IsRunning && !blackStopwatch.IsRunning)
                return; // No hay necesidad de actualizar si ambos están detenidos

            int whiteElapsed = (int)whiteStopwatch.Elapsed.TotalSeconds;
            int blackElapsed = (int)blackStopwatch.Elapsed.TotalSeconds;

            int whiteRemaining = Math.Max(whiteTimeLimit - whiteElapsed, 0);
            int blackRemaining = Math.Max(blackTimeLimit - blackElapsed, 0);

            lblWhiteClock.Text = $"{whiteRemaining / 60:D2}:{whiteRemaining % 60:D2}";
            lblBlackClock.Text = $"{blackRemaining / 60:D2}:{blackRemaining % 60:D2}";

            UpdateClockStyles();

            if (whiteRemaining == 0 && currentPlayer == "W")
            {
                displayTimer.Stop();
                MessageBox.Show("Tiempo agotado. Gana el jugador negro", "Fin del juego");
                DisableBoard();
            }

            if (blackRemaining == 0 && currentPlayer == "B")
            {
                displayTimer.Stop();
                MessageBox.Show("Tiempo agotado. Gana el jugador blanco", "Fin del juego");
                DisableBoard();
            }
        }


        // Inicializa y configura el temporizador de la partida
        private void InitializeTimers()
        {
            whiteStopwatch = new Stopwatch();
            blackStopwatch = new Stopwatch();

            displayTimer = new Timer();
            displayTimer.Interval = 1000; // Actualiza cada segundo visualmente
            displayTimer.Tick += (s, e) => UpdateStopwatchDisplay();
            displayTimer.Start();
        }

        private void InitializeGameControls()
        {
            
            startBtn = new Button();
            startBtn.Text = "Iniciar";
            startBtn.Location = new Point(10, BoardSize * TileSize + 55);
            startBtn.Click += (s, e) =>
            {
                startBtn.Enabled = false;
                SetBoardEnabled(true); // Habilita las casillas para comenzar a jugar
                whiteStopwatch.Stop();
                blackStopwatch.Stop();
                if (currentPlayer == "W")
                    whiteStopwatch.Start();
                else
                    blackStopwatch.Start();
            };
            this.Controls.Add(startBtn);
            Button historyBtn = new Button();
            historyBtn.Text = "Historial";
            historyBtn.Location = new Point(100, BoardSize * TileSize + 55);
            historyBtn.Click += (s, e) =>
            {
                string historial = moveHistory.Count == 0
                    ? "No hay movimientos registrados aún."
                    : string.Join("\n", moveHistory);

                MessageBox.Show(historial, "Historial de movimientos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.Controls.Add(historyBtn);


            Button aboutBtn = new Button();
            aboutBtn.Text = "Acerca de";
            aboutBtn.Font = new Font("Arial", 8, FontStyle.Regular);
            aboutBtn.Size = new Size(75, 25);
            aboutBtn.Location = new Point(190, BoardSize * TileSize + 55);
            aboutBtn.Click += (s, e) =>
            {
                string mensaje = "Desarrolladores:\n" +
                                 "Luis Alvarez\n" +
                                 "Gabriel Quesada\n" +
                                 "Jimmy Mata\n" +
                                 "Universidad Tecnológica Costarricense\n" +
                                 "Programación V - Proyecto Final Programado\n" +
                                 "Año 2025 - 2do Cuatrimestre";
                MessageBox.Show(mensaje, "Acerca de", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.Controls.Add(aboutBtn);

            lblWhiteClock = new Label();
            lblWhiteClock.Text = "03:00";
            lblWhiteClock.Font = new Font("Consolas", 14, FontStyle.Bold);
            lblWhiteClock.BackColor = Color.LightGray;
            lblWhiteClock.ForeColor = Color.Black;
            lblWhiteClock.Size = new Size(80, 30);
            lblWhiteClock.TextAlign = ContentAlignment.MiddleCenter;
            lblWhiteClock.Location = new Point(300, BoardSize * TileSize + 50);
            this.Controls.Add(lblWhiteClock);

            lblBlackClock = new Label();
            lblBlackClock.Text = "03:00";
            lblBlackClock.Font = new Font("Consolas", 14, FontStyle.Bold);
            lblBlackClock.BackColor = Color.LightGray;
            lblBlackClock.ForeColor = Color.Black;
            lblBlackClock.Size = new Size(80, 30);
            lblBlackClock.TextAlign = ContentAlignment.MiddleCenter;
            lblBlackClock.Location = new Point(390, BoardSize * TileSize + 50);
            this.Controls.Add(lblBlackClock);

            // Botón switch para jugador blanco
            btnWhiteSwitch = new Button();
            btnWhiteSwitch.Text = "◀";
            btnWhiteSwitch.Font = new Font("Arial", 10, FontStyle.Bold);
            btnWhiteSwitch.Size = new Size(30, 30);
            btnWhiteSwitch.BackColor = Color.White;
            btnWhiteSwitch.ForeColor = Color.Black;
            btnWhiteSwitch.FlatStyle = FlatStyle.Flat;
            btnWhiteSwitch.Location = new Point(lblWhiteClock.Left + 25, lblWhiteClock.Top - 35);
            btnWhiteSwitch.Click += async (s, e) =>
            {
                if (currentPlayer == "B")
                {
                    AnimateClockButton(btnWhiteSwitch, Color.LightGray, Color.White);
                    EndTurn();
                    TogglePlayerTimers();
                    SetBoardEnabled(true);
                    UpdateClockStyles();
                }
            };
            this.Controls.Add(btnWhiteSwitch);

            // Botón switch para jugador negro
            btnBlackSwitch = new Button();
            btnBlackSwitch.Text = "▶";
            btnBlackSwitch.Font = new Font("Arial", 10, FontStyle.Bold);
            btnBlackSwitch.Size = new Size(30, 30);
            btnBlackSwitch.BackColor = Color.Black;
            btnBlackSwitch.ForeColor = Color.White;
            btnBlackSwitch.FlatStyle = FlatStyle.Flat;
            btnBlackSwitch.Location = new Point(lblBlackClock.Left + 25, lblBlackClock.Top - 35);
            btnBlackSwitch.Click += async (s, e) =>
            {
                if (currentPlayer == "W")
                {
                    AnimateClockButton(btnBlackSwitch, Color.Gray, Color.Black);
                    EndTurn();
                    TogglePlayerTimers();
                    SetBoardEnabled(true);
                    UpdateClockStyles();
                }
            };

            this.Controls.Add(btnBlackSwitch);
        }

        private async void AnimateClockButton(Button btn, Color flashColor, Color originalColor)
        {
            btn.BackColor = flashColor;
            await Task.Delay(150); // Espera 150 ms
            btn.BackColor = originalColor;
        }


        private void TogglePlayerTimers()
        {
            whiteStopwatch.Stop();
            blackStopwatch.Stop();

            if (currentPlayer == "W")
                whiteStopwatch.Start();
            else
                blackStopwatch.Start();
        }


        private void UpdateClockStyles()
        {
            if (currentPlayer == "W")
            {
                if (lblWhiteClock.BackColor != Color.LightGreen)
                    lblWhiteClock.BackColor = Color.LightGreen;
                if (lblBlackClock.BackColor != Color.LightGray)
                    lblBlackClock.BackColor = Color.LightGray;
            }
            else
            {
                if (lblBlackClock.BackColor != Color.LightGreen)
                    lblBlackClock.BackColor = Color.LightGreen;
                if (lblWhiteClock.BackColor != Color.LightGray)
                    lblWhiteClock.BackColor = Color.LightGray;
            }

            btnWhiteSwitch.Enabled = currentPlayer == "B";
            btnBlackSwitch.Enabled = currentPlayer == "W";
        }

        private void DisableBoard()
        {
            foreach (var btn in tiles)
                btn.Enabled = false;
        }

        // Evento que ocurre cuando se hace clic en una casilla (botón) del tablero
        private void Tile_Click(object sender, EventArgs e)
        {

            if (!tiles[0, 0].Enabled) return; // Evita interacción si el tablero está bloqueado
            Button clicked = sender as Button; // Convierte sender a botón
            Point pos = (Point)clicked.Tag;    // Obtiene posición fila/columna

            // Si no hay ficha seleccionada previamente
            if (selectedPiece == null)
            {
                // Si la ficha clickeada pertenece al jugador actual
                if (IsCurrentPlayerPiece(clicked.Text))
                {
                    selectedPiece = clicked; // La selecciona
                    HighlightPossibleMoves(selectedPiece); // Resalta movimientos posibles
                }
            }
            else // Si ya hay ficha seleccionada
            {
                Point from = (Point)selectedPiece.Tag; // Posición origen
                Point to = (Point)clicked.Tag;         // Posición destino
                int dx = to.X - from.X;                 // Diferencia filas
                int dy = to.Y - from.Y;                 // Diferencia columnas

                // Movimiento normal (sin salto), solo permitido si no hay capturas obligatorias ni continúa captura múltiple
                if (Math.Abs(dx) == 1 && Math.Abs(dy) == 1 && clicked.Text == "" &&
                    !mustContinueCapture && !HasMandatoryCaptures())
                {
                    if (IsValidDirection(from, to)) // Valida que dirección sea correcta para el tipo de pieza
                    {
                        MovePiece(from, to);     // Mueve la ficha
                        string fromNotation = GetTileNotation(from);
                        string toNotation = GetTileNotation(to);
                        string piece = selectedPiece.Text;

                        moveHistory.Add($"{piece}: {fromNotation} → {toNotation}");
                        PromoteIfNeeded(to);    // Promociona a rey si es necesario
                        //EndTurn();             // Finaliza el turno y cambia jugador
                        selectedPiece = null;
                        ClearHighlights();

                        SetBoardEnabled(false); // Bloquea el tablero hasta que se pase el turno
                    }
                }
                // Movimiento de salto / captura (doble salto)
                else if (Math.Abs(dx) == 2 && Math.Abs(dy) == 2 && clicked.Text == "")
                {
                    int midX = from.X + dx / 2; // Posición fila de la ficha capturada
                    int midY = from.Y + dy / 2; // Posición columna de la ficha capturada
                    Button midTile = tiles[midX, midY]; // Botón de la ficha capturada

                    // Verifica que haya ficha enemiga para capturar
                    if (midTile.Text != "" && !IsSamePlayer(midTile.Text))
                    {
                        midTile.Text = ""; // Elimina ficha capturada
                        MovePiece(from, to); // Mueve ficha a destino
                        string fromNotation = GetTileNotation(from);
                        string toNotation = GetTileNotation(to);
                        string piece = selectedPiece.Text;

                        moveHistory.Add($"{piece}: {fromNotation} → {toNotation}");
                        PromoteIfNeeded(to); // Promociona si corresponde

                        // Si no hay más capturas posibles
                        mustContinueCapture = false;
                        selectedPiece = null; // Deselecciona pieza
                        //EndTurn(); // Finaliza turno y cambia jugador
                        ClearHighlights();

                        SetBoardEnabled(false); //  Bloquea el tablero al terminar captura

                    }
                }
                else // Movimiento inválido o fuera de las reglas
                {
                    selectedPiece = null; // Deselecciona pieza
                    ClearHighlights();    // Limpia los resaltados en el tablero
                }
            }
        }

        // Mueve una ficha del punto "from" al punto "to"
        private void MovePiece(Point from, Point to)
        {
            tiles[to.X, to.Y].Text = tiles[from.X, from.Y].Text; // Copia el texto (pieza) al destino
            tiles[from.X, from.Y].Text = "";                     // Limpia la casilla origen
        }

        // Promociona una ficha a "rey" si llega al lado opuesto
        private void PromoteIfNeeded(Point pos)
        {
            string piece = tiles[pos.X, pos.Y].Text; // Obtiene ficha en posición pos
            if (piece == "W" && pos.X == 0)           // Si ficha blanca llega a fila 0
                tiles[pos.X, pos.Y].Text = "WK";     // Se promociona a rey blanco
            else if (piece == "B" && pos.X == 7)      // Si ficha negra llega a fila 7
                tiles[pos.X, pos.Y].Text = "BK";     // Se promociona a rey negro
        }

        // Valida que el movimiento respete la dirección permitida según tipo de ficha
        private bool IsValidDirection(Point from, Point to)
        {
            string piece = tiles[from.X, from.Y].Text; // Obtiene ficha origen
            if (piece.EndsWith("K")) return true;      // Rey puede moverse en todas direcciones
            if (piece == "W") return to.X < from.X;    // Blanco solo puede subir (fila menor)
            if (piece == "B") return to.X > from.X;    // Negro solo puede bajar (fila mayor)
            return false;                              // Otros casos inválidos
        }

        // Verifica si un texto representa una ficha del jugador actual
        private bool IsCurrentPlayerPiece(string text)
        {
            return text == currentPlayer || text == currentPlayer + "K";
        }

        // Verifica si un texto representa una ficha del mismo jugador
        private bool IsSamePlayer(string text)
        {
            return text.StartsWith(currentPlayer);
        }

        // Resalta las jugadas posibles para la ficha seleccionada
        private void HighlightPossibleMoves(Button piece)
        {
            ClearHighlights(); // Primero limpia cualquier resaltado previo
            Point pos = (Point)piece.Tag; // Obtiene posición de la ficha
            string text = piece.Text;      // Obtiene el texto de la ficha (W, B, WK, BK)
            var directions = GetDirections(text); // Obtiene las direcciones válidas de movimiento

            foreach (var (dx, dy) in directions)
            {
                int x = pos.X + dx; // Posición fila para movimiento simple
                int y = pos.Y + dy; // Posición columna para movimiento simple

                // Resalta en verde movimientos simples válidos, solo si no hay capturas obligatorias
                if (IsInside(x, y) && tiles[x, y].Text == "" && !HasMandatoryCaptures())
                    tiles[x, y].BackColor = Color.LightGreen;

                // Posiciones para posibles saltos/capturas
                int jumpX = pos.X + 2 * dx;
                int jumpY = pos.Y + 2 * dy;
                int midX = pos.X + dx;
                int midY = pos.Y + dy;

                // Resalta en naranja movimientos de captura válidos
                if (IsInside(jumpX, jumpY) && tiles[jumpX, jumpY].Text == "" &&
                    tiles[midX, midY].Text != "" && !IsSamePlayer(tiles[midX, midY].Text))
                {
                    tiles[jumpX, jumpY].BackColor = Color.Orange;
                }
            }
        }

        // Devuelve las direcciones posibles para la ficha dependiendo si es rey o normal
        private List<(int dx, int dy)> GetDirections(string piece)
        {
            var dirs = new List<(int dx, int dy)>(); // Lista de tuplas con las direcciones
            if (piece.EndsWith("K")) // Rey puede moverse en las 4 diagonales
                dirs.AddRange(new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) });
            else if (piece == "W") // Blanco solo puede moverse hacia arriba
                dirs.AddRange(new[] { (-1, -1), (-1, 1) });
            else if (piece == "B") // Negro solo puede moverse hacia abajo
                dirs.AddRange(new[] { (1, -1), (1, 1) });
            return dirs;
        }

        // Limpia todos los resaltados del tablero dejando los colores base
        private void ClearHighlights()
        {
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    tiles[r, c].BackColor = (r + c) % 2 == 0 ? Color.Beige : Color.Brown;
        }

        // Verifica si una ficha en una posición puede realizar una captura
        private bool CanCapture(Point pos)
        {
            string piece = tiles[pos.X, pos.Y].Text; // Obtiene ficha en posición
            foreach (var (dx, dy) in GetDirections(piece)) // Recorre direcciones posibles
            {
                int midX = pos.X + dx; // Posición fila de ficha enemiga
                int midY = pos.Y + dy; // Posición columna de ficha enemiga
                int jumpX = pos.X + 2 * dx; // Posición fila de destino tras salto
                int jumpY = pos.Y + 2 * dy; // Posición columna de destino tras salto

                // Verifica que la casilla de salto esté dentro, vacía y que haya ficha enemiga para capturar
                if (IsInside(jumpX, jumpY) && tiles[jumpX, jumpY].Text == "" &&
                    tiles[midX, midY].Text != "" && !IsSamePlayer(tiles[midX, midY].Text))
                    return true; // Hay captura disponible
            }
            return false; // No hay capturas posibles
        }

        // Verifica si una coordenada está dentro de los límites del tablero
        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }

        // Finaliza el turno actual y cambia al siguiente jugador
        private void EndTurn()
        {
            selectedPiece = null; // Deselecciona pieza
            ClearHighlights();    // Limpia resaltados
            currentPlayer = currentPlayer == "W" ? "B" : "W"; // Cambia jugador
            lblTurn.Text = $"Turno: {(currentPlayer == "W" ? "Blanco" : "Negro")}"; // Actualiza etiqueta turno

            // Si se terminó el juego, anuncia ganador
            if (IsGameOver())
            {
                MessageBox.Show($"Jugador {(currentPlayer == "W" ? "Negro" : "Blanco")} gana", "Fin del Juego");
                foreach (var btn in tiles)
                    btn.Enabled = false; // Deshabilita tablero para no seguir jugando
            }
        }

        // Verifica si el juego ha terminado (sin movimientos para el jugador actual)
        private bool IsGameOver()
        {
            foreach (var btn in tiles)
            {
                // Si hay ficha del jugador que puede mover o capturar, el juego sigue
                if (IsCurrentPlayerPiece(btn.Text) && CanMoveOrCapture((Point)btn.Tag))
                    return false;
            }
            return true; // No hay movimientos => juego terminado
        }

        // Verifica si una ficha puede moverse o capturar
        private bool CanMoveOrCapture(Point pos)
        {
            string piece = tiles[pos.X, pos.Y].Text; // Obtiene ficha en posición
            foreach (var (dx, dy) in GetDirections(piece))
            {
                int newX = pos.X + dx;
                int newY = pos.Y + dy;
                int jumpX = pos.X + 2 * dx;
                int jumpY = pos.Y + 2 * dy;
                int midX = pos.X + dx;
                int midY = pos.Y + dy;

                // Movimiento simple válido
                if (IsInside(newX, newY) && tiles[newX, newY].Text == "")
                    return true;

                // Movimiento salto/captura válido
                if (IsInside(jumpX, jumpY) && tiles[jumpX, jumpY].Text == "" &&
                    tiles[midX, midY].Text != "" && !IsSamePlayer(tiles[midX, midY].Text))
                    return true;
            }
            return false;
        }

        // Verifica si el jugador actual tiene capturas obligatorias
        private bool HasMandatoryCaptures()
        {
            // Recorre todo el tablero buscando fichas del jugador que puedan capturar
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    if (IsCurrentPlayerPiece(tiles[r, c].Text) && CanCapture(new Point(r, c)))
                        return true; // Hay captura obligatoria
            return false; // No hay capturas obligatorias
        }
    }
}

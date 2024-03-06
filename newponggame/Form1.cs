using System.Windows.Forms;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using System;
using System.Drawing;

namespace newponggame
{
	public partial class Form1 : Form
	{
		private const int Width = 800;
		private const int Height = 600;
		private const int BallRadius = 10;
		private const int PaddleWidth = 10;
		private const int PaddleHeight = 100;
		private const int PaddleSpeed = 10;
		private const int BallVelocityIncrement = 8;
		private const int ScoreLimit = 5; // Score limit to win the game
		private readonly Color White = Color.White;
		private readonly Color Black = Color.Black;
		private void ResetBall()
		{
			ballPosition = new Point(Width / 2, Height / 2);
			Random rnd = new Random();
			ballVelocity = new Point(rnd.Next(-2, 2), rnd.Next(-2, 2));
		}
		private Point ballPosition = new Point(Width / 2, Height / 2);
		private Point ballVelocity = new Point(new Random().Next(-5, 5), new Random().Next(-5, 5));
		private Point player1Position = new Point(0, Height / 2 - PaddleHeight / 2);
		private Point player2Position = new Point(Width - PaddleWidth, Height / 2 - PaddleHeight / 2);
		private int player1Score = 0;
		private int player2Score = 0;

		private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

		public Form1()
		{
			InitializeComponent();
			this.ClientSize = new Size(Width, Height);
			this.BackColor = Black;
			this.Paint += Form1_Paint;
			this.KeyDown += Form1_KeyDown;
			this.DoubleBuffered = true;

			timer.Interval = 16; // ~60 FPS
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.FillEllipse(new SolidBrush(White), ballPosition.X - BallRadius, ballPosition.Y - BallRadius, BallRadius * 2, BallRadius * 2);
			g.FillRectangle(new SolidBrush(White), player1Position.X, player1Position.Y, PaddleWidth, PaddleHeight);
			g.FillRectangle(new SolidBrush(White), player2Position.X, player2Position.Y, PaddleWidth, PaddleHeight);

			// Draw scores
			string player1ScoreText = "Player 1: " + player1Score;
			string player2ScoreText = "Player 2: " + player2Score;
			Font font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Regular);
			SizeF player1ScoreSize = g.MeasureString(player1ScoreText, font);
			SizeF player2ScoreSize = g.MeasureString(player2ScoreText, font);
			g.DrawString(player1ScoreText, font, Brushes.White, new PointF((Width - player1ScoreSize.Width) / 2, 10));
			g.DrawString(player2ScoreText, font, Brushes.White, new PointF((Width - player2ScoreSize.Width) / 2, 30 + player1ScoreSize.Height));
		}
		private void CollisionDetection()
		{
			// Collision detection between ball and top/bottom boundaries
			if (ballPosition.Y - BallRadius <= 0 || ballPosition.Y + BallRadius >= Height)
			{
				ballVelocity.Y *= -1;
			}

			// Collision detection between ball and left/right boundaries
			if (ballPosition.X - BallRadius <= 0 || ballPosition.X + BallRadius >= Width)
			{
				ballVelocity.X *= -1;
			}

			// Collision detection between ball and left paddle
			if (ballPosition.X - BallRadius <= PaddleWidth && ballPosition.Y >= player1Position.Y && ballPosition.Y <= player1Position.Y + PaddleHeight)
			{
				ballVelocity.X *= -1;
			}

			// Collision detection between ball and right paddle
			if (ballPosition.X + BallRadius >= Width - PaddleWidth && ballPosition.Y >= player2Position.Y && ballPosition.Y <= player2Position.Y + PaddleHeight)
			{
				ballVelocity.X *= -1;
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			MovePlayer1(); // Move player 1's paddle
			MovePlayer2(); // Move player 2's paddle
			MoveBall(); // Move the ball
			CollisionDetection(); // Check for collisions
			Invalidate();
		}

		private void MoveBall()
		{
			ballPosition.X += ballVelocity.X * BallVelocityIncrement;
			ballPosition.Y += ballVelocity.Y * BallVelocityIncrement;

			if (ballPosition.Y <= 0 || ballPosition.Y >= Height)
				ballVelocity.Y *= -1;

			if (ballPosition.X <= PaddleWidth && ballPosition.Y >= player1Position.Y && ballPosition.Y <= player1Position.Y + PaddleHeight)
				ballVelocity.X *= -1;

			if (ballPosition.X >= Width - PaddleWidth && ballPosition.Y >= player2Position.Y && ballPosition.Y <= player2Position.Y + PaddleHeight)
				ballVelocity.X *= -1;

			if (ballPosition.X < 0 || ballPosition.X > Width)
			{
				if (ballPosition.X < 0)
					player2Score++;
				else
					player1Score++;

				ballPosition = new Point(Width / 2, Height / 2);
			}

			// Check if any player wins
			if (player1Score >= ScoreLimit || player2Score >= ScoreLimit)
			{
				timer.Stop();
				string winner = (player1Score >= ScoreLimit) ? "Player 1" : "Player 2";
				MessageBox.Show(winner + " wins!");
			}
		}

		private void MovePlayer1()
		{
			// Move player 1's paddle
			if (player1Position.Y > 0 && GetAsyncKeyState(Keys.W) < 0)
				player1Position.Y -= PaddleSpeed;
			if (player1Position.Y < Height - PaddleHeight && GetAsyncKeyState(Keys.S) < 0)
				player1Position.Y += PaddleSpeed;
		}

		private void MovePlayer2()
		{
			// Move player 2's paddle
			if (player2Position.Y > 0 && GetAsyncKeyState(Keys.Up) < 0)
				player2Position.Y -= PaddleSpeed;
			if (player2Position.Y < Height - PaddleHeight && GetAsyncKeyState(Keys.Down) < 0)
				player2Position.Y += PaddleSpeed;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(Keys vKey);

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			// Allow player 1 to control the paddle using arrow keys
			if (e.KeyCode == Keys.W && player1Position.Y > 0)
				player1Position.Y -= PaddleSpeed;
			if (e.KeyCode == Keys.S && player1Position.Y < Height - PaddleHeight)
				player1Position.Y += PaddleSpeed;

			// Allow player 2 to control the paddle using 'W' and 'S' keys
			if (e.KeyCode == Keys.Up && player2Position.Y > 0)
				player2Position.Y -= PaddleSpeed;
			if (e.KeyCode == Keys.Down && player2Position.Y < Height - PaddleHeight)
				player2Position.Y += PaddleSpeed;
		}
	}
}
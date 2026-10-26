using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace okrlab_2
{
    public partial class Form1 : Form
    {
        // Баланс та вибір фішки
        private int balance = 1000;
        private int selectedChip = 10;

        // Словники ставок
        private Dictionary<Button, int> placedBets = new Dictionary<Button, int>();
        private Dictionary<Button, string> betTypes = new Dictionary<Button, string>();

        // Червоні числа рулетки
        private readonly HashSet<int> redNumbers = new HashSet<int>
        {
            1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
        };

        // Порядок чисел на європейській рулетці
        private readonly int[] wheelNumbers = new int[]
        {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
            5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        // Історія останніх чисел
        private List<int> historyNumbers = new List<int>();
        private FlowLayoutPanel panelHistory;

        // Рендер анімації (60 FPS)
        private Stopwatch spinStopwatch = new Stopwatch();
        private float currentAngle = 0f;
        private float startAngle = 0f;
        private float totalRotationAngle = 0f;
        private const double spinDurationMs = 4200; // 4.2 секунди обертання
        private bool isSpinning = false;
        private int targetWinningNumber = 0;
        private Image wheelImage;

        public Form1()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            InitRouletteLogic();
            InitHistoryUI();
        }

        private void InitRouletteLogic()
        {
            UpdateBalanceUI();

            if (pictureBox1.Image != null)
            {
                wheelImage = new Bitmap(pictureBox1.Image);
                pictureBox1.Image = null; // Рендеримо вручну через Paint
            }

            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Click += OnWheelClick;

            // Кліки по фішках
            pictureBox2.Click += (s, e) => SelectChip(10, pictureBox2);
            pictureBox3.Click += (s, e) => SelectChip(25, pictureBox3);
            pictureBox4.Click += (s, e) => SelectChip(50, pictureBox4);
            pictureBox5.Click += (s, e) => SelectChip(100, pictureBox5);
            pictureBox6.Click += (s, e) => SelectChip(500, pictureBox6);

            HighlightSelectedChip(pictureBox2);

            // Кнопки столу: 0
            RegisterBetButton(button37, "0");

            // Ряд 1 (3..36)
            Button[] row1 = { button2, button3, button4, button5, button6, button7, button8, button9, button10, button11, button12, button13 };
            for (int i = 0; i < row1.Length; i++)
                RegisterBetButton(row1[i], ((i + 1) * 3).ToString());

            // Ряд 2 (2..35)
            Button[] row2 = { button14, button15, button16, button17, button18, button19, button20, button21, button22, button23, button24, button25 };
            for (int i = 0; i < row2.Length; i++)
                RegisterBetButton(row2[i], (2 + i * 3).ToString());

            // Ряд 3 (1..34)
            RegisterBetButton(button1, "1");
            Button[] row3 = { button26, button27, button28, button29, button30, button31, button32, button33, button34, button35, button36 };
            for (int i = 0; i < row3.Length; i++)
                RegisterBetButton(row3[i], (4 + i * 3).ToString());

            // Колонки "2 to 1"
            RegisterBetButton(button49, "COL_3");
            RegisterBetButton(button48, "COL_2");
            RegisterBetButton(button47, "COL_1");

            // Дюжини
            RegisterBetButton(button38, "1st12");
            RegisterBetButton(button39, "2st12");
            RegisterBetButton(button40, "3st12");

            // Зовнішні ставки
            RegisterBetButton(button41, "1to18");
            RegisterBetButton(button42, "EVEN");
            RegisterBetButton(button45, "RED");
            RegisterBetButton(button46, "BLACK");
            RegisterBetButton(button44, "ODD");
            RegisterBetButton(button43, "19to36");
        }

        // Панель історії останніх випавших чисел у правому верхньому кутку
        private void InitHistoryUI()
        {
            Label lblHistoryTitle = new Label();
            lblHistoryTitle.Text = "Історія:";
            lblHistoryTitle.ForeColor = Color.White;
            lblHistoryTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblHistoryTitle.AutoSize = true;
            lblHistoryTitle.Location = new Point(this.ClientSize.Width - 340, 12);
            this.Controls.Add(lblHistoryTitle);

            panelHistory = new FlowLayoutPanel();
            panelHistory.Location = new Point(this.ClientSize.Width - 280, 8);
            panelHistory.Size = new Size(260, 36);
            panelHistory.BackColor = Color.Transparent;
            panelHistory.FlowDirection = FlowDirection.LeftToRight;
            panelHistory.WrapContents = false;
            this.Controls.Add(panelHistory);
        }

        private void AddNumberToHistory(int num)
        {
            historyNumbers.Insert(0, num);
            if (historyNumbers.Count > 8)
            {
                historyNumbers.RemoveAt(historyNumbers.Count - 1);
            }

            panelHistory.Controls.Clear();
            foreach (int item in historyNumbers)
            {
                Label badge = new Label();
                badge.Text = item.ToString();
                badge.AutoSize = false;
                badge.Size = new Size(28, 28);
                badge.TextAlign = ContentAlignment.MiddleCenter;
                badge.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                badge.ForeColor = Color.White;
                badge.Margin = new Padding(2, 0, 2, 0);

                if (item == 0) badge.BackColor = Color.ForestGreen;
                else if (redNumbers.Contains(item)) badge.BackColor = Color.Crimson;
                else badge.BackColor = Color.Black;

                panelHistory.Controls.Add(badge);
            }
        }

        private void RegisterBetButton(Button btn, string betType)
        {
            if (btn == null) return;
            betTypes[btn] = betType;
            btn.Click += OnBetButtonClick;
            btn.Paint += OnBetButtonPaint;
        }

        private void SelectChip(int value, PictureBox pb)
        {
            selectedChip = value;
            HighlightSelectedChip(pb);
        }

        private void HighlightSelectedChip(PictureBox activePb)
        {
            PictureBox[] allChips = { pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6 };
            foreach (var pb in allChips)
            {
                if (pb != null) pb.BorderStyle = BorderStyle.None;
            }
            if (activePb != null)
            {
                activePb.BorderStyle = BorderStyle.Fixed3D;
            }
        }

        private void OnBetButtonClick(object sender, EventArgs e)
        {
            if (isSpinning) return;

            Button btn = sender as Button;
            if (btn == null) return;

            if (balance < selectedChip)
            {
                MessageBox.Show("Не вистачає коштів на балансі!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            balance -= selectedChip;

            if (placedBets.ContainsKey(btn))
                placedBets[btn] += selectedChip;
            else
                placedBets[btn] = selectedChip;

            UpdateBalanceUI();
            btn.Invalidate();
        }

        private void OnBetButtonPaint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || !placedBets.ContainsKey(btn)) return;

            int betSum = placedBets[btn];
            Color chipColor = Color.Green;
            if (betSum >= 500) chipColor = Color.Black;
            else if (betSum >= 100) chipColor = Color.Gold;
            else if (betSum >= 50) chipColor = Color.Crimson;
            else if (betSum >= 25) chipColor = Color.Purple;

            int size = 16;
            int x = btn.Width - size - 2;
            int y = btn.Height - size - 2;

            using (Brush brush = new SolidBrush(chipColor))
            using (Pen borderPen = new Pen(Color.White, 1.5f))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, x, y, size, size);
                e.Graphics.DrawEllipse(borderPen, x, y, size, size);
            }
        }

        private void OnWheelClick(object sender, EventArgs e)
        {
            if (isSpinning) return;

            if (placedBets.Count == 0)
            {
                MessageBox.Show("Спочатку зробіть хоча б одну ставку на столі!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Random rnd = new Random();
            int winningIndex = rnd.Next(0, wheelNumbers.Length);
            targetWinningNumber = wheelNumbers[winningIndex];

            // Розрахунок кута: сектор має зупинитися чітко під верхньою стрілкою (12 година)
            float sectorAngle = 360f / 37f;
            float baseTargetAngle = 360f - (winningIndex * sectorAngle);

            startAngle = currentAngle % 360f;
            float deltaAngle = baseTargetAngle - startAngle;
            if (deltaAngle < 0) deltaAngle += 360f;

            totalRotationAngle = (360f * 7) + deltaAngle;

            isSpinning = true;
            spinStopwatch.Restart();
            Application.Idle += GameLoop_Render;
        }

        private void GameLoop_Render(object sender, EventArgs e)
        {
            if (!isSpinning) return;

            double elapsedMs = spinStopwatch.Elapsed.TotalMilliseconds;
            float progress = (float)(elapsedMs / spinDurationMs);

            if (progress >= 1.0f)
            {
                Application.Idle -= GameLoop_Render;
                spinStopwatch.Stop();
                isSpinning = false;

                currentAngle = (startAngle + totalRotationAngle) % 360f;
                pictureBox1.Invalidate();
                pictureBox1.Update();

                // Додаємо випавше число в історію
                AddNumberToHistory(targetWinningNumber);

                int wonAmount = CalculateSpinPayout(targetWinningNumber);
                string colorName = targetWinningNumber == 0 ? "Зелене (Зеро)" : (redNumbers.Contains(targetWinningNumber) ? "Червоне" : "Чорне");
                MessageBox.Show($"Випало число: {targetWinningNumber} ({colorName})!\nВаш виграш: {wonAmount} $", "Результат раунду");
                return;
            }

            // Quintic Ease-Out
            float p = 1f - progress;
            float easeOutProgress = 1f - (p * p * p * p * p);

            currentAngle = startAngle + (totalRotationAngle * easeOutProgress);
            pictureBox1.Invalidate();
            pictureBox1.Update();
        }

        // Рендер колеса + покажчика сектора зверху
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (wheelImage == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float cx = pictureBox1.Width / 2f;
            float cy = pictureBox1.Height / 2f;

            // 1. Малюємо обертове колесо
            g.TranslateTransform(cx, cy);
            g.RotateTransform(currentAngle);

            int w = pictureBox1.Width;
            int h = pictureBox1.Height;
            g.DrawImage(wheelImage, -w / 2f, -h / 2f, w, h);

            g.ResetTransform();

            // 2. Статичний трикутний покажчик (стрілочка) на 12 годині зверху
            PointF[] pointer = new PointF[]
            {
                new PointF(cx, 18),        // Нижня гостра вершина, що дивиться на сектор
                new PointF(cx - 9, 3),     // Лівий верхній кут
                new PointF(cx + 9, 3)      // Правий верхній кут
            };

            using (SolidBrush pointerBrush = new SolidBrush(Color.Gold))
            using (Pen borderPen = new Pen(Color.FromArgb(20, 20, 20), 1.5f))
            {
                g.FillPolygon(pointerBrush, pointer);
                g.DrawPolygon(borderPen, pointer);
            }
        }

        private void UpdateBalanceUI()
        {
            if (label1 != null)
                label1.Text = $"Баланс: {balance} $";
        }

        public int CalculateSpinPayout(int winningNumber)
        {
            int totalPayout = 0;
            bool isRed = redNumbers.Contains(winningNumber);
            bool isBlack = winningNumber != 0 && !isRed;
            bool isEven = winningNumber != 0 && (winningNumber % 2 == 0);
            bool isOdd = winningNumber != 0 && (winningNumber % 2 != 0);

            foreach (var bet in placedBets)
            {
                Button btn = bet.Key;
                int amount = bet.Value;
                string type = betTypes[btn];

                if (int.TryParse(type, out int num))
                {
                    if (num == winningNumber) totalPayout += amount * 36;
                }
                else if (type == "RED" && isRed) totalPayout += amount * 2;
                else if (type == "BLACK" && isBlack) totalPayout += amount * 2;
                else if (type == "EVEN" && isEven) totalPayout += amount * 2;
                else if (type == "ODD" && isOdd) totalPayout += amount * 2;
                else if (type == "1to18" && winningNumber >= 1 && winningNumber <= 18) totalPayout += amount * 2;
                else if (type == "19to36" && winningNumber >= 19 && winningNumber <= 36) totalPayout += amount * 2;
                else if (type == "1st12" && winningNumber >= 1 && winningNumber <= 12) totalPayout += amount * 3;
                else if (type == "2st12" && winningNumber >= 13 && winningNumber <= 24) totalPayout += amount * 3;
                else if (type == "3st12" && winningNumber >= 25 && winningNumber <= 36) totalPayout += amount * 3;
                else if (type == "COL_1" && winningNumber != 0 && winningNumber % 3 == 1) totalPayout += amount * 3;
                else if (type == "COL_2" && winningNumber != 0 && winningNumber % 3 == 2) totalPayout += amount * 3;
                else if (type == "COL_3" && winningNumber != 0 && winningNumber % 3 == 0) totalPayout += amount * 3;
            }

            balance += totalPayout;
            ResetBetsAfterSpin();
            UpdateBalanceUI();

            return totalPayout;
        }

        private void ResetBetsAfterSpin()
        {
            foreach (var btn in placedBets.Keys)
            {
                btn.Invalidate();
            }
            placedBets.Clear();
        }
    }
}
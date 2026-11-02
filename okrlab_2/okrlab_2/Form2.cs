using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace okrlab_2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 gameForm = new Form1();

            // Показуємо рулетку
            gameForm.Show();

            // Якщо закрити рулетку — повертаємося назад у меню Form2
            gameForm.FormClosed += (s, args) => this.Show();

            // Ховаємо поточне меню
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form4 rulesForm = new Form4();
            rulesForm.ShowDialog(); // Відкриває поверх, блокуючи попереднє вікно
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace okrlab_2
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            label2.Text =
    "Європейська рулетка має 37 секторів: числа від 1 до 36 та один зелений сектор 0. " +
    "Числа від 1 до 36 поділяються на червоні та чорні. Перед початком гри гравець " +
    "обирає суму ставки та ставить фішку на потрібне поле. Після цього запускається " +
    "колесо, а кулька зупиняється на одному з секторів. Якщо ставка гравця збігається " +
    "з результатом, він отримує виграш відповідно до типу ставки.";
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

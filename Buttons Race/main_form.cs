using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Buttons_Race
{
    public delegate void HelperToCall(Button btn); // Делегат для кросспоточной работы

    public partial class main_form : Form
    {
        List<Thread> threads;

        static Random random;

        static ManualResetEvent mre;               // Событие для приостановки/возобновления работы потоков

        HelperToCall helper;

        ButtonCompare[] buttons;                   // Массив гонщиков

        public main_form()
        {
            InitializeComponent();

            helper = new HelperToCall(Motion);

            random = new Random();

            mre = new ManualResetEvent(false);

            buttons = new ButtonCompare[] { first_btn, second_btn, third_btn };
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            IsEnabled(false, true, false);
            if (threads != null)
            {
                mre.Set();
                return;
            }
            threads = new List<Thread>();                                       // Создание потоков для кнопок
            for (int i = 0; i < 3; i++)
            {
                threads.Add(new Thread(new ParameterizedThreadStart(Movement)));
                threads[i].IsBackground = true;
                threads[i].Start(buttons[i]);
            }

            mre.Set();                                                          // Установка сигнала (фактическое начало выполнения потоков)
        }

        private void pause_btn_Click(object sender, EventArgs e)
        {
            IsEnabled(true, false, true);
            mre.Reset();
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            pause_btn_Click(sender, e);
            ToStart();
        }

        private void Movement(object obj)                                       // Отвечает за работу потоков
        {
            while (true)
            {
                mre.WaitOne();                                       // Потоки ожидают установки сигнала Set() для начала выполнения или Reset() для приостановки
                Thread.Sleep(random.Next(5, 50));
                Invoke(helper, (obj as Button));
            }
        }

        private void Motion(Button button)                                      // Изминение координат и проверка позиции 
        {
            button.Location = new Point(button.Location.X + random.Next(3), button.Location.Y);
            Leader();
            Finish(button);
        }

        private void Leader()                                                   // Определение лидера в процессе гонки
        {
            Array.Sort(buttons);
            buttons[0].BackColor = Color.Yellow;
            for (int i = 1; i < buttons.Length; i++)
                buttons[i].BackColor = SystemColors.Control;
        }

        private void Finish(Button button)                                      // Определение победителя
        {
            if ((button.Location.X + button.Width) >= pictureBox1.Location.X)
            {
                pause_btn_Click(new object(), new EventArgs());
                DialogResult result = MessageBox.Show(button.Text + " win!", "Gratz!", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                    ToStart();
            }
        }

        private void ToStart()                                                  // Возврат кнопок в начальное положение
        {
            first_btn.Location = new Point(21, 45);
            second_btn.Location = new Point(21, 100);
            third_btn.Location = new Point(21, 155);

            IsEnabled(true, false, false);

            foreach (var bc in buttons)
                bc.BackColor = SystemColors.Control;

        }

        private void IsEnabled(bool startFlag, bool pauseFlag, bool stopFlag) // Метод, контролирующий доступность нажатия кнопок в каждый момент выполнения программы
        {
            start_btn.Enabled = startFlag;
            pause_btn.Enabled = pauseFlag;
            stop_btn.Enabled = stopFlag;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarmonogramSzabla
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult res3 = MessageBox.Show(String.Format("Prawdopodobieństwo danego zdarzenia w każdej akcji: \nFalstart = {0}%\nWyjście z planszy z tyłu = {1}%\nWyjście z planszy z boku = {2}%\nPrzekrok = {3}%\nPrzerwa medyczna = {4}%\nPo przerwie meycznej:\n\tKontynuuacja walki = {5}%\n\tPoddanie walki = {6}%", 6, 2.5, 1.5, 4.5, 4, 90, 10), "Szanse", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }
        

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            var poulesForm = new Poules();
            poulesForm.Show();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    
}

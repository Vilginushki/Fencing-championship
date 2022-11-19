using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace HarmonogramSzabla
{
    public partial class Poules : Form
    {
        private List<Athletes> _fencers;
        private List<List<Athletes>> _group = new List<List<Athletes>>();
        private List<DataGridView> _GroupView = new List<DataGridView>();
        private List<Rank> _groupRank = new List<Rank>();

        public Poules()
        {
            InitializeComponent();
        }

        private void Poules_Load(object sender, EventArgs e)
        {
            _fencers = LoadJson();
            List<Athletes> inTournament = new List<Athletes>();
            String f = "Zawodnicy którzy nie wystartowali z powodu przyczyn losowych:\n";
            bool x = false;
            int PosX = 10, PosY = 10, i = 0;
            foreach (var fencer in _fencers)
            {
                if (Utility.Random(10))
                {
                    x = true;
                    f += fencer.name;
                    f += "\n";
                    continue;
                }

                inTournament.Add(fencer);
                i++;
                if (inTournament.Count == 100)
                {
                    _fencers = inTournament;
                    break;
                }
            }

            if (x)
            {
                DialogResult res3 = MessageBox.Show(f, "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //prepare groups
            for (int j = 0; j < 12; j++)
            {
                _group.Add(new List<Athletes>());
            }

            //divide to groups algorithm
            i = 0;
            for (int j = 16; j < 100; j++)
            {
                _group[i].Add(_fencers[j]);
                i++;
                if (i > 11)
                {
                    i = 0;
                }
            }

            //draw groups
            i = 0;
            foreach (var g in _group)
            {
                i++;
                Label groupN = new Label()
                {
                    AutoSize = true,
                    Text = $"Group no. {i}",
                    Location = new Point(PosX, PosY),
                };
                PosY += 25;
                var gView = new DataGridView()
                {
                    Location = new Point(PosX, PosY),
                    RowHeadersVisible = false,
                };
                PosY += 225;
                panel1.Controls.Add(groupN);
                gView.Columns.Add("name", "Imie i nazwisko");
                gView.Columns.Add("no", "No.");
                gView.ScrollBars = ScrollBars.None;
                gView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                gView.Columns[0].Width = 150;
                gView.Columns[1].Width = 30;
                gView.Size = new Size(505, 200); //510
                int z = 1;
                foreach (var fencer in g)
                {
                    gView.Columns.Add($"{z}", $"{z}");
                    gView.Columns[z + 1].Width = 30;
                    gView.Rows.Add();
                    gView.Rows[z - 1].SetValues(fencer.name, z);
                    z++;
                }

                gView.Columns.Add("s1", "V/M");
                gView.Columns.Add("s2", "HS-HR");
                gView.Columns.Add("s3", "HS");
                gView.Columns[9].Width = 35;
                gView.Columns[10].Width = 50;
                gView.Columns[11].Width = 30;

                _GroupView.Add(gView);
                panel1.Controls.Add(gView);
            }
        }


        public List<Athletes> LoadJson()
        {
            var path = Path.GetTempPath();
            var data = Properties.Resources.athletes;
            Console.WriteLine(data);
            var myDeserializedClass = JsonConvert.DeserializeObject<List<Athletes>>(data);
            return myDeserializedClass;
        }

        /*
         * fence all bouts to 5 in groups
         */
        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0, wins = 0, hs = 0, hr = 0;
            int[,] scores = new int[7, 7];
            foreach (var g in _group)
            {
                for (int j = 0; j < 7; j++)
                {
                    wins = 0;
                    hs = 0;
                    hr = 0;
                    _GroupView[i][j + 2, j].Value = "X";
                    _GroupView[i][j + 2, j].Style.BackColor = Color.Black;
                    for (int k = 6; k > j; k--)
                    {
                        Report r = Utility.fence(g[j], g[k]);
                        if (r.getWinner().Equals(g[j]))
                        {
                            _GroupView[i][k + 2, j].Value = "V";
                            _GroupView[i][j + 2, k].Value = $"{r.getScore()}";
                            scores[j, k] = 5;
                            scores[k, j] = r.getScore();
                        }
                        else
                        {
                            _GroupView[i][j + 2, k].Value = "V";
                            _GroupView[i][k + 2, j].Value = $"{r.getScore()}";
                            scores[k, j] = 5;
                            scores[j, k] = r.getScore();
                        }
                    }

                    for (int k = 0; k < 7; k++)
                    {
                        if (scores[j, k] == 5)
                        {
                            wins++;
                        }

                        hs += scores[j, k];
                        hr += scores[k, j];
                    }

                    _groupRank.Add(new Rank());
                    _groupRank.Last().a = g[j];
                    _groupRank.Last().hr = hr;
                    _groupRank.Last().hs = hs;
                    _groupRank.Last().ratio = Math.Round((float)wins / 6.0, 2);
                    _GroupView[i][9, j].Value = $"{Math.Round((float)wins / 6.0, 2)}";
                    _GroupView[i][10, j].Value = $"{hs - hr}";
                    _GroupView[i][11, j].Value = $"{hs}";
                }

                i++;
            }

            _groupRank.Sort(new CompRank());
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            button3.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string s = "";
            int i = 1;
            foreach (var r in _groupRank)
            {
                s +=
                    $"{Utility.FixedLength(i.ToString() + ".", 4)} {Utility.FixedLength(r.a.name, 34)}\t{r.a.country}\t{((i > 48) ? "Unqualified" : "Qualified")}{((i % 2 == 1) ? "  \t\t" : "\n")}";
                i++;
            }

            DialogResult res = MessageBox.Show($"{s}", "Ranking", MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            List<Athletes> temp = new List<Athletes>(), eliminated = new List<Athletes>();
            for (int i = 0; i < 17; i++)
            {
                temp.Add(_fencers[i]);
            }

            foreach (var pos in _groupRank)
            {
                if (temp.Count >= 64)
                {
                    eliminated.Add(pos.a);
                }
                else
                {
                    temp.Add(pos.a);
                }
            }

            this.Hide();
            var tableauForm = new Tableau(temp, eliminated);
            tableauForm.Show();
            this.Close();
        }
    }


    public class Athletes
    {
        public int rank; //Rank based on FIE ranking on day 23.04.2022
        public String name; //Full name
        public String country; //capital 3 letters (f.e. korea = "KOR")
        public float total; //Total amount of points based on FIE ranking on day 23.04.2022
        public int seed { get; set; }
    }

    public class Rank
    {
        public Athletes a;
        public int hs;
        public int hr;
        public double ratio;
    }

    public class CompRank : IComparer<Rank>
    {
        public CompRank()
        {
        }

        public int Compare(Rank x, Rank y)
        {
            Rank r1 = x;
            Rank r2 = y;
            if (r1.ratio < r2.ratio)
            {
                return 1;
            }
            else if (r1.ratio > r2.ratio)
            {
                return -1;
            }

            if ((r1.hs - r1.hr) < (r2.hs - r2.hr))
            {
                return 1;
            }
            else if ((r1.hs - r1.hr) > (r2.hs - r2.hr))
            {
                return -1;
            }

            if (r1.hs < r2.hs)
            {
                return 1;
            }
            else if (r1.hs > r2.hs)
            {
                return -1;
            }

            if (Utility.Random(5000))
            {
                return 1;
            }

            return -1;
        }
    }

    public class Utility
    {
        /*
         * int chance - 1 is 0,01%
         */
        public static bool Random(int chance)
        {
            Random gen = new Random();
            return gen.Next(10000) <= chance ? true : false;
        }

        public static Report fence(Athletes a1, Athletes a2, int score = 5)
        {
            int score1 = 0, score2 = 0, c, diff = Math.Abs(a1.rank - a2.rank), i = 1;
            bool y1 = false, r1 = false, disq1 = false;
            bool y2 = false, r2 = false, disq2 = false;
            Report rep = new Report();
            Random gen = new Random();
            while (score1 < score && score2 < score)
            {
                rep.text += $"{i} - {score1}:{score2} - ";
                i++;
                c = gen.Next(1000);
                if (c < 60) //failstart
                {
                    if (Utility.Random(5000))

                    {
                        if (y1)
                        {
                            rep.text += $" {a1.name} czerwona kartka, falstart";
                            if (r1)
                            {
                                score2++;
                            }
                            else
                            {
                                score2++;
                                r1 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a1.name} żółta kartka, falstart";
                            y1 = true;
                        }
                    }
                    else
                    {
                        if (y2)
                        {
                            rep.text += $" {a2.name} czerwona kartka, falstart";
                            if (r2)
                            {
                                score1++;
                            }
                            else
                            {
                                score1++;
                                r2 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a2.name} żółta kartka, falstart";
                            y2 = true;
                        }
                    }
                }
                else if (c < 85) //exit on back
                {
                    if (Utility.Random(5000))
                    {
                        if (y1)
                        {
                            rep.text += $" {a1.name} czerwona kartka, wyjście z tyłu planszy";
                            if (r1)
                            {
                                score2++;
                            }
                            else
                            {
                                score2++;
                                r1 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a1.name} żółta kartka, wyjście z tyłu planszy";
                            y1 = true;
                        }
                    }
                    else
                    {
                        if (y2)
                        {
                            rep.text += $" {a2.name} czerwona kartka, wyjście z tyłu planszy";
                            if (r2)
                            {
                                score1++;
                            }
                            else
                            {
                                score1++;
                                r2 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a2.name} żółta kartka, wyjście z tyłu planszy";
                            y2 = true;
                        }
                    }
                }
                else if (c < 100) //exit on side
                {
                    if (Utility.Random(5000))
                    {
                        if (y1)
                        {
                            rep.text += $" {a1.name} czerwona kartka, wyjście z boku planszy";
                            if (r1)
                            {
                                score2++;
                            }
                            else
                            {
                                score2++;
                                r1 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a1.name} żółta kartka, wyjście z boku planszy";
                            y1 = true;
                        }
                    }
                    else
                    {
                        if (y2)
                        {
                            rep.text += $" {a2.name} czerwona kartka, wyjście z boku planszy";
                            if (r2)
                            {
                                score1++;
                            }
                            else
                            {
                                score1++;
                                r2 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a2.name} żółta kartka, wyjście z boku planszy";
                            y2 = true;
                        }
                    }
                }
                else if (c < 145) //overstepping
                {
                    if (Utility.Random(5000))
                    {
                        if (y1)
                        {
                            rep.text += $" {a1.name} czerwona kartka, przekrok";
                            if (r1)
                            {
                                score2++;
                            }
                            else
                            {
                                score2++;
                                r1 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a1.name} żółta kartka, przekrok";
                            y1 = true;
                        }
                    }
                    else
                    {
                        if (y2)
                        {
                            rep.text += $" {a2.name} czerwona kartka, przekrok";
                            if (r2)
                            {
                                score1++;
                            }
                            else
                            {
                                score1++;
                                r2 = true;
                            }
                        }
                        else
                        {
                            rep.text += $" {a2.name} żółta kartka, przekrok";
                            y2 = true;
                        }
                    }
                }
                else if (c < 165) //medical pause
                {
                    if (Utility.Random(5000))
                    {
                        rep.text += $" PRZERWA MEDYCZNA zaw 1";
                    }
                    else
                    {
                        rep.text += $" PRZERWA MEDYCZNA zaw 2";
                    }
                }
                else //no incidents
                {
                    c = gen.Next(1000);
                    if (c < 250) //no points
                    {
                        rep.text += "Trafienia jednoczesne, bez zmian wyniku";
                    }
                    else if (c < 850)
                    {
                        if (a1.rank <= a2.rank)
                        {
                            if (Utility.Random(5000 + (diff * 45)))
                            {
                                rep.text += $" {a1.name} zdobywa trafienie w natarciu";
                                score1++;
                            }
                            else
                            {
                                rep.text += $" {a2.name} zdobywa trafienie w natarciu";
                                score2++;
                            }
                        }
                        else
                        {
                            if (Utility.Random(5000 + (diff * 45)))
                            {
                                rep.text += $" {a2.name} zdobywa trafienie w natarciu";
                                score2++;
                            }
                            else
                            {
                                rep.text += $" {a1.name} zdobywa trafienie w natarciu";
                                score1++;
                            }
                        }
                    }
                    else
                    {
                        if (a1.rank <= a2.rank)
                        {
                            if (Utility.Random(5000 + (diff * 45)))
                            {
                                rep.text += $" {a1.name} zdobywa trafienie w obronie";
                                score1++;
                            }
                            else
                            {
                                rep.text += $" {a2.name} zdobywa trafienie w obronie";
                                score2++;
                            }
                        }
                        else
                        {
                            if (Utility.Random(5000 + (diff * 45)))
                            {
                                rep.text += $" {a2.name} zdobywa trafienie w obronie";
                                score2++;
                            }
                            else
                            {
                                rep.text += $" {a1.name} zdobywa trafienie w obronie";
                                score1++;
                            }
                        }
                    }
                }

                rep.text += "\n";
            }

            rep.text +=
                $"{i + 1} - {score1}:{score2} -  Koniec walki, wygrywa {((score1 == score) ? a1.name : a2.name)}";
            rep.setWinner((score1 == score) ? a1 : a2);
            rep.setLoser((score1 == score) ? a2 : a1);
            rep.setScore((score1 == score) ? score2 : score1);
            return rep;
        }

        public static string FixedLength(string input, int length)
        {
            if (input.Length > length)
                return input.Substring(0, length);
            else
                return input.PadRight(length, ' ');
        }
    }

    public class Report
    {
        private int s;
        public String text;
        private Athletes winner, loser;

        public Report()
        {
            s = 0;
            text = "";
            winner = new Athletes();
            loser = new Athletes();
        }

        public Athletes getWinner()
        {
            return winner;
        }

        public void setWinner(Athletes a)
        {
            winner = a;
        }

        public Athletes getLoser()
        {
            return loser;
        }

        public void setLoser(Athletes a)
        {
            loser = a;
        }

        public int getScore()
        {
            return s;
        }

        public void setScore(int s)
        {
            this.s = s;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace HarmonogramSzabla
{
    public partial class Tableau : Form
    {
        private List<Athletes> _fencers, _rankList, _eliminated;
        private List<int> _order = new List<int>();
        private List<Label> _l = new List<Label>();
        private List<Button> _b = new List<Button>();
        private List<Report> _r = new List<Report>();
        private int round = 1;
        List<Line> _lines = new List<Line>();

        public Tableau()
        {
            _rankList = new List<Athletes>();
            _eliminated = new List<Athletes>();
            _fencers = new List<Athletes>();//Just in case, that should never be called
            InitializeComponent();
        }
        public Tableau(List<Athletes> athletes, List<Athletes> el)
        {
            _rankList = new List<Athletes>();
            _eliminated = el;
            _fencers = athletes;
            InitializeComponent();
        }

        private void Tableau_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(Tableau_Closing);
            //First round brackets
            var lim = (int)(Math.Log(64, 2) + 1);
            Branch(1, 1, lim);
            int x = 10, y = 10, j = 1;
            _l.Add(new Label()
            {
                Text = "64",
                Location = new Point(x, y),
                Size = new Size(150, 50),
                BackColor = Color.Transparent,
            });
            panel2.Controls.Add(_l[0]);
            y += 50;
            foreach (var i in _order)
            {
                _l.Add(new Label()
                {
                    Text = $@"{i}. {_fencers[i - 1].name}",
                    Location = new Point(x, y),
                    Size = new Size(200, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[j]);
                //Drawing lines of bracket
                _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 25), new Point(x + 150, y + 25)));
                panel2.Invalidate();
                j++;
                y += 50;
            }
        }

        public void Branch(int seed, int level, int lim)
        {
            var levelSum = (int)Math.Pow(2, level) + 1;
            if (lim == level + 1)
            {
                _order.Add(seed);
                _order.Add(levelSum - seed);
                return;
            }
            else if (seed % 2 == 1)
            {
                Branch(seed, level + 1, lim);
                Branch(levelSum - seed, level + 1, lim);
            }
            else
            {
                Branch(levelSum - seed, level + 1, lim);
                Branch(seed, level + 1, lim);
            }
        }

        private void clearPanel()
        {
            foreach (var l in _l)
            {
                panel2.Controls.Remove(l);
            }
            _l.Clear();
            _order.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int vs= panel2.VerticalScroll.Value;
            int hs = panel2.HorizontalScroll.Value;
            panel2.VerticalScroll.Value = 0;
            panel2.HorizontalScroll.Value = 0;
            round++;
            int x = 160, y = 85;
            List<int> newOrder = new List<int>();
            if (round == 2)
            {
                _l.Add(new Label()
                {
                    Text = "32",
                    Location = new Point(x+50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);

                List<Athletes> temp = new List<Athletes>(new Athletes[32]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + (i * 100)), new Point(x + 50, y + 25 + ((i * 100)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 50 + (i * 100)), new Point(x + 50, y + 25 + ((i * 100)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 25 + (i * 100)), new Point(x + 275, y + 25 + ((i * 100)))));
                }

                for (int i = 0; i < (int)Math.Pow(2, 7 - round + 1); i += 2)
                {
                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    temp[(Math.Max(_order[i], _order[i + 1]) - 33)] = r.getLoser();
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed = Math.Min(_order[i], _order[i + 1]);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed = Math.Min(_order[i], _order[i + 1]);
                        newOrder.Add(_order[i+1]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i+1] - 1].seed}. {_fencers[_order[i+1] - 1].name} wynik: {r.getScore()}-15",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100,25),
                        Location = new Point(x+75, y-25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    panel2.Controls.Add(b);
                    _r.Add(r);
                    y += 100;
                }
                _rankList.AddRange(temp);

                _order = newOrder;

            }
            else if (round == 3)
            {
                x += 275;
                y += 25; 
                _l.Add(new Label()
                {
                    Text = "16",
                    Location = new Point(x + 50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);
                List<Athletes> temp = new List<Athletes>(new Athletes[16]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + ((i * 200))), new Point(x + 50, y + 50 + ((i * 200)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 100 + ((i * 200))), new Point(x + 50, y + 50 + ((i * 200)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 50 + (i * 200)), new Point(x + 275, y + 50 + ((i * 200)))));
                }

                y += 25;
                for (int i = 0; i < _order.Count; i += 2)
                {
                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    temp[Math.Abs(Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed) - 16)] = r.getLoser();
                    
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed = Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed = Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i + 1]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i + 1] - 1].seed}. {_fencers[_order[i + 1] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100, 25),
                        Location = new Point(x + 75, y - 25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    _r.Add(r);
                    panel2.Controls.Add(b);
                    y += 200;
                }
                _rankList.InsertRange(0,temp);
                _order = newOrder;
            }
            else if (round == 4)
            {
                List<Athletes> temp = new List<Athletes>(new Athletes[8]);
                x += 550;
                y += 75;
                _l.Add(new Label()
                {
                    Text = "8",
                    Location = new Point(x + 50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + ((i * 400))), new Point(x + 50, y + 100 + ((i * 400)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 200 + ((i * 400))), new Point(x + 50, y + 100 + ((i * 400)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 100 + (i * 400)), new Point(x + 275, y + 100 + ((i * 400)))));
                }

                y += 75;
                for (int i = 0; i < _order.Count; i += 2)
                {

                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    temp[Math.Abs(Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed) - 8)] = r.getLoser();
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed = Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed = Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i + 1]);
                        _l.Add(new Label()
                        {
                            Text = $@"{_fencers[_order[i + 1] - 1].seed}. {_fencers[_order[i + 1] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100, 25),
                        Location = new Point(x + 75, y - 25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    panel2.Controls.Add(b);
                    _r.Add(r);
                    y += 400;
                }
                _rankList.InsertRange(0, temp);
                _order = newOrder;
            }
            else if (round == 5)
            {
                List<Athletes> temp = new List<Athletes>(new Athletes[8]);
                x += 825;
                y += 175;
                _l.Add(new Label()
                {
                    Text = "Ćwierćfinał",
                    Location = new Point(x + 50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + ((i * 800))),
                        new Point(x + 50, y + 200 + ((i * 800)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 400 + ((i * 800))),
                        new Point(x + 50, y + 200 + ((i * 800)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 200 + (i * 800)),
                        new Point(x + 275, y + 200 + ((i * 800)))));
                }

                y += 175;
                for (int i = 0; i < _order.Count; i += 2)
                {

                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    temp[Math.Abs(Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed) - 4)] = r.getLoser();
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i + 1]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i + 1] - 1].seed}. {_fencers[_order[i + 1] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100, 25),
                        Location = new Point(x + 75, y - 25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    panel2.Controls.Add(b);
                    _r.Add(r);
                    y += 800;
                }
                _rankList.InsertRange(0, temp);
                _order = newOrder;
            }
            else if (round == 6)
            {
                List<Athletes> temp = new List<Athletes>(new Athletes[4]);
                x += 1100;
                y += 375;
                _l.Add(new Label()
                {
                    Text = "Półfinał",
                    Location = new Point(x + 50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + ((i * 1600))),
                        new Point(x + 50, y + 400 + ((i * 1600)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 800 + ((i * 1600))),
                        new Point(x + 50, y + 400 + ((i * 1600)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 400 + (i * 1600)),
                        new Point(x + 275, y + 400 + ((i * 1600)))));
                }

                y += 375;
                for (int i = 0; i < _order.Count; i += 2)
                {

                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    temp[Math.Abs(Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed) - 1)] = r.getLoser();
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i + 1]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i + 1] - 1].seed}. {_fencers[_order[i + 1] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100, 25),
                        Location = new Point(x + 75, y - 25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    panel2.Controls.Add(b);
                    _r.Add(r);
                    y += 1600;
                }
                _rankList.InsertRange(0, temp);
                _order = newOrder;
            }
            else if (round == 7)
            {
                List<Athletes> temp = new List<Athletes>(new Athletes[2]);
                x += 1375;
                y += 775;
                _l.Add(new Label()
                {
                    Text = "Finał",
                    Location = new Point(x + 50, 10),
                    Size = new Size(150, 50),
                    BackColor = Color.Transparent,
                });
                panel2.Controls.Add(_l[^1]);
                for (int i = 0; i < (int)Math.Pow(2, 7 - round); i++)
                {
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + ((i * 3200))),
                        new Point(x + 50, y + 800 + ((i * 3200)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x, y + 1600 + ((i * 3200))),
                        new Point(x + 50, y + 800 + ((i * 3200)))));
                    _lines.Add(new Line(Color.Black, 2f, new Point(x + 50, y + 800 + (i * 3200)),
                        new Point(x + 275, y + 800 + ((i * 3200)))));
                }

                y += 775;
                for (int i = 0; i < _order.Count; i += 2)
                {

                    Report r = Utility.fence(_fencers[_order[i] - 1], _fencers[_order[i + 1] - 1], 15);
                    _rankList.Insert(0,r.getLoser());
                    _rankList.Insert(0, r.getWinner());
                    if (r.getWinner() == _fencers[_order[i] - 1])
                    {
                        _fencers[_order[i] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i] - 1].seed}. {_fencers[_order[i] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else if (r.getWinner() == _fencers[_order[i + 1] - 1])
                    {
                        _fencers[_order[i + 1] - 1].seed =
                            Math.Min(_fencers[_order[i] - 1].seed, _fencers[_order[i + 1] - 1].seed);
                        newOrder.Add(_order[i + 1]);
                        _l.Add(new Label()
                        {
                            Text =
                                $@"{_fencers[_order[i + 1] - 1].seed}. {_fencers[_order[i + 1] - 1].name} wynik: 15-{r.getScore()}",
                            Location = new Point(x + 50, y),
                            Size = new Size(250, 50),
                            BackColor = Color.Transparent,
                        });
                        panel2.Controls.Add(_l[^1]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Button b = new Button()
                    {
                        Name = $"{_r.Count}",
                        Text = "Szczegóły",
                        Size = new Size(100, 25),
                        Location = new Point(x + 75, y - 25)
                    };
                    b.Click += new System.EventHandler(this.showReport);
                    _b.Add(b);
                    panel2.Controls.Add(b);
                    _r.Add(r);
                    y += 3200;
                }
                
                _order = newOrder;
            }
            panel2.Invalidate();
            panel2.VerticalScroll.Value = vs;
            panel2.HorizontalScroll.Value = hs;
            //next round
            if ((int)Math.Pow(2, 7 - round) == 1)
            {
                button1.Text = "Wyniki";
                DialogResult res = MessageBox.Show($"Nowym mistrzem świata w szabli został: {_fencers[_order[0]-1].name}", "Wygrany", MessageBoxButtons.OK);
                button2.Enabled = false;

            }
        }

        private void showReport(object sender, EventArgs e)
        {
            Button b = (Button) sender;
            int i = Int32.Parse(b.Name);
            if (i >= 0 && i <= _r.Count)
            {
                DialogResult d = MessageBox.Show(_r[i].text, "Szczegóły walki", MessageBoxButtons.OK);
            }
            
        }

        private void Tableau_Closing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Point scrollOffset = panel2.AutoScrollPosition;
            e.Graphics.TranslateTransform(scrollOffset.X, scrollOffset.Y);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var line in _lines)
            {
                line.Draw(e.Graphics);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = "";
            int i = 65, t=1;
            for (int j = 1; j < round; j++)
            {
                i-=(int)(64.0*Math.Pow(0.5, j));
            }

            if (round == 7) i = 1;
            foreach (var r in _rankList)
            {
                if(r!=null){
                    s += $"{Utility.FixedLength(i.ToString() + ".", 4)} {Utility.FixedLength(r.name, 34)}\t{r.country}{((i % 2 == 1 && round != 1) ? "  \t\t" : "\n") }";
                    i++;
                }
            }
            foreach (var r in _eliminated)
            {
                if (r != null)
                {
                    s += $"{Utility.FixedLength(i.ToString() + ".", 4)} {Utility.FixedLength(r.name, 34)}\t{r.country}{((i % 2 == 1 && round!=1) ? "  \t\t" : "\n") }";
                    i++;
                }
            }
            DialogResult res = MessageBox.Show($"{s}", "Ranking", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        private void panel2_Scroll(object sender, ScrollEventArgs e)
        {
            foreach (var l in _lines)
            {
                l.Start = new Point(l.Start.X, l.Start.Y + panel2.AutoScrollOffset.Y);
                l.End = new Point(l.End.X, l.End.Y + panel2.AutoScrollOffset.Y);
            }
            panel2.Invalidate();
        }
    }

    class Line
    {
        public Color LineColor { get; set; }
        public float Linewidth { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }

        public Line(Color c, float w, Point s, Point e)
        { LineColor = c; Linewidth = w; Start = s; End = e; }

        public void Draw(Graphics G)
        { using (Pen pen = new Pen(LineColor, Linewidth)) G.DrawLine(pen, Start, End); }


    }
}

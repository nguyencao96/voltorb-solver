using MoreLinq.Extensions;

namespace VoltorbFlipApp
{

    public partial class Form1 : Form
    {
        const int LENGTH = 5;

        public List<Limit> RowLimit { get; set; }
        public List<Limit> ColLimit { get; set; }

        public bool EnteringNumber = false;
        public List<int> EnteringLocation;
        public int[] CurrentPointer = new int[2] { -1, -1 };
        public int?[,] InitialMat;
        public int CurrentLevel = 1;
        public bool EnableRestrictMode = true;

        public class Limit
        {
            public int Sum { get; set; }
            public int MaxZero { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            RenderNumberTable();
            RenderHelpText();
            this.restrictCheckBox.Checked = EnableRestrictMode;
            //Execute("041051081042042061070052051023");
        }

        public void RenderNumberTable()
        {
            // This adds the controls to the form (you will need to specify thier co-ordinates etc. first)
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    var textBox = new TextBox();
                    textBox.Name = $"box{i}{j}";
                    textBox.AutoSize = false;
                    textBox.Width = 185;
                    textBox.Height = 185;
                    textBox.BackColor = Color.DarkSlateGray;
                    textBox.Font = new Font(FontFamily.GenericSansSerif, 35, FontStyle.Bold);
                    textBox.ForeColor = Color.White;
                    this.numberTable.Controls.Add(textBox, i, j);
                }
            }
        }

        public void RenderHelpText()
        {
            this.codeTextBox.AutoSize = false;
            this.codeTextBox.Height = 70;
            this.codeTextBox.BackColor = Color.DarkSlateGray;
            this.codeTextBox.ForeColor = Color.White;

            this.helpText.Text = @"
Press I OR Press +: Enter Edit mode.                   Press R OR Press -: Reset value.
Press number to enter value.            Press <- -> to navigate.
Press N OR Press .: Change code.                      Press Enter: Calculate.
";
        }

        public void ComputeLimit(string input)
        {
            RowLimit = new List<Limit>();
            ColLimit = new List<Limit>();

            input.Batch(3).ForEach(x =>
            {

                if (RowLimit.Count < LENGTH)
                    RowLimit.Add(new Limit()
                    {
                        Sum = int.Parse(x.ElementAt(0).ToString() + x.ElementAt(1)),
                        MaxZero = int.Parse(x.ElementAt(2).ToString())
                    });
                else
                    ColLimit.Add(new Limit()
                    {
                        Sum = int.Parse(x.ElementAt(0).ToString() + x.ElementAt(1)),
                        MaxZero = int.Parse(x.ElementAt(2).ToString())
                    });
            });
        }

        public int?[,] InitMat()
        {
            var mat = new int?[LENGTH, LENGTH];
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    var value = this.Controls.Find($"box{i}{j}", true).First().Text;
                    if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var knownValue))
                    {
                        mat[i, j] = knownValue;
                    }
                }
            }

            return mat;
        }

        public bool CanBeBomb(int posX, int posY, int?[,] mat)
        {
            var currentBombInRow = 1;
            var currentBombInCol = 1;

            var currentSumInRow = 0.0;
            var currentSumInCol = 0.0;

            for (int i = 0; i < LENGTH; i++)
            {
                if (mat[i, posY].HasValue && mat[i, posY].Value == 0)
                    currentBombInRow++;

                if (mat[posX, i].HasValue && mat[posX, i].Value == 0)
                    currentBombInCol++;

                if (mat[i, posY].HasValue && mat[i, posY].Value > 0)
                    currentSumInRow += mat[i, posY].Value;

                if (mat[posX, i].HasValue && mat[posX, i].Value > 0)
                    currentSumInCol += mat[posX, i].Value;
            }

            // Bom exceed limit
            if (currentBombInRow > RowLimit[posY].MaxZero || currentBombInCol > ColLimit[posX].MaxZero)
                return false;

            if (posY == LENGTH - 1 && currentSumInCol != ColLimit[posX].Sum)
                return false;

            if (posX == LENGTH - 1 && currentSumInRow != RowLimit[posY].Sum)
                return false;

            if (posY == LENGTH - 1 && currentBombInCol != ColLimit[posX].MaxZero)
                return false;

            if (posX == LENGTH - 1 && currentBombInRow != RowLimit[posY].MaxZero)
                return false;

            // Current row does not fit Sum
            //if (LENGTH - currentBombInRow > RowLimit[posY].Sum)
            return true;
        }

        public bool CanBeNumber(int posX, int posY, int number, int?[,] mat)
        {
            var currentBombInRow = 0.0;
            var currentBombInCol = 0.0;

            double currentSumInRow = number;
            double currentSumInCol = number;
            for (int i = 0; i < LENGTH; i++)
            {
                if (mat[i, posY].HasValue && mat[i, posY].Value == 0)
                    currentBombInRow++;

                if (mat[posX, i].HasValue && mat[posX, i].Value == 0)
                    currentBombInCol++;

                if (mat[i, posY].HasValue && mat[i, posY].Value > 0)
                    currentSumInRow += mat[i, posY].Value;

                if (mat[posX, i].HasValue && mat[posX, i].Value > 0)
                    currentSumInCol += mat[posX, i].Value;
            }

            if (currentSumInRow > RowLimit[posY].Sum || currentSumInCol > ColLimit[posX].Sum)
                return false;

            if (posY == LENGTH - 1 && currentSumInCol != ColLimit[posX].Sum)
                return false;

            if (posX == LENGTH - 1 && currentSumInRow != RowLimit[posY].Sum)
                return false;

            if (posY == LENGTH - 1 && currentBombInCol != ColLimit[posX].MaxZero)
                return false;

            if (posX == LENGTH - 1 && currentBombInRow != RowLimit[posY].MaxZero)
                return false;

            return true;
        }


        public int GetNextCellValue(int x, int y, int baseValue, int?[,] mat)
        {
            for (int i = baseValue; i <= 3; i++)
            {
                if (i == 0) // is bomb
                {
                    if (CanBeBomb(x, y, mat))
                        return 0;
                }
                else
                {
                    if (CanBeNumber(x, y, i, mat))
                        return i;
                }
            }

            return -1;
        }

        public int?[,] Clone(int?[,] data)
        {
            var cloneMat = new int?[LENGTH, LENGTH];
            for (int i = 0; i < LENGTH; i++)
            for (int j = 0; j < LENGTH; j++)
                if (data[i, j].HasValue)
                    cloneMat[i, j] = data[i, j];
            return cloneMat;
        }

        public int?[,] TryComputeMat(int?[,] data, int baseValue, List<int?[,]> result)
        {
            var mat = Clone(data);
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    if (mat[i, j].HasValue)
                    {
                        // skip
                    }
                    else
                    {
                        var nextVal = GetNextCellValue(i, j, baseValue, data);
                        if (nextVal < 0)
                        {
                            // return null
                            return null;
                        }
                        else
                        {
                            mat[i, j] = nextVal;
                            var nextMat = TryComputeMat(mat, 0, result);
                            if (nextMat == null) // We don't have possible steps. This value is false ?
                            {
                                if (nextVal < 3)
                                {
                                    nextMat = TryComputeMat(data, nextVal + 1, result);
                                    if (nextMat == null)
                                        return null;
                                    else
                                        return TryComputeMat(data, nextVal + 1, result);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return TryComputeMat(data, nextVal + 1, result);
                            }
                        }
                    }

                    //Console.WriteLine(Mat[i, j].Value);
                }
            }

            if (mat != null)
                result.Add(Clone(mat));
            return mat;
        }

        public CellResult[,] AggregateResult(IEnumerable<int?[,]> data)
        {
            var set = new HashSet<int?[,]>(data, new SameHash());
            //var set = data;
            var result = new CellResult[LENGTH, LENGTH];
            var total = set.Count();
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    var totalZero = 0;
                    var totalNumber = 0;
                    set.ForEach(x =>
                    {
                        if (x[i, j].Value == 0)
                            totalZero++;
                        else if (x[i, j].Value > 1)
                            totalNumber++;
                    });
                    result[i, j] = new CellResult()
                    {
                        PercentZero = Math.Round(100 * ((double)totalZero / total), 1),
                        PercentNumber = Math.Round(100 * ((double)totalNumber / total), 1)
                    };
                }
            }

            return result;
        }

        public class SameHash : EqualityComparer<int?[,]>
        {
            public override bool Equals(int?[,] p1, int?[,] p2)
            {
                for (int i = 0; i < LENGTH; i++)
                {
                    for (int j = 0; j < LENGTH; j++)
                    {
                        if (p1[i, j].Value != p2[i, j].Value)
                            return false;

                    }
                }

                return true;
            }

            public override int GetHashCode(int?[,] Data)
            {
                int seed = 13;
                var sum = 0.0;
                for (int i = 0; i < LENGTH; i++)
                {
                    for (int j = 0; j < LENGTH; j++)
                    {
                        sum += (i + 1) * seed + Math.Pow(j + 1, 2) * 31 + (Data[i, j].Value + 17) * 23;

                    }
                }

                return (int)Math.Ceiling(sum);
                //return base.GetHashCode();
                //return i[0] * 10000 + i[1];
                //Notice that this is a very basic implementation of a HashCode function
            }
        }

        public class CellResult
        {
            public double PercentZero { get; set; }
            public double PercentNumber { get; set; }
        }

        public void Execute(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var initialMat = InitMat();
                InitialMat = Clone(initialMat);

                //var input = "012040031040012031";
                ComputeLimit(input);
                var listResult = new List<int?[,]>();
                for (int i = 0; i < 4; i++)
                {
                    TryComputeMat(initialMat, i, listResult);

                }
                
                if (EnableRestrictMode)
                {
                    var constrains = Constraints[CurrentLevel];
                    listResult = listResult.Where(data =>constrains.Any(c => ValidateResult(data, c))).ToList();
                }
                
                this.resultLabel.Text = $"Total result: {listResult.Count}";

                var finalResult = AggregateResult(listResult);
                RenderResults(finalResult);

            }
            else
            {
                this.resultLabel.Text = "Invalid code";
            }
        }

        public bool ValidateResult(int?[,] data, Constraint constraint)
        {
            var sumBomb = 0;
            var sumX2 = 0;
            var sumX3 = 0;
            
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    if (data[i, j].Value == 0)
                        sumBomb++;
                    else if (data[i, j].Value == 2)
                        sumX2++;
                    else if (data[i, j].Value == 3)
                        sumX3++;
                }
            }

            return sumBomb == constraint.Bomb && sumX2 == constraint.X2 && sumX3 == constraint.X3;
        }

        public class MinMaxResult
        {
            public double Safe { get; set; }
            public double BestSafe { get; set; }
            public double Danger { get; set; }
            public double PotentialNumber { get; set; }
        }

        public void TryFindMinMax(CellResult[,] data, out MinMaxResult result)
        {
            //051051043051051061041033051061
            result = new MinMaxResult()
            {
                Safe = 100,
                BestSafe = 0,
                Danger = 0,
                PotentialNumber = 0
            };
            var listSafe = new List<double>();

            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    if (InitialMat[i, j].HasValue)
                        continue;
                    if (data[i, j].PercentNumber == 0)
                        continue;
                    result.Danger = Math.Max(data[i, j].PercentZero, result.Danger);
                    result.PotentialNumber = Math.Max(data[i, j].PercentNumber, result.PotentialNumber);

                    if (data[i, j].PercentZero <= result.Safe)
                    {
                        result.Safe = data[i, j].PercentZero;
                        listSafe.Add(data[i, j].PercentNumber);
                    }
                }
            }

            if (listSafe.Count > 0)
                result.BestSafe = listSafe.Max();
        }

        public void RenderResults(CellResult[,] data)
        {
            this.resultPanel.Controls.RemoveByKey("resultTable");
            var tlp = new TableLayoutPanel() { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 5 };
            tlp.Name = "resultTable";
            tlp.Location = new Point(1253, 262);
            tlp.MinimumSize = new Size(950, 950);
            tlp.Size = new Size(950, 950);
            tlp.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));

            tlp.RowStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.RowStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.RowStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.RowStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));
            tlp.RowStyles.Add(new ColumnStyle(SizeType.Percent, (float)16.67));

            TryFindMinMax(data, out var minMaxResult);
            // This adds the controls to the form (you will need to specify thier co-ordinates etc. first)
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    var panel = new Panel();
                    panel.Height = 200;
                    panel.BackColor = Color.SlateGray;
                    var label = new Label();
                    label.Name = $"resultLbl{i}{j}";
                    label.AutoSize = true;
                    label.Text = $"B: {data[i, j].PercentZero}%\nN: {data[i, j].PercentNumber}%";
                    label.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
                    label.ForeColor = Color.White;

                    if (data[i, j].PercentZero == minMaxResult.Safe)
                    {
                        if (data[i, j].PercentNumber == minMaxResult.BestSafe)
                            panel.BackColor = Color.DeepSkyBlue;
                        else
                            panel.BackColor = Color.LightSkyBlue;
                    }
                    else if (data[i, j].PercentZero == minMaxResult.Danger)
                    {
                        panel.BackColor = Color.DarkRed;
                    }
                    else if (data[i, j].PercentNumber == minMaxResult.PotentialNumber)
                    {
                        panel.BackColor = Color.Green;
                    }

                    if (data[i, j].PercentNumber == 0)
                    {
                        panel.BackColor = Color.IndianRed;
                    }

                    if (InitialMat[i, j].HasValue)
                    {
                        panel.BackColor = Color.SaddleBrown;
                        label.ForeColor = Color.DimGray;
                    }


                    panel.Controls.Add(label);
                    //label.BringToFront();

                    tlp.Controls.Add(panel, i, j);
                }
            }

            this.resultPanel.Controls.Add(tlp);
        }

        public bool LeaveEditMode(Keys keyData)
        {
            if (EnteringNumber)
            {
                if ((keyData >= Keys.D0 && keyData <= Keys.D9) ||
                    (keyData >= Keys.NumPad0 && keyData <= Keys.NumPad9) ||
                    keyData == Keys.Decimal)
                {
                    int keyVal = (int)keyData;
                    int value = -1;
                    if (keyVal >= (int)Keys.D0 && keyVal <= (int)Keys.D9)
                    {
                        value = (int)keyData - (int)Keys.D0;
                    }
                    else if (keyVal >= (int)Keys.NumPad0 && keyVal <= (int)Keys.NumPad9)
                    {
                        value = (int)keyData - (int)Keys.NumPad0;
                    }

                    EnteringLocation.Add(value);
                    // Edit mode
                }

                if (EnteringLocation.Count > 1)
                {
                    CurrentPointer[0] = Math.Min(Math.Max(EnteringLocation[0] - 1, 0), LENGTH - 1);
                    CurrentPointer[1] = Math.Min(Math.Max(EnteringLocation[1] - 1, 0), LENGTH - 1);

                    NavigateTo(CurrentPointer[0], CurrentPointer[1]);

                    EnteringNumber = false;
                    EnteringLocation = new List<int>();
                }

                return true;
            }

            return false;
        }

        public void NavigateTo(int posX, int posY)
        {
            var txt = (TextBox)this.numberTable.Controls
                .Find($"box{posX}{posY}", true)
                .FirstOrDefault();

            txt.BackColor = Color.CornflowerBlue;
            //txt.BorderStyle = BorderStyle.FixedSingle;
            txt.SelectAll();
            txt.Focus();
        }

        public bool EnterEditMode(Keys keyData)
        {
            if (keyData == Keys.I || keyData == Keys.Oemplus)
            {
                EnteringNumber = true;
                EnteringLocation = new List<int>();
                CurrentPointer[0] = -1;
                CurrentPointer[1] = -1;
                return true;
            }

            return false;
        }

        public bool IsNavigation(Keys keyData)
        {
            if (CurrentPointer[0] >= 0 || CurrentPointer[1] >= 0)
            {
                switch (keyData)
                {
                    case Keys.Right:
                        CurrentPointer[0]++;
                        break;
                    case Keys.Left:
                        CurrentPointer[0]--;
                        break;
                    case Keys.Up:
                        CurrentPointer[1]--;
                        break;
                    case Keys.Down:
                        CurrentPointer[1]++;
                        break;
                    default:
                        return false;
                        break;
                }

                CurrentPointer[0] = Math.Min(Math.Max(CurrentPointer[0], 0), LENGTH - 1);
                CurrentPointer[1] = Math.Min(Math.Max(CurrentPointer[1], 0), LENGTH - 1);

                NavigateTo(CurrentPointer[0], CurrentPointer[1]);

                return true;
            }

            return false;
        }

        public void ResetNumberTextBoxColor()
        {
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    this.Controls.Find($"box{i}{j}", true).First().BackColor = Color.DarkSlateGray;
                }
            }
        }

        public void NewGame()
        {
            NextLevel();
            resetBtn.PerformClick();
            CurrentLevel--;
            this.levelLabel.Text = CurrentLevel.ToString();
        }

        public void NextLevel()
        {
            EnteringNumber = false;
            EnteringLocation = new List<int>();
            CurrentPointer[0] = -1;
            CurrentPointer[1] = -1;

            this.codeTextBox.SelectAll();
            this.codeTextBox.Focus();

            CurrentLevel++;
            this.levelLabel.Text = CurrentLevel.ToString();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            var i = e.KeyCode;
            base.OnKeyUp(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ResetNumberTextBoxColor();

            if (LeaveEditMode(keyData))
                return true;

            if (IsNavigation(keyData))
                return true;

            if (CurrentPointer[0] >= 0 && CurrentPointer[1] >= 0)
            {
                NavigateTo(CurrentPointer[0], CurrentPointer[1]);
            }

            if (Helper.IsResetAll(keyData))
            {
                NextLevel();
                resetBtn.PerformClick();
                return true;
            }

            if (keyData == Keys.A)
            {
                EnableRestrictMode = !EnableRestrictMode;
                this.restrictCheckBox.Checked = EnableRestrictMode;
                solveBtn.PerformClick();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                NewGame();
                return true;
            }

            if (keyData == Keys.PageUp || keyData == Keys.W)
            {
                CurrentLevel++;
                this.levelLabel.Text = CurrentLevel.ToString();
                return true;
            }

            if (keyData == Keys.PageDown || keyData == Keys.S)
            {
                CurrentLevel--;
                CurrentLevel = Math.Max(CurrentLevel, 1);
                this.levelLabel.Text = CurrentLevel.ToString();
                return true;
            }
            //if (keyData == Keys.N || keyData == Keys.Decimal)
            //{
            //    NextLevel();
            //    return true;
            //}

            if (keyData == Keys.R || keyData == Keys.Subtract)
            {
                resetBtn.PerformClick();
                return true;
            }

            if (keyData == Keys.Enter)
            {
                solveBtn.PerformClick();
                return true;
            }

            if (keyData == Keys.I || keyData == Keys.Add)
            {
                EnteringNumber = true;
                EnteringLocation = new List<int>();
                return true;
            }

            if (EnterEditMode(keyData))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Execute(this.codeTextBox.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < LENGTH; i++)
            {
                for (int j = 0; j < LENGTH; j++)
                {
                    this.Controls.Find($"box{i}{j}", true).First().Text = String.Empty;
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        public Dictionary<int, List<Constraint>> Constraints = new Dictionary<int, List<Constraint>>()
        {
            [1] = new List<Constraint>()
            {
                new Constraint() { X2 = 3, X3 = 1, Bomb = 6, Coins = 24 },
                new Constraint() { X2 = 0, X3 = 3, Bomb = 6, Coins = 27 },
                new Constraint() { X2 = 5, X3 = 0, Bomb = 6, Coins = 32 },
                new Constraint() { X2 = 2, X3 = 2, Bomb = 6, Coins = 36 },
                new Constraint() { X2 = 4, X3 = 1, Bomb = 6, Coins = 48 }
            },
            [2] = new List<Constraint>()
            {
                new Constraint() { X2 = 1, X3 = 3, Bomb = 7, Coins = 54 },
                new Constraint() { X2 = 6, X3 = 0, Bomb = 7, Coins = 64 },
                new Constraint() { X2 = 3, X3 = 2, Bomb = 7, Coins = 72 },
                new Constraint() { X2 = 0, X3 = 4, Bomb = 7, Coins = 81 },
                new Constraint() { X2 = 5, X3 = 1, Bomb = 7, Coins = 96 }
            },
            [3] = new List<Constraint>()
            {
                new Constraint() { X2 = 2, X3 = 3, Bomb = 8, Coins = 108 },
                new Constraint() { X2 = 7, X3 = 0, Bomb = 8, Coins = 128 },
                new Constraint() { X2 = 4, X3 = 2, Bomb = 8, Coins = 144 },
                new Constraint() { X2 = 1, X3 = 4, Bomb = 8, Coins = 162 },
                new Constraint() { X2 = 6, X3 = 1, Bomb = 8, Coins = 192 }
            },
            [4] = new List<Constraint>()
            {
                new Constraint() { X2 = 3, X3 = 3, Bomb = 8, Coins = 216 },
                new Constraint() { X2 = 0, X3 = 5, Bomb = 8, Coins = 243 },
                new Constraint() { X2 = 8, X3 = 0, Bomb = 10, Coins = 256 },
                new Constraint() { X2 = 5, X3 = 2, Bomb = 10, Coins = 288 },
                new Constraint() { X2 = 2, X3 = 4, Bomb = 10, Coins = 324 }
            },
            [5] = new List<Constraint>()
            {
                new Constraint() { X2 = 7, X3 = 1, Bomb = 10, Coins = 384 },
                new Constraint() { X2 = 4, X3 = 3, Bomb = 10, Coins = 432 },
                new Constraint() { X2 = 1, X3 = 5, Bomb = 10, Coins = 486 },
                new Constraint() { X2 = 9, X3 = 0, Bomb = 10, Coins = 512 },
                new Constraint() { X2 = 6, X3 = 2, Bomb = 10, Coins = 576 }
            },
            [6] = new List<Constraint>()
            {
                new Constraint() { X2 = 3, X3 = 4, Bomb = 10, Coins = 648 },
                new Constraint() { X2 = 0, X3 = 6, Bomb = 10, Coins = 729 },
                new Constraint() { X2 = 8, X3 = 1, Bomb = 10, Coins = 768 },
                new Constraint() { X2 = 5, X3 = 3, Bomb = 10, Coins = 864 },
                new Constraint() { X2 = 2, X3 = 5, Bomb = 10, Coins = 972 }
            },
            [7] = new List<Constraint>()
            {
                new Constraint() { X2 = 7, X3 = 2, Bomb = 10, Coins = 1152 },
                new Constraint() { X2 = 4, X3 = 4, Bomb = 10, Coins = 1296 },
                new Constraint() { X2 = 1, X3 = 6, Bomb = 13, Coins = 1458 },
                new Constraint() { X2 = 9, X3 = 1, Bomb = 13, Coins = 1536 },
                new Constraint() { X2 = 6, X3 = 3, Bomb = 10, Coins = 1728 }
            },
            [8] = new List<Constraint>()
            {
                new Constraint() { X2 = 0, X3 = 7, Bomb = 10, Coins = 2187 },
                new Constraint() { X2 = 8, X3 = 2, Bomb = 10, Coins = 2304 },
                new Constraint() { X2 = 5, X3 = 4, Bomb = 10, Coins = 2592 },
                new Constraint() { X2 = 2, X3 = 6, Bomb = 10, Coins = 2916 },
                new Constraint() { X2 = 7, X3 = 3, Bomb = 10, Coins = 3456 }
            },
        };

        private void restrictCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EnableRestrictMode = (sender as CheckBox).Checked;
        }
    }
}
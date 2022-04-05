using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MinesWeeper;

public partial class MainPage : ContentPage
{
    static Color ColorSpace = new Color(204, 204, 204);
    static Color ColorDefault = new Color(0, 0, 0, -1);
    static Color ColorYellow = new Color(255, 207, 72);
    static Color ColorButtonDefault = new Color(81, 43, 223);
    static Color Color255 = new Color(255, 255, 255);
    int count = 9;
    int rows = 17;
    List<int> list = new List<int>();
    string stack = "";
    public MainPage()
    {
        InitializeComponent();
        StartGame();

    }

    public async void StartGame()
    {
        try
        {
            InitBack();
            InitFlag();
            sl.Clear();
            Load();
            GetWeeperCount();
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", ex.Message, "确定");
        }
    }

    public void Load()
    {
        Random rd = new Random();
        list = GenerateRandom(0, count * rows, count*rows / 10).Distinct().ToList();
    }

    public int[] GenerateRandom(int minValue, int maxValue, int randNum)
    {
        Random ran = new Random(GenerateRandomSeed());
        int[] arr = new int[randNum];

        for (int i = 0; i < randNum; i++)
        {
            arr[i] = ran.Next(minValue, maxValue);
        }
        return arr;
    }
    public static int GenerateRandomSeed()
    {
        return Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
    }


    private void GetWeeperCount()
    {
        var martrix = new int[rows][];
        for (int i = 0; i < martrix.Count(); i++)
        {
            martrix[i] = new int[count];
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < count; col++)
            {
                int l = HasWeeping(GetSort(row, col - 1)),
                    lt = HasWeeping(GetSort(row - 1, col - 1)),
                    lb = HasWeeping(GetSort(row + 1, col - 1)),
                    mt = HasWeeping(GetSort(row - 1, col)),
                    mb = HasWeeping(GetSort(row + 1, col)),
                    rt = HasWeeping(GetSort(row - 1, col + 1)),
                    r = HasWeeping(GetSort(row, col + 1)),
                    rb = HasWeeping(GetSort(row + 1, col + 1));

                //Debug.Write(GetSort(row, col) + " ");
                var self = HasWeeping(GetSort(row, col));
                int c = 0;
                if (self == 1) c = 99;
                martrix[row][col] = l + lt + lb + mt + mb + rt + r + rb + c;
            }
        }

        for (int row = 0; row < rows; row++)
        {
            var stackLayout = new StackLayout();
            stackLayout.Orientation = StackOrientation.Horizontal;
            for (int col = 0; col < count; col++)
            {
                var lb = new Button();
                //本身是雷
                if (martrix[row][col] >= 99)
                {
                    lb = new Button
                    {
                        Text = "💣",
                        BackgroundColor = Colors.DarkRed,
                        TextColor = ColorDefault,
                        HeightRequest = 40,
                        WidthRequest = 40,
                        CornerRadius = 3,
                        Opacity = 0,
                        Margin = new Thickness(1)
                    };
                    var idx = GetSort(row, col);
                    lb.Clicked += async (sender, e) =>
                    {
                        if (flag.BackgroundColor.Red == ColorYellow.Red)
                        {
                            var pos = GetXY(idx);
                            SetBackOpacity(pos.Item1, pos.Item2, 0);
                            SetFlagOpacity(pos.Item1, pos.Item2, 1);
                            lb.Opacity = 0;
                        }
                        else
                        {
                            var pos = GetXY(idx);
                            SetBackOpacity(pos.Item1, pos.Item2, 1);
                            SetFlagOpacity(pos.Item1, pos.Item2, 0);
                            lb.Opacity = 1;
                            lb.TextColor = new Color(255, 255, 255);
                            var result = await DisplayAlert("游戏结束", "点到炸弹啦", "再来一局", "退出");
                            if (result)
                            {
                                StartGame();
                                return;
                            }
                            else
                            {
                                EndGame();
                                return;
                            }

                            if (CheckWin())
                            {
                                result = await DisplayAlert("游戏结束", "你胜利了", "再来一局", "退出");
                                if (result)
                                {
                                    StartGame();
                                }
                            }
                        }
                    };
                }
                else
                {
                    lb = new Button
                    {
                        Text = martrix[row][col] == 0 ? "" : martrix[row][col] + "",
                        TextColor = ColorDefault,
                        HeightRequest = 40,
                        WidthRequest = 40,
                        CornerRadius = 3,
                        Margin = new Thickness(1)
                    };

                    var idx = GetSort(row, col);
                    lb.Clicked += async (sender, e) =>
                    {
                        if (flag.BackgroundColor.Red == ColorYellow.Red)
                        {
                            var pos = GetXY(idx);
                            SetBackOpacity(pos.Item1, pos.Item2, 0);
                            SetFlagOpacity(pos.Item1, pos.Item2, 1);
                            lb.Opacity = 0;
                        }
                        else
                        {
                            var cur = idx;
                            var pos = GetXY(idx);

                            SetBackOpacity(pos.Item1, pos.Item2, 1);
                            SetFlagOpacity(pos.Item1, pos.Item2, 0);
                            lb.Opacity = 1;

                            //不是炸弹且周围没有炸弹
                            if (string.IsNullOrEmpty(lb.Text))
                            {
                                lb.BackgroundColor = ColorSpace;
                            }
                            else
                            {
                                lb.TextColor = new Color(255, 255, 255);
                            }

                            for (int r = pos.Item1 - 1; r < pos.Item1 + 2; r++)
                            {
                                for (int c = pos.Item2 - 1; c < pos.Item2 + 2; c++)
                                {
                                    ChangeButtonColor(r, c);
                                }
                            }

                            if (CheckWin())
                            {
                                var result = await DisplayAlert("游戏结束", "你胜利了", "再来一局", "退出");
                                if (result)
                                {
                                    StartGame();
                                }
                            }
                        }

                    };
                }
                stackLayout.Add(lb);

                //martrix[row][col]
            }
            sl.Children.Add(stackLayout);
        }
    }

    private void ChangeButtonColor(int row, int col)
    {
        var pos = new List<KeyValuePair<int, int>>();
        for (var i = 0; i < sl.Children.Count; i++)
        {
            var r = sl.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                if (row == i && col == j)
                {
                    var btn = r[j] as Button;
                    if (string.IsNullOrEmpty(btn.Text))
                    {
                        btn.TextColor = ColorSpace;
                    }
                    else btn.TextColor = new Color(255, 255, 255);
                    pos.Add(new KeyValuePair<int, int>(i, j));
                }
            }
        }
        if (pos.Count == 0) return;
        foreach (var item in pos)
        {
            var items = Get9PosXY(item.Key, item.Value);
            var btn = GetButton(item.Key, item.Value);
            if (string.IsNullOrEmpty(btn.Text) && btn.TextColor.Red == ColorSpace.Red)
            {
                Change9PosButtonColor(items);
            }
        }
    }

    private Button GetButton(int row, int col)
    {
        for (var i = 0; i < sl.Children.Count; i++)
        {
            var r = sl.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                if (row == i && col == j)
                {
                    return r[j] as Button;
                }
            }
        }
        return null;
    }

    private void SetButtonColor(int row, int col, Color color)
    {
        for (var i = 0; i < sl.Children.Count; i++)
        {
            var r = sl.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                if (row == i && col == j)
                {
                    var btn = r[j] as Button;
                    btn.TextColor = color;
                }
            }
        }
    }

    private void Change9PosButtonColor(List<KeyValuePair<int, int>> items)
    {
        foreach (var item in items)
        {
            var button = GetButton(item.Key, item.Value);
            if (button != null && string.IsNullOrEmpty(button.Text) && button.TextColor.Red != ColorSpace.Red)
            {
                SetButtonColor(item.Key, item.Value, ColorSpace);
                //button.TextColor = ColorSpace;
                var nextItem = Get9PosXY(item.Key, item.Value);
                Change9PosButtonColor(nextItem);
            }
        }
    }


    private List<KeyValuePair<int, int>> Get9PosXY(int x, int y)
    {
        List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();
        result.Add(new KeyValuePair<int, int>(x-1, y-1));
        result.Add(new KeyValuePair<int, int>(x-1, y));
        result.Add(new KeyValuePair<int, int>(x-1, y+1));
        result.Add(new KeyValuePair<int, int>(x, y-1));
        result.Add(new KeyValuePair<int, int>(x, y+1));
        result.Add(new KeyValuePair<int, int>(x+1, y-1));
        result.Add(new KeyValuePair<int, int>(x+1, y));
        result.Add(new KeyValuePair<int, int>(x+1, y+1));
        return result;
    }

    private void EndGame()
    {
        for (var i = 0; i < sl.Children.Count; i++)
        {
            var r = sl.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                var btn = r[j] as Button;
                btn.TextColor = new Color(255, 255, 255);
                btn.Opacity = 1;
                if (string.IsNullOrEmpty(btn.Text))
                {
                    btn.BackgroundColor = ColorSpace;
                }
            }
        }
    }

    private bool CheckWin()
    {
        bool win = true;
        for (var i = 0; i < sl.Children.Count; i++)
        {
            var r = sl.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                var btn = r[j] as Button;
                if (btn.Opacity == 1 && btn.Text == "💣")
                {
                    win = false;
                }
                else if (string.IsNullOrEmpty(btn.Text) && btn.BackgroundColor.Red != ColorSpace.Red && btn.BackgroundColor.Green != ColorSpace.Green && btn.BackgroundColor.Blue != ColorSpace.Blue)
                {
                    win = false;
                }
                else if (!string.IsNullOrEmpty(btn.Text) && btn.Text != "💣" && btn.TextColor.Red != Color255.Red && btn.TextColor.Green != Color255.Green && btn.TextColor.Blue != Color255.Blue)
                {
                    win = false;
                }
            }
        }
        return win;
    }

    private Tuple<int, int> GetXY(int x)
    {
        var row = x / count;
        var col = x % count;
        return Tuple.Create(row, col);
    }

    private void Lb_Clicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private int GetSort(int row, int col)
    {
        if (row < 0 || row > rows - 1) row = -99;
        if (col < 0 || col > count - 1) col = -99;
        return row * count + col;
    }
    private int HasWeeping(int index)
    {
        return list.Contains(index) ? 1 : 0;
    }

    private void InitBack()
    {
        sl2.Clear();
        for (int row = 0; row < rows; row++)
        {
            var stackLayout = new StackLayout();
            stackLayout.Orientation = StackOrientation.Horizontal;
            for (int col = 0; col < count; col++)
            {
                var lb = new Button();
                lb = new Button
                {
                    Text = "",
                    TextColor = ColorDefault,
                    HeightRequest = 40,
                    WidthRequest = 40,
                    CornerRadius = 3,
                    Margin = new Thickness(1)
                };

                stackLayout.Add(lb);
            }
            sl2.Children.Add(stackLayout);
        }
    }

    private void InitFlag()
    {
        sl_flag.Clear();
        for (int row = 0; row < rows; row++)
        {
            var stackLayout = new StackLayout();
            stackLayout.Orientation = StackOrientation.Horizontal;
            for (int col = 0; col < count; col++)
            {
                var lb = new Button();
                lb = new Button
                {
                    Text = "🚩",
                    HeightRequest = 40,
                    WidthRequest = 40,
                    CornerRadius = 3,
                    Margin = new Thickness(1),
                    Opacity = 0
                };

                stackLayout.Add(lb);
            }
            sl_flag.Children.Add(stackLayout);
        }
    }


    private void SetFlagOpacity(int row, int col, int op)
    {
        for (var i = 0; i < sl_flag.Children.Count; i++)
        {
            var r = sl_flag.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                if (row == i && col == j)
                {
                    var btn = r[j] as Button;
                    btn.Opacity = btn.Opacity == 0 ? 1 : 0;
                }
            }
        }
    }

    private void SetBackOpacity(int row, int col, int op)
    {
        for (var i = 0; i < sl2.Children.Count; i++)
        {
            var r = sl2.Children[i] as StackLayout;
            for (int j = 0; j < r.Count; j++)
            {
                if (row == i && col == j)
                {
                    var btn = r[j] as Button;
                    btn.Opacity = btn.Opacity == 0 ? 1 : 0;
                }
            }
        }
    }

    private void flag_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            btn.BackgroundColor = btn.BackgroundColor.Red == ColorButtonDefault.Red ? ColorYellow : ColorButtonDefault;
        }
    }

    private async void restart_Clicked(object sender, EventArgs e)
    {
        try
        {
            rows = Convert.ToInt32(e_row.Text);
            count = Convert.ToInt32(e_col.Text);
            StartGame();
        }
        catch(Exception ex)
        {
           await DisplayAlert("错误", ex.Message, "确定");
        }
    }
}


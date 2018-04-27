using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Guepard.Block;
using Guepard.FileWork;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using WindowStartupLocation = System.Windows.WindowStartupLocation;


namespace Guepard
{
    /// <summary>
    /// Interaction logic for Weakness.xaml
    /// </summary>
    public partial class Weakness
    {
        private WorkWithFiles _files = new WorkWithFiles();
        private MainWindow _window;
        public bool OpenWindow;
        public Weakness(MainWindow window)
        {
            _window = window;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            OpenWindow = true;
        }
        public List<WeaknessType> Ks1Weakness = new List<WeaknessType>();
        public List<WeaknessType> Ks2Weakness = new List<WeaknessType>();
        public List<WeaknessType> GetWeakness = new List<WeaknessType>();
        public List<WeaknessType> CreateKs1Weakness = new List<WeaknessType>();
        public List<WeaknessType> CreateKs2Weakness = new List<WeaknessType>();
        public List<WeaknessType> CreateGetWeakness = new List<WeaknessType>();
        readonly List<string> _freqs = new List<string> { "2800-2849", "2850-2899", "2900-2949", "2950-2999",
            "3000-3049", "3050-3099", "3100-3149", "3150-3200" };
        private void ReadWeaknessButton_Click(object sender, RoutedEventArgs e)
        {
            string[,] weaknessArray;
            try
            {
                weaknessArray = _files.ReadWickeness();
                if (weaknessArray == null)
                {
                    throw new Exception();
                }


            Ks1Weakness.Clear();
            Ks2Weakness.Clear();
            GetWeakness.Clear();
            for (int i = 0; i < 4200; i++)
            {

                Ks1Weakness.Add(new WeaknessType(weaknessArray[i, 0], weaknessArray[i, 1], Convert.ToInt32(weaknessArray[i, 2]),
                    Convert.ToDouble(weaknessArray[i, 3], CultureInfo.InvariantCulture),
                    Convert.ToInt32(weaknessArray[i, 4])));
            }

            for (int i = 0; i < 4200; i++)
            {

                Ks2Weakness.Add(new WeaknessType(weaknessArray[i + 4200, 0], weaknessArray[i + 4200, 1], Convert.ToInt32(weaknessArray[i + 4200, 2]),
                    Convert.ToDouble(weaknessArray[i + 4200, 3], CultureInfo.InvariantCulture),
                    Convert.ToInt32(weaknessArray[i + 4200, 4])));
            }

            for (int i = 0; i < 2600; i++)
            {

                GetWeakness.Add(new WeaknessType(weaknessArray[i + 8400, 0], weaknessArray[i + 8400, 1], Convert.ToInt32(weaknessArray[i + 8400, 2]),
                    Convert.ToDouble(weaknessArray[i + 8400, 3], CultureInfo.InvariantCulture),
                    Convert.ToInt32(weaknessArray[i + 8400, 4])));
            }
            _newFile = false;
                UpdateList();
                SetVisability();
            }
            catch
            {
                Ks1WeaknessDg.ItemsSource = null;
                Ks1WeaknessDg.Items.Clear();
                Ks1WeaknessDg.Items.Refresh();
                Ks2WeaknessDg.ItemsSource = null;
                Ks2WeaknessDg.Items.Clear();
                Ks2WeaknessDg.Items.Refresh();
                GetWeaknessDg.ItemsSource = null;
                GetWeaknessDg.Items.Clear();
                GetWeaknessDg.Items.Refresh();
                _window.DisplayLog("Ошибка при открытии файла!");
                MessageBox.Show("Ошибка при открытии файла!");
            }
        }

        private void DG_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (dg.ItemsSource != null)
            {
                dg.Columns[0].Width = dg.ActualWidth / 5 - (dg.ActualWidth / 70);
                dg.Columns[1].Width = dg.ActualWidth / 5 - (dg.ActualWidth / 70);
                dg.Columns[2].Width = dg.ActualWidth / 5 - (dg.ActualWidth / 70);
                dg.Columns[3].Width = dg.ActualWidth / 5 - (dg.ActualWidth / 70);
            }
        }

        private void UpdateList()
        {
            switch (_visasbilityCounter)
            {
                case 0:
                    Ks1WeaknessDg.ItemsSource = null;
                    Ks1WeaknessDg.Items.Clear();
                    Ks1WeaknessDg.Items.Refresh();
                    Ks1WeaknessDg.ItemsSource = Ks1Weakness;
                    Ks1WeaknessDg.Columns[0].IsReadOnly = true;
                    Ks1WeaknessDg.Columns[1].IsReadOnly = true;
                    Ks1WeaknessDg.Columns[2].IsReadOnly = true;
                    Ks1WeaknessDg.Columns[3].IsReadOnly = true;
                    Ks1WeaknessDg.Columns[0].Header = "Тип";
                    Ks1WeaknessDg.Columns[1].Header = "Частота";
                    Ks1WeaknessDg.Columns[2].Header = "Температура";
                    Ks1WeaknessDg.Columns[3].Header = "Ослабление(Дб)";
                    Ks1WeaknessDg.Columns[4].Header = "Ослабление";
                    Ks1WeaknessDg.Columns[0].Width = Ks1WeaknessDg.ActualWidth / 5 - (Ks1WeaknessDg.ActualWidth / 70);
                    Ks1WeaknessDg.Columns[1].Width = Ks1WeaknessDg.ActualWidth / 5 - (Ks1WeaknessDg.ActualWidth / 70);
                    Ks1WeaknessDg.Columns[2].Width = Ks1WeaknessDg.ActualWidth / 5 - (Ks1WeaknessDg.ActualWidth / 70);
                    Ks1WeaknessDg.Columns[3].Width = Ks1WeaknessDg.ActualWidth / 5 - (Ks1WeaknessDg.ActualWidth / 70);
                    break;


               
                case 1:
                    Ks2WeaknessDg.ItemsSource = null;
                    Ks2WeaknessDg.Items.Clear();
                    Ks2WeaknessDg.Items.Refresh();
                    Ks2WeaknessDg.ItemsSource = Ks2Weakness;
                    Ks2WeaknessDg.Columns[0].IsReadOnly = true;
                    Ks2WeaknessDg.Columns[1].IsReadOnly = true;
                    Ks2WeaknessDg.Columns[2].IsReadOnly = true;
                    Ks2WeaknessDg.Columns[3].IsReadOnly = true;
                    Ks2WeaknessDg.Columns[0].Header = "Тип";
                    Ks2WeaknessDg.Columns[1].Header = "Частота";
                    Ks2WeaknessDg.Columns[2].Header = "Температура";
                    Ks2WeaknessDg.Columns[3].Header = "Ослабление(Дб)";
                    Ks2WeaknessDg.Columns[4].Header = "Ослабление";
                    Ks2WeaknessDg.Columns[0].Width = Ks2WeaknessDg.ActualWidth / 5 - (Ks2WeaknessDg.ActualWidth / 70);
                    Ks2WeaknessDg.Columns[1].Width = Ks2WeaknessDg.ActualWidth / 5 - (Ks2WeaknessDg.ActualWidth / 70);
                    Ks2WeaknessDg.Columns[2].Width = Ks2WeaknessDg.ActualWidth / 5 - (Ks2WeaknessDg.ActualWidth / 70);
                    Ks2WeaknessDg.Columns[3].Width = Ks2WeaknessDg.ActualWidth / 5 - (Ks2WeaknessDg.ActualWidth / 70);
                    break;
                    
                case 2:
                    GetWeaknessDg.ItemsSource = null;
                    GetWeaknessDg.Items.Clear();
                    GetWeaknessDg.Items.Refresh();
                    GetWeaknessDg.ItemsSource = GetWeakness;
                    GetWeaknessDg.Columns[0].IsReadOnly = true;
                    GetWeaknessDg.Columns[1].IsReadOnly = true;
                    GetWeaknessDg.Columns[2].IsReadOnly = true;
                    GetWeaknessDg.Columns[3].IsReadOnly = true;
                    GetWeaknessDg.Columns[0].Header = "Тип";
                    GetWeaknessDg.Columns[1].Header = "Частота";
                    GetWeaknessDg.Columns[2].Header = "Температура";
                    GetWeaknessDg.Columns[3].Header = "Ослабление(Дб)";
                    GetWeaknessDg.Columns[4].Header = "Ослабление";
                    GetWeaknessDg.Columns[0].Width = GetWeaknessDg.ActualWidth / 5 - (GetWeaknessDg.ActualWidth / 70);
                    GetWeaknessDg.Columns[1].Width = GetWeaknessDg.ActualWidth / 5 - (GetWeaknessDg.ActualWidth / 70);
                    GetWeaknessDg.Columns[2].Width = GetWeaknessDg.ActualWidth / 5 - (GetWeaknessDg.ActualWidth / 70);
                    GetWeaknessDg.Columns[3].Width = GetWeaknessDg.ActualWidth / 5 - (GetWeaknessDg.ActualWidth / 70);
                    break;
                
            }
        }

        public void Autosave(int numb, int temp, double db, int delta, int? localFreq = null)
        {
            
            
            List<WeaknessType> tempList;
            if (numb == 1)
            {
                tempList = Ks1Weakness;
            }
            else if (numb == 2)
            {
                tempList = Ks2Weakness;
            }
            else
            {
                tempList = GetWeakness;
            }
            if (localFreq != null)
            {
                string freq;
                if (localFreq <= 2849)
                {
                    freq = _freqs[0];
                }
                else if (localFreq <= 2899)
                {
                    freq = _freqs[1];
                }
                else if (localFreq <= 2949)
                {
                    freq = _freqs[2];
                }
                else if (localFreq <= 2999)
                {
                    freq = _freqs[3];
                }
                else if (localFreq <= 3049)
                {
                    freq = _freqs[4];
                }
                else if (localFreq <= 3099)
                {
                    freq = _freqs[5];
                }
                else if (localFreq <= 3149)
                {
                    freq = _freqs[6];
                }
                else if (localFreq <= 3200)
                {
                    freq = _freqs[7];
                }
                else
                {
                    throw new Exception();
                }
                foreach (var item in tempList.Where(f => f.Freq == freq).Where(t => t.Temp == temp).Where(d => d.WeaknessInDb * (-1) == db))
                {
                    item.RealWeakness = delta;
                }
            }
            else
            {
                foreach (var item in tempList.Where(t => t.Temp == temp).Where(d => d.WeaknessInDb * (-1) == db))
                {
                    item.RealWeakness = delta;
                }
            }

            WriteWeaknessButton_Click(null, null);
            UpdateList();
            SetVisability();
        }

        private bool _newFile;
        private void WriteWeaknessButton_Click(object sender, RoutedEventArgs e)
        {
            List<WeaknessType> ks1Weakness;
            List<WeaknessType> ks2Weakness;
            List<WeaknessType> getWeakness;
            if (Equals((Button) sender, CreateWeaknessButton))
            {
                ks1Weakness = CreateKs1Weakness;
                ks2Weakness = CreateKs2Weakness;
                getWeakness = CreateGetWeakness;
            }
            else
            {
                ks1Weakness = Ks1Weakness;
                ks2Weakness = Ks2Weakness;
                getWeakness = GetWeakness;
            }
            string[,] weaknessArray = new string[ks1Weakness.Count + ks2Weakness.Count + getWeakness.Count,5];
            int index = 0;
            for (int i = index; i < ks1Weakness.Count; i++)
            {
                weaknessArray[i, 0] = ks1Weakness[i].Type;
                weaknessArray[i, 1] = ks1Weakness[i].Freq;
                weaknessArray[i, 2] = ks1Weakness[i].Temp.ToString();
                weaknessArray[i, 3] = ks1Weakness[i].WeaknessInDb.ToString(CultureInfo.InvariantCulture);
                weaknessArray[i, 4] = ks1Weakness[i].RealWeakness.ToString();
                if (Convert.ToInt32(weaknessArray[i, 4]) < 0)
                {
                    weaknessArray[i, 4] = 0.ToString();
                }

                if (Convert.ToInt32(weaknessArray[i, 4]) > 4095)
                {
                    weaknessArray[i, 4] = 4095.ToString();
                }

                if (i == ks1Weakness.Count - 1)
                {
                    index = i;
                    break;
                }
            }
            index++;
            for (int i = index; i < ks2Weakness.Count + index; i++)
            {
                weaknessArray[i, 0] = ks2Weakness[i - index].Type;
                weaknessArray[i, 1] = ks2Weakness[i - index].Freq;
                weaknessArray[i, 2] = ks2Weakness[i - index].Temp.ToString();
                weaknessArray[i, 3] = ks2Weakness[i - index].WeaknessInDb.ToString(CultureInfo.InvariantCulture);
                weaknessArray[i, 4] = ks2Weakness[i - index].RealWeakness.ToString();

                if (Convert.ToInt32(weaknessArray[i, 4]) < 0)
                {
                    weaknessArray[i, 4] = 0.ToString();
                }

                if (Convert.ToInt32(weaknessArray[i, 4]) > 4095)
                {
                    weaknessArray[i, 4] = 4095.ToString();
                }

                if (i == ks2Weakness.Count + index - 1)
                {
                    index = i;
                    break;
                }
            }
            index++;
            for (int i = index; i < getWeakness.Count + index; i++)
            {
                weaknessArray[i, 0] = getWeakness[i - index].Type;
                weaknessArray[i, 1] = getWeakness[i - index].Freq;
                weaknessArray[i, 2] = getWeakness[i - index].Temp.ToString();
                weaknessArray[i, 3] = getWeakness[i - index].WeaknessInDb.ToString(CultureInfo.InvariantCulture);
                weaknessArray[i, 4] = getWeakness[i - index].RealWeakness.ToString();

                if (Convert.ToInt32(weaknessArray[i, 4]) < 0)
                {
                    weaknessArray[i, 4] = 0.ToString();
                }

                if (Convert.ToInt32(weaknessArray[i, 4]) > 4095)
                {
                    weaknessArray[i, 4] = 4095.ToString();
                }

                if (i == getWeakness.Count + index - 1)
                {
                    break;
                }
            }
            try
            {
                _files.SaveWickeness(weaknessArray, _newFile);
                if (_newFile)
                {
                    _newFile = false;
                    _window.DisplayLog("Файл успешно создан!");
                }
                else
                {
                    _window.DisplayLog("Файл успешно записан!");
                }
                UpdateList();

            }
            catch
            {
                if (_newFile)
                {
                    _newFile = false;
                }
                _window.DisplayLog("Ошибка при записи!");
            }

        }

        private void CreateWeaknessButton_Click(object sender, RoutedEventArgs e)
        {
            CreateKs1Weakness.Clear();
            CreateKs2Weakness.Clear();
            CreateGetWeakness.Clear();
            int temp = -50;
            double weaknessInDb = 0;

            for (int x = 0; x < 8; x++)
            {

                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        CreateKs1Weakness.Add(new WeaknessType("КС1", _freqs[x], temp, weaknessInDb, 0));
                        weaknessInDb += 0.5;
                    }
                    temp += 5;
                    weaknessInDb = 0;
                }
                temp = -50;
            }

            for (int x = 0; x < 8; x++)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        CreateKs2Weakness.Add(new WeaknessType("КС2", _freqs[x], temp, weaknessInDb, 0));
                        weaknessInDb += 0.5;
                    }
                    temp += 5;
                    weaknessInDb = 0;
                }
                temp = -50;
            }
            for (int x = 0; x < 8; x++)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j <= 12; j++)
                    {
                        CreateGetWeakness.Add(new WeaknessType("ГЕТ", _freqs[x], temp, weaknessInDb, 0));
                        weaknessInDb += 0.5;
                    }
                    temp += 5;
                    weaknessInDb = 0;
                }
                temp = -50;
            }
            _newFile = true;
            WriteWeaknessButton_Click(CreateWeaknessButton, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OpenWindow = false;
        }

        
        private int _visasbilityCounter;
        private void LeftSlideButton_Click(object sender, RoutedEventArgs e)
        {
            _visasbilityCounter--;
            if (_visasbilityCounter < 0)
            {
                _visasbilityCounter = 2;
            }
            SetVisability();
        }

        private void RightSlideButton_Click(object sender, RoutedEventArgs e)
        {
            _visasbilityCounter++;
            if (_visasbilityCounter > 2)
            {
                _visasbilityCounter = 0;
            }
            SetVisability();
        }

        private void SetVisability()
        {
            switch (_visasbilityCounter)
            {
                case 0:
                    Ks1WeaknessDg.Visibility = Visibility.Visible;
                    Ks2WeaknessDg.Visibility = Visibility.Collapsed;
                    GetWeaknessDg.Visibility = Visibility.Collapsed;
                    UpdateList();
                    break;
                case 1:
                    Ks1WeaknessDg.Visibility = Visibility.Collapsed;
                    Ks2WeaknessDg.Visibility = Visibility.Visible;
                    GetWeaknessDg.Visibility = Visibility.Collapsed;
                    UpdateList();
                    break;
                case 2:
                    Ks1WeaknessDg.Visibility = Visibility.Collapsed;
                    Ks2WeaknessDg.Visibility = Visibility.Collapsed;
                    GetWeaknessDg.Visibility = Visibility.Visible;
                    UpdateList();
                    break;
            }
        }

        private bool _rewriteTempEnable;
        private void TempWeacknesSet_Checked(object sender, RoutedEventArgs e)
        {
            _rewriteTempEnable = true;
            DataFrom.Minimum = -50;
            DataFrom.Maximum = 70;
            DataFrom.Increment = 5;
            DataFrom.Value = 0;
            DataTo.Minimum = -50;
            DataTo.Maximum = 70;
            DataTo.Increment = 5;
            DataTo.Value = 0;
            DataFromLabel.Content = "Из темп.:";
            DataToLabel.Content = "В темп.:";
            //FromSectorFreqComboB.Visibility = Visibility.Visible;
            //FromSectorFreqLabel.Visibility = Visibility.Visible;
            //ToSectorFreqComboB.Visibility = Visibility.Visible;
            //ToSectorFreqLabel.Visibility = Visibility.Visible;
            DataFrom.Visibility = Visibility.Visible;
            DataTo.Visibility = Visibility.Visible;
            DataFromLabel.Visibility = Visibility.Visible;
            DataToLabel.Visibility = Visibility.Visible;
        }

        private void FreqWeacknesSet_Checked(object sender, RoutedEventArgs e)
        {
            _rewriteTempEnable = false;
            DataFrom.Minimum = 2805;
            DataFrom.Maximum = 3200;
            DataFrom.Increment = 10;
            DataFrom.Value = 2805;
            DataTo.Minimum = 2805;
            DataTo.Maximum = 3200;
            DataTo.Increment = 10;
            DataTo.Value = 2805;
            DataFromLabel.Content = "Из частоты:";
            DataToLabel.Content = "В частоту:";
            //FromSectorFreqComboB.Visibility = Visibility.Collapsed;
            //FromSectorFreqLabel.Visibility = Visibility.Collapsed;
            //ToSectorFreqComboB.Visibility = Visibility.Collapsed;
            //ToSectorFreqLabel.Visibility = Visibility.Collapsed;
            DataFrom.Visibility = Visibility.Collapsed;
            DataTo.Visibility = Visibility.Collapsed;
            DataFromLabel.Visibility = Visibility.Collapsed;
            DataToLabel.Visibility = Visibility.Collapsed;
        }

        private void Data_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_rewriteTempEnable == false)
            {
                IntegerUpDown freq = (IntegerUpDown)sender;
                freq.Increment = freq.Value >= 3200 ? 5 : 10;
            }
        }

        private void Rewrite_Click(object sender, RoutedEventArgs e)
        {
            string fromSector = ((ComboBoxItem)FromSectorFreqComboB.SelectedItem).Content.ToString();
            string toSector = ((ComboBoxItem)ToSectorFreqComboB.SelectedItem).Content.ToString();
            switch (_visasbilityCounter)
            {
                case 0:
                    if (Ks1WeaknessDg.Visibility == Visibility.Visible)
                    {
                        string type = "КС1";
                        if (TempWeacknesSet.IsChecked != null && (bool) TempWeacknesSet.IsChecked)
                        {
                            foreach (var item in Ks1Weakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector).
                                Where(temp => temp.Temp == DataFrom.Value))
                            {
                                Ks1Weakness
                                    .Where(t => t.Type == type)
                                    .Where(temp => temp.Temp == DataTo.Value).Where(r => r.Freq == toSector).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                        else
                        {
                            foreach (var item in Ks1Weakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector))
                            {
                                Ks1Weakness
                                    .Where(t => t.Type == type)
                                    .Where(r => r.Freq == toSector).Where(temp => temp.Temp == item.Temp).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                    }
                    else
                    {
                        _window.DisplayLog("Ошибка перезаписи!");
                    }
                    break;
                case 1:
                    if (Ks2WeaknessDg.Visibility == Visibility.Visible)
                    {
                        string type = "КС2";
                        if (TempWeacknesSet.IsChecked != null && (bool)TempWeacknesSet.IsChecked)
                        {
                            foreach (var item in Ks2Weakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector).
                                Where(temp => temp.Temp == DataFrom.Value))
                            {
                                Ks2Weakness
                                    .Where(t => t.Type == type)
                                    .Where(temp => temp.Temp == DataTo.Value).Where(r => r.Freq == toSector).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                        else
                        {
                            foreach (var item in Ks2Weakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector))
                            {
                                Ks2Weakness
                                    .Where(t => t.Type == type)
                                    .Where(r => r.Freq == toSector).Where(temp => temp.Temp == item.Temp).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                    }
                    else
                    {
                        _window.DisplayLog("Ошибка перезаписи!");
                    }
                    break;
                case 2:
                    if (GetWeaknessDg.Visibility == Visibility.Visible)
                    {
                        string type = "ГЕТ";
                        if (TempWeacknesSet.IsChecked != null && (bool)TempWeacknesSet.IsChecked)
                        {
                            foreach (var item in GetWeakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector)
                                .Where(temp => temp.Temp == DataFrom.Value))
                            {
                                GetWeakness
                                    .Where(t => t.Type == type)
                                    .Where(temp => temp.Temp == DataTo.Value).Where(r => r.Freq == toSector).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                        else
                        {
                            foreach (var item in GetWeakness.Where(t => t.Type == type).Where(r => r.Freq == fromSector))
                            {
                                GetWeakness
                                    .Where(t => t.Type == type)
                                    .Where(r => r.Freq == toSector).Where(temp => temp.Temp == item.Temp).First(db => db.WeaknessInDb == item.WeaknessInDb).
                                    RealWeakness = item.RealWeakness + Convert.ToInt32(ChangeValueBox.Value);
                            }
                        }
                    }
                    else
                    {
                        _window.DisplayLog("Ошибка перезаписи!");
                        MessageBox.Show("Ошибка перезаписи!");
                    }
                    break;
            }
            WriteWeaknessButton_Click(null, null);
        }


        public (int RealKs1Weackness, int RealKs2Weackness, int RealGetWeackness) GetWeackness(int localFreq, int temp3, int temp4,
            double ks1WeacknessDb, double ks2WeacknessDb, double getWeacknessDb)
        {
            if (Ks1Weakness.Count == 0 || Ks2Weakness.Count == 0 || GetWeakness.Count == 0)
            {
                throw new Exception();
            }
            
            string freq;
            if (localFreq <= 2849)
            {
                freq = _freqs[0];
            }
            else if (localFreq <= 2899)
            {
                freq = _freqs[1];
            }
            else if (localFreq <= 2949)
            {
                freq = _freqs[2];
            }
            else if (localFreq <= 2999)
            {
                freq = _freqs[3];
            }
            else if (localFreq <= 3049)
            {
                freq = _freqs[4];
            }
            else if (localFreq <= 3099)
            {
                freq = _freqs[5];
            }
            else if (localFreq <= 3149)
            {
                freq = _freqs[6];
            }
            else if (localFreq <= 3200)
            {
                freq = _freqs[7];
            }
            else
            {
                throw new Exception();
            }
            int realKs1Weackness = Ks1Weakness.Where(f => f.Freq == freq).Where(t => t.Temp == temp3).First(w => w.WeaknessInDb == ks1WeacknessDb * (-1)).RealWeakness;
            int realKs2Weackness = Ks2Weakness.Where(f => f.Freq == freq).Where(t => t.Temp == temp3).First(w => w.WeaknessInDb == ks2WeacknessDb * (-1)).RealWeakness;
            int realGetWeackness = GetWeakness.Where(f => f.Freq == freq).Where(t => t.Temp == temp4).First(w => w.WeaknessInDb == getWeacknessDb * (-1)).RealWeakness;
            return (realKs1Weackness, realKs2Weackness, realGetWeackness);
        }



        //private void DG_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        //{

        //}

        //private void DG_SourceUpdated(object sender, DataTransferEventArgs e)
        //{
        //    DataGrid dg = (DataGrid)sender;
        //    if (dg.ItemsSource != null)
        //    {
        //        for (int i = 0; i < Ks1Weakness.Count; i++)
        //        {
        //            if (Ks1Weakness[i].RealWeakness < 0)
        //            {
        //                Ks1Weakness[i].RealWeakness = 0;
        //            }
        //            else if (Ks1Weakness[i].RealWeakness > 4095)
        //            {
        //                Ks1Weakness[i].RealWeakness = 4095;
        //            }

        //            if (Ks2Weakness[i].RealWeakness < 0)
        //            {
        //                Ks2Weakness[i].RealWeakness = 0;
        //            }
        //            else if (Ks2Weakness[i].RealWeakness > 4095)
        //            {
        //                Ks2Weakness[i].RealWeakness = 4095;
        //            }
        //        }

        //        for (int i = 0; i < GetWeakness.Count; i++)
        //        {
        //            if (GetWeakness[i].RealWeakness < 0)
        //            {
        //                GetWeakness[i].RealWeakness = 0;
        //            }
        //            else if (GetWeakness[i].RealWeakness > 4095)
        //            {
        //                GetWeakness[i].RealWeakness = 4095;
        //            }
        //        }

        //    }
        //}
    }
}

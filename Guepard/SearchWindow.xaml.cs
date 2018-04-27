using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Guepard.Block;
using Guepard.FileWork;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

namespace Guepard
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow
    {
        private readonly WorkWithFiles _files;
        private readonly bool _blocktype;
        private bool _freqSear4;
        private bool _tempSear4;
        public bool OpenWindow;
        private List<Block3> _result1;
        private List<Block4> _result2;
        private readonly List<int> _indexList = new List<int>();
        private static MainWindow _form;
        private bool _rewriteTempEnable;

        public SearchWindow(WorkWithFiles files, bool blockType, MainWindow mWindow)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OpenWindow = true;
            _files = files;
            _blocktype = blockType;
            InitializeComponent();


            _tempSear4 = false;
            _freqSear4 = false;
            _form = mWindow;

            GetAccesRule();
            //if (_blocktype)
            //{
            //    F0Label.Foreground = Brushes.Green;
            //    FhData.IsReadOnly = false;
            //    Ks1Label.Foreground = Brushes.Green;
            //    Ks1Data.IsReadOnly = false;
            //    Ks2Label.Foreground = Brushes.Green;
            //    Ks2Data.IsReadOnly = false;
            //    Ks1_10Label.Foreground = Brushes.Green;
            //    Ks1_10Data.IsReadOnly = false;
            //    Ks2_10Label.Foreground = Brushes.Green;
            //    Ks2_10Data.IsReadOnly = false;
            //    GetLabel.Foreground = Brushes.Red;
            //    GetData.IsReadOnly = true;
            //    Get_6Label.Foreground = Brushes.Red;
            //    Get_6Data.IsReadOnly = true;
            //}
            //else
            //{
            //    F0Label.Foreground = Brushes.Red;
            //    FhData.IsReadOnly = true;
            //    Ks1Label.Foreground = Brushes.Red;
            //    Ks1Data.IsReadOnly = true;
            //    Ks2Label.Foreground = Brushes.Red;
            //    Ks2Data.IsReadOnly = true;
            //    Ks1_10Label.Foreground = Brushes.Red;
            //    Ks1_10Data.IsReadOnly = true;
            //    Ks2_10Label.Foreground = Brushes.Red;
            //    Ks2_10Data.IsReadOnly = true;
            //    GetLabel.Foreground = Brushes.Green;
            //    GetData.IsReadOnly = false;
            //    Get_6Label.Foreground = Brushes.Green;
            //    Get_6Data.IsReadOnly = false;
            //}
            //DataFromFile.SelectionUnit = DataGridSelectionUnit.Cell;
        }

        public void GetAccesRule()
        {
            bool[] rules = new bool[4];
            if (Equals(_form.FhLabel.Foreground, Brushes.Red))
            {
                rules[0] = false;
            }
            else
            {
                rules[0] = true;
            }
            if (Equals(_form.Ks1Label.Foreground, Brushes.Red))
            {
                rules[1] = false;
            }
            else
            {
                rules[1] = true;
            }
            if (Equals(_form.Ks2Label.Foreground, Brushes.Red))
            {
                rules[2] = false;
            }
            else
            {
                rules[2] = true;
            }
            if (Equals(_form.GetLabel.Foreground, Brushes.Red))
            {
                rules[3] = false;
            }
            else
            {
                rules[3] = true;
            }
            SetAcess(rules);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            int freqForSearch = Convert.ToInt32(FreqSearch.Value);
            int minTempForSearch = Convert.ToInt32(MinTemp.Value);
            int maxTempForSearch = Convert.ToInt32(MaxTemp.Value);
            if (_blocktype)
            {

                _result1 = new List<Block3>(_files.Block.GetLength(0));
                for (int i = 0; i < _files.Block.GetLength(0); i++)
                {
                    int[] tempArray = new int[_files.Block.GetLength(1)];
                    for (int j = 0; j < _files.Block.GetLength(1); j++)
                    {
                        tempArray[j] = _files.Block[i, j];
                    }
                    if (_freqSear4 && !_tempSear4)
                    {
                        if (tempArray[0] == freqForSearch)
                        {
                            _result1.Add(new Block3(tempArray[0], tempArray[1], tempArray[2], tempArray[3],
                                tempArray[4], tempArray[5], tempArray[6]));

                        }
                    }
                    else if (_tempSear4 && !_freqSear4)
                    {
                        if (tempArray[1] >= minTempForSearch && tempArray[1] <= maxTempForSearch)
                        {
                            _result1.Add(new Block3(tempArray[0], tempArray[1], tempArray[2], tempArray[3],
                                tempArray[4], tempArray[5], tempArray[6]));
                        }
                    }
                    else if (_freqSear4 && _tempSear4)
                    {
                        if (tempArray[0] == freqForSearch && tempArray[1] >= minTempForSearch &&
                            tempArray[1] <= maxTempForSearch)
                        {
                            _result1.Add(new Block3(tempArray[0], tempArray[1], tempArray[2], tempArray[3],
                                tempArray[4], tempArray[5], tempArray[6]));
                        }
                    }
                    else
                    {
                        _result1.Add(new Block3(tempArray[0], tempArray[1], tempArray[2], tempArray[3], tempArray[4],
                            tempArray[5], tempArray[6]));
                    }
                }
                DataFromFile.ItemsSource = _result1;
                DataFromFile.Columns[0].Header = "Частота";
                DataFromFile.Columns[0].Width = DataFromFile.ActualWidth / 5;
                DataFromFile.Columns[0].IsReadOnly = true;
                DataFromFile.Columns[1].Header = "Температура";
                DataFromFile.Columns[1].Width = DataFromFile.ActualWidth / 5;
                DataFromFile.Columns[1].IsReadOnly = true;
                DataFromFile.Columns[2].Header = "At F несущий";
                DataFromFile.Columns[2].Width = DataFromFile.ActualWidth / 5;
                DataFromFile.Columns[3].Header = "ATКС1";
                DataFromFile.Columns[3].Width = DataFromFile.ActualWidth / 5;
                DataFromFile.Columns[4].Header = "ATКС2";
                DataFromFile.Columns[5].Header = "ATКС1(-10Дб)";
                DataFromFile.Columns[5].Visibility = Visibility.Collapsed;
                DataFromFile.Columns[6].Header = "ATКС2(-10Дб)";
                DataFromFile.Columns[6].Visibility = Visibility.Collapsed;

            }
            else
            {
                _result2 = new List<Block4>(_files.Block.GetLength(0));
                for (int i = 0; i < _files.Block.GetLength(0); i++)
                {
                    int[] tempArray = new int[_files.Block.GetLength(1)];
                    for (int j = 0; j < _files.Block.GetLength(1); j++)
                    {
                        tempArray[j] = _files.Block[i, j];
                    }
                    if (_freqSear4 && !_tempSear4)
                    {
                        if (tempArray[0] == freqForSearch)
                        {
                            _result2.Add(new Block4(tempArray[0], tempArray[1], tempArray[2], tempArray[3]));

                        }
                    }
                    else if (_tempSear4 && !_freqSear4)
                    {
                        if (tempArray[1] >= minTempForSearch && tempArray[1] <= maxTempForSearch)
                        {
                            _result2.Add(new Block4(tempArray[0], tempArray[1], tempArray[2], tempArray[3]));
                        }
                    }
                    else if (_freqSear4 && _tempSear4)
                    {
                        if (tempArray[0] == freqForSearch && tempArray[1] >= minTempForSearch &&
                            tempArray[1] <= maxTempForSearch)
                        {
                            _result2.Add(new Block4(tempArray[0], tempArray[1], tempArray[2], tempArray[3]));
                        }
                    }
                    else
                    {
                        _result2.Add(new Block4(tempArray[0], tempArray[1], tempArray[2], tempArray[3]));
                    }
                }
                DataFromFile.ItemsSource = _result2;
                DataFromFile.Columns[0].Header = "Частота";
                DataFromFile.Columns[0].Width = DataFromFile.ActualWidth / 3;
                DataFromFile.Columns[0].IsReadOnly = true;
                DataFromFile.Columns[1].Header = "Температура";
                DataFromFile.Columns[1].Width = DataFromFile.ActualWidth / 3;
                DataFromFile.Columns[1].IsReadOnly = true;
                DataFromFile.Columns[2].Header = "At F гетер";
                //DataFromFile.Columns[2].Width = DataFromFile.ActualWidth / 4;
                DataFromFile.Columns[3].Header = "At F гетер(-6Дб)";
                DataFromFile.Columns[3].Visibility = Visibility.Collapsed;

            }
        }

        private void DataFromFile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataFromFile.HasItems)
            {
                if (_blocktype)
                {
                    DataFromFile.Columns[0].Header = "Частота";
                    DataFromFile.Columns[0].Width = DataFromFile.ActualWidth / 5;
                    DataFromFile.Columns[0].IsReadOnly = true;
                    DataFromFile.Columns[1].Header = "Температура";
                    DataFromFile.Columns[1].Width = DataFromFile.ActualWidth / 5;
                    DataFromFile.Columns[1].IsReadOnly = true;
                    DataFromFile.Columns[2].Header = "At F несущий";
                    DataFromFile.Columns[2].Width = DataFromFile.ActualWidth / 5;
                    DataFromFile.Columns[3].Header = "ATКС1";
                    DataFromFile.Columns[3].Width = DataFromFile.ActualWidth / 5;
                    DataFromFile.Columns[4].Header = "ATКС2";
                    DataFromFile.Columns[5].Header = "ATКС1(-10Дб)";
                    DataFromFile.Columns[6].Header = "ATКС2(-10Дб)";

                }
                else
                {
                    DataFromFile.Columns[0].Header = "Частота";
                    DataFromFile.Columns[0].Width = DataFromFile.ActualWidth / 3;
                    DataFromFile.Columns[0].IsReadOnly = true;
                    DataFromFile.Columns[1].Header = "Температура";
                    DataFromFile.Columns[1].Width = DataFromFile.ActualWidth / 3;
                    DataFromFile.Columns[1].IsReadOnly = true;
                    DataFromFile.Columns[2].Header = "At F гетер";
                    DataFromFile.Columns[3].Header = "At F гетер(-6Дб)";

                }

            }
        }

        private void DataFromFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var cellInfo = DataFromFile.SelectedCells[0];
            //var content = cellInfo.Column.GetCellContent(cellInfo.Item);

            if (DataFromFile.HasItems && DataFromFile.SelectedItem != null)
            {
                if (_blocktype)
                {
                    Block3 data = DataFromFile.SelectedItem as Block3;
                    MainWindow.ShowResuiltBlock1InUi(data);
                }
                else
                {
                    Block4 data = DataFromFile.SelectedItem as Block4;
                    MainWindow.ShowResuiltBlock2InUi(data);
                }
            }
        }


        public void AutoSearch(int freq, int temp)
        {
            FreqSearch.Value = freq;
            MinTemp.Value = temp;
            MaxTemp.Value = temp;
            _freqSear4 = true;
            _tempSear4 = true;
            SearchButton_Click(null, null);
            DataFromFile.SelectedItem = DataFromFile.Items[0];
            DataFromFile_MouseDoubleClick(null, null);

        }

        private void WhatSearchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhatSearchCb.SelectedIndex == 0)
            {
                MinTemp.Visibility = Visibility.Hidden;
                MaxTemp.Visibility = Visibility.Hidden;
                TxBlock.Visibility = Visibility.Hidden;
                FreqSearch.Visibility = Visibility.Visible;
                _freqSear4 = true;
            }
            else if (WhatSearchCb.SelectedIndex == 1)
            {
                MinTemp.Visibility = Visibility.Visible;
                MaxTemp.Visibility = Visibility.Visible;
                TxBlock.Visibility = Visibility.Visible;
                FreqSearch.Visibility = Visibility.Hidden;
                _tempSear4 = true;
            }
            else
            {
                MinTemp.Visibility = Visibility.Hidden;
                MaxTemp.Visibility = Visibility.Hidden;
                TxBlock.Visibility = Visibility.Hidden;
                FreqSearch.Visibility = Visibility.Hidden;
                _freqSear4 = false;
                _tempSear4 = false;
            }
        }

        private void FreqSearch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _freqSear4 = true;

            IntegerUpDown freq = (IntegerUpDown) sender;
            freq.Increment = freq.Value >= 3200 ? 5 : 10;
        }

        private void MinTemp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MaxTemp != null && MaxTemp.Value < MinTemp.Value)
            {
                MaxTemp.Value = MinTemp.Value;
            }
            _tempSear4 = true;
        }

        private void MaxTemp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MinTemp != null && MaxTemp.Value < MinTemp.Value)
            {
                MaxTemp.Value = MinTemp.Value;
            }
            _tempSear4 = true;
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            DataFromFile.ItemsSource = null;
            DataFromFile.Items.Clear();
            DataFromFile.Items.Refresh();
            _freqSear4 = false;
            _tempSear4 = false;
            WhatSearchCb.SelectedIndex = 2;
        }

        private void SearchWindow1_Closed(object sender, EventArgs e)
        {
            OpenWindow = false;
        }

        private void RewriteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DefaultButton_Click(null, null);
                SearchButton_Click(null, null);
                if (_blocktype)
                {
                    List<Block3> data;
                    List<Block3> oldData;
                    if (_rewriteTempEnable == false)
                    {
                        data = (_result1.Where(v => DataToFreqIud.Value != null
                                                    && v.Freq == DataToFreqIud.Value)).ToList(); // Достаем все значения по определенной частоте Блока 3
                        oldData = (_result1.Where(v => DataFromFreqIud.Value != null
                                                       && v.Freq == DataFromFreqIud.Value)).ToList(); // Достаем все значения по определенной частоте Блока 3

                        for (int i = 0; i < data.Count; i++)
                        {
                            data[i].Freq = Convert.ToInt32(DataToFreqIud.Value);
                            if (Equals(F0Label.Foreground, Brushes.Green))
                            {
                                data[i].AtNes = oldData[i].AtNes + Convert.ToInt32(FhData.Value);
                                data[i].AtNes = Check(data[i].AtNes);
                            }
                            //else
                            //{
                            //    data[i].AtNes = oldData[i].AtNes;
                            //}
                            if (Equals(Ks1Label.Foreground, Brushes.Green))
                            {
                                data[i].AtKs10 = oldData[i].AtKs10 + Convert.ToInt32(Ks1Data.Value);
                                data[i].AtKs10 = Check(data[i].AtKs10);
                            }
                            //else
                            //{
                            //    data[i].AtKs10 = oldData[i].AtKs10;
                            //}
                            if (Equals(Ks2Label.Foreground, Brushes.Green))
                            {
                                data[i].AtKs20 = oldData[i].AtKs20 + Convert.ToInt32(Ks2Data.Value);
                                data[i].AtKs20 = Check(data[i].AtKs20);

                            }
                            //else
                            //{
                            //    data[i].AtKs20 = oldData[i].AtKs20;
                            //}
                            //item.AtKs110 += Convert.ToInt32(Ks1_10Data.Value);
                            //item.AtKs110 = Check(item.AtKs110);
                            //item.AtKs210 += Convert.ToInt32(Ks2_10Data.Value);
                            //item.AtKs210 = Check(item.AtKs210);
                        }

                    }
                    else
                    {
                        data = (_result1.Where(v => DataToFreqIud.Value != null
                                                    && v.Temp == DataToFreqIud.Value)).ToList(); // Достаем все значения по определенной темп Блока 3

                        oldData = (_result1.Where(v => DataFromFreqIud.Value != null
                                                       && v.Temp == DataFromFreqIud.Value)).ToList(); // Достаем все значения по определенной частоте Блока 3
                        for (int i = 0; i < data.Count; i++)
                        {

                            data[i].Temp = Convert.ToInt32(DataToFreqIud.Value);
                            if (Equals(F0Label.Foreground, Brushes.Green))
                            {
                                data[i].AtNes = oldData[i].AtNes + Convert.ToInt32(FhData.Value);
                                data[i].AtNes = Check(data[i].AtNes);
                            }
                            //else
                            //{
                            //    data[i].AtNes = oldData[i].AtNes;
                            //}
                            if (Equals(Ks1Label.Foreground, Brushes.Green))
                            {
                                data[i].AtKs10 = oldData[i].AtKs10 + Convert.ToInt32(Ks1Data.Value);
                                data[i].AtKs10 = Check(data[i].AtKs10);
                            }
                            else
                            //{
                            //    data[i].AtKs10 = oldData[i].AtKs10;
                            //}
                            if (Equals(Ks2Label.Foreground, Brushes.Green))
                            {
                                data[i].AtKs20 = oldData[i].AtKs20 + Convert.ToInt32(Ks2Data.Value);
                                data[i].AtKs20 = Check(data[i].AtKs20);
                            }
                            //else
                            //{
                            //    data[i].AtKs20 = oldData[i].AtKs20;
                            //}
                            //item.AtKs110 += Convert.ToInt32(Ks1_10Data.Value);
                            //item.AtKs110 = Check(item.AtKs110);
                            //item.AtKs210 += Convert.ToInt32(Ks2_10Data.Value);
                            //item.AtKs210 = Check(item.AtKs210);

                        }
                    }
                    _form.AutoSave(data);
                }
                else
                {
                    List<Block4> data;
                    List<Block4> oldData;
                    if (_rewriteTempEnable == false)
                    {
                        data = (_result2.Where(v => DataToFreqIud.Value != null
                                                    && v.Freq == DataToFreqIud.Value)).ToList(); // Достаем все значения по определенной частоте Блока 4

                        oldData = (_result2.Where(v => DataFromFreqIud.Value != null
                                                       && v.Freq == DataFromFreqIud.Value)).ToList(); // Достаем все значения по определенной темп Блока 3

                        for (int i = 0; i < data.Count; i++)
                        {
                            data[i].Freq = Convert.ToInt32(DataToFreqIud.Value);
                            if (Equals(GetLabel.Foreground, Brushes.Green))
                            {
                                data[i].AtFGet0 = oldData[i].AtFGet0 + Convert.ToInt32(GetData.Value);
                                data[i].AtFGet0 = Check(data[i].AtFGet0);
                            }
                            //else
                            //{
                            //    data[i].AtFGet0 = oldData[i].AtFGet0;
                            //}
                            //item.AtFGet6 += Convert.ToInt32(Get_6Data.Value);
                            //item.AtFGet6 = Check(item.AtFGet6);
                        }
                    }
                    else
                    {
                        data = (_result2.Where(v => DataToFreqIud.Value != null
                                                    && v.Temp == DataToFreqIud.Value)).ToList(); // Достаем все значения по определенной температуре Блока 4

                        oldData = (_result2.Where(v => DataFromFreqIud.Value != null
                                                       && v.Temp == DataFromFreqIud.Value)).ToList(); // Достаем все значения по определенной темп Блока 3
                        for (int i = 0; i < data.Count; i++)
                        {
                            data[i].Temp = Convert.ToInt32(DataToFreqIud.Value);
                            if (Equals(GetLabel.Foreground, Brushes.Green))
                            {
                                data[i].AtFGet0 = oldData[i].AtFGet0 + Convert.ToInt32(GetData.Value);
                                data[i].AtFGet0 = Check(data[i].AtFGet0);
                            }
                            //else
                            //{
                            //    data[i].AtFGet0 = oldData[i].AtFGet0;
                            //}
                            //item.AtFGet6 += Convert.ToInt32(Get_6Data.Value);
                            //item.AtFGet6 = Check(item.AtFGet6);
                        }
                    }
                    _form.AutoSave(null, data);
                }
                SearchButton_Click(null, null);
            }
            catch
            {
                _form.DisplayLog("Ошибка при перезаписи файла!");
            }

            int Check(int value)
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 4095)
                {
                    value = 4095;
                }
                return value;
            }
        }

        private void Data_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IntegerUpDown data = (IntegerUpDown) sender;
            if (data.Value == null)
            {
                data.Value = 0;
            }
        }

        private void RewriteTempRB_Checked(object sender, RoutedEventArgs e)
        {
            _rewriteTempEnable = true;
            DataFromFreqIud.Minimum = -50;
            DataFromFreqIud.Maximum = 70;
            DataFromFreqIud.Increment = 5;
            DataFromFreqIud.Value = 0;
            DataToFreqIud.Minimum = -50;
            DataToFreqIud.Maximum = 70;
            DataToFreqIud.Increment = 5;
            DataToFreqIud.Value = 0;
            RewriteFromLabel.Content = "Из темп.:";
            RewriteToLabel.Content = "В темп.:";
        }

        private void DataFromFreqIUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _freqSear4 = true;
            if (_rewriteTempEnable == false)
            {
                IntegerUpDown freq = (IntegerUpDown) sender;
                freq.Increment = freq.Value >= 3200 ? 5 : 10;
            }
        }

        private void RewriteFreqRB_Checked(object sender, RoutedEventArgs e)
        {
            _rewriteTempEnable = false;
            DataFromFreqIud.Minimum = 2805;
            DataFromFreqIud.Maximum = 3200;
            DataFromFreqIud.Increment = 10;
            DataFromFreqIud.Value = 2805;
            DataToFreqIud.Minimum = 2805;
            DataToFreqIud.Maximum = 3200;
            DataToFreqIud.Increment = 10;
            DataToFreqIud.Value = 2805;
            RewriteFromLabel.Content = "Из частоты:";
            RewriteToLabel.Content = "В частоту:";
        }


        private void SetAcess(bool[] arrayAcess)
        {
            AllRed();
            if (arrayAcess[0])
            {
                F0Label.Foreground = Brushes.Green;
                FhData.IsReadOnly = false;
            }
            if (arrayAcess[1])
            {
                Ks1Label.Foreground = Brushes.Green;
                Ks1Data.IsReadOnly = false;
            }
            if (arrayAcess[2])
            {
                Ks2Label.Foreground = Brushes.Green;
                Ks2Data.IsReadOnly = false;
            }
            if (arrayAcess[3])
            {
                GetLabel.Foreground = Brushes.Green;
                GetData.IsReadOnly = false;
            }
        }

        private void AllRed()
        {
            F0Label.Foreground = Brushes.Red;
            FhData.IsReadOnly = true;
            Ks1Label.Foreground = Brushes.Red;
            Ks1Data.IsReadOnly = true;
            Ks2Label.Foreground = Brushes.Red;
            Ks2Data.IsReadOnly = true;
            Ks110Label.Foreground = Brushes.Red;
            Ks110Data.IsReadOnly = true;
            Ks210Label.Foreground = Brushes.Red;
            Ks210Data.IsReadOnly = true;
            GetLabel.Foreground = Brushes.Red;
            GetData.IsReadOnly = true;
            Get6Label.Foreground = Brushes.Red;
            Get6Data.IsReadOnly = true;
        }

        private void WriteTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_blocktype)
                {
                    var data = _indexList.Select(i => _result1[i]).ToList();
                    _form.AutoSave(data);
                    _indexList.Clear();
                }
                else
                {
                    var data = _indexList.Select(i => _result2[i]).ToList();
                    _form.AutoSave(null, data);
                    _indexList.Clear();
                }
            }
            catch
            {
                _form.DisplayLog("Ошибка при перезаписи файла!");
                MessageBox.Show("Ошибка при перезаписи файла!");
            }
        }

        private void DataFromFile_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_blocktype)
            {
                var item = DataFromFile.SelectedItem;
                Block3 newitem = (Block3)item;
                int index = _result1.FindIndex(a => a.Freq == newitem.Freq && a.Temp == newitem.Temp);
                if (_indexList.IndexOf(index) != -1) { }
                bool contain = _indexList.IndexOf(index) != -1;
                if (contain)
                {
                    return;
                }
                _indexList.Add(index);
            }
            else
            {
                var item = DataFromFile.SelectedItem;
                Block4 newitem = (Block4) item;
                int index = _result2.FindIndex(a => a.Freq == newitem.Freq && a.Temp == newitem.Temp);
                bool contain = _indexList.IndexOf(index) != -1;
                if (contain)
                {
                    return;
                }
                _indexList.Add(index);
            }

        }

        private void DataFromFile_CurrentCellChanged(object sender, EventArgs e)
        {

        }
    }
}

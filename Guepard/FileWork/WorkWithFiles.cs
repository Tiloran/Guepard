using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Guepard.Block;
using Microsoft.Win32;

namespace Guepard.FileWork
{
    public class WorkWithFiles
    {
       public int[,] Block;
        private int[,] _sortedMassive;
        private int _blocktype;
        private string[] _tempMass;
        public string Path = String.Empty;
        public int SelectedFileType;

        public string Open(int blocktype) // Не лезь, она тебя сожрет!
        {
           
            OpenFileDialog openFileDialog = new OpenFileDialog {Filter = "Text files (*.txt)|*.txt"};
            if (openFileDialog.ShowDialog() == true)
            {
                Path = openFileDialog.FileName;
                _blocktype = blocktype;
            }
            else
            {
                Path = String.Empty;
                return Path;
            }
            ReadFile();
            if (blocktype == 7 || blocktype == 4 || blocktype == 9)
            {
                if (Block.GetLength(0) == 1025)
                {
                    for (int x = 0; x < Block.GetLength(0); x++)
                    {
                        if (blocktype == 7)
                        {
                            Block3 checkBlock = new Block3(Block[x, 0], Block[x, 1], Block[x, 2], Block[x, 3],
                                Block[x, 4], Block[x, 5], Block[x, 6]);
                            checkBlock.Freq = Block[x, 0];
                        }
                        else if (blocktype == 4)
                        {
                            Block4 checkBlock = new Block4(Block[x, 0], Block[x, 1], Block[x, 2], Block[x, 3]);
                            checkBlock.Freq = Block[x, 0];
                        }
                        else if (blocktype == 9)
                        {
                            Block3 checkBlock3 = new Block3(Block[x, 0], Block[x, 1], Block[x, 2], Block[x, 3],
                                Block[x, 4], Block[x, 5], Block[x, 6]);
                            checkBlock3.Freq = Block[x, 0];
                            Block4 checkBlock4 = new Block4(Block[x, 0], Block[x, 1], Block[x, 7], Block[x, 8]);
                            checkBlock4.Freq = Block[x, 0];
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            return Path;
        }


        private void ReadFile(bool rememberResult = false)
        {
            try
            {
                StreamReader f = new StreamReader(Path);
                char[] charsToTrim = {'\n', ' ', '\r'};
                if (f.ReadLine() != null)
                {
                    f.Close();
                    f = new StreamReader(Path);
                    var readLine = f.ReadLine();
                    if (readLine != null)
                    {
                        string[] temp = readLine.Split('\n');
                        temp[0] = temp[0].Trim(charsToTrim);
                        if ((_blocktype == 4 && temp[0].Split(',').Length - 1 == 7) ||
                            (_blocktype == 7 && temp[0].Split(',').Length - 1 == 4) ||
                            (_blocktype == 5 && temp[0].Split(',').Length - 1 != 5))
                        {
                            throw new ArgumentException();
                        }
                    }
                }
                f.Close();
                f = new StreamReader(Path);
                _tempMass = f.ReadToEnd().Split(',');
                f.Close();
                for (int i = 0; i < _tempMass.Length; i++)
                {
                    _tempMass[i] = _tempMass[i].Trim(charsToTrim);
                }

                Block = new int[_tempMass.Length / _blocktype, _blocktype];
                int count = 0;
                for (int y = 0, z = 0, i = 0; i < _tempMass.Length - 1; i++, count++)
                {
                    if (_blocktype == 5)
                    {
                        if (count == 5)
                        {
                            count = 0;
                        }
                        if (count == 0)
                        {
                            if (_tempMass[i] == "КС1")
                            {
                                Block[z, y++] = 1;
                            }
                            else if (_tempMass[i] == "КС2")
                            {
                                Block[z, y++] = 2;
                            }
                            else if (_tempMass[i] == "ГЕТ")
                            {
                                Block[z, y++] = 3;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else if (count == 1)
                        {
                            if (_tempMass[i] == "2800-2849")
                            {
                                Block[z, y++] = 1;
                            }
                            else if (_tempMass[i] == "2850-2899")
                            {
                                Block[z, y++] = 2;
                            }
                            else if (_tempMass[i] == "2900-2949")
                            {
                                Block[z, y++] = 3;
                            }
                            else if (_tempMass[i] == "2950-2999")
                            {
                                Block[z, y++] = 4;
                            }
                            else if (_tempMass[i] == "3000-3049")
                            {
                                Block[z, y++] = 5;
                            }
                            else if (_tempMass[i] == "3050-3099")
                            {
                                Block[z, y++] = 6;
                            }
                            else if (_tempMass[i] == "3100-3149")
                            {
                                Block[z, y++] = 7;
                            }
                            else if (_tempMass[i] == "3150-3200")
                            {
                                Block[z, y++] = 8;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else if (count == 2)
                        {
                            Block[z, y++] = Convert.ToInt32(_tempMass[i]);
                        }
                        else if (count == 3)
                        {
                            Block[z, y++] = Convert.ToInt32(Convert.ToDouble(_tempMass[i], CultureInfo.InvariantCulture) * 2);
                        }
                        else if (count == 4)
                        {
                            Block[z, y++] = Convert.ToInt32(_tempMass[i]);
                        }
                        else
                        {
                            y++;
                        }
                    }
                    else
                    {
                        Block[z, y++] = Convert.ToInt32(_tempMass[i]);
                    }
                    if (y == _blocktype)
                    {
                        y = 0;
                        z++;
                    }
                }



                if (rememberResult)
                {
                    if (_blocktype == 7)
                    {
                        _block3Mass = (int[,]) Block.Clone();
                    }
                    else
                    {
                        _block4Mass = (int[,]) Block.Clone();
                    }
                }

                SelectedFileType = _blocktype;

            }
            catch
            {
                SelectedFileType = 0;
                throw new Exception();
            }
        }

        public void SaveStringToFile(int[] aTarray)
        {
            ReadFile();
            List<int> avoidListIndex = new List<int>();
            for (int i = 0; i < Block.GetLength(0); i++)
            {
                int[] tempArray = new int[Block.GetLength(1)];
                for (int j = 0; j < Block.GetLength(1); j++)
                {
                    tempArray[j] = Block[i, j];
                }
                if (tempArray[0] == aTarray[0])
                {
                    if (tempArray[1] == aTarray[1])
                    {
                        avoidListIndex.Add(i);
                    }
                }
                
            }
            File.WriteAllText(Path, string.Empty);
            StreamWriter s = new StreamWriter(Path, true, Encoding.Default);
            for (int i = 0; i < Block.GetLength(0); i++)
            {
                bool finded = false;
                for (int j = 0; j < avoidListIndex.Count; j++)
                {
                    if (i == avoidListIndex[j])
                    {
                        finded = true;
                    }
                }
                if (finded == false)
                {
                    for (int j = 0; j < Block.GetLength(1); j++)
                    {
                        s.Write(" " + Block[i, j] + ",");
                    }
                    s.Write("\r" + "\n");
                }
            }
            for (int i = 0; i < aTarray.Length; i++)
            {
                    s.Write(" " + aTarray[i] + ",");
            }
            s.Write("\r" + "\n");
            s.Close();
            ReadFile();
        }


        public void Save(int[,] massive = null, string path = null)
        {
            if (massive == null)
            {
                massive = _sortedMassive;
            }
            if (path == null)
            {
                path = Path;
            }
            else
            {
                File.WriteAllText(path, string.Empty);
            }
            StreamWriter s = new StreamWriter(path, true, Encoding.Default);

            for (int i = 0; i < massive.GetLength(0); i++)
            {
                for (int j = 0; j < massive.GetLength(1); j++)
                {
                    s.Write(" " + massive[i, j] + ",");
                }
                s.Write("\r" + "\n");
            }
            s.Close();
        }


       public void SortFile()
       {
            ReadFile();
            File.WriteAllText(Path, string.Empty);
            int[] sorter = new int[_tempMass.Length / _blocktype];
            List<int> sortedList = new List<int>();
           for (int i = 0; i < _tempMass.Length / _blocktype; i++)
            {
                sorter[i] = Block[i, 0];
            }

            var result = Sorting(sorter);
            List<int> list = new List<int>();
            int temp = result.Item1[0];
            list.Add(result.Item2[0]);
            for (int i = 1; i < result.Item1.Length; i++)
            {
                if (temp == result.Item1[i])
                {
                    list.Add(result.Item2[i]);

                }
                else
                {
                    var sortList = SortingTemp(list, Block);
                    foreach (var item in sortList)
                    {
                        sortedList.Add(item);
                    }
                    temp = result.Item1[i];
                    list.Clear();
                    list.Add(result.Item2[i]);
                }
                if (i == result.Item1.Length - 1)
                {
                    var sortList = SortingTemp(list, Block);
                    foreach (var item in sortList)
                    {
                        sortedList.Add(item);
                    }
                }
            }
            _sortedMassive = new int[Block.GetLength(0), Block.GetLength(1)];
            for (int i = 0; i < Block.GetLength(0); i++)
            {
                for (int j = 0; j < Block.GetLength(1); j++)
                {
                    _sortedMassive[i, j] = Block[sortedList[i], j];
                }
            }
            Save();
           ReadFile();
       }


        private List<int> SortingTemp(List<int> mylistt, int[,] blockk)
        {
            int temp;
            int tempIndex;
            List<int> mylist = new List<int>(mylistt);
            int[,] block = new int[blockk.GetLength(0), blockk.GetLength(1)];
            Array.Copy(blockk, block, blockk.Length);
            for (int i = 0; i < mylistt.Count - 1; i++)
            {
                for (int j = i + 1; j < mylistt.Count; j++)
                {
                    if (block[mylistt[i], 1] > block[mylistt[j], 1])
                    {
                        temp = block[mylistt[i], 1];
                        block[mylistt[i], 1] = block[mylistt[j], 1];
                        block[mylistt[j], 1] = temp;

                        tempIndex = mylist[i];
                        mylist[i] = mylist[j];
                        mylist[j] = tempIndex;
                    }
                }
            }
            return mylist;
        }

        private static (int[] mass, int[] indexMass) Sorting(int[] nums)
        {
            int temp;
            int tempIndex;
            int[] indexMass = new int[nums.Length];
            for (int i = 0; i < indexMass.Length; i++)
            {
                indexMass[i] = i;
            }
            for (int i = 0; i < nums.Length - 1; i++)
            {
                for (int j = i + 1; j < nums.Length; j++)
                {
                    if (nums[i] > nums[j])
                    {
                        temp = nums[i];
                        nums[i] = nums[j];
                        nums[j] = temp;

                        tempIndex = indexMass[i];
                        indexMass[i] = indexMass[j];
                        indexMass[j] = tempIndex;
                    }
                }
            }
            return (nums, indexMass);
        }

        private int[,] _block3Mass;
        private int[,] _block4Mass;
        private int[,] _unionBlock;
        public bool UniteFile()
        {
            string pathBlock3;
            string pathBlock4;
            MessageBox.Show("Выберите файл блока 3!");
            pathBlock3 = Open(7);
            if (pathBlock3 == String.Empty)
            {
                ExitFunction();
                return true;
            }
            SortFile();
            ReadFile(true);
            MessageBox.Show("Выберите файл блока 4!");
            pathBlock4 = Open(4);
            if (pathBlock4 == String.Empty)
            {
                ExitFunction();
                return true;
            }
            SortFile();
            ReadFile(true);

            _unionBlock = new int[_block3Mass.LongLength / 7, 9];
            if (_block3Mass.LongLength / 7 == _block4Mass.LongLength / 4)
            {
                for (int i = 0; i < _block3Mass.LongLength / 7; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (j < 2)
                        {
                            if (_block3Mass[i, j] == _block4Mass[i, j])
                            {
                                _unionBlock[i, j] = _block3Mass[i, j];
                            }
                            else
                            {
                                ExitFunction();
                                return true;
                            }
                        }
                        else if (j >= 2 && j < 7)
                        {
                            _unionBlock[i, j] = _block3Mass[i, j];
                        }
                        else
                        {
                            _unionBlock[i, j] = _block4Mass[i, j - 5];
                        }
                    }
                }
            }
            else
            {
                ExitFunction();
                return true;
            }
            
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                string newfile = saveFileDialog.FileName;
                File.WriteAllText(newfile, string.Empty);
                StreamWriter s = new StreamWriter(newfile, true, Encoding.Default);

                for (int i = 0; i < _unionBlock.GetLength(0); i++)
                {
                    for (int j = 0; j < _unionBlock.GetLength(1); j++)
                    {
                        s.Write(" " + _unionBlock[i, j] + ",");
                    }
                    s.Write("\r" + "\n");
                }
                s.Close();
            }
            else
            {
                ExitFunction();
                return true;
            }

            void ExitFunction()
            {
                _block3Mass = null;
                _block4Mass = null;
                _unionBlock = null;
            }

            ExitFunction();
            return false;
        }


        public int GetStringCountInBlock()
        {
            return Block.GetLength(0);
        }

        public byte[] GetStringFromBlock(int index)
        {
            if (_blocktype == 5)
            {
                byte[] stringBlock =
                {
                    BitConverter.GetBytes(Block[index, 0])[0],
                    BitConverter.GetBytes(Block[index, 1])[0],
                    BitConverter.GetBytes(Block[index, 2])[0],
                    BitConverter.GetBytes(Block[index, 3])[0],
                    BitConverter.GetBytes(Block[index, 4])[0],
                    BitConverter.GetBytes(Block[index, 4])[1]


                };
                return stringBlock;
            }
            else
            {
                byte[] stringBlock =
                {
                    BitConverter.GetBytes(Block[index, 0])[0], // Старший байт частоты
                    BitConverter.GetBytes(Block[index, 0])[1], // Младший байт частоты
                    BitConverter.GetBytes(Block[index, 1])[0], // Температура 
                    BitConverter.GetBytes(Block[index, 2])[0], // Аттенюатор несущий старший байт
                    BitConverter.GetBytes(Block[index, 2])[1], // Аттенюатор несущий младший байт
                    BitConverter.GetBytes(Block[index, 3])[0], // Аттенюатор КС1 старший байт
                    BitConverter.GetBytes(Block[index, 3])[1], // Аттенюатор КС1 младший байт
                    BitConverter.GetBytes(Block[index, 4])[0], // Аттенюатор КС2 старший байт
                    BitConverter.GetBytes(Block[index, 4])[1], // Аттенюатор КС2 младший байт
                    BitConverter.GetBytes(Block[index, 5])[0], // Аттенюатор КС1(-10 ДБ) старший байт
                    BitConverter.GetBytes(Block[index, 5])[1], // Аттенюатор КС1(-10 ДБ) младший байт
                    BitConverter.GetBytes(Block[index, 6])[0], // Аттенюатор КС2(-10 ДБ) старший байт
                    BitConverter.GetBytes(Block[index, 6])[1], // Аттенюатор КС2(-10 ДБ) младший байт
                    BitConverter.GetBytes(Block[index, 7])[0], // Аттенюатор Геттер старший байт
                    BitConverter.GetBytes(Block[index, 7])[1], // Аттенюатор Геттер младший байт
                    BitConverter.GetBytes(Block[index, 8])[0], // Аттенюатор Геттер(-6 ДБ) старший байт
                    BitConverter.GetBytes(Block[index, 8])[1], // Аттенюатор Геттер(-6 ДБ) младший байт
                };
                return stringBlock;
            }
             

            
        }

        public void SaveWickeness(string[,] weaknessArray, bool newfile = false)
        {
            string adress;
            if (newfile)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog {Filter = "Text files (*.txt)|*.txt"};
                if (saveFileDialog.ShowDialog() == true)
                {
                    adress = saveFileDialog.FileName;
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                adress = _fileAddress;
            }
                File.WriteAllText(adress, string.Empty);
                StreamWriter s = new StreamWriter(adress, true, Encoding.UTF8);
                for (int i = 0; i < weaknessArray.GetLength(0); i++)
                {
                    for (int j = 0; j < weaknessArray.GetLength(1); j++)
                    {
                        s.Write(" " + weaknessArray[i, j] + ",");
                    }
                    s.Write("\r" + "\n");
                }
                s.Close();
        }
        private string _fileAddress;
        public string[,] ReadWickeness()
        {
            
            string[,] wicknessArray;
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == true)
            {
                _fileAddress = openFileDialog.FileName;
            }
            else
            {
               return null;
            }
            StreamReader f = new StreamReader(_fileAddress);
            char[] charsToTrim = { '\n', ' ', '\r' };
            string[] tempArray;
            
            if (f.ReadLine() != null)
            {
                f.Close();
                f = new StreamReader(_fileAddress);
                var readLine = f.ReadLine();
                if (readLine != null)
                {
                    string[] temp = readLine.Split('\n');
                    temp[0] = temp[0].Trim(charsToTrim);
                    temp = temp[0].Split(',');
                    if (temp.Length - 1 != 5) // Проверка на длину строки
                    {
                        throw new Exception();
                    }
                    //if (Blocktype == 4 && temp[0].Split(',').Length - 1 == 7 ||
                    //    Blocktype == 7 && temp[0].Split(',').Length - 1 == 4)
                    //{
                    //    throw new ArgumentException();
                    //}
                }
                f.Close();
                f = new StreamReader(_fileAddress);
                tempArray = f.ReadToEnd().Split(',');
                f.Close();
                for (int i = 0; i < tempArray.Length; i++)
                {
                    tempArray[i] = tempArray[i].Trim(charsToTrim);
                }

                wicknessArray = new string[tempArray.Length / 5, 5];
                for (int y = 0, z = 0, i = 0; i < tempArray.Length - 1; i++)
                {
                    wicknessArray[z, y++] = tempArray[i];
                    if (y == 5)
                    {
                        y = 0;
                        z++;
                    }
                }
                return wicknessArray;
            }
            return null;
        }
    }
}

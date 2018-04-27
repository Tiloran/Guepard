using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Guepard.Block;
using Guepard.Channels;
using Guepard.FileWork;
using Guepard.HID;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using WindowStartupLocation = System.Windows.WindowStartupLocation;
using WindowState = System.Windows.WindowState;

namespace Guepard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _form;
        private static int _vid;
        private static int _pid;
        private static Usb _usb;
        private ControlChannel _control;
        private DiagChannel _diagnostic;
        private ControlVariants _cmVariants;
        private CalibChannel _calibChannel;
        private Test _test;
        private WorkEmulation _workEmulation;
        private readonly WorkWithFiles _files = new WorkWithFiles();
        private readonly WorkWithFiles _downloadfiles = new WorkWithFiles();
        private bool _selectedBlockType;
        private SearchWindow _sear4Window;
        private Weakness _weaknessWindow;

        private delegate void StatusDelegate();



        //private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer(); // Таймер для ЮСБ
        //private System.Windows.Threading.DispatcherTimer _cycletimer = new System.Windows.Threading.DispatcherTimer(); //Таймер для циклов комманд
        private Timer _timer;

        private Timer _cycletimer;
        private Timer _tempTimer;



        //Проверяем запущено уже копия приложения или нет
        private void Application_Startup()
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Count(p => p.ProcessName == proc.ProcessName);
            if (count > 1)
            {
                MessageBox.Show("Приложение уже запущено!");
                Application.Current.Shutdown();
            }
        }




        public MainWindow()
        {
            Application_Startup();
            _form = this;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _vid = int.Parse(VendorIdBox.Text, NumberStyles.HexNumber);
            _pid = int.Parse(ProductIdBox.Text, NumberStyles.HexNumber);
            _usb = new Usb(_vid, _pid);
            _control = new ControlChannel(_usb);
            _diagnostic = new DiagChannel(_usb);
            _cmVariants = new ControlVariants(_usb);
            _calibChannel = new CalibChannel(_usb);
            _test = new Test(_usb);
            _workEmulation = new WorkEmulation(_usb);
            //_timer = new System.Threading.Timer(TimerCallback, null, 100, 0);
            _timer = new Timer(TimerCallback, null, 100, 100);
            //_timer.Tick += TimerCallback;
            //_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            //_timer.Start();
            AllRed();
        }



        private void TimerCallback(object sender)
        {
            if (_usb.Connection)
            {
                StatusDelegate sd = ConnectionOn;
                Dispatcher.Invoke(sd);
            }
            else
            {
                StatusDelegate sd = ConnectionOff;
                Dispatcher.Invoke(sd);
            }
            //_timer.Stop();
            //_timer.Start();
            //_timer.Dispose();
            //_timer = null;
            //_timer = new Timer(TimerCallback, null, 100, 0);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DiagButton_Click(object sender, RoutedEventArgs e) // Кнопка диагностики
        {
            object diagData = Dispatcher.Invoke(new Func<Tuple<byte, byte, byte[]>>(() =>
                {
                    byte[] transmitFreqArray = BitConverter.GetBytes(Convert.ToInt32(TransmitFreq.Value - 2800));
                    byte[] reciveFreqArray = BitConverter.GetBytes(Convert.ToInt32(ReceiverFreq.Value - 2800));
                    byte[] freqArray = //Получаем установленные значения частоты
                    {
                        transmitFreqArray[0],
                        transmitFreqArray[1],
                        reciveFreqArray[0],
                        reciveFreqArray[1]
                    };
                    byte receiverAdress = Convert.ToByte(
                        ((ComboBoxItem) ReceiverAdress.SelectedItem).Content.ToString(),
                        16); // Получаем выбранный адрес получателя
                    byte senderAdress = Convert.ToByte(((ComboBoxItem) SenderAdress.SelectedItem).Content.ToString(),
                        16); // Получаем выбранный адрес отправителя

                    Tuple<byte, byte, byte[]> result =
                        new Tuple<byte, byte, byte[]>(senderAdress, receiverAdress, freqArray);
                    return result;
                })
            );

            Tuple<byte, byte, byte[]> tupleDiagData = (Tuple<byte, byte, byte[]>) diagData;

            var status =
                _diagnostic.Send(tupleDiagData.Item1, tupleDiagData.Item2,
                    tupleDiagData.Item3); // Отправляем запрос по юсб и получаем статус об успешности отправки
            status.succes = true;
            if (status.Item2) //Проверка успешности передачи и получения по юсб
            {
                string request = "\nКоманда диагностики\nОтправленная посылка:\n";
                DisplayArray(status.Item1, request);
                var result = _diagnostic.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2 && result.Item1.Length >= 13)
                {
                    StringBuilder answer = new StringBuilder("Полученный ответ:\n", 1000);

                    for (int i = 1; i < 14; i++)
                    {
                        answer.Append(result.Item1[i].ToString("X2") + " ");
                    }
                    answer.AppendLine("\n Признак начала кодограммы:"); // Разбираем ответ по каналу диагностики

                    for (int i = 1; i < 5; i++)
                    {
                        answer.Append(result.Item1[i].ToString("X2") + " ");
                    }

                    answer.AppendLine("\nДлина кодограммы: " + (result.Item1[5] + (result.Item1[6] << 8)));
                    answer.AppendLine("Адрес получателя: " + result.Item1[7].ToString("X2"));
                    answer.AppendLine("Адрес отправителя: " + result.Item1[8].ToString("X2"));
                    answer.AppendLine("Мощность на выходе \"ОПОРНЫЙ\": " +
                                      ((result.Item1[9] & (1 << 0)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на выходе \"ТАКТОВЫЙ\": " +
                                      ((result.Item1[9] & (1 << 1)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на выходе \"СИНХРОНИЗИРУЮЩИЙ\": " +
                                      ((result.Item1[9] & (1 << 2)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на выходе \"ГЕТЕРОДИН\": " +
                                      ((result.Item1[9] & (1 << 3)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на основном выходе (ФКМ сигнал): " +
                                      ((result.Item1[9] & (1 << 4)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на выходе \"КС1\": " +
                                      ((result.Item1[9] & (1 << 5)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Мощность на выходе \"КС2\": " +
                                      ((result.Item1[9] & (1 << 6)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Захват петли ФАПЧ ЗГ: " +
                                      ((result.Item1[9] & (1 << 7)) != 0 ? "отсутствует" : "присутствует"));
                    answer.AppendLine("Признак отсутствия кодограмм по каналу управления: " +
                                      ((result.Item1[10] & (1 << 0)) != 0
                                          ? "не было кодограмм по каналу управления в течении 50 мсек"
                                          : "были кодограммы по каналу управления в течении 50 мсек"));
                    answer.AppendLine("Счет сбойных кодограмм по каналу диагностики: " + result.Item1[11]);
                    answer.AppendLine("Счет сбойных кодограмм по каналу управления: " + result.Item1[12]);
                    answer.AppendLine("Контрольная сумма: " + result.Item1[13].ToString("X2"));
                    Dispatcher.Invoke((Action) delegate
                    {
                        MessageN1Box.Text = ("0x" + $"{result.Item1[9]:X2}");
                        MessageN2Box.Text = ("0x" + $"{result.Item1[10]:X2}");
                    });
                    DisplayLog(sbText: answer);
                }
                else if (result.Item2 && result.Item1.Length < 13)
                {
                    string answer = "Получен неправильный ответ: ";
                    for (int i = 1; i < result.Item1.Length; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void ControlButton_Click(object sender, RoutedEventArgs e) // Кнопка канала контроля
        {
            byte choosed = (byte) Dispatcher.Invoke(new Func<byte>(() =>
                {
                    if (RadioBfkm.IsChecked != null && RadioBfkm.IsChecked.Value) // Проверяем какой радиобаттон включен
                    {
                        return 0b00000001;
                    }
                    if (RadioBks1.IsChecked != null && RadioBks1.IsChecked.Value)
                    {
                        return 0b00000010;
                    }
                    if (RadioBks2.IsChecked != null && RadioBks2.IsChecked.Value)
                    {
                        return 0b00000100;
                    }
                    return 0b00001000;
                })
            );


            var status = _control.Send(choosed); // Посылаем по ЮСБ и получаем статус об отправке
            if (status.Item2)
            {
                string request = "\nКоманда управления\nОтправленная посылка:\n";
                DisplayArray(status.Item1, request);
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e) //Кнопка смены номера устройства
        {
            _vid = int.Parse(VendorIdBox.Text, NumberStyles.HexNumber);
            _pid = int.Parse(ProductIdBox.Text, NumberStyles.HexNumber);


            _timer.Dispose();
            _timer = null;
            _usb = null;
            _usb = new Usb(_vid, _pid);
            _control = new ControlChannel(_usb);
            _diagnostic = new DiagChannel(_usb);
            _cmVariants = new ControlVariants(_usb);
            _calibChannel = new CalibChannel(_usb);
            _test = new Test(_usb);
            _workEmulation = new WorkEmulation(_usb);

            _timer = new Timer(TimerCallback, null, 100, 100);

            //_usb.ChangeDevice(_vid, _pid); // Меняем номеры вендора и продукта
        }

        private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
            string log = new TextRange(LogBox.Document.ContentStart, LogBox.Document.ContentEnd).Text;
            //log = new TextRange(LogBox.Document.ContentStart, LogBox.Document.ContentEnd).Text.Remove(0, log.Length / 2); // Получаем текст лога
            int linesCount = log.Count(c => c == '\n');
            //Label1.Content = lines_Count;


            if (linesCount > 1000)
            {
                //string[] lines_Log = log.Split('\n');   // Разбиваем лог на строки
                //log = String.Empty;
                LogBox.Document.Blocks.Clear();

                //int count = lines_Log.Length - 150; // Считаем лишнии строки в логе
                //for (int i = 0; i < 15; i++)
                //{
                //    //log += lines_Log[i] + "\n";
                //    LogBox.Document.Blocks.Remove(LogBox.Document.Blocks.FirstBlock); // Удаляем первую строку лога
                //}
                //DisplayLog(log);
            }
        }

        public void DisplayLog(string sText = null, StringBuilder sbText = null)
        {
            Dispatcher.Invoke((Action) delegate
            {
                if (sText != null && sbText == null)
                {
                    LogBox.AppendText($"\n {sText}");
                }
                else if (sbText != null && sText == null)
                {
                    LogBox.AppendText($"\n {sbText}");
                }
            });
        }



        private void TestButton_Click(object sender, RoutedEventArgs e) // Кнопка для теста связи по юсб
        {
            var status = _test.Send();
            if (status.Item2)
            {
                string request = "Тест команда\nОтправленная посылка:\n";
                DisplayArray(status.Item1, request);
                var result = _test.Receive();
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n";
                    DisplayArray(result.Item1, answer);
                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }



        private void ConnectionOn()
        {
            ConnectionTBlock.Text = "Подключено";
            ConnectionTBlock.Foreground = Brushes.Green;
        }

        private void ConnectionOff()
        {
            ConnectionTBlock.Text = "Не подключено";
            ConnectionTBlock.Foreground = Brushes.Red;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Process.GetCurrentProcess()
                .Kill(); // При завершении убиваем все процессы (из-за возможных проблем с потоками)
        }

        private void TestCycleCB_Checked(object sender, RoutedEventArgs e) // Цикл для теста
        {
            if (TestCycleCb.IsChecked != null && TestCycleCb.IsChecked.Value)
            {
                ControlCycleCb.IsChecked = false;
                DiagCycleCb.IsChecked = false;
                //_cycletimer.Tick += CycleCommand;
                //_cycletimer.Interval = new TimeSpan(0, 0, 1);
                //_cycletimer.Start();

                _cycletimer?.Dispose();
                _cycletimer = null;

                _cycletimer = new Timer(CycleCommand, null, 1000, 1000);
            }
            else
            {
                //_cycletimer.Stop();
                _cycletimer?.Dispose();
                _cycletimer = null;
            }
        }

        private readonly object _threadHolder = new object();

        private void CycleCommand(object sender) // Цикл для команд сообщений управления
        {

            //Dispatcher.Invoke((Action) delegate()
            lock (_threadHolder)
            {
                Dispatcher.Invoke((Action) delegate
                {
                    if (TestCycleCb.IsChecked != null && TestCycleCb.IsChecked.Value)
                    {
                        Task testTask = new Task(() =>
                        {
                            TestButton_Click(null, null);
                        });
                        testTask.Start();

                    }
                    else if (ControlCycleCb.IsChecked != null && ControlCycleCb.IsChecked.Value)
                    {
                        Task controlTask = new Task(() =>
                        {
                            ControlButton_Click(null, null);
                        });
                        controlTask.Start();
                    }
                    else if (DiagCycleCb.IsChecked != null && DiagCycleCb.IsChecked.Value)
                    {
                        Task diaglTask = new Task(() =>
                        {
                            DiagButton_Click(null, null);
                        });
                        diaglTask.Start();
                    }
                    else if (CbMessage1Cycle.IsChecked != null && CbMessage1Cycle.IsChecked.Value)
                    {
                        Task m1LTask = new Task(() =>
                        {
                            CMessageButton1_Click(null, null);
                        });
                        m1LTask.Start();
                    }
                    else if (CbMessage2Cycle.IsChecked != null && CbMessage2Cycle.IsChecked.Value)
                    {
                        Task m2LTask = new Task(() =>
                        {
                            CMessageButton2_Click(null, null);
                        });
                        m2LTask.Start();
                    }
                    else if (CbMessage3Cycle.IsChecked != null && CbMessage3Cycle.IsChecked.Value)
                    {
                        Task m3LTask = new Task(() =>
                        {
                            CMessageButton3_Click(null, null);
                        });
                        m3LTask.Start();
                    }
                    else if (CbMessage4Cycle.IsChecked != null && CbMessage4Cycle.IsChecked.Value)
                    {
                        Task m4LTask = new Task(() =>
                        {
                            CMessageButton4_Click(null, null);
                        });
                        m4LTask.Start();
                    }

                });
            }


        }

        private void ControlCycleCB_Checked(object sender, RoutedEventArgs e) // Цикл для команды управления
        {
            if (ControlCycleCb.IsChecked != null && ControlCycleCb.IsChecked.Value)
            {
                TestCycleCb.IsChecked = false;
                DiagCycleCb.IsChecked = false;
                //_cycletimer.Tick += CycleCommand;
                //_cycletimer.Interval = new TimeSpan(0, 0, 1);
                //_cycletimer.Start();

                _cycletimer?.Dispose();
                _cycletimer = null;
                _cycletimer = new Timer(CycleCommand, null, 1000, 1000);
            }
            else
            {
                _cycletimer?.Dispose();
                _cycletimer = null;
                //_cycletimer.Stop();
            }
        }

        private void DiagCycleCB_Checked(object sender, RoutedEventArgs e) // Цикл для команды диагностики
        {
            if (DiagCycleCb.IsChecked != null && DiagCycleCb.IsChecked.Value)
            {
                TestCycleCb.IsChecked = false;
                ControlCycleCb.IsChecked = false;
                //_cycletimer.Tick += CycleCommand;
                //_cycletimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                //_cycletimer.Start();

                _cycletimer?.Dispose();
                _cycletimer = null;
                _cycletimer = new Timer(CycleCommand, null, 1000, 1000);
            }
            else
            {
                _cycletimer?.Dispose();
                _cycletimer = null;
                //_cycletimer.Stop();
            }
        }

        private void CMessageButton1_Click(object sender, RoutedEventArgs e) //  Разные сообщения управления
        {
            SendControlVariant(1);
        }

        private void CMessageButton2_Click(object sender, RoutedEventArgs e)
        {
            SendControlVariant(2);
        }

        private void CMessageButton3_Click(object sender, RoutedEventArgs e)
        {
            SendControlVariant(3);
        }

        private void CMessageButton4_Click(object sender, RoutedEventArgs e)
        {
            SendControlVariant(4);
        }

        private void SendControlVariant(byte control) //  функция отправки соообщений
        {
            var result = _cmVariants.Send(control);
            if (result.Item2)
            {
                string request = $"Вариант {control} сообщения управления\nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void
            CMcycle(object sender,
                RoutedEventArgs e) // При включении циклы в сообщениях, выключаем циклы в других местах
        {
            CheckBox cb = (CheckBox) sender;
            if (cb.IsChecked != null && cb.Name == "CbMessage1Cycle" && cb.IsChecked.Value)
            {
                CbMessage2Cycle.IsChecked = false;
                CbMessage3Cycle.IsChecked = false;
                CbMessage4Cycle.IsChecked = false;
                StartTimer();
            }
            else if (cb.IsChecked != null && cb.Name == "CbMessage2Cycle" && cb.IsChecked.Value)
            {
                CbMessage1Cycle.IsChecked = false;
                CbMessage3Cycle.IsChecked = false;
                CbMessage4Cycle.IsChecked = false;
                StartTimer();
            }
            else if (cb.IsChecked != null && cb.Name == "CbMessage3Cycle" && cb.IsChecked.Value)
            {
                CbMessage2Cycle.IsChecked = false;
                CbMessage1Cycle.IsChecked = false;
                CbMessage4Cycle.IsChecked = false;
                StartTimer();
            }
            else if (cb.IsChecked != null && cb.Name == "CbMessage4Cycle" && cb.IsChecked.Value)
            {
                CbMessage2Cycle.IsChecked = false;
                CbMessage3Cycle.IsChecked = false;
                CbMessage1Cycle.IsChecked = false;
                StartTimer();
            }
            else
            {
                _cycletimer?.Dispose();
                _cycletimer = null;
                //_cycletimer.Stop();

            }


            void StartTimer()
            {
                _cycletimer?.Dispose();
                _cycletimer = null;
                _cycletimer = new Timer(CycleCommand, null, 1000, 1000);
                //_cycletimer.Tick += CycleCommand;
                //_cycletimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                //_cycletimer.Start();
            }

        }

        private void WorkOnButton_Click(object sender, RoutedEventArgs e) // Включаем эмуляцию работы
        {
            var result = _workEmulation.Send(100); // 100 код включения
            if (result.Item2)
            {
                string request = "Включение эмуляции работы\nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void WorkOffButton_Click(object sender, RoutedEventArgs e) // Выключаем эмуляцию работы
        {
            var result = _workEmulation.Send(101); // 101 код выключения
            if (result.Item2)
            {
                string request = "Выключение эмуляции работы\nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);

            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void DisplayArray(byte[] array, string message) // Функция для записи массивов в строку
        {
            for (int i = 1; i < array.Length; i++)
            {
                message += array[i].ToString("X2") + " ";
            }
            DisplayLog(message);
        }

        private void SetControlButton_Click(object sender, RoutedEventArgs e)
        {
            byte choosed;
            if (RadioBfkm.IsChecked != null && RadioBfkm.IsChecked.Value) // Проверяем какой радиобаттон включен
            {
                choosed = 0b00000001;
            }
            else if (RadioBks1.IsChecked != null && RadioBks1.IsChecked.Value)
            {
                choosed = 0b00000010;
            }
            else if (RadioBks2.IsChecked != null && RadioBks2.IsChecked.Value)
            {
                choosed = 0b00000100;
            }
            else
            {
                choosed = 0b00001000;
            }
            var status = _control.Send_SetControl(choosed); // Посылаем по ЮСБ и получаем статус об отправке
            if (status.Item2)
            {
                string request = "\nКоманда управления\nОтправленная посылка:\n";
                DisplayArray(status.Item1, request);
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void SetCalibFreqButton_Click(object sender, RoutedEventArgs e)
        {
            



            byte[] transmitFreqArray = BitConverter.GetBytes(Convert.ToInt32(CalibFreq.Value - 2800));
            byte[] reciveFreqArray = BitConverter.GetBytes(Convert.ToInt32(CalibFreq.Value - 2800));
            byte[] freqArray = //Получаем установленные значения частоты
            {
                transmitFreqArray[0],
                transmitFreqArray[1],
                reciveFreqArray[0],
                reciveFreqArray[1]
            };

            var status =
                _calibChannel.SendFreq(freqArray); // Отправляем запрос по юсб и получаем статус об успешности отправки
            if (status.Item2) //Проверка успешности передачи и получения по юсб
            {
                string request = "Команда диагностики\nОтправленная посылка:\n";
                DisplayArray(status.Item1, request);
                var result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
                    DisplayArray(result.Item1, answer);
                    answer = "Признак начала кодограммы: ";
                    for (int i = 1; i < 5; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                    if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
                    {
                        TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((128 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


                        TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((128 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
                    }
                    else
                    {
                        TemperatureBlock1Lb1.Text = "Error";
                        TemperatureBlock2Lb2.Text = "Error";
                    }

                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
            //_temp_timer = new Timer(TimerTemp , null, 1000, 0);
        }

        private void TimerTemp(object sender)
        {
            Dispatcher.Invoke((Action) delegate
            {
                SetCalibFreqButton_Click(null, null);
            });
        }


        private void ChooseFileButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = _files.Open(4);
                if (path == String.Empty)
                {
                    return;
                }
                _selectedBlockType = false;
                _sear4Window?.Close();
                _sear4Window = null;
                DisplayLog($"Файл {path} для блока 4, успешно открыт!");
            }
            catch
            {
                DisplayLog("Ошибка при открытии файла!");
                MessageBox.Show("Ошибка при открытии файла!");
            }

        }

        private void ChooseFileButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string path = _files.Open(7);
                if (path == String.Empty)
                {
                    return;
                }
                _selectedBlockType = true;
                _sear4Window?.Close();
                _sear4Window = null;
                DisplayLog($"Файл {path} для блока 3, успешно открыт!");
            }
            catch
            {
                DisplayLog("Ошибка при открытии файла!");
                MessageBox.Show("Ошибка при открытии файла!");
            }

        }

        private void SaveBlock1Button_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Path != String.Empty && _selectedBlockType)
            {
                try
                {
                    int[] aTarray = new int[7];
                    aTarray[0] = Convert.ToInt32(CalibFreq.Value);
                    if (UseManuallyTemp1Cb.IsChecked != null && UseManuallyTemp1Cb.IsChecked.Value)
                    {
                        aTarray[1] = Convert.ToInt32(TempManuallyBlock1Iud.Value);
                    }
                    else
                    {
                        try
                        {
                            aTarray[1] = Convert.ToInt32(TemperatureBlock1Lb1.Text);
                        }
                        catch
                        {
                            DisplayLog("Ошибка, установите ручную температуру или получите новую!");
                            return;
                        }
                    }
                    aTarray[2] = Convert.ToInt32(AtFNes.Value);
                    aTarray[3] = Convert.ToInt32(AtKs10.Value);
                    aTarray[4] = Convert.ToInt32(AtKs20.Value);
                    aTarray[5] = Convert.ToInt32(AtKs110.Value);
                    aTarray[6] = Convert.ToInt32(AtKs210.Value);
                    Block3 checkBlock = new Block3(aTarray[0], aTarray[1], aTarray[2], aTarray[3],aTarray[4], aTarray[5], aTarray[6]);
                    aTarray[0] = checkBlock.Freq;
                    aTarray[1] = checkBlock.Temp;
                    aTarray[2] = checkBlock.AtNes;
                    aTarray[3] = checkBlock.AtKs10;
                    aTarray[4] = checkBlock.AtKs20;
                    aTarray[5] = checkBlock.AtKs110;
                    aTarray[6] = checkBlock.AtKs210;
                    _files.SaveStringToFile(aTarray);
                    DisplayLog("Запись успешно внесена в файл!");
                    if (AutoSortCbBlock3.IsChecked == true)
                    {
                        SortingButton_Click(null, null);
                    }
                }
                catch
                {
                    DisplayLog("Ошибка при записи в файл!");
                    MessageBox.Show("Ошибка при записи в файл!");
                }
            }
            else
            {
                MessageBox.Show("Выберите сначала файл для 3 блока!");
            }
        }

        private void SaveBlock2Button_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Path != String.Empty && !_selectedBlockType)
            {
                try
                {
                    int[] aTarray = new int[4];
                    aTarray[0] = Convert.ToInt32(CalibFreq.Value);
                    if (UseManuallyTemp2Cb.IsChecked != null && UseManuallyTemp2Cb.IsChecked.Value)
                    {
                        aTarray[1] = Convert.ToInt32(TempManuallyBlock2Iud.Value);
                    }
                    else
                    {
                        try
                        {
                            aTarray[1] = Convert.ToInt32(TemperatureBlock2Lb2.Text);
                        }
                        catch
                        {
                            DisplayLog("Ошибка, установите ручную температуру или получите новую!");
                            return;
                        }
                    }
                    aTarray[2] = Convert.ToInt32(AtFGet0.Value);
                    aTarray[3] = Convert.ToInt32(AtFGet6.Value);
                    _files.SaveStringToFile(aTarray);
                    Block4 checkBlock = new Block4(aTarray[0], aTarray[1], aTarray[2], aTarray[3]);
                    aTarray[0] = checkBlock.Freq;
                    aTarray[1] = checkBlock.Temp;
                    aTarray[2] = checkBlock.AtFGet0;
                    aTarray[3] = checkBlock.AtFGet6;
                    DisplayLog("Запись успешно внесена в файл!");
                    if (AutoSortCbBlock4.IsChecked == true)
                    {
                        SortingButton_Click(null, null);
                    }
                }
                catch
                {
                    DisplayLog("Ошибка при записи в файл!");
                    MessageBox.Show("Ошибка при записи в файл!");
                }
            }
            else
            {
                MessageBox.Show("Выберите сначала файл для 4 блока!");
            }
        }

        public void AutoSave(List<Block3> block3 = null, List<Block4> block4 = null)
        {
            if (block3 != null && block4 == null)
            {
                foreach (var item in block3)
                {
                    int[] aTarray = new int[7];
                    aTarray[0] = item.Freq;
                    aTarray[1] = item.Temp;
                    aTarray[2] = item.AtNes;
                    aTarray[3] = item.AtKs10;
                    aTarray[4] = item.AtKs20;
                    aTarray[5] = item.AtKs110;
                    aTarray[6] = item.AtKs210;
                    Block3 checkBlock3 = new Block3(aTarray[0], aTarray[1], aTarray[2], aTarray[3], aTarray[4], aTarray[5], aTarray[6]);
                    checkBlock3.Freq = aTarray[0];
                    _files.SaveStringToFile(aTarray);
                }
                DisplayLog("Запись успешно внесена в файл!");
                if (AutoSortCbBlock3.IsChecked == true)
                {
                    SortingButton_Click(null, null);
                }
            }
            else if (block4 != null && block3 == null)
            {

                foreach (var item in block4)
                {
                    var aTarray = new int[4];
                    aTarray[0] = item.Freq;
                    aTarray[1] = item.Temp;
                    aTarray[2] = item.AtFGet0;
                    aTarray[3] = item.AtFGet6;
                    Block4 checkBlock4 = new Block4(aTarray[0], aTarray[1], aTarray[2], aTarray[3]);
                    checkBlock4.Freq = aTarray[0];
                    _files.SaveStringToFile(aTarray);
                }
                DisplayLog("Запись успешно внесена в файл!");
                if (AutoSortCbBlock4.IsChecked == true)
                {
                    SortingButton_Click(null, null);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        private void SortingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Path != String.Empty)
            {
                try
                {
                    _files.SortFile();
                    DisplayLog("Сортировка прошла успешно!");
                }
                catch
                {
                    DisplayLog("Ошибка во время сортировки файла!");
                }
            }
            else
            {
                DisplayLog("Сначала выберите файл!");
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Path != String.Empty && (_files.SelectedFileType == 7 || _files.SelectedFileType == 4))
            {
                if (_sear4Window != null)
                {
                    if (_sear4Window.OpenWindow == false)
                    {
                        _sear4Window = new SearchWindow(_files, _selectedBlockType, _form); // {ShowInTaskbar = false};
                        _sear4Window.Show();
                    }
                    else
                    {
                        _sear4Window.Close();
                        _sear4Window = new SearchWindow(_files, _selectedBlockType, _form); //{ShowInTaskbar = false};
                        _sear4Window.Show();
                    }
                }
                else
                {
                    _sear4Window = new SearchWindow(_files, _selectedBlockType, _form); //{ ShowInTaskbar = false };
                    _sear4Window.Show();
                }
            }
            else
            {
                DisplayLog("Сначала выберите файл!");
            }
        }

        public static void ShowResuiltBlock1InUi(Block3 data)
        {
            _form.CalibFreq.Value = data.Freq;
            _form.TempManuallyBlock1Iud.Value = data.Temp;
            _form.AtFNes.Value = data.AtNes;
            _form.AtKs10.Value = data.AtKs10;
            _form.AtKs20.Value = data.AtKs20;
            _form.AtKs110.Value = data.AtKs110;
            _form.AtKs210.Value = data.AtKs210;
        }

        public static void ShowResuiltBlock2InUi(Block4 data)
        {
            _form.CalibFreq.Value = data.Freq;
            _form.TempManuallyBlock2Iud.Value = data.Temp;
            _form.AtFGet0.Value = data.AtFGet0;
            _form.AtFGet6.Value = data.AtFGet6;
        }

        private void Block1_0DbButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] block10 =
            {
                BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[1],
                BitConverter.GetBytes(Convert.ToInt32(AtKs10.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(AtKs10.Value))[1],
                BitConverter.GetBytes(Convert.ToInt32(AtKs20.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(AtKs20.Value))[1]
            };
            var result = _calibChannel.SendAt(61, 1, block10);
            if (result.Item2)
            {
                string request = "Блок 3 (0 Дб) \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
                result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
                    DisplayArray(result.Item1, answer);
                    answer = "Признак начала кодограммы: ";
                    for (int i = 1; i < 5; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                    if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
                    {
                        TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((128 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


                        TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((128 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
                    }
                    else
                    {
                        TemperatureBlock1Lb1.Text = "Error";
                        TemperatureBlock2Lb2.Text = "Error";
                    }

                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void Block1_10DbButton_Click(object sender, RoutedEventArgs e)
        {
            if (_weaknessWindow != null)
            {
                if (_weaknessWindow.OpenWindow == false)
                {
                    _weaknessWindow = null;
                }
                else
                {
                    _weaknessWindow.Close();
                    _weaknessWindow = null;
                }
            }
            _weaknessWindow = new Weakness(_form);
            _weaknessWindow.Show();
            //byte[] block110 =
            //{
            //    BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[0],
            //    BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[1],
            //    BitConverter.GetBytes(Convert.ToInt32(AtKs110.Value))[0],
            //    BitConverter.GetBytes(Convert.ToInt32(AtKs110.Value))[1],
            //    BitConverter.GetBytes(Convert.ToInt32(AtKs210.Value))[0],
            //    BitConverter.GetBytes(Convert.ToInt32(AtKs210.Value))[1]
            //};
            //var result = _calibChannel.SendAt(61, 2, block110);
            //if (result.Item2)
            //{
            //    string request = "Блок 3 (-10 Дб) \nОтправленная посылка:\n";
            //    DisplayArray(result.Item1, request);
            //    result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
            //    if (result.Item2)
            //    {
            //        string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
            //        DisplayArray(result.Item1, answer);
            //        answer = "Признак начала кодограммы: ";
            //        for (int i = 1; i < 5; i++)
            //        {
            //            answer += result.Item1[i].ToString("X2") + " ";
            //        }
            //        DisplayLog(answer);
            //        if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
            //        {
            //            TemperatureBlock1Lb.Content = result.Item1[7] >> 7 == 1 ? ((127 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


            //            TemperatureBlock2Lb.Content = result.Item1[8] >> 7 == 1 ? ((127 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
            //        }
            //        else
            //        {
            //            TemperatureBlock1Lb.Content = "Error";
            //            TemperatureBlock2Lb.Content = "Error";
            //        }

            //    }
            //    else
            //    {
            //        DisplayLog("Нет ответа!");
            //    }
            //}
            //else
            //{
            //    DisplayLog("Ошибка при отправке посылки!");
            //}
        }

        private void Block2_0DbButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] block20 =
            {
                BitConverter.GetBytes(Convert.ToInt32(AtFGet0.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(AtFGet0.Value))[1],
                0,
                0,
                0,
                0
            };
            var result = _calibChannel.SendAt(62, 1, block20);
            if (result.Item2)
            {
                string request = "Блок 4 (0 Дб) \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
                result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
                    DisplayArray(result.Item1, answer);
                    answer = "Признак начала кодограммы: ";
                    for (int i = 1; i < 5; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                    if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
                    {
                        TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((128 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


                        TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((128 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
                    }
                    else
                    {
                        TemperatureBlock1Lb1.Text = "Error";
                        TemperatureBlock2Lb2.Text = "Error";
                    }

                }
                else
                {
                    DisplayLog("Нет ответа!");
                }

            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void Block2_6DbButton_Click(object sender, RoutedEventArgs e)
        {
            if (_weaknessWindow != null)
            {
                if (_weaknessWindow.OpenWindow == false)
                {
                    _weaknessWindow = null;
                }
                else
                {
                    _weaknessWindow.Close();
                    _weaknessWindow = null;
                }
            }
            _weaknessWindow = new Weakness(_form);
            _weaknessWindow.Show();
            //byte[] block26 =
            //{
            //    BitConverter.GetBytes(Convert.ToInt32(AtFGet6.Value))[0],
            //    BitConverter.GetBytes(Convert.ToInt32(AtFGet6.Value))[1],
            //    0,
            //    0,
            //    0,
            //    0
            //};

            //var result = _calibChannel.SendAt(62, 2, block26);
            //if (result.Item2)
            //{
            //    string request = "Блок 4 (-6 Дб) \nОтправленная посылка:\n";
            //    DisplayArray(result.Item1, request);
            //    result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
            //    if (result.Item2)
            //    {
            //        string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
            //        DisplayArray(result.Item1, answer);
            //        answer = "Признак начала кодограммы: ";
            //        for (int i = 1; i < 5; i++)
            //        {
            //            answer += result.Item1[i].ToString("X2") + " ";
            //        }
            //        DisplayLog(answer);
            //        if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
            //        {
            //            TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((127 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();

            //            TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((127 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
            //        }
            //        else
            //        {
            //            TemperatureBlock1Lb1.Text = "Error";
            //            TemperatureBlock2Lb2.Text = "Error";
            //        }

            //    }
            //    else
            //    {
            //        DisplayLog("Нет ответа!");
            //    }
            //}
            //else
            //{
            //    DisplayLog("Ошибка при отправке посылки!");
            //}
        }

        private void CommonFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ReceiverFreq.Value = CommonFreq.Value;
            TransmitFreq.Value = CommonFreq.Value;
        }

        private void CalibCycle_Checked(object sender, RoutedEventArgs e)
        {
            TempCycleFunc();
        }

        private void CalibCycle_Unchecked(object sender, RoutedEventArgs e)
        {
            TempCycleFunc();
        }

        private void TempCycleFunc()
        {
            if (CalibCycle.IsChecked != null && CalibCycle.IsChecked.Value)
            {
                DiagCycleCb.IsChecked = false;
                TestCycleCb.IsChecked = false;
                ControlCycleCb.IsChecked = false;

                _tempTimer?.Dispose();
                _tempTimer = null;
                _tempTimer = new Timer(TimerTemp, null, 0, 1000);
            }
            else
            {
                _tempTimer?.Dispose();
                _tempTimer = null;
            }
        }

        private void UnionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_files.UniteFile())
                {
                    throw new Exception();
                }
                DisplayLog("Файл успешно объединен.");
            }
            catch
            {
                DisplayLog("Ошибка при соединении файлов!");
                MessageBox.Show("ООшибка при соединении файлов!");
            }
        }

        private void CalibFreq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CalibFreq.Increment = CalibFreq.Value >= 3200 ? 5 : 10;
            if (SearchInFile != null && SearchInFile.IsChecked == true)
            {
                SearchInFile_Checked(null, null);
            }
        }

        private bool _canWriteEeprom = true; // Переменная для включения и выключения записи в EEPROM
        private void StartWriteButton_Click(object sender, RoutedEventArgs e)
        {
            byte command;
            string answer;
            string wronganswer;
            if (RbCheckEeprom.IsChecked != null && (bool) RbCheckEeprom.IsChecked && StandartFile.IsChecked == true)
            {
                command = 64;
                answer = "Обычный файл успешно прошел проверку!";
                wronganswer = "Обычный файл не соответcвует содержимому памяти EEPROM!";
            }
            else if (RbWriteEeprom.IsChecked == true && StandartFile.IsChecked == true)
            {
                command = 63;
                answer = "Последняя строка была успешно записана и вычитана!";
                wronganswer = "Записаное и вычитаное не соответствуют друг другу!";
            }
            else if (RbCheckEeprom.IsChecked == true && WeaknessFile.IsChecked == true)
            {
                command = 67;
                answer = "Файл ослабления успешно прошел проверку!";
                wronganswer = "Файл ослабления не соответcвует содержимому памяти EEPROM!";
            }
            else if (RbWriteEeprom.IsChecked == true && WeaknessFile.IsChecked == true)
            {
                command = 68;
                answer = "Последняя строка была успешно записана и вычитана!";
                wronganswer = "Записаное и вычитаное не соответствуют друг другу!";
            }
            else
            {
                throw new Exception();
            }
            UploadBar.Minimum = Convert.ToDouble(StringNumberBox.Value);
            UploadBar.Maximum = Convert.ToDouble(StringNumberBox.Maximum);
            UploadBar.Value = UploadBar.Minimum;
            Task eepromTask = new Task(() =>
            {
                int selectedType = 0;
                _canWriteEeprom = true;
                while (true)
                {
                    
                    if (_canWriteEeprom && (_downloadfiles.SelectedFileType == 9 || _downloadfiles.SelectedFileType == 5))
                    {
                        selectedType = _downloadfiles.SelectedFileType;
                        int? max = (int?)Dispatcher.Invoke(new Func<int?>(() => StringNumberBox.Maximum));
                        if (max == _downloadfiles.GetStringCountInBlock())
                        {
                            int? stringNumber = (int?) Dispatcher.Invoke(new Func<int?>(() => StringNumberBox.Value));
                            if (stringNumber != null)
                            {
                                byte[] stringFromUnionBlock = _downloadfiles.GetStringFromBlock(((int) stringNumber) - 1);
                                var result = _calibChannel.SendWriteEeprom(stringFromUnionBlock, command);
                                
                                if (result.succes)
                                {
                                    bool error = false;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (i != 0)
                                        {
                                            _calibChannel.SendWriteEeprom(stringFromUnionBlock, command);
                                        }
                                        result = _calibChannel.Receive();
                                        
                                        if (result.succes)
                                        {
                                            if (result.transmitBuffer[5] == 50)
                                            {
                                                Dispatcher.Invoke((Action) delegate
                                                {
                                                    if (StringNumberBox.Value < StringNumberBox.Maximum)
                                                    {
                                                        StringNumberBox.Value++;
                                                        UploadBar.Value++;
                                                        error = false;
                                                    }
                                                    else
                                                    {
                                                        error = true;
                                                    }
                                                });
                                                if (error)
                                                {
                                                    Dispatcher.Invoke((Action) delegate
                                                    {
                                                        DisplayLog(answer);
                                                    });
                                                }
                                                break;
                                            }
                                            else
                                            {
                                                error = true;
                                                Dispatcher.Invoke((Action) delegate
                                                {
                                                    DisplayLog(wronganswer);
                                                });
                                            }
                                        }
                                        else
                                        {
                                            error = true;
                                            Dispatcher.Invoke((Action) delegate
                                            {
                                                DisplayLog("Нет ответа!");
                                            });
                                        }
                                    }
                                    if (error)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke((Action) delegate
                                    {
                                        DisplayLog("Ошибка при отправке!");
                                    });
                                    break;
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke((Action) delegate
                                {
                                    DisplayLog("Ошибка при работе с файлом!");
                                });
                                break;
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke((Action) delegate
                            {
                                DisplayLog("Ошибка в чтении файла!");
                            });
                            break;
                        }
                    }
                    if ((selectedType == 9 && _downloadfiles.SelectedFileType != 9) || ((selectedType == 3 && _downloadfiles.SelectedFileType != 3)))
                    {
                        Dispatcher.Invoke((Action) delegate
                        {
                            DisplayLog("Выбран неправильный файл!");
                        });
                        break;
                    }
                }
            });
            
            eepromTask.Start();
        }

        private void StopWriteButton_Click(object sender, RoutedEventArgs e)
        {
            _canWriteEeprom = false;
            UploadBar.Value = UploadBar.Minimum;
        }

        private void FileChooseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UploadBar.Value = UploadBar.Minimum;
                if (StandartFile.IsChecked == true)
                {
                    // Выбираем общий файл
                    string path = _downloadfiles.Open(9);
                    // Устнавливаем пределы для управления строками
                    StringNumberBox.Maximum = _downloadfiles.GetStringCountInBlock();
                    StringNumberBox.Value = 1;
                    if (path != String.Empty)
                    {
                        DisplayLog("Обычный файл готов к записи!");
                    }
                }
                else if (WeaknessFile.IsChecked == true)
                {
                    // Выбираем общий файл
                    string path = _downloadfiles.Open(5);
                    StringNumberBox.Maximum = _downloadfiles.GetStringCountInBlock();
                    StringNumberBox.Value = 1;
                    if (path != String.Empty)
                    {
                        DisplayLog("Файл ослабления готов к записи!");
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                DisplayLog("Выбран не правильный файл или файл имеет неправильный формат!");
                MessageBox.Show("Выбран не правильный файл или файл имеет неправильный формат!");
            }

        }

        private void WriteCoefDbButton_Click(object sender, RoutedEventArgs e)
        {
            byte ks1;
            byte ks2;
            byte get;
            if (Ks1CoefBox.Value != null)
            {
                ks1 = (byte) ((Ks1CoefBox.Value * -1) / 0.5);
            }
            else
            {
                DisplayLog("Ошибка?, неправильно значение КС1!");
                return;
            }

            if (Ks2CoefBox.Value != null)
            {
                ks2 = (byte) ((Ks2CoefBox.Value * -1) / 0.5);
            }
            else
            {
                DisplayLog("Ошибка!, неправильно значение КС2!");
                return;
            }

            if (GetterCoefBox.Value != null)
            {
                get = (byte) ((GetterCoefBox.Value * -1) / 0.5);
            }
            else
            {
                DisplayLog("Ошибка!, неправильно значение гет.!");
                return;
            }
            var result = _calibChannel.SendWriteCoefDb(ks1, ks2, get);
            if (result.Item2)
            {
                string request = "Неоперативное управление запись \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void ReadCoefDbButton_Click(object sender, RoutedEventArgs e)
        {
            var result = _calibChannel.SendReadCoefDb();
            if (result.Item2)
            {
                string request = "Неоперативное управление чтение \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);

                result = _calibChannel.Receive();
                if (result.succes)
                {
                    string answer = "Полученный ответ:\n";
                    DisplayArray(result.Item1, answer);
                    Ks1CoefBox.Value = GetDb(result.Item1[7]);
                    Ks2CoefBox.Value = GetDb(result.Item1[8]);
                    GetterCoefBox.Value = GetDb(result.Item1[9]);

                    double? GetDb(byte data)
                    {
                        return (data * 0.5) * -1;
                    }
                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void PatternBlock3Button_Click(object sender, RoutedEventArgs e)
        {
            var tempMin = TempManuallyBlock1Iud.Minimum;
            var tempMax = TempManuallyBlock1Iud.Maximum;
            var calValue = CalibFreq.Value;
            var calMin = CalibFreq.Minimum;
            var calMax = CalibFreq.Maximum;
            var tempValue = tempMin;
            CalibFreq.Value = calMin;
            var dropChangeTemp = false;
            var patterMassiveBlock3 = new int[1025,7];
            int Test = 0;
            
            for (int i = 0; i < 1025; i++)
            {

                if (calValue != null && tempValue != null)
                {
                    if (Test > 3000)
                    {
                        Test = 0;
                    }
                    patterMassiveBlock3[i, 0] = (int) calValue;
                    patterMassiveBlock3[i, 1] = (int) tempValue;
                    patterMassiveBlock3[i, 2] = Test++;
                    patterMassiveBlock3[i, 3] = Test++;
                    patterMassiveBlock3[i, 4] = Test++;
                    patterMassiveBlock3[i, 5] = Test++;
                    patterMassiveBlock3[i, 6] = Test++;

                    
                    if (tempValue == tempMax)
                    {
                        tempValue = tempMin;
                        dropChangeTemp = true;
                        if (calValue < (calMax - 5))
                        {
                            calValue += 10;
                        }
                        else if (calValue >= (calMax - 5) && calValue < calMax)
                        {
                            calValue += 5;
                        }
                    }

                    if (tempValue < tempMax && dropChangeTemp == false)
                    {
                        tempValue += 5;
                    }
                    else if (dropChangeTemp)
                    {
                        dropChangeTemp = false;
                    }
                }
                else
                {
                    DisplayLog("Ошибка при создании шаблона!");
                    return;
                }

            }
            var saveFileDialog = new SaveFileDialog {Filter = "Text files (*.txt)|*.txt"};
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string newfile = saveFileDialog.FileName;
                    _files.Save(patterMassiveBlock3, newfile);
                    DisplayLog("Шаблон для блока 3, успешно создан!");
                }
                catch
                {
                    DisplayLog("Ошибка при создании файла!");
                }
                


            }

        }

        private void PatternBlock4Button_Click(object sender, RoutedEventArgs e)
        {
            var tempMin = TempManuallyBlock2Iud.Minimum;
            var tempMax = TempManuallyBlock2Iud.Maximum;
            var calMin = CalibFreq.Minimum;
            var calMax = CalibFreq.Maximum;
            var tempValue = tempMin;
            var calValue = calMin;
            var dropChangeTemp = false;
            var patterMassiveBlock4 = new int[1025, 4];
            int Test = 3000;

            for (int i = 0; i < 1025; i++)
            {

                if (calValue != null)
                {
                    if (Test < 0)
                    {
                        Test = 3000;
                    }
                    patterMassiveBlock4[i, 0] = Convert.ToInt32(calValue);
                    patterMassiveBlock4[i, 1] = Convert.ToInt32(tempValue);
                    patterMassiveBlock4[i, 2] = Test--;
                    patterMassiveBlock4[i, 3] = Test--;


                    if (tempValue == tempMax)
                    {
                        tempValue = tempMin;
                        dropChangeTemp = true;
                        if (calValue < (calMax - 5))
                        {
                            calValue += 10;
                        }
                        else if (calValue >= (calMax - 5) && calValue < calMax)
                        {
                            calValue += 5;
                        }
                    }

                    if (tempValue < tempMax && dropChangeTemp == false)
                    {
                        tempValue += 5;
                    }
                    else if (dropChangeTemp)
                    {
                        dropChangeTemp = false;
                    }
                }
                else
                {
                    DisplayLog("Ошибка при создании шаблона!");
                    return;
                }

            }
            var saveFileDialog = new SaveFileDialog {Filter = "Text files (*.txt)|*.txt"};
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string newfile = saveFileDialog.FileName;
                    _files.Save(patterMassiveBlock4, newfile);
                    DisplayLog("Шаблон для блока 4, успешно создан!");
                }
                catch
                {
                    DisplayLog("Ошибка при создании файла!");
                }
                
            }
        }

        private bool _firstTime = true;
        private bool _allowUseDeltaKs1;
        private bool _allowUseDeltaKs2;
        private bool _allowUseDeltaGet;
        private void SearchInFile_Checked(object sender, RoutedEventArgs e)
        {
            if (_files.Path != String.Empty && (_files.SelectedFileType == 7 || _files.SelectedFileType == 4))
            {
                if (_sear4Window == null)
                {
                    _sear4Window = new SearchWindow(_files, _selectedBlockType, _form); // {ShowInTaskbar = false};
                    _sear4Window.Show();
                    _sear4Window.WindowState = WindowState.Minimized;
                }
                else
                {
                    if(_sear4Window.OpenWindow == false)
                    {
                        _sear4Window = null;
                        _sear4Window = new SearchWindow(_files, _selectedBlockType, _form); //{ ShowInTaskbar = false };
                        _sear4Window.Show();
                        _sear4Window.WindowState = WindowState.Minimized;
                    }
                }
                try
                {
                    int temp3 = (UseManuallyTemp1Cb.IsChecked == true)
                    ? Convert.ToInt32(TempManuallyBlock1Iud.Value)
                    : Convert.ToInt32(TemperatureBlock1Lb1.Text);
                    int temp4 = (UseManuallyTemp2Cb.IsChecked == true)
                        ? Convert.ToInt32(TempManuallyBlock2Iud.Value)
                        : Convert.ToInt32(TemperatureBlock2Lb2.Text);
                if (_files.SelectedFileType == 7)
                    {
                        if (_firstTime)
                        {
                            UseManuallyTemp1Cb.IsChecked = true;
                            _firstTime = false;
                        }
                        if (CalibFreq.Value != null)
                        {
                            if (TempManuallyBlock1Iud.Value != null)
                                _sear4Window.AutoSearch(Convert.ToInt32(CalibFreq.Value), temp3);
                        }
                    }
                    else if (_files.SelectedFileType == 4)
                    {
                        if (_firstTime)
                        {
                            UseManuallyTemp2Cb.IsChecked = true;
                            _firstTime = false;
                        }
                        if (CalibFreq.Value != null)
                    {
                            if (TempManuallyBlock2Iud.Value != null)
                                _sear4Window.AutoSearch(Convert.ToInt32(CalibFreq.Value), temp4);

                            
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                    if (temp3 > 70 || temp4 > 70)
                    {
                        throw new Exception();
                    }
                    var realWeackness = _weaknessWindow.GetWeackness(Convert.ToInt32(CalibFreq.Value), temp3, temp4,
                        Convert.ToDouble(Ks1HelpDb.Value), Convert.ToDouble(Ks2HelpDb.Value),
                        Convert.ToDouble(GetHelpDb.Value));
                    Ks1Delta.Value = -1;
                    Ks2Delta.Value = -1;
                    GetDelta.Value = -1;
                    _allowUseDeltaKs1 = true;
                    _allowUseDeltaKs2 = true;
                    _allowUseDeltaGet = true;
                    Ks1Delta.Value = realWeackness.RealKs1Weackness;
                    Ks2Delta.Value = realWeackness.RealKs2Weackness;
                    GetDelta.Value = realWeackness.RealGetWeackness;


            }
            catch
            {
                DisplayLog("Ошибка во время поиска файла!");
                DisplayLog("Оба файла (обычный, ослабления) должны быть открыты! Температура должна быть известна!");
            }


        }
            else
            {
                SearchInFile.IsChecked = false;
                UseManuallyTemp1Cb.IsChecked = false;
                UseManuallyTemp2Cb.IsChecked = false;
                DisplayLog("Сначала выберите файл!");
            }
        }

        private void SearchInFile_Unchecked(object sender, RoutedEventArgs e)
        {
            UseManuallyTemp1Cb.IsChecked = false;
            UseManuallyTemp2Cb.IsChecked = false;
            _firstTime = true;
            _allowUseDeltaKs1 = false;
            _allowUseDeltaKs2 = false;
            _allowUseDeltaGet = false;
        }

        private void UseManuallyTemp1Cb_Checked(object sender, RoutedEventArgs e)
        {
            TemperatureBlock1Lb1.Foreground = Brushes.Red;
            TempManuallyBlock1Iud.Foreground = Brushes.Green;

        }

        private void UseManuallyTemp1Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            TempManuallyBlock1Iud.Foreground = Brushes.Red;
            TemperatureBlock1Lb1.Foreground = Brushes.Green;
        }

        private void UseManuallyTemp2Cb_Checked(object sender, RoutedEventArgs e)
        {
            TemperatureBlock2Lb2.Foreground = Brushes.Red;
            TempManuallyBlock2Iud.Foreground = Brushes.Green;
        }

        private void UseManuallyTemp2Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            TempManuallyBlock2Iud.Foreground = Brushes.Red;
            TemperatureBlock2Lb2.Foreground = Brushes.Green;
        }

        void HelpSear4()
        {
            if (_sear4Window != null && _sear4Window.OpenWindow)
            {
                _sear4Window.GetAccesRule();
            }
        }

        private void FhLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AllRed();
            FhLabel.Foreground = Brushes.Green;
            AtFNes.Foreground = Brushes.Green;
            AtFNes.IsReadOnly = false;
            HelpSear4();
        }

        private void Ks1Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AllRed();
            Ks1Label.Foreground = Brushes.Green;
            AtKs10.Foreground = Brushes.Green;
            AtKs10.IsReadOnly = false;
            Ks1Wick.Foreground = Brushes.Green;
            Ks1Wick.IsReadOnly = false;
            Ks1Delta.Foreground = Brushes.Green;
            Ks1Delta.IsReadOnly = false;
            Ks1HelpDb.Foreground = Brushes.Green;
            Ks1HelpDb.IsReadOnly = false;
            Ks1HelpDbButton.Foreground = Brushes.Green;
            HelpSear4();
        }

        private void Ks2Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AllRed();
            Ks2Label.Foreground = Brushes.Green;
            AtKs20.Foreground = Brushes.Green;
            AtKs20.IsReadOnly = false;
            Ks2Wick.Foreground = Brushes.Green;
            Ks2Wick.IsReadOnly = false;
            Ks2Delta.Foreground = Brushes.Green;
            Ks2Delta.IsReadOnly = false;
            Ks2HelpDb.Foreground = Brushes.Green;
            Ks2HelpDb.IsReadOnly = false;
            Ks2HelpDbButton.Foreground = Brushes.Green;
            HelpSear4();
        }

        private void GetLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AllRed();
            GetLabel.Foreground = Brushes.Green;
            AtFGet0.Foreground = Brushes.Green;
            AtFGet0.IsReadOnly = false;
            GetWick.Foreground = Brushes.Green;
            GetWick.IsReadOnly = false;
            GetDelta.Foreground = Brushes.Green;
            GetDelta.IsReadOnly = false;
            GetHelpDb.Foreground = Brushes.Green;
            GetHelpDb.IsReadOnly = false;
            GetHelpDbButton.Foreground = Brushes.Green;
            HelpSear4();
        }

        private void AllRed()
        {
            FhLabel.Foreground = Brushes.Red;
            AtFNes.Foreground = Brushes.Red;
            AtFNes.IsReadOnly = true;
            Ks1Wick.Foreground = Brushes.Red;
            Ks1Wick.IsReadOnly = true;
            Ks1Delta.Foreground = Brushes.Red;
            Ks1Delta.IsReadOnly = true;
            Ks1Label.Foreground = Brushes.Red;
            AtKs10.Foreground = Brushes.Red;
            AtKs10.IsReadOnly = true;
            Ks2Wick.Foreground = Brushes.Red;
            Ks2Wick.IsReadOnly = true;
            Ks2Delta.Foreground = Brushes.Red;
            Ks2Delta.IsReadOnly = true;
            Ks2Label.Foreground = Brushes.Red;
            AtKs20.Foreground = Brushes.Red;
            AtKs20.IsReadOnly = true;
            GetLabel.Foreground = Brushes.Red;
            AtFGet0.Foreground = Brushes.Red;
            AtFGet0.IsReadOnly = true;
            GetWick.Foreground = Brushes.Red;
            GetWick.IsReadOnly = true;
            GetDelta.Foreground = Brushes.Red;
            GetDelta.IsReadOnly = true;
            Ks1HelpDb.Foreground = Brushes.Red;
            Ks1HelpDb.IsReadOnly = true;
            Ks1HelpDbButton.Foreground = Brushes.Red;
            Ks2HelpDb.Foreground = Brushes.Red;
            Ks2HelpDb.IsReadOnly = true;
            Ks2HelpDbButton.Foreground = Brushes.Red;
            GetHelpDb.Foreground = Brushes.Red;
            GetHelpDb.IsReadOnly = true;
            GetHelpDbButton.Foreground = Brushes.Red;
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void RBWriteEEPROM_Checked(object sender, RoutedEventArgs e)
        {
            if (StringNumberBox != null)
            {
                StringNumberBox.Value = 1;
            }
        }

        private void RBCheckEEPROM_Checked(object sender, RoutedEventArgs e)
        {
            if (StringNumberBox != null)
            {
                StringNumberBox.Value = 1;
            }
        }

        private string _tempBlock3 = "0"; 
        private void TemperatureBlock1Lb1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WatchTemp.IsChecked == true && _files.SelectedFileType == 7)
            {
                if (_tempBlock3 != TemperatureBlock1Lb1.Text)
                {
                    MessageBox.Show($"Температура 3 блока изменилась с ({_tempBlock3}) на {TemperatureBlock1Lb1.Text}");
                }
            }
            _tempBlock3 = TemperatureBlock1Lb1.Text;
        }
        private string _tempBlock4 = "0";
        private void TemperatureBlock2Lb2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WatchTemp.IsChecked == true && _files.SelectedFileType == 4)
            {
                if (_tempBlock4 != TemperatureBlock2Lb2.Text)
                {
                    MessageBox.Show($"Температура 4 блока изменилась с ({_tempBlock4}) на {TemperatureBlock2Lb2.Text}");
                }
            }
            _tempBlock4 = TemperatureBlock2Lb2.Text;
        }

        private void AtKs10_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (((IntegerUpDown) sender).Name != ((IntegerUpDown) _focusedElement).Name)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            if (Ks1Delta != null && AtKs10 != null && Ks1Wick != null)
            {
                int diffrence = Convert.ToInt32(AtKs10.Value - Ks1Wick.Value);

                if (diffrence > 4095)
                {
                    diffrence = 4095;
                }
                else if (diffrence < 0)
                {
                    diffrence = 0;
                }
                Ks1Delta.Value = diffrence;
            }
        }

        private void Ks1_delta_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {


            if (!_allowUseDeltaKs1)
            {
                try
                {
                    if (((IntegerUpDown) sender).Name != ((IntegerUpDown) _focusedElement).Name)
                    {
                        return;
                    }
                }
                catch
                {
                    return;
                }
            }
            if (Ks1Delta.Value < 0)
            {
                Ks1Delta.Value = 0;
            }
            int realPower = Convert.ToInt32(AtKs10.Value - Ks1Delta.Value);
            if (realPower > 4095)
            {
                realPower = 4095;
            }
            else if (realPower < 0)
            {
                realPower = 0;
            }
            Ks1Wick.Value = realPower;
            _allowUseDeltaKs1 = false;
        }

        private void AtKs20_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (((IntegerUpDown)sender).Name != ((IntegerUpDown)_focusedElement).Name)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            if (Ks2Delta != null && AtKs20 != null && Ks2Wick != null)
            {
                int diffrence = Convert.ToInt32(AtKs20.Value - Ks2Wick.Value);

                if (diffrence > 4095)
                {
                    diffrence = 4095;
                }
                else if (diffrence < 0)
                {
                    diffrence = 0;
                }
                Ks2Delta.Value = diffrence;
            }
        }

        private void AtFGet0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (((IntegerUpDown)sender).Name != ((IntegerUpDown)_focusedElement).Name)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            if (GetDelta != null && AtFGet0 != null && GetWick != null)
            {
                int diffrence = Convert.ToInt32(AtFGet0.Value - GetWick.Value);

                if (diffrence > 4095)
                {
                    diffrence = 4095;
                }
                else if (diffrence < 0)
                {
                    diffrence = 0;
                }
                GetDelta.Value = diffrence;
            }
        }

        private void Ks2_delta_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_allowUseDeltaKs2)
            {
                try
                {
                    if (((IntegerUpDown) sender).Name != ((IntegerUpDown) _focusedElement).Name)
                    {
                        return;
                    }
                }
                catch
                {
                    return;
                }
            }
            if (Ks2Delta.Value < 0)
            {
                Ks2Delta.Value = 0;
            }
            int realPower = Convert.ToInt32(AtKs20.Value - Ks2Delta.Value);
            if (realPower > 4095)
            {
                realPower = 4095;
            }
            else if (realPower < 0)
            {
                realPower = 0;
            }
            _allowUseDeltaKs2 = false;
            Ks2Wick.Value = realPower;
        }

        private void Get_delta_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_allowUseDeltaGet)
            {
                try
                {
                    if (((IntegerUpDown) sender).Name != ((IntegerUpDown) _focusedElement).Name)
                    {
                        return;
                    }
                }
                catch
                {
                    return;
                }
            }
            if (GetDelta.Value < 0)
            {
                GetDelta.Value = 0;
            }
            int realPower = Convert.ToInt32(AtFGet0.Value - GetDelta.Value);
            if (realPower > 4095)
            {
                realPower = 4095;
            }
            else if (realPower < 0)
            {
                realPower = 0;
            }
            _allowUseDeltaGet = false;
            GetWick.Value = realPower;
        }

        private void Block10DbButton1_Click(object sender, RoutedEventArgs e)
        {

            byte[] block10 =
            {
                BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(AtFNes.Value))[1],
                BitConverter.GetBytes(Convert.ToInt32(Ks1Wick.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(Ks1Wick.Value))[1],
                BitConverter.GetBytes(Convert.ToInt32(Ks2Wick.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(Ks2Wick.Value))[1]
            };
            var result = _calibChannel.SendAt(61, 1, block10);
            if (result.Item2)
            {
                string request = "Блок 3 (0 Дб) \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
                result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
                    DisplayArray(result.Item1, answer);
                    answer = "Признак начала кодограммы: ";
                    for (int i = 1; i < 5; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                    if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
                    {
                        TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((128 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


                        TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((128 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
                    }
                    else
                    {
                        TemperatureBlock1Lb1.Text = "Error";
                        TemperatureBlock2Lb2.Text = "Error";
                    }

                }
                else
                {
                    DisplayLog("Нет ответа!");
                }
            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void Block20DbButton1_Click(object sender, RoutedEventArgs e)
        {

            byte[] block20 =
            {
                BitConverter.GetBytes(Convert.ToInt32(GetWick.Value))[0],
                BitConverter.GetBytes(Convert.ToInt32(GetWick.Value))[1],
                0,
                0,
                0,
                0
            };
            var result = _calibChannel.SendAt(62, 1, block20);
            if (result.Item2)
            {
                string request = "Блок 4 (0 Дб) \nОтправленная посылка:\n";
                DisplayArray(result.Item1, request);
                result = _calibChannel.Receive(); // Получаем по ЮСБ ответ
                if (result.Item2)
                {
                    string answer = "Полученный ответ:\n"; // Разбираем ответ по каналу диагностики
                    DisplayArray(result.Item1, answer);
                    answer = "Признак начала кодограммы: ";
                    for (int i = 1; i < 5; i++)
                    {
                        answer += result.Item1[i].ToString("X2") + " ";
                    }
                    DisplayLog(answer);
                    if (result.Item1[7] != 0xFF && result.Item1[8] != 0xFF)
                    {
                        TemperatureBlock1Lb1.Text = result.Item1[7] >> 7 == 1 ? ((128 - (result.Item1[7] - 128)) * -1).ToString() : result.Item1[7].ToString();


                        TemperatureBlock2Lb2.Text = result.Item1[8] >> 7 == 1 ? ((128 - (result.Item1[8] - 128)) * -1).ToString() : result.Item1[8].ToString();
                    }
                    else
                    {
                        TemperatureBlock1Lb1.Text = "Error";
                        TemperatureBlock2Lb2.Text = "Error";
                    }

                }
                else
                {
                    DisplayLog("Нет ответа!");
                }

            }
            else
            {
                DisplayLog("Ошибка при отправке посылки!");
            }
        }

        private void Ks1_help_Db_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Equals(Ks1HelpDbButton.Foreground, Brushes.Red))
                {
                    return;
                }
                var temp = UseManuallyTemp1Cb.IsChecked == true ? Convert.ToInt32(TempManuallyBlock1Iud.Value) : Convert.ToInt32(TemperatureBlock1Lb1.Text);
                double db = Convert.ToDouble(Ks1HelpDb.Value);
                int delta = Convert.ToInt32(Ks1Delta.Value);
                _weaknessWindow.Autosave(1, temp, db, delta, CalibFreq.Value);

            }
            catch 
            {
                MessageBox.Show("Ошибка, сначала откройте нужный файл ослабления!");
                DisplayLog("Ошибка, сначала откройте нужный файл ослабления!");
            }
        }

        private void Ks2_help_Db_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Equals(Ks2HelpDbButton.Foreground, Brushes.Red))
                {
                    return;
                }
                var temp = UseManuallyTemp1Cb.IsChecked == true ? Convert.ToInt32(TempManuallyBlock1Iud.Value) : Convert.ToInt32(TemperatureBlock1Lb1.Text);
                double db = Convert.ToDouble(Ks2HelpDb.Value);
                int delta = Convert.ToInt32(Ks2Delta.Value);
                _weaknessWindow.Autosave(2, temp, db, delta, CalibFreq.Value);

            }
            catch 
            {
                MessageBox.Show("Ошибка, сначала откройте нужный файл ослабления!");
                DisplayLog("Ошибка, сначала откройте нужный файл ослабления!");
            }
        }

        private void Get_help_Db_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Equals(GetHelpDbButton.Foreground, Brushes.Red))
                {
                    return;
                }
                var temp = UseManuallyTemp2Cb.IsChecked == true ? Convert.ToInt32(TempManuallyBlock2Iud.Value) : Convert.ToInt32(TemperatureBlock2Lb2.Text);
                double db = Convert.ToDouble(GetHelpDb.Value);
                int delta = Convert.ToInt32(GetDelta.Value);
                _weaknessWindow.Autosave(3, temp, db, delta, CalibFreq.Value);

            }
            catch 
            {
                MessageBox.Show("Ошибка, сначала откройте нужный файл ослабления!");
                DisplayLog("Ошибка, сначала откройте нужный файл ослабления!");
            }
        }

        private void help_Db_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SearchInFile != null && SearchInFile.IsChecked == true)
            {
                SearchInFile_Checked(null, null);
            }
        }

        private void TempManually1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SearchInFile != null && SearchInFile.IsChecked == true &&
                (Convert.ToBoolean(UseManuallyTemp1Cb.IsChecked)))
            {
                SearchInFile_Checked(null, null);
            }
        }

        private void TempManually2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SearchInFile != null && SearchInFile.IsChecked == true &&
                (Convert.ToBoolean(UseManuallyTemp2Cb.IsChecked)))
            {
                SearchInFile_Checked(null, null);
            }
        }

        private object _focusedElement = 0;
        private new void GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedElement = sender;
        }

        private void DeleteLog_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Document.Blocks.Clear();
            LogBox.Document.Blocks.Add(new Paragraph(new Run("Log:")));
        }
    }
}

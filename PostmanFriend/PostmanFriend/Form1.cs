using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using PostmanFriend.Protocols;
using System.Configuration;
using System.Collections;
using PostmanFriend.GameScripts;
using PostmanFriend.Protocols.Baccarat;
using PostmanFriend.Protocols.PushChess;
using PostmanFriend.Protocols.Pisces2;
using PostmanFriend.Protocols.BQ.Request;
using PostmanFriend.Protocols.MiaoJi;
using PostmanFriend.Protocols.TreasureBowl;
using PostmanFriend.Protocols.Vegas;
using System.Text.RegularExpressions;
using System.Globalization;
using PostmanFriend.Buffer;//本來要做玩家帳號是否重複的比對(第三方註冊引起的資料庫資料重複)，但已經用postman軟體解決
using System.Security.Cryptography;

namespace PostmanFriend
{
    public partial class Form1 : Form
    {
        string Username;
        string Password;
        string token;
        private int ZoneType;
        private int delayTime;

        string ApiGatewayUnity;
        readonly List<DailyRoutineScore> DailyRoutineScoreList = new List<DailyRoutineScore>();
        string playerScoresDataPath;
        string area;

        class DailyRoutineScore
        {
            public string GameName;//遊戲名稱
            public long GoldBefore;//遊戲前鑽幣
            public long GoldAfter;//遊戲後鑽幣
            public long TotalBet;//總押注
            public long TotalScore;//總得點
            public long TotalWin;//總勝點
            public long GoldSpan;//鑽幣變動 = 總勝點
        }

        //App.config內容
        //帳號，密碼，遊戲，高級區(富豪區)，機台

        class Student
        {
            public int id;
        }

        //身分證號驗證
        private bool CheckOldResidentID(string firstLetter, string secondLetter, string num)
        {
            ///建立字母對應表(A~Z)
            ///A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            ///P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo =
                $"{alphabet.IndexOf(firstLetter) + 10}" +
                $"{(alphabet.IndexOf(secondLetter) + 10) % 10}" +
                $"{num}";
            int[] idNoArray = transferIdNo.ToCharArray()
                                          .Select(c => Convert.ToInt32(c.ToString()))
                                          .ToArray();

            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
            {
                sum += weight[i] * idNoArray[i + 1];
            }
            return (sum % 10 == 0);
        }

        private bool CheckNewResidentID(string firstLetter, string num)
        {
            ///建立字母對應表(A~Z)
            ///A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            ///P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo = $"{(alphabet.IndexOf(firstLetter) + 10)}" +
                                  $"{num}";
            int[] idNoArray = transferIdNo.ToCharArray()
                                          .Select(c => Convert.ToInt32(c.ToString()))
                                          .ToArray();

            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
            {
                sum += (weight[i] * idNoArray[i + 1]) % 10;
            }
            return (sum % 10 == 0);
        }

        public Form1()
        {
            InitializeComponent();

            #region 牌型驗證(練習)
            List<Student> students = new List<Student>()
            {
                new Student(){ id = 11 },
                new Student(){ id = 24 },
                new Student(){ id = 12 },
                new Student(){ id = 25 },
                new Student(){ id = 13 },
                new Student(){ id = 53 },
                new Student(){ id = 54 },
            };


            //取得手牌中有幾組3條
            int a = students.GroupBy(x => x.id % 13).Where(g => g.Count() == 3).Count();
            Console.WriteLine("a = " + a);

            IEnumerable<IGrouping<int, int>> query = students.GroupBy(x => x.id % 13, x => x.id);
            //分類
            //foreach (var item in query)
            //{
            //    //大類0~12，J = 11，Q = 12，K = 0
            //    Console.WriteLine("Key = " + item.Key);

            //    //撲克牌
            //    //梅花，方塊，紅心，黑桃
            //    foreach (int item2 in item) {
            //        //Console.WriteLine(item2);
            //    }

            //    Console.WriteLine(item.Count());
            //}

            //先將手牌依照數字分群組0~12
            //從群組中(0~12)找出該數字有2張的(ex.梅花9、紅心9)

            int count = students.GroupBy(x => x.id % 13, x => x.id).Where(g => g.Count() == 2).Count();

            if (students.Exists(x => x.id == 53) && students.Exists(x => x.id == 54))
            {
                Console.WriteLine("兩張鬼牌");

            }
            else if (students.Exists(x => x.id == 53) || students.Exists(x => x.id == 54))
            {
                Console.WriteLine("一張鬼牌");

            }
            else
            {
                Console.WriteLine("零張鬼牌");
                if (students.GroupBy(x => x.id % 13, x => x.id).Where(g => g.Count() == 2).Count() == 2)
                {

                }
                else if (students.GroupBy(x => x.id % 13, x => x.id).Where(g => g.Count() == 2).Count() == 3) { }
                else if (students.GroupBy(x => x.id % 13, x => x.id).Where(g => g.Count() == 2).Count() == 4) { }
            }
            #endregion

            #region Regex(練習)
            //NumberFormatInfo nfi = NumberFormatInfo.CurrentInfo;

            //// Define the regular expression pattern.
            //string pattern;
            //pattern = @"^\s*[";
            //// Get the positive and negative sign symbols.
            //pattern += Regex.Escape(nfi.PositiveSign + nfi.NegativeSign) + @"]?\s?";
            //// Get the currency symbol.
            //pattern += Regex.Escape(nfi.CurrencySymbol) + @"?\s?";
            //// Add integral digits to the pattern.
            //pattern += @"(\d*";
            //// Add the decimal separator.
            //pattern += Regex.Escape(nfi.CurrencyDecimalSeparator) + "?";
            //// Add the fractional digits.
            //pattern += @"\d{";
            //// Determine the number of fractional digits in currency values.
            //pattern += nfi.CurrencyDecimalDigits.ToString() + "}?){1}$";

            //Regex rgx = new Regex(pattern);
            //Console.WriteLine(pattern);
            //// Define some test strings.
            //string[] tests = { "-42", "19.99", "0.001", "100 USD",
            //             ".34", "0.34", "1,052.21", "$10.62",
            //             "+1.43", "-$0.23" };

            //// Check each test string against the regular expression.
            //foreach (string test in tests)
            //{
            //    if (rgx.IsMatch(test))
            //        Console.WriteLine("{0} is a currency value.", test);
            //    else
            //        Console.WriteLine("{0} is not a currency value.", test);
            //}


            //Console.WriteLine("請輸入手機電話號碼:");
            //string tel = "A123456788";
            //bool telcheck = Regex.IsMatch(tel, @"^[A-Z]{1}[1-2]{1}[0-9]{8}");//規則:09開頭，後面接著8個0~9的數字，@是避免跳脫字元
            //                                                                 //isMatch回傳布林值T或F
            //List<string> country = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "W", "Z", "I", "O" };
            //if (telcheck)
            //{
            //    char[] arr = tel.ToCharArray();
            //    int k = country.FindIndex(x => x == tel.Substring(0, 1)) + 10;//身分證規則A = 10, B = 11, ....I = 34, O = 35
            //    Console.WriteLine("英文代表的數字 = " + k);
            //    int Esum = (k / 10) + (k % 10) * 9;

            //    int Nsum = 0;
            //    for (int i = 1; i < 9; i++)
            //    {
            //        Nsum += Convert.ToInt32(tel[i].ToString()) * (9 - i);
            //    }
            //    Nsum += Convert.ToInt32(tel[9].ToString());

            //    if ((Esum + Nsum) % 10 == 0)
            //    {
            //        Console.WriteLine("身分證格式正確");
            //    }
            //    else
            //    {
            //        Console.WriteLine("身分證格式錯誤");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("電話格式恩丟喔");
            //}
            #endregion

            //Console.WriteLine(CheckOldResidentID("S", "C", "30239893"));
            //Console.WriteLine(CheckNewResidentID("S", "C", "30239893"));



            if (!Directory.Exists(Application.StartupPath + @"/PlayerScoresFolder"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"/PlayerScoresFolder");
            }

            if (!Directory.Exists(Application.StartupPath + @"/BQ_Request"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"/BQ_Request");
            }

            comboBox1.Items.Add("金鑽水果盤");
            comboBox1.Items.Add("北歐諸神");
            comboBox1.Items.Add("赤壁三國");
            comboBox1.Items.Add("藍鑽7PK紅");
            comboBox1.Items.Add("招財喵吉");
            comboBox1.Items.Add("聚寶盆");
            comboBox1.Items.Add("賭城狂歡");
            comboBox1.Items.Add("百家樂");
            comboBox1.Items.Add("推筒子");
            comboBox1.Items.Add("德州撲克");
            comboBox1.Items.Add("台灣麻將");
            comboBox1.Items.Add("小瑪莉");
            comboBox1.Items.Add("賓果Bingo");
            comboBox1.Items.Add("女神星光");
            comboBox2.Enabled = false;
            comboBox3.Enabled = false;
        }


        #region UI部分
        /// <summary>
        /// 遊戲選擇
        /// </summary>
        /// <returns></returns>
        private async void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox1_SelectedIndexChanged();
        }

        async void ComboBox1_SelectedIndexChanged()
        {
            Dictionary<string, string> header;

            comboBox2.Enabled = false;
            comboBox3.Enabled = false;
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            PlayerInfo playerInfo = new PlayerInfo();
            object body1 = new
            {
                Username,
                Password
            };
            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5API = await playerInfo.GetAuthenticate("https://" + ApiGatewayUnity, "/identity-player-api/Users/Authenticate?passwordFormat=md5", body1);
            string token = authenticatePasswordFormatMd5API.token;

            if (comboBox1.Text == "金鑽水果盤")
            {
                Fruit fruit = new Fruit();
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                GetMachineListAPI getMachineListAPI = await fruit.GetMachineList("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/GetMachineList?zonetype=High", header);
                List<long> idleMachines = GetIdleMachines(getMachineListAPI);
                foreach (long item in idleMachines)
                {
                    comboBox2.Items.Add(item);
                }

                int betMin = 80;
                int betCount = 10;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }

                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
            else if (comboBox1.Text == "北歐諸神")
            {
                Ragnarok ragnarok = new Ragnarok();
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                GetMachineListAPI getMachineListAPI = await ragnarok.GetMachineList("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/GetMachineList?zonetype=High", header);
                List<long> idleMachines = GetIdleMachines(getMachineListAPI);
                foreach (long item in idleMachines)
                {
                    comboBox2.Items.Add(item);
                }

                int betMin = 90;
                int betCount = 10;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }

                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
            else if (comboBox1.Text == "赤壁三國")
            {
                Chibi chibi = new Chibi();
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                GetMachineListAPI getMachineListAPI = await chibi.GetMachineList("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/GetMachineList?zonetype=High", header);
                List<long> idleMachines = GetIdleMachines(getMachineListAPI);
                foreach (long item in idleMachines)
                {
                    comboBox2.Items.Add(item);
                }

                int betMin = 100;
                int betCount = 10;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }

                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
            else if (comboBox1.Text == "藍鑽7PK紅")
            {
                PK7Red pK7Red = new PK7Red();
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                GetMachineListAPI getMachineListAPI = await pK7Red.GetMachineList("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/GetMachineList?zonetype=High", header);
                List<long> idleMachines = GetIdleMachines(getMachineListAPI);
                foreach (long item in idleMachines)
                {
                    comboBox2.Items.Add(item);
                }

                int betMin = 100;
                int betCount = 5;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }

                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
            else if (comboBox1.Text == "招財喵吉")
            {
                int betMin = 300;
                int betCount = 3;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }
            }
            else if (comboBox1.Text == "聚寶盆")
            {
                int betMin = 300;
                int betCount = 3;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }
            }
            else if (comboBox1.Text == "賭城狂歡")
            {
                int betMin = 300;
                int betCount = 3;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }
            }
            else if (comboBox1.Text == "百家樂")
            {
                comboBox3.Items.Add(100);
                comboBox3.Items.Add(500);
                comboBox3.Items.Add(2000);
                comboBox3.Items.Add(5000);
            }
            else if (comboBox1.Text == "推筒子")
            {
                comboBox3.Items.Add(100);
                comboBox3.Items.Add(500);
                comboBox3.Items.Add(1000);
                comboBox3.Items.Add(5000);
                comboBox3.Items.Add(10000);
                comboBox3.Items.Add(50000);
            }
            else if (comboBox1.Text == "德州撲克")
            {
                //看遊戲流程跟API再決定
            }
            else if (comboBox1.Text == "台灣麻將")
            {
                comboBox3.Items.Add(9000);
                comboBox3.Items.Add(15000);
                comboBox3.Items.Add(30000);
                comboBox3.Items.Add(60000);
            }
            else if (comboBox1.Text == "小瑪莉")
            {
                //看遊戲流程跟API再決定
            }
            else if (comboBox1.Text == "賓果Bingo")
            {
                //看遊戲流程跟API再決定
            }
            else if (comboBox1.Text == "女神星光")
            {
                int betMin = 100;
                int betCount = 10;
                for (int i = 0; i < betCount; i++)
                {
                    comboBox3.Items.Add(betMin * (i + 1));
                }
            }
        }

        List<long> GetIdleMachines(GetMachineListAPI getMachineListAPI)
        {
            List<long> idleMachines = new List<long>();
            List<long> otherMachines = new List<long>();

            foreach (var item in getMachineListAPI.svData.otherMachineList)
            {
                otherMachines.Add(item.machineNo);
            }
            otherMachines.Sort();

            if (getMachineListAPI.svData.ownMachineInfo != null)
            {
                long ownMachine = getMachineListAPI.svData.ownMachineInfo.machineNo;
                idleMachines.Add(ownMachine);
            }

            for (int i = 0; i < 30; i++)
            {
                if (!otherMachines.Contains(i + 1))
                {
                    idleMachines.Add(i + 1);
                }
            }
            idleMachines.Sort();


            return idleMachines;
        }

        /// <summary>
        /// 機台選擇
        /// </summary>
        /// <returns></returns>
        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = false;

            string message = "something about...";
            int machineNo = int.Parse(comboBox2.Text);
            string TransferCode = null;
            object body2 = new
            {
                ZoneType,
                MachineNo = machineNo,
                TransferCode
            };
            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };

            if (comboBox1.Text == "金鑽水果盤")
            {
                Fruit fruit = new Fruit();
                await fruit.EnterMachine("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/EnterMachine", body2, header);
            }
            else if (comboBox1.Text == "北歐諸神")
            {
                Fruit fruit = new Fruit();
                await fruit.EnterMachine("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/EnterMachine", body2, header);
            }
            else if (comboBox1.Text == "赤壁三國")
            {
                Fruit fruit = new Fruit();
                await fruit.EnterMachine("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/EnterMachine", body2, header);
            }
            else if (comboBox1.Text == "藍鑽7PK紅")
            {
                Fruit fruit = new Fruit();
                await fruit.EnterMachine("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/EnterMachine", body2, header);
            }
            else if (comboBox1.Text == "招財喵吉")
            {

            }
            else if (comboBox1.Text == "聚寶盆")
            {

            }
            else if (comboBox1.Text == "賭城狂歡")
            {

            }
            else if (comboBox1.Text == "百家樂")
            {

            }
            else if (comboBox1.Text == "推筒子")
            {

            }
            else if (comboBox1.Text == "德州撲克")
            {

            }
            else if (comboBox1.Text == "台灣麻將")
            {

            }
            else if (comboBox1.Text == "小瑪莉")
            {

            }
            else if (comboBox1.Text == "賓果Bingo")
            {

            }
            else if (comboBox1.Text == "女神星光")
            {

            }


            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show("機台已被占用，請重新選擇！");
                ComboBox1_SelectedIndexChanged();
            }

            comboBox1.Enabled = true;
        }

        /// <summary>
        /// 下注額選擇
        /// </summary>
        /// <returns></returns>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 圖片按鈕
        /// </summary>
        /// <returns></returns>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            string[] talk = { "喵嗚~~", "喵~", "嘶~~~", "呼嚕~ 呼嚕~", "冠賢葛格好棒", "依萍美眉好棒", "咚咚咚..." };
            int tIndex = rnd.Next(talk.Length);
            listBox1.Items.Add(talk[tIndex]);
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            string[] talk = { "按三小!?" };
            int tIndex = rnd.Next(talk.Length);
            MessageBox.Show(talk[tIndex], "洛克人：");
            //listBox1.Items.Add(talk[tIndex]);
            //listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        /// <summary>
        /// 玩一把
        /// </summary>
        /// <returns></returns>
        private async void button1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 自動驗證
        /// </summary>
        /// <returns></returns>
        private async void button2_Click(object sender, EventArgs e)
        {
            DailyRoutineScoreList.Clear();
            //area = "test";
            //ApiGatewayUnity = "test-apigateway-unity.diamondonline.com.tw";
            //Username = textBox1.Text;
            //Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(textBox2.Text))).Replace("-", null);
            //ZoneType = 2;


            area = "prod";
            ApiGatewayUnity = "prod-apigateway-unity.diamondonline.com.tw";
            Username = textBox1.Text;
            Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(textBox2.Text))).Replace("-", null);
            ZoneType = 2;
            Console.WriteLine("Md5 Password = " + Password);
            PlayerInfo playerInfo = new PlayerInfo();
            object body1 = new
            {
                Username,
                Password
            };

            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5API = await playerInfo.GetAuthenticate("https://" + ApiGatewayUnity, "/identity-player-api/Users/Authenticate?passwordFormat=md5", body1);
            long id = authenticatePasswordFormatMd5API.id;
            string token = authenticatePasswordFormatMd5API.token;
            long bet;
            delayTime = 1000;
            playerScoresDataPath = Application.StartupPath + @"/" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");

            //bet = 80;
            //await PlayFruit(bet, id, token, ZoneType);
            //Thread.Sleep(10);

            //bet = 90;
            //await PlayRagnarok(bet, id, token, ZoneType);
            ////Thread.Sleep(10);

            //bet = 100;
            //await PlayChibi(bet, id, token, ZoneType);
            ////Thread.Sleep(10);

            //bet = 100;
            //await PlayPK7Red(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 300;
            //await PlayMiaoJi(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 300;
            //await PlayTreasureBowl(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 300;
            //await PlayVegas(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 100;
            //await PlayBaccarat(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 100;
            //await PlayPushChess(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 10;
            //await PlayFarmerMario(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 100;
            //await PlayBingoBingo(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 100;
            //await PlayMuseStar(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //bet = 1;
            //await PlayPisces2(bet, id, token, ZoneType);
            //Thread.Sleep(10000);

            //if (area == "prod")
            //{
            //    await BQVerify(Username, DailyRoutineScoreList);
            //}

            //bet = 1;
            //await PlayPisces2ForTest(bet, id, token, ZoneType);
        }
        
        //柏青斯洛 撿預告
        private async void button3_Click(object sender, EventArgs e)
        {
            //area = "test";
            //ApiGatewayUnity = "test-apigateway-unity.diamondonline.com.tw";
            area = "prod";
            ApiGatewayUnity = "prod-apigateway-unity.diamondonline.com.tw";
            Username = textBox1.Text;
            Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(textBox2.Text))).Replace("-", null);
            ZoneType = 2;

            PlayerInfo playerInfo = new PlayerInfo();
            object body1 = new
            {
                Username,
                Password
            };

            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5API = await playerInfo.GetAuthenticate("https://" + ApiGatewayUnity, "/identity-player-api/Users/Authenticate?passwordFormat=md5", body1);
            string token = authenticatePasswordFormatMd5API.token;
            long bet;
            delayTime = 2000;
            playerScoresDataPath = Application.StartupPath + @"/" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");

            bet = 300;
            await FindBB(token, ZoneType);
            Thread.Sleep(1000);
        }

        //聚寶盆轉到120
        private async void button4_Click(object sender, EventArgs e)
        {
            //area = "test";
            //ApiGatewayUnity = "test-apigateway-unity.diamondonline.com.tw";
            area = "prod";
            ApiGatewayUnity = "prod-apigateway-unity.diamondonline.com.tw";
            Username = textBox1.Text;
            Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(textBox2.Text))).Replace("-", null);

            PlayerInfo playerInfo = new PlayerInfo();
            object body1 = new
            {
                Username,
                Password
            };

            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5API = await playerInfo.GetAuthenticate("https://" + ApiGatewayUnity, "/identity-player-api/Users/Authenticate?passwordFormat=md5", body1);
            long id = authenticatePasswordFormatMd5API.id;
            string token = authenticatePasswordFormatMd5API.token;

            await FindUnder120(id, token);
            Thread.Sleep(1000);
        }

        //雙魚座2活動 策略測試
        private async void button5_Click(object sender, EventArgs e)
        {
            area = "prod";
            ApiGatewayUnity = "prod-apigateway-unity.diamondonline.com.tw";
            Username = textBox1.Text;
            Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(textBox2.Text))).Replace("-", null);
            ZoneType = 3;

            PlayerInfo playerInfo = new PlayerInfo();
            object body1 = new
            {
                Username,
                Password
            };

            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5API = await playerInfo.GetAuthenticate("https://" + ApiGatewayUnity, "/identity-player-api/Users/Authenticate?passwordFormat=md5", body1);
            long id = authenticatePasswordFormatMd5API.id;
            string token = authenticatePasswordFormatMd5API.token;
            long bet;
            delayTime = 2000;
            playerScoresDataPath = Application.StartupPath + @"/" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");

            bet = 200;
            await PlayPisces2Activity(bet, id, token, ZoneType);
            Thread.Sleep(1000);
        }
        #endregion

        #region 遊戲下注
        async Task<long> GetGold(long id, string token)
        {
            long gold = -1;
            PlayerInfo playerInfo = new PlayerInfo();
            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token }
            };
            GetPlayerInfo_ByUidAPI getPlayerInfo_ByUidAPI = await playerInfo.GetPlayerInfo("https://" + ApiGatewayUnity, "/back-api/Member/GetPlayerInfo_ByUid", id, header);
            gold = getPlayerInfo_ByUidAPI.Data.iPoints;

            return gold;
        }

        /// <summary>
        /// 金鑽水果盤
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayFruit(long bet, long id, string token, long zoneType)
        {
            string gameName = "金鑽水果盤";

            long Bet = bet;
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Fruit fruit = new Fruit();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            bool success = false;
            int myMachineNo = 1;

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await fruit.GetMachineList("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/GetMachineList?zonetype=High", header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;

                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                string TransferCode = null;
                object body2 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    TransferCode
                };
                success = await fruit.EnterMachine("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/EnterMachine", body2, header);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            int count = 0;
            while (playCount > count)
            {
                object body3 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Bet
                };
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                long score = await fruit.Spin("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/StartGame", body3, header);

                if (score >= 0)
                {
                    count += 1;
                    totalScore += score;
                    listBox1.Items.Add("第" + count + "場得分：" + score);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("API請求失敗！");
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                await Task.Delay(delayTime);
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            await fruit.LeaveGame("https://" + ApiGatewayUnity, "/Fruit-api/Fruit/LeaveGame", header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 北歐諸神
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayRagnarok(long bet, long id, string token, long zoneType)
        {
            string gameName = "北歐諸神";

            string Bet = "{\"Id\":1,\"Amount\":" + bet + "}";
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Ragnarok ragnarok = new Ragnarok();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            bool success = false;
            int myMachineNo = 1;

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await ragnarok.GetMachineList("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/GetMachineList?zonetype=High", header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;
                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                string TransferCode = null;
                object body2 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    TransferCode
                };
                success = await ragnarok.EnterMachine("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/EnterMachine", body2, header);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            int count = 0;
            while (playCount > count)
            {
                object body3 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Bet
                };
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                long score = await ragnarok.Spin("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/StartGame", body3, header);

                if (score >= 0)
                {
                    count += 1;
                    totalScore += score;
                    listBox1.Items.Add("第" + count + "場得分：" + score);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("API請求失敗！");
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                await Task.Delay(delayTime);
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            await ragnarok.LeaveGame("https://" + ApiGatewayUnity, "/Ragnarok-api/Ragnarok/LeaveGame", header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 赤壁三國
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayChibi(long bet, long id, string token, long zoneType)
        {
            string gameName = "赤壁三國";

            long Bet = bet;
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Chibi chibi = new Chibi();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            int myMachineNo = 1;

            bool success = false;
            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await chibi.GetMachineList("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/GetMachineList?zonetype=High", header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;

                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                string TransferCode = null;
                object body2 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    TransferCode
                };
                success = await chibi.EnterMachine("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/EnterMachine", body2, header);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            int count = 0;
            while (playCount > count)
            {
                object body3 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Bet
                };
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                long score = await chibi.Spin("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/StartGame", body3, header);

                if (score >= 0)
                {
                    count += 1;
                    totalScore += score;
                    listBox1.Items.Add("第" + count + "場得分：" + score);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("API請求失敗！");
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                await Task.Delay(delayTime);
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            await chibi.LeaveGame("https://" + ApiGatewayUnity, "/RedCliff-api/RedCliff/LeaveGame", header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 7PK紅
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayPK7Red(long bet, long id, string token, long zoneType)
        {
            string gameName = "7PK紅";

            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            PK7Red pK7Red = new PK7Red();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };

            int myMachineNo = 1;
            bool success = false;
            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await pK7Red.GetMachineList("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/GetMachineList?zonetype=High", header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;

                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                string TransferCode = null;
                object body2 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    TransferCode
                };
                success = await pK7Red.EnterMachine("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/EnterMachine", body2, header);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            while (playCount > count)
            {
                //StartGame
                object body3 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Bet = bet
                };
                Console.WriteLine(zoneType + " = " + myMachineNo + " = " + bet);
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "2.0.0" }
                };
                long Id = await pK7Red.FirstSpin("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/StartGame", body3, header);
                await Task.Delay(delayTime);

                //Add
                int Phase = 2;
                object body4 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Id,
                    Phase
                };
                await pK7Red.AddSpin("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/AddBet", body4, header);
                await Task.Delay(delayTime);
                Console.WriteLine("Add bet");
                //Add
                Phase = 3;
                object body5 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Id,
                    Phase
                };
                await pK7Red.AddSpin("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/AddBet", body5, header);
                await Task.Delay(delayTime);
                Console.WriteLine("Add bet");
                //Add
                Phase = 4;
                object body6 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Id,
                    Phase
                };
                await pK7Red.AddSpin("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/AddBet", body6, header);
                await Task.Delay(delayTime);
                Console.WriteLine("Add bet");
                //Get Result
                object body7 = new
                {
                    ZoneType = zoneType,
                    MachineNo = myMachineNo,
                    Id,
                    BetTotal = bet * 4
                };
                long score = await pK7Red.GetResult("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/GetResult", body7, header);
                await Task.Delay(delayTime);
                Console.WriteLine("GetResult");
                if (score >= 0)
                {
                    count += 1;
                    totalScore += score;
                    listBox1.Items.Add("第" + count + "場得分：" + score);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 4 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "2.0.0" }
            };
            await pK7Red.LeaveGame("https://" + ApiGatewayUnity, "/PK7R-api/PK7R/LeaveGame", header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 招財喵吉
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayMiaoJi(long bet, long id, string token, long zoneType)
        {
            string gameName = "招財喵吉";

            long Bet = bet;
            bool success = false;
            int myMachineNo = 1;
            long Award = 0;//Award = 4，表示上一局是青再連線
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            MiaoJi miaoJi = new MiaoJi();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await miaoJi.GetMachineList("https://" + ApiGatewayUnity, "/pachislot-LuckyCat-api/MachineLobby?zoneid=" + zoneType, header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;
                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    //從1號機台開始找空機台進入
                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                success = await miaoJi.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LuckyCat-api/machine/?access_token=" + token, zoneType, myMachineNo);
                if (success)
                {
                    Console.WriteLine("myMachineNo " + myMachineNo);
                    Award = await miaoJi.GetMachineInfo();
                }

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long score = await miaoJi.Spin(bet);

                    if (score >= 0)
                    {
                        count += 1;

                        //上一局的玩家轉到青再連線，沒使用就離開機台
                        if (Award != 4)
                        {
                            totalBet += bet;
                        }
                        else
                        {
                            listBox1.Items.Add("青再連線");
                            Award = 0;
                        }

                        //如果轉到青再連線，記錄起來
                        if (score == 4)
                        {
                            score = 0;
                            Award = 4;
                            listBox1.Items.Add("下一局青再連線");
                        }

                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }

            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            //強連線斷線
            await miaoJi.Leave();

            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            await miaoJi.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LuckyCat-api/LetItGo", zoneType, header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 聚寶盆
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayTreasureBowl(long bet, long id, string token, long zoneType)
        {
            string gameName = "聚寶盆";

            long Bet = bet;
            bool success = false;
            int myMachineNo = 1;
            long Award = 0;//Award = 4，表示上一局是青再連線
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            TreasureBowl treasureBowl = new TreasureBowl();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await treasureBowl.GetMachineList("https://" + ApiGatewayUnity, "/pachislot-TreasureBowl-api/MachineLobby?zoneid=" + zoneType, header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;
                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                success = await treasureBowl.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-TreasureBowl-api/machine/?access_token=" + token, zoneType, myMachineNo);
                if (success)
                {
                    Console.WriteLine("myMachineNo " + myMachineNo);
                    Award = await treasureBowl.GetMachineInfo();
                }

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long score = await treasureBowl.Spin(bet);

                    if (score >= 0)
                    {
                        count += 1;

                        //上一局的玩家轉到青再連線，沒使用就離開機台
                        if (Award != 4)
                        {
                            totalBet += bet;
                        }
                        else
                        {
                            listBox1.Items.Add("青再連線");
                            Award = 0;
                        }

                        //如果轉到青再連線，記錄起來
                        if (score == 4)
                        {
                            score = 0;
                            Award = 4;
                            listBox1.Items.Add("下一局青再連線");
                        }

                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }

            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            await treasureBowl.Leave();

            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            await treasureBowl.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-TreasureBowl-api/LetItGo", zoneType, header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 賭城狂歡
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayVegas(long bet, long id, string token, long zoneType)
        {
            string gameName = "賭城狂歡";

            long Bet = bet;
            bool success = false;
            int myMachineNo = 1;
            long Award = 0;//Award = 4，表示上一局是青再連線
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Vegas vegas = new Vegas();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await vegas.GetMachineList("https://" + ApiGatewayUnity, "/pachislot-LetsGoToVegas-api/MachineLobby?zoneid=" + zoneType, header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;
                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                success = await vegas.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LetsGoToVegas-api/machine/?access_token=" + token, zoneType, myMachineNo);
                if (success)
                {
                    Console.WriteLine("myMachineNo " + myMachineNo);
                    Award = await vegas.GetMachineInfo();
                }

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long score = await vegas.Spin(bet);

                    if (score >= 0)
                    {
                        count += 1;

                        //上一局的玩家轉到青再連線，沒使用就離開機台
                        if (Award != 4)
                        {
                            totalBet += bet;
                        }
                        else
                        {
                            Award = 0;
                            listBox1.Items.Add("青再連線");
                        }

                        //如果轉到青再連線，記錄起來
                        if (score == 4)
                        {
                            score = 0;
                            Award = 4;
                            listBox1.Items.Add("下一局青再連線");
                        }

                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }

            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            await vegas.Leave();

            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            await vegas.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LetsGoToVegas-api/LetItGo", zoneType, header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 百家樂
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayBaccarat(long bet, long id, string token, long zoneType)
        {
            string gameName = "百家樂";

            long Bet = bet;
            long playCount = 2;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Baccarat baccarat = new Baccarat();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇牌桌進入
            string groupName = "";
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token }
            };

            bool success = false;
            while (!success)
            {
                List<BaccaratTablesAPI> baccaratTablesAPI = await baccarat.GetTableList("https://" + ApiGatewayUnity, "/Baccarat-api/Back/Tables/2", header);
                foreach (var item in baccaratTablesAPI)
                {
                    if (item.userCount < item.userMaxCount)
                    {
                        groupName = item.groupName;
                        break;
                    }
                }

                //進入機台(Post)
                success = await baccarat.ConnectVerify("wss://" + ApiGatewayUnity + "/Baccarat-api/Baccarat_ClientHub/?access_token=" + token, groupName);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long score = -1;

                    if (count % 2 == 0)
                    {
                        long[] Bets = new long[] { 0, bet, bet, bet, bet, bet };
                        score = await baccarat.Spin(Bets);
                    }
                    else
                    {
                        long[] Bets = new long[] { bet, 0, bet, bet, bet, bet };
                        score = await baccarat.Spin(Bets);
                    }

                    if (score >= 0)
                    {
                        count += 1;
                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 5 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            await baccarat.LeaveGame();
            #endregion

            return gameName;
        }

        /// <summary>
        /// 推筒子
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayPushChess(long bet, long id, string token, long zoneType)
        {
            string gameName = "推筒子";

            long Bet = bet;
            long playCount = 2;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            PushChess pushChess = new PushChess();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇牌桌進入
            //進入機台(Post)
            bool success = false;
            while (!success)
            {
                success = await pushChess.ConnectVerify("wss://" + ApiGatewayUnity + "/pushchess-api/ClientHub/?access_token=" + token, zoneType);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    string Bets = "{\"Chu\":" + bet + ",\"Chuan\":" + bet + ",\"Wei\":" + bet + "}";
                    long score = await pushChess.Spin(Bets);

                    if (score >= 0)
                    {
                        count += 1;
                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 3 * playCount;
            totalWin = totalScore / 2 * 195 / 100 - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            await pushChess.LeaveGame();
            #endregion

            return gameName;
        }

        /// <summary>
        /// 小瑪莉
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayFarmerMario(long bet, long id, string token, long zoneType)
        {
            string gameName = "小瑪莉";

            long Bet = bet;
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            FarmerMario farmerMario = new FarmerMario();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            int myMachineNo = 1;
            bool success = false;

            while (!success)
            {
                GetMachineListAPI getMachineListAPI = await farmerMario.GetMachineList("https://" + ApiGatewayUnity, "/FarmerMario-api/MachineLobby?zoneid=" + zoneType, header);
                List<long> machineNos = new List<long>();
                myMachineNo = 1;

                if (getMachineListAPI.svData.otherMachineList.Count > 0)
                {
                    foreach (var item in getMachineListAPI.svData.otherMachineList)
                    {
                        machineNos.Add(item.machineNo);
                    }

                    //將自己佔有的機台移除
                    if (getMachineListAPI.svData.ownMachineInfo != null)
                    {
                        machineNos.Remove(getMachineListAPI.svData.ownMachineInfo.machineNo);
                    }
                    machineNos.Sort();

                    //從1號機台開始找空機台進入
                    for (int i = 0; i < machineNos.Count; i++)
                    {
                        if (myMachineNo == machineNos[i])
                        {
                            myMachineNo += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //進入機台(Post)
                success = await farmerMario.ConnectVerify("wss://" + ApiGatewayUnity + "/FarmerMario-api/machine/?access_token=" + token, zoneType, myMachineNo);

                await Task.Delay(1000);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long[] Bets = new long[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };//0~10注
                    long score = await farmerMario.Spin(Bets);

                    if (score >= 0)
                    {
                        count += 1;
                        totalScore += score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("進入機台失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 10 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            await farmerMario.LeaveGame("https://" + ApiGatewayUnity, "/FarmerMario-api/LetItGo", zoneType, header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// BingoBingo
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayBingoBingo(long bet, long id, string token, long zoneType)
        {
            string gameName = "BingoBingo";

            long Bet = bet;
            long playCount = 1;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            BingoBingo bingoBingo = new BingoBingo();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //進入機台(Post)
            bool success = false;
            while (!success)
            {
                success = await bingoBingo.ConnectVerify("wss://" + "bingo.endpoints.diamondonline-" + area + ".cloud.goog/high/?access_token=" + token);

                await Task.Delay(delayTime);
            }
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            if (success)
            {
                while (playCount > count)
                {
                    long bigOrSmall = 2;//Big = 1, Small = 2
                    long bigOrSmallBetCount = 1;
                    long oddOrEven = 2;//Odd = 1, Even = 2
                    long oddOrEvenBetCount = 1;

                    List<object> superNumber = new List<object>();
                    superNumber.Add(new { Number = 1, Count = 1 });
                    superNumber.Add(new { Number = 6, Count = 1 });
                    superNumber.Add(new { Number = 8, Count = 1 });
                    long[] sixStar = new long[] { 1, 2, 3, 4, 5, 6 };//六星
                    object[] arguments = new object[]
                    {
                        bigOrSmall,
                        bigOrSmallBetCount,
                        oddOrEven,
                        oddOrEvenBetCount,
                        superNumber,
                        sixStar
                    };

                    float score = await bingoBingo.Spin(arguments);
                    if (score >= 0)
                    {
                        count += 1;
                        totalScore += (long)score;
                        listBox1.Items.Add("第" + count + "場得分：" + score);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(delayTime);
                }
            }
            else
            {
                listBox1.Items.Add("連線失敗");
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 15 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 女神星光
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayMuseStar(long bet, long id, string token, long zoneType)
        {
            string gameName = "女神星光";

            long Bet = bet;
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            MuseStar museStar = new MuseStar();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            while (playCount > count)
            {
                object body3 = new
                {
                    ZoneId = zoneType,
                    Bet
                };
                header = new Dictionary<string, string>()
                {
                    {"platform", "WebGLPlayer" },
                    {"authorization", "Bearer " + token },
                    {"version", "1" }
                };

                long score = await museStar.Spin("https://" + ApiGatewayUnity, "/MuseStar-api/Spin", body3, header);
                if (score >= 0)
                {
                    count += 1;
                    totalScore += score;
                    listBox1.Items.Add("第" + count + "場得分：" + score);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }

                await Task.Delay(delayTime);
            }
            #endregion

            #region 存入DailyRoutineScore
            totalBet = bet * 1 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };
            await museStar.LeaveGame("https://" + ApiGatewayUnity, "/MuseStar-api/LetItGo", zoneType, header);
            #endregion

            return gameName;
        }

        /// <summary>
        /// 雙魚座2
        /// </summary>
        /// <returns></returns>
        async Task<string> PlayPisces2(long bet, long id, string token, long zoneType)
        {
            string gameName = "雙魚座2";

            long Bet = bet;
            long playCount = 3;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Pisces2 pisces2 = new Pisces2();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "1.0.1" }
            };
            int myMachineNo = 0;
            bool success = false;

            while (!success)
            {
                List<GetLobbyMachinesHighAPI> getLobbyMachinesHighAPIs = await pisces2.GetMachineList("https://pisces2.endpoints.diamondonline-" + area + ".cloud.goog/api/lobby/machines/High", "", header);
                myMachineNo = 0;

                foreach (var item in getLobbyMachinesHighAPIs)
                {
                    if (item.status == 1)//空機台
                    {
                        myMachineNo = item.machineNo;
                        break;
                    }
                }

                //進入機台(Websocket)
                if (myMachineNo != 0)
                {
                    success = await pisces2.ConnectVerify("wss://pisces2.endpoints.diamondonline-" + area + ".cloud.goog/machine?access_token=" + token, ZoneType, myMachineNo);
                }
                else
                {
                    MessageBox.Show("全機台已被佔滿");
                }

                await Task.Delay(1000);
            }

            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            long credits = 0;

            if (success)
            {
                //credits = await pisces2.AddCredits();//credits = 0 表示錯誤或是玩家沒鑽幣
                while (playCount > count)
                {
                    if (credits <= 59)
                    {
                        credits += await pisces2.AddCredits();
                    }

                    if (credits > 0)
                    {
                        await pisces2.Bet(bet);
                        string round = await pisces2.StartGame(bet);
                        Console.WriteLine(round);
                        await pisces2.DealCard(bet);
                        credits -= bet;
                        if (!string.IsNullOrEmpty(round))
                        {
                            await pisces2.Bet(bet);
                            await pisces2.DealCard(bet);
                            credits -= bet;

                            await pisces2.Bet(bet);
                            await pisces2.DealCard(bet);
                            credits -= bet;

                            await pisces2.Bet(bet);
                            Pisces2StartGameBetResponse pisces2StartGameBetResponse = await pisces2.DealAll(bet);
                            credits -= bet;
                            //牌型驗證，有空再做...
                            //pisces2.PokerTypeJudge(pisces2StartGameBetResponse.pokerInfos);
                            Pisces2StartGameAwardResult pisces2StartGameAwardResult = await pisces2.AwardResult();
                            long score = pisces2StartGameAwardResult.awardWin;

                            if (score >= 0)
                            {
                                count += 1;
                                totalScore += score;
                                credits += score;
                                listBox1.Items.Add("第" + count + "場得分：" + score);
                                listBox1.TopIndex = listBox1.Items.Count - 1;
                            }

                            await pisces2.GameResult();
                        }
                    }
                    else
                    {
                        listBox1.Items.Add("玩家身上鑽幣不足！");
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }

                    await Task.Delay(200);
                }
            }
            #endregion

            #region 存入DailyRoutineScore
            if (!success)
            {
                return gameName;
            }
            totalBet = bet * 4 * playCount;
            totalWin = totalScore - totalBet;

            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            dailyRoutineScore.GameName = gameName;
            dailyRoutineScore.GoldBefore = goldBefore;
            dailyRoutineScore.GoldAfter = goldBefore - totalBet + totalWin;
            dailyRoutineScore.TotalBet = totalBet;
            dailyRoutineScore.TotalScore = totalScore;
            dailyRoutineScore.TotalWin = totalWin;
            dailyRoutineScore.GoldSpan = totalWin;
            DailyRoutineScoreList.Add(dailyRoutineScore);
            #endregion

            #region 離開遊戲
            await pisces2.LeaveMachine();
            #endregion

            return gameName;
        }

        //搜尋日期目前寫死，就是當天例測的金流
        /// <summary>
        /// BQ後台驗證
        /// </summary>
        /// <returns></returns>
        async Task BQVerify(string username, List<DailyRoutineScore> DailyRoutineScoreList)
        {
            string gameName = "BQ後台驗證";

            //後台BQ取資料測試
            BQ bQ = new BQ();
            string YearMonthDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
            BQ_Data bQ_Data = new BQ_Data();
            DailyRoutineScore dailyRoutineScore = new DailyRoutineScore();
            string data;
            BQBatchedDataRequestAPI bQBatchedDataRequestAPI;
            string bQurl = "https://lookerstudio.google.com/embed/batchedDataV2?appVersion=20230130_00020038";

            #region 金鑽水果盤
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "金鑽水果盤");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetFruitScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);
                Console.WriteLine(JsonConvert.SerializeObject(bQBatchedDataRequestAPI));
                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetFruitScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 北歐諸神
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "北歐諸神");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetRagnarokScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetRagnarokScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 赤壁三國
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "赤壁三國");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetChibiScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetFruitScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 7PK紅
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "7PK紅");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetPK7RedScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetPK7RedScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 招財喵吉
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "招財喵吉");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetMiaoJiScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetMiaoJiScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 聚寶盆
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "聚寶盆");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetTreasureBowlScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetTreasureBowlScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 賭城狂歡
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "賭城狂歡");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetVegasScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetVegasScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 百家樂
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "百家樂");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetBaccaratScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetBaccaratScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 推筒子
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "推筒子");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetPushChessScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetPushChessScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 小瑪莉
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "小瑪莉");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetFarmerMarioScoreRequest");
                data = data.Replace("20221215", YearMonthDay).Replace("TA313", username);

                bQ_Data = await bQ.GetFarmerMarioScore(bQurl, "", data);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 賓果
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "BingoBingo");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetBingoBingoScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetBingoBingoScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 女神星光
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "女神星光");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetMuseStarScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetMuseStarScore(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion

            #region 雙魚座2
            //取得BQ的body的json內容 (因為用程式碼寫會太複雜，所以將請求文字先寫入檔案，再讀出修改欲搜尋的日期)
            dailyRoutineScore = DailyRoutineScoreList.Find(x => x.GameName == "雙魚座2");

            if (dailyRoutineScore != null)
            {
                data = File.ReadAllText(Application.StartupPath + @"\BQ_Request\BQ_GetPisces2ScoreRequest");
                bQBatchedDataRequestAPI = JsonConvert.DeserializeObject<BQBatchedDataRequestAPI>(data);

                //設定要搜尋的日期區間
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.dateRanges[0].startDate = YearMonthDay;
                    item.datasetSpec.dateRanges[0].endDate = YearMonthDay;
                }

                //設定要搜尋的帳號
                foreach (var item in bQBatchedDataRequestAPI.dataRequest)
                {
                    item.datasetSpec.filters[0].filterDefinition.filterExpression.stringValues[0] = username;
                }

                bQ_Data = await bQ.GetPisces2Score(bQurl, "", bQBatchedDataRequestAPI);

                if (dailyRoutineScore.TotalBet == bQ_Data.TotalBet &&
                    dailyRoutineScore.TotalScore == bQ_Data.TotalScore &&
                    dailyRoutineScore.TotalWin == bQ_Data.TotalWin)
                {
                    listBox1.Items.Add(dailyRoutineScore.GameName + " 驗證正常！");
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    listBox1.Items.Add("總下注：" + dailyRoutineScore.TotalBet + " " + bQ_Data.TotalBet);
                    listBox1.Items.Add("總得分：" + dailyRoutineScore.TotalScore + " " + bQ_Data.TotalScore);
                    listBox1.Items.Add("總勝點：" + dailyRoutineScore.TotalWin + " " + bQ_Data.TotalWin);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    MessageBox.Show("遊戲：" + dailyRoutineScore.GameName + " 金流異常！");
                }
            }

            await Task.Delay(delayTime);
            #endregion
        }
        #endregion

        #region 右方功能
        /// <summary>
        /// 找BB
        /// </summary>
        /// <returns></returns>
        async Task FindBB(string token, int zoneType)
        {
            string gameName;
            int myMachineNo;
            //long Award = 0;//Award = 4，表示上一局是青再連線

            //進入機台，看300、600、900、3000的Award = 7 8 9
            // 7 = RB, 8 = BB, 9 = 1G連
            //發現後，使用別的帳號進去轉。掃台帳號繼續掃台。
            // -> 轉完後離開
            // -> 掃完台後離開

            //進入1~30台
            //檢查Award = 7 8 9

            Dictionary<string, string> header;

            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };

            #region 喵吉
            gameName = "招財喵吉";
            //娛樂區
            //listBox1.Items.Add(gameName + "娛樂區");
            //listBox1.TopIndex = listBox1.Items.Count - 1;
            //myMachineNo = 1;
            //while (myMachineNo <= 30)
            //{
            //    //進入機台(Post)
            //    MiaoJi miaoJi = new MiaoJi();
            //    bool success = await miaoJi.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LuckyCat-api/machine/?access_token=" + token, 6, myMachineNo);
            //    if (success)
            //    {
            //        MiaoJiStartGameResult2 miaoJiStartGameResult2 = await miaoJi.GetMachineInfoAward();
            //        long Award30 = miaoJiStartGameResult2.BetAndAward[0].Award;
            //        long Award60 = miaoJiStartGameResult2.BetAndAward[1].Award;
            //        long Award90 = miaoJiStartGameResult2.BetAndAward[2].Award;

            //        if (Award30 == 7 || Award30 == 8 || Award30 == 9)
            //        {
            //            //開一個thread，用另一個玩家玩
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[0]);

            //            //ApiGatewayUnity  (進入機台)
            //            //token
            //            //zoneType
            //            //myMachineNo
            //            //Player player = new Player();
            //            //player.Play("TA310", "Fudan168ta11", ApiGatewayUnity,  ZoneType, myMachineNo);
            //            //aac

            //        }

            //        if (Award60 == 7 || Award60 == 8 || Award60 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[1]);
            //        }

            //        if (Award90 == 7 || Award90 == 8 || Award90 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[2]);
            //        }
            //    }

            //    myMachineNo += 1;

            //    await miaoJi.Leave();

            //    await miaoJi.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LuckyCat-api/LetItGo", 6, header);

            //    await Task.Delay(100);
            //}


            listBox1.Items.Add(gameName + "高級區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                MiaoJi miaoJi = new MiaoJi();
                bool success = await miaoJi.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LuckyCat-api/machine/?access_token=" + token, 2, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    MiaoJiStartGameResult2 miaoJiStartGameResult2 = await miaoJi.GetMachineInfoAward();
                    long Award300 = miaoJiStartGameResult2.BetAndAward[0].Award;
                    long Award600 = miaoJiStartGameResult2.BetAndAward[1].Award;
                    long Award900 = miaoJiStartGameResult2.BetAndAward[2].Award;

                    if (Award300 == 7 || Award300 == 8 || Award300 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[0]);

                        //ApiGatewayUnity  (進入機台)
                        //token
                        //zoneType
                        //myMachineNo
                        //Player player = new Player();
                        //player.Play("TA310", "Fudan168ta11", ApiGatewayUnity,  ZoneType, myMachineNo);
                        //aac

                    }

                    if (Award600 == 7 || Award600 == 8 || Award600 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[1]);
                    }

                    if (Award900 == 7 || Award900 == 8 || Award900 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[2]);
                    }
                }

                myMachineNo += 1;

                await miaoJi.Leave();

                await miaoJi.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LuckyCat-api/LetItGo", 2, header);

                await Task.Delay(100);
            }

            listBox1.Items.Add(gameName + "富豪區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                MiaoJi miaoJi = new MiaoJi();
                bool success = await miaoJi.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LuckyCat-api/machine/?access_token=" + token, 3, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    MiaoJiStartGameResult2 miaoJiStartGameResult2 = await miaoJi.GetMachineInfoAward();
                    long Award3000 = miaoJiStartGameResult2.BetAndAward[0].Award;
                    //long Award600 = miaoJiStartGameResult2.BetAndAward[1].Award;
                    //long Award900 = miaoJiStartGameResult2.BetAndAward[2].Award;

                    if (Award3000 == 7 || Award3000 == 8 || Award3000 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + miaoJiStartGameResult2.BetAndAward[0]);

                        //ApiGatewayUnity  (進入機台)
                        //token
                        //zoneType
                        //myMachineNo
                        //Player player = new Player();
                        //player.Play("TA310", "Fudan168ta11", ApiGatewayUnity,  ZoneType, myMachineNo);
                        //aac

                    }

                }

                myMachineNo += 1;

                await miaoJi.Leave();

                await miaoJi.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LuckyCat-api/LetItGo", 3, header);

                await Task.Delay(100);
            }
            #endregion

            #region 聚寶盆
            gameName = "聚寶盆";

            //娛樂區
            //listBox1.Items.Add(gameName + "娛樂區");
            //listBox1.TopIndex = listBox1.Items.Count - 1;
            //myMachineNo = 1;
            //while (myMachineNo <= 30)
            //{
            //    //進入機台(Post)
            //    TreasureBowl treasureBowl = new TreasureBowl();
            //    bool success = await treasureBowl.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-TreasureBowl-api/machine/?access_token=" + token, 6, myMachineNo);
            //    if (success)
            //    {
            //        listBox1.Items.Add("機台：" + myMachineNo);
            //        listBox1.TopIndex = listBox1.Items.Count - 1;

            //        TreasureBowlStartGameResult2 treasureBowlStartGameResult2 = await treasureBowl.GetMachineInfoAward();
            //        long Award30 = treasureBowlStartGameResult2.BetAndAward[0].Award;
            //        long Award60 = treasureBowlStartGameResult2.BetAndAward[1].Award;
            //        long Award90 = treasureBowlStartGameResult2.BetAndAward[2].Award;

            //        if (Award30 == 7 || Award30 == 8 || Award30 == 9)
            //        {
            //            //開一個thread，用另一個玩家玩
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[0]);
            //        }

            //        if (Award60 == 7 || Award60 == 8 || Award60 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[1]);
            //        }

            //        if (Award90 == 7 || Award90 == 8 || Award90 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[2]);
            //        }
            //    }

            //    myMachineNo += 1;

            //    await treasureBowl.Leave();

            //    await treasureBowl.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-TreasureBowl-api/LetItGo", 6, header);

            //    await Task.Delay(100);
            //}

            listBox1.Items.Add(gameName + "高級區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                TreasureBowl treasureBowl = new TreasureBowl();
                bool success = await treasureBowl.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-TreasureBowl-api/machine/?access_token=" + token, 2, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    TreasureBowlStartGameResult2 treasureBowlStartGameResult2 = await treasureBowl.GetMachineInfoAward();
                    long Award300 = treasureBowlStartGameResult2.BetAndAward[0].Award;
                    long Award600 = treasureBowlStartGameResult2.BetAndAward[1].Award;
                    long Award900 = treasureBowlStartGameResult2.BetAndAward[2].Award;

                    if (Award300 == 7 || Award300 == 8 || Award300 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[0]);
                    }

                    if (Award600 == 7 || Award600 == 8 || Award600 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[1]);
                    }

                    if (Award900 == 7 || Award900 == 8 || Award900 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[2]);
                    }
                }

                myMachineNo += 1;

                await treasureBowl.Leave();

                await treasureBowl.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-TreasureBowl-api/LetItGo", 2, header);

                await Task.Delay(100);
            }

            listBox1.Items.Add(gameName + "富豪區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                TreasureBowl treasureBowl = new TreasureBowl();
                bool success = await treasureBowl.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-TreasureBowl-api/machine/?access_token=" + token, 3, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    TreasureBowlStartGameResult2 treasureBowlStartGameResult2 = await treasureBowl.GetMachineInfoAward();
                    long Award3000 = treasureBowlStartGameResult2.BetAndAward[0].Award;

                    if (Award3000 == 7 || Award3000 == 8 || Award3000 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + treasureBowlStartGameResult2.BetAndAward[0]);
                    }
                }

                myMachineNo += 1;

                await treasureBowl.Leave();

                await treasureBowl.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-TreasureBowl-api/LetItGo", 3, header);

                await Task.Delay(100);
            }
            #endregion

            #region 賭城狂歡
            gameName = "賭城狂歡";
            //listBox1.Items.Add(gameName + "娛樂區");
            //listBox1.TopIndex = listBox1.Items.Count - 1;
            //myMachineNo = 1;
            //while (myMachineNo <= 30)
            //{
            //    //進入機台(Post)
            //    Vegas vegas = new Vegas();
            //    bool success = await vegas.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LetsGoToVegas-api/machine/?access_token=" + token, 6, myMachineNo);
            //    if (success)
            //    {
            //        listBox1.Items.Add("機台：" + myMachineNo);
            //        listBox1.TopIndex = listBox1.Items.Count - 1;

            //        VegasStartGameResult2 vegasStartGameResult2 = await vegas.GetMachineInfoAward();
            //        long Award30 = vegasStartGameResult2.BetAndAward[0].Award;
            //        long Award60 = vegasStartGameResult2.BetAndAward[1].Award;
            //        long Award90 = vegasStartGameResult2.BetAndAward[2].Award;

            //        if (Award30 == 7 || Award30 == 8 || Award30 == 9)
            //        {
            //            //開一個thread，用另一個玩家玩
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[0].Bet);
            //        }

            //        if (Award60 == 7 || Award60 == 8 || Award60 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[1].Bet);
            //        }

            //        if (Award90 == 7 || Award90 == 8 || Award90 == 9)
            //        {
            //            MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[2].Bet);
            //        }
            //    }

            //    myMachineNo += 1;

            //    await vegas.Leave();

            //    await vegas.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LetsGoToVegas-api/LetItGo", 6, header);

            //    await Task.Delay(100);
            //}

            listBox1.Items.Add(gameName + "高級區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                Vegas vegas = new Vegas();
                bool success = await vegas.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LetsGoToVegas-api/machine/?access_token=" + token, 2, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    VegasStartGameResult2 vegasStartGameResult2 = await vegas.GetMachineInfoAward();
                    long Award300 = vegasStartGameResult2.BetAndAward[0].Award;
                    long Award600 = vegasStartGameResult2.BetAndAward[1].Award;
                    long Award900 = vegasStartGameResult2.BetAndAward[2].Award;

                    if (Award300 == 7 || Award300 == 8 || Award300 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[0].Bet);
                    }

                    if (Award600 == 7 || Award600 == 8 || Award600 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[1].Bet);
                    }

                    if (Award900 == 7 || Award900 == 8 || Award900 == 9)
                    {
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[2].Bet);
                    }
                }

                myMachineNo += 1;

                await vegas.Leave();

                await vegas.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LetsGoToVegas-api/LetItGo", 2, header);

                await Task.Delay(100);
            }

            listBox1.Items.Add(gameName + "富豪區");
            listBox1.TopIndex = listBox1.Items.Count - 1;
            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                //進入機台(Post)
                Vegas vegas = new Vegas();
                bool success = await vegas.ConnectVerify("wss://" + ApiGatewayUnity + "/pachislot-LetsGoToVegas-api/machine/?access_token=" + token, 3, myMachineNo);
                if (success)
                {
                    listBox1.Items.Add("機台：" + myMachineNo);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    VegasStartGameResult2 vegasStartGameResult2 = await vegas.GetMachineInfoAward();
                    long Award3000 = vegasStartGameResult2.BetAndAward[0].Award;

                    if (Award3000 == 7 || Award3000 == 8 || Award3000 == 9)
                    {
                        //開一個thread，用另一個玩家玩
                        MessageBox.Show(gameName + "機台：" + myMachineNo + " 下注額：" + vegasStartGameResult2.BetAndAward[0].Bet);
                    }
                }

                myMachineNo += 1;

                await vegas.Leave();

                await vegas.LeaveGame("https://" + ApiGatewayUnity, "/pachislot-LetsGoToVegas-api/LetItGo", 3, header);

                await Task.Delay(100);
            }
            #endregion

            listBox1.Items.Add("結束！");
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        async Task FindUnder120(long id, string token)
        {

            string gameName;
            int myMachineNo;
            //long Award = 0;//Award = 4，表示上一局是青再連線

            //進入機台，看300、600、900、3000的Award = 7 8 9
            // 7 = RB, 8 = BB, 9 = 1G連
            //發現後，使用別的帳號進去轉。掃台帳號繼續掃台。
            // -> 轉完後離開
            // -> 掃完台後離開

            //進入1~30台
            //檢查Award = 7 8 9

            Dictionary<string, string> header;

            header = new Dictionary<string, string>()
            {
                {"platform", "WebGLPlayer" },
                {"authorization", "Bearer " + token },
                {"version", "1" }
            };

            #region 聚寶盆
            gameName = "聚寶盆";
            listBox1.Items.Add(gameName + "高級區找120轉");
            listBox1.TopIndex = listBox1.Items.Count - 1;

            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                listBox1.Items.Add("機台：" + myMachineNo);
                listBox1.TopIndex = listBox1.Items.Count - 1;

                TreasureBowl treasureBowl = new TreasureBowl();
                NoBonusTimeSVData noBonusTimeSVData = await treasureBowl.GetMachineInfo120("https://prod-apigateway-unity.diamondonline.com.tw/pachislot-TreasureBowl-api/Record?zoneId=" + 2 + "&machineNo=" + myMachineNo, "", header);
                long noBonusTime300 = noBonusTimeSVData.betAndNoBonusTimes[0].noBonusTimes;
                long noBonusTime600 = noBonusTimeSVData.betAndNoBonusTimes[1].noBonusTimes;
                long noBonusTime900 = noBonusTimeSVData.betAndNoBonusTimes[2].noBonusTimes;

                if (noBonusTime300 < 120)
                {
                    MessageBox.Show("120機台：" + myMachineNo + " 下注額：" + noBonusTimeSVData.betAndNoBonusTimes[0].bet);
                }

                if (noBonusTime600 < 120)
                {
                    MessageBox.Show("120機台：" + myMachineNo + " 下注額：" + noBonusTimeSVData.betAndNoBonusTimes[1].bet);
                }

                if (noBonusTime900 < 120)
                {
                    MessageBox.Show("120機台：" + myMachineNo + " 下注額：" + noBonusTimeSVData.betAndNoBonusTimes[2].bet);
                }

                myMachineNo += 1;

                await Task.Delay(1000);
            }


            listBox1.Items.Add(gameName + "富豪區找120轉");
            listBox1.TopIndex = listBox1.Items.Count - 1;

            myMachineNo = 1;
            while (myMachineNo <= 30)
            {
                listBox1.Items.Add("機台：" + myMachineNo);
                listBox1.TopIndex = listBox1.Items.Count - 1;

                TreasureBowl treasureBowl = new TreasureBowl();
                NoBonusTimeSVData noBonusTimeSVData = await treasureBowl.GetMachineInfo120("https://prod-apigateway-unity.diamondonline.com.tw/pachislot-TreasureBowl-api/Record?zoneId=" + 3 + "&machineNo=" + myMachineNo, "", header);
                long noBonusTime3000 = noBonusTimeSVData.betAndNoBonusTimes[0].noBonusTimes;

                if (noBonusTime3000 < 120)
                {
                    MessageBox.Show("120機台：" + myMachineNo + " 下注額：" + noBonusTimeSVData.betAndNoBonusTimes[0].bet);
                }

                myMachineNo += 1;

                await Task.Delay(1000);
            }
            #endregion

            listBox1.Items.Add("結束！");
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        //雙魚座2活動 自動遊玩
        async Task<string> PlayPisces2Activity(long bet, long id, string token, long zoneType)
        {
            string gameName = "雙魚座2";

            int myMachineNo = 8;
            long playCount = 1000;//遊玩次數
            long totalBet = 0;//總押注 = 單次押注 * 遊玩次數
            long totalScore = 0;//總得分
            long totalWin = 0;//總勝點 = 總得分 - 總押注

            Dictionary<string, string> header;
            Pisces2 pisces2 = new Pisces2();

            listBox1.Items.Add(gameName);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            #region 取得玩家身上鑽幣(前)
            long goldBefore = await GetGold(id, token);

            listBox1.Items.Add("遊戲前鑽幣：" + goldBefore);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            #endregion

            #region 選擇空機台進入
            //機台使用狀態API(Get)
            header = new Dictionary<string, string>()
            {
                {"authorization", "Bearer " + token },
                {"version", "1.0.1" }
            };
            bool success = false;

            while (!success)
            {
                //List<GetLobbyMachinesHighAPI> getLobbyMachinesHighAPIs = await pisces2.GetMachineList("https://pisces2.endpoints.diamondonline-" + area + ".cloud.goog/api/lobby/machines/High", "", header);
                //myMachineNo = 0;

                //foreach (var item in getLobbyMachinesHighAPIs)
                //{
                //    if (item.status == 1)//空機台
                //    {
                //        myMachineNo = item.machineNo;
                //        break;
                //    }
                //}

                //進入機台(Websocket)
                if (myMachineNo != 0)
                {
                    success = await pisces2.ConnectVerify("wss://pisces2.endpoints.diamondonline-" + area + ".cloud.goog/machine?access_token=" + token, ZoneType, myMachineNo);
                }
                else
                {
                    MessageBox.Show("全機台已被佔滿");
                }

                if (!success) {
                    listBox1.Items.Add(myMachineNo + "號機台有人");
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }

                await Task.Delay(1000);
            }

            #endregion

            #region 玩N把(紀錄總共獲得)
            long count = 0;
            long credits = 0;

            if (success)
            {
                //取得機台資訊
                credits = await pisces2.InitMachine();

                while (playCount > count)
                {
                    if (credits < 400)
                    {
                        credits += await pisces2.AddCredits();
                    }

                    if (credits >= 400 && credits < 800)
                    {
                        Console.WriteLine("注意這裡沒測試過 有可能會有錯");
                        await pisces2.BetAll(bet);
                        string round = await pisces2.StartGame(bet);
                        await pisces2.DealCard(bet);
                        credits -= bet;
                        if (!string.IsNullOrEmpty(round))
                        {
                            await pisces2.Bet(bet);
                            Pisces2StartGameBetResponse pisces2StartGameBetResponse = await pisces2.DealAll(bet);
                            credits -= bet;

                            Pisces2StartGameAwardResult pisces2StartGameAwardResult = await pisces2.AwardResult();
                            long score = pisces2StartGameAwardResult.awardWin;

                            await pisces2.GameResult();
                            
                            count += 1;
                            totalScore += score;
                            credits += score;
                            listBox1.Items.Add("*第" + count + "場得分：" + score + " 剩餘credits：" + credits);
                            listBox1.TopIndex = listBox1.Items.Count - 1;
                        }
                    }

                    if (credits >= 800)
                    {
                        await pisces2.Bet(bet);
                        string round = await pisces2.StartGame(bet);
                        Console.WriteLine(round);

                        await pisces2.DealCard(bet);
                        credits -= bet;
                        if (!string.IsNullOrEmpty(round))
                        {
                            await pisces2.Bet(bet);
                            await pisces2.DealCard(bet);
                            credits -= bet;

                            await pisces2.Bet(bet);
                            await pisces2.DealCard(bet);
                            credits -= bet;

                            await pisces2.Bet(bet);
                            Pisces2StartGameBetResponse pisces2StartGameBetResponse = await pisces2.DealAll(bet);
                            credits -= bet;
                            //牌型驗證，有空再做...
                            //pisces2.PokerTypeJudge(pisces2StartGameBetResponse.pokerInfos);
                            Pisces2StartGameAwardResult pisces2StartGameAwardResult = await pisces2.AwardResult();
                            long score = pisces2StartGameAwardResult.awardWin;

                            //score > 0 比倍直到score > 40000 收下
                            //score == 0 結束

                            while (score > 0 && score < 8000)//比倍
                            {
                                score = await pisces2.DoubleUp(false);

                                await Task.Delay(2000);
                            }

                            //得分
                            await pisces2.GameResult();

                            count += 1;
                            totalScore += score;
                            credits += score;
                            listBox1.Items.Add("第" + count + "場得分：" + score + " 剩餘credits：" + credits);
                            listBox1.TopIndex = listBox1.Items.Count - 1;
                        }

                        //上分
                        if (credits > 5000)
                        {
                            credits -= await pisces2.TakeCredits();
                            listBox1.Items.Add("上分後剩餘credits：" + credits);
                            listBox1.TopIndex = listBox1.Items.Count - 1;
                        }
                    }

                    await Task.Delay(5000);
                }
            }
            #endregion

            #region 離開遊戲
            listBox1.Items.Add("結束迴圈" + credits);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            //await pisces2.LeaveMachine();
            #endregion

            return gameName;
        }
        #endregion

    }
}

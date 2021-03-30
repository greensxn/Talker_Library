using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace FormSayNumber
{
    class Talk
    {
        static String Dir = "Numbers";
        static List<String> prefix = new List<string>();
        static SoundPlayer sp;
        static bool IsPrefixSay = false;
        static bool IsSpecial = false;
        static Voice G;

        public enum Voice
        {
            Man, Woman
        }

        private static async Task Say(string Num, string LengthNum, bool Special)
        {
            if (Num == "0")
                return;

            if (Special && LengthNum.Length == 1) {
                if (Num == "1" || Num == "2") {
                    sp = new SoundPlayer($"{Dir}/{G}/0/{Num}T.wav");
                    await Task.Factory.StartNew(() => {
                        sp.PlaySync();
                        //Process.Start(@"Numbers\" + "0" + @"\" + Num + "T" + ".txt");
                    });
                    return;
                }
            }

            String CountZero = "";
            for (int i = 0; i < LengthNum.Length; i++)
                CountZero += "0";

            sp = new SoundPlayer($"{Dir}/{G}/{CountZero}/{Num}.wav");
            await Task.Factory.StartNew(() => {
                sp.PlaySync();
                //Process.Start(@"Numbers\" + Loc + @"\" + Num + ".txt");
            });
        }

        static List<String> MP3 = new List<String>();
        public static void CreateMP3(String Number /*string[] mp3filenames*/, Stream OutFile) //////
        {
            setListNum(Number);
            prefixNum(list);

            int Special = 0;
            bool IsCheck = false;
            if (list.Count > 1)
                Special = (int)list[list.Count - 2];
            foreach (decimal number in list) {
                if (number == 0)
                    continue;
                if ((int)number == Special && !IsCheck && prefix.Count == 1) {
                    IsCheck = true;
                    IsSpecial = true;
                }
                switch (number.ToString().Length) {                     //say number
                    case 1:
                        setdodesiati(number, list.Count, IsSpecial);
                        break;
                    case 2:
                        setdestichnie(number, list.Count, IsSpecial);
                        break;
                    case 3:
                        setsotie(number, list.Count, IsSpecial);
                        break;
                }
                if (prefix.Count == 1)
                    IsSpecial = false;

                if (IsPrefixSay && prefix.Count > 0) {  //say prefix
                    MP3.Add(prefix[0]);
                    prefix.RemoveAt(0);
                    IsPrefixSay = false;
                }
            }


            String Desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            List<String> way = new List<String>();
            foreach (String name in MP3) {
                way.Add($"{Desk}/S/{name}.mp3");
            }

            foreach (string file in way) {
                Mp3FileReader reader = new Mp3FileReader(file);
                if ((OutFile.Position == 0) && (reader.Id3v2Tag != null)) {
                    OutFile.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null) {
                    OutFile.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }

            MP3.Clear();
        }

        static List<decimal> betweenList = new List<decimal>();
        static bool IsStopBetween = false;
        public async static Task SayBetween(string Num1, string Num2, int DelayInSec, Voice Gender)
        {
            if (Gender == Voice.Woman)
                return;

            betweenList.Clear();
            decimal num1 = Convert.ToDecimal(Num1);
            decimal num2 = Convert.ToDecimal(Num2);
            int delay = DelayInSec * 1000;

            await Task.Factory.StartNew(() => {
                try {
                    if (num1 <= num2)
                        for (decimal i = num1; i <= num2; i++)
                            betweenList.Add(i);
                    else
                        for (decimal i = num1; i >= num2; i--)
                            betweenList.Add(i);
                }
                catch { }
            });

            foreach (decimal Num in betweenList) {
                await Say(Num.ToString(), Gender);
                await Task.Factory.StartNew(() => {
                    Thread.Sleep(delay);
                });
                if (IsStopBetween) {
                    IsStopBetween = false;
                    break;
                }
            }
        }

        public static void StopSay()
        {
            IsStopBetween = true;
        }

        private async static Task SayPrefix(string Name)
        {
            if (Name == null)
                return;

            sp = new SoundPlayer($"{Dir}/{G}/prefix/{Name}.wav");
            await Task.Factory.StartNew(() => {
                sp.PlaySync();
                //Process.Start(@"Numbers\" + @"prefix\" + Name + ".txt");
            });
        }

        static List<decimal> list;
        static List<String> getList;
        public async static Task Say(string Number, Voice Gender)
        {
            G = Gender;
            if (G == Voice.Woman)
                return;

            try {
                setListNum(Number);
                prefixNum(list);

                //-----------------------------------------

                int Special = 0;
                bool IsCheck = false;
                if (list.Count > 1)
                    Special = (int)list[list.Count - 2];
                foreach (var number in list) {
                    if (number == 0)
                        continue;
                    if ((int)number == Special && !IsCheck && prefix.Count == 1) {
                        IsCheck = true;
                        IsSpecial = true;
                    }
                    switch (number.ToString().Length) {                     //say number
                        case 1:
                            await dodesiati(number, list.Count, IsSpecial);
                            break;
                        case 2:
                            await destichnie(number, list.Count, IsSpecial);
                            break;
                        case 3:
                            await sotie(number, list.Count, IsSpecial);
                            break;
                    }
                    if (prefix.Count == 1)
                        IsSpecial = false;
                    if (IsPrefixSay && prefix.Count > 0) {  //say prefix
                        await SayPrefix(prefix[0]);
                        prefix.RemoveAt(0);
                        IsPrefixSay = false;
                    }
                }

                //-----------------------------------------
            }
            catch { }
        }

        public static string getNum(String Number)
        {
            String T = "";
            try {
                if (Convert.ToDecimal(Number) == 0)
                    return "0";

                setListNum(Number);
                foreach (var part in getList) {
                    T += part + " ";
                }
                T = DelZero(T);
            }
            catch {
                return "Invalid data.";
            }
            return T;
        }

        private static String DelZero(String Num)
        {
            while (Num[0] == '0' || Num[0] == ' ')
                Num = Num.Remove(0, 1);
            return Num;
        }

        private static void setListNum(String Number)
        {
            list = new List<decimal>();
            getList = new List<string>();

            String numPart = "";
            int len = getLength(Number.Length);
            for (decimal u = 0; u < len; u++) {
                numPart = "";
                for (int i = Number.Length - 1; i >= 0; i--) {
                    numPart += Number[i];
                    if (numPart.Length > 2)
                        break;
                }
                Number = Number.Remove(Number.Length - numPart.Length, numPart.Length);
                numPart = Backward(numPart);

                if (list.Count < 1) {
                    list.Add(Convert.ToInt32(numPart));
                    getList.Add(numPart);
                }
                else {
                    list.Insert(0, Convert.ToInt32(numPart));
                    getList.Insert(0, numPart);
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------
        private async static Task dodesiati(decimal number, int count, bool Special)
        {
            String Loc = Dir + @"\0";
            await Say(number.ToString(), "*", Special);
            IsPrefixSay = true;
        }

        private async static Task destichnie(decimal number, int count, bool Special)
        {
            if (await pravka(number, Special)) {
                String n = number.ToString();

                String Loc = Dir + @"\00";
                await Say(n[0].ToString() + "0", "**", false);

                Loc = Dir + @"\0";
                await Say(n[1].ToString(), "*", Special);
            }
            IsPrefixSay = true;
        }

        private async static Task sotie(decimal number, int count, bool Special)
        {
            String n = number.ToString();

            String Loc = Dir + @"\000";
            await Say(n[0] + "00", "***", false);

            if (await pravka(Convert.ToDecimal($"{n[1]}{n[2]}"), Special)) {
                Loc = Dir + @"\00";
                await Say(n[1].ToString() + "0", "**", false);

                Loc = Dir + @"\0";
                await Say(n[2].ToString(), "*", Special);
            }
            IsPrefixSay = true;
        }
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------
        private static void setdodesiati(decimal number, int count, bool Special)
        {
            String n = number.ToString();
            if (Special) {
                if (n[0] == '1' || n[0] == '2') {
                    MP3.Add($"{n[0]}T");
                }
                else {
                    if (n[0] != '0')
                        MP3.Add($"{n[0]}");
                }
            }
            else {
                if (n[0] != '0')
                    MP3.Add($"{n[0]}");
            }
            IsPrefixSay = true;
        }

        private static void setdestichnie(decimal number, int count, bool Special)
        {
            if (setpravka(number, Special)) {
                String n = number.ToString();

                MP3.Add($"{n[0]}0");

                if (Special) {
                    if (n[1] == '1' || n[1] == '2') {
                        MP3.Add($"{n[1]}T");
                    }
                    else {
                        if (n[1] != '0')
                            MP3.Add($"{n[1]}");
                    }
                }
                else {
                    if (n[1] != '0')
                        MP3.Add($"{n[1]}");
                }
            }
            IsPrefixSay = true;
        }

        private static void setsotie(decimal number, int count, bool Special)
        {
            String n = number.ToString();

            MP3.Add($"{n[0]}00");

            if (setpravka(Convert.ToDecimal($"{n[1]}{n[2]}"), Special)) {
                MP3.Add($"{n[1]}0");
                if (Special) {
                    if (n[2] == '1' || n[2] == '2') {
                        MP3.Add($"{n[2]}T");
                    }
                    else {
                        if (n[2] != '0')
                            MP3.Add($"{n[2]}");
                    }
                }
                else {
                    if (n[2] != '0')
                        MP3.Add($"{n[2]}");
                }
            }
            IsPrefixSay = true;
        }
        //--------------------------------------------------------------------------------------------------------

        static int count = 0;
        private static void prefixNum(List<decimal> Number)
        {
            if (Number.Count > 5 && IsNull(Number[count]))
                if (CheckNum(Number[count], 1))
                    prefix.Add("Квадриллион");
                else if (CheckNum(Number[count], 2))
                    prefix.Add("Квадриллиона");
                else {
                    count++;
                    prefix.Add("Квадриллионов");
                }
            if (Number.Count > 4 && IsNull(Number[count]))
                if (CheckNum(Number[count], 1))
                    prefix.Add("Триллион");
                else if (CheckNum(Number[count], 2))
                    prefix.Add("Триллиона");
                else {
                    count++;
                    prefix.Add("Триллионов");
                }
            if (Number.Count > 3 && IsNull(Number[count]))
                if (CheckNum(Number[count], 1))
                    prefix.Add("Миллиард");
                else if (CheckNum(Number[count], 2))
                    prefix.Add("Миллиарда");
                else {
                    count++;
                    prefix.Add("Миллиардов");
                }
            if (Number.Count > 2 && IsNull(Number[count]))
                if (CheckNum(Number[count], 1))
                    prefix.Add("Миллион");
                else if (CheckNum(Number[count], 2))
                    prefix.Add("Миллиона");
                else {
                    count++;
                    prefix.Add("Миллионов");
                }
            if (Number.Count > 1 && IsNull(Number[count]))
                if (CheckNum(Number[count], 1))
                    prefix.Add("Тысяча");
                else if (CheckNum(Number[count], 2))
                    prefix.Add("Тысячи");
                else
                    prefix.Add("Тысяч");
            count = 0;
        }

        private static bool CheckNum(decimal Num, int Interval)
        {
            if (IsPravka(Num)) {
                return false;
            }

            Char N = Num.ToString()[Num.ToString().Length - 1];
            if (Interval == 1) {
                if (N.ToString() == 1.ToString()) {
                    count++;
                    return true;
                }
            }
            else {
                for (int i = 2; i <= 4; i++)
                    if (N.ToString() == i.ToString() && N.ToString() != 0.ToString()) {
                        count++;
                        return true;
                    }
            }
            return false;
        }

        private static bool IsNull(decimal v)
        {
            foreach (var _0 in v.ToString())
                if (_0 == '0') {
                }
                else return true;
            count++;
            return false;
        }

        private async static Task<bool> pravka(decimal number, bool Special)
        {
            String Loc;
            if (number.ToString().Length == 1) {
                Loc = Dir + @"\0";
                await Say(number.ToString(), "*", Special);
                return false;
            }
            if (number > 19 || number < 10)
                return true;
            Loc = Dir + @"\00";
            await Say(number.ToString(), "**", Special);
            return false;
        }

        private static bool setpravka(decimal number, bool Special)
        {
            if (number.ToString().Length == 1) {
                if (Special) {
                    if (number.ToString() == "1" || number.ToString() == "2") {
                        MP3.Add($"{number.ToString()}T");
                    }
                    else
                         if (number != 0)
                        MP3.Add($"{number.ToString()}");
                }
                else {
                    if (number != 0)
                        MP3.Add($"{number.ToString()}");
                }
                return false;
            }
            if (number > 19 || number < 10)
                return true;

            MP3.Add($"{number.ToString()}");
            return false;
        }

        private static bool IsPravka(decimal number)
        {
            String Numb = number.ToString();
            if (Numb.Length == 3)
                Numb = $"{Numb[1]}{Numb[2]}";

            int N = Convert.ToInt32(Numb);
            if (N < 21 && N > 9)
                return true;

            return false;
        }

        private static int getLength(int Number)
        {
            if (Number % 3 == 0)
                return Number / 3;
            else
                return Number / 3 + 1;
        }

        private static string Backward(string Text)
        {
            String z = "";
            for (int i = Text.Length - 1; i >= 0; i--)
                z += Text[i];
            return z;
        }

    }
}

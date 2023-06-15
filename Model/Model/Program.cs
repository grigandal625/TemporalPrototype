using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
namespace Model
{
    [assembly: AssemblyKeyFile("SimModel.snk")]
    [Guid("FDE2C8D0-E950-2654-9AE9-516221AFAC31")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [ComVisible(true)]
    public interface IModel
    {
        [DispId(1)]
        String Name { get; set; }
        [DispId(2)]
        System.Object Broker { get; set; }
        [DispId(3)]
        System.Object Get_Broker();
        [DispId(4)]
        string Get_Name();
        [DispId(5)]
        void Set_Broker(System.Object Value);
        [DispId(6)]
        void Set_Name(string Value);
        [DispId(7)]
        void Configurate(string Config);
        [DispId(8)]
        void ProcessMessage(string SenderName, string MessageText, System.Object OleVariant);
        [DispId(9)]
        void Stop();
    }
    [Guid("CBAEA8CE-0FA0-2324-97BA-5E204DD791F9")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class ATModel : IModel
    {
        static Random rnd = new Random();

        static Пациент Пациент;
        static Счетчик Счетчик;
        static int Номер_текущего_такта = 0;
        System.Object m_broker;
        string m_name;
        public string Name
        {
            get
            {
                using (StreamWriter sw = File.AppendText("Log.txt"))
                { sw.WriteLine("get Name"); }
                return m_name;
            }
            set
            {
                using (StreamWriter sw = File.AppendText("Log.txt"))
                { sw.WriteLine("set Name" + value); }
                m_name = value;
            }
        }
        public System.Object Broker
        {
            get
            {
                using (StreamWriter sw = File.AppendText("Log.txt"))
                { sw.WriteLine("get broker"); }
                return m_broker;
            }
            set
            {
                using (StreamWriter sw = File.AppendText("Log.txt"))
                { sw.WriteLine("1set broker" + value); }
                m_broker = value;
                using (StreamWriter sw = File.AppendText("Log.txt"))
                { sw.WriteLine("2set broker" + m_broker.ToString()); }
            }
        }
        public System.Object Get_Broker()
        {
            using (StreamWriter sw = File.AppendText("Log.txt"))
            { sw.WriteLine("get broker wrong"); }
            return Broker;
        }
        public string Get_Name()
        {
            using (StreamWriter sw = File.AppendText("Log.txt"))
            { sw.WriteLine("get Name wrong"); }
            return Name;
        }
        public void Set_Broker(System.Object Value)
        {
            using (StreamWriter sw = File.AppendText("Log.txt"))
            { sw.WriteLine("set broker wrong"); }
            Broker = Value;
        }
        public void Set_Name(string Value)
        {
            using (StreamWriter sw = File.AppendText("Log.txt"))
            { sw.WriteLine("set name wrong"); }
            Name = Value;
        }
        public void Configurate(string Config)
        { }
        public void ProcessMessage(string SenderName, string MessageText, System.Object OleVariant)
        {
            Номер_текущего_такта = Номер_текущего_такта + 1;
            if (Номер_текущего_такта == 1)
            {
                Пациент = new Пациент();
                Пациент.Температура = Пациент.Enum_Температура.Нормальная;
                Пациент.Кашель = Пациент.Enum_Кашель.Нет;
                Пациент.Сыпь = Пациент.Enum_Есть_Нет.Нет;
                Пациент.Озноб = Пациент.Enum_Есть_Нет.Нет;

                Счетчик = new Счетчик();
                Счетчик.Отчет = Счетчик.Enum_Отчет.Да;
                Счетчик.Секунды = 0;
                Счетчик.Индикатор = 0;
                formingOutParam(MessageText);
                Random rnd = new Random();
                Операция_время();

                Операция_температура();
                Операция_сыпь();
            }
            else
            {
                formingOutParam(MessageText);
                Random rnd = new Random();
                Операция_время();

                Операция_температура();
                Операция_сыпь();
            }
        }
        public void Stop()
        {
            Environment.Exit(0);
        }
        static void Операция_время()
        {
            Образец_изменения_времени(Счетчик);
        }
        static void Операция_температура()
        {
            Определение_температуры(Пациент);
        }
        static void Операция_сыпь()
        {
            Определение_сыпи(Пациент);
        }

        /*
        static void Операция_3()
        {
            Определение_определения_2(Стакан);
        }
        private static void Определение_определения_1(Стакан Стакан)
        {
            if (Счетчик.Индикатор <= 50)
            {
                Стакан.Состояние = Стакан.Enum_Состояние.Пуст;
            }
        }
        private static void Определение_определения_2(Стакан Стакан)
        {
            if (Счетчик.Индикатор > 50)
            {
                Стакан.Состояние = Стакан.Enum_Состояние.Полон;
            }
        }
        */

        private static void Образец_изменения_времени(Счетчик Счетчик)
        {
            if (Счетчик.Отчет == Счетчик.Enum_Отчет.Да)
            {
                Счетчик.Отчет = Счетчик.Enum_Отчет.Нет;
                System.Threading.Thread.Sleep(10);
                Счетчик.Индикатор = Случайное_число(1, 100);
                Счетчик.Секунды = Счетчик.Секунды + 1;
                Счетчик.Отчет = Счетчик.Enum_Отчет.Да;
            }
        }
        private static void Определение_температуры(Пациент Пациент)
        {
            if (Счетчик.Секунды % 5 > 1)
            {
                Пациент.Температура = Пациент.Enum_Температура.Повышенная;
            }
            else
            {
                Пациент.Температура = Пациент.Enum_Температура.Нормальная;
            }
        }
        private static void Определение_сыпи(Пациент Пациент)
        {
            if (Счетчик.Секунды == 3)
            {
                Пациент.Сыпь = Пациент.Enum_Есть_Нет.Есть;
            }
            else
            {
                Пациент.Сыпь = Пациент.Enum_Есть_Нет.Нет;
            }
        }


        static int Случайное_число(int inputNumber1, int inputNumber2)
        {
            return rnd.Next(inputNumber1, inputNumber2);
        }
        static void formingOutParam(string tactNum)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"bb2.xml");
            XmlNode factlist = doc.SelectSingleNode("//facts");
            factlist.RemoveAll();

            XmlNode Пациент_Температура = doc.CreateElement("fact");
            XmlAttribute Пациент_Температура_atr = doc.CreateAttribute("AttrPath");
            Пациент_Температура_atr.Value = "Пациент.Температура";
            Пациент_Температура.Attributes.Append(Пациент_Температура_atr);

            XmlAttribute Пациент_Температура_atr2 = doc.CreateAttribute("Value");
            Пациент_Температура_atr2.Value = Пациент.Температура.ToString();
            Пациент_Температура.Attributes.Append(Пациент_Температура_atr2);

            XmlAttribute Пациент_Температура_atr3 = doc.CreateAttribute("Belief");
            Пациент_Температура_atr3.Value = "50";
            Пациент_Температура.Attributes.Append(Пациент_Температура_atr3);

            XmlAttribute Пациент_Температура_atr4 = doc.CreateAttribute("MaxBelief");
            Пациент_Температура_atr4.Value = "100";
            Пациент_Температура.Attributes.Append(Пациент_Температура_atr4);

            XmlAttribute Пациент_Температура_atr5 = doc.CreateAttribute("Accuracy");
            Пациент_Температура_atr5.Value = "0";
            Пациент_Температура.Attributes.Append(Пациент_Температура_atr5);
            factlist.AppendChild(Пациент_Температура);
            // ------
            XmlNode Пациент_Сыпь = doc.CreateElement("fact");
            XmlAttribute Пациент_Сыпь_atr = doc.CreateAttribute("AttrPath");
            Пациент_Сыпь_atr.Value = "Пациент.Сыпь";
            Пациент_Сыпь.Attributes.Append(Пациент_Сыпь_atr);

            XmlAttribute Пациент_Сыпь_atr2 = doc.CreateAttribute("Value");
            Пациент_Сыпь_atr2.Value = Пациент.Сыпь.ToString();
            Пациент_Сыпь.Attributes.Append(Пациент_Сыпь_atr2);

            XmlAttribute Пациент_Сыпь_atr3 = doc.CreateAttribute("Belief");
            Пациент_Сыпь_atr3.Value = "50";
            Пациент_Сыпь.Attributes.Append(Пациент_Сыпь_atr3);

            XmlAttribute Пациент_Сыпь_atr4 = doc.CreateAttribute("MaxBelief");
            Пациент_Сыпь_atr4.Value = "100";
            Пациент_Сыпь.Attributes.Append(Пациент_Сыпь_atr4);

            XmlAttribute Пациент_Сыпь_atr5 = doc.CreateAttribute("Accuracy");
            Пациент_Сыпь_atr5.Value = "0";
            Пациент_Сыпь.Attributes.Append(Пациент_Сыпь_atr5);
            factlist.AppendChild(Пациент_Сыпь);
            // ------
            XmlNode Пациент_Озноб = doc.CreateElement("fact");
            XmlAttribute Пациент_Озноб_atr = doc.CreateAttribute("AttrPath");
            Пациент_Озноб_atr.Value = "Пациент.Озноб";
            Пациент_Озноб.Attributes.Append(Пациент_Озноб_atr);

            XmlAttribute Пациент_Озноб_atr2 = doc.CreateAttribute("Value");
            Пациент_Озноб_atr2.Value = Пациент.Озноб.ToString();
            Пациент_Озноб.Attributes.Append(Пациент_Озноб_atr2);

            XmlAttribute Пациент_Озноб_atr3 = doc.CreateAttribute("Belief");
            Пациент_Озноб_atr3.Value = "50";
            Пациент_Озноб.Attributes.Append(Пациент_Озноб_atr3);

            XmlAttribute Пациент_Озноб_atr4 = doc.CreateAttribute("MaxBelief");
            Пациент_Озноб_atr4.Value = "100";
            Пациент_Озноб.Attributes.Append(Пациент_Озноб_atr4);

            XmlAttribute Пациент_Озноб_atr5 = doc.CreateAttribute("Accuracy");
            Пациент_Озноб_atr5.Value = "0";
            Пациент_Озноб.Attributes.Append(Пациент_Озноб_atr5);
            factlist.AppendChild(Пациент_Озноб);
            // ------
            XmlNode Пациент_Кашель = doc.CreateElement("fact");
            XmlAttribute Пациент_Кашель_atr = doc.CreateAttribute("AttrPath");
            Пациент_Кашель_atr.Value = "Пациент.Кашель";
            Пациент_Кашель.Attributes.Append(Пациент_Кашель_atr);

            XmlAttribute Пациент_Кашель_atr2 = doc.CreateAttribute("Value");
            Пациент_Кашель_atr2.Value = Пациент.Кашель.ToString();
            Пациент_Кашель.Attributes.Append(Пациент_Кашель_atr2);

            XmlAttribute Пациент_Кашель_atr3 = doc.CreateAttribute("Belief");
            Пациент_Кашель_atr3.Value = "50";
            Пациент_Кашель.Attributes.Append(Пациент_Кашель_atr3);

            XmlAttribute Пациент_Кашель_atr4 = doc.CreateAttribute("MaxBelief");
            Пациент_Кашель_atr4.Value = "100";
            Пациент_Кашель.Attributes.Append(Пациент_Кашель_atr4);

            XmlAttribute Пациент_Кашель_atr5 = doc.CreateAttribute("Accuracy");
            Пациент_Кашель_atr5.Value = "0";
            Пациент_Кашель.Attributes.Append(Пациент_Кашель_atr5);
            factlist.AppendChild(Пациент_Кашель);

            XmlNode Счетчик_Отчет = doc.CreateElement("fact");
            XmlAttribute Счетчик_Отчет_atr = doc.CreateAttribute("AttrPath");
            Счетчик_Отчет_atr.Value = "Счетчик.Отчет";
            Счетчик_Отчет.Attributes.Append(Счетчик_Отчет_atr);
            XmlAttribute Счетчик_Отчет_atr2 = doc.CreateAttribute("Value");
            Счетчик_Отчет_atr2.Value = Счетчик.Отчет.ToString();
            Счетчик_Отчет.Attributes.Append(Счетчик_Отчет_atr2);
            XmlAttribute Счетчик_Отчет_atr3 = doc.CreateAttribute("Belief");
            Счетчик_Отчет_atr3.Value = "50";
            Счетчик_Отчет.Attributes.Append(Счетчик_Отчет_atr3);
            XmlAttribute Счетчик_Отчет_atr4 = doc.CreateAttribute("MaxBelief");
            Счетчик_Отчет_atr4.Value = "100";
            Счетчик_Отчет.Attributes.Append(Счетчик_Отчет_atr4);
            XmlAttribute Счетчик_Отчет_atr5 = doc.CreateAttribute("Accuracy");
            Счетчик_Отчет_atr5.Value = "0";
            Счетчик_Отчет.Attributes.Append(Счетчик_Отчет_atr5);
            factlist.AppendChild(Счетчик_Отчет);
            XmlNode Счетчик_Секунды = doc.CreateElement("fact");
            XmlAttribute Счетчик_Секунды_atr = doc.CreateAttribute("AttrPath");
            Счетчик_Секунды_atr.Value = "Счетчик.Секунды";
            Счетчик_Секунды.Attributes.Append(Счетчик_Секунды_atr);
            XmlAttribute Счетчик_Секунды_atr2 = doc.CreateAttribute("Value");
            Счетчик_Секунды_atr2.Value = Счетчик.Секунды.ToString();
            Счетчик_Секунды.Attributes.Append(Счетчик_Секунды_atr2);
            XmlAttribute Счетчик_Секунды_atr3 = doc.CreateAttribute("Belief");
            Счетчик_Секунды_atr3.Value = "50";
            Счетчик_Секунды.Attributes.Append(Счетчик_Секунды_atr3);
            XmlAttribute Счетчик_Секунды_atr4 = doc.CreateAttribute("MaxBelief");
            Счетчик_Секунды_atr4.Value = "100";
            Счетчик_Секунды.Attributes.Append(Счетчик_Секунды_atr4);
            XmlAttribute Счетчик_Секунды_atr5 = doc.CreateAttribute("Accuracy");
            Счетчик_Секунды_atr5.Value = "0";
            Счетчик_Секунды.Attributes.Append(Счетчик_Секунды_atr5);
            factlist.AppendChild(Счетчик_Секунды);
            XmlNode Счетчик_Индикатор = doc.CreateElement("fact");
            XmlAttribute Счетчик_Индикатор_atr = doc.CreateAttribute("AttrPath");
            Счетчик_Индикатор_atr.Value = "Счетчик.Индикатор";
            Счетчик_Индикатор.Attributes.Append(Счетчик_Индикатор_atr);
            XmlAttribute Счетчик_Индикатор_atr2 = doc.CreateAttribute("Value");
            Счетчик_Индикатор_atr2.Value = Счетчик.Индикатор.ToString();
            Счетчик_Индикатор.Attributes.Append(Счетчик_Индикатор_atr2);
            XmlAttribute Счетчик_Индикатор_atr3 = doc.CreateAttribute("Belief");
            Счетчик_Индикатор_atr3.Value = "50";
            Счетчик_Индикатор.Attributes.Append(Счетчик_Индикатор_atr3);
            XmlAttribute Счетчик_Индикатор_atr4 = doc.CreateAttribute("MaxBelief");
            Счетчик_Индикатор_atr4.Value = "100";
            Счетчик_Индикатор.Attributes.Append(Счетчик_Индикатор_atr4);
            XmlAttribute Счетчик_Индикатор_atr5 = doc.CreateAttribute("Accuracy");
            Счетчик_Индикатор_atr5.Value = "0";
            Счетчик_Индикатор.Attributes.Append(Счетчик_Индикатор_atr5);
            factlist.AppendChild(Счетчик_Индикатор);
            doc.Save(@"bb2.xml");
        }
    }
    /* 
    class Стакан
    {
        public enum Enum_Состояние { Пуст, Полон };
        public Enum_Состояние Состояние { get; set; }
        public enum Enum_Имя { Дима, Лёва, Саша };
        public Enum_Имя Имя { get; set; }
    } 
    */

    class Счетчик
    {
        public enum Enum_Отчет { Да, Нет };
        public Enum_Отчет Отчет { get; set; }
        public int Секунды { get; set; }
        public int Индикатор { get; set; }
    }
}





class Пациент
{
    public enum Enum_Есть_Нет { Есть, Нет };
    public enum Enum_Температура { Пониженная, Повышенная, Нормальная };
    public enum Enum_Кашель { Сухой, Влажный, Нет };

    public Enum_Температура Температура { get; set; }
    public Enum_Кашель Кашель { get; set; }
    public Enum_Есть_Нет Озноб { get; set; }
    public Enum_Есть_Нет Сыпь { get; set; }
}
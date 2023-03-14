using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using AT_TemporalReasoner;
using MProgram;

namespace AT_DynamicPlanner
{

    public class DPlanner
    {
        public Dictionary<string, List<string>> ClassesDic;
        public Dictionary<string, string> CurrentData;
        public Dictionary<string, string> PreviousData;

        public List<XElement> GeneralRules;
        public List<XElement> PrivateRules;
        public List<XElement> Rules;

        public TemporalReasoner TReasoner;

        public int StepNum;

        XDocument DataDoc;
        XDocument TKBDoc;

        public DPlanner()
        {
            ClassesDic = new Dictionary<string, List<string>>();
            CurrentData = new Dictionary<string, string>();
            PreviousData = new Dictionary<string, string>();

            PrivateRules = new List<XElement>();
            GeneralRules = new List<XElement>();
            Rules = new List<XElement>();

            TReasoner = new TemporalReasoner();
        }

        public void LoadTKB(string TKBName)
        {
            string kbdata = File.ReadAllText(TKBName, Encoding.GetEncoding(1251));
            TKBDoc = XDocument.Parse(kbdata);
        }

        //Загрузка данных за шаг = StepNum
        public void LoadData()
        {
            // Загружаем модель развития событий из файла
            TReasoner.Events.Clear();

            // Устанавливаем StepNum (такт моделирования) из файла
            XDocument ModelDoc = XDocument.Load("Model.xml");
            var isFound = ModelDoc.Root.Elements("temporalModel");
            if (isFound.Any()) // Если в файле нет тега temporalModel, значит начинаем сначала, нулевой такт
            {
                string StepNumFromFile = ModelDoc.Root.Elements("temporalModel").ElementAt(0).Attribute("StepNum").Value;
                StepNum = Convert.ToInt32(StepNumFromFile) + 1;
            }
            else
                StepNum = 0;


            // Загрузка событий из файла
            IEnumerable<XElement> events = ModelDoc.Root.Elements("temporalModel").Elements("events").Elements();
            foreach (XElement ev in events)
            {
                string evName = ev.Attribute("Name").Value;

                List<int> eventTimes = new List<int>();
                string[] words = ev.Value.Split(' ');
                foreach (string word in words)
                {
                    eventTimes.Add(Convert.ToInt32(word));
                }
                TReasoner.Events.Add(evName, eventTimes);
            }

            //Загрузка Интервалов из файла
            var isIntFound = ModelDoc.Root.Elements("temporalModel").Elements("intervals");
            IEnumerable<XElement> intervals = ModelDoc.Root.Elements("temporalModel").Elements("intervals").Elements();
            if (isIntFound.Any()) // если же нет в файле, то оставляем для каждого интервала только (-1, -1)
            {
                // Удаляем времена из словаря, оставляю только названия событий
                foreach (string key in TReasoner.intervalsTimes.Keys.ToList())
                {
                    TReasoner.intervalsTimes[key] = null;
                }
                foreach (XElement interv in intervals)
                {
                    string intName = interv.Attribute("Name").Value;
                    string[] words = interv.Value.Split(';');
                    foreach (string word in words)
                    {
                        TemporalReasoner.StartFinishTime SFT;
                        int comma = word.IndexOf(",");
                        SFT.S = Convert.ToInt32(word.Substring(1, comma - 1));
                        SFT.F = Convert.ToInt32(word.Substring(comma + 2, word.Length - comma - 3));

                        var listSFTimes = new List<TemporalReasoner.StartFinishTime>();
                        if (TReasoner.intervalsTimes[intName] != null)
                            listSFTimes = TReasoner.intervalsTimes[intName];

                        listSFTimes.Add(SFT);
                        TReasoner.intervalsTimes[intName] = listSFTimes;
                    }
                }
            }


            DataDoc = XDocument.Load("BB2.xml");
            //Сохранение предыдущего состояния
            PreviousData.Clear();
            foreach (string key in CurrentData.Keys) PreviousData.Add(key, CurrentData[key]);
            CurrentData.Clear();

            ClassesDic.Clear();

            //Считывание данных
            //XElement StepData = DataDoc.Root.Elements().Single(XElement => XElement.Attribute("Step").Value == StepNum.ToString());

            IEnumerable<XElement> BBData = DataDoc.Root.Elements().ElementAt(0).Elements().ElementAt(0).Elements();
            foreach (XElement fact in BBData)
            {
                string attrPath = fact.Attribute("AttrPath").Value;
                string value;
                if (fact.Attribute("Value") != null)
                {
                    value = fact.Attribute("Value").Value;
                }
                else
                    continue;
                CurrentData.Add(attrPath, value);
            }

        }

        //Загрузка правил
        public void LoadRules()
        {
            GeneralRules.Clear();
            PrivateRules.Clear();

            //Считываем из файла
            IEnumerable<XElement> topClass = from cl in TKBDoc.Root.Elements("classes").Elements() where cl.Attribute("id").Value == "world" select cl;
            IEnumerable<XElement> rules = topClass.Elements("rules").Elements();
            //Сортируем
            foreach (XElement rule in rules)
            {
                //switch (rule.Attribute("General").Value)
                //{
                //    case "True":
                //        GeneralRules.Add(rule);
                //        break;
                //    case "False":
                PrivateRules.Add(rule);
            }
        }

        //Обработка правил
        public void RuleWork()
        {
            //TReasoner.UpdateEventsTime(CurrentData, StepNum);

            Rules.Clear();
            foreach (XElement grule in GeneralRules) Rules.AddRange(TReasoner.EvalGeneral(grule, ClassesDic));
            foreach (XElement prule in PrivateRules) Rules.Add(new XElement(prule));
            TReasoner.EvalRules(Rules, CurrentData, PreviousData, StepNum);

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("TKBnew.xml");
            XmlNode childNode = xdoc.SelectSingleNode("/knowledge-base/classes/class[@id='world']/rules"); // apply your xpath here
            childNode.ParentNode.RemoveChild(childNode);

            XmlDocumentFragment xfrag = xdoc.CreateDocumentFragment();
            xfrag.InnerXml = "<rules></rules>";
            XmlNode childNode2 = xdoc.SelectSingleNode("knowledge-base/classes/class[@id='world']");
            childNode2.AppendChild(xfrag);

            XmlNode RuleschildNode = xdoc.SelectSingleNode("knowledge-base/classes/class[@id='world']/rules");
            foreach (XElement rule in Rules)
            {
                xfrag.InnerXml = rule.ToString();
                RuleschildNode.AppendChild(xfrag);
            }

            xdoc.Save("TKBnewforAT.xml");

        }


        public void SaveModelToFile(string fileName)
        {
            //Запись модели развития событий в файл
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("BB2.xml");

            XmlNode childNode2 = xdoc.SelectSingleNode("/bb");
            XmlElement model = xdoc.CreateElement("temporalModel");
            model.SetAttribute("StepNum", StepNum.ToString());
            childNode2.AppendChild(model);

            XmlNode childNodeEv = xdoc.SelectSingleNode("/bb/temporalModel");
            XmlDocumentFragment xfragEv = xdoc.CreateDocumentFragment();
            XmlElement evnt = xdoc.CreateElement("events");
            childNodeEv.AppendChild(evnt);
            XmlElement intrv = xdoc.CreateElement("intervals");
            childNodeEv.AppendChild(intrv);
            xdoc.Save("Model.xml");

            //Сохранение событий
            XmlNode Events = xdoc.SelectSingleNode("/bb/temporalModel/events");
            foreach (String ev in TReasoner.Events.Keys)
            {
                XmlElement elem = xdoc.CreateElement("eventTimes");
                elem.SetAttribute("Name", ev);
                string timesString = "";
                foreach (int times in TReasoner.Events[ev])
                {
                    timesString += times.ToString() + " ";
                }
                elem.InnerText = timesString.Trim();
                Events.AppendChild(elem);
            }
            // Сохранение интервалов
            XmlNode Intervals = xdoc.SelectSingleNode("/bb/temporalModel/intervals");
            foreach (String interval in TReasoner.intervalsTimes.Keys)
            {
                XmlElement elem = xdoc.CreateElement("intervalTimes");
                elem.SetAttribute("Name", interval);
                string l = "";
                var timesList = TReasoner.intervalsTimes[interval];
                for (var i = 0; i < timesList.Count; i++)
                {
                    l = l + "(" + timesList[i].S + ", " + timesList[i].F + ")" + ";";
                }
                elem.InnerText = l.Trim(';');
                Intervals.AppendChild(elem);
            }

            xdoc.Save("Model.xml");
        }




    }
}

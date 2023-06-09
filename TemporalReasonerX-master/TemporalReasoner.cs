﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AT_TemporalReasoner
{
    public class TemporalReasoner
    {
        //public Dictionary<string, int> EventsTime;
        public Dictionary<string, List<int>> Events;
        public struct StartFinishTime
        {
            public int S;
            public int F;
        }
        StartFinishTime SFT;
        public Dictionary<string, List<StartFinishTime>> intervalsTimes;


        public XDocument AllenDoc;
        public Dictionary<string, List<string>> TemporalSignifications;

        public TemporalReasoner()
        {
            TemporalSignifications = new Dictionary<string, List<string>>() { };
            Events = new Dictionary<string, List<int>>();
            intervalsTimes = new Dictionary<string, List<StartFinishTime>>();

            string allendata = File.ReadAllText("Allen2.xml", Encoding.GetEncoding(1251));
            AllenDoc = XDocument.Parse(allendata);

            foreach (XElement el in AllenDoc.Root.Elements("Intervals").Elements())
            {
                SFT.S = -1;
                SFT.F = -1;
                var list2 = new List<StartFinishTime>();
                intervalsTimes.Add(el.Attribute("Name").Value, list2);
                list2 = intervalsTimes[el.Attribute("Name").Value];
                list2.Add(SFT);
                intervalsTimes[el.Attribute("Name").Value] = list2;
            }
        }

        public void setRuleRef(XElement node, string ruleId) {
            node.SetAttributeValue("ruleRef", ruleId);
            foreach (XElement c in node.Elements())
            {
                setRuleRef(c, ruleId);
            }
        }
        public string getNodePath(XElement node) {
            Dictionary<string, string> attrs = new Dictionary<string,string>();
            foreach (XAttribute attr in node.Attributes()){
                attrs.Add(attr.Name.ToString(), attr.Value.ToString());
            }
            if (attrs.ContainsKey("initialPath"))
            {
                return attrs["initialPath"];
            }
            string path = node.Name.ToString();
            if (node.Parent != null)
            {
                List<XElement> sameNamedNodes = new List<XElement> { };
                foreach (XElement e in node.Parent.Elements().ToList())
                {
                    if (e.Name.ToString() == node.Name.ToString())
                    {
                        sameNamedNodes.Add(e);
                    }
                }
                if (sameNamedNodes.Count > 1)
                {
                    path += @"[" + sameNamedNodes.IndexOf(node) + @"]";
                }
                if (node.Name == "rule")
                {
                    setRuleRef(node, node.Attribute("id").Value);
                }
                path = getNodePath(node.Parent) + @"/" + path;
            }
            return path;
        }
        public void UpdateIntervalsEventsTime(Dictionary<string, string> CurrentData, int StepNum)
        {
            IEnumerable<XElement> events = AllenDoc.Root.Elements("Events").Elements(); //Выбираем события
            XElement t;

            foreach (XElement ev in events)
            {
                String eventName = ev.Attribute("Name").Value;
                t = ev.Element("Formula").Elements().ElementAt(0);

                if (EvalLog(t, CurrentData))
                {

                    if (!Events.ContainsKey(eventName))
                    {
                        var list2 = new List<int>();
                        list2.Clear();
                        Events.Add(eventName, list2);
                        list2 = Events[eventName];
                        list2.Add(StepNum);
                        Events[eventName] = list2;

                    }
                    else
                    {
                        var list = new List<int>();
                        list.Clear();
                        list = Events[eventName];
                        list.Add(StepNum);
                        Events[eventName] = list;
                    }
                }
            }

            IEnumerable<XElement> intervals = AllenDoc.Root.Elements("Intervals").Elements();  //Выбираем интервалы
            XElement open, close;
            StartFinishTime SFT;
            foreach (XElement intv in intervals)
            {
                open = intv.Element("Open").Elements().ElementAt(0);
                close = intv.Element("Close").Elements().ElementAt(0);
                int S = intervalsTimes[intv.Attribute("Name").Value].Last().S;
                int F = intervalsTimes[intv.Attribute("Name").Value].Last().F;
                if (EvalLog(open, CurrentData))  // условие открытия выполнено?
                {
                    if (S <= F) // если интервал закрыт
                    {
                        SFT.S = StepNum;
                        SFT.F = -1;
                        var listSFTimes = intervalsTimes[intv.Attribute("Name").Value];
                        listSFTimes.Add(SFT);
                        intervalsTimes[intv.Attribute("Name").Value] = listSFTimes;
                    }
                }
                else
                {
                    if (EvalLog(close, CurrentData)) // условие закрытия выполнено?
                        if (S > F) // если интервал открыт
                        {
                            var list2 = intervalsTimes[intv.Attribute("Name").Value];
                            var sft = list2[list2.Count - 1];
                            sft.F = StepNum;
                            list2[list2.Count - 1] = sft;
                        }
                }
            }
        }

        public void EvalAllenRules(List<XElement> rules, int StepNum)
        {
            foreach (XElement rule in rules.Elements("condition")) Eval(rule, StepNum);
        }

        private XElement Eval(XElement rule, int StepNum)  // Переделать!
        {
            switch (rule.Name.ToString())
            {
                case "EvIntRel":
                    {
                        XElement result = EvalAlEventInterval(rule);
                        return result;
                    }
                case "AlAttr":
                    {
                        XElement result = EvalAlAttr(rule, StepNum);
                        return result;
                    }
                case "IntRel":
                    {
                        XElement result = EvalAlIntervals(rule);
                        return result;
                    }
                case "EvRel":
                    {
                        XElement result = EvalAlEvents(rule);
                        return result;
                    }
            }
            
            foreach (XElement xel in rule.Elements())
            {
                switch (xel.Name.ToString())
                {
                    case "and":
                        foreach (XElement xel2 in xel.Elements())
                        {
                            XElement result = Eval(xel2, StepNum);
                            return result;
                        }
                        break;
                    case "or":
                        foreach (XElement xel2 in xel.Elements())
                        {
                            XElement result = Eval(xel2, StepNum);
                            return result;
                        }
                        break;
                    case "not":
                        foreach (XElement xel2 in xel.Elements())
                        {
                            XElement result = Eval(xel2, StepNum);
                            return result;
                        }
                        break;
                    case "EvIntRel":
                        {
                            XElement result = EvalAlEventInterval(xel);
                            return result;
                        }
                    case "AlAttr":
                        {
                            XElement result = EvalAlAttr(xel, StepNum);
                            return result;
                        }
                    case "IntRel":
                        {
                            XElement result = EvalAlIntervals(xel);
                            return result;
                        }
                    case "EvRel":
                        {
                            XElement result = EvalAlEvents(xel);
                            return result;
                        }

                }
            }
            return rule;
        }

        private XElement EvalAlIntervals(XElement Node)
        {
            string Int1 = Node.Elements().ElementAt(0).Attribute("Value").Value;
            int S1 = intervalsTimes[Int1].Last().S;
            int F1 = intervalsTimes[Int1].Last().F;
            string Int2 = Node.Elements().ElementAt(1).Attribute("Value").Value;
            int S2 = intervalsTimes[Int2].Last().S;
            int F2 = intervalsTimes[Int2].Last().F;
            switch (Node.Attribute("Value").Value)
            {
                case "b":
                    if ((F1 > 0) && (F1 < S2))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "bi":
                    if ((F2 > 0) && (F2 < S1))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "m":
                    if ((F1 > 0) && (F1 == S2))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "mi":
                    if ((F2 > 0) && (F2 == S1))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "s":
                    if ((F1 > 0) && (S1 == S2) && ((F1 < F2) || (F2 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "si":
                    if ((F2 > 0) && (S2 == S1) && ((F2 < F1) || (F1 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "f":
                    if ((F1 > 0) && (F1 == F2) && (S1 > S2))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "fi":
                    if ((F2 > 0) && (F2 == F1) && (S2 > S1))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "d":
                    if ((F1 > 0) && (S1 > S2) && ((F1 < F2) || (F2 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "di":
                    if ((F2 > 0) && (S2 > S1) && ((F2 < F1) || (F1 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "o":
                    if ((S1 < S2) && (F1 > S2) && ((F1 < F2) || (F2 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "oi":
                    if ((S2 < S1) && (F2 > S1) && ((F2 < F1) || (F1 < 0)))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "e":
                    if ((F1 > S1) && (S1 == S2) && (F1 == F2))
                    {
                        setTruthVal(Node, "TRUE");
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;
            }
            return Node;
        }

        private XElement EvalAlEvents(XElement Node)
        {
            string ev1 = Node.Elements().ElementAt(0).Attribute("Name").Value;
            string ev2 = Node.Elements().ElementAt(1).Attribute("Name").Value;
            switch (Node.Attribute("Value").Value)
            {
                case "b":

                    if (Events.ContainsKey(ev1) && Events.ContainsKey(ev2))
                    {
                        if (Events[ev1].Last() < Events[ev2].Last())
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "e":
                    if (Events.ContainsKey(ev1) && Events.ContainsKey(ev2))
                    {
                        if (Events[ev1].Last() == Events[ev2].Last())
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;
            }
            return Node;
        }

        private XElement EvalAlEventInterval(XElement Node)
        {
            string ev = Node.Elements().ElementAt(0).Attribute("Name").Value;
            string interval = Node.Elements().ElementAt(1).Attribute("Name").Value;
            //int intervalStartTime = EventsTime[interval + ".S"];
            // int intervalEndTime = EventsTime[interval + ".F"];
            int intervalStartTime = intervalsTimes[interval].Last().S;
            int intervalEndTime = intervalsTimes[interval].Last().F;
            switch (Node.Attribute("Value").Value)
            {
                case "b":
                    if (Events.ContainsKey(ev))
                    {
                        if (Events[ev].Last() < intervalStartTime)
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "s":
                    if (Events.ContainsKey(ev))
                    {
                        if (Events[ev].Last() == intervalStartTime)
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "d":
                    if (Events.ContainsKey(ev))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("interval.txt", true))
                        {
                            sw.Write(Events[ev].Last().ToString() + " d (");
                            sw.Write(intervalStartTime + ", ");
                            sw.Write(intervalEndTime + ")");
                        }
                        if ((Events[ev].Last() > intervalStartTime && intervalStartTime != -1) &&
                            (Events[ev].Last() < intervalEndTime || intervalEndTime == -1)) 
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("interval.txt", true))
                            {
                                sw.WriteLine(" = true");
                            }
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("interval.txt", true))
                            {
                                sw.WriteLine(" = false");
                            }
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "f":
                    if (Events.ContainsKey(ev))
                    {
                        if ((Events[ev].Last() == intervalEndTime) && intervalEndTime != -1)
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;

                case "a":
                    if (Events.ContainsKey(ev))
                    {
                        if ((Events[ev].Last() > intervalEndTime) && intervalEndTime != -1)
                        {
                            setTruthVal(Node, "TRUE");
                        }
                        else
                        {
                            setTruthVal(Node, "FALSE");
                        }
                    }
                    else
                    {
                        setTruthVal(Node, "FALSE");
                    }
                    break;
            }
            return Node;
        }

        private XElement EvalAlAttr(XElement Node, int StepNum)
        {
            string value = Node.Elements().ElementAt(0).Attribute("Value").Value;
            if (value.Contains("."))
            {
                int dotPosition = value.IndexOf(".");
                int k = value.Length - dotPosition - 1;
                string op = value.Substring(dotPosition + 1, k);
                switch (op)
                {
                    case "length":
                        string intervalName = value.Substring(0, dotPosition);
                        int lengthNum = Convert.ToInt32(Node.Elements().ElementAt(1).Attribute("Value").Value);
                        int F = intervalsTimes[intervalName].Last().F;
                        int S = intervalsTimes[intervalName].Last().S;
                        int intervalLength;
                        if (F == -1)
                        {
                            if (S != -1)
                                intervalLength = StepNum - S;
                            else
                                intervalLength = 0;
                        }
                        else
                        {
                            intervalLength = F - S;
                        }

                        switch (Node.Attribute("Value").Value)
                        {
                            case "eq":
                                if (intervalLength == lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                            case "ne":
                                if (intervalLength != lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                            case "gt":
                                if (intervalLength > lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                            case "ge":
                                if (intervalLength >= lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                            case "lt":
                                if (intervalLength < lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                            case "le":
                                if (intervalLength <= lengthNum)
                                {
                                    setTruthVal(Node, "TRUE");
                                }
                                else
                                {
                                    setTruthVal(Node, "FALSE");
                                }
                                break;
                        }

                        break;


                    case "count":
                        string element = Node.Elements().ElementAt(0).Name.ToString();
                        if (element == "AttrInterval") // обработка для интервалов
                        {
                            string intName = value.Substring(0, dotPosition);
                            int countNum = Convert.ToInt32(Node.Elements().ElementAt(1).Attribute("Value").Value);
                            int intervalCount = intervalsTimes[intName].Count() - 1; //-1 т.к. не считаем пару (-1,-1)
                            switch (Node.Attribute("Value").Value)
                            {
                                case "eq":
                                    if (intervalCount == countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "ne":
                                    if (intervalCount != countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "gt":
                                    if (intervalCount > countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "ge":
                                    if (intervalCount >= countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "lt":
                                    if (intervalCount < countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "le":
                                    if (intervalCount <= countNum)
                                    {
                                        setTruthVal(Node, "TRUE");
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                            }

                        }
                        if (element == "AttrEvent") // обработка для событий
                        {
                            string eventName = value.Substring(0, dotPosition);
                            int countNum = Convert.ToInt32(Node.Elements().ElementAt(1).Attribute("Value").Value);
                            int eventCount;
                            if (Events.ContainsKey(eventName))
                            {
                                eventCount = Events[eventName].Count;
                            }
                            else
                            {
                                eventCount = 0;
                            }
                            switch (Node.Attribute("Value").Value)
                            {
                                case "eq":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount == countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "ne":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount != countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "gt":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount > countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "ge":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount >= countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "lt":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount < countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                                case "le":
                                    if (Events.ContainsKey(eventName))
                                    {
                                        if (eventCount <= countNum)
                                        {
                                            setTruthVal(Node, "TRUE");
                                        }
                                        else
                                        {
                                            setTruthVal(Node, "FALSE");
                                        }
                                    }
                                    else
                                    {
                                        setTruthVal(Node, "FALSE");
                                    }
                                    break;
                            }
                        }
                        break;

                }

            }
            return Node;
        }

        public void setTruthVal(XElement Node, string truthVal)
        {
            //Node.Name = "TruthVal";
            //Node.RemoveAll();
            //Node.Add(new XAttribute("Value", truthVal));
            
            // --- Григорьев А.А. 14.06.2023 закомментил все, что ниже и написал установку означиваний
            //if (truthVal == "TRUE")
            //{
            //    Node.RemoveAll();
            //    Node.Name = "eq";
            //    Node.Add(new XElement("value", 1));
            //    Node.Add(new XElement("value", 1));
            //}
            //if (truthVal == "FALSE")
            //{
            //    Node.RemoveAll();
            //    Node.Name = "eq";
            //    Node.Add(new XElement("value", 1));
            //    Node.Add(new XElement("value", 2));
            //}

            string nodePath = getNodePath(Node);

            if (TemporalSignifications.ContainsKey(nodePath))
            {
                TemporalSignifications.Remove(nodePath);
            }
            string ruleId = "";
            string temporalExpressionData = "";
            string sEvInt = "EvIntRel";
            string sAlAttr = "AlAttr";
            string sIntRel = "IntRel";
            string sEvRel = "EvRel";
            if (Node.Name == sEvInt)
            {
                temporalExpressionData = "Event(" + Node.Descendants("Event").First().Attribute("Name").Value + ") " +
                    Node.Attribute("Value").Value + 
                    " Interval(" + Node.Descendants("Interval").First().Attribute("Name").Value + ")";
            }
            else if (Node.Name == sIntRel)
            {
                temporalExpressionData = "Interval(" + Node.Descendants("Interval").First().Attribute("Name").Value + ") " +
                    Node.Attribute("Value").Value +
                    " Interval(" + Node.Descendants("Interval").Last().Attribute("Name").Value + ")";
            }
            else if (Node.Name == sEvRel)
            {
                temporalExpressionData = "Event(" + Node.Descendants("Event").First().Attribute("Name").Value + ") " +
                    Node.Attribute("Value").Value +
                    " Event(" + Node.Descendants("Event").Last().Attribute("Name").Value + ")";
            }
            else if (Node.Name == sAlAttr)
            {
                temporalExpressionData = "Выражение над атрибутами";
            }

            XElement n = Node;
            while (n.Parent != null)
            {
                if (n.Attribute("ruleRef") != null)
                {
                    ruleId = n.Attribute("ruleRef").Value;
                    break;
                }
                if (n.Name == "rule") {
                    ruleId = n.Attribute("id").Value;
                    break;
                }
                n = n.Parent;
            }
            List<string> v = new List<string>() {truthVal, "ПРАВИЛО " + ruleId, temporalExpressionData};
            TemporalSignifications.Add(nodePath, v);
            
            
        }


        //Означивание When-правила
        public void EvalWhen(XElement rule, int StepNum)
        {
            XElement per = rule.Element("Period");
            if (StepNum % Convert.ToInt32(per.Attribute("Value").Value) == 0)
            {
                rule.SetAttributeValue("Type", "Normal");
                per.Remove();
            }
        }

        //Означивание нового Whenever-правила - разрешаем напрямую указывать события
        public void EvalWheneverNew(XElement rule, Dictionary<string, string> CurrentDara, Dictionary<string, List<int>> Events, int StepNum)
        {
            String eventName = rule.Elements("If").Elements("Events").Elements().ElementAt(0).Attribute("Name").Value;
            rule.SetAttributeValue("Type", "Normal");
            XElement NodeEv = rule.Elements("If").ElementAt(0);
            if (Events.ContainsKey(eventName))
            {
                if (Events[eventName].Contains(StepNum))
                {
                    setTruthVal(NodeEv, "TRUE");
                }
                else
                {
                    setTruthVal(NodeEv, "FALSE");
                }
            }
            else
            {
                setTruthVal(NodeEv, "FALSE");
            }
            String intervalName = rule.Elements("If").Elements("Intervals").Elements().ElementAt(0).Attribute("Name").Value;
            rule.SetAttributeValue("Type", "Normal");
            XElement NodeInt = rule.Elements("If").ElementAt(0);
            if (intervalsTimes.ContainsKey(intervalName))
            {
                if (intervalsTimes[intervalName].Last().F == -1)
                {
                    setTruthVal(NodeInt, "TRUE");
                }
                else
                {
                    setTruthVal(NodeInt, "FALSE");
                }
            }
            else
            {
                setTruthVal(NodeInt, "FALSE");
            }
        }

        //Означивание Whenever-правила
        public void EvalWhenever(XElement rule, Dictionary<string, string> CurrentData, Dictionary<string, string> PreviousData)
        {
            XElement cas = rule.Element("Case");
            XElement formula = cas.Elements().ElementAt(0);

            if (PreviousData.Count == 0)
            {
                if (EvalLog(formula, CurrentData))
                {
                    rule.SetAttributeValue("Type", "Normal");
                    cas.Remove();
                }
                return;
            }

            if (EvalLog(formula, CurrentData) && (!EvalLog(formula, PreviousData)))  //why need PreviousData compare?
            {
                rule.SetAttributeValue("Type", "Normal");
                cas.Remove();
            }
        }

        //Означить все правила в соответствии с  типом
        public void EvalRules(List<XElement> rules, Dictionary<string, string> CurrentData, Dictionary<string, string> PreviousData, int StepNum)
        {
            EvalAllenRules(rules, StepNum);
            //foreach (XElement rule in rules)
            //{
            //    switch (rule.Attribute("Type").Value)
            //    {
            //        case "When":
            //            EvalWhen(rule, StepNum);
            //            break;
            //        case "Whenever":
            //            EvalWhenever(rule, CurrentData, PreviousData);
            //            break;
            //        case "WheneverNew":
            //            EvalWheneverNew(rule, CurrentData, Events, StepNum);
            //            break;
            //    }
            //}

            //for (int i = 0; i < rules.Count; i++)
            //{
            //    XElement rule = rules[i];
            //    if (rule.Attribute("Type").Value != "Normal")
            //    {
            //        rules.Remove(rule);
            //        i--;
            //    }
            //}
        }

        //Означивание общего правила
        public List<XElement> EvalGeneral(XElement rule, Dictionary<string, List<string>> ClassesDic)
        {
            //MessageBox.Show(rule.ToString());
            XElement rulecl = rule;
            List<XElement> res = new List<XElement>();
            XElement vars = rulecl.Element("Variables");
            if (vars.Elements().Count() == 0)
            {
                rulecl.SetAttributeValue("General", "False");
                vars.Remove();
                res.Add(rulecl);
                return res;
            }
            else
            {
                XElement firstvar = vars.Element("Variable");
                string var = firstvar.Attribute("Name").Value;
                string cl = firstvar.Attribute("Class").Value;

                firstvar.Remove();

                string rulename = rulecl.Attribute("Name").Value;
                string tp = rulecl.Attribute("Type").Value;

                foreach (string obj in ClassesDic[cl])
                {
                    XElement newrule = new XElement("Rule", new XAttribute("Name", rulename + "." + obj), new XAttribute("General", "True"), new XAttribute("Type", tp), new XAttribute("initialPath", obj + @"@" + getNodePath(rulecl)));
                    foreach (XElement el in rulecl.Elements()) newrule.Add(el);

                    EvalGenInNode(newrule.Element("If"), var, obj);
                    EvalGenInNode(newrule.Element("Then"), var, obj);

                    res.AddRange(EvalGeneral(newrule, ClassesDic));
                }

                return res;
            }
        }

        //Означивание переменных проходом
        private void EvalGenInNode(XElement Node, string Var, string Val)
        {
            if (Node.Name.ToString() == "Attribute")
            {
                string Attr = Node.Attribute("Value").Value;
                string[] Parts = Attr.Split('.');
                if (Parts[0] == Var) Node.SetAttributeValue("Value", Val + "." + Parts[1]);
            }
            else
            {
                foreach (XElement el in Node.Elements()) EvalGenInNode(el, Var, Val);
            }
        }

        //Расчет арифметического выражения
        private double EvalAr(XElement Node, Dictionary<string, string> CurrentData)
        {
            switch (Node.Name.ToString())
            {
                case "Number":
                    return Convert.ToDouble(Node.Attribute("Value").Value);

                case "Attribute":
                    return Convert.ToDouble(CurrentData[Node.Attribute("Value").Value]);


                case "ArOp":
                    switch (Node.Attribute("Value").Value)
                    {
                        case "+":
                            return EvalAr(Node.Elements().ElementAt(0), CurrentData) + EvalAr(Node.Elements().ElementAt(1), CurrentData);
                        case "-":
                            if (Node.Elements().Count() == 2) return EvalAr(Node.Elements().ElementAt(0), CurrentData) - EvalAr(Node.Elements().ElementAt(1), CurrentData);
                            else return 0 - EvalAr(Node.Elements().ElementAt(0), CurrentData);
                        case "*":
                            return EvalAr(Node.Elements().ElementAt(0), CurrentData) * EvalAr(Node.Elements().ElementAt(1), CurrentData);
                        case "/":
                            return EvalAr(Node.Elements().ElementAt(0), CurrentData) / EvalAr(Node.Elements().ElementAt(1), CurrentData);
                    }
                    break;
            }

            return 0;
        }

        //Расчет логических выражений
        private bool EvalLog(XElement Node, Dictionary<string, string> CurrentData)
        {
            string value = Node.Elements().ElementAt(0).Attribute("Value").Value;
            if (CurrentData.ContainsKey(value))
            {
                switch (Node.Name.ToString())
                {
                    case "EqOp":
                        XElement el;
                        switch (Node.Attribute("Value").Value)
                        {
                            case "eq":
                                el = Node.Elements().ElementAt(1);
                                if (el.Name.ToString() == "String")
                                {
                                    return (CurrentData[Node.Elements().ElementAt(0).Attribute("Value").Value.ToString()] == Node.Elements().ElementAt(1).Attribute("Value").Value.ToString());
                                }
                                else
                                {
                                    return (EvalAr(Node.Elements().ElementAt(0), CurrentData) == EvalAr(Node.Elements().ElementAt(1), CurrentData));
                                }

                            case "ne":
                                el = Node.Elements().ElementAt(1);
                                if (el.Name.ToString() == "String")
                                {
                                    return (CurrentData[Node.Elements().ElementAt(0).Attribute("Value").Value] != Node.Elements().ElementAt(1).Attribute("Value").Value);
                                }
                                else
                                {
                                    return (EvalAr(Node.Elements().ElementAt(0), CurrentData) != EvalAr(Node.Elements().ElementAt(1), CurrentData));
                                }

                            case "gt":
                                return (EvalAr(Node.Elements().ElementAt(0), CurrentData) > EvalAr(Node.Elements().ElementAt(1), CurrentData));

                            case "ge":
                                return (EvalAr(Node.Elements().ElementAt(0), CurrentData) >= EvalAr(Node.Elements().ElementAt(1), CurrentData));

                            case "lt":
                                return (EvalAr(Node.Elements().ElementAt(0), CurrentData) < EvalAr(Node.Elements().ElementAt(1), CurrentData));

                            case "le":
                                return (EvalAr(Node.Elements().ElementAt(0), CurrentData) <= EvalAr(Node.Elements().ElementAt(1), CurrentData));
                        }
                        break;
                    case "LogOp":
                        switch (Node.Attribute("Value").Value)
                        {
                            case "NOT":
                                return !EvalLog(Node.Elements().ElementAt(0), CurrentData);
                            case "AND":
                                return EvalLog(Node.Elements().ElementAt(0), CurrentData) && EvalLog(Node.Elements().ElementAt(1), CurrentData);
                            case "OR":
                                return EvalLog(Node.Elements().ElementAt(0), CurrentData) || EvalLog(Node.Elements().ElementAt(1), CurrentData);
                            case "XOR":
                                return EvalLog(Node.Elements().ElementAt(0), CurrentData) ^ EvalLog(Node.Elements().ElementAt(1), CurrentData);
                        }
                        break;
                    case "TruthVal":
                        switch (Node.Attribute("Value").Value)
                        {
                            case "TRUE":
                                return true;
                            case "FALSE":
                                return false;
                        }
                        break;
                }
            }
            else
                return false;

            return false;
        }
    }
}

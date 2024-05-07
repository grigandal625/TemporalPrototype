using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using AT_TemporalReasoner;
using AT_DynamicPlanner;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;

[assembly: AssemblyKeyFile("TReasoner.snk")]
namespace MProgram
{

    [Guid("952D242C-00B2-403A-84F2-A5BF42C80B60")]
    public interface ITemporalReasoner
    {
        String Name { get; set; }
        System.Object Broker { get; set; }
        void Configurate(string Config);
        void ProcessMessage(string SenderName, string MessageText, System.Object OleVariant);
    };

    [Guid("786CBF15-DFAC-4C11-A748-F88F85FDB751")]
    public class TemporalReasonerX : ITemporalReasoner
    {
        public TemporalReasonerX()
        { }

        System.Object m_broker;
        string m_name;
        string TKBfileName;
        string processedNTBfileName;
        string eventsIntervalsFileName;
        string ModelFileName;
        string bbFileName;

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public System.Object Broker
        {
            get
            {
                return m_broker;
            }
            set
            {
                m_broker = value;
            }
        }


        public void Configurate(string Config)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(Config);
            TKBfileName = xdoc.SelectSingleNode("/config/FileName").InnerText;
            processedNTBfileName = xdoc.SelectSingleNode("/config/FileNameAT").InnerText;
            eventsIntervalsFileName = xdoc.SelectSingleNode("/config/EventsIntervals").InnerText;
            ModelFileName = xdoc.SelectSingleNode("/config/ModelFileName").InnerText;

            //TKBfileName = "TKBnew.xml";
            //processedNTBfileName = "TKBnewforAT.xml";
            //eventsIntervalsFileName = "Allen2.xml";
            //ModelFileName = "Model.xml";
            //bbFileName = "bb2.xml";
        }

        public void ProcessMessage(string SenderName, string MessageText, System.Object OleVariant)
        {
            // Работа темпорального решателя
            DPlanner Planner;
            Planner = new DPlanner(eventsIntervalsFileName);
            Planner.LoadTKB(TKBfileName);
            Planner.LoadRules();
            Planner.LoadData();
            Planner.TReasoner.UpdateIntervalsEventsTime(Planner.CurrentData, Planner.StepNum);
            Planner.RuleWork();
            Planner.SaveModelToFile(ModelFileName);
            // Конец работы темпорального решателя

            /*
            string clearMemory = "<message ProcName=\"TKnowledgeBase.ClearWorkMemory\"></message>";
            m_broker.GetType().InvokeMember("SendMessage", BindingFlags.InvokeMethod, null, m_broker, new[] { "Dialoger", "ESKernel", clearMemory, m_broker });

            string runScripter = "<message ProcName=\"Run\"><func name=\"WriteBB\" module=\"report\" /></message>";
            m_broker.GetType().InvokeMember("ConfigurateObject", BindingFlags.InvokeMethod, null, m_broker, new[] { "Scripter", "<config><script Timeout=\"0\" Language=\"VBScript\" Silent=\"true\" /><file>report.vbs</file></config>" });
            // Запуск АТ-РЕШАТЕЛЯ
            m_broker.GetType().InvokeMember("SendMessage", BindingFlags.InvokeMethod, null, m_broker, new[] { "Dialoger", "Scripter", runScripter, m_broker });

            string runATsolver = "<message ProcName=\"TSolve\"/>";
            m_broker.GetType().InvokeMember("ConfigurateObject", BindingFlags.InvokeMethod, null, m_broker, new[] { "ESKernel", @"<config><FileName>TKBnewforAT.xml</FileName></config>" });
            // Запуск АТ-РЕШАТЕЛЯ          
            m_broker.GetType().InvokeMember("SendMessage", BindingFlags.InvokeMethod, null, m_broker, new[] { "Dialoger", "ESKernel", runATsolver, m_broker });
            */
        }


        public void Stop()
        {
            Environment.Exit(0);
        }

    }

    class Program
    {
        public static void Main()
        {
            TemporalReasonerX tr = new TemporalReasonerX();
            tr.Configurate("");
            tr.ProcessMessage("", "", new Object());
        }
    }


}
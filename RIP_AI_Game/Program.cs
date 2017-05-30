using System;
using System.Collections.Generic;
using System.Xml;


namespace RIP_AI_Game
{
    class Program
    {

        static readonly string titleCard = "Slowly, your flashlight clicks on, illuminating the darkness before you...\n\n\tSICUT HOMULLUS\n\tCopr. 2016 R.N.Ward\n";
        static readonly string endCard = "\nYour flashlight flickers and dies, leaving you alone in the faint glow of the computer..\n\npress enter to close the game.";

        static void Main(string[] args)
        {
            Console.WriteLine(titleCard);
            Tuple<Dictionary<string, Room>, Dictionary<string, Dictionary<string, Tuple<string, Predicate>>>> rooms = LoadRooms("Rooms.xml");
            PlotNode p = LoadPlot("Plot.xml", rooms.Item1);
            Dictionary<string, Dictionary<string[], string>> responses = LoadResponses("Responses.xml");
            Game game = new Game(rooms.Item2, rooms.Item1, p,  responses);
            game.play();
            Console.WriteLine(endCard);
            Console.ReadLine();
        }
        //loading stuff from xml

        static Dictionary<string,Dictionary<string[],string>> LoadResponses(string file)
        {
            /*
            <responces>
                <messenger name = >
                    <response>
                        <input></input>
                        <output></output>
            */
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            Dictionary < string, Dictionary<string[], string>> r = new Dictionary<string, Dictionary<string[], string>>();
            foreach(XmlNode xn in doc.SelectSingleNode("responses").SelectNodes("messenger"))
            {
                r.Add(xn.Attributes["name"].Value, new Dictionary<string[], string>());
                foreach(XmlNode m in xn.SelectNodes("response"))
                {
                    List<string> inputs = new List<string>();
                    foreach(XmlNode o in m.SelectNodes("input"))
                    {
                        inputs.Add(o.InnerText);
                    }
                    r[xn.Attributes["name"].Value].Add(inputs.ToArray(), m.SelectSingleNode("output").InnerText);
                }
            }
            return r;
        }

        static Tuple<Dictionary<string,Room>,Dictionary<string,Dictionary<string,Tuple<string,Predicate>>>>
            LoadRooms(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            Dictionary<string, Room> rooms = new Dictionary<string, Room>();
            Dictionary<string, Dictionary<string, Tuple<string, Predicate>>> doors = new Dictionary<string, Dictionary<string, Tuple<string, Predicate>>>();
            foreach(XmlNode n in doc.SelectSingleNode("rooms").SelectNodes("room"))
            {
                string name = n.Attributes["name"].Value;
                rooms.Add(name, new Room(n.FirstChild.Value));
                doors.Add(name, new Dictionary<string, Tuple<string, Predicate>>());
                foreach(XmlNode o in n.SelectNodes("door"))
                {
                    doors[name].Add(o.Attributes["direction"].Value, new Tuple<string,Predicate>(o.Attributes["to"].Value, loadPred(o.SelectSingleNode("pred"), rooms)));
                }
            }
            return new Tuple<Dictionary<string, Room>, Dictionary<string, Dictionary<string, Tuple<string, Predicate>>>>(rooms, doors);
        }

        static PlotNode LoadPlot(string file, Dictionary<string, Room> rooms)
        {
            string start = "start";
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
//          Console.WriteLine(doc.LastChild.InnerXml);
            Dictionary<string, PlotNode> nodes = new Dictionary<string, PlotNode>();
            foreach(XmlNode n in doc.SelectSingleNode("plot").SelectNodes("node"))
            {
//              Console.WriteLine(n.InnerXml);
                if (n.Attributes.Count > 1 && n.Attributes["class"].Value == "predFork")
                {
                    Dictionary<Predicate, PlotNode.Event> paths = new Dictionary<Predicate, PlotNode.Event>();
                    foreach(XmlNode xm in n.SelectNodes("path"))
                    {
                        paths.Add(loadPred(xm.SelectSingleNode("pred"),rooms), loadEvent(xm.SelectSingleNode("event"),nodes));
                    }
                    nodes.Add(n.Attributes["name"].Value, new PredForkPlotNode(paths));
                }
                else
                {
                    Predicate pred = loadPred(n.SelectSingleNode("pred"), rooms);
                    PlotNode.Event e = loadEvent(n.SelectSingleNode("event"), nodes);
                    nodes.Add(n.Attributes["name"].Value, new PlotNode(pred, e));
                }
            }
            //plotless testing
            //return null;
            return nodes[start];
        }

        static PlotNode.Event loadEvent(XmlNode xn, Dictionary<string, PlotNode> nodes)
        {
//            Console.WriteLine(xn.InnerText);
            //text
            if (xn.Attributes["class"].Value == "text")
            {
                return new PlotNode.TextEvent(xn.InnerText, nodes[xn.Attributes["next"].Value], 
                    (xn.Attributes["prio"] == null ? 0 : Int32.Parse(xn.Attributes["prio"].Value)));
            }
            //text fork
            else if (xn.Attributes["class"].Value == "textFork")
            {
                Dictionary<string, PlotNode> forkNodes = new Dictionary<string, PlotNode>();
                foreach (XmlNode xm in xn.SelectNodes("choice"))
                {
                    forkNodes.Add(xm.Attributes["text"].Value, nodes[xm.Attributes["next"].Value]);
                }
                return new PlotNode.TextForkEvent(xn.InnerText, forkNodes);
            }
            //message
            else if (xn.Attributes["class"].Value == "messenger")
            {
                return new PlotNode.MessageEvent(xn.Attributes["messenger"].Value, xn.InnerText,
                        (xn.Attributes["next"].Value == "null"? null : nodes[xn.Attributes["next"].Value]));
            }
            //clear messenger
            else if(xn.Attributes["class"].Value == "clear")
            {
                return new PlotNode.ClearMessengerEvent(xn.Attributes["messenger"].Value, nodes[xn.Attributes["next"].Value]);
            }
            //file
            else if (xn.Attributes["class"].Value == "file")
            {
                return new PlotNode.FileEvent(xn.InnerText, nodes[xn.Attributes["next"].Value]);
            }
            //add response
            else if(xn.Attributes["class"].Value == "addResponse")
            {
                Dictionary<string[], string> responses = new Dictionary<string[], string>();
                foreach(XmlNode xm in xn.SelectNodes("response"))
                {
                    List<string> l = new List<string>();
                    foreach(XmlNode xo in xm.SelectNodes("input"))
                    {
                        l.Add(xo.InnerText);
                    }
                    responses.Add(l.ToArray(),xm.SelectSingleNode("output").InnerText);
                }
                return new PlotNode.AddResponsesEvent(xn.Attributes["messenger"].Value,responses,nodes[xn.Attributes["next"].Value]);
            }
            else
            {
                return null;
            }
        }

        static Predicate loadPred(XmlNode xn, Dictionary<string, Room> rooms)
        { 
            if(xn == null)
            {
                return null;
            }
            //room
            else if(xn.Attributes["class"].Value == "room")
            {
                return new RoomPred(rooms[xn.Attributes["room"].Value]);
            }
            //file
            else if(xn.Attributes["class"].Value == "file")
            {
                return new FilePred(xn.Attributes["file"].Value);
            }
            //timer
            else if(xn.Attributes["class"].Value == "timer")
            {
                return new TimerPred(Int32.Parse(xn.Attributes["count"].Value));
            }
            //message
            else if(xn.Attributes["class"].Value == "messenger")
            {
                return new MessagePred(xn.Attributes["messenger"].Value, xn.InnerText);
            }
            //not
            else if(xn.Attributes["class"].Value == "not")
            {
                return new NotPred(loadPred(xn.SelectSingleNode("pred"), rooms));
            }
            //of
            else if(xn.Attributes["class"].Value == "or")
            {
                List<Predicate> preds = new List<Predicate>();
                foreach (XmlNode xm in xn.SelectNodes("pred"))
                {
                    preds.Add(loadPred(xm, rooms));
                }
                return new OrPred(preds.ToArray());
            }
            else
            {
                return new NoPred();
            }
        }
    }

}

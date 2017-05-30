using System;
using System.Collections.Generic;

namespace RIP_AI_Game
{
    public class PlotNode
    {

        //events
        public abstract class Event { public abstract PlotNode run(Game g); }
        public abstract class LinearEvent : Event
        {
            protected PlotNode node;
            public LinearEvent(PlotNode node)
            {
                this.node = node;
            }
            public override PlotNode run(Game g)
            {
                return node;
            }
        }
        public class MessageEvent : LinearEvent
        {
            private string sender;
            private string message;

            public MessageEvent(string sender, string message, PlotNode pn) :
                base(pn)
            {
                this.sender = sender;
                this.message = message;
            }

            public override PlotNode run(Game g)
            {
                g.writeMessage(sender, message);
                return base.run(g);
            }
        }
        public class TextEvent : LinearEvent
        {
            private int prio;
            private string text;
            public TextEvent(string text, PlotNode pn, int prio = 0) :
                base(pn)
            {
                this.text = text;
                this.prio = prio;
            }
            public override PlotNode run(Game g)
            {
                g.print("\n" + text, prio);
                return base.run(g);
            }
        }

        public abstract class ForkEvent : Event
        {
            private Dictionary<string, PlotNode> nodes;
            public ForkEvent(Dictionary<string, PlotNode> nodes)
            {
                this.nodes = nodes;
            }

            public abstract override PlotNode run(Game g);

            protected PlotNode _run(string s)
            {
                if (!nodes.ContainsKey(s))
                {
                    s = "dflt";
                }
                return nodes[s];
            }
        }

        public class TextForkEvent : ForkEvent
        {
            private string text;
            public TextForkEvent(string text, Dictionary<string, PlotNode> nodes) :
                base(nodes)
            {
                this.text = text;
            }
            public override PlotNode run(Game g)
            {
                Console.WriteLine(text);
                return _run(Console.ReadLine());
            }
        }
        
        public class AddResponsesEvent : LinearEvent
        {
            private Dictionary<string[], string> responses;
            private string messenger;
            public AddResponsesEvent(string messenger, Dictionary<string[], string> responses, PlotNode node):
                base(node)
            {
                this.messenger = messenger;
                this.responses = responses;
            }
            public override PlotNode run(Game g)
            {
                foreach (string[] key in responses.Keys)
                {
                    g.getMessenger(messenger).addResponse(key, responses[key]);
                }
                return base.run(g);
            }
        }

        public class RemoveResponseEvent : ForkEvent
        {
            private string[] messages;
            private string messenger;
            public RemoveResponseEvent(string messenger, string[] messages, PlotNode trueNode, PlotNode falseNode):
                base(toDict(trueNode, falseNode))
            {
                this.messages = messages;
                this.messenger = messenger;
            }
            public override PlotNode run(Game g)
            {
                return _run(g.getMessenger(messenger).removeResponse(messages).ToString());
            }
            private static Dictionary<string, PlotNode> toDict(PlotNode tn, PlotNode fn)
            {
                Dictionary<string, PlotNode> r = new Dictionary<string, PlotNode>();
                r.Add("true", tn);
                r.Add("false", fn);
                return r;
            }
        }

        public class FileEvent : LinearEvent
        {
            private string file;
            public FileEvent(string file, PlotNode pn):
                base(pn)
            {
                this.file = file;
            }
            public override PlotNode run(Game g)
            {
                g.addFile(file);
                return base.run(g);
            }
        }

        public class ClearMessengerEvent : LinearEvent
        {
            private string messenger;
            public ClearMessengerEvent(string messenger, PlotNode next):
                base(next)
            {
                this.messenger = messenger;
            }

            public override PlotNode run(Game g)
            {
                g.getMessenger(messenger).clear();
                return base.run(g);
            }
        }

        //the class itself
        protected Event e;
        protected Predicate pred;
        public PlotNode(Predicate pred, Event e)
        {
            this.pred = pred;
            this.e = e;
        }
        public virtual bool test(Game g) { return pred.test(g); }
        public PlotNode run(Game g) { return e.run(g); }
        public virtual bool getAutoRun()
        {
            return pred == null;
        }
    }

    public class PredForkPlotNode : PlotNode
    {
        private Dictionary<Predicate, Event> paths;
        public PredForkPlotNode(Dictionary<Predicate,Event> paths):
            base(null, null)
        {
            this.paths = paths;
        }

        public override bool test(Game g)
        {
            foreach(Predicate p in paths.Keys)
            {
                if(p.test(g)){
                    e = paths[p];
                    return true;
                }
            }
            return false;
        }
        public override bool getAutoRun()
        {
            return false;
        }
    }
}

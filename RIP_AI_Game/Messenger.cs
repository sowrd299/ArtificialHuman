using System;
using System.Collections.Generic;
using System.Linq;

namespace RIP_AI_Game
{
    public class Messenger
    {
        private const string split = ": ";
        private const int readSize = 4;

        private string name;
        private string playerName;
        private Queue<string> read;
        private Queue<string> unread;
        private Dictionary<string[], string> responses;

        public Messenger(string name, string playerName, Dictionary<string[], string> responses)
        {
            this.name = name;
            this.playerName = playerName;
            this.responses = responses;
            read = new Queue<string>();
            unread = new Queue<string>();
        }

        public void writeMessage(string message)
        {
            /*
            adds a message from the npc to the unread queue
            */
            unread.Enqueue(toMessage(name, message));
        }

        public virtual void readMessage(string message)
        {
            /*
            recieves a message from the player
            */
            enread(toMessage(playerName, message));
            message = message.ToLower();
            foreach (string[] s in responses.Keys)
            {
                if (s.Contains(message))
                {
                    writeMessage(responses[s]);
                    break;
                }
            }
        }

        public string dispUnread()
        {
            /*
            returns a string telling the player how many unread messages they have
            */
            return "You have " + unread.Count.ToString() + " unread message(s) from " + name + ".";
        }

        public string disp()
        {
            /*
            displays a recap of the conversation, and any unread messages
            */
            string r = "";
            foreach (string s in read)
            {
                r += s + "\n";
            }
            while (unread.Count > 0)
            {
                //Console.WriteLine(unread.Count);
                string s = unread.Dequeue();
                r += s + "\n";
                enread(s);
            }
            return r;
        }

        public string prompt()
        {
            return playerName + split;
        }

        public bool contains(string message)
        {
            return read.Contains(message) || unread.Contains(message);
        }

        public bool areUnread()
        {
            return unread.Count > 0;
        }
        
        public void addResponse(string[] messages, string response)
        {
            responses.Add(messages, response);
        }

        public bool removeResponse(string[] messages)
        {
            return responses.Remove(messages);
        }

        public void clear()
        {
            read = new Queue<string>();
        }

        private void enread(string message)
        {
            /*
            a decorator for read.Enqueue that limits its size to readSize by dequeueing old items
            */
            read.Enqueue(message);
            while (read.Count > readSize)
            {
                read.Dequeue();
            }
        }

        private string toMessage(string sender, string message)
        {
            /*
            encodes a message in a string format that contains sender and message.
            said format is also used in displaying messages.
            */
            return sender + split + message;
        }

        private Tuple<string, string> fromMessage(string message)
        {
            /*
            decodes a message to (sender,message)
            */
            string sender = message.Split(split[0])[0];
            return Tuple.Create<string, string>(sender, message.Replace(sender + split, ""));
        }
    }

    public class DoorOpenerMessenger : Messenger
    {
        public DoorOpenerMessenger(string n, string pn, Dictionary<string[],string> r):
            base(n, pn, r)
        {}
        public override void readMessage(string message)
        {
            base.readMessage(message);
            if(message.Contains("open door"))
            {
                writeMessage("Opening door "+message.Split(' ').Last());
            }
        }
    }

}

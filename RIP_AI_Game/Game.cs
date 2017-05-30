using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Ambiance;

namespace RIP_AI_Game
{
    public class Game
    {
        private Room room;
        private Dictionary<string, string> files;
        private Dictionary<string, Messenger> messengers;
        private PlotNode plot;
        private PrioQueue<string> pq;
        private AmbianceGenerator ambiance;
        private int dir;

        public Game(Dictionary<string, Dictionary<string, Tuple<string, Predicate>>> doors, Dictionary<string, Room> rooms, PlotNode plot, Dictionary<string,Dictionary<string[], string>> responses)
        {
            pq = new PrioQueue<string>();
            files = new Dictionary<string, string>();
            ambiance = new AmbianceGenerator();
            room = buildMap("start", doors, rooms);
            room.setAltDoors("n");
            dir = 0;
            this.plot = plot;
            messengers = new Dictionary<string, Messenger>();
            foreach (string r in responses.Keys)
            {
                messengers.Add(r.ToLower(),new Messenger(r, "You", responses[r]));
            }
        }

        public void play()
        {
            /*
            runs the game (loop)
            */
            printRoom();
            while (true)
            {
                //advance the plot (node)
                if (plot != null && plot.test(this))
                {
                    plot = plot.run(this);
                    //auto runs nodes that should occure subsequently
                    while (plot != null && plot.getAutoRun())
                    {
                        //                        Console.WriteLine("Auto Running");
                        plot = plot.run(this);
                    }
                    //end the game
                    if (plot == null)
                    {
                        printQueue();
                        break;
                    }
                }
                printQueue();
                //display unread messages
                foreach (Messenger m in messengers.Values)
                {
                    if (m.areUnread())
                    {
                        Console.WriteLine(m.dispUnread());
                    }
                }
                string a = ambiance.genAmbiance();
                if (a != null) { print("\n"+a, 0); }
                loop();
                Console.WriteLine("");
            }
        }

        private void loop()
        {
            Console.WriteLine("What do you do? ");
            string[] input = Console.ReadLine().ToLower().Split(' ');
            if(input[0] == "i")
            {
                List<string> temp = input.ToList();
                temp.Remove("i");
                input = temp.ToArray();
            }
            //the read command
          
            /*
            if (input.Contains("read"))
            {
                if ((input.Contains("message") || input.Contains("messages")) == (input.Contains("file")) || input.Contains("files"))
                {
                    Console.WriteLine("Choose a file or a message to read.");
                    loop();
                }
            }
            */
            
            //various commands
            //WALK
            if ( (new string[] { "walk", "go" }.Contains(input[0]) || (input.Contains("open") && input.Contains("door"))) && input.Length > 1)
            {
                try
                {
                    //general room change
                    Door tr = room.walk(input.Last());
                    if (tr.getUnlocked(this))
                    {
                        room = tr.to;
                        //rotating; must happen after possibility of null reg except
                        Dictionary<string, int> dirs = new Dictionary<string, int>();
                        dirs.Add("forward", 0);
                        dirs.Add("left", 1);
                        dirs.Add("back", 2);
                        dirs.Add("right", 3);
                        if (dirs.ContainsKey(input.Last()))
                        {
                            dir += dirs[input.Last()];
                            dir %= 4;
                        }
                        //misc
                        room.setAltDoors(new string[]{"n","w","s","e"}[dir]);
                        printRoom();
                        print("You walk through the door.", -1);
                    }
                    else
                    {
                        print("The door is locked.");
                    }
                }
                catch (NullReferenceException e)
                {
                    print("You cannot walk that way.");
                }
            }
            //SEARCH
            else if (input[0] == "search" || input[0] == "use")
            {
                SearchLoc s = room.search(input.Last());
                if (s != null)
                {
                    switch ((int)s.type)
                    {
                        case (int)SearchLoc.Type.FILE:
                            addFile(s.text);
                            print("You find a file. It reads:");
                            goto case (int)SearchLoc.Type.TEXT;
                        case (int)SearchLoc.Type.TEXT:
                            print(s.text);
                            break;
                    }
                }
                else
                {
                    print("You find nothing");
                }
            }
            //SEND
            else if (new string[] { "write", "reply", "send", "respond" }.Contains(input[0]))
            {
                string i = input.Last();
                if (!messengers.ContainsKey(i))
                {
                    Console.WriteLine("Who do you send the message to?");
                    i = Console.ReadLine().ToLower().Split(' ').Last();
                }
                try
                {
                    Console.Write(messengers[i].prompt());
                    messengers[i].readMessage(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("There is no such person.");
                }
            }
            //READ MESSAGES
            else if (input[0].Contains("message") || (input.Length > 1 && input[1].Contains("message")) || (input.Length > 2 && input.Contains("message")))
            {
                string i = input.Last();
                if (!messengers.ContainsKey(input.Last()))
                {
                    Console.WriteLine("Who do you read messages from?");
                    i = Console.ReadLine().ToLower().Split(' ').Last();
                }
                print("\n\tMessages from " + i.ToUpper() + ":");
                try
                {
                    print("\t" + messengers[i].disp().Replace("\n", "\n\t"));
                }
                catch (KeyNotFoundException e)
                {
                    print("There is no such person.");
                }
            }
            //FILE
            else if (input.Length > 1 && (input[0] == "file" || input[0] == "read" || input[1] == "file"))
            {
                if (files.ContainsKey(input.Last()))
                {
                    print(files[input.Last()]);
                }
                else
                {
                    Console.WriteLine("You don't have a file called " + input[1] + ".");
                }
            }
            //FILES
            else if (input[0] == "files" || (input.Length > 1 && input[1] == "files"))
            {
                print("\tFILES:");
                foreach (string file in files.Keys)
                {
                    print("\t"+ file);
                }
            }
            else if (input[0] == "look")
            {
                printRoom();
            }
            /*
                        else
                        {
                            loop();
                        }
             */
        }

        private void printRoom()
        {
            print("\n" + room.text, 2);
        }

        public void addFile(string file)
        {
            string name = file.Split(':')[0].ToLower();
            if (!files.ContainsKey(name))
            {
                print("\nFile '" + name + "' has been added to your files.",3);
                files.Add(name, file);
            }
        }

        public bool hasFile(string file)
        {
            //          Console.WriteLine(files.ContainsKey(file.ToLower()).ToString());
            return files.ContainsKey(file.ToLower());
        }

        public bool inRoom(Room room)
        {
            return this.room.Equals(room);
        }

        public Messenger getMessenger(string m)
        {
            return messengers[m];
        }

        public void writeMessage(string sender, string message)
        {
            sender = sender.ToLower();
            if (messengers.ContainsKey(sender))
            {
                messengers[sender].writeMessage(message);
            }
        }

        public void print(string text, int prio = 1)
        {
            //            Console.WriteLine(prio.ToString());
            pq.Enqueue(text, prio);
        }

        private void printQueue()
        {
            string s = "";
            while (s != null)
            {
                s = pq.Dequeque();
                Console.WriteLine(s);
//                s = pq.Dequeque();
            }
        }

        private Room buildMap(string startKey, Dictionary<string, Dictionary<string, Tuple<string, Predicate>>> doors, Dictionary<string, Room> rooms)
        {
            foreach (string key in rooms.Keys)
            {
                foreach (string door in doors[key].Keys)
                {

                    rooms[key].setDoor(door, rooms[doors[key][door].Item1], doors[key][door].Item2);
                }
            }
            return rooms[startKey];
        }
    }
}

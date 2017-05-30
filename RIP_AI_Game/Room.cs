using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RIP_AI_Game
{
    public class Room
    {
        private Dictionary<string, string> altDoorTerms;
        private string _text; //a description of the room
        public string text {
            get {
                string _text = this._text;
                foreach (string s in altDoors.Keys) {
                    _text = _text.Replace("[" + altDoors[s] + "]", s);
                    _text = _text.Replace("[*" + altDoors[s] + "]", altDoorTerms[s]);
                }
                return _text;
            }
        }
        private Dictionary<string, string> altDoors;
        private Dictionary<string, Door> doors; //direction : room leads to
        private Dictionary<string, SearchLoc> searches;

        public Room(string fText /*formated with certain markup*/)
        {
            //init alt door terms
            altDoorTerms = new Dictionary<string, string>();
            altDoorTerms.Add("back", "behind you");
            altDoorTerms.Add("left", "to your left");
            altDoorTerms.Add("right", "to your right");
            altDoorTerms.Add("forward", "in front of you");
            //read formating from fText
            string text = fText;
            doors = new Dictionary<string, Door>();
            altDoors = new Dictionary<string, string>();
            searches = new Dictionary<string, SearchLoc>();
            //code for a door feature that would be nice to have one day.
            foreach (Match match in Regex.Matches(fText, @"<door,(\w+),(\w+)>"))
            {
                //addresses door markup
                altDoors.Add(match.Groups[1].ToString(), match.Groups[2].ToString());
                text = text.Replace(match.Groups[0].ToString(), match.Groups[1].ToString());
            }
            foreach (Match match in Regex.Matches(fText, @"<search,(\w+),(TEXT|FILE),(HIDDEN,)?([^>]+)>"))
            {
                //address searchable location markup
                searches.Add(match.Groups[1].ToString(), new SearchLoc((match.Groups[2].ToString() != "TEXT" ? SearchLoc.Type.FILE : SearchLoc.Type.TEXT), match.Groups[4].ToString()));
//              Console.WriteLine(searches[match.Groups[1].ToString()].text);
                text = text.Replace(match.Groups[0].ToString(), (match.Groups[3].Length == 0 ? match.Groups[1].ToString() : ""));
            }
            _text = text; //keep at bottom so gets eddited version
        }

        public Door walk(string d)
        {
            /*
            returns the door in the provided direction
            */
            if (doors.ContainsKey(d))
            {
                return doors[d];
            }
            else if (altDoors != null && altDoors.ContainsKey(d) && doors.ContainsKey(altDoors[d]))
            {
                return doors[altDoors[d]];
            }
            return null;
        }

        public void setAltDoors(string f)
        {
            Dictionary<string, int> dirs = new Dictionary<string, int>();
            dirs.Add("n", 0);
            dirs.Add("w", 1);
            dirs.Add("s", 2);
            dirs.Add("e", 3);
            if (!dirs.ContainsKey(f)) {
                return;
            }
            altDoors = new Dictionary<string, string>();
            string[] locDirs = new string[] { "forward", "left", "back", "right" };
            foreach(string dir in dirs.Keys)
            {
                altDoors.Add(locDirs[dirs[dir]-dirs[f]+(dirs[dir]<dirs[f]? 4 : 0)],dir);
            }
        }

        public SearchLoc search(string d)
        {
            if (searches.ContainsKey(d))
            {
                return searches[d];
            }
            return null;
        }

        public void setDoor(string key, Room val, Predicate pred = null)
        {
            if (pred == null)
            {
                pred = new NoPred();
            }
            doors.Add(key, new Door(val, pred));
        }
    }

    public class Door
    {
        private Room _to;
        public Room to { get { return _to; } }
        private Predicate pred;

        public Door(Room to, Predicate pred)
        {
            this._to = to;
            this.pred = pred;
        }

        public bool getUnlocked(Game g)
        {
            return pred.test(g);
        }
    }

    public class SearchLoc
    {
        public enum Type { TEXT, FILE };
        private Type _type;
        public Type type { get { return _type; } }
        private string _text;
        public string text
        {
            get
            {
                if (type == Type.FILE)
                {
                    return _text.Replace("\n", "||").TrimEnd('|');
                }
                return _text;
            }
        }

        public SearchLoc(Type type, string text)
        {
            this._type = type;
            this._text = text;
        }

    }
}

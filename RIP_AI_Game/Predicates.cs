using System;

namespace RIP_AI_Game
{
    //predicates
    public abstract class Predicate { public abstract bool test(Game g); }
    public class NoPred : Predicate { public override bool test(Game g) { return true; } }

    public class FilePred : Predicate
    {
        private string file;
        public FilePred(string file)
        {
            this.file = file;
        }
        public override bool test(Game g)
        {
            return g.hasFile(file);
        }
    }

    public class RoomPred : Predicate
    {
        private Room room;
        public RoomPred(Room room)
        {
            this.room = room;
        }
        public override bool test(Game g)
        {
            return g.inRoom(room);
        }
    }

    public class MessagePred : Predicate
    {
        private string messenger;
        private string message;
        public MessagePred(string messenger, string message)
        {
            this.message = message;
            this.messenger = messenger;
        }
        public override bool test(Game g)
        {
            return g.getMessenger(messenger).contains(message);
        }
    }

    public class TimerPred: Predicate
    {
        private int count;
        private int unlockCount;
        public TimerPred(int unlockCount)
        {
            this.unlockCount = unlockCount;
        }
        public override bool test(Game g)
        {
            count++;
            return count > unlockCount;
        }
    }

    public class NotPred: Predicate
    {
        private Predicate pred;
        public NotPred(Predicate pred)
        {
            this.pred = pred;
        }
        public override bool test(Game g)
        {
            return !pred.test(g);
        }
    }

    public class OrPred: Predicate
    {
        private Predicate[] preds;
        public OrPred(Predicate[] preds)
        {
            this.preds = preds;
        }
        public override bool test(Game g)
        {
            foreach(Predicate p in preds)
            {
                if (p.test(g)) { return true; };
            }
            return false;
        }
    }
}

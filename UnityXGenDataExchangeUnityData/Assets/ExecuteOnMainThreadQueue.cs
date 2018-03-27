using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

class ExecuteOnMainThreadQueue
{
   internal readonly static Queue<Action> executeOnMainThread = new Queue<Action>();
   
   ///////////////////////////////////////////////////

   public static void AddAction(Action act)
   {
      executeOnMainThread.Enqueue(act);
   }

   public static void ProcessQueuedActions()
   {
      while (executeOnMainThread.Count > 0)
      {
         executeOnMainThread.Dequeue().Invoke();
      }
   }
}

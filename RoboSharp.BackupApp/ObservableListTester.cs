using RoboSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RoboSharp.BackupApp
{
    /// <summary>
    /// Test <see cref="ObservableList{T}.CollectionChanged"/> event
    /// </summary>
    internal class ObservableListTester
    {
        public ObservableListTester()
        {
            OList = new ObservableList<int>();
            OList.CollectionChanged += List_CollectionChanged;
        }

        private EventHandler SetNextExpectation;

        private readonly ObservableList<int> OList;
        private NotifyCollectionChangedAction ExpectedAction;
        private int NewItemsCount;
        private int OldItemsCount;
        private int NewStartingIndex;
        private int OldStartingIndex;
        private bool DidEventFire;
        private int EventsFiredCount;
        private int ExpectedEventsFiredCount;

        private void SetAddExpectation(int Count = 1, int StartingIndex = -1) => SetExpectations(NotifyCollectionChangedAction.Add, Count, 0, StartingIndex);
        private void SetMoveExpectation(int Count, int StartingIndex) => SetExpectations(NotifyCollectionChangedAction.Move, Count, 0, StartingIndex);
        private void SetRemoveExpectation(int Count, int StartingIndex) => SetExpectations(NotifyCollectionChangedAction.Remove, 0, Count, -1, StartingIndex);
        private void SetReplaceExpectation(int Count = 1, int StartingIndex = -1) => SetExpectations(NotifyCollectionChangedAction.Replace, Count, Count, StartingIndex, StartingIndex);

        private void SetInsertExpectations(int count, int StartingIndex, int ExpectedMoveEvents = 1)
        {
            SetAddExpectation(count, StartingIndex);
            SetNextExpectation += (_, __) => SetMoveExpectation(ExpectedMoveEvents, StartingIndex + count);
        }

        private void SetExpectations(NotifyCollectionChangedAction action, int NewCount, int OldCount, int NewIndex = -1, int OldIndex = -1)
        {
            if (!DidEventFire)
                throw new Exception("Previous Event did not fire!");

            ExpectedAction = action;
            NewItemsCount = NewCount;
            OldItemsCount = OldCount;
            NewStartingIndex = NewIndex;
            OldStartingIndex = OldIndex;
            EventsFiredCount = 0;
            ExpectedEventsFiredCount = action == NotifyCollectionChangedAction.Move ? NewCount : 1;
            DidEventFire = false;
        }

        private void ThrowIfCountUnexpected(string TestID)
        {
            if (ExpectedEventsFiredCount != EventsFiredCount)
                throw new Exception($"Test ID: {TestID}\nExpectedEventsFiredCount != EventsFiredCount\nExpected:{ExpectedEventsFiredCount }\nReceived:{EventsFiredCount}");
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.Action != ExpectedAction)
                throw new Exception("NotifyCollectionChangedAction Not Of Expected Type");

            bool CheckAdd = false;
            bool CheckRemove = false;

            EventsFiredCount++;
            if (e.Action == NotifyCollectionChangedAction.Move) { }
            else if (e.Action == NotifyCollectionChangedAction.Reset) { }
            else if (e.Action == NotifyCollectionChangedAction.Add) { CheckAdd = true; }
            else if (e.Action == NotifyCollectionChangedAction.Remove)  { CheckRemove = true; }
            else if (e.Action == NotifyCollectionChangedAction.Replace) { CheckAdd = true; CheckRemove = true; }
            else { }

            if (CheckAdd)
            {
                if (NewItemsCount != e.NewItems.Count) throw new Exception($"NewItems.Count != Expected Count\nExpected:{NewItemsCount}\nReceived:{e.NewItems.Count}");
                if (NewStartingIndex >= 0 && NewStartingIndex != e.NewStartingIndex) 
                    throw new Exception($"NewStartingIndex != Expected Index\nExpected:{NewStartingIndex }\nReceived:{e.NewStartingIndex}");
            }

            if (CheckRemove)
            {
                if (OldItemsCount != e.OldItems.Count) throw new Exception($"OldItems.Count != Expected Count\nExpected:{OldItemsCount}\nReceived:{e.OldItems.Count}");
                if (OldStartingIndex != e.OldStartingIndex) throw new Exception($"OldStartingIndex != Expected Index\nExpected:{OldStartingIndex}\nReceived:{e.OldStartingIndex}");
            }

            DidEventFire = true;
            SetNextExpectation?.Invoke(this, new EventArgs());
            SetNextExpectation = null;
        }

        public void RunTest()
        {
            DidEventFire = true;
            //Add & Remove & Clear
            AddTests();
            RemoveTests();
            //Replace & Sort
            ReplaceTests();
            SortTests();
        }

        private void AddTests()
        {

            SetAddExpectation();
            OList.Add(1); // OList now = { 1 }                                                -> 1 items added

            SetAddExpectation(3);
            OList.AddRange(new int[] { 4, 5, 6 });   // OList now = { 1, 4, 5, 6 }            -> 3 items added

            SetInsertExpectations(1, 0);
            OList.Insert(0, 0); // OList now = { 0, 1, 4, 5, 6 }                              -> 1 items added, index 0
            ThrowIfCountUnexpected("InsertTest_1");

            SetInsertExpectations(1, 2);
            OList.Insert(2, 3); // OList now = { 0, 1, 3, 4, 5, 6 }                           -> 1 items added, index 2
            ThrowIfCountUnexpected("InsertTest_2");

            SetInsertExpectations(2, 2);
            OList.InsertRange(2, new int[] { 8, 9 }); // OList now = { 0, 1, 2, 3, 3, 4, 5, 6 }  -> 2 items added
            ThrowIfCountUnexpected("InsertTest_3");

            int[] test = new int[] { 0, 1, 8, 9, 3, 4, 5, 6 };
            for (int i = 0; i < test.Length; i++)
                if (test[i] != OList[i])
                    throw new Exception("Add Tests Failed - OList does not contain correct values");
        }

        private void RemoveTests()
        {
            SetRemoveExpectation(1, 4);
            bool success = OList.Remove(3);      // OList now = { 0, 1, 8, 9, 3, 4, 5, 6  }      -> 1 items removed

            SetRemoveExpectation(2, -1);
            int removedCount = OList.RemoveAll(i => i < 3); // OList now = { 0, 1, 8, 9, 4, 5, 6  }     -> 3 items removed

            DidEventFire = false;
            bool success2 = OList.Remove(0);     //Should return false, no collection change -> 2 items removed
            if (DidEventFire) throw new Exception("Event Fired Unexpectedly");
            DidEventFire = true;

            SetRemoveExpectation(1, 0);
            OList.RemoveAt(0);                   //OList now = { 8, 9, 4, 5, 6 }                    -> 1 items removed, index 0
            SetRemoveExpectation(3, 1);
            OList.RemoveRange(1, 3);             //OList now = { 9, 4, 5, 6  }                          -> 2 items removed, index 0

            if (!(OList.Count == 1 && OList[0] == 9)) 
                throw new Exception("Add Tests Failed - OList does not contain correct values");

            //ClearTest
            SetExpectations(NotifyCollectionChangedAction.Reset, 0, 0);
            OList.Clear();
            if (!DidEventFire) throw new Exception("Clear()Event Did Not Fired");

        }

        private void ReplaceTests()
        {
            SetAddExpectation(7);
            OList.AddRange(new int[] { 0, 1, 2, 3, 4, 5, 6 });

            SetReplaceExpectation(1, 0);
            OList[0] = 7;                        // Replace 0 with 7
            
            SetExpectations(NotifyCollectionChangedAction.Add, 1, 0, 7);
            OList[7] = 0;                        // Add 0 to end of the OList

            SetReplaceExpectation(1, 0);
            OList.Replace(0, 0);                 // Put 0 back at start of OList              -> 1 items replaced, index 0
            
            SetReplaceExpectation(1, 7);
            OList.Replace(7, 7);                 // Put 7 back at end of OList                -> 1 items replaced, index 7

            //Replace 4 items
            SetReplaceExpectation(4, 3);
            OList.Replace(3, new int[] { 6, 5, 4, 3 }); // Replace 3,4,5,6 with 6,5,4,3      -> 4 items replaced, index 3
            
            // Replace some items, then add the rest
            SetReplaceExpectation(5, 3);
            SetNextExpectation += (o, e) => SetExpectations(NotifyCollectionChangedAction.Add, 2, 0);
            OList.Replace(3, new int[] { 3, 4, 5, 6, 7, 8, 9 }); // Replace 3,4,5,6 with 6,5,4,3      -> 4 items replaced, index 3
            ThrowIfCountUnexpected("ReplaceTest_2");
        }

        private void SortTests()
        {
            NotifyCollectionChangedAction ResetAction = NotifyCollectionChangedAction.Reset;

            SetMoveExpectation(OList.Count, 0);
            OList.Reverse();                            //  -> 7 items moved
            ThrowIfCountUnexpected("ReverseTest");

            SetExpectations(ResetAction, 1, 0);
            OList.Sort(false);                          //  -> 7 items moved -> 1 event (Reset)
            ThrowIfCountUnexpected("SortTest_1");

            SetExpectations(ResetAction, 1, 0);
            OList.Reverse(false);                            //  -> 7 items moved
            ThrowIfCountUnexpected("ReverseTest_2");

            SetMoveExpectation(OList.Count, 0);
            OList.Sort(true);             
            ThrowIfCountUnexpected("SortTest_2");
        }

    }
}

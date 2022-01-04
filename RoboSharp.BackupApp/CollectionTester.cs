using RoboSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RoboSharp.BackupApp
{
    /// <summary>
    /// Test <see cref="ObservableList{T}.CollectionChanged"/> event
    /// </summary>
    internal class CollectionTester
    {
        public CollectionTester(){}

        public void RunTest()
        {
            var list = new ObservableList<int>(capacity: 7 );
            list.CollectionChanged += List_CollectionChanged;
            list.Add(1); // list now = { 1 }                                                -> 1 items added
            list.AddRange(new int[] { 4, 5, 6 });   // list now = { 1, 4, 5, 6 }            -> 3 items added
            list.Insert(0, 0); // list now = { 0, 1, 4, 5, 6 }                              -> 1 items added, index 0
            list.Insert(2, 3); // list now = { 0, 1, 3, 4, 5, 6 }                           -> 1 items added, index 2
            list.InsertRange(2, new int[] { 2, 3 }); // list now = { 1, 2, 3, 3, 4, 5, 6 }  -> 2 items added
            bool success = list.Remove(3);      // list now = { 0, 1, 2, 3, 4, 5, 6 }       -> 1 items removed
            int removedCount = list.RemoveAll(i => i < 3); // list now = { 3, 4, 5, 6 }     -> 3 items removed
            bool success2 = list.Remove(0);     //Should return false, no collection change -> 2 items removed
            list.RemoveAt(0);                   //list now = { 4, 5, 6 }                    -> 1 items removed, index 0
            list.RemoveRange(0, 2);             //list now = { 6 }                          -> 2 items removed, index 0
            list.Clear();
            list.AddRange(new int[] {0, 1, 2, 3, 4, 5, 6 });
            list[0] = 7;                        // Replace 0 with 7
            list[7] = 0;                        // Add 0 to end of the list
            list.Replace(0, 0);                 // Put 0 back at start of list              -> 1 items replaced, index 0
            list.Replace(7, 7);                 // Put 7 back at end of list                -> 1 items replaced, index 0
            list.Replace(3, new int[] { 6, 5, 4, 3 }); // Replace 3,4,5,6 with 6,5,4,3      -> 4 items replaced, index 3
            list.Replace(3, new int[] { 3, 4, 5, 6, 7, 8, 9 }); // Replace 3,4,5,6 with 6,5,4,3      -> 4 items replaced, index 3
            list.Reverse();                     //                                          -> 7 items moved
            list.Sort(true);                        //                                          -> 7 items moved
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Do Nothing
        }
    }
}

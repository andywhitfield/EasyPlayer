using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPlayer.Tests
{
    [TestClass]
    public class CustomObservableCollectionTest
    {
        [TestMethod]
        public void Should_raise_changed_event_when_adding_deleting_and_manual()
        {
            var changedEvents = new List<NotifyCollectionChangedEventArgs>();
            var collection = new CustomObservableCollection<string>();
            collection.CollectionChanged += (s, e) => changedEvents.Add(e);

            collection.Add("one");
            collection.Add("two");
            collection.Remove("one");
            collection.RaiseCollectionChanged();

            Assert.AreEqual(4, changedEvents.Count);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, changedEvents[0].Action);
            Assert.AreEqual("one", changedEvents[0].NewItems.OfType<string>().FirstOrDefault());

            Assert.AreEqual(NotifyCollectionChangedAction.Add, changedEvents[1].Action);
            Assert.AreEqual("two", changedEvents[1].NewItems.OfType<string>().FirstOrDefault());

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, changedEvents[2].Action);
            Assert.AreEqual("one", changedEvents[2].OldItems.OfType<string>().FirstOrDefault());

            Assert.AreEqual(NotifyCollectionChangedAction.Reset, changedEvents[3].Action);
        }
    }
}

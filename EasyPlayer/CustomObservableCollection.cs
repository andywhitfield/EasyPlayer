using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Caliburn.Micro;

namespace EasyPlayer
{
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public CustomObservableCollection() : base() { }
        public CustomObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public CustomObservableCollection(List<T> list) : base(list) { }

        public void RaiseCollectionChanged()
        {
            Execute.OnUIThread(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Execute.OnUIThread(() => base.OnCollectionChanged(e));
        }
    }
}

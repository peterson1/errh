﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ErrH.WpfTools.CollectionShims
{
    public class Observables<T> : ObservableCollection<T>
    {
        //public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public Observables(List<T> list = null) : base(list ?? new List<T>()) { }




        //public void Fire_CollectionChanged(object sender)
        //{
        //    CollectionChanged?.Invoke(sender, 
        //        new NotifyCollectionChangedEventArgs
        //            (NotifyCollectionChangedAction.Add));
        //}
    }
}



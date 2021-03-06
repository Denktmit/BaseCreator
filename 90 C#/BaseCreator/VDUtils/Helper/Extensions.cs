﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDUtils.Helper {

  public static class Extensions {

    public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable {
      List<T> sorted = collection.OrderBy(x => x).ToList();
      for (int i = 0; i < sorted.Count(); i++)
        collection.Move(collection.IndexOf(sorted[i]), i);
    }

  }

}

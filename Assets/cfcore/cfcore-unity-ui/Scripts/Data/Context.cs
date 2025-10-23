
using Assets.Scripts.Utils;
using System;
using UnityEngine;

namespace Overwolf.CFCore.CFCContext {
  public class Context {
    public class Key<T> {
      public readonly string Name;
      public Key(string name) {
        Name = name;
      }
    }

    ObservableDictionary<string> _observableDictionary;

    private static Context _instance;
    public static Context Instance {
      get {
        if (_instance == null) {
          _instance = new Context();
          _instance._observableDictionary = new ObservableDictionary<string>();
        }
        return _instance;
      }
    }

    public void SetContext<T>(Key<T> key, T value) {
      _observableDictionary.GetObservable<T>(key.Name).Value = value;
    }

    public T GetContext<T>(Key<T> key) {
      return _observableDictionary.GetObservable<T>(key.Name).Value;
    }

    public void AddListener<T>(Key<T> key, Action<T> action) {
      _observableDictionary.GetObservable<T>(key.Name).ValueChanged += action;
    }

    public void RemoveListener<T>(Key<T> key, Action<T> action) {
      _observableDictionary.GetObservable<T>(key.Name).ValueChanged -= action;
    }

  }
}

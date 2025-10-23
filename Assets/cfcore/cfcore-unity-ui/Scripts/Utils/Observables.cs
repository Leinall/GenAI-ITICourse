using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils {
  public class Observable<T> {
    private T _value;
    public event Action<T> ValueChanged;

    public Observable(T value) {
      _value = value;
    }

    public T Value {
      get => _value;
      set {
        if (!EqualityComparer<T>.Default.Equals(_value, value)) {
          _value = value;
          ValueChanged?.Invoke(value);
        }
      }
    }

    public void SetValue(object value) {
      Value = (T)value;
    }

    public object GetValue() {
      return Value;
    }
  }
  public class ObservableDictionary<TKey> {
    private readonly Dictionary<TKey, object> _dictionary = new Dictionary<TKey, object>();

    public event Action<TKey, object> ValueChanged;

    public Observable<T> GetObservable<T>(TKey key) {
      if (!_dictionary.ContainsKey(key)) {
        _dictionary[key] = new Observable<T>(default(T));
        ((Observable<T>)_dictionary[key]).ValueChanged += value => ValueChanged?.Invoke(key, value);
      }

      var observable = _dictionary[key] as Observable<T>;
      if (observable == null) {
        throw new ArgumentException($"Value stored for key {key} is not of type {typeof(T)}");
      }

      return observable;
    }

    public void SetObservable<T>(TKey key, Observable<T> observable) {
      _dictionary[key] = observable;
      observable.ValueChanged += value => ValueChanged?.Invoke(key, value);
    }
  }
}

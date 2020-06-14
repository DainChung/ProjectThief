using System;
using System.Collections.Generic;

namespace Com.MyCompany.MyGame
{
    public static class MyConvertType
    {
        public static T ConvertToGeneric<T>(object v)
        {
            return (T)((object)(Convert.ChangeType(v, typeof(T))));
        }
    }

    public abstract class Observer<T>
    {
        protected List<Subject<T>> _subjects = new List<Subject<T>>();

        public abstract void UpdateMultipleObserver();
        public abstract void UpdateObserver();

        public void Add(Subject<T> sub)
        {
            _subjects.Add(sub);
        }
        public void Remove(Subject<T> sub)
        {
            _subjects.Remove(sub);
        }
    }
    public abstract class Subject<T>
    {
        private List<Observer<T>> _observers = new List<Observer<T>>();
        private T _value;
        public T value { get { return _value; } set { _value = value; } }

        public void Add(Observer<T> ob)
        {
            _observers.Add(ob);
            _observers[_observers.Count - 1].Add(this);
        }
        public void Remove(Observer<T> ob)
        {
            _observers.Remove(ob);
        }
        public void Notify()
        {
            if (_observers.Count > 1)
            {
                for (int i = 0; i < _observers.Count; i++)
                    _observers[i].UpdateMultipleObserver();
            }
            else
                _observers[0].UpdateObserver();
        }

    }

    public class ConcreteSubject<T> : Subject<T>
    {
    }
    public class ConcreteObserver<T> : Observer<T>
    {
        private T _value;
        private ConcreteSubject<T> _subject;

        public T value { get { return _value; } }

        public ConcreteObserver(ConcreteSubject<T> subject, T value)
        {
            _value = value;
            _subject = subject;
        }
        public ConcreteObserver(T value)
        {
            _value = value;
        }

        public override void UpdateMultipleObserver()
        {
            _value = _subject.value;
        }
        public override void UpdateObserver()
        {
            if (typeof(T) == typeof(bool))
            {
                bool v = false;
                for (int i = 0; i < _subjects.Count; i++)
                {
                    bool subV;
                    bool.TryParse(_subjects[i].value.ToString(), out subV);
                    v |= subV;
                }
                _value = MyConvertType.ConvertToGeneric<T>(v);
            }
        }
    }
}

// Cross-platform System.ComponentModel stubs
using System;

namespace System.ComponentModel
{
    /// <summary>Base class for components that need Dispose pattern</summary>
    public class Component : IComponent, IDisposable
    {
        private bool _disposed;
        private ISite _site;
        private EventHandlerList _events = new EventHandlerList();

        public ISite Site
        {
            get => _site;
            set => _site = value;
        }

        public event EventHandler Disposed
        {
            add => _events.AddHandler("Disposed", value);
            remove => _events.RemoveHandler("Disposed", value);
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _events?.Dispose();
            }
            _disposed = true;
        }

        ~Component()
        {
            Dispose(false);
        }
    }

    public interface IComponent : IDisposable
    {
        ISite Site { get; set; }
        event EventHandler Disposed;
    }

    public interface ISite
    {
        IComponent Component { get; }
        IContainer Container { get; }
        bool DesignMode { get; }
        string Name { get; set; }
    }

    public interface IContainer : IDisposable
    {
        ComponentCollection Components { get; }
        void Add(IComponent component);
        void Add(IComponent component, string name);
        void Remove(IComponent component);
    }

    public class ComponentCollection : System.Collections.ReadOnlyCollectionBase
    {
        public ComponentCollection(IComponent[] components) : base()
        {
            if (components != null)
                InnerList.AddRange(components);
        }

        public virtual IComponent this[int index] => InnerList[index] as IComponent;
        public virtual IComponent this[string name] => null;
    }

    public class EventHandlerList : IDisposable
    {
        private System.Collections.Generic.Dictionary<string, Delegate> _handlers = new();

        public void AddHandler(string key, Delegate value)
        {
            if (_handlers.ContainsKey(key))
                _handlers[key] = Delegate.Combine(_handlers[key], value);
            else
                _handlers[key] = value;
        }

        public void RemoveHandler(string key, Delegate value)
        {
            if (_handlers.ContainsKey(key))
                _handlers[key] = Delegate.Remove(_handlers[key], value);
        }

        public Delegate this[string key] => _handlers.ContainsKey(key) ? _handlers[key] : null;

        public void Dispose()
        {
            _handlers.Clear();
        }
    }
}

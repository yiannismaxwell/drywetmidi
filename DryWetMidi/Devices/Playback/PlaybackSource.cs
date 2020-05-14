using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class PlaybackSource : IPlaybackSource, IEnumerable<ITimedObject>
    {
        #region Events

        public event EventHandler<ICollection<ITimedObject>> ObjectsAdded;
        
        public event EventHandler<ICollection<ITimedObject>> ObjectsRemoved;
        
        public event EventHandler<ICollection<ITimedObject>> ObjectsTimesChanged;
        
        public event EventHandler CollectionModified;

        #endregion

        #region Fields

        private readonly List<ITimedObject> _timedObjects = new List<ITimedObject>();

        #endregion

        #region Properties


        #endregion

        #region Methods

        public void Add(ITimedObject timedObject)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);

            _timedObjects.Add(timedObject);
            SubscribeToTimedObjectEvents(timedObject);
            OnObjectsAdded(new[] { timedObject });
        }

        public void Add(params ITimedObject[] timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            Add((IEnumerable<ITimedObject>)timedObjects);
        }

        public void Add(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            _timedObjects.AddRange(timedObjects);

            foreach (var timedObject in timedObjects)
            {
                SubscribeToTimedObjectEvents(timedObject);
            }

            OnObjectsAdded(timedObjects.ToArray());
        }

        public bool Remove(ITimedObject timedObject)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);

            UnsubscribeFromTimedObjectEvents(timedObject);
            var removed = _timedObjects.Remove(timedObject);
            if (removed)
                OnObjectsRemoved(new[] { timedObject });

            return removed;
        }

        public int Remove(params ITimedObject[] timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            return Remove((IEnumerable<ITimedObject>)timedObjects);
        }

        public int Remove(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            var removedObjects = new List<ITimedObject>();

            var removedObjectsCount = _timedObjects.RemoveAll(o =>
            {
                var result = timedObjects.Contains(o);
                if (result)
                {
                    UnsubscribeFromTimedObjectEvents(o);
                    removedObjects.Add(o);
                }

                return result;
            });

            OnObjectsRemoved(removedObjects);

            return removedObjectsCount;
        }

        public void Clear()
        {
            _timedObjects.Clear();
            CollectionModified?.Invoke(this, EventArgs.Empty);
        }

        private void OnObjectsAdded(ICollection<ITimedObject> timedObjects)
        {
            ObjectsAdded?.Invoke(this, timedObjects);
        }

        private void OnObjectsRemoved(ICollection<ITimedObject> timedObjects)
        {
            ObjectsRemoved?.Invoke(this, timedObjects);
        }

        private void SubscribeToTimedObjectEvents(ITimedObject timedObject)
        {
            var notifyTimeChanged = timedObject as INotifyTimeChanged;
            if (notifyTimeChanged != null)
                notifyTimeChanged.TimeChanged += OnTimeChanged;

            var notifyLengthChanged = timedObject as INotifyLengthChanged;
            if (notifyLengthChanged != null)
                notifyLengthChanged.LengthChanged += OnLengthChanged;
        }

        private void UnsubscribeFromTimedObjectEvents(ITimedObject timedObject)
        {
            var notifyTimeChanged = timedObject as INotifyTimeChanged;
            if (notifyTimeChanged != null)
                notifyTimeChanged.TimeChanged -= OnTimeChanged;

            var notifyLengthChanged = timedObject as INotifyLengthChanged;
            if (notifyLengthChanged != null)
                notifyLengthChanged.LengthChanged -= OnLengthChanged;
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            ObjectsTimesChanged?.Invoke(this, new[] { (ITimedObject)sender });
        }

        private void OnLengthChanged(object sender, LengthChangedEventArgs e)
        {
            ObjectsTimesChanged?.Invoke(this, new[] { (ITimedObject)sender });
        }

        #endregion

        #region  IEnumerable<ITimedObject>

        public IEnumerator<ITimedObject> GetEnumerator()
        {
            return _timedObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
